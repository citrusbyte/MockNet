using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemHttpRequestMessage = System.Net.Http.HttpRequestMessage;

namespace Theorem.MockNet.Http
{
    internal class Setup : ISetup
    {
        private readonly RequestMessage request;
        private readonly int defaultHttpStatusCode = 200;

        internal ResultCollection Results { get; }

        public Setup(RequestMessage request)
        {
            this.request = request;
            this.defaultHttpStatusCode = request.HttpMethod.DefaultStatusCode();

            Results = new ResultCollection();
        }

        public IReturns ReturnsAsync(int code)
        {
            return ReturnsAsync(code, null, (HttpContent)null);
        }

        public IReturns ReturnsAsync<THttpContent>(THttpContent content) where THttpContent : HttpContent
        {
            return ReturnsAsync(defaultHttpStatusCode, null, content);
        }

        public IReturns ReturnsAsync(HttpResponseHeaders headers)
        {
            return ReturnsAsync(defaultHttpStatusCode, headers, (HttpContent)null);
        }

        public IReturns ReturnsAsync<THttpContent>(int code, THttpContent content) where THttpContent : HttpContent
        {
            return ReturnsAsync(code, null, content);
        }

        public IReturns ReturnsAsync(int code, HttpResponseHeaders headers)
        {
            return ReturnsAsync(code, headers, (HttpContent)null);
        }

        public IReturns ReturnsAsync<THttpContent>(HttpResponseHeaders headers, THttpContent content) where THttpContent : HttpContent
        {
            return ReturnsAsync(defaultHttpStatusCode, headers, content);
        }

        public IReturns ReturnsAsync(object content)
        {
            return ReturnsAsync(defaultHttpStatusCode, null,
                content is null
                    ? (HttpContent)null
                    : new ObjectContent(content.GetType(), content));
        }

        public IReturns ReturnsAsync(int code, object content)
        {
            return ReturnsAsync(code, null,
                content is null
                    ? (HttpContent)null
                    : new ObjectContent(content.GetType(), content));
        }

        public IReturns ReturnsAsync(int code, HttpResponseHeaders headers, object content)
        {
            return ReturnsAsync(code, headers,
                content is null
                    ? (HttpContent)null
                    : new ObjectContent(content.GetType(), content));
        }

        public IReturns ReturnsAsync(HttpResponseHeaders headers, object content)
        {
            return ReturnsAsync(defaultHttpStatusCode, headers,
                content is null
                    ? (HttpContent)null
                    : new ObjectContent(content.GetType(), content));
        }

        public IReturns ReturnsAsync<THttpContent>(int code, HttpResponseHeaders headers, THttpContent content) where THttpContent : HttpContent
        {
            var message = new HttpResponseMessageBuilder()
                .WithStatusCode(code)
                .WithContent(content)
                .WithHeaders(headers)
                .Build();

            Results.Add(new Result()
            {
                HttpResponseMessage = message
            });

            return this;
        }

        public async Task<MockHttpClientException> Matches(SystemHttpRequestMessage message)
        {
            if (request.HttpMethod.Method != message.Method.Method)
            {
                return await MockHttpClientException.UnmatchedHttpMethod(request, message);
            }

            // TODO: better match request uri
            if (request.RequestUri != message.RequestUri.PathAndQuery)
            {
                return await MockHttpClientException.UnmatchedRequestUri(request, message);
            }

            if (request.HeadersValidator is Delegate)
            {
                var headers = new HttpRequestHeaders(message.Headers, message.Content?.Headers);
                var b = request.HeadersValidator.DynamicInvoke(headers);
                if (b is false)
                {
                    return await MockHttpClientException.UnmatchedRequestHeaders(request, message);
                }
            }

            if (request.ContentValidator is Delegate)
            {
                var content = await request.Deserializer(message);
                var b = request.ContentValidator.DynamicInvoke(content);
                if (b is false)
                {
                    return await MockHttpClientException.UnmatchedRequestContent(request, message);
                }
            }

            return null;
        }

        public MockHttpClientException VerifyAll()
        {
            return TryVerify(result => result.VerifyAll());
        }

        private MockHttpClientException TryVerify(Func<Result, MockHttpClientException> verify)
        {
            List<MockHttpClientException> errors = new List<MockHttpClientException>();

            foreach (var result in Results)
            {
                var error = verify(result);
                if (error is MockHttpClientException)
                {
                    errors.Add(error);
                }
            }

            if (errors.Any())
            {
                return MockHttpClientException.Combined(errors);
            }

            return null;
        }
    }
}