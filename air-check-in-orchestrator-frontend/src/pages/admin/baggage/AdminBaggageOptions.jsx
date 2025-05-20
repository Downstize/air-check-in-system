import React, { useEffect, useState, useCallback } from "react";
import { Table, Button, message, Popconfirm } from "antd";
import { getAllPaidOptions, deletePaidOption } from "../../../api/api";
import { useAuth } from "../../../context/AuthContext";

const AdminBaggageOptions = () => {
  const { token, logout } = useAuth();
  const [options, setOptions] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchOptions = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    try {
      const res = await getAllPaidOptions();
      setOptions(res.data);
    } catch (err) {
      console.error("Ошибка при получении опций", err);
      if (err?.response?.status === 401) {
        logout();
        message.warning("Сессия истекла, войдите снова");
      } else if (err?.response?.status === 403) {
        message.error("Нет доступа к опциям багажа");
      } else {
        message.error("Ошибка загрузки опций");
      }
    } finally {
      setLoading(false);
    }
  }, [token, logout]);

  const handleDelete = async (id) => {
    try {
      await deletePaidOption(id);
      message.success("Опция удалена");
      fetchOptions();
    } catch (err) {
      console.error("Ошибка удаления", err);
      message.error("Не удалось удалить опцию");
    }
  };

  useEffect(() => {
    fetchOptions();
  }, [fetchOptions, token]);

  const columns = [
    { title: "ID", dataIndex: "paidOptionId", key: "paidOptionId" },
    { title: "Мест", dataIndex: "pieces", key: "pieces" },
    { title: "Вес (кг)", dataIndex: "weightKg", key: "weightKg" },
    {
      title: "Цена",
      dataIndex: "price",
      key: "price",
      render: (value) => `${value.toFixed(2)} ₽`,
    },
    {
      title: "Действия",
      key: "action",
      render: (_, record) => (
        <Popconfirm
          title="Удалить опцию?"
          onConfirm={() => handleDelete(record.paidOptionId)}
        >
          <Button danger size="small">Удалить</Button>
        </Popconfirm>
      ),
    },
  ];

  return (
    <div style={{ padding: 20 }}>
      <h2>Опции багажа</h2>
      <Table
        columns={columns}
        dataSource={options}
        rowKey="paidOptionId"
        loading={loading}
      />
    </div>
  );
};

export default AdminBaggageOptions;
