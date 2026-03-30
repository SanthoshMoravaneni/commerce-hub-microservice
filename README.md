# Commerce Hub Microservice

This project is a backend microservice that simulates a simplified commerce system where users can place orders, inventory is managed safely, and events are emitted for downstream processing.

The goal of this system is not just to implement APIs, but to demonstrate **real-world backend concerns** such as atomic operations, consistency, event-driven communication, and clean architecture design.

---

# What this system does

At a high level, the service handles the full lifecycle of an order:

* A user places an order
* The system checks inventory
* Stock is safely updated
* The order is stored
* An event is published so other systems can react

This mirrors how modern e-commerce systems are built — **decoupled, reliable, and scalable**.

---

# Architecture Overview

The project is structured using a **Clean Architecture approach**, which separates responsibilities clearly and keeps the system maintainable.

### Domain Layer

This is the core of the system.

* Defines entities like `Order`, `Product`
* Contains enums such as order status
* Defines events like `OrderCreated`

This layer has **no external dependencies** and represents pure business concepts.

---

### Application Layer

This is where all the business logic lives.

* Handles order checkout logic
* Validates input (e.g., no negative quantities)
* Coordinates between repositories and messaging
* Enforces business rules (like blocking shipped updates)

This layer defines interfaces (repositories, event publishers) so it stays independent from infrastructure.

---

### Infrastructure Layer

This layer connects the application to the outside world.

* MongoDB integration for data storage
* RabbitMQ implementation for event publishing

It implements the interfaces defined in the Application layer.

---

### API Layer

This is the entry point to the system.

* Exposes REST endpoints
* Maps HTTP requests to application services
* Handles dependency injection and configuration

---

### Tests

Unit tests validate the critical parts of the system:

* Validation rules
* Stock decrement logic
* Event publishing behavior

---

# How the system works (End-to-End Flow)

Let’s walk through what happens when a user places an order:

### Step 1: Checkout Request

A client calls:

POST /api/orders/checkout

with a list of products and quantities.

---

### Step 2: Validation

The system ensures:

* Quantities are positive
* Products exist in the database

If any validation fails, the process stops immediately.

---

### Step 3: Inventory Check & Atomic Update

For each product:

* The system verifies available stock
* It performs an **atomic decrement operation in MongoDB**

This is critical because it prevents:

* Race conditions
* Overselling during concurrent requests

---

### Step 4: Order Creation

If all stock operations succeed:

* The order is created
* Total price is calculated
* Order is stored in MongoDB

---

### Step 5: Event Publishing

After successful creation:

* An `OrderCreated` event is published to RabbitMQ

This allows other services (like shipping, notifications, or payments) to react asynchronously.

---

### Step 6: Response

The API returns the created order with:

* Order ID
* Items
* Total amount
* Status
* Timestamps

---

#Example Flow You Can Test

1. Seed products into MongoDB
2. Call checkout endpoint
3. Verify:

   * Stock is reduced
   * Order is created
   * Event appears in RabbitMQ queue

---

# API Endpoints

### Checkout Order

POST /api/orders/checkout

Processes a new order and triggers the full workflow.

---

### Get Order

GET /api/orders/{id}

Returns order details or 404 if not found.

---

### Replace Order

PUT /api/orders/{id}

Replaces an existing order completely.
Blocked if the order is already shipped.

---

### Update Stock

PATCH /api/products/{id}/stock

Adjusts inventory safely using atomic operations.

---

# Running the Project

Run everything with a single command:

```bash
docker compose up --build
```

This will start:

* API service
* MongoDB
* RabbitMQ

---

# Access Points

### Swagger UI (locally)

http://localhost:8080/swagger

### RabbitMQ Dashboard (locally)

http://localhost:15672 
Username: guest

Password: guest

---

# Verifying Event Flow

After placing orders:

* Open RabbitMQ UI
* Navigate to **Queues**
* Open `order-created`

You should see messages corresponding to successful orders.

---

# Important Design Decisions

### Atomic Inventory Updates

MongoDB operations are designed to prevent stock from going below zero, ensuring consistency under concurrent load.

---

### Event-Driven Architecture

Publishing events allows the system to scale by decoupling responsibilities (e.g., shipping, notifications).

---

### Idempotent Updates

PUT endpoint ensures consistent results even if the same request is repeated.

---

# Assumptions

* Product IDs are string-based
* Orders begin in a pending state
* Inventory is modified only through controlled endpoints

---

# Notes

* Default RabbitMQ credentials are used for simplicity in local development
* MongoDB uses `_id` as the primary identifier for products
* In a production system, an **outbox pattern** would be used for guaranteed message delivery

---

# Conclusion

This project demonstrates a complete backend flow combining:

* API design
* Database consistency
* Event-driven messaging
* Clean architecture
* Testable business logic

It reflects how modern backend systems are structured and implemented in real-world applications.
