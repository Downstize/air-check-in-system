import React, { useEffect, useState, useCallback } from "react";
import {
  Table,
  Button,
  Modal,
  Form,
  InputNumber,
  Input,
  message,
  Popconfirm,
} from "antd";
import {
  getAllPayments,
  getPaymentById,
  createPayment,
  updatePayment,
  deletePayment,
} from "../../../api/api";

const AdminPayments = () => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form] = Form.useForm();
  const token = localStorage.getItem("token");

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getAllPayments(token);
      setData(res.data);
    } catch {
      message.error("Ошибка загрузки оплат");
    } finally {
      setLoading(false);
    }
  }, [token]);

  const openModal = async (id) => {
    if (id) {
      setEditingId(id);
      const res = await getPaymentById(id, token);
      form.setFieldsValue(res.data);
    } else {
      setEditingId(null);
      form.resetFields();
    }
    setModalVisible(true);
  };

  const handleOk = async () => {
    const vals = await form.validateFields();
    try {
      if (editingId) await updatePayment(editingId, vals, token);
      else await createPayment(vals, token);
      message.success(editingId ? "Обновлено" : "Создано");
      setModalVisible(false);
      fetchData();
    } catch {
      message.error("Ошибка");
    }
  };

  const handleDelete = async (id) => {
    try {
      await deletePayment(id, token);
      message.success("Удалено");
      fetchData();
    } catch {
      message.error("Ошибка удаления");
    }
  };

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const columns = [
    { title: "ID", dataIndex: "paymentId", key: "paymentId" },
    { title: "Order ID", dataIndex: "orderId", key: "orderId" },
    { title: "Сумма", dataIndex: "amount", key: "amount" },
    { title: "Статус", dataIndex: "status", key: "status" },
    {
      title: "Действия",
      key: "actions",
      render: (_, r) => (
        <>
          <Button onClick={() => openModal(r.paymentId)}>Изменить</Button>
          <Popconfirm
            title="Удалить?"
            onConfirm={() => handleDelete(r.paymentId)}
          >
            <Button danger style={{ marginLeft: 8 }}>
              Удалить
            </Button>
          </Popconfirm>
        </>
      ),
    },
  ];

  return (
    <div style={{ padding: 20 }}>
      <Button
        type="primary"
        onClick={() => openModal(null)}
        style={{ marginBottom: 16 }}
      >
        Добавить оплату
      </Button>
      <Table
        columns={columns}
        dataSource={data}
        rowKey="paymentId"
        loading={loading}
      />
      <Modal
        visible={modalVisible}
        title={editingId ? "Редактировать оплату" : "Новая оплата"}
        onOk={handleOk}
        onCancel={() => setModalVisible(false)}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="orderId"
            label="Order ID"
            rules={[{ required: true }]}
          >
            <InputNumber style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item name="amount" label="Сумма" rules={[{ required: true }]}>
            <InputNumber style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item name="status" label="Статус" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AdminPayments;
