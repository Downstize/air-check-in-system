import React from 'react';
import { Form, Input, Button, Card, message } from 'antd';
import { reserveSeat } from '../../api/api';
import { motion } from 'framer-motion';

const SeatReserveForm = ({ setCurrent }) => {

    const onFinish = async (values) => {
        try {
            const response = await reserveSeat({
                dynamicId: values.dynamicId,
                departureId: values.departureId,
                passengerId: values.passengerId,
                seatNumber: values.seatNumber
            });
            message.success('Место успешно зарезервировано');
            console.log(response);
        } catch (error) {
            console.error(error);
            message.error('Ошибка резервирования места');
        }
    };

    return (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <Card title="Резервирование места" style={{ width: 600, margin: '20px auto' }}>
                <Form onFinish={onFinish} layout="vertical">
                    <Form.Item name="dynamicId" label="DynamicId" rules={[{ required: true }]}>
                        <Input placeholder="DynamicId" />
                    </Form.Item>
                    <Form.Item name="departureId" label="DepartureId (рейс)" rules={[{ required: true }]}>
                        <Input placeholder="DepartureId" />
                    </Form.Item>
                    <Form.Item name="passengerId" label="PassengerId" rules={[{ required: true }]}>
                        <Input placeholder="PassengerId" />
                    </Form.Item>
                    <Form.Item name="seatNumber" label="Номер места" rules={[{ required: true }]}>
                        <Input placeholder="SeatNumber (например 12A)" />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit">Зарезервировать место</Button>
                        <Button style={{ marginLeft: '10px' }} onClick={() => setCurrent('home')}>
                            На главную
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </motion.div>
    );
};

export default SeatReserveForm;
