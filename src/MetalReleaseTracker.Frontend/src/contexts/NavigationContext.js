import React, { createContext, useContext, useState, useEffect, useRef } from 'react';
import { fetchPublicNavigation } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';

const NavigationContext = createContext();

export const NavigationProvider = ({ children }) => {
  const { language } = useLanguage();
  const [navItems, setNavItems] = useState([]);
  const cacheRef = useRef({});

  useEffect(() => {
    if (cacheRef.current[language]) {
      setNavItems(cacheRef.current[language]);
      return;
    }

    let cancelled = false;
    fetchPublicNavigation(language)
      .then((response) => {
        if (cancelled) return;
        const data = response.data;
        if (Array.isArray(data) && data.length > 0) {
          cacheRef.current[language] = data;
          setNavItems(data);
        }
      })
      .catch((error) => {
        console.error('Failed to fetch navigation:', error);
      });
    return () => { cancelled = true; };
  }, [language]);

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
