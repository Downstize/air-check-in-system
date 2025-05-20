import React, { useState } from 'react';
import { Form, InputNumber, Button, message, Card } from 'antd';
import { registerBaggage } from '../../api/api';

const RegisterBaggage = () => {
  const [loading, setLoading] = useState(false);
  const onFinish = async vals => {
    setLoading(true);
    try {
      await registerBaggage(vals);
      message.success('Багаж зарегистрирован');
    } catch {
      message.error('Ошибка регистрации');
    } finally {
      setLoading(false);
    }
  };
  return (
    <Card title="Регистрация багажа" style={{ maxWidth: 400, margin: '20px auto' }}>
      <Form onFinish={onFinish} layout="vertical">
        <Form.Item name="dynamicId" label="DynamicId" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item name="passengerId" label="Passenger ID" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item name="departureId" label="Departure ID" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item name="pieces" label="Pieces" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item name="weightKg" label="Weight (kg)" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading}>
            Зарегистрировать
          </Button>
        </Form.Item>
      </Form>
    </Card>
  );
};

export default RegisterBaggage;
