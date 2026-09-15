import { useEffect, useState } from 'react';
import { api } from '../api';

export default function AdminMembersPage() {
  const [members, setMembers] = useState([]);
  const [loans, setLoans] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    Promise.all([api.getMembers(), api.getAllLoans()])
      .then(([membersResult, loansResult]) => {
        setMembers(membersResult);
        setLoans(loansResult);
      })
      .catch((err) => setError(err.message));
  }, []);

  const activeLoans = loans.filter((loan) => loan.status === 'Active');

  return (
    <div>
      {error && <p className="error">{error}</p>}

      <h2>מנויים</h2>
      <table>
        <thead>
          <tr>
            <th>שם</th>
            <th>אימייל</th>
            <th>ספרים אצלו כרגע</th>
          </tr>
        </thead>
        <tbody>
          {members.map((member) => {
            const memberLoans = activeLoans.filter((loan) => loan.memberId === member.id);
            return (
              <tr key={member.id}>
                <td>{member.name}</td>
                <td>{member.email}</td>
                <td>{memberLoans.length > 0 ? memberLoans.map((l) => l.bookTitle).join(', ') : '—'}</td>
              </tr>
            );
          })}
        </tbody>
      </table>

      <h2>השאלות פעילות (איזה ספר אצל מי)</h2>
      <table>
        <thead>
          <tr>
            <th>ספר</th>
            <th>מנוי</th>
            <th>תאריך השאלה</th>
            <th>תאריך יעד</th>
          </tr>
        </thead>
        <tbody>
          {activeLoans.length === 0 && (
            <tr>
              <td colSpan="4">אין השאלות פעילות כרגע.</td>
            </tr>
          )}
          {activeLoans.map((loan) => (
            <tr key={loan.id}>
              <td>{loan.bookTitle}</td>
              <td>{loan.memberName}</td>
              <td>{new Date(loan.loanDate).toLocaleDateString('he-IL')}</td>
              <td>{new Date(loan.dueDate).toLocaleDateString('he-IL')}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
