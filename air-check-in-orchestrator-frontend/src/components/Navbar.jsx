import { Menu } from 'antd';
import { motion } from 'framer-motion';
import {
    HomeOutlined,
    UserOutlined,
    ShoppingOutlined,
    SearchOutlined,
    AppstoreAddOutlined,
    CheckCircleOutlined,
    CreditCardOutlined,
    TeamOutlined,
    LockOutlined
} from '@ant-design/icons';

const Navbar = ({ current, setCurrent }) => {

    const items = [
        {
            key: 'home',
            icon: <HomeOutlined />,
            label: 'Главная',
            onTitleClick: () => setCurrent('home')
        },
        {
            key: 'orchestrator',
            label: 'Оркестратор',
            children: [
                {
                    key: 'passenger',
                    icon: <UserOutlined />,
                    label: 'Регистрация пассажира',
                    onClick: () => setCurrent('passenger')
                },
                {
                    key: 'baggage',
                    icon: <ShoppingOutlined />,
                    label: 'Регистрация багажа',
                    onClick: () => setCurrent('baggage')
                }
            ]
        },
        {
            key: 'registration',
            label: 'Регистрация',
            children: [
                {
                    key: 'searchOrder',
                    icon: <SearchOutlined />,
                    label: 'Поиск заказа',
                    onClick: () => setCurrent('searchOrder')
                },
                {
                    key: 'reserveSeat',
                    icon: <AppstoreAddOutlined />,
                    label: 'Резерв места',
                    onClick: () => setCurrent('reserveSeat')
                },
                {
                    key: 'registerFree',
                    icon: <CheckCircleOutlined />,
                    label: 'Бесплатная регистрация',
                    onClick: () => setCurrent('registerFree')
                },
                {
                    key: 'registerPaid',
                    icon: <CreditCardOutlined />,
                    label: 'Платная регистрация',
                    onClick: () => setCurrent('registerPaid')
                }
            ]
        },
        {
            key: 'passengerService',
            label: 'Пассажиры',
            children: [
                {
                    key: 'passengers',
                    icon: <TeamOutlined />,
                    label: 'Список пассажиров',
                    onClick: () => setCurrent('passengers')
                }
            ]
        },
        {
            key: 'sessionService',
            label: 'Сессия',
            children: [
                {
                    key: 'session',
                    icon: <LockOutlined />,
                    label: 'Работа с DynamicId',
                    onClick: () => setCurrent('session')
                }
            ]
        }
    ];

    return (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <div style={{
                backgroundColor: '#001529',
                padding: '10px 20px',
                borderBottom: '2px solid #1890ff',
                display: 'flex',
                alignItems: 'center'
            }}>
                <div style={{
                    color: '#fff',
                    fontSize: '20px',
                    fontWeight: 'bold',
                    marginRight: '30px',
                    display: 'flex',
                    alignItems: 'center'
                }}>
                    ✈ Air Check Admin
                </div>
                <Menu
                    mode="horizontal"
                    theme="dark"
                    selectedKeys={[current]}
                    items={items.map(item => {
                        if (!item.children) {
                            return {
                                key: item.key,
                                icon: item.icon,
                                label: item.label,
                                onClick: () => setCurrent(item.key)
                            };
                        } else {
                            return {
                                key: item.key,
                                label: item.label,
                                children: item.children.map(sub => ({
                                    key: sub.key,
                                    icon: sub.icon,
                                    label: sub.label,
                                    onClick: sub.onClick
                                }))
                            };
                        }
                    })}
                    style={{ flexGrow: 1, fontSize: '16px' }}
                />
            </div>
        </motion.div>
    );
};

export default Navbar;
