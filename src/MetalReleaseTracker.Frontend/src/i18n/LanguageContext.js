import React, { createContext, useContext, useState, useCallback, useEffect, useRef } from 'react';
import { fetchPublicTranslations, fetchPublicLanguages } from '../services/api';

const LanguageContext = createContext();

export const LanguageProvider = ({ children }) => {
  const [availableLanguages, setAvailableLanguages] = useState([]);
  const [language, setLanguage] = useState(() => {
    return localStorage.getItem('language') || 'en';
  });
  const [translations, setTranslations] = useState({});
  const [translationsLoaded, setTranslationsLoaded] = useState(false);
  const fetchedLanguageRef = useRef(null);

  useEffect(() => {
    fetchPublicLanguages()
      .then(({ data }) => {
        setAvailableLanguages(data);
        const stored = localStorage.getItem('language');
        if (stored && !data.find(l => l.code === stored)) {
          const defaultLang = data.find(l => l.isDefault)?.code || 'en';
          setLanguage(defaultLang);
          localStorage.setItem('language', defaultLang);
        }
      })
      .catch((error) => {
        console.error('Failed to fetch languages:', error);
      });
  }, []);

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

  const changeLanguage = useCallback((newLang) => {
    setLanguage(newLang);
    localStorage.setItem('language', newLang);
  }, []);

  // Keep toggleLanguage for backward compatibility during transition
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
    <LanguageContext.Provider value={{ language, changeLanguage, toggleLanguage, availableLanguages, t, translationsLoaded }}>
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
