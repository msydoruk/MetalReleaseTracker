import client from './client';

export const fetchAuditLogs = (params) => client.get('/audit-log', { params });
