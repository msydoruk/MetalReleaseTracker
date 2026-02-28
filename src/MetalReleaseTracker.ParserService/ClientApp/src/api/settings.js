import client from './client';

// AI Agents
export const fetchAiAgents = () =>
  client.get('/settings/ai-agents');

export const fetchAiAgentById = (id) =>
  client.get(`/settings/ai-agents/${id}`);

export const createAiAgent = (data) =>
  client.post('/settings/ai-agents', data);

export const updateAiAgent = (id, data) =>
  client.put(`/settings/ai-agents/${id}`, data);

export const deleteAiAgent = (id) =>
  client.delete(`/settings/ai-agents/${id}`);

// Parsing Sources
export const fetchParsingSources = () =>
  client.get('/settings/parsing-sources');

export const updateParsingSource = (id, data) =>
  client.put(`/settings/parsing-sources/${id}`, data);

// Category Settings
export const fetchCategorySettings = (category) =>
  client.get(`/settings/${category}`);

export const updateCategorySettings = (category, settings) =>
  client.put(`/settings/${category}`, { settings });
