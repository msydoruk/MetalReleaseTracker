import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';

export default function FilterBar({ children }) {
  return (
    <Paper
      variant="outlined"
      sx={{
        p: 2,
        mb: 2,
        borderColor: 'rgba(255, 255, 255, 0.06)',
        backgroundColor: 'rgba(255, 255, 255, 0.02)',
        borderRadius: 2,
      }}
    >
      <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap' }} useFlexGap>
        {children}
      </Stack>
    </Paper>
  );
}
