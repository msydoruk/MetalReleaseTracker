import React, { useState, useEffect } from 'react';
import { Box, Typography } from '@mui/material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { fetchPriceHistory } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';
import { useCurrency } from '../contexts/CurrencyContext';

const PriceHistoryChart = ({ albumName, bandName }) => {
  const { t } = useLanguage();
  const { format: formatPrice, convert } = useCurrency();
  const [data, setData] = useState([]);

  useEffect(() => {
    if (!albumName || !bandName) return;

    const load = async () => {
      try {
        const response = await fetchPriceHistory(albumName, bandName);
        setData(response.data || []);
      } catch {
        setData([]);
      }
    };

    load();
  }, [albumName, bandName]);

  if (!data.length) return null;

  const chartData = data.map((point) => ({
    date: new Date(point.changedAt).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
    }),
    price: convert(point.price),
    rawPrice: point.price,
    oldPrice: point.oldPrice,
    changeType: point.changeType,
    distributor: point.distributorName,
  }));

  const CustomTooltip = ({ active, payload }) => {
    if (!active || !payload || !payload.length) return null;
    const item = payload[0].payload;
    return (
      <Box
        sx={{
          bgcolor: 'rgba(30, 30, 30, 0.95)',
          border: '1px solid rgba(255,255,255,0.15)',
          borderRadius: 1,
          p: 1.5,
          color: '#fff',
          fontSize: 13,
        }}
      >
        <Typography variant="body2" sx={{ fontWeight: 600 }}>
          {item.distributor}
        </Typography>
        <Typography variant="body2">
          {formatPrice(item.rawPrice)}
          {item.oldPrice != null && ` (was ${formatPrice(item.oldPrice)})`}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {item.changeType} &middot; {item.date}
        </Typography>
      </Box>
    );
  };

  return (
    <Box sx={{ mb: 4 }}>
      <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2 }}>
        {t('priceHistory.title')}
      </Typography>
      <ResponsiveContainer width="100%" height={260}>
        <LineChart data={chartData} margin={{ top: 5, right: 20, bottom: 5, left: 10 }}>
          <XAxis
            dataKey="date"
            tick={{ fill: '#aaa', fontSize: 12 }}
            axisLine={{ stroke: '#444' }}
            tickLine={{ stroke: '#444' }}
          />
          <YAxis
            tick={{ fill: '#aaa', fontSize: 12 }}
            axisLine={{ stroke: '#444' }}
            tickLine={{ stroke: '#444' }}
            tickFormatter={(value) => formatPrice(value / (convert(1) || 1))}
          />
          <Tooltip content={<CustomTooltip />} />
          <Line
            type="stepAfter"
            dataKey="price"
            stroke="#90caf9"
            strokeWidth={2}
            dot={{ r: 4, fill: '#90caf9' }}
            activeDot={{ r: 6 }}
          />
        </LineChart>
      </ResponsiveContainer>
    </Box>
  );
};

export default PriceHistoryChart;
