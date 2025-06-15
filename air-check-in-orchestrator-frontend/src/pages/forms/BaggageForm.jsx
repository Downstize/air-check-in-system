import React, { useState, useEffect } from "react";
import { Form, Select, Input, Button, Card, Modal, Progress, Spin } from "antd";
import { motion, AnimatePresence } from "framer-motion";
import Confetti from "react-confetti";
import { addBaggage, getAllSessions, getPassengers } from "../../api/api";

const { Option } = Select;

const BaggageForm = ({ setCurrent }) => {
  const [form] = Form.useForm();
  const token = localStorage.getItem("token");

  const [loading, setLoading] = useState(false);
  const [progress, setProgress] = useState(0);
  const [isSuccess, setIsSuccess] = useState(false);
  const [dimensions, setDimensions] = useState({ width: 0, height: 0 });

  const [sessions, setSessions] = useState([]);
  const [sessionsLoading, setSessionsLoading] = useState(false);

  const [passengers, setPassengers] = useState([]);
  const [passengersLoading, setPassengersLoading] = useState(false);

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
      form.setFieldsValue({ pnr: "" });
      return;
    }
    const p = passengers.find((x) => x.passengerId === passengerId);
    if (p) {
      form.setFieldsValue({ pnr: p.pnrId || p.pnr });
    }
  };

  const startProgress = () => {
    setLoading(true);
    setProgress(0);
    const step = 5;
    const interval = setInterval(() => {
      setProgress((prev) => {
        const next = prev + step;
        if (next >= 100) {
          clearInterval(interval);
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
      await addBaggage(values, token);
      startProgress();
    } catch (err) {
      Modal.error({
        title: "Ошибка",
        content: "Не удалось зарегистрировать багаж. Попробуйте ещё раз.",
      });
      setCurrent("home");
    }
  };

  const handleOk = () => {
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
          title="Регистрация багажа"
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

            <Form.Item name="pnr" label="PNR" rules={[{ required: true }]}>
              <Input placeholder="PNR" disabled />
            </Form.Item>

            <Form.Item
              name="passengerId"
              label="Фамилия (PassengerId)"
              rules={[{ required: true }]}
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

            <Form.Item
              name="pieces"
              label="Количество мест"
              rules={[{ required: true }]}
            >
              <Input type="number" placeholder="Количество мест" />
            </Form.Item>

            <Form.Item
              name="weight"
              label="Вес (кг)"
              rules={[{ required: true }]}
            >
              <Input type="number" placeholder="Вес (кг)" />
            </Form.Item>

            <Form.Item>
              <Button type="primary" htmlType="submit" block loading={loading}>
                Зарегистрировать багаж
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
        title="🎉 Багаж зарегистрирован!"
        onOk={handleOk}
        onCancel={handleOk}
        width={420}
        footer={[
          <Button key="ok" type="primary" onClick={handleOk}>
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
          <p>Багаж успешно добавлен в систему.</p>
        </motion.div>
      </Modal>
    </>
  );
};

export default BaggageForm;
