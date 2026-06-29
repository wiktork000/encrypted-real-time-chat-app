import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login, register } from "../api/auth";
import { me } from "../api/users.js";

export default function Login({ setUser }) {
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPass, setConfirmPass] = useState("");
  const [isRegistering, setIsRegistering] = useState(false);
  const [username, setUsername] = useState("");
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const { data } = isRegistering
        ? await register(email, password, username)
        : await login(email, password);

      localStorage.setItem("token", data.token);
    } catch (err) {
      alert(err.message);
    }

    try {
      const { data } = await me();
      localStorage.setItem("id", data.id);
      localStorage.setItem("name", data.name);
      localStorage.setItem("public_key", data.publicKey);
      localStorage.setItem("private_key", data.privateKey);
      localStorage.setItem("password", password);

      setUser({ id: data.id, name: data.name });
      //window.location.href = "/chatapp/";
      navigate("/");
    } catch (err) {
      alert("Error" + err);
    }
  };

  return (
    <div className="flex flex-col items-center justify-center h-dvh bg-[#fff0f6] p-4 font-sans">
      <div className="bg-white p-6 rounded-2xl shadow-xl w-full max-w-sm border border-[#ff80c9] mx-auto">
        <h1 className="text-3xl font-bold mb-6 text-center text-[#9b1859]">
          Short n’ Sweet 💋
        </h1>

        <form onSubmit={handleSubmit} className="space-y-3">
          {isRegistering && (
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Username"
              className="border border-[#ff80c9] bg-[#fff5fa] placeholder-[#ff80c9] text-[#9b1859] rounded-full px-4 py-2 w-full focus:outline-none focus:ring-2 focus:ring-[#ff5ca2]"
            />
          )}

          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="Email"
            className="border border-[#ff80c9] bg-[#fff5fa] placeholder-[#ff80c9] text-[#9b1859] rounded-full px-4 py-2 w-full focus:outline-none focus:ring-2 focus:ring-[#ff5ca2]"
          />

          <div className="relative">
            <input
              type={showPassword ? "text" : "password"}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Password"
              className="border border-[#ff80c9] bg-[#fff5fa] placeholder-[#ff80c9] text-[#9b1859] rounded-full px-4 py-2 w-full pr-10 focus:outline-none focus:ring-2 focus:ring-[#ff5ca2]"
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-3 top-2 text-sm text-[#9b1859]"
            >
              {showPassword ? "Hide" : "Show"}
            </button>
          </div>

          {isRegistering && (
            <input
              type="password"
              value={confirmPass}
              onChange={(e) => setConfirmPass(e.target.value)}
              placeholder="Confirm Password"
              className="border border-[#ff80c9] bg-[#fff5fa] placeholder-[#ff80c9] text-[#9b1859] rounded-full px-4 py-2 w-full focus:outline-none focus:ring-2 focus:ring-[#ff5ca2]"
            />
          )}

          <div className="flex justify-between items-center text-sm text-[#9b1859]">
            <button
              type="button"
              onClick={() => setIsRegistering(!isRegistering)}
              className="hover:underline ml-auto w-full"
            >
              {isRegistering ? "I already have an account" : "Create account"}
            </button>
          </div>

          <button
            type="submit"
            className="bg-[#ff5ca2] hover:bg-[#ff3d94] text-white px-4 py-2 rounded-full w-full transition"
          >
            {isRegistering ? "Register" : "Login"}
          </button>
        </form>
      </div>
    </div>
  );
}
