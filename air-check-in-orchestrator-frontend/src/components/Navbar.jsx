import { Menu, ConfigProvider, Tooltip } from "antd";
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
  InfoCircleOutlined,
} from "@ant-design/icons";

const Navbar = ({ current, setCurrent }) => {
  const tooltips = {
    home: "Перейти на главную страницу",
    passenger: "Регистрация пассажира по DynamicId и PNR",
    baggage: "Регистрация багажа к выбранному пассажиру",
    searchOrder: "Поиск заказа по PNR",
    reserveSeat: "Резерв места на рейс",
    registerFree: "Бесплатная регистрация пассажира",
    registerPaid: "Платная регистрация пассажира",
    adminPayments: "Просмотр и управление оплатами",
    adminRegistrations: "Управление регистрациями пассажиров",
    adminReservations: "Управление резервами мест",
    authenticate: "Аутентификация в системе регистрации",
    simulateBaggagePayment: "Смоделировать оплату багажа",
    getAllowance: "Получить норму провоза багажа",
    registerBaggage: "Регистрация багажа",
    cancelBaggage: "Отмена регистрации багажа",
    passengers: "Список пассажиров",
    adminPassengers: "Администрирование пассажиров",
    session: "Работа с DynamicId сессий",
    adminSessions: "Администрирование сессий",
    adminBaggageRegistrations: "Просмотр регистраций багажа",
    adminBaggagePayments: "Просмотр и управление оплатами багажа",
    adminBaggageOptions: "Управление опциями багажа",
  };

  const wrapWithTooltip = (key, content) => (
    <Tooltip title={tooltips[key] || ""}>
      <span>
        {content} <InfoCircleOutlined style={{ marginLeft: 4 }} />
      </span>
    </Tooltip>
  );

  const items = [
    {
      key: "home",
      icon: <HomeOutlined />,
      label: "Главная",
      onClick: () => setCurrent("home"),
    },
    {
      key: "orchestrator",
      icon: <DatabaseOutlined />,
      label: "Оркестратор",
      children: [
        {
          key: "passenger",
          icon: <UserOutlined />,
          label: wrapWithTooltip("passenger", "Регистрация пассажира"),
          onClick: () => setCurrent("passenger"),
        },
        {
          key: "baggage",
          icon: <ShoppingOutlined />,
          label: wrapWithTooltip("baggage", "Регистрация багажа"),
          onClick: () => setCurrent("baggage"),
        },
      ],
    },
    {
      key: "registration",
      icon: <CheckCircleOutlined />,
      label: "Регистрация",
      children: [
        {
          key: "searchOrder",
          icon: <SearchOutlined />,
          label: wrapWithTooltip("searchOrder", "Поиск заказа"),
          onClick: () => setCurrent("searchOrder"),
        },
        {
          key: "reserveSeat",
          icon: <AppstoreAddOutlined />,
          label: wrapWithTooltip("reserveSeat", "Резерв места"),
          onClick: () => setCurrent("reserveSeat"),
        },
        {
          key: "registerFree",
          icon: <CheckCircleOutlined />,
          label: wrapWithTooltip("registerFree", "Бесплатная регистрация"),
          onClick: () => setCurrent("registerFree"),
        },
        {
          key: "registerPaid",
          icon: <CreditCardOutlined />,
          label: wrapWithTooltip("registerPaid", "Платная регистрация"),
          onClick: () => setCurrent("registerPaid"),
        },
        {
          key: "adminPayments",
          icon: <DollarOutlined />,
          label: wrapWithTooltip("adminPayments", "Админ оплаты"),
          onClick: () => setCurrent("adminPayments"),
        },
        {
          key: "adminRegistrations",
          icon: <DatabaseOutlined />,
          label: wrapWithTooltip("adminRegistrations", "Админ регистрации"),
          onClick: () => setCurrent("adminRegistrations"),
        },
        {
          key: "adminReservations",
          icon: <AppstoreAddOutlined />,
          label: wrapWithTooltip("adminReservations", "Админ резервы"),
          onClick: () => setCurrent("adminReservations"),
        },
        {
          key: "authenticate",
          icon: <LockOutlined />,
          label: wrapWithTooltip("authenticate", "Аутентификация"),
          onClick: () => setCurrent("authenticate"),
        },
        {
          key: "simulateBaggagePayment",
          icon: <ShoppingOutlined />,
          label: wrapWithTooltip(
            "simulateBaggagePayment",
            "Сим. оплата багажа"
          ),
          onClick: () => setCurrent("simulateBaggagePayment"),
        },
        {
          key: "getAllowance",
          icon: <DatabaseOutlined />,
          label: wrapWithTooltip("getAllowance", "Получить норму"),
          onClick: () => setCurrent("getAllowance"),
        },
        {
          key: "registerBaggage",
          icon: <ShoppingOutlined />,
          label: wrapWithTooltip("registerBaggage", "Рег. багажа"),
          onClick: () => setCurrent("registerBaggage"),
        },
        {
          key: "cancelBaggage",
          icon: <ToolOutlined />,
          label: wrapWithTooltip("cancelBaggage", "Отмена багажа"),
          onClick: () => setCurrent("cancelBaggage"),
        },
      ],
    },
    {
      key: "passengerService",
      icon: <TeamOutlined />,
      label: "Пассажиры",
      children: [
        {
          key: "passengers",
          icon: <TeamOutlined />,
          label: wrapWithTooltip("passengers", "Список пассажиров"),
          onClick: () => setCurrent("passengers"),
        },
        {
          key: "adminPassengers",
          icon: <TeamOutlined />,
          label: wrapWithTooltip("adminPassengers", "Админ пассажиры"),
          onClick: () => setCurrent("adminPassengers"),
        },
      ],
    },
    {
      key: "sessionService",
      icon: <LockOutlined />,
      label: "Сессии",
      children: [
        {
          key: "session",
          icon: <LockOutlined />,
          label: wrapWithTooltip("session", "Работа с DynamicId"),
          onClick: () => setCurrent("session"),
        },
        {
          key: "adminSessions",
          icon: <LockOutlined />,
          label: wrapWithTooltip("adminSessions", "Админ сессии"),
          onClick: () => setCurrent("adminSessions"),
        },
      ],
    },
    {
      key: "baggageService",
      icon: <ShoppingOutlined />,
      label: "Багаж",
      children: [
        {
          key: "adminBaggageRegistrations",
          icon: <DatabaseOutlined />,
          label: wrapWithTooltip(
            "adminBaggageRegistrations",
            "Регистрации багажа"
          ),
          onClick: () => setCurrent("adminBaggageRegistrations"),
        },
        {
          key: "adminBaggagePayments",
          icon: <DollarOutlined />,
          label: wrapWithTooltip("adminBaggagePayments", "Оплаты багажа"),
          onClick: () => setCurrent("adminBaggagePayments"),
        },
        {
          key: "adminBaggageOptions",
          icon: <ToolOutlined />,
          label: wrapWithTooltip("adminBaggageOptions", "Опции багажа"),
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
          {}
          <div
            style={{
              color: "#fff",
              fontSize: "20px",
              fontWeight: "bold",
              marginRight: "40px",
              cursor: "pointer",
            }}
            onClick={() => setCurrent("home")}
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
              overflow: "visible",
            }}
          />
        </div>
      </motion.div>
    </ConfigProvider>
  );
};

export default Navbar;
