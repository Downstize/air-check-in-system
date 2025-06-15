import React, { useState, useEffect } from "react";
import { Form, Select, Input, Button, Card, Spin, Progress, Modal } from "antd";
import {
  registerPaid,
  getAllSessions,
  getAllBaggagePayments,
  getPassengers,
} from "../../api/api";
import { motion, AnimatePresence } from "framer-motion";
import Confetti from "react-confetti";

const { Option } = Select;

const PaidRegistrationForm = ({ setCurrent }) => {
  const [form] = Form.useForm();

  const [sessions, setSessions] = useState([]);
  const [sessionsLoading, setSessionsLoading] = useState(false);

  const [departures, setDepartures] = useState([]);
  const [departuresLoading, setDeparturesLoading] = useState(false);

  const [passengers, setPassengers] = useState([]);
  const [passengersLoading, setPassengersLoading] = useState(false);

  const [loading, setLoading] = useState(false);
  const [progress, setProgress] = useState(0);
  const [isSuccess, setIsSuccess] = useState(false);
  const [dimensions, setDimensions] = useState({ width: 0, height: 0 });

  useEffect(() => {
    const update = () =>
      setDimensions({ width: window.innerWidth, height: window.innerHeight });
    update();
    window.addEventListener("resize", update);
    return () => window.removeEventListener("resize", update);
  }, []);

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
    const loadDepartures = async () => {
      setDeparturesLoading(true);
      try {
        const { data } = await getAllBaggagePayments();
        const unique = Array.from(
          new Set((data || []).map((item) => item.departureId))
        );
        setDepartures(unique);
      } catch (err) {
        console.error("Ошибка загрузки рейсов", err);
      } finally {
        setDeparturesLoading(false);
      }
    };
    loadDepartures();
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

  const startProgress = () => {
    setProgress(0);
    setLoading(true);
    const step = 5;
    const timer = setInterval(() => {
      setProgress((prev) => {
        const next = prev + step;
        if (next >= 100) {
          clearInterval(timer);
          setLoading(false);
          setIsSuccess(true);
          return 100;
        }
        return next;
      });
    }, 100);
  };

  const onFinish = async (values) => {
    try {
      await registerPaid({
        dynamicId: values.dynamicId,
        departureId: values.departureId,
        passengerId: values.passengerId,
        seatNumber: values.seatNumber,
        amount: parseFloat(values.amount),
      });
      startProgress();
    } catch (err) {
      Modal.error({
        title: "Ошибка",
        content:
          "Не удалось зарегистрировать пассажира платно. Попробуйте ещё раз.",
      });
    }
  };

  const handleSuccessOk = () => {
    setIsSuccess(false);
    setCurrent("home");
  };

  return (
    <>
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.5 }}
      >
        <Card
          title="Платная регистрация пассажира"
          style={{ maxWidth: 600, margin: "20px auto" }}
        >
          <Form
            form={form}
            onFinish={onFinish}
            layout="vertical"
            disabled={loading}
          >
            <Form.Item
              name="dynamicId"
              label="DynamicId"
              rules={[{ required: true }]}
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
              name="departureId"
              label="DepartureId (рейс)"
              rules={[{ required: true }]}
            >
              {departuresLoading ? (
                <Spin />
              ) : (
                <Select
                  showSearch
                  placeholder="Выберите рейс"
                  optionFilterProp="children"
                  allowClear
                >
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
              label="Фамилия пассажира"
              rules={[{ required: true }]}
            >
              {passengersLoading ? (
                <Spin />
              ) : (
                <Select
                  showSearch
                  placeholder="Выберите пассажира"
                  optionFilterProp="children"
                  allowClear
                >
                  {passengers.map((p) => (
                    <Option
                      key={p.passengerId}
                      value={p.passengerId}
                    >{`${p.lastName} (${p.passengerId})`}</Option>
                  ))}
                </Select>
              )}
            </Form.Item>

            <Form.Item
              name="seatNumber"
              label="Номер места"
              rules={[{ required: true }]}
            >
              <Input placeholder="Например: 12A" />
            </Form.Item>

            <Form.Item
              name="amount"
              label="Сумма оплаты (в рублях)"
              rules={[{ required: true }]}
            >
              <Input type="number" placeholder="Например: 1500" />
            </Form.Item>

            <Form.Item>
              <Button type="primary" htmlType="submit" block loading={loading}>
                Зарегистрировать платно
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
      </motion.div>

      <AnimatePresence>
        {isSuccess && (
          <Confetti width={dimensions.width} height={dimensions.height} />
        )}
      </AnimatePresence>

      <Modal
        centered
        open={isSuccess}
        title="🎉 Пассажир зарегистрирован платно!"
        onOk={handleSuccessOk}
        onCancel={handleSuccessOk}
        width={420}
        footer={[
          <Button key="ok" type="primary" onClick={handleSuccessOk}>
            Закрыть
          </Button>,
        ]}
      >
        <motion.div
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0.8, opacity: 0 }}
          transition={{ type: "spring", stiffness: 300, damping: 20 }}
        >
          <p>Пассажир успешно зарегистрирован с оплатой в системе.</p>
        </motion.div>
      </Modal>
    </>
  );
};

export default PaidRegistrationForm;
