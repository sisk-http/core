# Implicit return types

Proposed to version 0.15.

Status: included in production release 0.15.

Implicitly typed returns allow callbacks to return any object other than HttpResponse. The motivation is to make it possible to transform in a single place for standardized types of responses, such as API responses, and so that it is possible to handle implicit returns and convert them to HttpResponse objects.

Inspired by ASP.NET and other simpler frameworks, which allows registering type associations for each value received. Unregistered types will throw an exception. Also, for value-types objects, it will be necessary to box the value in an object by reference (see document below).

```cs

/*
    The ability to return objects by reference always works with first-level objects.
*/

[RouteGet("/<id>")]
public User View(HttpRequest request)
{
    int id = Int32.Parse(request.Query["id"]!);
    User dUser = Users.First(u => u.Id == id);

    return dUser;
}

/*
    For structures (value types) it is necessary to package the object. This
    feature-preview also includes the ValueResult<> type, which is nothing
    more than an implicit reflection for an existing object.
*/

[RoutePost("/")]
public ValueResult<bool> Create(HttpRequest request)
{
    User fromBody = JsonSerializer.Deserialize<User>(request.Body)!;
    Users.Add(fromBody);

    return true;
}
```

So, it is necessary to associate how the router will create an HttpResponse from the object you gave it.

```cs
router.RegisterValueHandler<User>(user =>
{
    return new HttpResponse(200) { ... }; // do something with user
});

// for value-types, you don't need to use ValueResult<> for handling
// these values
router.RegisterValueHandler<bool>(boolvalue => ...);
```

Type comparison is not explicit. It always compares whether the type returned from the callback is assignable to a type registered in the binding.

```cs
class A {}
class B : A {}

router.RegisterValueHandler<B>(user =>
{
    // will also match A, since B is assignable to A
});
router.RegisterValueHandler<A>(user =>
{
    // will not match B
});
```

Following this rule, if you bind the type `object`, you may have a fallback to types not recognized by previous types. However, the record order matters, so the association "object" must be the last record.

```cs
router.RegisterValueHandler<Foo>();
router.RegisterValueHandler<Bar>();
router.RegisterValueHandler<Daz>();
router.RegisterValueHandler<object>(); // <- fallback
```

In addition to type, it is mandatory that all returned objects **should not be null**. Values returned by callbacks other than `IRequestHandlers` will return an error by the HttpResponse handler.