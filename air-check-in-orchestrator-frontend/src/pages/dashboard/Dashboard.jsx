import React, { useState, useEffect } from "react";
import { Card } from "antd";
import { motion } from "framer-motion";
import Navbar from "../../components/Navbar";
import PassengerForm from "../forms/PassengerForm";
import BaggageForm from "../forms/BaggageForm";
import OrderSearchForm from "../forms/OrderSearchForm";
import SeatReserveForm from "../forms/SeatReserveForm";
import FreeRegistrationForm from "../forms/FreeRegistrationForm";
import PaidRegistrationForm from "../forms/PaidRegistrationForm";
import PassengerList from "../passengers/PassengerList";
import SessionForm from "../forms/SessionForm";
import AdminBaggageRegistrations from "../admin/baggage/AdminBaggageRegistrations";
import AdminBaggagePayments from "../admin/baggage/AdminBaggagePayments";
import AdminBaggageOptions from "../admin/baggage/AdminBaggageOptions";
import AdminPassengerList from "../admin/passengers/AdminPassengerList";
import AdminPayments from "../admin/payments/AdminPayments";
import AdminRegistrations from "../admin/registrations/AdminRegistrations";
import AdminReservations from "../admin/reservations/AdminReservations";
import AdminSessions from "../admin/sessions/AdminSessions";
import Authentication from "../forms/Authentication";
import SimulateBaggagePayment from "../operations/SimulateBaggagePayment";
import Allowance from "../operations/Allowance";
import RegisterBaggage from "../forms/RegisterBaggage";
import CancelBaggage from "../operations/CancelBaggage";

const Dashboard = ({ current, setCurrent }) => {
  const [stats, setStats] = useState({ checkIns: 0, baggage: 0 });

  useEffect(() => {
    if (current === "home") {
      fetchMetrics().then(setStats);
    }
  }, [current]);

  async function fetchMetrics() {
    const response = await fetch("http://localhost:8080/metrics");
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
      {current === "home" && (
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          <Card
            title="Air Check-In Orchestrator"
            style={{ width: 700, margin: "20px auto" }}
          >
            <p>
              Добро пожаловать в административную панель системы регистрации
              пассажиров и багажа.
            </p>
            <p>
              Здесь вы можете выполнять регистрацию пассажиров на рейсы и
              оформление багажа.
            </p>
            <p style={{ fontStyle: "italic", color: "#555" }}>
              "Your gateway to seamless check-in automation."
            </p>
          </Card>

          <div
            style={{
              display: "flex",
              justifyContent: "center",
              gap: "20px",
              marginTop: "20px",
            }}
          >
            <Card
              title="Регистраций сегодня"
              style={{ width: 220 }}
              bordered={false}
            >
              <p style={{ fontSize: "24px", textAlign: "center" }}>
                {stats.checkIns}
              </p>
            </Card>
            <Card
              title="Багажа оформлено"
              style={{ width: 220 }}
              bordered={false}
            >
              <p style={{ fontSize: "24px", textAlign: "center" }}>
                {stats.baggage}
              </p>
            </Card>
          </div>
        </motion.div>
      )}
      {current === "passenger" && <PassengerForm setCurrent={setCurrent} />}
      {current === "baggage" && <BaggageForm setCurrent={setCurrent} />}
      {current === "searchOrder" && <OrderSearchForm setCurrent={setCurrent} />}
      {current === "reserveSeat" && <SeatReserveForm setCurrent={setCurrent} />}
      {current === "registerFree" && (
        <FreeRegistrationForm setCurrent={setCurrent} />
      )}
      {current === "registerPaid" && (
        <PaidRegistrationForm setCurrent={setCurrent} />
      )}
      {current === "passengers" && <PassengerList setCurrent={setCurrent} />}
      {current === "session" && <SessionForm setCurrent={setCurrent} />}
      {current === "adminBaggageRegistrations" && <AdminBaggageRegistrations />}
      {current === "adminBaggagePayments" && <AdminBaggagePayments />}
      {current === "adminBaggageOptions" && <AdminBaggageOptions />}
      {current === "adminPassengers" && (
        <AdminPassengerList setCurrent={setCurrent} />
      )}
      {current === "adminPayments" && <AdminPayments />}
      {current === "adminRegistrations" && <AdminRegistrations />}
      {current === "adminReservations" && <AdminReservations />}
      {current === "adminSessions" && <AdminSessions />}
      {current === "authenticate" && <Authentication setCurrent={setCurrent} />}
      {current === "simulateBaggagePayment" && <SimulateBaggagePayment />}
      {current === "getAllowance" && <Allowance />}
      {current === "registerBaggage" && <RegisterBaggage />}
      {current === "cancelBaggage" && <CancelBaggage />}
    </>
  );
};

export default Dashboard;
