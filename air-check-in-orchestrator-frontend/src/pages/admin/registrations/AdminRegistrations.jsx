import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Modal, Form, InputNumber, message, Popconfirm } from 'antd';
import {
  getAllRegistrationsAdmin,
  getRegistrationByIdAdmin,
  createRegistrationAdmin,
  updateRegistrationAdmin,
  deleteRegistrationAdmin
} from '../../../api/api';

const AdminRegistrations = () => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form] = Form.useForm();
  const token = localStorage.getItem('token');

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getAllRegistrationsAdmin(token);
      setData(res.data);
    } catch {
      message.error('Ошибка загрузки регистраций');
    } finally {
      setLoading(false);
    }
  }, [token]);

  const openModal = async id => {
    if (id) {
      setEditingId(id);
      const res = await getRegistrationByIdAdmin(id, token);
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
      if (editingId) await updateRegistrationAdmin(editingId, vals, token);
      else await createRegistrationAdmin(vals, token);
      message.success(editingId ? 'Обновлено' : 'Создано');
      setModalVisible(false);
      fetchData();
    } catch {
      message.error('Ошибка');
    }
  };

  const handleDelete = async id => {
    try {
      await deleteRegistrationAdmin(id, token);
      message.success('Удалено');
      fetchData();
    } catch {
      message.error('Ошибка удаления');
    }
  };

  useEffect(() => { fetchData(); }, [fetchData]);

  const columns = [
    { title: 'ID', dataIndex: 'registrationId', key: 'registrationId' },
    { title: 'Order ID', dataIndex: 'orderId', key: 'orderId' },
    { title: 'Багаж (шт.)', dataIndex: 'pieces', key: 'pieces' },
    { title: 'Вес (кг)', dataIndex: 'weightKg', key: 'weightKg' },
    {
      title: 'Действия',
      render: (_, r) => (
        <Popconfirm title="Удалить?" onConfirm={() => handleDelete(r.registrationId)}>
          <Button danger size="small">Удалить</Button>
        </Popconfirm>
      )
    }
  ];

  return (
    <div style={{ padding: 20 }}>
      <Button type="primary" onClick={() => openModal(null)} style={{ marginBottom: 16 }}>
        Добавить регистрацию
      </Button>
      <Table columns={columns} dataSource={data} rowKey="registrationId" loading={loading} />
      <Modal
        visible={modalVisible}
        title={editingId ? 'Редактировать регистрацию' : 'Новая регистрация'}
        onOk={handleOk}
        onCancel={() => setModalVisible(false)}
      >
        <Form form={form} layout="vertical">
          <Form.Item name="orderId" label="Order ID" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="pieces" label="Багаж (шт.)" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="weightKg" label="Вес (кг)" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AdminRegistrations;
