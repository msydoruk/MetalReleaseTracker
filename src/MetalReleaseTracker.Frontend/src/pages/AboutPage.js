import React from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Grid,
  Divider
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import PublicIcon from '@mui/icons-material/Public';
import LocalShippingIcon from '@mui/icons-material/LocalShipping';
import LibraryMusicIcon from '@mui/icons-material/LibraryMusic';
import TrackChangesIcon from '@mui/icons-material/TrackChanges';
import GroupsIcon from '@mui/icons-material/Groups';
import usePageMeta from '../hooks/usePageMeta';

const features = [
  {
    icon: <SearchIcon sx={{ fontSize: 40 }} />,
    title: 'Discover',
    description: 'Find Ukrainian metal releases available from foreign distributors and labels that are otherwise hard to track down.'
  },
  {
    icon: <PublicIcon sx={{ fontSize: 40 }} />,
    title: 'Global Reach',
    description: 'We aggregate catalogs from distributors and labels across Europe and beyond, bringing them all to one place.'
  },
  {
    icon: <LocalShippingIcon sx={{ fontSize: 40 }} />,
    title: 'Order Direct',
    description: 'Go straight to the distributor\'s store page and order physical releases directly from the source — no middlemen.'
  },
  {
    icon: <TrackChangesIcon sx={{ fontSize: 40 }} />,
    title: 'Stay Updated',
    description: 'Our automated parsers continuously scan distributor catalogs so you never miss a new release, restock, or pre-order.'
  },
  {
    icon: <LibraryMusicIcon sx={{ fontSize: 40 }} />,
    title: 'All Formats',
    description: 'Vinyl, CD, cassette — browse releases across all physical media formats in one unified catalog.'
  },
  {
    icon: <GroupsIcon sx={{ fontSize: 40 }} />,
    title: 'For the Community',
    description: 'Built by Ukrainian metalheads, for Ukrainian metalheads. Supporting the scene by making its music more accessible worldwide.'
  }
];

const AboutPage = () => {
  usePageMeta('About — Ukrainian Metal Release Tracker', 'Metal Release Tracker aggregates Ukrainian metal releases from foreign distributors and labels into one searchable catalog. Find vinyl, CD, and tape releases and order directly.');

  return (
    <Container maxWidth="lg" sx={{ py: 6 }}>
      {/* Hero */}
      <Box sx={{ textAlign: 'center', mb: 6 }}>
        <Typography variant="h3" component="h1" sx={{ fontWeight: 800, mb: 2 }}>
          Metal Release Tracker {'\uD83C\uDDFA\uD83C\uDDE6'}
        </Typography>
        <Typography variant="h5" color="text.secondary" sx={{ mb: 3, maxWidth: 700, mx: 'auto', lineHeight: 1.6 }}>
          The centralized hub for tracking Ukrainian metal releases sold by foreign distributors and labels.
        </Typography>
        <Divider sx={{ maxWidth: 100, mx: 'auto', borderColor: 'primary.main', borderWidth: 2 }} />
      </Box>

      {/* Problem & Solution */}
      <Paper sx={{ p: 4, mb: 6, borderLeft: '4px solid', borderColor: 'primary.main' }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          The Problem
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 3, lineHeight: 1.8 }}>
          Ukrainian metal bands are releasing incredible music — but their physical releases (vinyl, CD, tape) are often
          distributed exclusively through foreign labels and distros scattered across Europe. For fans in Ukraine and
          worldwide, finding where to buy these releases means manually checking dozens of online shops, many of which
          have no search filters for Ukrainian bands. Releases come and go, and by the time you find out about one,
          it's often sold out.
        </Typography>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          The Solution
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8 }}>
          Metal Release Tracker automatically scans the catalogs of foreign distributors and labels, extracts every
          Ukrainian metal release it finds, and presents them in a single, searchable catalog. Each release links
          directly to the distributor's product page so you can order it immediately. New releases, restocks, and
          pre-orders are tracked continuously — so you'll always know what's available and where to get it.
        </Typography>
      </Paper>

      {/* Features Grid */}
      <Typography variant="h4" sx={{ fontWeight: 700, mb: 4, textAlign: 'center' }}>
        How It Works
      </Typography>
      <Grid container spacing={3} sx={{ mb: 6 }}>
        {features.map((feature, index) => (
          <Grid key={index} size={{ xs: 12, sm: 6, md: 4 }}>
            <Paper sx={{
              p: 3,
              height: '100%',
              textAlign: 'center',
              transition: 'transform 0.2s',
              '&:hover': { transform: 'translateY(-4px)' }
            }}>
              <Box sx={{ color: 'primary.main', mb: 2 }}>
                {feature.icon}
              </Box>
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
                {feature.title}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.7 }}>
                {feature.description}
              </Typography>
            </Paper>
          </Grid>
        ))}
      </Grid>

      {/* Currently Tracking */}
      <Paper sx={{ p: 4, mb: 6, textAlign: 'center' }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          Growing Network of Distributors
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8, maxWidth: 700, mx: 'auto' }}>
          We're continuously adding new foreign distributors and labels that carry Ukrainian metal releases.
          Our automated system monitors their catalogs around the clock, ensuring the most up-to-date information
          on availability, pricing, and new arrivals. The more distributors we track, the less you have to search on your own.
        </Typography>
      </Paper>

      {/* Call to Action */}
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          Support Ukrainian Metal
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8, maxWidth: 600, mx: 'auto' }}>
          Every purchase from a legitimate distributor supports Ukrainian artists and the global metal community.
          Browse the catalog, find something heavy, and order it directly.
        </Typography>
      </Box>
    </Container>
  );
};

export default AboutPage;
