import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { Spin } from "antd";

const ProtectedRoute = ({ children }) => {
  const { isAuthenticated } = useAuth();

  if (isAuthenticated === null) return <Spin size="large" fullscreen />;

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return children;
};

export default ProtectedRoute;
