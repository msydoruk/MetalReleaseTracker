import client from './client';

export const fetchTranslations = (params) => client.get('/translations', { params });
export const updateTranslation = (id, data) => client.put(`/translations/${id}`, data);
export const bulkUpdateTranslations = (data) => client.put('/translations/bulk', data);
