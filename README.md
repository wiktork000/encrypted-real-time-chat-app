# Encrypted Real-Time Chat App

A full-stack real-time chat application with JWT authentication, WebSocket-based messaging and hybrid client-side message encryption.

Messages are encrypted in the browser using AES-GCM. Each conversation uses a symmetric key that is separately wrapped for every participant using RSA-OAEP with SHA-256. Users' private RSA keys are stored as password-encrypted PKCS#8 data and decrypted by the React client when accessing a conversation.

The system combines an ASP.NET Core REST API, React frontend, Node.js WebSocket service, PostgreSQL database and Docker Compose environment.

The project was developed as an academic team project.

## Features

* User registration and login with JWT-based authentication.
* Conversation creation and participant management.
* Real-time message delivery using a separate WebSocket service.
* REST API for users, conversations, messages and cryptographic keys.
* Client-side AES-GCM encryption of message content.
* RSA-OAEP encryption of conversation keys for individual participants.
* Password-encrypted PKCS#8 private RSA keys.
* PostgreSQL database managed through Entity Framework Core migrations.
* Trigger-backed unread-message counters.
* Docker Compose setup for running the backend, frontend, WebSocket service and database together.

## Encryption Model

The application uses hybrid encryption:

1. Each user has an RSA key pair.
2. The public key and password-encrypted private key are stored by the backend.
3. Each conversation uses a symmetric AES key.
4. The conversation key is encrypted separately for every participant using their RSA public key and RSA-OAEP with SHA-256.
5. The React client decrypts the user's private key using their password.
6. The private RSA key is then used to decrypt the conversation AES key.
7. Message content is encrypted and decrypted in the browser using AES-GCM.

The backend stores encrypted message content and RSA-wrapped conversation keys. Plaintext message content is produced by the client after decryption.

## Real-Time Message Flow

When a user sends a message:

1. The React client encrypts the message using AES-GCM.
2. The encrypted content is sent to the Node.js WebSocket service.
3. The WebSocket service forwards the encrypted payload to the ASP.NET Core REST API for persistence.
4. The service broadcasts a new-message event to clients subscribed to the conversation.
5. Receiving clients refresh the latest messages through the REST API.
6. Retrieved message content is decrypted locally in the browser.

If the WebSocket connection is unavailable, the frontend sends the same encrypted payload directly through the REST API.

## Architecture

| Component              | Role                                                                                   | Technology                          |
| ---------------------- | -------------------------------------------------------------------------------------- | ----------------------------------- |
| `SabrinaChatAPI`       | Main backend API for authentication, users, conversations, messages and key management | ASP.NET Core, Entity Framework Core |
| `SabrinaChatFront`     | Web client, message encryption and chat interface                                      | React, Vite, Web Crypto API         |
| `SabrinaChatWebSocket` | Real-time encrypted-message relay and notification service                             | Node.js, WebSocket                  |
| PostgreSQL             | Persistent storage for users, conversations, encrypted messages and key data           | PostgreSQL                          |
| Docker Compose         | Local multi-service environment                                                        | Docker                              |

## Database Design

The PostgreSQL database is managed through Entity Framework Core migrations and includes:

* separate user profile and credential entities,
* conversations with a many-to-many participant relationship,
* messages assigned to conversations and authors,
* per-user encrypted conversation keys,
* key-validity ranges connected to message history,
* unique and composite indexes,
* cascade and restricted deletion rules,
* per-participant unread-message counters,
* a database trigger responsible for updating unread-message counts.

The main database entities are:

* `User`,
* `UserCredentials`,
* `Conversation`,
* `ConversationParticipant`,
* `Message`,
* `Key`.

## My Role and Contributions

I worked on the integration between the React frontend and the ASP.NET Core backend, covering authentication, conversations and message flows.

My contributions included:

* integrating frontend API calls with ASP.NET Core REST endpoints,
* working with request and response DTOs for authentication, conversations and messages,
* implementing the frontend JWT authentication flow and protected API requests,
* connecting chat-room components with the Node.js WebSocket service,
* working across controller, service and Entity Framework Core layers,
* testing login, conversation access and real-time message exchange across the application.

## Tech Stack

**Backend:** ASP.NET Core, C#, Entity Framework Core  
**Frontend:** React, Vite, JavaScript  
**Real-time service:** Node.js, WebSocket  
**Database:** PostgreSQL, EF Core migrations  
**Infrastructure:** Docker, Docker Compose  
**Authentication:** JWT  
**Encryption:** Web Crypto API, AES-GCM, RSA-OAEP with SHA-256, encrypted PKCS#8, node-forge  
**Other:** REST API, DTOs, database triggers  

## Getting Started

### Prerequisites

* Docker and Docker Compose
* .NET SDK
* Node.js and npm
* PostgreSQL, if running without Docker

### Run with Docker Compose

```bash
docker compose up --build
