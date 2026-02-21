import client from './client';

export const fetchAiVerifications = (params) =>
  client.get('/ai-verification', { params });

export const runVerification = (distributorCode) =>
  client.post('/ai-verification/run', { distributorCode: distributorCode || null });

export const setDecision = (id, decision) =>
  client.put(`/ai-verification/${id}/decision`, { decision });

export const batchSetDecision = (ids, decision) =>
  client.put('/ai-verification/batch-decision', { ids, decision });
