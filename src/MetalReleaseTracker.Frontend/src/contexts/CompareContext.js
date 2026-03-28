import { createContext, useContext, useState, useCallback, useMemo } from 'react';

const CompareContext = createContext();
const MAX_COMPARE_ITEMS = 5;
const STORAGE_KEY = 'compareItems';

const loadItems = () => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
};

export const CompareProvider = ({ children }) => {
  const [compareItems, setCompareItems] = useState(loadItems);

  const persist = (items) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  };

  const addToCompare = useCallback((album) => {
    setCompareItems((prev) => {
      if (prev.length >= MAX_COMPARE_ITEMS) return prev;
      if (prev.some((item) => item.id === album.id)) return prev;
      const next = [...prev, {
        id: album.id,
        name: album.name || album.albumName,
        bandName: album.bandName,
        bandSlug: album.bandSlug,
        slug: album.slug || album.albumSlug,
        photoUrl: album.photoUrl,
        price: album.price,
        media: album.media,
        distributorName: album.distributorName,
        status: album.status,
        originalYear: album.originalYear,
        purchaseUrl: album.purchaseUrl,
      }];
      persist(next);
      return next;
    });
  }, []);

  const removeFromCompare = useCallback((albumId) => {
    setCompareItems((prev) => {
      const next = prev.filter((item) => item.id !== albumId);
      persist(next);
      return next;
    });
  }, []);

  const clearCompare = useCallback(() => {
    setCompareItems([]);
    localStorage.removeItem(STORAGE_KEY);
  }, []);

  const isInCompare = useCallback((albumId) => {
    return compareItems.some((item) => item.id === albumId);
  }, [compareItems]);

  const value = useMemo(() => ({
    compareItems,
    addToCompare,
    removeFromCompare,
    clearCompare,
    isInCompare,
    isFull: compareItems.length >= MAX_COMPARE_ITEMS,
    count: compareItems.length,
  }), [compareItems, addToCompare, removeFromCompare, clearCompare, isInCompare]);

  return <CompareContext.Provider value={value}>{children}</CompareContext.Provider>;
};

export const useCompare = () => {
  const context = useContext(CompareContext);
  if (!context) throw new Error('useCompare must be used within CompareProvider');
  return context;
};
