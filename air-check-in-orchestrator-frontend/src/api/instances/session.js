import axios from 'axios';

const axiosSession = axios.create({
  baseURL: process.env.REACT_APP_SESSION_URL || "http://localhost:8084/api",
});

axiosSession.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosSession.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const url = error.config?.url || "";

    if (
      status === 401 &&
      (url.endsWith("/session/validate") || url.endsWith("/session/register"))
    ) {
      return Promise.reject(error);
    }

    localStorage.removeItem("token");
    window.location.href = "/#/login";
    return Promise.reject(error);
  }
);

export default axiosSession;
