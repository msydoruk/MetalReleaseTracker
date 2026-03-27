import client from './client';

export const fetchAlbumsPerWeek = (params) => client.get('/analytics/albums-per-week', { params });

export const fetchUserGrowth = (params) => client.get('/analytics/user-growth', { params });

export const fetchPopularGenres = (params) => client.get('/analytics/popular-genres', { params });

export const fetchTopDistributors = (params) => client.get('/analytics/top-distributors', { params });

export const fetchTopWatchedAlbums = () => client.get('/analytics/top-watched-albums');
