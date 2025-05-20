import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Modal, Form, InputNumber, message, Popconfirm } from 'antd';
import {
  getAllReservations,
  getReservationById,
  createReservation,
  updateReservation,
  deleteReservation
} from '../../../api/api';

const AdminReservations = () => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form] = Form.useForm();
  const token = localStorage.getItem('token');

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getAllReservations(token);
      setData(res.data);
    } catch {
      message.error('Ошибка загрузки резервов');
    } finally {
      setLoading(false);
    }
  }, [token]);

  const openModal = async id => {
    if (id) {
      setEditingId(id);
      const res = await getReservationById(id, token);
      form.setFieldsValue(res.data);
    } else {
      setEditingId(null);
      form.resetFields();
    }
    setModalVisible(true);
  };

  const handleOk = async () => {
    const vals = await form.validateFields();
    try {
      if (editingId) await updateReservation(editingId, vals, token);
      else await createReservation(vals, token);
      message.success(editingId ? 'Обновлено' : 'Создано');
      setModalVisible(false);
      fetchData();
    } catch {
      message.error('Ошибка');
    }
  };

  const handleDelete = async id => {
    try {
      await deleteReservation(id, token);
      message.success('Удалено');
      fetchData();
    } catch {
      message.error('Ошибка удаления');
    }
  };

  useEffect(() => { fetchData(); }, [fetchData]);

  const columns = [
    { title: 'ID', dataIndex: 'reservationId', key: 'reservationId' },
    { title: 'Order ID', dataIndex: 'orderId', key: 'orderId' },
    { title: 'Seat №', dataIndex: 'seatNumber', key: 'seatNumber' },
    {
      title: 'Действия',
      render: (_, r) => (
        <Popconfirm title="Удалить?" onConfirm={() => handleDelete(r.reservationId)}>
          <Button danger size="small">Удалить</Button>
        </Popconfirm>
      )
    }
  ];

  return (
    <div style={{ padding: 20 }}>
      <Button type="primary" onClick={() => openModal(null)} style={{ marginBottom: 16 }}>
        Добавить резерв
      </Button>
      <Table columns={columns} dataSource={data} rowKey="reservationId" loading={loading} />
      <Modal
        visible={modalVisible}
        title={editingId ? 'Редактировать резерв' : 'Новый резерв'}
        onOk={handleOk}
        onCancel={() => setModalVisible(false)}
      >
        <Form form={form} layout="vertical">
          <Form.Item name="orderId" label="Order ID" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="seatNumber" label="Seat №" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AdminReservations;
