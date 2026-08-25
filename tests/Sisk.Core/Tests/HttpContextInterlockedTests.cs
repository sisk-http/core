// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpContextInterlockedTests.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;

namespace Sisk.Core.Tests;

[TestClass]
public class HttpContextInterlockedTests {

    [TestMethod]
    public async Task Add_ShouldBeAtomic () {
        const Int32 operationCount = 10_000;
        using var context = CreateContext ();

        await Task.WhenAll ( Enumerable.Range ( 0, operationCount )
            .Select ( _ => Task.Run ( () => context.Interlocked.Add ( "counter", 1 ) ) ) );

        Assert.AreEqual ( operationCount, context.Interlocked.Inspect ( "counter" ) );
    }

    [TestMethod]
    public void CompareExchange_ShouldReturnOriginalValue () {
        using var context = CreateContext ();
        context.Interlocked.Add ( "value", 1 );

        var originalValue = context.Interlocked.CompareExchange ( "value", 2, 1 );

        Assert.AreEqual ( 1, originalValue );
        Assert.AreEqual ( 2, context.Interlocked.Inspect ( "value" ) );
    }

    [TestMethod]
    public void CompareExchange_ShouldNotCreateMissingValue () {
        using var context = CreateContext ();

        var originalValue = context.Interlocked.CompareExchange ( "missing", 2, 1, notFound: 3 );

        Assert.AreEqual ( 3, originalValue );
        Assert.IsNull ( context.Interlocked.Inspect ( "missing" ) );
    }

    [TestMethod]
    public void Exchange_ShouldReturnOriginalValue () {
        using var context = CreateContext ();
        context.Interlocked.Add ( "value", 1 );

        var originalValue = context.Interlocked.Exchange ( "value", 2 );

        Assert.AreEqual ( 1, originalValue );
        Assert.AreEqual ( 2, context.Interlocked.Inspect ( "value" ) );
    }

    private static HttpContext CreateContext () =>
        (HttpContext) Activator.CreateInstance (
            typeof ( HttpContext ),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            args: [ new HttpServer () ],
            culture: null )!;
}
