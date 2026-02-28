import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';

export default function PageHeader({ icon, title, subtitle, action }) {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
        {icon && (
          <Box sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: 40,
            height: 40,
            borderRadius: 2,
            backgroundColor: 'rgba(183, 28, 28, 0.15)',
            color: '#e53935',
          }}>
            {icon}
          </Box>
        )}
        <Box>
          <Typography variant="h5" sx={{ lineHeight: 1.2 }}>{title}</Typography>
          {subtitle && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
              {subtitle}
            </Typography>
          )}
        </Box>
      </Box>
      {action && <Box>{action}</Box>}
    </Box>
  );
}
