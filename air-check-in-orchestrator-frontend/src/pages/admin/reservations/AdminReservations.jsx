import React, { useEffect, useState, useCallback } from "react";
import {
  Table,
  Button,
  Modal,
  Form,
  Select,
  Input,
  Spin,
  message,
  Popconfirm,
} from "antd";
import moment from "moment";
import {
  getAllReservations,
  getReservationById,
  createReservation,
  updateReservation,
  deleteReservation,
  getAllSessions,
  getAllBaggagePayments,
  getPassengers,
} from "../../../api/api";

const { Option } = Select;

const AdminReservations = () => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);

  const [sessions, setSessions] = useState([]);
  const [sessionsLoading, setSessionsLoading] = useState(false);

  const [departures, setDepartures] = useState([]);
  const [departuresLoading, setDeparturesLoading] = useState(false);

  const [passengers, setPassengers] = useState([]);
  const [passengersLoading, setPassengersLoading] = useState(false);

  const [modalVisible, setModalVisible] = useState(false);
  const [editingId, setEditingId] = useState(null);

  const [form] = Form.useForm();
  const token = localStorage.getItem("token");

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getAllReservations(token);
      setData(Array.isArray(res.data) ? res.data : []);
    } catch {
      message.error("Ошибка загрузки резервов");
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    const load = async () => {
      setSessionsLoading(true);
      try {
        const { data } = await getAllSessions();
        setSessions(Array.isArray(data) ? data : []);
      } catch {
        console.error("Ошибка загрузки сессий");
      } finally {
        setSessionsLoading(false);
      }
    };
    load();
  }, []);

  useEffect(() => {
    const load = async () => {
      setDeparturesLoading(true);
      try {
        const { data } = await getAllBaggagePayments();
        const ids = Array.from(
          new Set(
            Array.isArray(data) ? data.map((item) => item.departureId) : []
          )
        );
        setDepartures(ids);
      } catch {
        console.error("Ошибка загрузки рейсов");
      } finally {
        setDeparturesLoading(false);
      }
    };
    load();
  }, []);

  useEffect(() => {
    const load = async () => {
      setPassengersLoading(true);
      try {
        const { data } = await getPassengers();
        setPassengers(Array.isArray(data) ? data : []);
      } catch {
        console.error("Ошибка загрузки пассажиров");
      } finally {
        setPassengersLoading(false);
      }
    };
    load();
  }, []);

  const openModal = async (id) => {
    setEditingId(id);
    if (id) {
      try {
        const res = await getReservationById(id, token);
        form.setFieldsValue({
          dynamicId: res.data.dynamicId,
          departureId: res.data.departureId,
          passengerId: res.data.passengerId,
          seatNumber: res.data.seatNumber,
        });
      } catch {
        message.error("Ошибка загрузки данных резерва");
        return;
      }
    } else {
      form.resetFields();
    }
    setModalVisible(true);
  };

  const handleOk = async () => {
    try {
      const vals = await form.validateFields();
      if (editingId) {
        await updateReservation(editingId, vals, token);
        message.success("Резерв обновлён");
      } else {
        await createReservation(vals, token);
        message.success("Резерв создан");
      }
      setModalVisible(false);
      fetchData();
    } catch {
      message.error("Ошибка при сохранении");
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteReservation(id, token);
      message.success("Резерв удалён");
      fetchData();
    } catch {
      message.error("Ошибка удаления");
    }
  };

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const columns = [
    {
      title: "ID",
      dataIndex: "seatReservationId",
      key: "seatReservationId",
    },
    {
      title: "DynamicId",
      dataIndex: "dynamicId",
      key: "dynamicId",
    },
    {
      title: "Рейс",
      dataIndex: "departureId",
      key: "departureId",
    },
    {
      title: "Пассажир",
      dataIndex: "passengerId",
      key: "passengerId",
      render: (id) => {
        const p = passengers.find((x) => x.passengerId === id);
        return p ? `${p.lastName} (${id})` : id;
      },
    },
    {
      title: "Место",
      dataIndex: "seatNumber",
      key: "seatNumber",
    },
    {
      title: "Зарезервирован",
      dataIndex: "reservedAt",
      key: "reservedAt",
      render: (ts) => moment(ts).format("YYYY-MM-DD HH:mm"),
    },
    {
      title: "Действия",
      key: "actions",
      render: (_, record) => (
        <Popconfirm
          title="Удалить резерв?"
          onConfirm={() => handleDelete(record.seatReservationId)}
        >
          <Button danger size="small">
            Удалить
          </Button>
        </Popconfirm>
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
        Добавить резерв
      </Button>

      <Table
        columns={columns}
        dataSource={data}
        rowKey="seatReservationId"
        loading={loading || passengersLoading}
      />

      <Modal
        visible={modalVisible}
        title={editingId ? "Редактировать резерв" : "Новый резерв"}
        onOk={handleOk}
        onCancel={() => setModalVisible(false)}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="dynamicId"
            label="DynamicId"
            rules={[
              { required: true, message: "Пожалуйста, выберите DynamicId" },
            ]}
          >
            {sessionsLoading ? (
              <Spin />
            ) : (
              <Select placeholder="DynamicId" allowClear>
                {sessions.map((s) => (
                  <Option key={s.dynamicId} value={s.dynamicId}>
                    {s.dynamicId}
                  </Option>
                ))}
              </Select>
            )}
          </Form.Item>

          <Form.Item
            name="departureId"
            label="Рейс (DepartureId)"
            rules={[{ required: true, message: "Пожалуйста, выберите рейс" }]}
          >
            {departuresLoading ? (
              <Spin />
            ) : (
              <Select placeholder="Рейс" allowClear>
                {departures.map((dep) => (
                  <Option key={dep} value={dep}>
                    {dep}
                  </Option>
                ))}
              </Select>
            )}
          </Form.Item>

          <Form.Item
            name="passengerId"
            label="Пассажир"
            rules={[
              { required: true, message: "Пожалуйста, выберите пассажира" },
            ]}
          >
            {passengersLoading ? (
              <Spin />
            ) : (
              <Select placeholder="Пассажир" allowClear>
                {passengers.map((p) => (
                  <Option key={p.passengerId} value={p.passengerId}>
                    {`${p.lastName} (${p.passengerId})`}
                  </Option>
                ))}
              </Select>
            )}
          </Form.Item>

          <Form.Item
            name="seatNumber"
            label="Номер места"
            rules={[{ required: true, message: "Введите номер места" }]}
          >
            <Input placeholder="Например: 12A" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AdminReservations;
