import client from './client';

export const fetchParsingSessions = (params) =>
  client.get('/parsing-sessions', { params });

export const fetchParsingSessionById = (id) =>
  client.get(`/parsing-sessions/${id}`);

export const updateParsingSessionStatus = (id, parsingStatus) =>
  client.put(`/parsing-sessions/${id}/status`, { parsingStatus });
