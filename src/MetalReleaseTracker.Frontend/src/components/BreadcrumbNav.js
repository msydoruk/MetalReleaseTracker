import { useEffect } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import MuiBreadcrumbs from '@mui/material/Breadcrumbs';
import MuiLink from '@mui/material/Link';
import Typography from '@mui/material/Typography';
import HomeIcon from '@mui/icons-material/Home';
import NavigateNextIcon from '@mui/icons-material/NavigateNext';

const BreadcrumbNav = ({ items }) => {
  useEffect(() => {
    if (!items || items.length === 0) return;

    const jsonLd = {
      '@context': 'https://schema.org',
      '@type': 'BreadcrumbList',
      itemListElement: [
        { '@type': 'ListItem', position: 1, name: 'Home', item: window.location.origin },
        ...items.map((item, index) => ({
          '@type': 'ListItem',
          position: index + 2,
          name: item.label,
          ...(item.to && { item: `${window.location.origin}${item.to}` }),
        })),
      ],
    };

    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.textContent = JSON.stringify(jsonLd);
    script.id = 'breadcrumb-jsonld';
    const existing = document.getElementById('breadcrumb-jsonld');
    if (existing) existing.remove();
    document.head.appendChild(script);
    return () => { script.remove(); };
  }, [items]);

  if (!items || items.length === 0) return null;

  return (
    <MuiBreadcrumbs
      separator={<NavigateNextIcon fontSize="small" />}
      sx={{ mb: 2, '& .MuiBreadcrumbs-separator': { color: 'text.secondary' } }}
    >
      <MuiLink
        component={RouterLink}
        to="/"
        color="text.secondary"
        underline="hover"
        sx={{ display: 'flex', alignItems: 'center', gap: 0.5, fontSize: '0.875rem' }}
      >
        <HomeIcon sx={{ fontSize: '1rem' }} />
        Home
      </MuiLink>
      {items.map((item, index) => {
        const isLast = index === items.length - 1;
        return isLast ? (
          <Typography key={item.label} color="text.primary" sx={{ fontSize: '0.875rem' }}>
            {item.label}
          </Typography>
        ) : (
          <MuiLink
            key={item.label}
            component={RouterLink}
            to={item.to}
            color="text.secondary"
            underline="hover"
            sx={{ fontSize: '0.875rem' }}
          >
            {item.label}
          </MuiLink>
        );
      })}
    </MuiBreadcrumbs>
  );
};

export default BreadcrumbNav;
