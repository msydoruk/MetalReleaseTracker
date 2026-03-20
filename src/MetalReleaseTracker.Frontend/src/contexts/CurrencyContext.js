import React, { createContext, useContext, useState, useCallback } from 'react';

const RATES = {
  EUR: 1,
  UAH: 44.5,
  USD: 1.08,
};

const SYMBOLS = {
  EUR: '\u20AC',
  UAH: '\u20B4',
  USD: '$',
};

const CurrencyContext = createContext();

export const CurrencyProvider = ({ children }) => {
  const [currency, setCurrency] = useState(() => {
    return localStorage.getItem('currency') || 'EUR';
  });

  const changeCurrency = useCallback((newCurrency) => {
    setCurrency(newCurrency);
    localStorage.setItem('currency', newCurrency);
  }, []);

  const convert = useCallback(
    (eurPrice) => {
      const rate = RATES[currency] || 1;
      return eurPrice * rate;
    },
    [currency]
  );

  const format = useCallback(
    (eurPrice) => {
      const converted = convert(eurPrice);
      const symbol = SYMBOLS[currency] || '\u20AC';
      return `${symbol}${converted.toFixed(2)}`;
    },
    [convert, currency]
  );

  return (
    <CurrencyContext.Provider value={{ currency, changeCurrency, convert, format }}>
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
