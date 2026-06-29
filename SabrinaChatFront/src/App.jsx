import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { useEffect, useState } from "react";
import Login from "./pages/Login";
import ChatApp from "./components/ChatApp";
import { me } from "./api/users.js";

export default function App() {
  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState(null);

  const fetchUser = async () => {
    const token = localStorage.getItem("token");
    const name = localStorage.getItem("name");
    const keysValid = ["id", "public_key", "private_key"].every((k) =>
      localStorage.getItem(k),
    );

    if (!token || !keysValid) {
      setLoading(false);
      return;
    }

    try {
      const { data } = await me();

      if (data.name === name) {
        setUser({ id: data.id, name: data.name });
      } else {
        console.warn("User mismatch between token and localStorage");
      }
    } catch (e) {
      console.error("Token invalid or expired", e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (user === null) fetchUser();
  }, [user]);

  if (loading) return <div className="p-4">Loading…</div>;

  return (
    <div className="h-dvh bg-[#fff0f6]">
      <Router basename="/chatapp/">
        <Routes>
          <Route
            path="/"
            element={
              user ? (
                <ChatApp user={user} setUser={setUser} />
              ) : (
                <Navigate to="/login" />
              )
            }
          />
          <Route path="/login" element={<Login setUser={setUser} />} />
          <Route path="*" element={<Navigate to={user ? "/" : "/login"} />} />
        </Routes>
      </Router>
    </div>
  );
}
