import client from './client';

export const fetchLanguages = () => client.get('/languages');
export const createLanguage = (data) => client.post('/languages', data);
export const updateLanguage = (code, data) => client.put(`/languages/${code}`, data);
export const deleteLanguage = (code) => client.delete(`/languages/${code}`);
