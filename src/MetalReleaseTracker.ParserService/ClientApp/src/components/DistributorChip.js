import Chip from '@mui/material/Chip';
import { DISTRIBUTOR_CODES } from '../constants';

export default function DistributorChip({ code }) {
  const config = DISTRIBUTOR_CODES[code] || { label: String(code), color: '#9e9e9e' };
  return (
    <Chip
      label={config.label}
      size="small"
      sx={{
        backgroundColor: config.color + '1a',
        color: config.color,
        borderColor: config.color + '55',
        border: '1px solid',
        fontWeight: 500,
        fontSize: '0.75rem',
      }}
    />
  );
}
