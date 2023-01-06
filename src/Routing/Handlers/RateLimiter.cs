using Sisk.Core.Http;
using System.Net;

namespace Sisk.Core.Routing.Handlers
{
    /// <summary>
    /// Represents a rate limiter that is executed after the socket receives the HTTP message from the server.
    /// </summary>
    /// <definition>
    /// public class RateLimiter : IRequestHandler
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing.Handlers
    /// </namespace>
    public class RateLimiter : IRequestHandler
    {
        /// <summary>
        /// Gets or sets the cache of requests that this Rate Limiter stores.
        /// </summary>
        /// <definition>
        /// public RateLimiterRepository Repository { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public RateLimiterRepository Repository { get; set; }

        /// <summary>
        /// Gets or sets the routing limitation policy settings for this RateLimiter.
        /// </summary>
        /// <definition>
        /// public RateLimiterPolicy LimitingPolicy { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public RateLimiterPolicy LimitingPolicy { get; set; }

        /// <summary>
        /// Gets a unique identifier for this <see cref="RateLimiter"/> instance.
        /// </summary>
        /// <definition>
        /// public string Identifier { get; init; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public string Identifier { get; init; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets when this RequestHandler should run.
        /// </summary>
        /// <definition>
        /// public RequestHandlerExecutionMode ExecutionMode { get; init; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public RequestHandlerExecutionMode ExecutionMode { get; init; } = RequestHandlerExecutionMode.BeforeResponse;

        /// <summary>
        /// Creates a new <see cref="RateLimiter"/> instance with given parameters.
        /// </summary>
        /// <param name="repository">Gets or sets the cache of requests that this Rate Limiter stores.</param>
        /// <param name="limitingPolicy">Gets or sets the routing limitation policy settings for this RateLimiter.</param>
        /// <definition>
        /// public RateLimiter(RateLimiterRepository repository, RateLimiterPolicy limitingPolicy)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public RateLimiter(RateLimiterRepository repository, RateLimiterPolicy limitingPolicy)
        {
            Repository = repository;
            LimitingPolicy = limitingPolicy;
        }

        /// <summary>
        /// Executes the rate limiter action on the route and checks if it will be blocked or not.
        /// </summary>
        /// <definition>
        /// public HttpResponse? Execute(HttpRequest request, HttpContext context)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public HttpResponse? Execute(HttpRequest request, HttpContext context)
        {
            Repository.CacheRequest(request);
            bool result = Repository.TestRequestByPolicy(request, this.LimitingPolicy);
            if (result == false)
            {
                HttpResponse res = new HttpResponse();
                res.Status = HttpStatusCode.TooManyRequests;
                return res;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Represents an <see cref="RateLimiter"/> cache repository.
    /// </summary>
    /// <definition>
    /// public class RateLimiterRepository
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing.Handlers
    /// </namespace>
    public class RateLimiterRepository
    {
        private List<HttpRequest> limiterCache = new List<HttpRequest>();

        /// <summary>
        /// Gets or sets the maximum number of requests this cache can store.
        /// </summary>
        /// <definition>
        /// public int HeapSize { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public int HeapSize { get; set; } = 0xFF;

        /// <summary>
        /// Caches the given HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request to be cached.</param>
        /// <definition>
        /// public void CacheRequest(HttpRequest request)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public void CacheRequest(HttpRequest request)
        {
            limiterCache.Add(request);
            if (limiterCache.Count > HeapSize)
            {
                limiterCache.RemoveAt(0);
            }
        }

        /// <summary>
        /// Tests whether a request is eligible to continue or will be throttled.
        /// </summary>
        /// <param name="request">The testing HTTP request.</param>
        /// <param name="policy">The Rate Limiter parameters for testing.</param>
        /// <returns></returns>
        /// <definition>
        /// public bool TestRequestByPolicy(HttpRequest request, RateLimiterPolicy policy)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public bool TestRequestByPolicy(HttpRequest request, RateLimiterPolicy policy)
        {
            if (policy.LimitByOriginIP)
            {
                var requestsByIP = limiterCache.Where((e) =>
                {
                    return (DateTime.Now - e.RequestedAt) <= policy.TimeToLive
                       && e.Origin.Equals(request.Origin)
                       && e.Path == request.Path;
                });
                if (requestsByIP.Count() >= policy.MaximumRequests)
                {
                    return false;
                }
            }
            if (policy.LimitByCookies)
            {
                var requestsByIP = limiterCache.Where((e) =>
                {
                    return (DateTime.Now - e.RequestedAt) <= policy.TimeToLive
                        && (e.Headers["Cookie"] ?? "A") == (request.Headers["Cookie"] ?? "B")
                        && e.Path == request.Path;
                });
                if (requestsByIP.Count() >= policy.MaximumRequests)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Represents the settings of a <see cref="RateLimiter"/> object.
    /// </summary>
    /// <definition>
    /// public class RateLimiterPolicy
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing.Handlers
    /// </namespace>
    public class RateLimiterPolicy
    {
        /// <summary>
        /// Gets or sets the maximum time between the maximum requests that it should be executed.
        /// </summary>
        /// <definition>
        /// public TimeSpan TimeToLive { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public TimeSpan TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of requests that will be allowed in the time specified in TimeToLive.
        /// </summary>
        /// <definition>
        /// public int MaximumRequests { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public int MaximumRequests { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="RateLimiter"/> will limit by the IP address.
        /// </summary>
        /// <definition>
        /// public bool LimitByOriginIP { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public bool LimitByOriginIP { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="RateLimiter"/> will limit by the Cookie value.
        /// </summary>
        /// <definition>
        /// public bool LimitByCookies { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public bool LimitByCookies { get; set; }

        /// <summary>
        /// Create a new <see cref="RateLimiterPolicy"/> instance with given parameters.
        /// </summary>
        /// <definition>
        /// public RateLimiterPolicy(TimeSpan timeToLive, int maximumRequests, bool limitByOriginIP, bool limitByCookies)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public RateLimiterPolicy(TimeSpan timeToLive, int maximumRequests, bool limitByOriginIP, bool limitByCookies)
        {
            TimeToLive = timeToLive;
            MaximumRequests = maximumRequests;
            LimitByOriginIP = limitByOriginIP;
            LimitByCookies = limitByCookies;
        }
    }
}
