import { Form, Input, Button, Card, message } from 'antd';
import { addBaggage } from '../../api/api';
import { motion } from 'framer-motion';

const BaggageForm = ({ setCurrent }) => {
    const token = localStorage.getItem('token');

    const onFinish = async (values) => {
        try {
            await addBaggage(values, token);
            message.success('Багаж успешно зарегистрирован');
            setCurrent('home');
        } catch {
            message.error('Ошибка при регистрации багажа');
            setCurrent('home');
        }
    };

    return (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <Card title="Регистрация багажа" style={{ width: 600, margin: '20px auto' }}>
                <Form onFinish={onFinish}>
                    <Form.Item name="dynamicId" rules={[{ required: true }]}>
                        <Input placeholder="DynamicId" />
                    </Form.Item>
                    <Form.Item name="pnr" rules={[{ required: true }]}>
                        <Input placeholder="PNR" />
                    </Form.Item>
                    <Form.Item name="passengerId" rules={[{ required: true }]}>
                        <Input placeholder="PassengerId" />
                    </Form.Item>
                    <Form.Item name="pieces" rules={[{ required: true }]}>
                        <Input type="number" placeholder="Количество мест" />
                    </Form.Item>
                    <Form.Item name="weight" rules={[{ required: true }]}>
                        <Input type="number" placeholder="Вес (кг)" />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit">Зарегистрировать багаж</Button>
                    </Form.Item>
                </Form>
            </Card>
        </motion.div>
    );
};

export default BaggageForm;
