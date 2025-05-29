import React, { useState, useEffect } from 'react';
import {
  Form,
  Input,
  Button,
  Checkbox,
  Card,
  Modal,
  Progress,
} from 'antd';
import { motion, AnimatePresence } from 'framer-motion';
import Confetti from 'react-confetti';
import { checkInPassenger } from '../../api/api';

const PassengerForm = ({ setCurrent }) => {
  const token = localStorage.getItem('token');

  const [loading, setLoading] = useState(false);
  const [progress, setProgress] = useState(0);
  const [isSuccess, setIsSuccess] = useState(false);

  const [dimensions, setDimensions] = useState({ width: 0, height: 0 });
  useEffect(() => {
    const update = () =>
      setDimensions({ width: window.innerWidth, height: window.innerHeight });
    update();
    window.addEventListener('resize', update);
    return () => window.removeEventListener('resize', update);
  }, []);

  const startProgress = () => {
    setLoading(true);
    setProgress(0);
    const step = 5;
    const interval = setInterval(() => {
      setProgress(p => {
        const next = p + step;
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

  const onFinish = async values => {
    try {
      await checkInPassenger(values, token);
      startProgress();
    } catch (err) {
      Modal.error({
        title: 'Ошибка',
        content: 'Не удалось зарегистрировать пассажира. Попробуйте ещё раз.',
      });
      setCurrent('home');
    }
  };

  const handleOk = () => {
    setIsSuccess(false);
    setCurrent('home');
  };

  return (
    <>
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.5 }}
      >
        <Card
          title="Регистрация пассажира"
          style={{ maxWidth: 600, margin: '20px auto' }}
        >
          <Form onFinish={onFinish} layout="vertical" disabled={loading}>
            <Form.Item
              name="dynamicId"
              label="DynamicId"
              rules={[{ required: true }]}
            >
              <Input placeholder="Введите DynamicId" />
            </Form.Item>
            <Form.Item
              name="lastName"
              label="Фамилия"
              rules={[{ required: true }]}
            >
              <Input placeholder="Введите фамилию" />
            </Form.Item>
            <Form.Item name="pnr" label="PNR" rules={[{ required: true }]}>
              <Input placeholder="Введите PNR" />
            </Form.Item>
            <Form.Item name="paidSeat" valuePropName="checked">
              <Checkbox>Платное место</Checkbox>
            </Form.Item>
            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                block
                loading={loading}
              >
                Зарегистрировать
              </Button>
            </Form.Item>
          </Form>

          {}
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

      {}
      <AnimatePresence>
        {isSuccess && (
          <Confetti
            width={dimensions.width}
            height={dimensions.height}
          />
        )}
      </AnimatePresence>

      {}
      <Modal
        centered
        open={isSuccess}
        title="🎉 Пассажир зарегистрирован!"
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
          transition={{ type: 'spring', stiffness: 300, damping: 20 }}
        >
          <p>Пассажир успешно добавлен в систему.</p>
        </motion.div>
      </Modal>
    </>
  );
};

export default PassengerForm;
