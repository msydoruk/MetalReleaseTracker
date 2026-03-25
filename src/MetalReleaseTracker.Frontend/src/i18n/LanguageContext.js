import React, { createContext, useContext, useState, useCallback, useEffect, useRef } from 'react';
import { fetchPublicTranslations } from '../services/api';

const LanguageContext = createContext();

export const LanguageProvider = ({ children }) => {
  const [language, setLanguage] = useState(() => {
    return localStorage.getItem('language') || 'en';
  });
  const [translations, setTranslations] = useState({});
  const [translationsLoaded, setTranslationsLoaded] = useState(false);
  const fetchedLanguageRef = useRef(null);

  useEffect(() => {
    if (fetchedLanguageRef.current === language) {
      return;
    }

    let cancelled = false;
    setTranslationsLoaded(false);
    fetchPublicTranslations(language)
      .then((response) => {
        if (cancelled) return;
        const data = response.data;
        if (data && typeof data === 'object') {
          setTranslations(data);
          fetchedLanguageRef.current = language;
        }
        setTranslationsLoaded(true);
      })
      .catch((error) => {
        if (cancelled) return;
        console.error('Failed to fetch translations:', error);
        setTranslationsLoaded(true);
      });
    return () => { cancelled = true; };
  }, [language]);

  const toggleLanguage = useCallback(() => {
    setLanguage((prev) => {
      const next = prev === 'en' ? 'ua' : 'en';
      localStorage.setItem('language', next);
      return next;
    });
  }, []);

  const t = useCallback(
    (key) => {
      return translations[key] || key;
    },
    [translations]
  );

  return (
    <LanguageContext.Provider value={{ language, toggleLanguage, t, translationsLoaded }}>
      {children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
};
