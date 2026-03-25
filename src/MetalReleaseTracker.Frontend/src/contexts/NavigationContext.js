import React, { createContext, useContext, useState, useEffect } from 'react';
import { fetchPublicNavigation } from '../services/api';

const NavigationContext = createContext();

export const NavigationProvider = ({ children }) => {
  const [navItems, setNavItems] = useState([]);

  useEffect(() => {
    let cancelled = false;
    fetchPublicNavigation()
      .then((response) => {
        if (cancelled) return;
        const data = response.data;
        if (Array.isArray(data) && data.length > 0) {
          setNavItems(data);
        }
      })
      .catch((error) => {
        console.error('Failed to fetch navigation:', error);
      });
    return () => { cancelled = true; };
  }, []);

  return (
    <NavigationContext.Provider value={{ navItems }}>
      {children}
    </NavigationContext.Provider>
  );
};

export const useNavigation = () => {
  const context = useContext(NavigationContext);
  if (!context) {
    throw new Error('useNavigation must be used within a NavigationProvider');
  }
  return context;
};
