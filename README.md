# Encrypted Real-Time Chat App

A full-stack real-time chat application with user authentication, conversation management, WebSocket-based messaging and client-side encryption support.

The project was developed as an academic team project and demonstrates integration between an ASP.NET Core backend, a React frontend, a Node.js WebSocket service and a PostgreSQL database.

## Features

* User registration and login with JWT-based authentication.
* Conversation creation and participant management.
* Real-time message delivery using a separate WebSocket service.
* REST API for users, conversations, messages and cryptographic keys.
* Client-side encryption utilities for securing message content.
* PostgreSQL database with Entity Framework Core migrations.
* Docker Compose setup for running the backend, frontend, WebSocket service and database together.

## Architecture

| Component              | Role                                                                         | Technology                          |
| ---------------------- | ---------------------------------------------------------------------------- | ----------------------------------- |
| `SabrinaChatAPI`       | Main backend API for authentication, users, conversations, messages and keys | ASP.NET Core, Entity Framework Core |
| `SabrinaChatFront`     | Web client for login, chat list and chat room views                          | React, Vite                         |
| `SabrinaChatWebSocket` | Real-time communication service                                              | Node.js, WebSocket                  |
| PostgreSQL             | Persistent storage for users, conversations, messages and key data           | PostgreSQL                          |
| Docker Compose         | Local multi-service environment                                              | Docker                              |


## My Role and Contributions

I contributed to selected full-stack parts of the project, with a focus on integration between the frontend, backend API and real-time communication layer.

My work included:

* working with the ASP.NET Core REST API structure,
* using DTOs and service classes for backend request handling,
* supporting frontend-backend communication for authentication and chat-related flows,
* working with React components responsible for chat list and chat room views,
* integrating API calls from the frontend with backend endpoints,
* helping test user flows such as login, conversation access and message exchange,
* working with the project setup based on Docker Compose and PostgreSQL.

The project helped me practice backend API design, frontend integration, real-time communication concepts and database-backed application development.

## Tech Stack

**Backend:** ASP.NET Core, C#, Entity Framework Core
**Frontend:** React, Vite, JavaScript
**Real-time service:** Node.js, WebSocket
**Database:** PostgreSQL
**Infrastructure:** Docker, Docker Compose
**Authentication:** JWT
**Other:** REST API, DTOs, EF Core migrations, client-side encryption utilities

## Getting Started

### Prerequisites

* Docker and Docker Compose
* .NET SDK
* Node.js and npm
* PostgreSQL, if running without Docker

### Run with Docker Compose

```bash
docker compose up --build
```

### Run frontend locally

```bash
cd SabrinaChatFront
npm install
npm run dev
```

### Run WebSocket service locally

```bash
cd SabrinaChatWebSocket
npm install
node server.js
```

### Run backend locally

```bash
cd SabrinaChatAPI
dotnet restore
dotnet run
```
