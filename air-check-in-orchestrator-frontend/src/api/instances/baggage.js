import axios from 'axios';

const axiosBaggage = axios.create({
  baseURL: process.env.REACT_APP_BAGGAGE_URL || 'http://localhost:8081/api',
});

axiosBaggage.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosBaggage.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/#/login';
    }
    return Promise.reject(err);
  }
);

export default axiosBaggage;
