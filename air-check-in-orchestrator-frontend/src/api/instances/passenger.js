import axios from 'axios';

const axiosPassenger = axios.create({
  baseURL: process.env.REACT_APP_PASSENGER_URL || 'http://localhost:8082/api',
});

axiosPassenger.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosPassenger.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/#/login';
    }
    return Promise.reject(err);
  }
);

export default axiosPassenger;
