import React from 'react';
import { Form, Input, Button, Card, message } from 'antd';
import { registerPaid } from '../api/api';
import { motion } from 'framer-motion';

const PaidRegistrationForm = ({ setCurrent }) => {

    const onFinish = async (values) => {
        try {
            const response = await registerPaid({
                dynamicId: values.dynamicId,
                departureId: values.departureId,
                passengerId: values.passengerId,
                seatNumber: values.seatNumber,
                amount: parseFloat(values.amount)
            });
            message.success('Пассажир успешно зарегистрирован с оплатой');
            console.log(response);
        } catch (error) {
            console.error(error);
            message.error('Ошибка платной регистрации');
        }
    };

    return (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <Card title="Платная регистрация пассажира" style={{ width: 600, margin: '20px auto' }}>
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
                    <Form.Item name="amount" label="Сумма оплаты (в рублях)" rules={[{ required: true }]}>
                        <Input type="number" placeholder="Amount" />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit">Зарегистрировать платно</Button>
                        <Button style={{ marginLeft: '10px' }} onClick={() => setCurrent('home')}>
                            На главную
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </motion.div>
    );
};

export default PaidRegistrationForm;
