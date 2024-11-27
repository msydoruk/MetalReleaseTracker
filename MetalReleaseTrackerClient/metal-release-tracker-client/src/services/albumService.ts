import axios from "axios";

const API_BASE_URL = "https://localhost:44354/api/albums";

export const fetchAllAlbums = async (distributorId: string) => {
  const response = await axios.get(`${API_BASE_URL}/albums`, {
    params: { distributorId },
  });
  return response.data;
};

export const fetchFilteredAlbums = async (filters: {
  BandName?: string;
  Media?: number;
  Status?: number;
  Page?: number;
  PageSize?: number;
}) => {
  const response = await axios.get(`${API_BASE_URL}/filter`, { params: filters });
  return response.data;
};

export const fetchAlbumById = async (id: string) => {
    const response = await axios.get(`${API_BASE_URL}/album?id=${id}`);
    return response.data;
};