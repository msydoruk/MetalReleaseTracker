import client from './client';

export const exportAlbums = (format = 'csv') =>
  client.get(`/bulk-data/export/albums?format=${format}`, { responseType: 'blob' });

export const exportBands = (format = 'csv') =>
  client.get(`/bulk-data/export/bands?format=${format}`, { responseType: 'blob' });

export const importAlbums = (file, confirm = false) => {
  const formData = new FormData();
  formData.append('file', file);
  return client.post(`/bulk-data/import/albums?confirm=${confirm}`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
};
