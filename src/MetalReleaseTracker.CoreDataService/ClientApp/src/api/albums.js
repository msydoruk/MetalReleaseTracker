import client from './client';

export const fetchAlbums = (params) => client.get('/albums', { params });
export const fetchAlbumById = (id) => client.get(`/albums/${id}`);
export const updateAlbum = (id, data) => client.put(`/albums/${id}`, data);
export const deleteAlbum = (id) => client.delete(`/albums/${id}`);
export const bulkUpdateAlbumStatus = (data) => client.put('/albums/bulk-status', data);
export const generateAlbumSeo = (id) => client.post(`/ai-seo/album/${id}`);
