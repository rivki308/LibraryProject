import { createContext, useContext, useEffect, useMemo, useState } from 'react';

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
  const [sessionMessage, setSessionMessage] = useState('');

  const login = (authResponse) => {
    localStorage.setItem('token', authResponse.token);
    localStorage.setItem('member', JSON.stringify(authResponse.member));
    setToken(authResponse.token);
    setMember(authResponse.member);
    setSessionMessage('');
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('member');
    setToken(null);
    setMember(null);
  };

  useEffect(() => {
    const handleExpired = (e) => {
      logout();
      setSessionMessage(e.detail?.message || 'פג תוקף ההתחברות. יש להתחבר מחדש.');
    };
    window.addEventListener('auth:expired', handleExpired);
    return () => window.removeEventListener('auth:expired', handleExpired);
  }, []);

  const isAdmin = useMemo(() => (token ? decodeRole(token) === 'Admin' : false), [token]);

  const value = useMemo(
    () => ({ token, member, isAdmin, login, logout, sessionMessage }),
    [token, member, isAdmin, sessionMessage]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
