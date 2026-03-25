import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { fetchPublicCurrencies } from '../services/api';

const CurrencyContext = createContext();

export const CurrencyProvider = ({ children }) => {
  const [currency, setCurrency] = useState(() => {
    return localStorage.getItem('currency') || 'EUR';
  });
  const [rates, setRates] = useState({});
  const [symbols, setSymbols] = useState({});
  const [availableCurrencies, setAvailableCurrencies] = useState([]);

  useEffect(() => {
    let cancelled = false;
    fetchPublicCurrencies()
      .then((response) => {
        if (cancelled) return;
        const data = response.data;
        if (Array.isArray(data) && data.length > 0) {
          const newRates = {};
          const newSymbols = {};
          data.forEach((item) => {
            newRates[item.code] = item.rateToEur;
            newSymbols[item.code] = item.symbol;
          });
          setRates(newRates);
          setSymbols(newSymbols);
          setAvailableCurrencies(data.map((item) => item.code));
        }
      })
      .catch((error) => {
        console.error('Failed to fetch currencies:', error);
      });
    return () => { cancelled = true; };
  }, []);

  const changeCurrency = useCallback((newCurrency) => {
    setCurrency(newCurrency);
    localStorage.setItem('currency', newCurrency);
  }, []);

  const convert = useCallback(
    (eurPrice) => {
      const rate = rates[currency] || 1;
      return eurPrice * rate;
    },
    [currency, rates]
  );

  const format = useCallback(
    (eurPrice) => {
      const converted = convert(eurPrice);
      const symbol = symbols[currency] || '\u20AC';
      return `${symbol}${converted.toFixed(2)}`;
    },
    [convert, currency, symbols]
  );

  return (
    <CurrencyContext.Provider value={{ currency, changeCurrency, convert, format, availableCurrencies }}>
      {children}
    </CurrencyContext.Provider>
  );
};

export const useCurrency = () => {
  const context = useContext(CurrencyContext);
  if (!context) {
    throw new Error('useCurrency must be used within a CurrencyProvider');
  }
  return context;
};
