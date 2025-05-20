import axios from 'axios';

const axiosOrchestrator = axios.create({
  baseURL: process.env.REACT_APP_ORCHESTRATOR_URL || 'http://localhost:8080/api',
});

axiosOrchestrator.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosOrchestrator.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/#/login';
    }
    return Promise.reject(err);
  }
);

export default axiosOrchestrator;
