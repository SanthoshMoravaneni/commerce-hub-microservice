# Developer Log

## Approach

I approached this assignment as a real-world backend system rather than just an API implementation. My focus was on ensuring correctness, consistency, and maintainability.

AI tools were used as an accelerator for scaffolding and generating initial patterns, but all critical logic and design decisions were reviewed and refined manually.

---

## AI Strategy

I used AI to:

* Generate initial project structure and layering
* Suggest repository and service patterns
* Help outline unit test scenarios
* Identify potential edge cases

I provided clear constraints to the AI such as:

* atomic MongoDB operations
* event publishing only on success
* strict validation rules

---

## Human Corrections & Improvements

### 1. MongoDB Identifier Mapping

AI initially assumed a standard `Id` field, but MongoDB uses `_id`.
I corrected this to ensure product lookup works correctly.

---

### 2. Atomic Stock Handling

An early approach used separate read and update operations.
I replaced it with atomic update logic to prevent race conditions and overselling.

---

### 3. Event Publishing Timing

AI suggested publishing events during processing.
I ensured events are emitted **only after successful order creation**, preventing inconsistent states.

---

### 4. Business Rule Enforcement

I added explicit checks to prevent updates on shipped orders, ensuring business rules are enforced strictly.

---

## Verification Strategy

I validated the system through:

### Unit Tests

* Negative quantity validation
* Insufficient stock handling
* Correct stock decrement logic
* Event publishing verification
* No event emission on failure

### Manual Testing

* Swagger API testing
* MongoDB inspection
* RabbitMQ queue verification

---

## Key Learnings

* Atomic operations are essential for real-world concurrency safety
* Event-driven systems require careful control of when events are emitted
* Clean architecture improves clarity and testability
* AI is most effective when guided with clear constraints

---

## Final Thoughts

This project reflects a balance between speed (using AI tools) and correctness (manual validation and refinement). The result is a system that aligns with real-world backend design principles and demonstrates both technical and architectural understanding.
