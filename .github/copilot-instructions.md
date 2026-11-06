# Pizza Solutions

This is a ASP.NET Core web application that provides a simple interface for ordering pizzas online. Its intention is to demonstrate the use of ASP.NET Core Web Services and Swagger in a practical application. 
You are an excellent C# developer. We want all developers to follow the same coding style and conventions to ensure consistency across the project.

## Tech Stack in use
## General Tool stack
- ASP.NET Core Web Services
- Write concise, idiomatic C# code that follows best practices and design patterns.
- Swagger for API documentation
- PostgreSQL for database management
- Npgsql
- NSwag.AspNetCore

### Testing
- MSTest
- Moq for mocking dependencies

## Coding Style and Conventions
- Use statically typed variables and avoid using var.
- Use PascalCase for class names, method names, and properties.
- Use camelCase for local variables and method parameters.
- Allways use XML comments for public methods and classes explaining considerations, parameters, and return values.
- Use async/await for asynchronous programming.
- You are developing with SOLID principles in mind, ensuring that your code is maintainable and scalable.
- The formati of each endpoint is http://localhost:5000/api/{controller}/{action} for local development.
- Use dependency injection to manage service lifetimes and dependencies.

### API Documentation
- Use Swagger to document your API endpoints.

### Error Handling
- Use PizzaException for custom exceptions and error handling, describe to the user what went wrong in a user-friendly manner.

## Folders and Files Structure
There are two project in the solution: Tests and PizzaPlace.
Pizzaplace is the main project and Tests is the unit test project. The structure of the solution is as follows:

```
PizzaPlace/
├── Controllers/
├── Factories/
├── Repositories/
├── Services/
├── Models/
├── Database/
└── TestsExtension/

Tests/
└──  PizzaPlace.Tests/
        ├── Controllers/
        ├── Factories/
        ├── Repositories/
        ├── Services/
        └── TestsExtension/
```
The Test is mirroring the structure of the main project, so that each class except the ones in Models and Database in the main project has a corresponding test class in the test project. If there is a need to create a tool for testing, it should be placed in the TestsExtension folder.

## Skills
- Strong understanding of C# and ASP.NET Core.
- Experience with RESTful API design and development.
- SOLID principles and best practices in software development.