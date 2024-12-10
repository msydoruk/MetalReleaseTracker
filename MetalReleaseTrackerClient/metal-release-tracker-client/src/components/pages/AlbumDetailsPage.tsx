import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { fetchAlbumById } from "../../services/albumService";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Grid,
} from "@mui/material";
import { getAlbumMediaType } from "../../utils/albumUtils";

const AlbumDetails = () => {
  const { id } = useParams();
  const [album, setAlbum] = useState<any | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAlbumDetails = async () => {
      if (id) {
        try {
          const data = await fetchAlbumById(id);
          setAlbum(data);
        } catch (error) {
          setError("Error loading album details");
        } finally {
          setLoading(false);
        }
      } else {
        setError("Album ID is missing");
        setLoading(false);
      }
    };

    fetchAlbumDetails();
  }, [id]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;
  if (!album) return <div>Album not found</div>;

  return (
    <Box
      padding={2}
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
    >
      <Card style={{ display: "flex", maxWidth: "1200px", width: "100%" }}>
        <Grid container spacing={3} style={{ padding: "20px" }}>
          <Grid item xs={12} md={6}>
            <CardContent>
              <Typography variant="h3" gutterBottom>
                {album.name}
              </Typography>
              <Typography variant="h6" color="textSecondary" gutterBottom>
                Band: {album.band.name}
              </Typography>
              <Typography>Media: {getAlbumMediaType(album.media)}</Typography>
              <Typography>Label: {album.label}</Typography>
              <Typography>Press: {album.sku}</Typography>
              <Typography>Info: {album.description}</Typography>
              <Typography
                variant="body1"
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
                BUY
              </Button>
            </CardContent>
          </Grid>
          <Grid item xs={12} md={6}>
            <CardContent style={{ display: "flex", justifyContent: "center" }}>
              <a href={album.purchaseUrl}>
                <img
                  src={album.photoUrl}
                  alt={album.name}
                  style={{
                    maxWidth: "100%",
                    maxHeight: "400px",
                    objectFit: "cover",
                    borderRadius: "8px",
                  }}
                />
              </a>
            </CardContent>
          </Grid>
        </Grid>
      </Card>
    </Box>
  );
};

export default AlbumDetails;
