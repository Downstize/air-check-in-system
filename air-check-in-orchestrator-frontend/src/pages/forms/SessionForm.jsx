import { useState } from "react";
import { Form, Input, Button, Card, Alert, message } from "antd";
import { registerSession, validateSession } from "../../api/api";
import { motion } from "framer-motion";

const SessionForm = () => {
  const [form] = Form.useForm();
  const [result, setResult] = useState(null);

  const handleRegister = async () => {
    const dynamicId = form.getFieldValue("dynamicId");
    if (!dynamicId) {
      message.warning("Введите DynamicId");
      return;
    }
    try {
      await registerSession(dynamicId);
      setResult({ type: "success", text: "Сессия успешно зарегистрирована" });
      message.success("Сессия успешно зарегистрирована");
    } catch (error) {
      console.error(error);
      setResult({ type: "error", text: "Ошибка регистрации сессии" });
      message.error("Ошибка регистрации сессии");
    }
  };

  const handleValidate = async () => {
    const dynamicId = form.getFieldValue("dynamicId");
    if (!dynamicId) {
      message.warning("Введите DynamicId");
      return;
    }
    try {
      await validateSession(dynamicId);
      setResult({ type: "success", text: "Сессия действительна" });
      message.success("Сессия действительна");
    } catch (error) {
      console.error(error);
      setResult({ type: "error", text: "Сессия недействительна" });
      message.error("Сессия недействительна");
    }
  };

  return (
    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
      <Card
        title="Работа с DynamicId (сессия)"
        style={{ width: 600, margin: "20px auto" }}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="dynamicId"
            label="DynamicId"
            rules={[{ required: true, message: "Введите DynamicId" }]}
          >
            <Input placeholder="Введите DynamicId" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" onClick={handleRegister}>
              Зарегистрировать
            </Button>
            <Button style={{ marginLeft: "10px" }} onClick={handleValidate}>
              Проверить
            </Button>
          </Form.Item>
        </Form>

        {result && (
          <Alert
            style={{ marginTop: 16 }}
            type={result.type}
            message={result.text}
            showIcon
          />
        )}
      </Card>
    </motion.div>
  );
};

export default SessionForm;
