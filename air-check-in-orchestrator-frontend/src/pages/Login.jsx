import { useState } from 'react';
import { Button, Form, Input, Card, message } from 'antd';
import { login } from '../api/api';
import { useNavigate } from 'react-router-dom';

const Login = () => {
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const onFinish = async (values) => {
        setLoading(true);
        try {
            const res = await login(values.username, values.password);
            localStorage.setItem('token', res.data.token);
            navigate('/');
        } catch {
            message.error('Неверный логин или пароль');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card title="Вход" style={{ width: 300, margin: '100px auto' }}>
            <Form onFinish={onFinish}>
                <Form.Item name="username" rules={[{ required: true, message: 'Введите логин' }]}>
                    <Input placeholder="Логин" />
                </Form.Item>
                <Form.Item name="password" rules={[{ required: true, message: 'Введите пароль' }]}>
                    <Input.Password placeholder="Пароль" />
                </Form.Item>
                <Form.Item>
                    <Button type="primary" htmlType="submit" loading={loading} block>
                        Войти
                    </Button>
                </Form.Item>
            </Form>
        </Card>
    );
};

export default Login;
