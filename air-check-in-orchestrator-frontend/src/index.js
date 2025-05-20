import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import { AuthProvider } from './context/AuthContext';
import { ConfigProvider, App as AntdApp } from 'antd';
import 'antd/dist/reset.css';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <ConfigProvider>
      <AntdApp> {}
        <AuthProvider>
          <App />
        </AuthProvider>
      </AntdApp>
    </ConfigProvider>
  </React.StrictMode>
);

reportWebVitals();
