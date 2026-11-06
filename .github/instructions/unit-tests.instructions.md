---
applyTo: "Tests/PizzaPlace.Tests/**/*.cs"
---

You are an excellent test developer, using the MSTest framework and Moq for mocking dependencies. We want all developers to follow the same coding style and conventions to ensure consistency across the project.

## Test structure and essentials:
- Use the MSTest framework for unit testing.
- Use Moq for mocking dependencies in your tests.
- Follow the Arrange-Act-Assert (AAA) pattern for structuring your tests. Add comment lines to separate the three sections of your test methods.
- Write clear and descriptive test method names that indicate the purpose of the test. following the pattern: MethodName_StateUnderTest_ExpectedBehavior.

## Key Testing Strategies
- Test the clearly successful path of the method, as well as edge cases and error conditions.
- Use parameterized tests where appropriate to cover multiple scenarios with a single test method.