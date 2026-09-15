import { useEffect, useState } from 'react';
import { api } from '../api';

const PAGE_SIZE = 5;

export default function BooksPage() {
  const [books, setBooks] = useState([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState('');
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState('');

  const loadBooks = async (page, searchTerm) => {
    const query = new URLSearchParams({
      pageNumber: page,
      pageSize: PAGE_SIZE,
      search: searchTerm || '',
    });
    const result = await api.getBooks(query.toString());
    setBooks(result.items);
    setTotalPages(result.totalPages || 1);
  };

  useEffect(() => {
    loadBooks(pageNumber, search);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageNumber]);

  const handleSearch = (e) => {
    e.preventDefault();
    setPageNumber(1);
    loadBooks(1, search);
  };

  const handleLoan = async (bookId) => {
    setMessage('');
    try {
      await api.createLoan(bookId);
      setMessageType('success');
      setMessage('הספר הושאל בהצלחה!');
      loadBooks(pageNumber, search);
    } catch (err) {
      setMessageType('error');
      if (err.status === 409) {
        setMessage(`הספר נתפס: ${err.message}`);
      } else {
        setMessage(err.message);
      }
    }
  };

  return (
    <div>
      <form onSubmit={handleSearch} className="search-bar">
        <input
          type="text"
          placeholder="חיפוש לפי שם, מחבר או ISBN"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button type="submit">חפש</button>
      </form>

      {message && <p className={messageType === 'error' ? 'error' : 'info'}>{message}</p>}

      <table>
        <thead>
          <tr>
            <th>כותרת</th>
            <th>מחבר</th>
            <th>קטגוריה</th>
            <th>סטטוס</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {books.map((book) => (
            <tr key={book.id}>
              <td>{book.title}</td>
              <td>{book.author}</td>
              <td>{book.categoryName}</td>
              <td>{book.isAvailable === 'Available' ? 'זמין' : 'מושאל'}</td>
              <td>
                <button disabled={book.isAvailable !== 'Available'} onClick={() => handleLoan(book.id)}>
                  השאל
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="pagination">
        <button disabled={pageNumber <= 1} onClick={() => setPageNumber((p) => p - 1)}>
          הקודם
        </button>
        <span>
          עמוד {pageNumber} מתוך {totalPages}
        </span>
        <button disabled={pageNumber >= totalPages} onClick={() => setPageNumber((p) => p + 1)}>
          הבא
        </button>
      </div>
    </div>
  );
}
