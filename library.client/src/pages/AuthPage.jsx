import { useState } from 'react';
import { api } from '../api';
import { useAuth } from '../AuthContext';

export default function AuthPage() {
  const [mode, setMode] = useState('login');
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [info, setInfo] = useState('');
  const { login, sessionMessage } = useAuth();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const result = await api.login({ email, password });
      login(result);
    } catch (err) {
      setError(err.message);
    }
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');
    setInfo('');
    try {
      await api.register({ name, email, password });
      setInfo('ההרשמה הצליחה! כעת ניתן להתחבר.');
      setMode('login');
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="auth-page">
      <div className="tabs">
        <button className={mode === 'login' ? 'active' : ''} onClick={() => setMode('login')}>
          התחברות
        </button>
        <button className={mode === 'register' ? 'active' : ''} onClick={() => setMode('register')}>
          הרשמה
        </button>
      </div>

      {sessionMessage && <p className="error">{sessionMessage}</p>}
      {error && <p className="error">{error}</p>}
      {info && <p className="info">{info}</p>}

      {mode === 'login' ? (
        <form onSubmit={handleLogin}>
          <input type="email" placeholder="אימייל" value={email} onChange={(e) => setEmail(e.target.value)} required />
          <input type="password" placeholder="סיסמה" value={password} onChange={(e) => setPassword(e.target.value)} required />
          <button type="submit">התחבר</button>
        </form>
      ) : (
        <form onSubmit={handleRegister}>
          <input type="text" placeholder="שם מלא" value={name} onChange={(e) => setName(e.target.value)} required />
          <input type="email" placeholder="אימייל" value={email} onChange={(e) => setEmail(e.target.value)} required />
          <input type="password" placeholder="סיסמה" value={password} onChange={(e) => setPassword(e.target.value)} required />
          <button type="submit">הירשם</button>
        </form>
      )}
    </div>
  );
}
