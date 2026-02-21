import React from 'react';
import { Box, Tooltip } from '@mui/material';
import { useLanguage } from '../i18n/LanguageContext';

const CdIcon = ({ size }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none">
    <circle cx="12" cy="12" r="10.5" stroke="#64b5f6" strokeWidth="1.5" />
    <circle cx="12" cy="12" r="3" fill="#64b5f6" />
    <circle cx="12" cy="12" r="1" fill="#1e1e1e" />
    <path d="M12 5.5a6.5 6.5 0 0 1 6.5 6.5" stroke="#64b5f6" strokeWidth="0.5" opacity="0.4" />
    <path d="M12 7a5 5 0 0 1 5 5" stroke="#64b5f6" strokeWidth="0.5" opacity="0.3" />
  </svg>
);

const VinylIcon = ({ size }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none">
    <circle cx="12" cy="12" r="10.5" stroke="#ff9800" strokeWidth="1.5" />
    <circle cx="12" cy="12" r="3.5" fill="#ff9800" />
    <circle cx="12" cy="12" r="1.2" fill="#1e1e1e" />
    <circle cx="12" cy="12" r="7" stroke="#ff9800" strokeWidth="0.4" opacity="0.3" />
    <circle cx="12" cy="12" r="8.5" stroke="#ff9800" strokeWidth="0.4" opacity="0.25" />
    <circle cx="12" cy="12" r="5.5" stroke="#ff9800" strokeWidth="0.4" opacity="0.35" />
    <circle cx="12" cy="12" r="9.5" stroke="#ff9800" strokeWidth="0.4" opacity="0.2" />
  </svg>
);

const CassetteIcon = ({ size }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none">
    <rect x="2" y="5" width="20" height="14" rx="2" stroke="#66bb6a" strokeWidth="1.5" />
    <circle cx="8" cy="12" r="2.5" stroke="#66bb6a" strokeWidth="1" />
    <circle cx="16" cy="12" r="2.5" stroke="#66bb6a" strokeWidth="1" />
    <line x1="10.5" y1="12" x2="13.5" y2="12" stroke="#66bb6a" strokeWidth="0.8" />
    <rect x="6" y="16" width="12" height="2" rx="1" stroke="#66bb6a" strokeWidth="0.8" opacity="0.5" />
  </svg>
);

const mediaConfig = {
  0: { Icon: CdIcon, color: '#64b5f6', labelKey: 'albumCard.mediaCD' },
  1: { Icon: VinylIcon, color: '#ff9800', labelKey: 'albumCard.mediaVinyl' },
  2: { Icon: CassetteIcon, color: '#66bb6a', labelKey: 'albumCard.mediaCassette' },
};

const MediaTypeIcon = ({ mediaType, size = 22 }) => {
  const { t } = useLanguage();
  const config = mediaConfig[mediaType];

  if (!config) {
    return null;
  }

  const { Icon, color, labelKey } = config;
  const label = t(labelKey);

  return (
    <Tooltip title={label} arrow placement="top">
      <Box
        sx={{
          display: 'inline-flex',
          alignItems: 'center',
          gap: 0.5,
          px: 0.8,
          py: 0.3,
          borderRadius: '12px',
          border: `1px solid ${color}33`,
          bgcolor: `${color}15`,
          cursor: 'default',
          lineHeight: 1,
        }}
      >
        <Icon size={size} />
        <Box
          component="span"
          sx={{
            fontSize: '0.7rem',
            fontWeight: 600,
            color,
            letterSpacing: '0.03em',
          }}
        >
          {label}
        </Box>
      </Box>
    </Tooltip>
  );
};

export default MediaTypeIcon;
