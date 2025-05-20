import { HashRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './pages/auth/Login';
import Dashboard from './pages/dashboard/Dashboard';
import ProtectedRoute from './components/ProtectedRoute';
import 'antd/dist/reset.css';
import { useState } from 'react';

function App() {
  const [current, setCurrent] = useState('home');

  return (
      <Router>
          <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="/" element={
                  <ProtectedRoute>
                      <Dashboard current={current} setCurrent={setCurrent} />
                  </ProtectedRoute>
              } />
          </Routes>
      </Router>
  );
}

export default App;