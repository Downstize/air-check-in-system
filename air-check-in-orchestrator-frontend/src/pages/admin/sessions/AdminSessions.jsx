import React, { useState, useEffect, useCallback } from 'react';
import { Table, Button, Modal, Form, Input, message, Popconfirm } from 'antd';
import { LockOutlined } from '@ant-design/icons';
import {
  getAllSessions,
  getSessionById,
  createSession,
  updateSession,
  deleteSession
} from '../../../api/api';

const AdminSessions = () => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form] = Form.useForm();

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getAllSessions();
      setData(res.data);
    } catch (err) {
      console.error(err);
      message.error('Ошибка загрузки сессий');
    } finally {
      setLoading(false);
    }
  }, []);

  const openModal = async (id) => {
    if (id !== null && id !== undefined) {
      setEditingId(id);
      try {
        const res = await getSessionById(id);
        form.setFieldsValue({ dynamicId: res.data.dynamicId });
      } catch (err) {
        console.error(err);
        message.error('Ошибка получения данных сессии');
        return;
      }
    } else {
      setEditingId(null);
      form.resetFields();
    }
    setModalVisible(true);
  };

  const handleOk = async () => {
    try {
      const vals = await form.validateFields();
      if (editingId) {
        await updateSession(editingId, vals);
        setData(prev => prev.map(item =>
          item.id === editingId ? { ...item, dynamicId: vals.dynamicId } : item
        ));
        message.success('Сессия обновлена');
      } else {
        const res = await createSession(vals);
        setData(prev => [...prev, res.data]);
        message.success('Сессия создана');
      }
      setModalVisible(false);
    } catch (err) {
      console.error(err);
      message.error('Ошибка сохранения сессии');
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteSession(id);
      setData(prev => prev.filter(item => item.id !== id));
      message.success('Сессия удалена');
    } catch (err) {
      console.error(err);
      message.error('Ошибка удаления сессии');
    }
  };

  useEffect(() => { fetchData(); }, [fetchData]);

  const columns = [
    { title: 'ID', dataIndex: 'id', key: 'id' },
    { title: 'DynamicId', dataIndex: 'dynamicId', key: 'dynamicId' },
    {
      title: 'Создано',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: d => new Date(d).toLocaleString()
    },
    {
      title: 'Действия',
      key: 'actions',
      render: (_, record) => (
        <>
          <Button onClick={() => openModal(record.id)}>Изменить</Button>
          <Popconfirm
            title="Удалить эту сессию?"
            onConfirm={() => handleDelete(record.id)}
          >
            <Button danger style={{ marginLeft: 8 }}>Удалить</Button>
          </Popconfirm>
        </>
      )
    }
  ];

  return (
    <div style={{ padding: 20 }}>
      <Button
        type="primary"
        onClick={() => openModal(null)}
        style={{ marginBottom: 16 }}
      >
        Добавить сессию
      </Button>
      <Table
        columns={columns}
        dataSource={data}
        rowKey="id"
        loading={loading}
      />

      <Modal
        visible={modalVisible}
        title={editingId ? 'Редактировать сессию' : 'Новая сессия'}
        onOk={handleOk}
        onCancel={() => setModalVisible(false)}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="dynamicId"
            label="DynamicId"
            rules={[{ required: true, message: 'Введите DynamicId' }]}
          >
            <Input prefix={<LockOutlined />} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AdminSessions;
