import { createContext, useContext, useMemo, useState } from 'react';

const AuthContext = createContext(null);
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

function decodeRole(token) {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join('')
    );
    return JSON.parse(json)[ROLE_CLAIM];
  } catch {
    return null;
  }
}

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

  const isAdmin = useMemo(() => (token ? decodeRole(token) === 'Admin' : false), [token]);

  const value = useMemo(() => ({ token, member, isAdmin, login, logout }), [token, member, isAdmin]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
