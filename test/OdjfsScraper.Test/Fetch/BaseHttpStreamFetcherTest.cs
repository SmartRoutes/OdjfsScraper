﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Moq.Protected;
using OdjfsScraper.Fetch;
using OdjfsScraper.Models;
using Xunit;

namespace OdjfsScraper.Test.Fetch
{
    public abstract class BaseHttpStreamFetcherTest<TFetcher> where TFetcher : HttpStreamFetcher
    {
        protected abstract TFetcher GetFetcherForHttpStreamFetcherTests(HttpMessageHandler handler, string userAgent);

        [Fact]
        public void Constructor_HttpClientHandler()
        {
            VerifyHttpMessageHandler(new HttpClientHandler());
        }

        [Fact]
        public void Constructor_WebRequestHandler()
        {
            VerifyHttpMessageHandler(new WebRequestHandler());
        }

        [Fact]
        public void UnknownError()
        {
            var childCare = new ChildCare { ExternalUrlId = "AAAAAAAAAAAAAAAAAA" };
            VerifyAsyncException<FetcherException>(
                HttpStatusCode.InternalServerError, 
                null,
                string.Empty,
                f => f.GetChildCareDocument(childCare).Wait(),
                e => Assert.Equal(e.Message, "The response body or status code was unexpected."));
        }

        [Fact]
        public void PermanentError_NonNumeric()
        {
            VerifyPermanentError(
                HttpStatusCode.InternalServerError,
                null,
                "OraOLEDB error '80040e14' ORA-01858: a non-numeric character was found where a numeric was expected");
        }

        [Fact]
        public void PermanentError_InvalidHour()
        {
            VerifyPermanentError(
                HttpStatusCode.InternalServerError,
                null,
                "OraOLEDB error '80040e14' ORA-01850: hour must be between 0 and 23");
        }

        [Fact]
        public void PermanentError_DeletedRecord()
        {
            VerifyPermanentError(
                HttpStatusCode.InternalServerError,
                null,
                "ADODB.Field error '800a0bcd' Either BOF or EOF is True, or the current record has been deleted. Requested operation requires a current record.");
        }

        [Fact]
        public void TemporaryError_Unspecified()
        {
            VerifyTemporaryError(
                HttpStatusCode.InternalServerError,
                null,
                "Provider error '80004005' Unspecified error");
        }

        [Fact]
        public void TemporaryError_OracleNotConnected()
        {
            VerifyTemporaryError(
                HttpStatusCode.InternalServerError,
                null,
                "OraOLEDB error '80040e14' ORA-03114: not connected to ORACLE");
        }

        [Fact]
        public void TemporaryError_OracleNotAvailable()
        {
            VerifyTemporaryError(
                HttpStatusCode.InternalServerError,
                null,
                "OraOLEDB error '80004005' ORA-01034: ORACLE not available ORA-27101: shared memory realm does not exist IBM AIX RISC System/6000 Error: 2: No such file or directory");
        }

        [Fact]
        public void TemporaryError_ImmediateShutdown()
        {
            VerifyTemporaryError(
                HttpStatusCode.InternalServerError,
                null,
                "OraOLEDB error '80004005' ORA-01089: immediate shutdown in progress - no operations are permitted");
        }

        [Fact]
        public void TemporaryError_OracleShutdown()
        {
            VerifyTemporaryError(
                HttpStatusCode.InternalServerError,
                null,
                "OraOLEDB error '80004005' ORA-01033: ORACLE initialization or shutdown in progress");
        }

        [Fact]
        public void TemporaryError_Redirect()
        {
            VerifyTemporaryError(
                HttpStatusCode.Redirect,
                new Dictionary<string, string> {{"Location", "http://www.odjfs.state.oh.us/maintenance/"}},
                string.Empty);
        }

        [Fact]
        public void GetChildCareStubListDocument_HappyPath()
        {
            var county = new County {Name = "FRANKLIN"};
            VerifyRequest(
                HttpStatusCode.OK,
                null,
                string.Empty,
                fetcher => fetcher.GetChildCareStubListDocument(county),
                (request, userAgent) => VerifyCountyRequest(request, county.Name, userAgent),
                Assert.NotNull);
        }

        [Fact]
        public void GetChildCareStubListDocument_NullCounty()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                null,
                string.Empty,
                f => f.GetChildCareStubListDocument(null),
                e => Assert.Equal(e.ParamName, "county"));
        }

        [Fact]
        public void GetChildCareStubListDocument_NullCountyName()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                null,
                string.Empty,
                f => f.GetChildCareStubListDocument(new County {Name = null}),
                e => Assert.Equal(e.ParamName, "county.Name"));
        }

        [Fact]
        public void GetChildCareDocument_ChildCare_NullExternalUrlId()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                null,
                string.Empty,
                f => f.GetChildCareDocument(new ChildCare {ExternalUrlId = null}),
                e => Assert.Equal(e.ParamName, "childCare.ExternalUrlId"));
        }

        [Fact]
        public void GetChildCareDocument_ChildCareStub_NullExternalUrlId()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                null,
                string.Empty,
                f => f.GetChildCareDocument(new ChildCareStub {ExternalUrlId = null}),
                e => Assert.Equal(e.ParamName, "childCareStub.ExternalUrlId"));
        }

        [Fact]
        public void GetChildCareDocument_ChildCare_NullChildCare()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                null,
                string.Empty,
                f => f.GetChildCareDocument((ChildCare) null),
                e => Assert.Equal(e.ParamName, "childCare"));
        }

        [Fact]
        public void GetChildCareDocument_ChildCareStub_NullChildCareStub()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                null,
                string.Empty,
                f => f.GetChildCareDocument((ChildCareStub) null),
                e => Assert.Equal(e.ParamName, "childCareStub"));
        }

        [Fact]
        public void GetChildCareDocument_ChildCare_HappyPath()
        {
            var childCare = new ChildCare {ExternalUrlId = "CCCCCCCCCCCCCCCCCC"};
            VerifyRequest(
                HttpStatusCode.OK,
                null,
                string.Empty,
                fetcher => fetcher.GetChildCareDocument(childCare),
                (request, userAgent) => VerifyChildCareRequest(request, childCare.ExternalUrlId, userAgent),
                Assert.NotNull);
        }

        [Fact]
        public void GetChildCareDocument_ChildCareStub_HappyPath()
        {
            var childCareStub = new ChildCareStub {ExternalUrlId = "CCCCCCCCCCCCCCCCCC"};
            VerifyRequest(
                HttpStatusCode.OK,
                null,
                string.Empty,
                fetcher => fetcher.GetChildCareDocument(childCareStub),
                (request, userAgent) => VerifyChildCareRequest(request, childCareStub.ExternalUrlId, userAgent),
                Assert.NotNull);
        }

        private void VerifyException<T>(HttpStatusCode httpStatusCode, IDictionary<string, string> headers, string content, Action<HttpStreamFetcher> act, Action<T> verify) where T : Exception
        {
            // ARRANGE
            var mock = new Mock<HttpMessageHandler>();
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(GetHttpResponseMessage(httpStatusCode, headers, content));

            TFetcher fetcher = GetFetcherForHttpStreamFetcherTests(mock.Object, "Foo user agent");

            // ACT
            try
            {
                act(fetcher);
                Assert.True(false);
            }
            catch (T e)
            {
                // ASSERT
                verify(e);
            }
        }

        private void VerifyAsyncException<T>(HttpStatusCode httpStatusCode, IDictionary<string, string> headers, string content, Action<HttpStreamFetcher> act, Action<T> verify) where T : Exception
        {
            // ARRANGE
            var mock = new Mock<HttpMessageHandler>();
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(GetHttpResponseMessage(httpStatusCode, headers, content));

            TFetcher fetcher = GetFetcherForHttpStreamFetcherTests(mock.Object, "Foo user agent");

            // ACT
            try
            {
                act(fetcher);
                Assert.True(false);
            }
            catch (AggregateException ae)
            {
                T e = ae.Flatten().InnerExceptions.Take(1).OfType<T>().FirstOrDefault();
                if (e == null)
                {
                    Assert.True(false);
                }

                // ASSERT
                verify(e);
            }
        }

        private void VerifyHttpMessageHandler(HttpMessageHandler handler)
        {
            GetFetcherForHttpStreamFetcherTests(handler, "Foo");
        }

        private void VerifyHttpMessageHandler(HttpClientHandler handler)
        {
            VerifyHttpMessageHandler((HttpMessageHandler) handler);
            Assert.False(handler.AllowAutoRedirect);
            Assert.Equal(handler.AutomaticDecompression, DecompressionMethods.GZip | DecompressionMethods.Deflate);
            Assert.False(handler.UseCookies);
        }

        private void VerifyHttpMessageHandler(WebRequestHandler handler)
        {
            VerifyHttpMessageHandler((HttpClientHandler) handler);
            Assert.True(handler.AllowPipelining);
        }

        private void VerifyPermanentError(HttpStatusCode httpStatusCode, IDictionary<string, string> headers, string content)
        {
            var childCare = new ChildCare {ExternalUrlId = "AAAAAAAAAAAAAAAAAA"};
            VerifyRequest(
                httpStatusCode,
                headers,
                content,
                fetcher => fetcher.GetChildCareDocument(childCare),
                (request, userAgent) => VerifyChildCareRequest(request, childCare.ExternalUrlId, userAgent),
                Assert.Null);
        }

        private void VerifyTemporaryError(HttpStatusCode httpStatusCode, IDictionary<string, string> headers, string content)
        {
            var childCare = new ChildCare { ExternalUrlId = "AAAAAAAAAAAAAAAAAA" };
            VerifyAsyncException<HttpRequestException>(
                httpStatusCode,
                headers,
                content,
                f => f.GetChildCareDocument(childCare).Wait(),
                e => Assert.Equal(e.Message, "The response body has indicated that the document is temporarily unavailable."));
        }

        private void VerifyRequest(HttpStatusCode httpStatusCode, IDictionary<string, string> headers, string content, Func<HttpStreamFetcher, Task<Stream>> getStreamTask, Action<HttpRequestMessage, string> verifyRequest, Action<Stream> verifyStream)
        {
            // ARRANGE
            const string userAgent = "Foo user agent";
            var mock = new Mock<HttpMessageHandler>();
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(GetHttpResponseMessage(httpStatusCode, headers, content))
                .Callback<HttpRequestMessage, CancellationToken>((request, ct) => verifyRequest(request, userAgent));

            TFetcher fetcher = GetFetcherForHttpStreamFetcherTests(mock.Object, userAgent);

            // ACT
            Task<Stream> task = getStreamTask(fetcher);

            // ASSERT
            Stream result = task.Result;
            verifyStream(result);
        }

        protected static Task<HttpResponseMessage> GetHttpResponseMessage(HttpStatusCode httpStatusCode, IDictionary<string, string> headers, byte[] bytes)
        {
            var response = new HttpResponseMessage(httpStatusCode);
            response.Content = new ByteArrayContent(bytes);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }
            return Task.FromResult(response);
        }

        protected static Task<HttpResponseMessage> GetHttpResponseMessage(HttpStatusCode httpStatusCode, IDictionary<string, string> headers, string content)
        {
            return GetHttpResponseMessage(httpStatusCode, headers, Encoding.UTF8.GetBytes(content));
        }

        private static void VerifyCountyRequest(HttpRequestMessage request, string countyName, string userAgent)
        {
            VerifyRequest(request, userAgent);

            // parse the query
            NameValueCollection query = HttpUtility.ParseQueryString(request.RequestUri.Query);

            Assert.Equal(request.RequestUri.AbsolutePath, "/cdc/results1.asp");
            Assert.Equal(query["county"], countyName);
            Assert.Equal(query["rating"], "ALL");
            Assert.Equal(query["Printable"], "Y");
            Assert.Equal(query["ShowAllPages"], "Y");
        }

        private static void VerifyChildCareRequest(HttpRequestMessage request, string externalUrlId, string userAgent)
        {
            VerifyRequest(request, userAgent);

            // parse the query
            NameValueCollection query = HttpUtility.ParseQueryString(request.RequestUri.Query);

            Assert.Equal(request.RequestUri.AbsolutePath, "/cdc/results2.asp");
            Assert.Equal(query["provider_number"], externalUrlId);
            Assert.Equal(query["Printable"], "Y");
        }

        private static void VerifyRequest(HttpRequestMessage request, string userAgent)
        {
            Assert.Equal(request.RequestUri.Scheme, "http");
            Assert.Equal(request.RequestUri.Host, "www.odjfs.state.oh.us");
            Assert.Equal(request.Headers.UserAgent.ToString(), userAgent);
            Assert.Equal(request.Method, HttpMethod.Get);
        }
    }
}