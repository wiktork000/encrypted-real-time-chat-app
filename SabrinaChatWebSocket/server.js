const WebSocket = require("ws");
const axios = require("axios");

const wss = new WebSocket.Server({ port: 8080 });

let clientsByChat = new Map();

wss.on("connection", (ws) => {
  ws.on("message", (data) => {
    try {
      const msg = JSON.parse(data);

      if (msg.type === "subscribe") {
        const { chatId } = msg;
        if (!clientsByChat.has(chatId)) clientsByChat.set(chatId, new Set());
        clientsByChat.get(chatId).add(ws);
        ws.chatId = chatId;
      }

      if (msg.type === "message") {
        const { chatId, content, userId, token } = msg;

        // Send to backend
        axios
          .post(
            "https://sabrinachat.mylovelyserver.fun/api/Messages",
            {
              conversationId: chatId,
              content,
            },
            {
              headers: { Authorization: `Bearer ${token}` },
            },
          )
          .catch((err) => {
            console.error(
              "Error sending message to API:",
              err.response?.data || err.message,
            );
          });

        // Broadcast to all subscribers
        const payload = {
          type: "new_message",
          chatId,
          content,
          userId,
          timestamp: Date.now(),
        };

        const recipients = clientsByChat.get(chatId);
        if (recipients) {
          for (const client of recipients) {
            if (client.readyState === WebSocket.OPEN) {
              client.send(JSON.stringify(payload));
            }
          }
        }
      }
    } catch (e) {
      console.error("Invalid message received:", e);
    }
  });

  ws.on("close", () => {
    for (const [chatId, clients] of clientsByChat) {
      clients.delete(ws);
      if (clients.size === 0) clientsByChat.delete(chatId);
    }
  });
});

console.log("WebSocket server running on ws://localhost:8080");
