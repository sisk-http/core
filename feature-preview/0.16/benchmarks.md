Command and script used:

```
> k6 run .\k6.js --vus 500 --duration 10s
```

```js
import http from 'k6/http';

export default function () {
    http.get('http://localhost:5155/');
}
```

----

# Sync test 1

Syncronous method, defined from instanted class, casting object into HttpResponse

```cs
HttpServer server = HttpServer.Emit(5155, out HttpServerConfiguration config, out var host, out var router);
config.ThrowExceptions = true;
config.AccessLogsStream = null;
router.SetObject(new MyController());
router.RegisterValueHandler<object>(i => new HttpResponse().WithContent(i.ToString()!));

server.Start();
```

```cs
class MyController
{
    [RouteGet("/")]
    public string Index(HttpRequest request)
    {
        return "Hello, world!";
    }
}
```

Results:

```
data_received..................: 39 MB  3.9 MB/s
data_sent......................: 17 MB  1.7 MB/s
http_req_blocked...............: avg=10.44µs min=0s      med=0s      max=28.02ms  p(90)=0s      p(95)=0s
http_req_connecting............: avg=7.8µs   min=0s      med=0s      max=8.55ms   p(90)=0s      p(95)=0s
http_req_duration..............: avg=23.59ms min=998.5µs med=21.66ms max=273.58ms p(90)=34.04ms p(95)=39.26ms
{ expected_response:true }...: avg=23.59ms min=998.5µs med=21.66ms max=273.58ms p(90)=34.04ms p(95)=39.26ms
http_req_failed................: 0.00%  ✓ 0            ✗ 211128
http_req_receiving.............: avg=38.71µs min=0s      med=0s      max=77.28ms  p(90)=0s      p(95)=0s
http_req_sending...............: avg=13.92µs min=0s      med=0s      max=59.97ms  p(90)=0s      p(95)=0s
http_req_tls_handshaking.......: avg=0s      min=0s      med=0s      max=0s       p(90)=0s      p(95)=0s
http_req_waiting...............: avg=23.54ms min=998.5µs med=21.62ms max=273.58ms p(90)=33.94ms p(95)=39.2ms
http_reqs......................: 211128 21083.501312/s
iteration_duration.............: avg=23.66ms min=998.5µs med=21.69ms max=275.68ms p(90)=34.15ms p(95)=39.39ms
iterations.....................: 211128 21083.501312/s
vus............................: 500    min=500        max=500
vus_max........................: 500    min=500        max=500
```

# Sync test 2

Syncronous method, defined from instanted class, returning HttpResponse object, no cast

```cs
HttpServer server = HttpServer.Emit(5155, out HttpServerConfiguration config, out var host, out var router);
config.ThrowExceptions = true;
config.AccessLogsStream = null;
router.SetObject(new MyController());

server.Start();
```

```cs
class MyController
{
    [RouteGet("/")]
    public HttpResponse Index(HttpRequest request)
    {
        return new HttpResponse()
            .WithContent("Hello, world!");
    }
}
```

Results:

```
data_received..................: 38 MB  3.8 MB/s
data_sent......................: 16 MB  1.6 MB/s
http_req_blocked...............: avg=21.23µs min=0s med=0s      max=19.6ms   p(90)=0s      p(95)=0s
http_req_connecting............: avg=18.84µs min=0s med=0s      max=17.59ms  p(90)=0s      p(95)=0s
http_req_duration..............: avg=24.28ms min=0s med=22.89ms max=105.66ms p(90)=35.42ms p(95)=40.8ms
{ expected_response:true }...: avg=24.28ms min=0s med=22.89ms max=105.66ms p(90)=35.42ms p(95)=40.8ms
http_req_failed................: 0.00%  ✓ 0           ✗ 205405
http_req_receiving.............: avg=39.87µs min=0s med=0s      max=71.4ms   p(90)=0s      p(95)=0s
http_req_sending...............: avg=12.24µs min=0s med=0s      max=20.72ms  p(90)=0s      p(95)=0s
http_req_tls_handshaking.......: avg=0s      min=0s med=0s      max=0s       p(90)=0s      p(95)=0s
http_req_waiting...............: avg=24.22ms min=0s med=22.86ms max=99.53ms  p(90)=35.34ms p(95)=40.7ms
http_reqs......................: 205405 20465.75497/s
iteration_duration.............: avg=24.35ms min=0s med=22.96ms max=105.66ms p(90)=35.58ms p(95)=40.87ms
iterations.....................: 205405 20465.75497/s
vus............................: 500    min=500       max=500
vus_max........................: 500    min=500       max=500
```

# Sync test 3

Syncronous method, defined from instanted class, returning HttpResponse object defined into an static instance

```cs
HttpServer server = HttpServer.Emit(5155, out HttpServerConfiguration config, out var host, out var router);
config.ThrowExceptions = true;
config.AccessLogsStream = null;
router.SetObject(new MyController());

server.Start();
```

```cs
class MyController
{
    static HttpResponse res = new HttpResponse()
            .WithContent("Hello, world!");

    [RouteGet("/")]
    public HttpResponse Index(HttpRequest request)
    {
        return res;
    }
}
```

Results:

```
data_received..................: 39 MB  3.9 MB/s
data_sent......................: 17 MB  1.7 MB/s
http_req_blocked...............: avg=20.66µs min=0s     med=0s      max=25.12ms  p(90)=0s      p(95)=0s
http_req_connecting............: avg=17.53µs min=0s     med=0s      max=20.6ms   p(90)=0s      p(95)=0s
http_req_duration..............: avg=23.63ms min=1.52ms med=21.39ms max=240.84ms p(90)=34.77ms p(95)=40.81ms
{ expected_response:true }...: avg=23.63ms min=1.52ms med=21.39ms max=240.84ms p(90)=34.77ms p(95)=40.81ms
http_req_failed................: 0.00%  ✓ 0            ✗ 210610
http_req_receiving.............: avg=38.42µs min=0s     med=0s      max=112.71ms p(90)=0s      p(95)=0s
http_req_sending...............: avg=20.07µs min=0s     med=0s      max=25.12ms  p(90)=0s      p(95)=0s
http_req_tls_handshaking.......: avg=0s      min=0s     med=0s      max=0s       p(90)=0s      p(95)=0s
http_req_waiting...............: avg=23.57ms min=1.52ms med=21.34ms max=240.84ms p(90)=34.72ms p(95)=40.72ms
http_reqs......................: 210610 21007.849091/s
iteration_duration.............: avg=23.71ms min=1.52ms med=21.44ms max=260.44ms p(90)=34.86ms p(95)=41ms
iterations.....................: 210610 21007.849091/s
vus............................: 500    min=500        max=500
vus_max........................: 500    min=500        max=500
```

# Async test 1

Async method, defined from instanted class, returning Task'HttpResponse

```cs
class MyController
{
    [RouteGet("/")]
    public async Task<HttpResponse> Index(HttpRequest request)
    {
        return new HttpResponse().WithContent("Hello, world!");
    }
}
```

Results:

```
data_received..................: 35 MB  3.5 MB/s
data_sent......................: 15 MB  1.5 MB/s
http_req_blocked...............: avg=38.93µs min=0s  med=0s      max=53.39ms  p(90)=0s      p(95)=0s
http_req_connecting............: avg=35.45µs min=0s  med=0s      max=23.67ms  p(90)=0s      p(95)=0s
http_req_duration..............: avg=26.31ms min=2ms med=24.08ms max=208.93ms p(90)=39.09ms p(95)=44.88ms
{ expected_response:true }...: avg=26.31ms min=2ms med=24.08ms max=208.93ms p(90)=39.09ms p(95)=44.88ms
http_req_failed................: 0.00%  ✓ 0            ✗ 189315
http_req_receiving.............: avg=47.31µs min=0s  med=0s      max=72.46ms  p(90)=0s      p(95)=0s
http_req_sending...............: avg=14.79µs min=0s  med=0s      max=48.83ms  p(90)=0s      p(95)=0s
http_req_tls_handshaking.......: avg=0s      min=0s  med=0s      max=0s       p(90)=0s      p(95)=0s
http_req_waiting...............: avg=26.24ms min=2ms med=24.05ms max=205.25ms p(90)=39.02ms p(95)=44.81ms
http_reqs......................: 189315 18880.428064/s
iteration_duration.............: avg=26.41ms min=2ms med=24.12ms max=217.9ms  p(90)=39.16ms p(95)=45.01ms
iterations.....................: 189315 18880.428064/s
vus............................: 500    min=500        max=500
vus_max........................: 500    min=500        max=500
```

# Async test 2

Async method, defined from instanted class, returning Task'int object

```cs
HttpServer server = HttpServer.Emit(5155, out HttpServerConfiguration config, out var host, out var router);
config.ThrowExceptions = true;
config.AccessLogsStream = null;

router.SetObject(new MyController());
router.RegisterValueHandler<int>(s => new HttpResponse().WithContent(s.ToString()!));
```

```cs
class MyController
{
    [RouteGet("/")]
    public async Task<int> Index(HttpRequest request)
    {
        return 123;
    }
}
```

Results:

```
data_received..................: 143 kB 7.4 kB/s
data_sent......................: 65 kB  3.4 kB/s
http_req_blocked...............: avg=5.71ms   min=0s      med=0s    max=24.14ms p(90)=18.14ms  p(95)=19.2ms
http_req_connecting............: avg=5.5ms    min=0s      med=0s    max=24.14ms p(90)=18.14ms  p(95)=18.14ms
http_req_duration..............: avg=9.23s    min=62ms    med=9.47s max=18.4s   p(90)=15.33s   p(95)=16.35s
{ expected_response:true }...: avg=9.23s    min=62ms    med=9.47s max=18.4s   p(90)=15.33s   p(95)=16.35s
http_req_failed................: 0.00%  ✓ 0         ✗ 818
http_req_receiving.............: avg=205.88µs min=0s      med=0s    max=1.5ms   p(90)=968.09µs p(95)=1ms
http_req_sending...............: avg=1.51ms   min=0s      med=0s    max=16.55ms p(90)=4.82ms   p(95)=13.04ms
http_req_tls_handshaking.......: avg=0s       min=0s      med=0s    max=0s      p(90)=0s       p(95)=0s
http_req_waiting...............: avg=9.23s    min=60.08ms med=9.47s max=18.4s   p(90)=15.33s   p(95)=16.35s
http_reqs......................: 818    42.434039/s
iteration_duration.............: avg=9.24s    min=82.14ms med=9.48s max=18.4s   p(90)=15.33s   p(95)=16.36s
iterations.....................: 818    42.434039/s
vus............................: 27     min=27      max=500
vus_max........................: 500    min=500     max=500
```