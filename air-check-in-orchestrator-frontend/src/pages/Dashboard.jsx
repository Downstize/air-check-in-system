import React, { useState, useEffect } from 'react';
import { Card } from 'antd';
import { motion } from 'framer-motion';
import Navbar from '../components/Navbar';
import PassengerForm from './PassengerForm';
import BaggageForm from './BaggageForm';
import OrderSearchForm from './OrderSearchForm';
import SeatReserveForm from './SeatReserveForm';
import FreeRegistrationForm from './FreeRegistrationForm';
import PaidRegistrationForm from './PaidRegistrationForm';
import PassengerList from './PassengerList';
import SessionForm from './SessionForm';


const Dashboard = ({ current, setCurrent }) => {
    const [stats, setStats] = useState({ checkIns: 0, baggage: 0 });

    useEffect(() => {
        if (current === 'home') {
            fetchMetrics().then(setStats);
        }
    }, [current]);

    async function fetchMetrics() {
        const response = await fetch('http://localhost:8080/metrics');
        const text = await response.text();

        const checkInsMatch = text.match(/passenger_checkins_total\s+(\d+)/);
        const baggageMatch = text.match(/baggage_registrations_total\s+(\d+)/);

        const checkIns = checkInsMatch ? parseInt(checkInsMatch[1], 10) : 0;
        const baggage = baggageMatch ? parseInt(baggageMatch[1], 10) : 0;

        return { checkIns, baggage };
    }

    return (
        <>
            <Navbar current={current} setCurrent={setCurrent} />
            {current === 'home' && (
                <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                    <Card title="Air Check-In Orchestrator" style={{ width: 700, margin: '20px auto' }}>
                        <p>
                            Добро пожаловать в административную панель системы регистрации пассажиров и багажа.
                        </p>
                        <p>
                            Здесь вы можете выполнять регистрацию пассажиров на рейсы и оформление багажа. 
                            Система интегрирована с микросервисами и очередями RabbitMQ.
                        </p>
                        <p style={{ fontStyle: 'italic', color: '#555' }}>
                            "Your gateway to seamless check-in automation."
                        </p>
                    </Card>

                    <div style={{ display: 'flex', justifyContent: 'center', gap: '20px', marginTop: '20px' }}>
                        <Card title="Регистраций сегодня" style={{ width: 220 }} bordered={false}>
                            <p style={{ fontSize: '24px', textAlign: 'center' }}>{stats.checkIns}</p>
                        </Card>
                        <Card title="Багажа оформлено" style={{ width: 220 }} bordered={false}>
                            <p style={{ fontSize: '24px', textAlign: 'center' }}>{stats.baggage}</p>
                        </Card>
                    </div>
                </motion.div>
            )}
            {current === 'passenger' && <PassengerForm setCurrent={setCurrent} />}
            {current === 'baggage' && <BaggageForm setCurrent={setCurrent} />}
            {current === 'searchOrder' && <OrderSearchForm setCurrent={setCurrent} />}
            {current === 'reserveSeat' && <SeatReserveForm setCurrent={setCurrent} />}
            {current === 'registerFree' && <FreeRegistrationForm setCurrent={setCurrent} />}
            {current === 'registerPaid' && <PaidRegistrationForm setCurrent={setCurrent} />}
            {current === 'passengers' && <PassengerList setCurrent={setCurrent} />}
            {current === 'session' && <SessionForm setCurrent={setCurrent} />}
        </>
    );
};

export default Dashboard;
