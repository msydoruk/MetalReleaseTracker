import React, { useEffect, useState } from "react";
import axios from "axios";
import {
  Grid,
  Card,
  Typography,
  Button,
  Box,
  Chip,
  Pagination,
} from "@mui/material";
import { getAlbumStatus } from "../../utils/getAlbumStatus";

const AlbumList = () => {
  const [albums, setAlbums] = useState<any[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [albumsPerPage] = useState(40);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAlbums = async () => {
      try {
        const response = await axios.get(
          "https://localhost:44354/api/albums/albums?distributorId=5a638e17-877f-49f0-a782-b03c58315c25"
        );
        setAlbums(response.data);
      } catch (err) {
        setError("Error loading albums");
      } finally {
        setLoading(false);
      }
    };

    fetchAlbums();
  }, []);

  const handlePageChange = (_: React.ChangeEvent<unknown>, page: number) => {
    setCurrentPage(page);
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  const indexOfLastAlbum = currentPage * albumsPerPage;
  const indexOfFirstAlbum = indexOfLastAlbum - albumsPerPage;
  const currentAlbums = albums.slice(indexOfFirstAlbum, indexOfLastAlbum);

  return (
    <Box padding={2}>
      <Grid container spacing={3}>
        {currentAlbums.length === 0 ? (
          <Typography variant="h6" color="textSecondary">
            No albums found.
          </Typography>
        ) : (
          currentAlbums.map((album) => (
            <Grid item xs={12} sm={6} md={4} lg={3} key={album.id}>
              <Card>
                <Box
                  style={{
                    display: "flex",
                    flexDirection: "row",
                    padding: "16px",
                  }}
                >
                  <Box style={{ flex: 1, textAlign: "center" }}>
                    <a href={album.purchaseUrl}>
                      <img
                        src={album.photoUrl}
                        alt={album.name}
                        style={{
                          maxWidth: "100%",
                          maxHeight: "120px",
                          objectFit: "cover",
                        }}
                      />
                    </a>
                  </Box>
                  <Box style={{ flex: 2, paddingLeft: "16px" }}>
                    <Typography variant="h6" gutterBottom>
                      <a
                        href={album.purchaseUrl}
                        style={{ textDecoration: "none" }}
                      >
                        {album.name}
                      </a>
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
