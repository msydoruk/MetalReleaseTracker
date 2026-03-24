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

export const fetchAlbums = (filters) => {
  const queryParams = new URLSearchParams();
  if (filters) {
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        queryParams.append(key, value);
      }
    });
  }
  return api.get(`/albums/filtered?${queryParams.toString()}`);
};
export const fetchSuggestions = (query) => api.get(`/albums/suggest?q=${encodeURIComponent(query)}`);
export const fetchAlbumById = (id) => api.get(`/albums/${id}`);
export const fetchAlbumDetail = (id) => api.get(`/albums/${id}/detail`);
export const fetchAlbumBySlug = (slug) => api.get(`/albums/by-slug/${slug}`);
export const fetchAlbumDetailBySlug = (slug) => api.get(`/albums/by-slug/${slug}/detail`);
export const fetchGroupedAlbums = (filters) => {
  const queryParams = new URLSearchParams();
  if (filters) {
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        queryParams.append(key, value);
      }
    });
  }
  return api.get(`/albums/grouped?${queryParams.toString()}`);
};

export const fetchBands = () => api.get('/bands/all');
export const fetchBandById = (id) => api.get(`/bands/${id}`);
export const fetchBandBySlug = (slug) => api.get(`/bands/by-slug/${slug}`);
export const fetchBandsWithAlbumCount = () => api.get('/bands/with-album-count');
export const fetchSimilarBands = (bandId) => api.get(`/bands/${bandId}/similar`);

export const fetchGenres = () => api.get('/bands/genres');
export const fetchDistributors = () => api.get('/distributors/all');
export const fetchDistributorById = (id) => api.get(`/distributors/${id}`);
export const fetchDistributorsWithAlbumCount = () => api.get('/distributors/with-album-count');

export const addFavorite = (albumId, status = 0) => api.post(`/favorites/${albumId}?status=${status}`);
export const removeFavorite = (albumId) => api.delete(`/favorites/${albumId}`);
export const fetchFavorites = (page, pageSize, status) => {
  const params = new URLSearchParams({ page, pageSize });
  if (status !== undefined && status !== null) params.append('status', status);
  return api.get(`/favorites?${params.toString()}`);
};
export const fetchFavoriteIds = () => api.get('/favorites/ids');
export const checkFavorite = (albumId) => api.get(`/favorites/${albumId}/check`);
export const updateFavoriteStatus = (albumId, status) => api.put(`/favorites/${albumId}/status`, { status });
export const exportCollection = (format = 'csv') =>
  api.get(`/favorites/export?format=${format}`, { responseType: 'blob' });

export const fetchAlbumRating = (albumId) => api.get(`/ratings/${albumId}`);
export const submitAlbumRating = (albumId, rating) => api.post(`/ratings/${albumId}`, { rating });
export const deleteAlbumRating = (albumId) => api.delete(`/ratings/${albumId}`);

export const fetchReviews = () => api.get('/reviews');
export const submitReview = (data) => api.post('/reviews', data);

export const fetchChangelog = (page, pageSize) =>
  api.get(`/changelog?page=${page}&pageSize=${pageSize}`);

export const fetchPriceHistory = (albumName, bandName) =>
  api.get(`/changelog/price-history?albumName=${encodeURIComponent(albumName)}&bandName=${encodeURIComponent(bandName)}`);

export const followBand = (bandId) => api.post(`/followed-bands/${bandId}`);
export const unfollowBand = (bandId) => api.delete(`/followed-bands/${bandId}`);
export const fetchFollowedBandIds = () => api.get('/followed-bands/ids');
export const checkFollowingBand = (bandId) => api.get(`/followed-bands/${bandId}/check`);
export const fetchFollowedBands = () => api.get('/followed-bands');
export const fetchBandFollowerCount = (bandId) => api.get(`/followed-bands/${bandId}/count`);
export const fetchFeed = (page = 1, pageSize = 20) => api.get(`/followed-bands/feed?page=${page}&pageSize=${pageSize}`);

export const watchAlbum = (albumId) => api.post(`/watches/${albumId}`);
export const unwatchAlbum = (albumId) => api.delete(`/watches/${albumId}`);
export const checkWatchingAlbum = (albumId) => api.get(`/watches/${albumId}/check`);
export const fetchWatchedKeys = () => api.get('/watches/keys');

export const fetchNotifications = (page = 1, pageSize = 20) =>
  api.get(`/notifications?page=${page}&pageSize=${pageSize}`);
export const fetchUnreadNotificationCount = () => api.get('/notifications/unread-count');
export const markNotificationRead = (notificationId) => api.put(`/notifications/${notificationId}/read`);
export const markAllNotificationsRead = () => api.put('/notifications/read-all');

export const generateTelegramToken = () => api.post('/telegram/link-token');
export const getTelegramStatus = () => api.get('/telegram/status');
export const unlinkTelegram = () => api.delete('/telegram/unlink');

// Config
export const fetchPublicCurrencies = () => api.get('/config/currencies');
export const fetchPublicNavigation = () => api.get('/config/navigation');
export const fetchPublicNews = () => api.get('/config/news');