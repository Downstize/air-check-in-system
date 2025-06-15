import React, { useState, useEffect } from "react";
import { Form, Select, Input, Button, Card, Spin, message } from "antd";
import {
  getAllowance,
  getAllSessions,
  getPassengers,
  searchOrder,
} from "../../api/api";

const { Option } = Select;

const Allowance = ({ setCurrent }) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [sessions, setSessions] = useState([]);
  const [sessionsLoading, setSessionsLoading] = useState(false);
  const [passengers, setPassengers] = useState([]);
  const [passengersLoading, setPassengersLoading] = useState(false);
  const [allowance, setAllowance] = useState(null);

  useEffect(() => {
    (async () => {
      setSessionsLoading(true);
      try {
        const { data } = await getAllSessions();
        setSessions(data || []);
      } catch (err) {
        console.error("Ошибка загрузки DynamicId", err);
      } finally {
        setSessionsLoading(false);
      }
    })();
  }, []);

  useEffect(() => {
    (async () => {
      setPassengersLoading(true);
      try {
        const { data } = await getPassengers();
        setPassengers(data || []);
      } catch (err) {
        console.error("Ошибка загрузки пассажиров", err);
      } finally {
        setPassengersLoading(false);
      }
    })();
  }, []);

  const handlePassengerChange = async (passengerId) => {
    form.setFieldsValue({ orderId: "" });

    const dynamicId = form.getFieldValue("dynamicId");
    if (!dynamicId) {
      message.warning("Сначала выберите DynamicId");
      return;
    }
    if (!passengerId) return;

    const p = passengers.find((x) => x.passengerId === passengerId);
    if (!p) return;

    try {
      const { data } = await searchOrder({
        dynamicId: dynamicId,
        lastName: p.lastName,
        pnrId: p.pnrId,
      });
      const oid = data.order?.orderId;
      if (oid) {
        form.setFieldsValue({ orderId: oid });
      } else {
        message.warning("OrderId не найден");
      }
    } catch (err) {
      console.error("Ошибка поиска Order:", err);
      message.error("Не удалось получить OrderId");
    }
  };

  const onFinish = async (values) => {
    setLoading(true);
    try {
      const res = await getAllowance(
        values.dynamicId,
        values.orderId,
        values.passengerId
      );
      setAllowance(res.data);
    } catch {
      message.error("Ошибка получения нормы провоза");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card title="Норма багажа" style={{ maxWidth: 400, margin: "20px auto" }}>
      <Form
        form={form}
        onFinish={onFinish}
        layout="vertical"
        disabled={loading}
      >
        {}
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
            <Select
              showSearch
              placeholder="Выберите DynamicId"
              optionFilterProp="children"
              allowClear
              onChange={() => form.setFieldsValue({ orderId: "" })}
            >
              {sessions.map((s) => (
                <Option key={s.dynamicId} value={s.dynamicId}>
                  {s.dynamicId}
                </Option>
              ))}
            </Select>
          )}
        </Form.Item>

        {}
        <Form.Item
          name="passengerId"
          label="Passenger ID"
          rules={[
            { required: true, message: "Пожалуйста, выберите пассажира" },
          ]}
        >
          {passengersLoading ? (
            <Spin />
          ) : (
            <Select
              showSearch
              placeholder="Выберите пассажира"
              optionFilterProp="children"
              allowClear
              onChange={handlePassengerChange}
            >
              {passengers.map((p) => (
                <Option key={p.passengerId} value={p.passengerId}>
                  {`${p.lastName} (${p.passengerId})`}
                </Option>
              ))}
            </Select>
          )}
        </Form.Item>

        {}
        <Form.Item
          name="orderId"
          label="Order ID"
          rules={[
            { required: true, message: "OrderId заполняется автоматически" },
          ]}
        >
          <Input placeholder="Сначала выберите пассажира" disabled />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading} block>
            Получить норму
          </Button>
        </Form.Item>
      </Form>

      {allowance != null && (
        <div style={{ marginTop: 16, textAlign: "center" }}>
          <b>Норма:</b> {JSON.stringify(allowance)}
        </div>
      )}
    </Card>
  );
};

export default Allowance;
