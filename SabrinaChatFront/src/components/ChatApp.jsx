import { useState } from "react";
import ChatList from "./ChatList";
import ChatRoom from "./ChatRoom";

export default function ChatApp({ user, setUser }) {
  const [chatId, setChat] = useState(null);
  const [partner, setPartner] = useState(null);
  const [chatListKey, setChatListKey] = useState(0);

  const handleBack = () => {
    setChat(null);
    setChatListKey((prev) => prev + 1); // trigger refresh
  };

  const signOut = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("id");
    localStorage.removeItem("name");
    localStorage.removeItem("public_key");
    localStorage.removeItem("private_key");
    localStorage.removeItem("password");

    setUser(null);
  };

  return chatId ? (
    <ChatRoom
      user={user}
      chatId={chatId}
      chatPartner={partner}
      onBack={handleBack}
    />
  ) : (
    <div className="h-dvh bg-pink-50 flex flex-col">
      <header className="flex-shrink-0 flex justify-between items-center p-3 sm:p-4 bg-pink-100 border-b border-pink-300 shadow-md">
        <h1 className="text-xl sm:text-2xl font-bold text-pink-700">
          Short n' Sweet 💌
        </h1>
        <button
          onClick={() =>
            confirm("Do you really want to log out?") ? signOut() : ""
          }
          className="text-white bg-pink-500 px-3 sm:px-4 py-1 rounded-full hover:bg-pink-600 flex-shrink-0"
        >
          ✕
        </button>
      </header>
      <div className="flex-1 min-h-0">
        <ChatList
          user={user}
          refreshTrigger={chatListKey}
          onSelectChat={(id, other) => {
            setChat(id);
            setPartner(other);
          }}
        />
      </div>
    </div>
  );
}
