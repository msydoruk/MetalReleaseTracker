import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  fetchAllAlbums,
  fetchFilteredAlbums,
  fetchAvailableBands,
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
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedStatus, setSelectedStatus] = useState<number | null>(null);
  const [selectedMediaType, setSelectedMediaType] = useState<number | null>(
    null
  );
  const [bandName, setBandName] = useState("");
  const [availableBands, setAvailableBands] = useState<string[]>([]);
  const [minPrice, setMinPrice] = useState<string>("");
  const [maxPrice, setMaxPrice] = useState<string>("");

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

  useEffect(() => {
    const fetchInitialAlbums = async () => {
      try {
        const distributorId = "5a638e17-877f-49f0-a782-b03c58315c25";
        const albums = await fetchAllAlbums(distributorId);
        setAlbums(albums);
      } catch {
        setError("Error loading albums");
      } finally {
        setLoading(false);
      }
    };

    fetchInitialAlbums();
  }, []);

  useEffect(() => {
    const fetchBands = async () => {
      try {
        const bands = await fetchAvailableBands();
        setAvailableBands(bands.map((band: any) => band.name));
      } catch (error) {
        setError("Error loading bands");
      } finally {
        setLoading(false);
      }
    };

    fetchBands();
  }, []);

  useEffect(() => {
    const fetchFilteredAlbumsData = async () => {
      try {
        setLoading(true);
        const filters = {
          Media: selectedMediaType !== null ? selectedMediaType : undefined,
          Status: selectedStatus !== null ? selectedStatus : undefined,
          BandName: bandName || undefined,
          MinimumPrice: minPrice ? parseFloat(minPrice) : undefined,
          MaximumPrice: maxPrice ? parseFloat(maxPrice) : undefined,
          Page: currentPage,
          PageSize: albumsPerPage,
        };
        const filteredAlbums = await fetchFilteredAlbums(filters);
        setAlbums(filteredAlbums);
      } catch {
        setError("Error filtering albums");
      } finally {
        setLoading(false);
      }
    };

    fetchFilteredAlbumsData();
  }, [
    currentPage,
    selectedStatus,
    selectedMediaType,
    bandName,
    minPrice,
    maxPrice,
  ]);

  const handlePageChange = (_: React.ChangeEvent<unknown>, page: number) => {
    setCurrentPage(page);
  };

  const handleAlbumClick = (albumId: string) => {
    navigate(`/album/${albumId}`);
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  const indexOfLastAlbum = currentPage * albumsPerPage;
  const indexOfFirstAlbum = indexOfLastAlbum - albumsPerPage;
  const currentAlbums = albums.slice(indexOfFirstAlbum, indexOfLastAlbum);

  return (
    <Box padding={2}>
      <Box mb={4}>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={3}>
            <FormControl fullWidth>
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
          <Grid item xs={12} sm={3}>
            <FormControl fullWidth>
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
          <Grid item xs={12} sm={3}>
            <FormControl fullWidth>
              <Autocomplete
                options={availableBands}
                getOptionLabel={(option) => option}
                value={bandName}
                onChange={(event, newValue) => setBandName(newValue || "")}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Band"
                    placeholder="Select a band"
                  />
                )}
                noOptionsText="No bands available"
                isOptionEqualToValue={(option, value) => option === value}
              />
            </FormControl>
          </Grid>
          <Grid item xs={6} sm={3}>
            <TextField
              label="Min Price"
              type="number"
              value={minPrice}
              onChange={(e) => setMinPrice(e.target.value)}
              fullWidth
              placeholder="Enter minimum price"
            />
          </Grid>
          <Grid item xs={6} sm={3}>
            <TextField
              label="Max Price"
              type="number"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value)}
              fullWidth
              placeholder="Enter maximum price"
            />
          </Grid>
        </Grid>
      </Box>
      <Grid container spacing={3}>
        {currentAlbums.length === 0 ? (
          <Typography variant="h6" color="textSecondary">
            No albums found.
          </Typography>
        ) : (
          currentAlbums.map((album) => (
            <Grid item xs={12} sm={6} md={4} lg={3} key={album.id}>
              <Card onClick={() => handleAlbumClick(album.id)}>
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
                    {album.status && (
                      <Chip
                        label={getAlbumStatus(album.status)}
                        color="primary"
                        style={{ marginTop: "8px" }}
                      />
                    )}
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
                      href={album.purchaseUrl}
                      style={{ marginTop: "8px" }}
                    >
                      Add to Cart
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
          count={Math.ceil(albums.length / albumsPerPage)}
          page={currentPage}
          onChange={handlePageChange}
          color="primary"
        />
      </Box>
    </Box>
  );
};

export default AlbumList;
