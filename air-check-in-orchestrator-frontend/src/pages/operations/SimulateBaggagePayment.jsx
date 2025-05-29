import React, { useState } from 'react';
import { Form, Input, InputNumber, Button, message, Card } from 'antd';
import { simulateBaggagePayment } from '../../api/api';

const SimulateBaggagePayment = () => {
  const [loading, setLoading] = useState(false);

  const onFinish = async (vals) => {
    setLoading(true);
    try {
      await simulateBaggagePayment(
        vals.dynamicId,
        vals.passengerId,
        vals.departureId,
        vals.amount
      );
      message.success('Платеж багажа смоделирован');
    } catch {
      message.error('Ошибка при симуляции платежа');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card title="Симуляция оплаты багажа" style={{ maxWidth: 400, margin: '20px auto' }}>
      <Form
        onFinish={onFinish}
        layout="vertical"
      >
        <Form.Item
          name="dynamicId"
          label="DynamicId"
          rules={[{ required: true, message: 'Введите DynamicId' }]}
        >
          <Input
            placeholder="Например: ABC123"
            allowClear
          />
        </Form.Item>

        <Form.Item
          name="passengerId"
          label="Passenger ID"
          rules={[{ required: true, message: 'Введите Passenger ID' }]}
        >
          <Input
            placeholder="Например: 456XYZ"
            allowClear
          />
        </Form.Item>

        <Form.Item
          name="departureId"
          label="Departure ID"
          rules={[{ required: true, message: 'Введите Departure ID' }]}
        >
          <Input
            placeholder="Например: DPT789"
            allowClear
          />
        </Form.Item>

        <Form.Item
          name="amount"
          label="Сумма"
          rules={[{ required: true, message: 'Введите сумму' }]}
        >
          <InputNumber
            style={{ width: '100%' }}
            min={0}
            step={0.01}
            placeholder="0.00"
          />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block loading={loading}>
            Симулировать
          </Button>
        </Form.Item>
      </Form>
    </Card>
  );
};

export default SimulateBaggagePayment;
