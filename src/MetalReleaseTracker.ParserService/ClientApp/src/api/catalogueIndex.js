import client from './client';

export const fetchCatalogueIndex = (params) =>
  client.get('/catalogue-index', { params });

export const updateCatalogueIndexStatus = (id, status) =>
  client.put(`/catalogue-index/${id}/status`, { status });

export const batchUpdateCatalogueIndexStatus = (ids, status) =>
  client.put('/catalogue-index/batch-status', { ids, status });
