import React, { useState } from 'react';
import { Form, Input, Button, Card, message } from 'antd';
import { registerSession, validateSession } from '../api/api';
import { motion } from 'framer-motion';

const SessionForm = ({ setCurrent }) => {
    const [form] = Form.useForm();

    const handleRegister = async () => {
        try {
            const dynamicId = form.getFieldValue('dynamicId');
            await registerSession(dynamicId);
            message.success('Сессия успешно зарегистрирована');
        } catch (error) {
            console.error(error);
            message.error('Ошибка регистрации сессии');
        }
    };

    const handleValidate = async () => {
        try {
            const dynamicId = form.getFieldValue('dynamicId');
            await validateSession(dynamicId);
            message.success('Сессия действительна');
        } catch (error) {
            console.error(error);
            message.error('Сессия недействительна');
        }
    };

    return (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <Card title="Работа с DynamicId (сессия)" style={{ width: 600, margin: '20px auto' }}>
                <Form form={form} layout="vertical">
                    <Form.Item name="dynamicId" label="DynamicId" rules={[{ required: true }]}>
                        <Input placeholder="Введите DynamicId" />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" onClick={handleRegister}>
                            Зарегистрировать
                        </Button>
                        <Button style={{ marginLeft: '10px' }} onClick={handleValidate}>
                            Проверить
                        </Button>
                        <Button style={{ marginLeft: '10px' }} onClick={() => setCurrent('home')}>
                            На главную
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </motion.div>
    );
};

export default SessionForm;
