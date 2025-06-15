import React, { useState, useEffect, useMemo, useCallback } from "react";
import { Table, Card, message, Descriptions } from "antd";
import { getPassengers } from "../../api/api";
import { motion, AnimatePresence } from "framer-motion";

const formatDate = (dateStr) =>
  dateStr ? new Date(dateStr).toLocaleDateString("ru-RU") : "-";

const PassengerList = () => {
  const [passengers, setPassengers] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const { data } = await getPassengers();
        if (!Array.isArray(data)) throw new Error("Invalid format");
        if (data.length === 0) message.info("Пассажиры не найдены");
        const valid = data.filter((p) => p.passengerId);
        if (valid.length !== data.length)
          message.warning("Некоторые записи без passengerId были исключены");
        setPassengers(valid);
      } catch (err) {
        console.error(err);
        message.error("Ошибка загрузки пассажиров");
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const columns = useMemo(
    () => [
      { title: "PNR", dataIndex: "pnrId", key: "pnrId" },
      { title: "Фамилия", dataIndex: "lastName", key: "lastName" },
      { title: "Имя", dataIndex: "firstName", key: "firstName" },
      { title: "Категория", dataIndex: "category", key: "category" },
      { title: "Место", dataIndex: "seatNumber", key: "seatNumber" },
      {
        title: "Статус регистрации",
        dataIndex: "checkInStatus",
        key: "checkInStatus",
      },
    ],
    []
  );

  const expandedRowRender = useCallback(
    (record) => (
      <AnimatePresence>
        <motion.div
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -10 }}
          transition={{ duration: 0.3 }}
        >
          <Descriptions size="small" column={2} bordered>
            <Descriptions.Item label="Pax No">{record.paxNo}</Descriptions.Item>
            <Descriptions.Item label="Дата рождения">
              {formatDate(record.birthDate)}
            </Descriptions.Item>
            <Descriptions.Item label="Причина">
              {record.reason || "-"}
            </Descriptions.Item>
            <Descriptions.Item label="Занято мест">
              {record.seatsOccupied}
            </Descriptions.Item>
            <Descriptions.Item label="E-ticket">
              {record.eticket ? "Да" : "Нет"}
            </Descriptions.Item>
            <Descriptions.Item label="Документ №">
              {record.document?.number || "-"}
            </Descriptions.Item>
            <Descriptions.Item label="Страна выдачи">
              {record.document?.issueCountryCode || "-"}
            </Descriptions.Item>
            <Descriptions.Item label="Срок действия">
              {formatDate(record.document?.expiryDate)}
            </Descriptions.Item>
            <Descriptions.Item label="Виза №">
              {record.visaDocument?.number || "-"}
            </Descriptions.Item>
            <Descriptions.Item label="Виза выдана">
              {record.visaDocument?.issuePlace || "-"}
            </Descriptions.Item>
            <Descriptions.Item label="Статус места">
              {record.seatStatus}
            </Descriptions.Item>
            <Descriptions.Item label="Класс места">
              {record.seatLayerType}
            </Descriptions.Item>
            <Descriptions.Item label="Замечания" span={2}>
              {Array.isArray(record.remarks) && record.remarks.length > 0
                ? record.remarks.join(", ")
                : "-"}
            </Descriptions.Item>
            <Descriptions.Item label="APIS">{record.apis}</Descriptions.Item>
            <Descriptions.Item label="Booking ID">
              {record.bookingId}
            </Descriptions.Item>
          </Descriptions>
        </motion.div>
      </AnimatePresence>
    ),
    []
  );

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.5 }}
    >
      <Card
        title="Список пассажиров"
        style={{ width: "90%", margin: "20px auto" }}
      >
        <Table
          dataSource={passengers}
          columns={columns}
          rowKey="passengerId"
          loading={loading}
          pagination={{ pageSize: 10 }}
          expandable={{ expandedRowRender, expandRowByClick: true }}
          locale={{ emptyText: loading ? "Загрузка..." : "Нет пассажиров" }}
        />
      </Card>
    </motion.div>
  );
};

export default PassengerList;
