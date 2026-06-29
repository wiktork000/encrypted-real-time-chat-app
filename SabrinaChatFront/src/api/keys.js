import api from ".";

export const getConversationKey = (id) =>
  api.get(`/Keys/conversation/${id}/current`);
