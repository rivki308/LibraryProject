import { createContext, useContext, useMemo, useState } from 'react';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [member, setMember] = useState(() => {
    const stored = localStorage.getItem('member');
    return stored ? JSON.parse(stored) : null;
  });

  const login = (authResponse) => {
    localStorage.setItem('token', authResponse.token);
    localStorage.setItem('member', JSON.stringify(authResponse.member));
    setToken(authResponse.token);
    setMember(authResponse.member);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('member');
    setToken(null);
    setMember(null);
  };

  const value = useMemo(() => ({ token, member, login, logout }), [token, member]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
