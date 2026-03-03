import client from './client';

export const fetchParsingRuns = (params) =>
  client.get('/parsing-monitor/runs', { params });

export const fetchParsingRunById = (runId) =>
  client.get(`/parsing-monitor/runs/${runId}`);

export const fetchParsingRunItems = (runId, params) =>
  client.get(`/parsing-monitor/runs/${runId}/items`, { params });

export const subscribeToLiveWithAuth = (onEvent, onError) => {
  const token = localStorage.getItem('admin_token');
  const controller = new AbortController();

  fetch('/api/admin/parsing-monitor/live', {
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    signal: controller.signal,
  })
    .then(async (response) => {
      if (!response.ok) {
        if (response.status === 401) {
          localStorage.removeItem('admin_token');
          window.location.href = '/admin/login';
          return;
        }
        throw new Error(`HTTP ${response.status}`);
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
    })
    .catch((err) => {
      if (err.name !== 'AbortError' && onError) {
        onError(err);
      }
    });

  return () => controller.abort();
};
