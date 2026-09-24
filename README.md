# Shared Expense Tracker

A collaborative expense tracking application for families, roommates, or any group of people who need to manage shared costs together.

Users can create or join groups, maintain multiple expense lists, add expenses, mark them as paid or unpaid, and view real-time unpaid summaries — all with a clean, responsive UI.

---

## Overview

- Multiple people can share expense lists within a group.
- When a group is created, the **group creator defines the first (default/primary) list**. The name and purpose are fully dynamic (e.g. “Monthly Bills”, “Rent & Utilities”, “Household”).
- Members can create additional **custom lists** (shopping, utilities, necessities, etc.).
- Inside any list, members can add expenses and mark them as paid or unpaid.
- All changes are visible to group members in real time.
- Users can view a clear summary of unpaid expenses.

---

## Core Features

| Feature | Description |
|---------|-------------|
| **Authentication** | Register and login (phone number + password). Protected routes via JWT. |
| **Groups** | Create a group, list your groups, view group details & members, join via share code. |
| **Expense Lists** | Primary list created by group creator + unlimited custom lists. Update name, delete custom lists (primary list cannot be deleted). |
| **Expenses** | Add expenses to a list, mark paid / unpaid, view all expenses in a list. |
| **Unpaid Summary** | See unpaid totals and counts. Live updates via SignalR. |
| **Real-time** | Unpaid count badge and summaries update live for all group members. |

---

## Application Flow

1. **Register / Login**
2. Land on the **Group Dashboard**
3. Either:
   - **Create a new group** → enter group name → enter Group View → create the first (default) list → manage lists & expenses
   - **Join an existing group** using a share code
4. Inside a group:
   - Create additional custom lists
   - Open any list
5. Inside a list:
   - Add expenses
   - Mark expenses paid / unpaid
   - View unpaid total / summary
6. Changes are reflected in real time for every group member

---

## Technology Stack

| Layer | Technology | Purpose |
|-------|------------|---------|
| Framework | ASP.NET Core Web MVC | Server-rendered views + controllers |
| Styling | Tailwind CSS | Responsive, utility-first UI |
| HTTP Client | Axios | All API calls (CRUD, auth, error handling) |
| Real-time | SignalR | Live unpaid-expense count updates |
| Charts | ApexCharts | Unpaid expense summary visualization |

---

## Prerequisites

- .NET 8 SDK (or later)
- Node.js (for Tailwind CSS build, if using the Tailwind CLI)
- A running backend API that implements the endpoints listed below

---

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd SharedExpenseTracker
```

### 2. Restore & run

```bash
dotnet restore
dotnet run
```

The application will be available at `https://localhost:5xxx` (or the port shown in the console).

### 3. Configure the API base URL

Update the Axios base URL in `wwwroot/js/api.js` to point to your backend:

```js
const api = axios.create({
  baseURL: 'https://your-api-url.com',
  // ...
});
```

### 4. Tailwind CSS (if not already set up)

```bash
npm install -D tailwindcss
npx tailwindcss -i ./wwwroot/css/site.css -o ./wwwroot/css/output.css --watch
```

---

## Project Structure

```
/
├── Controllers
│   ├── AuthController.cs
│   ├── GroupsController.cs
│   ├── ListsController.cs
│   └── ExpensesController.cs
├── Views
│   ├── Shared
│   │   ├── _Layout.cshtml          # Tailwind + SignalR + Axios + ApexCharts
│   │   └── _Nav.cshtml
│   ├── Auth
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Groups
│   │   ├── Index.cshtml            # Group Dashboard
│   │   ├── Create.cshtml
│   │   ├── Details.cshtml          # Group View
│   │   └── Join.cshtml
│   ├── Lists
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Details.cshtml          # Single list + expenses
│   └── Expenses
│       ├── Create.cshtml
│       └── Summary.cshtml          # Unpaid summary + ApexCharts
└── wwwroot
    ├── js
    │   ├── api.js                  # Axios wrappers
    │   ├── signalr-client.js       # Hub connection + unpaid count handlers
    │   └── charts.js               # ApexCharts helpers
    └── css
        └── site.css                # Tailwind entry / custom utilities
```

---

## API Endpoints (Expected Backend)

### Auth

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/auth/register` | Register new user | No |
| POST | `/auth/login` | Login | No |
| GET | `/auth/me` | Get current user profile | Yes |

### Groups

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/groups` | Create a new group (no auto list) | Yes |
| GET | `/groups` | List all groups the current user belongs to | Yes |
| GET | `/groups/:groupId` | Get group details + members | Yes |
| POST | `/groups/join` | Join a group using share code | Yes |
| GET | `/groups/:groupId/members` | List members of a group | Yes |

### Expense Lists

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/groups/:groupId/lists` | Get all lists in a group | Yes |
| POST | `/groups/:groupId/lists` | Create a list (first becomes primary) | Yes |
| GET | `/lists/:listId` | Get a single list + its expenses | Yes |
| PUT | `/lists/:listId` | Update list name | Yes |
| DELETE | `/lists/:listId` | Delete a custom list (primary cannot be deleted) | Yes |

> Expense endpoints (create, toggle paid/unpaid, summary) should be implemented under an Expense resource according to the product requirements.

---

## Data Concepts

| Entity | Description |
|--------|-------------|
| **UserData** | User accounts and profiles |
| **Group Data** | Groups, members, share codes |
| **List Data** | Expense lists belonging to groups (first list = primary/default) |
| **Expense Data** | Individual expenses (amount, paid/unpaid status, etc.) |

---
