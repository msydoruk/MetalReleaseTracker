import client from './client';

export const fetchCurrencies = () => client.get('/currencies');
export const createCurrency = (data) => client.post('/currencies', data);
export const updateCurrency = (id, data) => client.put(`/currencies/${id}`, data);
