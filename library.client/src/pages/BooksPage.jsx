import { useEffect, useState } from 'react';
import { api } from '../api';
import { useAuth } from '../AuthContext';

const PAGE_SIZE = 5;
const EMPTY_FORM = { title: '', author: '', isbn: '', publishYear: '', categoryId: '' };

export default function BooksPage() {
  const { isAdmin } = useAuth();
  const [books, setBooks] = useState([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState('');
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState('');

  const [categories, setCategories] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState('');

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

  useEffect(() => {
    if (isAdmin) {
      api.getCategories().then(setCategories).catch(() => setCategories([]));
    }
  }, [isAdmin]);

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

  const handleDelete = async (bookId) => {
    if (!window.confirm('למחוק את הספר?')) {
      return;
    }
    setMessage('');
    try {
      await api.deleteBook(bookId);
      setMessageType('success');
      setMessage('הספר נמחק בהצלחה.');
      const isLastOnPage = books.length === 1 && pageNumber > 1;
      if (isLastOnPage) {
        setPageNumber((p) => p - 1);
      } else {
        loadBooks(pageNumber, search);
      }
    } catch (err) {
      setMessageType('error');
      setMessage(err.message);
    }
  };

  const handleFormChange = (field) => (e) => {
    setForm((f) => ({ ...f, [field]: e.target.value }));
  };

  const handleCreateBook = async (e) => {
    e.preventDefault();
    setFormError('');
    try {
      await api.createBook({
        title: form.title,
        author: form.author,
        isbn: form.isbn,
        publishYear: Number(form.publishYear),
        categoryId: Number(form.categoryId),
      });
      setForm(EMPTY_FORM);
      setShowForm(false);
      setMessageType('success');
      setMessage('הספר נוסף בהצלחה!');
      setPageNumber(1);
      loadBooks(1, search);
    } catch (err) {
      setFormError(err.message);
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
        {isAdmin && (
          <button type="button" onClick={() => setShowForm((s) => !s)}>
            {showForm ? 'ביטול' : 'הוסף ספר +'}
          </button>
        )}
      </form>

      {isAdmin && showForm && (
        <form onSubmit={handleCreateBook} className="book-form">
          {formError && <p className="error">{formError}</p>}
          <input type="text" placeholder="כותרת" value={form.title} onChange={handleFormChange('title')} required />
          <input type="text" placeholder="מחבר" value={form.author} onChange={handleFormChange('author')} required />
          <input type="text" placeholder="ISBN" value={form.isbn} onChange={handleFormChange('isbn')} required />
          <input
            type="number"
            placeholder="שנת הוצאה"
            value={form.publishYear}
            onChange={handleFormChange('publishYear')}
            required
          />
          <select value={form.categoryId} onChange={handleFormChange('categoryId')} required>
            <option value="" disabled>
              בחר קטגוריה
            </option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
          <button type="submit">שמור ספר</button>
        </form>
      )}

      {message && <p className={messageType === 'error' ? 'error' : 'info'}>{message}</p>}

      <table>
        <thead>
          <tr>
            <th>כותרת</th>
            <th>מחבר</th>
            <th>קטגוריה</th>
            <th>סטטוס</th>
            <th></th>
            {isAdmin && <th></th>}
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
              {isAdmin && (
                <td>
                  <button className="danger" onClick={() => handleDelete(book.id)}>
                    מחק
                  </button>
                </td>
              )}
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
