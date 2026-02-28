import client from './client';

export const fetchBandReferences = (params) =>
  client.get('/band-references', { params });

export const fetchBandReferenceById = (id) =>
  client.get(`/band-references/${id}`);
