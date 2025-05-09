import { Form, Input, Button, Checkbox, Card, message } from 'antd';
import { checkInPassenger } from '../api/api';
import { motion } from 'framer-motion';

const PassengerForm = ({ setCurrent }) => {
    const token = localStorage.getItem('token');

    const onFinish = async (values) => {
        try {
            await checkInPassenger(values, token);
            message.success('Пассажир успешно зарегистрирован');
            setCurrent('home');
        } catch {
            message.error('Ошибка при регистрации пассажира');
            setCurrent('home');
        }
    };

    return (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <Card title="Регистрация пассажира" style={{ width: 600, margin: '20px auto' }}>
                <Form onFinish={onFinish}>
                    <Form.Item name="dynamicId" rules={[{ required: true }]}>
                        <Input placeholder="DynamicId" />
                    </Form.Item>
                    <Form.Item name="lastName" rules={[{ required: true }]}>
                        <Input placeholder="Фамилия" />
                    </Form.Item>
                    <Form.Item name="pnr" rules={[{ required: true }]}>
                        <Input placeholder="PNR" />
                    </Form.Item>
                    <Form.Item name="paidSeat" valuePropName="checked">
                        <Checkbox>Платное место</Checkbox>
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit">Зарегистрировать</Button>
                    </Form.Item>
                </Form>
            </Card>
        </motion.div>
    );
};

export default PassengerForm;
