import React, { createContext, useContext, useState, useCallback, useEffect, useMemo } from 'react';
import { createTheme } from '@mui/material';

const ThemeModeContext = createContext();

const baseTypography = {
  fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  h1: { fontWeight: 700 },
  h2: { fontWeight: 600 },
  h3: { fontWeight: 600 },
  h4: { fontWeight: 600 },
  h5: { fontWeight: 500 },
  h6: { fontWeight: 500 },
};

const baseComponents = {
  MuiButton: {
    styleOverrides: {
      root: {
        textTransform: 'none',
        borderRadius: 4,
      },
      contained: {
        boxShadow: 'none',
        '&:hover': {
          boxShadow: '0px 2px 4px rgba(0, 0, 0, 0.25)',
        },
      },
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        borderRadius: 8,
      },
    },
  },
};

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: { main: '#f44336' },
    secondary: { main: '#9e9e9e' },
    background: { default: '#121212', paper: '#1e1e1e' },
  },
  typography: baseTypography,
  components: {
    ...baseComponents,
    MuiAppBar: {
      styleOverrides: { root: { backgroundColor: '#000000' } },
    },
    MuiCard: {
      styleOverrides: { root: { backgroundColor: '#1e1e1e', borderRadius: 8 } },
    },
  },
});

const lightTheme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#d32f2f' },
    secondary: { main: '#616161' },
    background: { default: '#f5f5f5', paper: '#ffffff' },
    text: { primary: '#1a1a1a', secondary: '#555555' },
  },
  typography: baseTypography,
  components: {
    ...baseComponents,
    MuiAppBar: {
      styleOverrides: { root: { backgroundColor: '#d32f2f' } },
    },
    MuiCard: {
      styleOverrides: { root: { backgroundColor: '#ffffff', borderRadius: 8 } },
    },
  },
});

const getSystemPreference = () => {
  if (typeof window !== 'undefined' && window.matchMedia) {
    return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
  }
  return 'dark';
};

export const ThemeModeProvider = ({ children }) => {
  const [themeMode, setThemeMode] = useState(() => {
    return localStorage.getItem('themeMode') || 'dark';
  });

  const changeThemeMode = useCallback((newMode) => {
    setThemeMode(newMode);
    localStorage.setItem('themeMode', newMode);
  }, []);

  const toggleThemeMode = useCallback(() => {
    setThemeMode((previous) => {
      const next = previous === 'dark' ? 'light' : 'dark';
      localStorage.setItem('themeMode', next);
      return next;
    });
  }, []);

  const theme = useMemo(
    () => (themeMode === 'light' ? lightTheme : darkTheme),
    [themeMode]
  );

  useEffect(() => {
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    if (metaThemeColor) {
      metaThemeColor.setAttribute('content', themeMode === 'light' ? '#d32f2f' : '#000000');
    }
  }, [themeMode]);

  return (
    <ThemeModeContext.Provider value={{ themeMode, changeThemeMode, toggleThemeMode, theme }}>
      {children}
    </ThemeModeContext.Provider>
  );
};

export const useThemeMode = () => {
  const context = useContext(ThemeModeContext);
  if (!context) {
    throw new Error('useThemeMode must be used within a ThemeModeProvider');
  }
  return context;
};
