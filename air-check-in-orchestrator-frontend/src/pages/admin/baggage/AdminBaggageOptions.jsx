import React, { useEffect, useState, useCallback } from "react";
import {
  Table,
  Button,
  message,
  Popconfirm,
  Modal,
  Form,
  InputNumber,
} from "antd";
import {
  getAllPaidOptions,
  deletePaidOption,
  createPaidOption,
  updatePaidOption,
} from "../../../api/api";
import { useAuth } from "../../../context/AuthContext";

const AdminBaggageOptions = () => {
  const { token, logout } = useAuth();
  const [options, setOptions] = useState([]);
  const [loading, setLoading] = useState(true);

  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingOption, setEditingOption] = useState(null);
  const [form] = Form.useForm();

  const fetchOptions = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    try {
      const res = await getAllPaidOptions();
      setOptions(res.data);
    } catch (err) {
      console.error("Ошибка при получении опций", err);
      if (err?.response?.status === 401) {
        logout();
        message.warning("Сессия истекла, войдите снова");
      } else if (err?.response?.status === 403) {
        message.error("Нет доступа к опциям багажа");
      } else {
        message.error("Ошибка загрузки опций");
      }
    } finally {
      setLoading(false);
    }
  }, [token, logout]);

  useEffect(() => {
    fetchOptions();
  }, [fetchOptions]);

  const showAddModal = () => {
    setEditingOption(null);
    form.resetFields();
    setIsModalVisible(true);
  };

  const showEditModal = (record) => {
    setEditingOption(record);
    form.setFieldsValue({
      pieces: record.pieces,
      weightKg: record.weightKg,
      price: record.price,
    });
    setIsModalVisible(true);
  };

  const handleDelete = async (id) => {
    try {
      await deletePaidOption(id);
      message.success("Опция удалена");
      fetchOptions();
    } catch (err) {
      console.error("Ошибка удаления", err);
      message.error("Не удалось удалить опцию");
    }
  };

  const handleCancel = () => {
    setIsModalVisible(false);
  };

  const handleFinish = async (values) => {
    try {
      if (editingOption) {
        await updatePaidOption(editingOption.paidOptionId, values);
        message.success("Опция обновлена");
      } else {
        await createPaidOption(values);
        message.success("Опция добавлена");
      }
      setIsModalVisible(false);
      fetchOptions();
    } catch (err) {
      console.error("Ошибка при сохранении", err);
      message.error("Не удалось сохранить опцию");
    }
  };

  const columns = [
    { title: "ID", dataIndex: "paidOptionId", key: "paidOptionId" },
    { title: "Мест", dataIndex: "pieces", key: "pieces" },
    { title: "Вес (кг)", dataIndex: "weightKg", key: "weightKg" },
    {
      title: "Цена",
      dataIndex: "price",
      key: "price",
      render: (value) => `${value.toFixed(2)} ₽`,
    },
    {
      title: "Действия",
      key: "action",
      render: (_, record) => (
        <>
          <Button
            type="link"
            onClick={() => showEditModal(record)}
            style={{ padding: 0, marginRight: 8 }}
          >
            Ред.
          </Button>
          <Popconfirm
            title="Удалить опцию?"
            onConfirm={() => handleDelete(record.paidOptionId)}
          >
            <Button danger size="small">
              Удалить
            </Button>
          </Popconfirm>
        </>
      ),
    },
  ];

  return (
    <div style={{ padding: 20 }}>
      <h2>Опции багажа</h2>
      <Button
        type="primary"
        style={{ marginBottom: 16 }}
        onClick={showAddModal}
      >
        Добавить опцию
      </Button>
      <Table
        columns={columns}
        dataSource={options}
        rowKey="paidOptionId"
        loading={loading}
      />

      <Modal
        title={editingOption ? "Редактировать опцию" : "Новая опция"}
        visible={isModalVisible}
        onCancel={handleCancel}
        onOk={() => form.submit()}
        okText="Сохранить"
        cancelText="Отмена"
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleFinish}
          initialValues={{
            pieces: 1,
            weightKg: 0,
            price: 0,
          }}
        >
          <Form.Item
            name="pieces"
            label="Количество мест"
            rules={[
              { required: true, message: "Введите количество мест" },
              { type: "number", min: 1, message: "Должно быть ≥ 1" },
            ]}
          >
            <InputNumber style={{ width: "100%" }} min={1} />
          </Form.Item>
          <Form.Item
            name="weightKg"
            label="Вес (кг)"
            rules={[
              { required: true, message: "Введите вес" },
              {
                type: "number",
                min: 0,
                message: "Не может быть отрицательным",
              },
            ]}
          >
            <InputNumber style={{ width: "100%" }} min={0} step={0.1} />
          </Form.Item>
          <Form.Item
            name="price"
            label="Цена (₽)"
            rules={[
              { required: true, message: "Введите цену" },
              {
                type: "number",
                min: 0,
                message: "Не может быть отрицательной",
              },
            ]}
          >
            <InputNumber style={{ width: "100%" }} min={0} step={0.01} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AdminBaggageOptions;
