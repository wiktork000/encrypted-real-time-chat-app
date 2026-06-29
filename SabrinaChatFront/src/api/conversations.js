import api from ".";

export const deleteConversation = (conversationId) =>
  api.delete(`/Conversations/${conversationId}`);

export const getConversations = () => api.get("/Conversations");

export const createConversation = (name, ids) =>
  api.post("/Conversations", {
    name: name,
    participantIds: ids,
  });
