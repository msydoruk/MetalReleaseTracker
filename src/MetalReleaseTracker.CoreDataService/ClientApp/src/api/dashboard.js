import client from './client';

export const fetchDashboardStats = () => client.get('/dashboard/stats');
