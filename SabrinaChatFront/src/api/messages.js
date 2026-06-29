import api from ".";

export const sendMessage = (message, chatId) =>
  api.post("/Messages", {
    content: message,
    conversationId: chatId,
  });

export const getMessages = (chatId, limit = 50, offset = 0) =>
  api.get(`/Messages/conversation/${chatId}?limit=${limit}&offset=${offset}`);

export const removeMessage = (id) =>
  api.put(`/Messages/${id}`, "", {
    headers: {
      "Content-Type": "application/json",
    },
  });
