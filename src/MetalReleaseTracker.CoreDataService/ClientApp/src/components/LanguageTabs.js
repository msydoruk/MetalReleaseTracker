import { useState, useEffect } from 'react';
import Box from '@mui/material/Box';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Typography from '@mui/material/Typography';
import { fetchLanguages } from '../api/languages';

export default function LanguageTabs({ value, onChange }) {
  const [languages, setLanguages] = useState([]);

  useEffect(() => {
    fetchLanguages().then(({ data }) => setLanguages(data)).catch(() => {});
  }, []);

  if (languages.length === 0) return null;

  const current = languages.find(l => l.code === value);

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
      <ToggleButtonGroup value={value} exclusive onChange={(e, v) => { if (v) onChange(v); }} size="small">
        {languages.map(lang => (
          <ToggleButton key={lang.code} value={lang.code} sx={{ px: 2 }}>
            {lang.code.toUpperCase()}
          </ToggleButton>
        ))}
      </ToggleButtonGroup>
      {current && (
        <Typography variant="caption" color="text.secondary">
          Editing {current.name} content
        </Typography>
      )}
    </Box>
  );
}
