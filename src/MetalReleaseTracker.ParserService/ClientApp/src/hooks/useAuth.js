import { createContext, useContext, useState, useMemo, useCallback } from 'react';
import { login as loginApi } from '../api/auth';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('admin_token'));

  const loginFn = useCallback(async (username, password) => {
    const { data } = await loginApi(username, password);
    localStorage.setItem('admin_token', data.token);
    setToken(data.token);
  }, []);

  const logoutFn = useCallback(() => {
    localStorage.removeItem('admin_token');
    setToken(null);
  }, []);

  const value = useMemo(
    () => ({ token, isAuthenticated: !!token, login: loginFn, logout: logoutFn }),
    [token, loginFn, logoutFn]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
