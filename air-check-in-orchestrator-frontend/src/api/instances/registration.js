import axios from 'axios';

const axiosRegistration = axios.create({
  baseURL: process.env.REACT_APP_REGISTRATION_URL || 'http://localhost:8083/api',
});

axiosRegistration.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosRegistration.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/#/login';
    }
    return Promise.reject(err);
  }
);

export default axiosRegistration;
