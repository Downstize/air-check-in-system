import React, { useState, useEffect } from 'react';
import { Table, Button, Card, message, Popconfirm } from 'antd';
import { getPassengers, deletePassenger } from '../api/api';
import { motion } from 'framer-motion';

const PassengerList = ({ setCurrent }) => {
    const [passengers, setPassengers] = useState([]);
    const [loading, setLoading] = useState(false);

    const loadPassengers = async () => {
        setLoading(true);
        try {
            const response = await getPassengers();
            setPassengers(response.data);
        } catch (error) {
            console.error(error);
            message.error('Ошибка загрузки пассажиров');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadPassengers();
    }, []);

    const handleDelete = async (id) => {
        try {
            await deletePassenger(id);
            message.success('Пассажир удалён');
            loadPassengers();
        } catch (error) {
            console.error(error);
            message.error('Ошибка при удалении пассажира');
        }
    };

    const columns = [
        {
            title: 'Фамилия',
            dataIndex: 'lastName',
            key: 'lastName',
        },
        {
            title: 'Имя',
            dataIndex: 'firstName',
            key: 'firstName',
        },
        {
            title: 'Дата рождения',
            dataIndex: 'birthDate',
            key: 'birthDate',
        },
        {
            title: 'Место',
            dataIndex: 'seatNumber',
            key: 'seatNumber',
        },
        {
            title: 'Статус регистрации',
            dataIndex: 'checkInStatus',
            key: 'checkInStatus',
        },
        {
            title: 'Действия',
            key: 'actions',
            render: (text, record) => (
                <Popconfirm
                    title="Удалить пассажира?"
                    onConfirm={() => handleDelete(record.passengerId)}
                    okText="Да"
                    cancelText="Нет"
                >
                    <Button danger>Удалить</Button>
                </Popconfirm>
            ),
        }
    ];

    return (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <Card title="Список пассажиров" style={{ width: '90%', margin: '20px auto' }}>
                <Button type="primary" onClick={() => setCurrent('home')} style={{ marginBottom: '10px' }}>
                    На главную
                </Button>
                <Table
                    dataSource={passengers}
                    columns={columns}
                    rowKey="passengerId"
                    loading={loading}
                    pagination={{ pageSize: 10 }}
                />
            </Card>
        </motion.div>
    );
};

export default PassengerList;
