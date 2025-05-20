import React, { useState } from 'react';
import { Form, InputNumber, Button, message, Card } from 'antd';
import { cancelBaggage } from '../../api/api';

const CancelBaggage = () => {
  const [loading, setLoading] = useState(false);
  const onFinish = async vals => {
    setLoading(true);
    try {
      await cancelBaggage(vals);
      message.success('Багаж отменен');
    } catch {
      message.error('Ошибка отмены');
    } finally {
      setLoading(false);
    }
  };
  return (
    <Card title="Отмена багажа" style={{ maxWidth: 400, margin: '20px auto' }}>
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
        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading}>
            Отменить
          </Button>
        </Form.Item>
      </Form>
    </Card>
  );
};

export default CancelBaggage;
