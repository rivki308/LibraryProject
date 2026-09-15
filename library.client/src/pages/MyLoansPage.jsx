import { useEffect, useState } from 'react';
import { api } from '../api';

export default function MyLoansPage() {
  const [loans, setLoans] = useState([]);
  const [message, setMessage] = useState('');

  const loadLoans = async () => {
    const result = await api.getMyLoans();
    setLoans(result);
  };

  useEffect(() => {
    loadLoans();
  }, []);

  const handleReturn = async (loanId) => {
    setMessage('');
    try {
      await api.returnLoan(loanId);
      loadLoans();
    } catch (err) {
      setMessage(err.message);
    }
  };

  return (
    <div>
      <h2>ההשאלות שלי</h2>
      {message && <p className="error">{message}</p>}

      <table>
        <thead>
          <tr>
            <th>ספר</th>
            <th>תאריך השאלה</th>
            <th>תאריך יעד</th>
            <th>סטטוס</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {loans.map((loan) => (
            <tr key={loan.id}>
              <td>{loan.bookTitle}</td>
              <td>{new Date(loan.loanDate).toLocaleDateString('he-IL')}</td>
              <td>{new Date(loan.dueDate).toLocaleDateString('he-IL')}</td>
              <td>{loan.status === 'Active' ? 'פעילה' : 'הוחזר'}</td>
              <td>
                {loan.status === 'Active' && (
                  <button onClick={() => handleReturn(loan.id)}>החזר</button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
