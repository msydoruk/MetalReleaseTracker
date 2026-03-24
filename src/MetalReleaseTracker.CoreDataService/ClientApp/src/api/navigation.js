import client from './client';

export const fetchNavigationItems = () => client.get('/navigation');
export const createNavigationItem = (data) => client.post('/navigation', data);
export const updateNavigationItem = (id, data) => client.put(`/navigation/${id}`, data);
