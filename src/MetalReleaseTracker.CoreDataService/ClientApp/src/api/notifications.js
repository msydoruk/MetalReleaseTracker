import client from './client';

export const fetchNotificationStats = () => client.get('/notifications/stats');
export const sendBroadcast = (data) => client.post('/notifications/broadcast', data);
