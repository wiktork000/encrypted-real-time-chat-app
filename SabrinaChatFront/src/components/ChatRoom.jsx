import { useEffect, useState, useRef } from "react";
import { format } from "date-fns";
import { getMessages, removeMessage, sendMessage } from "../api/messages";
import { getConversationKey } from "../api/keys";
import {
  decryptRSA,
  encryptAESGCM,
  decryptAESGCM,
  importAESKeyFromBuffer,
  decryptEncryptedPrivateKey,
} from "../tools/encryption";
import { useCallback } from "react";

export default function ChatRoom({ user, chatId, chatPartner, onBack }) {
  const [messages, setMessages] = useState([]);
  const [newMsg, setNewMsg] = useState("");
  const [aesKey, setAesKey] = useState(null);
  const scrollRef = useRef(null);
  const textareaRef = useRef(null);
  const wsRef = useRef(null);
  const messageCountRef = useRef(-1);

  const fetchMessages = useCallback(
    async (limit = 50, lazy = false, currentlen = 0) => {
      if (!aesKey) return;
      const container = scrollRef.current;
      const previousHeight = container.scrollHeight;
      try {
        let offset = lazy ? currentlen : 0;
        const { data } = await getMessages(chatId, limit, offset);
        const decrypted = await Promise.all(
          data.map(async (m) => {
            try {
              const plain = await decryptAESGCM(m.content, aesKey);
              return { ...m, content: plain };
            } catch {
              return { ...m, content: "[failed to decrypt]" };
            }
          }),
        );
        const ok = decrypted.filter((i) => i.content !== "[failed to decrypt]");

        setMessages((prev) => {
          const combined = [...prev, ...ok];

          const unique = Array.from(
            new Map(combined.map((m) => [m.id, m])).values(),
          );

          unique.sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp));
          messageCountRef.current = unique.length;
          return unique;
        });

        console.log(messages);
        setTimeout(() => {
          if (lazy) {
            const newHeight = container.scrollHeight;
            container.scrollTop = newHeight - previousHeight;
          } else {
            container.scrollTo({
              top: container.scrollHeight,
              behavior: "smooth",
            });
          }
        }, 100);
      } catch (e) {
        console.error("fetchMessages ", e);
      }
    },
    [aesKey, chatId],
  );

  useEffect(() => {
    const container = scrollRef.current;
    if (!container) return;

    const handleScroll = () => {
      if (container.scrollTop === 0) {
        console.log("Scrolled to top — fetching more...");
        fetchMessages(50, true, messageCountRef.current);
      }
    };

    container.addEventListener("scroll", handleScroll);

    return () => container.removeEventListener("scroll", handleScroll);
  }, [fetchMessages]);

  useEffect(() => {
    if (aesKey) {
      fetchMessages();

      setTimeout(() => {
        textareaRef.current?.focus();
      }, 150);
    }
  }, [aesKey, fetchMessages]);

  useEffect(() => {
    const pem = localStorage.getItem("private_key");
    const password = localStorage.getItem("password");
    if (!pem || !password) return;

    (async () => {
      try {
        /* ‣ decrypt user’s RSA private key */
        const rsaKey = await decryptEncryptedPrivateKey(
          pem.replace(/\\n/g, "\n"),
          password,
        );

        const { data } = await getConversationKey(chatId);
        const aesBuf = await decryptRSA(data.keyValue, rsaKey);
        const symKey = await importAESKeyFromBuffer(aesBuf);
        setAesKey(symKey);
      } catch (e) {
        console.error("key-setup →", e);
      }
    })();
  }, [chatId]);

  useEffect(() => {
    if (!aesKey) return;

    const ws = new WebSocket("wss://sabrinachat.mylovelyserver.fun/ws/");
    wsRef.current = ws;

    ws.onopen = () => {
      ws.send(JSON.stringify({ type: "subscribe", chatId }));
    };

    ws.onmessage = async (event) => {
      try {
        setTimeout(() => {
          fetchMessages(5);
        }, 200);
      } catch (e) {
        console.error("WebSocket msg error:", e);
      }
    };

    ws.onerror = (err) => console.error("WebSocket error:", err);
    ws.onclose = () => console.log("WebSocket closed");

    return () => ws.close();
  }, [aesKey, chatId]);

  const handleSend = async () => {
    const text = newMsg.trim();
    if (!text) return;
    const content = await encryptAESGCM(text, aesKey);
    try {
      if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
        wsRef.current.send(
          JSON.stringify({
            type: "message",
            chatId,
            content,
            userId: user.id,
            token: localStorage.getItem("token"),
          }),
        );
      } else {
        console.warn("WebSocket not connected, fallback to HTTP");
        await sendMessage(content, chatId);
      }
      setNewMsg("");
      textareaRef.current?.focus();
    } catch (err) {
      console.log(err);
    }
  };

  const deleteMessage = async (id) => {
    if (window.confirm("Delete this message?")) {
      try {
        await removeMessage(id);
        setMessages((prev) => prev.filter((m) => m.id !== id));
      } catch (err) {
        console.log(err);
      }
    }
  };

  const handleInput = (e) => {
    textareaRef.current.style.height = "auto";
    textareaRef.current.style.height = textareaRef.current.scrollHeight + "px";
    setNewMsg(e.target.value);
  };

  const onKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="flex flex-col h-dvh bg-[#fff0f6] font-sans">
      <div className="flex-shrink-0 p-3 sm:p-4 bg-[#ffb6d9] border-b border-[#ff80c9] shadow-md flex justify-between items-center">
        <h2 className="text-lg sm:text-xl font-bold text-[#9b1859] truncate mr-2">
          {chatPartner.name}
        </h2>
        <button
          onClick={onBack}
          className="text-white bg-[#ff5ca2] px-3 py-1 rounded-full hover:bg-[#ff3d94] transition flex-shrink-0"
        >
          ← Back
        </button>
      </div>

      <div
        ref={scrollRef}
        className="flex-1 overflow-y-auto px-4 py-3 space-y-2 bg-[#fff0f6] min-h-0"
      >
        {messages.map((msg) => (
          <div
            key={msg.id}
            className={`relative max-w-xs px-4 py-2 rounded-2xl shadow-md text-sm whitespace-pre-wrap ${
              msg.author.id === user.id
                ? "bg-[#ff5ca2] text-white self-end ml-auto"
                : "bg-[#fff5fa] text-[#9b1859] self-start"
            }`}
          >
            <div>{msg.content}</div>
            <div className="text-xs mt-1 text-right opacity-70">
              {msg.timestamp
                ? format(new Date(msg.timestamp), "d MMM HH:mm")
                : ""}
            </div>
            {msg.author.id === user.id && (
              <button
                onClick={() => deleteMessage(msg.id)}
                className="absolute top-1 right-2 px-3 py-2 text-base text-white/70 hover:text-white/90"
              >
                ✕
              </button>
            )}
          </div>
        ))}
      </div>

      <form
        onSubmit={(e) => {
          e.preventDefault();
          handleSend();
        }}
        className="flex-shrink-0 p-3 sm:p-4 bg-white border-t border-[#ff80c9] flex gap-2 items-end"
      >
        <textarea
          ref={textareaRef}
          className="flex-1 border border-[#ff80c9] bg-[#fff5fa] text-[#d63384] rounded-full px-4 py-2 placeholder-[#ff80c9] focus:outline-none resize-none overflow-hidden min-h-[40px] max-h-[120px]"
          value={newMsg}
          onChange={handleInput}
          onKeyDown={onKeyDown}
          placeholder="Type something sweet..."
          rows={1}
        />
        <button
          type="submit"
          className="bg-[#ff5ca2] hover:bg-[#ff3d94] text-white px-4 sm:px-5 py-2 rounded-full font-semibold transition flex-shrink-0"
        >
          Send
        </button>
      </form>
    </div>
  );
}
