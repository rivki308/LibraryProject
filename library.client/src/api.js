const API_BASE_URL = 'http://localhost:5178/api';

function getToken() {
  return localStorage.getItem('token');
}

async function request(path, options = {}) {
  const token = getToken();
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, { ...options, headers });

  let data = null;
  const text = await response.text();
  if (text) {
    data = JSON.parse(text);
  }

  if (!response.ok) {
    const error = new Error(data?.message || 'אירעה שגיאה');
    error.status = response.status;
    error.data = data;
    throw error;
  }

  return data;
}

export const api = {
  register: (body) => request('/auth/register', { method: 'POST', body: JSON.stringify(body) }),
  login: (body) => request('/auth/login', { method: 'POST', body: JSON.stringify(body) }),
  getBooks: (query) => request(`/books?${query}`, { method: 'GET' }),
  getCategories: () => request('/categories', { method: 'GET' }),
  createLoan: (bookId) => request('/loans', { method: 'POST', body: JSON.stringify({ bookId }) }),
  getMyLoans: () => request('/loans/my-loans', { method: 'GET' }),
  returnLoan: (loanId) => request(`/loans/${loanId}/return`, { method: 'PUT' }),
};
