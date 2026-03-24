import client from './client';

export const fetchNewsArticles = () => client.get('/news');
export const fetchNewsArticleById = (id) => client.get(`/news/${id}`);
export const createNewsArticle = (data) => client.post('/news', data);
export const updateNewsArticle = (id, data) => client.put(`/news/${id}`, data);
export const deleteNewsArticle = (id) => client.delete(`/news/${id}`);
