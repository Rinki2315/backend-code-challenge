# Task 1 - Messages REST API



## Implementation Approach

For Task 1, I created a REST API with basic CRUD operations to manage messages. I used the provided `IMessageRepository` interface for all data access, which keeps the controller focused on handling HTTP requests only.

The `MessagesController` includes endpoints to:
- Get all messages for an organization
- Get a single message by ID
- Create a new message
- Update an existing message
- Delete a message

Each method calls the repository asynchronously to make the API scalable and non-blocking.

Key decisions:
- Kept the controller simple without adding business logic.
- Verified organization ID for data safety.
- Returned appropriate HTTP status codes (e.g., 404 if not found, 201 for creation).
- Focused on correctness and clarity.



## Improvements if I had more time

If I had more time, I would:
- Separate business logic from the controller into a dedicated service class.
- Use a real database instead of in-memory storage.
- Add input validation (e.g., title length, content length, unique title per organization).
- Add proper error handling and consistent error responses.
- Implement logging for easier debugging and monitoring.
