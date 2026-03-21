import { useState, useCallback } from 'react';

const STORAGE_KEY = 'recentlyViewedAlbums';
const MAX_ITEMS = 15;

const getStoredAlbums = () => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
};

const useRecentlyViewed = () => {
  const [recentAlbums, setRecentAlbums] = useState(getStoredAlbums);

  const addRecentlyViewed = useCallback((album) => {
    const entry = {
      id: album.primaryAlbumId || album.id,
      albumName: album.albumName || album.name,
      bandName: album.bandName,
      bandId: album.bandId,
      photoUrl: album.photoUrl,
      minPrice: album.minPrice || album.price,
      originalYear: album.originalYear,
    };

    setRecentAlbums((previous) => {
      const filtered = previous.filter((item) => item.id !== entry.id);
      const updated = [entry, ...filtered].slice(0, MAX_ITEMS);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
      return updated;
    });
  }, []);

  return { recentAlbums, addRecentlyViewed };
};

export default useRecentlyViewed;
