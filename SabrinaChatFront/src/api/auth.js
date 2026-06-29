import api from "./index";

export const register = (email, password, username) =>
  api.post("/auth/register", { email, password, username });

export const login = (email, password) =>
  api.post("/auth/login", { email, password });
