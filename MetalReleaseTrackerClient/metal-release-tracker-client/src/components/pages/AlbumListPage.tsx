import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  fetchFilteredAlbums,
  fetchAvailableBands,
  fetchAvailableDistributors,
} from "../../services/albumService";
import {
  Grid,
  Card,
  Typography,
  Button,
  Box,
  Chip,
  Pagination,
  Select,
  MenuItem,
  TextField,
  FormControl,
  InputLabel,
  Autocomplete,
} from "@mui/material";
import { getAlbumStatus } from "../../utils/albumUtils";

const AlbumList = () => {
  const [albums, setAlbums] = useState<any[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [albumsPerPage] = useState(40);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedStatus, setSelectedStatus] = useState<number | null>(null);
  const [selectedMediaType, setSelectedMediaType] = useState<number | null>(
    null
  );
  const [band, setBand] = useState<{ id: string; name: string } | null>(null);
  const [selectedDistributor, setSelectedDistributor] = useState<{
    id: string;
    name: string;
    parsingUrl: string;
    code: number;
  } | null>(null);
  const [availableBands, setAvailableBands] = useState<
    { id: string; name: string }[]
  >([]);
  const [availableDistributors, setAvailableDistributors] = useState<
    { id: string; name: string; parsingUrl: string; code: number }[]
  >([]);
  const [minPrice, setMinPrice] = useState<string>("");
  const [maxPrice, setMaxPrice] = useState<string>("");
  const [albumName, setAlbumName] = useState("");
  const [releaseDateStart, setReleaseDateStart] = useState<string>("");
  const [releaseDateEnd, setReleaseDateEnd] = useState<string>("");
  const [sortBy, setSortBy] = useState<string>("Name");
  const [sortOrder, setSortOrder] = useState<string>("asc");

  const navigate = useNavigate();

  const mediaTypes = [
    { value: 0, label: "CD" },
    { value: 1, label: "LP" },
    { value: 2, label: "Tape" },
  ];

  const albumStatuses = [
    { value: 0, label: "New" },
    { value: 1, label: "Restock" },
    { value: 2, label: "Preorder" },
  ];

  const sortFields = [
    { value: "Name", label: "Title" },
    { value: "Price", label: "Price" },
    { value: "Band", label: "Band" },
    { value: "ReleaseDate", label: "Newest" },
  ];

  useEffect(() => {
    const fetchBands = async () => {
      try {
        const bands = await fetchAvailableBands(0, 40);
        setAvailableBands(
          bands.map((band: any) => ({ id: band.id, name: band.name }))
        );
      } catch (error) {
        setError("Error loading bands");
      } finally {
        setLoading(false);
      }
    };

    fetchBands();
  }, []);

  useEffect(() => {
    const fetchDistributors = async () => {
      try {
        const distributors = await fetchAvailableDistributors();
        setAvailableDistributors(distributors);
      } catch {
        setError("Error loading distributors");
      }
    };
    fetchDistributors();
  }, []);

  const fetchFilteredAlbumsData = async () => {
    try {
      setLoading(true);
      const filters = {
        DistributorId: selectedDistributor?.id || undefined,
        BandId: band?.id || undefined,
        AlbumName: albumName || undefined,
        Media: selectedMediaType !== null ? selectedMediaType : undefined,
        Status: selectedStatus !== null ? selectedStatus : undefined,
        MinimumPrice: minPrice ? parseFloat(minPrice) : undefined,
        MaximumPrice: maxPrice ? parseFloat(maxPrice) : undefined,
        ReleaseDateStart: releaseDateStart
          ? new Date(releaseDateStart).toISOString()
          : undefined,
        ReleaseDateEnd: releaseDateEnd
          ? new Date(releaseDateEnd).toISOString()
          : undefined,
        Skip: (currentPage - 1) * albumsPerPage,
        Take: albumsPerPage,
        OrderBy: sortBy,
        Descending: sortOrder === "desc",
      };
      const data = await fetchFilteredAlbums(filters);
      setAlbums(data.albums || []);
      setTotalCount(data.totalCount);
    } catch {
      setError("Error filtering albums");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchFilteredAlbumsData();
  }, [sortBy, sortOrder]);

  useEffect(() => {
    if (releaseDateStart || releaseDateEnd) {
      fetchFilteredAlbumsData();
    }
  }, [releaseDateStart, releaseDateEnd]);

  const handleMinPriceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    if (value && parseFloat(value) < 0) {
      setMinPrice("0");
    } else {
      setMinPrice(value);
    }
  };

  const handleMaxPriceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    if (value && parseFloat(value) < 0) {
      setMaxPrice("0");
    } else {
      setMaxPrice(value);
    }
  };

  const handleSearchClick = () => {
    setCurrentPage(1);
    fetchFilteredAlbumsData();
  };

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.key === "Enter") {
      handleSearchClick();
    }
  };

  const handlePageChange = (_: React.ChangeEvent<unknown>, page: number) => {
    setCurrentPage(page);
    fetchFilteredAlbumsData();
  };

  const handleAlbumClick = (albumId: string) => {
    navigate(`/albums/${albumId}`);
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  return (
    <Box padding={2}>
      <Box mb={4}>
        <Typography variant="h6" gutterBottom>
          Filter by
        </Typography>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={6} sm={3} md={1}>
            <FormControl fullWidth size="small">
              <InputLabel>Status</InputLabel>
              <Select
                value={selectedStatus}
                onChange={(e) =>
                  setSelectedStatus(e.target.value as number | null)
                }
                displayEmpty
              >
                <MenuItem value="">All</MenuItem>
                {albumStatuses.map((status) => (
                  <MenuItem key={status.value} value={status.value}>
                    {status.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6} sm={3} md={1}>
            <FormControl fullWidth size="small">
              <InputLabel>Media Type</InputLabel>
              <Select
                value={selectedMediaType}
                onChange={(e) =>
                  setSelectedMediaType(e.target.value as number | null)
                }
                displayEmpty
              >
                <MenuItem value="">All</MenuItem>
                {mediaTypes.map((media) => (
                  <MenuItem key={media.value} value={media.value}>
                    {media.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={3} md={1}>
            <Autocomplete
              options={availableDistributors}
              getOptionLabel={(option) => option.name}
              value={selectedDistributor}
              onChange={(event, newValue) => setSelectedDistributor(newValue)}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Distributor"
                  placeholder="Select a distributor"
                  fullWidth
                  size="small"
                />
              )}
              noOptionsText="No distributors available"
              isOptionEqualToValue={(option, value) => option.id === value?.id}
            />
          </Grid>
          <Grid item xs={12} sm={3} md={1}>
            <Autocomplete
              options={availableBands}
              getOptionLabel={(option) => option.name}
              value={band}
              onChange={(event, newValue) => setBand(newValue)}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Band"
                  placeholder="Select a band"
                  fullWidth
                  size="small"
                />
              )}
              noOptionsText="No bands available"
              isOptionEqualToValue={(option, value) => option === value}
            />
          </Grid>
          <Grid item xs={12} sm={3} md={1}>
            <TextField
              label="Album Name"
              value={albumName}
              onChange={(e) => setAlbumName(e.target.value)}
              fullWidth
              placeholder="Enter album name"
              size="small"
              onKeyDown={handleKeyPress}
            />
          </Grid>
          <Grid item xs={6} sm={3} md={1}>
            <TextField
              label="Min Price"
              type="number"
              value={minPrice}
              onChange={handleMinPriceChange}
              fullWidth
              placeholder="Enter minimum price"
              size="small"
              onKeyDown={handleKeyPress}
            />
          </Grid>
          <Grid item xs={6} sm={3} md={1}>
            <TextField
              label="Max Price"
              type="number"
              value={maxPrice}
              onChange={handleMaxPriceChange}
              fullWidth
              placeholder="Enter maximum price"
              size="small"
              onKeyDown={handleKeyPress}
            />
          </Grid>
          <Grid item xs={6} sm={3} md={1}>
            <TextField
              label="Release Date From"
              type="date"
              value={releaseDateStart}
              onChange={(e) => setReleaseDateStart(e.target.value)}
              fullWidth
              size="small"
            />
          </Grid>
          <Grid item xs={6} sm={3} md={1}>
            <TextField
              label="Release Date To"
              type="date"
              value={releaseDateEnd}
              onChange={(e) => setReleaseDateEnd(e.target.value)}
              fullWidth
              size="small"
            />
          </Grid>
          <Grid item xs={12}>
            <Button
              onClick={handleSearchClick}
              variant="contained"
              color="primary"
              disabled={parseFloat(maxPrice) < parseFloat(minPrice)}
            >
              Search
            </Button>
          </Grid>
        </Grid>
      </Box>

      <Box mb={4}>
        <Typography variant="h6" gutterBottom>
          Sort by
        </Typography>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={6} sm={3} md={1}>
            <FormControl fullWidth size="small">
              <InputLabel>Sort By</InputLabel>
              <Select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
                displayEmpty
              >
                {sortFields.map((field) => (
                  <MenuItem key={field.value} value={field.value}>
                    {field.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6} sm={3} md={1}>
            <FormControl fullWidth size="small">
              <InputLabel>Sort Order</InputLabel>
              <Select
                value={sortOrder}
                onChange={(e) => setSortOrder(e.target.value)}
                displayEmpty
              >
                <MenuItem value="asc">Ascending</MenuItem>
                <MenuItem value="desc">Descending</MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Box>
      <Grid container spacing={3}>
        {albums.length === 0 ? (
          <Typography variant="h6" color="textSecondary">
            No albums found.
          </Typography>
        ) : (
          albums.map((album) => (
            <Grid item xs={12} sm={6} md={4} lg={3} key={album.id}>
              <Card
                onClick={() => handleAlbumClick(album.id)}
                style={{ cursor: "pointer" }}
              >
                <Box
                  style={{
                    display: "flex",
                    flexDirection: "row",
                    padding: "16px",
                  }}
                >
                  <Box style={{ flex: 1, textAlign: "center" }}>
                    <img
                      src={album.photoUrl}
                      alt={album.name}
                      style={{
                        maxWidth: "100%",
                        maxHeight: "120px",
                        objectFit: "cover",
                      }}
                    />
                  </Box>
                  <Box style={{ flex: 2, paddingLeft: "16px" }}>
                    <Typography variant="h6" gutterBottom>
                      <a style={{ textDecoration: "none" }}>{album.name}</a>
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      {album.band.name}
                    </Typography>
                    {getAlbumStatus(album.status) ? (
                      <Chip
                        label={getAlbumStatus(album.status)}
                        color="primary"
                        style={{ marginTop: "8px" }}
                      />
                    ) : null}
                    <Typography
                      variant="body2"
                      color="textPrimary"
                      style={{ marginTop: "8px" }}
                    >
                      {album.price} EUR
                    </Typography>
                    <Button
                      variant="contained"
                      color="primary"
                      style={{ marginTop: "8px" }}
                    >
                      Details
                    </Button>
                  </Box>
                </Box>
              </Card>
            </Grid>
          ))
        )}
      </Grid>

      <Box mt={4} display="flex" justifyContent="center">
        <Pagination
          count={Math.ceil(totalCount / albumsPerPage)}
          page={currentPage}
          onChange={handlePageChange}
          color="primary"
        />
      </Box>
    </Box>
  );
};

export default AlbumList;
