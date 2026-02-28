import client from './client';

export const fetchAiVerifications = (params) =>
  client.get('/ai-verification', { params });

export const runVerification = ({ distributorCode, search, catalogueIndexIds } = {}) =>
  client.post('/ai-verification/run', {
    distributorCode: distributorCode || null,
    search: search || null,
    catalogueIndexIds: catalogueIndexIds?.length ? catalogueIndexIds : null,
  });

export const runVerificationStream = async ({ distributorCode, search, catalogueIndexIds } = {}, onEvent) => {
  const token = localStorage.getItem('admin_token');
  const response = await fetch('/api/admin/ai-verification/run', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify({
      distributorCode: distributorCode || null,
      search: search || null,
      catalogueIndexIds: catalogueIndexIds?.length ? catalogueIndexIds : null,
    }),
  });

  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem('admin_token');
      window.location.href = '/admin/login';
      return;
    }
    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
  }

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = '';

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });
    const lines = buffer.split('\n');
    buffer = lines.pop();

    let eventType = null;
    for (const line of lines) {
      if (line.startsWith('event: ')) {
        eventType = line.slice(7).trim();
      } else if (line.startsWith('data: ') && eventType) {
        try {
          const data = JSON.parse(line.slice(6));
          onEvent({ type: eventType, ...data });
        } catch {
          // skip malformed JSON
        }
        eventType = null;
      }
    }
  }
};

export const setDecision = (id, decision) =>
  client.put(`/ai-verification/${id}/decision`, { decision });

export const batchSetDecision = (ids, decision) =>
  client.put('/ai-verification/batch-decision', { ids, decision });

export const bulkSetDecision = (distributorCode, isUkrainian, decision) =>
  client.put('/ai-verification/bulk-decision', {
    distributorCode: distributorCode || null,
    isUkrainian: isUkrainian !== '' ? isUkrainian === 'true' : null,
    decision,
  });
