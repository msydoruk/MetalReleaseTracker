import client from './client';

export const fetchBands = (params) => client.get('/bands', { params });
export const fetchBandById = (id) => client.get(`/bands/${id}`);
export const updateBand = (id, data) => client.put(`/bands/${id}`, data);
export const mergeBands = (data) => client.post('/bands/merge', data);
export const deleteBand = (id) => client.delete(`/bands/${id}`);
export const generateBandSeo = (id) => client.post(`/ai-seo/band/${id}`);
export const bulkGenerateBandSeo = (limit = 50) => client.post('/ai-seo/bulk/bands', { limit });
