import client from './client';

export const fetchTelegramStats = () => client.get('/telegram/stats');
export const fetchLinkedUsers = () => client.get('/telegram/linked-users');
