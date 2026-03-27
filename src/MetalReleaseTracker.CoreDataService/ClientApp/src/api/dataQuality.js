import client from './client';

export const fetchDataQualitySummary = () => client.get('/data-quality');

export const fetchAlbumsMissingCovers = (params) => client.get('/data-quality/albums-missing-covers', { params });

export const fetchBandsMissingGenre = (params) => client.get('/data-quality/bands-missing-genre', { params });

export const fetchBandsMissingPhoto = (params) => client.get('/data-quality/bands-missing-photo', { params });

export const fetchPotentialDuplicateBands = (params) => client.get('/data-quality/potential-duplicate-bands', { params });

export const hideAlbum = (id) => client.post(`/data-quality/hide-album/${id}`);

export const mergeBands = (data) => client.post('/bands/merge', data);
