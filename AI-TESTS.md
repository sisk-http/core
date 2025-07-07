# AI Tests file

Sisk uses generative artificial intelligence to write tests for the framework and maintain a quality trace for each update. This file is directed to the LLM model itself to organize which tests it should write and the rules it should follow.

## Rules

- Write tests for the latest .NET language, in this case, .NET 9.
- Use MSTest as the testing platform.
- Follow the same test format adopted in the test files in "tests/Tests".
- Under no circumstances alter the test .csproj file.
- Under no circumstances touch other projects besides the test project in "/tests".
- Create a specialized branch for each test you will write, prefixed with "test/".
- The code created must be compilable and compatible with the current Sisk API.

## Tests todo

### LogStream tests

Write tests for LogStreams.

These tests should cover:
- basic I/O to files and TextWriters.
- I/O to two simultaneous outputs (files and TextWriters)
- race condition on writing
- writing exceptions
- writing prefixed logs
- log rotation
- log buffering

Test state:

- Code status: not written
- Assistant result: ...

### Cookies tests

Write tests for cookie handling.

- cookie parsing for correct and complex cookie syntax.
- cookie parsing for incorrect cookie syntax.
- cookie sending through HttpResponse.
- cookie building through CookieHelper.
- cookie sending through multiple Set-Cookie headers.

Test state:

- Code status: not written
- Assistant result: ...