import axios from 'axios';
import authService from './auth';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5002/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for adding JWT token
api.interceptors.request.use(
  async (config) => {
    try {
      // Get current token (with possible refresh if expiring)
      const token = await authService.getToken();
      
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    } catch (error) {
      console.error('API request interceptor error:', error);
      return config;
    }
  },
  (error) => Promise.reject(error)
);

// Response interceptor for handling auth errors
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response && error.response.status === 401) {
      const hadToken = !!localStorage.getItem('auth_token');
      if (hadToken) {
        console.log('Token expired or invalid, logging out...');
        await authService.logout();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

// Helper to format date for API
const formatDateParam = (date) => {
  if (!date) return null;
  
  try {
    const d = new Date(date);
    if (isNaN(d.getTime())) return null;
    
    // Make sure the date is in UTC format
    return new Date(d.getFullYear(), d.getMonth(), d.getDate()).toISOString();
  } catch (error) {
    console.error('Error formatting date:', error);
    return null;
  }
};

export const fetchAlbums = (filters) => {
  const queryParams = new URLSearchParams();
  if (filters) {
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        if (key === 'releaseDateFrom' || key === 'releaseDateTo') {
          const formattedDate = formatDateParam(value);
          if (formattedDate) {
            queryParams.append(key, formattedDate);
          }
        } else {
          queryParams.append(key, value);
        }
      }
    });
  }
  return api.get(`/albums/filtered?${queryParams.toString()}`);
};
export const fetchAlbumById = (id) => api.get(`/albums/${id}`);
export const fetchGroupedAlbums = (filters) => {
  const queryParams = new URLSearchParams();
  if (filters) {
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        if (key === 'releaseDateFrom' || key === 'releaseDateTo') {
          const formattedDate = formatDateParam(value);
          if (formattedDate) {
            queryParams.append(key, formattedDate);
          }
        } else {
          queryParams.append(key, value);
        }
      }
    });
  }
  return api.get(`/albums/grouped?${queryParams.toString()}`);
};

export const fetchBands = () => api.get('/bands/all');
export const fetchBandById = (id) => api.get(`/bands/${id}`);
export const fetchBandsWithAlbumCount = () => api.get('/bands/with-album-count');

export const fetchDistributors = () => api.get('/distributors/all');
export const fetchDistributorById = (id) => api.get(`/distributors/${id}`);
export const fetchDistributorsWithAlbumCount = () => api.get('/distributors/with-album-count');

export const addFavorite = (albumId) => api.post(`/favorites/${albumId}`);
export const removeFavorite = (albumId) => api.delete(`/favorites/${albumId}`);
export const fetchFavorites = (page, pageSize) => api.get(`/favorites?page=${page}&pageSize=${pageSize}`);
export const fetchFavoriteIds = () => api.get('/favorites/ids');
export const checkFavorite = (albumId) => api.get(`/favorites/${albumId}/check`);

export const submitFeedback = (data) => api.post('/feedback', data);