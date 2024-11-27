import "./App.css";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import AlbumList from "./components/pages/AlbumListPage";
import AlbumDetails from "./components/pages/AlbumDetailsPage";

function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<AlbumList />} />
        <Route path="/album/:id" element={<AlbumDetails />} />
      </Routes>
    </>
  );
}

export default App;
