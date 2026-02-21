import Chip from '@mui/material/Chip';

const COLOR_MAP = {
  success: { bg: 'rgba(76, 175, 80, 0.15)', text: '#66bb6a', border: 'rgba(76, 175, 80, 0.4)' },
  error: { bg: 'rgba(244, 67, 54, 0.15)', text: '#ef5350', border: 'rgba(244, 67, 54, 0.4)' },
  warning: { bg: 'rgba(255, 152, 0, 0.15)', text: '#ffa726', border: 'rgba(255, 152, 0, 0.4)' },
  info: { bg: 'rgba(33, 150, 243, 0.15)', text: '#42a5f5', border: 'rgba(33, 150, 243, 0.4)' },
  primary: { bg: 'rgba(183, 28, 28, 0.15)', text: '#e53935', border: 'rgba(183, 28, 28, 0.4)' },
  default: { bg: 'rgba(158, 158, 158, 0.15)', text: '#bdbdbd', border: 'rgba(158, 158, 158, 0.3)' },
};

export default function StatusChip({ value, statusMap }) {
  const config = statusMap[value] || { label: String(value), color: 'default' };
  const colors = COLOR_MAP[config.color] || COLOR_MAP.default;
  return (
    <Chip
      label={config.label}
      size="small"
      sx={{
        backgroundColor: colors.bg,
        color: colors.text,
        border: `1px solid ${colors.border}`,
        fontWeight: 600,
        fontSize: '0.7rem',
        letterSpacing: '0.03em',
      }}
    />
  );
}
