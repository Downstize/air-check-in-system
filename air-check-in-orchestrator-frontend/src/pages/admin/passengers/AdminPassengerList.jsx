import React, { useState, useEffect } from "react";
import {
  Table,
  Button,
  Modal,
  Form,
  Input,
  message,
  Popconfirm,
  Space,
  Checkbox,
} from "antd";
import {
  getAllAdminPassengers,
  getAdminPassengerById,
  createAdminPassenger,
  updateAdminPassenger,
  deleteAdminPassenger,
} from "../../../api/api";

const AdminPassengerList = ({ setCurrent }) => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [form] = Form.useForm();
  const [editingId, setEditingId] = useState(null);

  const fetchData = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("Токен не найден");
      const res = await getAllAdminPassengers(token);
      setData(res.data);
    } catch (error) {
      console.error("Ошибка загрузки данных:", error);
      message.error("Не удалось загрузить список пассажиров");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const openModal = async (id) => {
    const token = localStorage.getItem("token");
    if (!token) {
      message.error("Неавторизован");
      return;
    }

    if (id) {
      setEditingId(id);
      try {
        const res = await getAdminPassengerById(id, token);
        const data = res.data;
        form.setFieldsValue({
          ...data,
          birthDate: data.birthDate?.substring(0, 10),
          remarks: data.remarks ?? [],
          apis: data.apis ?? 0,
          document: {
            ...(data.document ?? {}),
            birthDate: data.document?.birthDate?.substring(0, 10),
            expiryDate: data.document?.expiryDate?.substring(0, 10),
          },
          visaDocument: {
            ...(data.visaDocument ?? {}),
            issueDate: data.visaDocument?.issueDate?.substring(0, 10),
          },
        });
      } catch {
        message.error("Ошибка при загрузке пассажира");
        return;
      }
    } else {
      setEditingId(null);
      form.resetFields();
    }
    setModalVisible(true);
  };

  const handleOk = async () => {
    const token = localStorage.getItem("token");
    if (!token) {
      message.error("Неавторизован");
      return;
    }

    try {
      const values = await form.validateFields();
      console.log("Значения формы:", values);

      if (values.remarks === undefined) values.remarks = [];
      if (values.apis === undefined) values.apis = 0;

      if (editingId) {
        values.passengerId = editingId;
        await updateAdminPassenger(editingId, values);
        message.success("Пассажир успешно обновлён");
      } else {
        await createAdminPassenger(values);
        message.success("Пассажир успешно добавлен");
      }

      setModalVisible(false);
      fetchData();
    } catch (error) {
      console.error("Ошибка при сохранении:", error);
      message.error("Ошибка при сохранении. Проверьте корректность данных");
    }
  };

  const handleDelete = async (id) => {
    const token = localStorage.getItem("token");
    if (!token) {
      message.error("Неавторизован");
      return;
    }

    try {
      await deleteAdminPassenger(id, token);
      message.success("Пассажир удалён");
      fetchData();
    } catch (error) {
      message.error("Ошибка при удалении");
    }
  };

  const columns = [
    { title: "ID", dataIndex: "passengerId", key: "passengerId" },
    { title: "Имя", dataIndex: "firstName", key: "firstName" },
    { title: "Фамилия", dataIndex: "lastName", key: "lastName" },
    { title: "PNR", dataIndex: "pnrId", key: "pnrId" },
    {
      title: "Действия",
      key: "actions",
      render: (_, record) => (
        <Space>
          <Button type="link" onClick={() => openModal(record.passengerId)}>
            Редактировать
          </Button>
          <Popconfirm
            title="Удалить пассажира?"
            onConfirm={() => handleDelete(record.passengerId)}
            okText="Да"
            cancelText="Нет"
          >
            <Button type="link" danger>
              Удалить
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: "24px" }}>
      <Button
        type="primary"
        onClick={() => openModal(null)}
        style={{ marginBottom: 16 }}
      >
        Добавить пассажира
      </Button>
      <Table
        columns={columns}
        dataSource={data}
        rowKey="passengerId"
        loading={loading}
        pagination={{ pageSize: 8 }}
      />
      <Modal
        open={modalVisible}
        title={editingId ? "Редактирование пассажира" : "Добавление пассажира"}
        onOk={handleOk}
        onCancel={() => setModalVisible(false)}
        destroyOnClose
        okText="Сохранить"
        cancelText="Отмена"
        width={800}
      >
        <Form form={form} layout="vertical">
          <Form.Item name="passengerId" hidden>
            <Input />
          </Form.Item>

          <Form.Item name="pnrId" label="PNR" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="paxNo" label="Pax No" rules={[{ required: true }]}>
            <Input type="number" />
          </Form.Item>
          <Form.Item name="firstName" label="Имя" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item
            name="lastName"
            label="Фамилия"
            rules={[{ required: true }]}
          >
            <Input />
          </Form.Item>
          <Form.Item name="birthDate" label="Дата рождения">
            <Input type="date" />
          </Form.Item>
          <Form.Item name="category" label="Категория">
            <Input />
          </Form.Item>
          <Form.Item name="checkInStatus" label="Статус регистрации">
            <Input />
          </Form.Item>
          <Form.Item name="reason" label="Причина">
            <Input />
          </Form.Item>
          <Form.Item name="seatsOccupied" label="Мест занято">
            <Input type="number" />
          </Form.Item>
          <Form.Item name="eticket" valuePropName="checked">
            <Checkbox>Электронный билет</Checkbox>
          </Form.Item>
          <Form.Item name="seatNumber" label="Место">
            <Input />
          </Form.Item>
          <Form.Item name="seatStatus" label="Статус места">
            <Input />
          </Form.Item>
          <Form.Item name="seatLayerType" label="Класс места">
            <Input />
          </Form.Item>
          <Form.Item name="bookingId" label="ID брони">
            <Input type="number" />
          </Form.Item>

          <Form.Item name="remarks" label="Заметки">
            <Input.TextArea rows={3} />
          </Form.Item>

          <Form.Item name="apis" label="APIS">
            <Input type="number" />
          </Form.Item>

          <h3>Документ</h3>
          <Form.Item name={["document", "type"]} label="Тип">
            <Input />
          </Form.Item>
          <Form.Item name={["document", "number"]} label="Номер">
            <Input />
          </Form.Item>
          <Form.Item
            name={["document", "issueCountryCode"]}
            label="Страна выдачи"
          >
            <Input />
          </Form.Item>
          <Form.Item name={["document", "nationalityCode"]} label="Гражданство">
            <Input />
          </Form.Item>
          <Form.Item
            name={["document", "birthDate"]}
            label="Дата рождения (док)"
          >
            <Input type="date" />
          </Form.Item>
          <Form.Item name={["document", "expiryDate"]} label="Срок действия">
            <Input type="date" />
          </Form.Item>

          <h3>Виза</h3>
          <Form.Item
            name={["visaDocument", "birthPlace"]}
            label="Место рождения"
          >
            <Input />
          </Form.Item>
          <Form.Item name={["visaDocument", "number"]} label="Номер визы">
            <Input />
          </Form.Item>
          <Form.Item name={["visaDocument", "issuePlace"]} label="Место выдачи">
            <Input />
          </Form.Item>
          <Form.Item name={["visaDocument", "issueDate"]} label="Дата выдачи">
            <Input type="date" />
          </Form.Item>
          <Form.Item
            name={["visaDocument", "applicCountryCode"]}
            label="Страна подачи"
          >
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AdminPassengerList;
