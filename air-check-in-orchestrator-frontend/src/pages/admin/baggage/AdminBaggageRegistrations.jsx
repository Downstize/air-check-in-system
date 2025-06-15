import React, { useEffect, useState, useCallback } from "react";
import { Table, Button, message, Popconfirm } from "antd";
import { getAllRegistrations, deleteRegistration } from "../../../api/api";

const AdminBaggageRegistrations = () => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);
  const token = localStorage.getItem("token");

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getAllRegistrations(token);
      setData(res.data);
    } catch {
      message.error("Ошибка загрузки регистраций багажа");
    } finally {
      setLoading(false);
    }
  }, [token]);

  const handleDelete = async (id) => {
    try {
      await deleteRegistration(id, token);
      message.success("Регистрация удалена");
      fetchData();
    } catch {
      message.error("Ошибка удаления");
    }
  };

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const columns = [
    { title: "ID", dataIndex: "registrationId", key: "registrationId" },
    { title: "Пассажир", dataIndex: "passengerId", key: "passengerId" },
    { title: "Мест", dataIndex: "pieces", key: "pieces" },
    { title: "Вес (кг)", dataIndex: "weightKg", key: "weightKg" },
    { title: "Цена", dataIndex: "price", key: "price" },
    {
      title: "Действия",
      render: (_, record) => (
        <Popconfirm
          title="Удалить регистрацию?"
          onConfirm={() => handleDelete(record.registrationId)}
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
      <h2>Регистрации багажа</h2>
      <Table
        columns={columns}
        dataSource={data}
        rowKey="registrationId"
        loading={loading}
      />
    </div>
  );
};

export default AdminBaggageRegistrations;
