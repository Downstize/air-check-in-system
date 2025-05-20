import React, { useState } from 'react';
import { Form, InputNumber, Button, message, Card } from 'antd';
import { getAllowance } from '../../api/api';

const Allowance = () => {
  const [loading, setLoading] = useState(false);
  const [allowance, setAllowance] = useState(null);
  const onFinish = async vals => {
    setLoading(true);
    try {
      const res = await getAllowance(vals.dynamicId, vals.orderId, vals.passengerId);
      setAllowance(res.data);
    } catch {
      message.error('Ошибка');
    } finally {
      setLoading(false);
    }
  };
  return (
    <Card title="Норма багажа" style={{ maxWidth: 400, margin: '20px auto' }}>
      <Form onFinish={onFinish} layout="vertical">
        <Form.Item name="dynamicId" label="DynamicId" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item name="orderId" label="Order ID" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item name="passengerId" label="Passenger ID" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading}>
            Получить норму
          </Button>
        </Form.Item>
      </Form>
      {allowance != null && (
        <div style={{ marginTop: 16, textAlign: 'center' }}>
          <b>Норма:</b> {JSON.stringify(allowance)}
        </div>
      )}
    </Card>
  );
};

export default Allowance;
