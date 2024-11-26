import "./App.css";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import AlbumList from "./components/pages/AlbumListPage";

function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<AlbumList />} />
      </Routes>
    </>
  );
}

export default App;
