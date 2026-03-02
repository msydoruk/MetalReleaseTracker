import client from './client';

export const fetchCatalogueDetails = (params) =>
  client.get('/catalogue-details', { params });
