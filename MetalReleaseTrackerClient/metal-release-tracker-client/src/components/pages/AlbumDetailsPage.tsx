import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { fetchAlbumById } from "../../services/albumService";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Chip,
  Button,
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
        } catch (err) {
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
    <Box padding={2}>
      <Card>
        <CardContent>
          <Box style={{ textAlign: "center" }}>
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
          <Typography variant="h3" gutterBottom>
            {album.name}
          </Typography>
          <Typography>Band: {album.band.name}</Typography>
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
            ADD TO SHOPPING CART
          </Button>
        </CardContent>
      </Card>
    </Box>
  );
};

export default AlbumDetails;
