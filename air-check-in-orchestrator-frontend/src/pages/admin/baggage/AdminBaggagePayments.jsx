import React, { useEffect, useState, useCallback } from "react";
import { Table, Button, message, Popconfirm } from "antd";
import { getAllBaggagePayments, deleteBaggagePayment } from "../../../api/api";

const AdminBaggagePayments = () => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);
  const token = localStorage.getItem("token");

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getAllBaggagePayments(token);
      setData(res.data);
    } catch {
      message.error("Ошибка загрузки оплат");
    } finally {
      setLoading(false);
    }
  }, [token]);

  const handleDelete = async (id) => {
    try {
      await deleteBaggagePayment(id, token);
      message.success("Оплата удалена");
      fetchData();
    } catch {
      message.error("Ошибка удаления");
    }
  };

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const columns = [
    { title: "ID", dataIndex: "paymentId", key: "paymentId" },
    { title: "Пассажир", dataIndex: "passengerId", key: "passengerId" },
    { title: "Сумма", dataIndex: "amount", key: "amount" },
    {
      title: "Оплачен",
      dataIndex: "isPaid",
      key: "isPaid",
      render: (val) => (val ? "Да" : "Нет"),
    },
    {
      title: "Действия",
      render: (_, record) => (
        <Popconfirm
          title="Удалить оплату?"
          onConfirm={() => handleDelete(record.paymentId)}
        >
          <Button danger size="small">
            Удалить
          </Button>
        </Popconfirm>
      ),
    },
  ];

  return (
    <div style={{ padding: 20 }}>
      <h2>Оплаты багажа</h2>
      <Table
        columns={columns}
        dataSource={data}
        rowKey="paymentId"
        loading={loading}
      />
    </div>
  );
};

export default AdminBaggagePayments;
