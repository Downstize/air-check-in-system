import React, { createContext, useContext, useEffect, useState } from "react";
import { jwtDecode } from "jwt-decode";

const AuthContext = createContext();

const isJwt = (token) => token.split(".").length === 3;

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [isAuthenticated, setIsAuthenticated] = useState(!!token);

  useEffect(() => {
    if (!token) {
      setIsAuthenticated(false);
      return;
    }

    if (isJwt(token)) {
      try {
        const { exp } = jwtDecode(token);
        if (Date.now() < exp * 1000) {
          setIsAuthenticated(true);
          return;
        }
      } catch (err) {
        console.warn("Не удалось декодировать JWT:", err);
      }
      localStorage.removeItem("token");
      setToken(null);
      setIsAuthenticated(false);
      return;
    }

    setIsAuthenticated(true);
  }, [token]);

  const login = (newToken) => {
    localStorage.setItem("token", newToken);
    setToken(newToken);
    setIsAuthenticated(true);
  };

  const logout = () => {
    localStorage.removeItem("token");
    setToken(null);
    setIsAuthenticated(false);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
