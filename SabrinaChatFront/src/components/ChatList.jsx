import { useEffect, useState } from "react";
import { format } from "date-fns";
import {
  createConversation,
  deleteConversation,
  getConversations,
} from "../api/conversations";
import { getAllUsers } from "../api/users";

export default function ChatList({ refreshTrigger, user, onSelectChat }) {
  const [allUsers, setAllUsers] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [chatHistory, setChatHistory] = useState([]);

  const fetchUsers = async () => {
    try {
      const { data } = await getAllUsers();
      const filteredUsers = data.filter((u) => u.id !== user.id);
      setAllUsers(filteredUsers);
      return filteredUsers;
    } catch (err) {
      console.log(err);
    }
    return [];
  };

  const fetchChatHistory = async (users) => {
    try {
      const currentUserId = parseInt(localStorage.getItem("id"), 10);
      const { data } = await getConversations();

      if (data) {
        const modified = data
          .filter((conv) => conv.participants.length > 1)
          .map((conv) => {
            const other = conv.participants.find((p) => p.id !== currentUserId);
            const cleanedName = conv.name
              .replace(
                new RegExp(`\\b${localStorage.getItem("name")}\\b`, "g"),
                "",
              )
              .trim();

            return {
              ...conv,
              name: cleanedName,
              participant: other,
            };
          });

        const sorted = modified.sort(
          (a, b) => new Date(b.createdAt) - new Date(a.createdAt),
        );

        setChatHistory(sorted);
      }
    } catch (err) {
      console.log(err);
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      const users = await fetchUsers();
      await fetchChatHistory(users);
    };
    fetchData();
  }, [user.id, refreshTrigger]);

  const softDeleteChat = async (chatId) => {
    const confirm = window.confirm(
      "Are you sure you want to delete this chat?",
    );
    if (!confirm) return;

    try {
      await deleteConversation(chatId);
      setChatHistory((prev) => prev.filter((chat) => chat.id !== chatId));
    } catch (err) {
      console.log(err);
    }
  };

  const filteredUsers = allUsers.filter((u) =>
    (u.name || u.email || "").toLowerCase().includes(searchQuery.toLowerCase()),
  );

  const startChat = async (u) => {
    try {
      const { data } = await createConversation(user.name + " " + u.name, [
        user.id,
        u.id,
      ]);
      onSelectChat(data.id, {
        id: data.participants[1].id,
        ...data.participants[1].name,
      });
    } catch (err) {
      console.log(err);
    }
  };

  return (
    <div className="flex flex-col h-full bg-[#fff0f6] font-sans">
      <div className="flex-shrink-0 p-3 sm:p-4">
        <input
          type="text"
          className="border border-[#ff80c9] bg-[#fff5fa] placeholder-[#ff80c9] text-[#d63384] rounded-full px-4 py-2 w-full focus:outline-none focus:ring-2 focus:ring-[#ff5ca2]"
          placeholder="Search your sweethearts…"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
        />
      </div>

      <div className="flex-1 overflow-y-auto px-3 sm:px-4 pb-4">
        {searchQuery ? (
          <>
            <h2 className="text-lg font-bold mb-3 text-[#9b1859]">
              Search Results
            </h2>
            {filteredUsers.length === 0 && (
              <p className="text-[#ff5ca2] italic">No users found 💔</p>
            )}
            <ul className="space-y-3">
              {filteredUsers.map((u) => (
                <li key={u.id}>
                  <button
                    onClick={() => startChat(u)}
                    className="w-full text-left p-3 bg-[#fff0c4] text-yellow-800 rounded-xl hover:bg-[#ffeaa7] transition"
                  >
                    {u.name || u.email}
                  </button>
                </li>
              ))}
            </ul>
          </>
        ) : (
          <>
            <h2 className="text-lg font-bold mb-3 text-[#9b1859]">
              Recent Chats
            </h2>
            {chatHistory.length === 0 && (
              <p className="text-[#ff5ca2] italic">No recent chats yet…</p>
            )}
            <ul className="space-y-3">
              {chatHistory.map((chat) => (
                <li key={chat.id}>
                  <div className="w-full flex justify-between items-center p-3 bg-[#ffe0f0] text-[#9b1859] rounded-xl hover:bg-[#ffc4dc] transition shadow-sm">
                    <div
                      onClick={() =>
                        onSelectChat(chat.id, {
                          id: chat.participant.id,
                          name: chat.participant.name,
                        })
                      }
                      className="flex-1 text-left cursor-pointer"
                    >
                      <div>{chat.name}</div>
                      <div className="text-xs text-[#c16a95] mt-1">
                        {chat.createdAt
                          ? format(new Date(chat.createdAt), "d MMM HH:mm")
                          : ""}
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      {chat.numberOfUnreadMessages > 0 && (
                        <span className="text-sm bg-red-500 text-white rounded-full px-2 py-0.5 shadow">
                          {chat.numberOfUnreadMessages}
                        </span>
                      )}
                      <button
                        onClick={() => softDeleteChat(chat.id)}
                        className="px-3 py-2 text-base text-red-600 hover:text-red-800"
                      >
                        ✕
                      </button>
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          </>
        )}
      </div>
    </div>
  );
}
