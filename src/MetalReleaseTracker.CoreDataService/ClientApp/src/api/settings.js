import client from './client';

export const fetchCategorySettings = (category) =>
  client.get(`/settings/${category}`);

export const updateCategorySettings = (category, settings) =>
  client.put(`/settings/${category}`, { settings });
