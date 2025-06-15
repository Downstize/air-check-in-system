import React, { useState, useEffect } from "react";
import {
  Form,
  Select,
  Input,
  Button,
  Card,
  Descriptions,
  Progress,
  Spin,
  message,
} from "antd";
import { searchOrder, getAllSessions, getPassengers } from "../../api/api";
import { motion, AnimatePresence } from "framer-motion";

const { Option } = Select;

const OrderSearchForm = () => {
  const [form] = Form.useForm();
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [progress, setProgress] = useState(0);

  const [sessions, setSessions] = useState([]);
  const [sessionsLoading, setSessionsLoading] = useState(false);

  const [passengers, setPassengers] = useState([]);
  const [passengersLoading, setPassengersLoading] = useState(false);

  useEffect(() => {
    const loadSessions = async () => {
      setSessionsLoading(true);
      try {
        const { data } = await getAllSessions();
        setSessions(data || []);
      } catch (err) {
        console.error("Ошибка загрузки сессий", err);
      } finally {
        setSessionsLoading(false);
      }
    };
    loadSessions();
  }, []);

  useEffect(() => {
    const loadPassengers = async () => {
      setPassengersLoading(true);
      try {
        const { data } = await getPassengers();
        setPassengers(data || []);
      } catch (err) {
        console.error("Ошибка загрузки пассажиров", err);
      } finally {
        setPassengersLoading(false);
      }
    };
    loadPassengers();
  }, []);

  const handlePassengerSelect = (passengerId) => {
    if (!passengerId) {
      form.setFieldsValue({ LastName: "", PnrId: "" });
      return;
    }
    const p = passengers.find((x) => x.passengerId === passengerId);
    if (p) {
      form.setFieldsValue({ LastName: p.lastName, PnrId: p.pnrId || p.pnr });
    }
  };

  const startProgress = (onComplete) => {
    setProgress(0);
    setLoading(true);
    const step = 5;
    const interval = setInterval(() => {
      setProgress((prev) => {
        const next = prev + step;
        if (next >= 100) {
          clearInterval(interval);
          setLoading(false);
          onComplete();
          return 100;
        }
        return next;
      });
    }, 100);
  };

  const onFinish = async (values) => {
    try {
      const response = await searchOrder({
        DynamicId: values.DynamicId,
        LastName: values.LastName,
        PnrId: values.PnrId,
      });
      const payload = response.data ?? response;
      startProgress(() => {
        message.success("Заказ найден");
        setResult(payload);
      });
    } catch (error) {
      setLoading(false);
      setProgress(0);
      console.error(error);
      if (error.response && error.response.data && error.response.data.errors) {
        const apiErrors = error.response.data.errors;
        const fields = Object.entries(apiErrors).map(
          ([fieldName, messages]) => ({ name: fieldName, errors: messages })
        );
        form.setFields(fields);
      } else {
        message.error("Не удалось найти заказ");
      }
      setResult(null);
    }
  };

  const formatDate = (iso) =>
    iso
      ? new Date(iso).toLocaleString("ru-RU", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
          hour: "2-digit",
          minute: "2-digit",
        })
      : "—";

  const formatDateOnly = (iso) =>
    iso ? new Date(iso).toLocaleDateString("ru-RU") : "—";

  const order = result?.order;
  const segments = order?.segments ?? [];
  const paxList = order?.passengers ?? [];

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.5 }}
    >
      <Card title="Поиск заказа" style={{ width: 800, margin: "20px auto" }}>
        <Form
          form={form}
          onFinish={onFinish}
          layout="vertical"
          disabled={loading}
        >
          <Form.Item
            name="DynamicId"
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
              >
                {sessions.map((s) => (
                  <Option key={s.dynamicId} value={s.dynamicId}>
                    {s.dynamicId}
                  </Option>
                ))}
              </Select>
            )}
          </Form.Item>

          <Form.Item
            name="passengerId"
            label="Фамилия пассажира"
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
                onChange={handlePassengerSelect}
                allowClear
                filterOption={(input, option) =>
                  option.children.toLowerCase().includes(input.toLowerCase())
                }
              >
                {passengers.map((p) => (
                  <Option key={p.passengerId} value={p.passengerId}>
                    {`${p.lastName} (${p.passengerId})`}
                  </Option>
                ))}
              </Select>
            )}
          </Form.Item>

          <Form.Item name="LastName" style={{ display: "none" }}>
            <Input />
          </Form.Item>

          <Form.Item
            name="PnrId"
            label="PNR"
            rules={[{ required: true, message: "Пожалуйста, введите PNR" }]}
          >
            <Input placeholder="PNR" disabled />
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" block loading={loading}>
              Найти заказ
            </Button>
          </Form.Item>
        </Form>

        <AnimatePresence>
          {loading && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 10 }}
              transition={{ duration: 0.3 }}
              style={{ marginTop: 24 }}
            >
              <Progress percent={progress} status="active" />
            </motion.div>
          )}
        </AnimatePresence>
      </Card>

      {order && !loading && (
        <Card title="Данные заказа" style={{ width: 800, margin: "20px auto" }}>
          <Descriptions bordered column={1}>
            <Descriptions.Item label="PNR">{order.orderId}</Descriptions.Item>
            <Descriptions.Item label="Вес багажа">
              {order.luggageWeight ?? "—"} кг
            </Descriptions.Item>
            <Descriptions.Item label="Чек-ин оплачен">
              {order.paidCheckin ? "Да" : "Нет"}
            </Descriptions.Item>
            <Descriptions.Item label="Рейс">
              {segments[0]?.aircompanyCode} {segments[0]?.flightNumber}
            </Descriptions.Item>
            <Descriptions.Item label="Маршрут">
              {segments[0]?.departurePortCode} → {segments[0]?.arrivalPortCode}
            </Descriptions.Item>
            <Descriptions.Item label="Время вылета">
              {formatDate(segments[0]?.departureTime)}
            </Descriptions.Item>
            <Descriptions.Item label="Время прибытия">
              {formatDate(segments[0]?.arrivalTime)}
            </Descriptions.Item>
            <Descriptions.Item label="Статус рейса">
              {segments[0]?.flightStatus ?? "—"}
            </Descriptions.Item>
            <Descriptions.Item label="Открытие онлайн-регистрации">
              {formatDate(segments[0]?.webCheckInOpening)}
            </Descriptions.Item>
            <Descriptions.Item label="Закрытие онлайн-регистрации">
              {formatDate(segments[0]?.webCheckInClosing)}
            </Descriptions.Item>
          </Descriptions>

          {paxList.map((pax, index) => {
            const status = pax.checkInStatus;
            const isChecked = [
              "checked",
              "web_checked",
              "agent_checked",
            ].includes(status);
            return (
              <Card
                key={pax.passengerId}
                type="inner"
                title={`Пассажир ${index + 1}: ${pax.lastName} ${
                  pax.firstName
                }`}
                style={{ marginTop: 20 }}
              >
                <Descriptions bordered column={1}>
                  <Descriptions.Item label="Дата рождения">
                    {formatDateOnly(pax.birthDate)}
                  </Descriptions.Item>
                  <Descriptions.Item label="Место">
                    Место {pax.seatNumber} ({pax.seatLayerType})
                  </Descriptions.Item>
                  <Descriptions.Item label="Статус регистрации">
                    {isChecked ? "Зарегистрирован" : "Не зарегистрирован"}
                  </Descriptions.Item>
                  <Descriptions.Item label="Категория">
                    {pax.category}
                  </Descriptions.Item>
                  <Descriptions.Item label="Паспорт">
                    Номер: {pax.document?.number}
                    <br /> Страна выдачи: {pax.document?.issueCountryCode}
                    <br /> Национальность: {pax.document?.nationalityCode}
                    <br /> Действует до:{" "}
                    {formatDateOnly(pax.document?.expiryDate)}
                  </Descriptions.Item>
                  <Descriptions.Item label="Виза">
                    Номер: {pax.visaDocument?.number}
                    <br /> Место выдачи: {pax.visaDocument?.issuePlace}
                    <br /> Дата выдачи:{" "}
                    {formatDateOnly(pax.visaDocument?.issueDate)}
                    <br /> Страна назначения:{" "}
                    {pax.visaDocument?.applicCountryCode}
                    <br /> Место рождения: {pax.visaDocument?.birthPlace}
                  </Descriptions.Item>
                </Descriptions>
              </Card>
            );
          })}
        </Card>
      )}
    </motion.div>
  );
};

export default OrderSearchForm;
