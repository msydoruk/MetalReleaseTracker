import client from './client';

export const fetchDistributors = () => client.get('/distributors');
export const fetchDistributorById = (id) => client.get(`/distributors/${id}`);
export const createDistributor = (data) => client.post('/distributors', data);
export const updateDistributor = (id, data) => client.put(`/distributors/${id}`, data);
export const deleteDistributor = (id) => client.delete(`/distributors/${id}`);
