import React, { createContext, useContext, useState, useEffect } from 'react';
import { fetchSeoConfig } from '../services/api';

const SeoContext = createContext();

const DEFAULTS = {
  SiteName: 'Metal Release Tracker',
  SiteUrl: 'https://metal-release.com',
  ContactEmail: 'metal.release.tracker@gmail.com',
  DefaultOgImage: '',
};

export const SeoProvider = ({ children }) => {
  const [seoConfig, setSeoConfig] = useState(DEFAULTS);

  useEffect(() => {
    let cancelled = false;
    fetchSeoConfig()
      .then((response) => {
        if (cancelled) return;
        const data = response.data;
        if (data && typeof data === 'object') {
          setSeoConfig((prev) => ({ ...prev, ...data }));
        }
      })
      .catch((error) => {
        console.error('Failed to fetch SEO config:', error);
      });
    return () => { cancelled = true; };
  }, []);

  return (
    <SeoContext.Provider value={seoConfig}>
      {children}
    </SeoContext.Provider>
  );
};

export const useSeoConfig = () => {
  const context = useContext(SeoContext);
  if (!context) {
    throw new Error('useSeoConfig must be used within a SeoProvider');
  }
  return context;
};
