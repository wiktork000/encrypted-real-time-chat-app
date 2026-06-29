import api from ".";

export const me = () => api.get("/Users/me");

export const getAllUsers = () => api.get("/Users");

export const getUser = (id) => api.get(`/Users/${id}`);
