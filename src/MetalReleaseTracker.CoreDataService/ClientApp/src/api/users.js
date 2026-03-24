import client from './client';

export const fetchUsers = (params) => client.get('/users', { params });
export const fetchUserById = (id) => client.get(`/users/${id}`);
export const updateUserRole = (id, data) => client.put(`/users/${id}/role`, data);
export const lockUser = (id, data) => client.put(`/users/${id}/lock`, data);
export const unlockUser = (id) => client.put(`/users/${id}/unlock`);
