# Improved route regex

Proposed to version 0.15.

Status: approved.

Routes that use regex have improved functionality, with improved performance and the ability to access regex components through groups.
Also, regex are cached into routes. The compilation is made when the route is accessed for the first time, and then there is no need
to parse the regex again. Changing the Path, which is the Pattern of the route that uses Regex, after being compiled, will invalidate
the newly interpreted regex and will force the router to interpret the pattern again.

This proposal also contemplates UseRegex for the route attribute.

```C#
[RouteGet(@"/uploads/(?<filename>.*\.(jpeg|jpg|png))", UseRegex = true)]
static HttpResponse RegexRoute(HttpRequest request)
{
    string? filename = request.Query["filename"];
    return new HttpResponse().WithContent($"Acessing file {filename}");
}
```

When testing, we can get:

```http
GET http://localhost:8080/uploads/2012/05/cute-cat.png

Acessing file 2012/05/cute-cat.png
```