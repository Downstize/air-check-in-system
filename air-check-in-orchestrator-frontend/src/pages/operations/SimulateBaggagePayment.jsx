import React, { useState } from 'react';
import { Form, InputNumber, Button, message, Card } from 'antd';
import { simulateBaggagePayment } from '../../api/api';

const SimulateBaggagePayment = () => {
  const [loading, setLoading] = useState(false);
  const onFinish = async vals => {
    setLoading(true);
    try {
      await simulateBaggagePayment(vals.dynamicId, vals.passengerId, vals.departureId, vals.amount);
      message.success('Платеж багажа смоделирован');
    } catch {
      message.error('Ошибка');
    } finally {
      setLoading(false);
    }
  };
  return (
    <Card title="Симуляция оплаты багажа" style={{ maxWidth: 400, margin: '20px auto' }}>
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
        <Form.Item name="amount" label="Amount" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading}>
            Симулировать
          </Button>
        </Form.Item>
      </Form>
    </Card>
  );
};

export default SimulateBaggagePayment;
