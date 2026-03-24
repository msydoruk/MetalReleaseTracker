import client from './client';

export const fetchReviews = (params) => client.get('/reviews', { params });
export const deleteReview = (id) => client.delete(`/reviews/${id}`);
