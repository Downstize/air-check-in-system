import React from "react";
import { Menu, ConfigProvider } from "antd";
import { motion } from "framer-motion";
import {
  HomeOutlined,
  UserOutlined,
  ShoppingOutlined,
  SearchOutlined,
  AppstoreAddOutlined,
  CheckCircleOutlined,
  CreditCardOutlined,
  TeamOutlined,
  LockOutlined,
  ToolOutlined,
  DollarOutlined,
  DatabaseOutlined,
} from "@ant-design/icons";

const Navbar = ({ current, setCurrent }) => {
  const items = [
    {
      key: "home",
      icon: <HomeOutlined />,
      label: "Главная",
      onClick: () => setCurrent("home"),
    },
    {
      key: "orchestrator",
      label: "Оркестратор",
      icon: <DatabaseOutlined />,
      children: [
        {
          key: "passenger",
          icon: <UserOutlined />,
          label: "Регистрация пассажира",
          onClick: () => setCurrent("passenger"),
        },
        {
          key: "baggage",
          icon: <ShoppingOutlined />,
          label: "Регистрация багажа",
          onClick: () => setCurrent("baggage"),
        },
      ],
    },
    {
      key: "registration",
      label: "Регистрация",
      icon: <CheckCircleOutlined />,
      children: [
        {
          key: "searchOrder",
          icon: <SearchOutlined />,
          label: "Поиск заказа",
          onClick: () => setCurrent("searchOrder"),
        },
        {
          key: "reserveSeat",
          icon: <AppstoreAddOutlined />,
          label: "Резерв места",
          onClick: () => setCurrent("reserveSeat"),
        },
        {
          key: "registerFree",
          icon: <CheckCircleOutlined />,
          label: "Бесплатная регистрация",
          onClick: () => setCurrent("registerFree"),
        },
        {
          key: "registerPaid",
          icon: <CreditCardOutlined />,
          label: "Платная регистрация",
          onClick: () => setCurrent("registerPaid"),
        },
        {
          key: "adminPayments",
          icon: <DollarOutlined />,
          label: "Админ оплаты",
          onClick: () => setCurrent("adminPayments"),
        },
        {
          key: "adminRegistrations",
          icon: <DatabaseOutlined />,
          label: "Админ регистрации",
          onClick: () => setCurrent("adminRegistrations"),
        },
        {
          key: "adminReservations",
          icon: <AppstoreAddOutlined />,
          label: "Админ резервы",
          onClick: () => setCurrent("adminReservations"),
        },
        {
          key: "authenticate",
          icon: <LockOutlined />,
          label: "Аутентификация",
          onClick: () => setCurrent("authenticate"),
        },
        {
          key: "simulatePayment",
          icon: <CreditCardOutlined />,
          label: "Сим. оплата",
          onClick: () => setCurrent("simulatePayment"),
        },
        {
          key: "simulateBaggagePayment",
          icon: <ShoppingOutlined />,
          label: "Сим. оплата багажа",
          onClick: () => setCurrent("simulateBaggagePayment"),
        },
        {
          key: "getAllowance",
          icon: <DatabaseOutlined />,
          label: "Получить норму",
          onClick: () => setCurrent("getAllowance"),
        },
        {
          key: "registerBaggage",
          icon: <ShoppingOutlined />,
          label: "Рег. багажа",
          onClick: () => setCurrent("registerBaggage"),
        },
        {
          key: "cancelBaggage",
          icon: <ToolOutlined />,
          label: "Отмена багажа",
          onClick: () => setCurrent("cancelBaggage"),
        },
      ],
    },
    {
      key: "passengerService",
      label: "Пассажиры",
      icon: <TeamOutlined />,
      children: [
        {
          key: "passengers",
          icon: <TeamOutlined />,
          label: "Список пассажиров",
          onClick: () => setCurrent("passengers"),
        },
        {
          key: "adminPassengers",
          icon: <TeamOutlined />,
          label: "Админ пассажиры",
          onClick: () => setCurrent("adminPassengers"),
        },
      ],
    },
    {
      key: "sessionService",
      label: "Сессии",
      icon: <LockOutlined />,
      children: [
        {
          key: "session",
          icon: <LockOutlined />,
          label: "Работа с DynamicId",
          onClick: () => setCurrent("session"),
        },
        {
          key: "adminSessions",
          icon: <LockOutlined />,
          label: "Админ сессии",
          onClick: () => setCurrent("adminSessions"),
        },
      ],
    },
    {
      key: "baggageService",
      label: "Багаж",
      icon: <ShoppingOutlined />,
      children: [
        {
          key: "adminBaggageRegistrations",
          icon: <DatabaseOutlined />,
          label: "Регистрации багажа",
          onClick: () => setCurrent("adminBaggageRegistrations"),
        },
        {
          key: "adminBaggagePayments",
          icon: <DollarOutlined />,
          label: "Оплаты багажа",
          onClick: () => setCurrent("adminBaggagePayments"),
        },
        {
          key: "adminBaggageOptions",
          icon: <ToolOutlined />,
          label: "Опции багажа",
          onClick: () => setCurrent("adminBaggageOptions"),
        },
      ],
    },
  ];

  return (
    <ConfigProvider
      theme={{
        token: {
          colorPrimary: "#1677ff",
          colorBgContainer: "#001529",
          colorText: "#fff",
          controlHeightLG: 40,
        },
        components: {
          Menu: {
            itemColor: "#ccc",
            itemSelectedColor: "#fff",
            itemBg: "transparent",
            itemHoverColor: "#fff",
            itemHoverBg: "#1677ff50",
            itemSelectedBg: "#1677ff",
            borderRadius: 8,
            subMenuItemBorderRadius: 8,
            itemMarginInline: 10,
            itemPaddingInline: 20,
          },
        },
      }}
    >
      <motion.div
        initial={{ opacity: 0, y: -10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3, ease: "easeOut" }}
      >
        <div
          style={{
            backgroundColor: "#001529",
            padding: "12px 20px",
            borderBottom: "2px solid #1890ff",
            display: "flex",
            alignItems: "center",
          }}
        >
          <div
            style={{
              color: "#fff",
              fontSize: "20px",
              fontWeight: "bold",
              marginRight: "40px",
            }}
          >
            ✈ Air Check Admin
          </div>
          <Menu
            mode="horizontal"
            theme="dark"
            selectedKeys={[current]}
            items={items}
            style={{
              flexGrow: 1,
              fontSize: "15px",
              fontWeight: 500,
              maxHeight: "100%",
              overflow: "visible",
            }}
            popupClassName="custom-submenu"
          />
        </div>
      </motion.div>
    </ConfigProvider>
  );
};

export default Navbar;
