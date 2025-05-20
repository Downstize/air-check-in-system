import React, { useState } from "react";
import { Form, Input, Button, message, Card, Typography, Space } from "antd";
import { CopyOutlined } from "@ant-design/icons";
import { authenticate } from "../../api/api";
import axios from "axios";
import { useAuth } from "../../context/AuthContext";

const { Text } = Typography;

const Authentication = ({ setCurrent }) => {
  const [loading, setLoading] = useState(false);
  const [dynamicId, setDynamicId] = useState(null);
  const { token: adminToken } = useAuth();

  const onFinish = async (values) => {
    setLoading(true);
    try {
      const res = await authenticate(values.login, values.pwd);
      setDynamicId(res.data.dynamicId);

      if (adminToken) {
        axios.defaults.headers.common["Authorization"] = `Bearer ${adminToken}`;
      }

      message.success("Аутентификация прошла успешно (Dynamic ID получен)");
    } catch (error) {
      console.error(error);
      message.error("Ошибка аутентификации");
    } finally {
      setLoading(false);
    }
  };

  const handleCopy = () => {
    if (!dynamicId) return;
    navigator.clipboard.writeText(dynamicId);
    message.info("Скопировано в буфер обмена", 2);
  };

  return (
    <Card title="Аутентификация" style={{ maxWidth: 450, margin: "40px auto" }}>
      <Form onFinish={onFinish} layout="vertical">
        <Form.Item name="login" label="Логин" rules={[{ required: true, message: "Введите логин" }]}
        >
          <Input />
        </Form.Item>
        <Form.Item name="pwd" label="Пароль" rules={[{ required: true, message: "Введите пароль" }]}
        >
          <Input.Password />
        </Form.Item>
        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading} block>
            Войти
          </Button>
        </Form.Item>
      </Form>

      {dynamicId && (
        <Space direction="vertical" style={{ marginTop: 24 }}>
          <Text strong>Ваш Dynamic ID:</Text>
          <Card size="small" style={{ background: "#f6ffed", borderColor: "#b7eb8f" }}>
            <Space>
              <Text code>{dynamicId}</Text>
              <Button type="link" icon={<CopyOutlined />} onClick={handleCopy}>
                Копировать
              </Button>
            </Space>
          </Card>
        </Space>
      )}
    </Card>
  );
};

export default Authentication;
