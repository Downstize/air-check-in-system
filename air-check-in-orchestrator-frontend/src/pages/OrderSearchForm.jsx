import React, { useState } from 'react';
import { Form, Input, Button, Card, message, Descriptions } from 'antd';
import { searchOrder } from '../api/api';
import { motion } from 'framer-motion';

const OrderSearchForm = ({ setCurrent }) => {
    const [result, setResult] = useState(null);

    const onFinish = async (values) => {
        try {
            const response = await searchOrder({
                dynamicId: values.dynamicId,
                surname: values.surname,
                ticketNumber: values.ticketNumber
            });
            message.success('Заказ найден');
            setResult(response);
        } catch (error) {
            console.error(error);
            message.error('Не удалось найти заказ');
            setResult(null);
        }
    };

    return (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <Card title="Поиск заказа" style={{ width: 700, margin: '20px auto' }}>
                <Form onFinish={onFinish} layout="vertical">
                    <Form.Item name="dynamicId" label="DynamicId" rules={[{ required: true }]}>
                        <Input placeholder="DynamicId" />
                    </Form.Item>
                    <Form.Item name="surname" label="Фамилия пассажира" rules={[{ required: true }]}>
                        <Input placeholder="Фамилия" />
                    </Form.Item>
                    <Form.Item name="ticketNumber" label="Номер билета" rules={[{ required: true }]}>
                        <Input placeholder="Номер билета" />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit">Найти заказ</Button>
                        <Button style={{ marginLeft: '10px' }} onClick={() => setCurrent('home')}>
                            На главную
                        </Button>
                    </Form.Item>
                </Form>
            </Card>

            {result && (
                <Card title="Данные заказа" style={{ width: 700, margin: '20px auto', marginTop: '20px' }}>
                    <Descriptions bordered column={1}>
                        <Descriptions.Item label="PNR">{result.pnr}</Descriptions.Item>
                        <Descriptions.Item label="Рейс">{result.flightNumber}</Descriptions.Item>
                        <Descriptions.Item label="Дата вылета">{result.departureDate}</Descriptions.Item>
                        <Descriptions.Item label="Статус">{result.status}</Descriptions.Item>
                    </Descriptions>

                    {result.passengers && result.passengers.length > 0 && (
                        <div style={{ marginTop: '20px' }}>
                            <h3>Пассажиры:</h3>
                            {result.passengers.map((pax, index) => (
                                <Card key={index} type="inner" title={`Пассажир ${index + 1}`}>
                                    <p><b>Фамилия:</b> {pax.lastName}</p>
                                    <p><b>Имя:</b> {pax.firstName}</p>
                                    <p><b>Дата рождения:</b> {pax.birthDate}</p>
                                    <p><b>Место:</b> {pax.seatNumber}</p>
                                    <p><b>Статус регистрации:</b> {pax.checkInStatus}</p>
                                </Card>
                            ))}
                        </div>
                    )}
                </Card>
            )}
        </motion.div>
    );
};

export default OrderSearchForm;
