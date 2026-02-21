import axios from 'axios';

export const login = (username, password) =>
  axios.post('/api/admin/auth/login', { username, password });
