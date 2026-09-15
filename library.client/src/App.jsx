import { useState } from 'react';
import './App.css';
import { useAuth } from './AuthContext';
import AuthPage from './pages/AuthPage';
import BooksPage from './pages/BooksPage';
import MyLoansPage from './pages/MyLoansPage';

function App() {
  const { member, logout } = useAuth();
  const [view, setView] = useState('books');

  if (!member) {
    return <AuthPage />;
  }

  return (
    <div className="app-shell">
      <nav className="navbar">
        <div className="brand">מערכת ספרייה</div>
        <div className="nav-links">
          <button className={view === 'books' ? 'active' : ''} onClick={() => setView('books')}>
            ספרים
          </button>
          <button className={view === 'loans' ? 'active' : ''} onClick={() => setView('loans')}>
            ההשאלות שלי
          </button>
        </div>
        <div className="nav-user">
          <span>שלום, {member.name}</span>
          <button onClick={logout}>התנתק</button>
        </div>
      </nav>

      <main className="content">
        {view === 'books' ? <BooksPage /> : <MyLoansPage />}
      </main>
    </div>
  );
}

export default App;
