# Task 2 - MessageLogic and Validation


## How did you approach the validation requirements and why

For Task 2, I created a separate MessageLogic class to handle all business rules and validations. This keeps the controller simple and focused only on handling HTTP requests and responses.

The validation rules implemented are:

 -Title is required and must be between 3 and 200 characters.

 -Content length must be between 10 and 1000 characters.

 -Title must be unique within the same organization to avoid duplicates.

 -Updates and deletes can only be performed on active messages (IsActive = true).

 -UpdatedAt is set automatically when a message is updated.

This approach makes the code easier to maintain, test, and extend, as all business rules live in one place rather than being scattered across the controller.



## What changes would you make to this implementation for a production environment

- Use a real database instead of in-memory storage.  
- Add logging for all operations.  
- Handle exceptions and errors more gracefully.  
- Possibly add authentication/authorization so only allowed users can create/update/delete messages.  
- Add caching or pagination for large data sets.
