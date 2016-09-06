#region License
/*
    Copyright (c) 2010, Paweł Hofman (CodeTitans)
    All Rights Reserved.

    Licensed under the Apache License version 2.0.
    For more information please visit:

    http://codetitans.codeplex.com/license
        or
    http://www.apache.org/licenses/


    For latest source code, documentation, samples
    and more information please visit:

    http://codetitans.codeplex.com/
*/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using CodeTitans.Diagnostics;

#if !CODETITANS_LIB_CORE
namespace CodeTitans.Bayeux
#else
namespace CodeTitans.Core.Net
#endif
{
    /// <summary>
    /// Class responsible for transmitting text data for designated web servers over HTTP protocol.
    /// </summary>
    public class HttpDataSource : IHttpDataSource
    {
        /// <summary>
        /// Gets the POST method name.
        /// </summary>
        public const string MethodPost = "POST";
        /// <summary>
        /// Gets the GET method name.
        /// </summary>
        public const string MethodGet = "GET";

        /// <summary>
        /// Defines the default value of timeout.
        /// </summary>
        public const int DefaultTimeout = 20 * 1000;

        private const string HeaderSentAt = "X-Sent";
        private const string NoContent = "(nothing)";
        private const string BinaryContent = "(binary data)";

        private readonly string _url;
        private readonly Encoding _defaultEncoding;
        private readonly Encoding _responseEncoding;
        private HttpWebRequest _request;
        private Dictionary<string, string> _headers;

        #region Events

        /// <summary>
        /// Event fired if data has been received for current request.
        /// </summary>
        public event EventHandler<HttpDataSourceEventArgs> DataReceived;
        /// <summary>
        /// Event fired if data reception failed for current request.
        /// </summary>
        public event EventHandler<HttpDataSourceEventArgs> DataReceiveFailed;

        #endregion

        #region Private Classes

        /// <summary>
        /// Wrapper class for asynchronous data to send for given request.
        /// </summary>
        private class AsyncDataRequest
        {
            public AsyncDataRequest(HttpWebRequest request, string dataDescription, byte[] data, HttpDataSourceResponseType responseType)
            {
                if (request == null)
                    throw new ArgumentNullException("request");

                Request = request;
                Data = data;
                ResponseType = responseType;
#if DEBUG
                DataDescription = dataDescription;
#endif
            }

            #region Properties

            /// <summary>
            /// Gets request.
            /// </summary>
            public HttpWebRequest Request
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets data associated with given request, that should be printed in debug logs to reflect the current action.
            /// </summary>
            public string DataDescription
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets data associated with given request, that should be send to the server.
            /// </summary>
            public byte[] Data
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the type of expected response.
            /// </summary>
            public HttpDataSourceResponseType ResponseType
            {
                get;
                private set;
            }


            #endregion
        }

        #endregion

        /// <summary>
        /// Init constructor.
        /// </summary>
        public HttpDataSource(string url, Encoding defaultEncoding, string contentType, Encoding responseEncoding, string acceptContentType)
        {
            _url = url;
            _defaultEncoding = defaultEncoding ?? Encoding.UTF8;
            ContentType = contentType;
            _responseEncoding = responseEncoding;
            AcceptContentType = acceptContentType ?? contentType;
            Timeout = DefaultTimeout;
        }

        /// <summary>
        /// Init constructor.
        /// </summary>
        public HttpDataSource(string url, Encoding defaultEncoding, string contentType)
            : this(url, defaultEncoding, contentType, null, null)
        {
        }

        /// <summary>
        /// Init constructor.
        /// </summary>
        public HttpDataSource(string url)
            : this(url, null, null)
        {
        }

        /// <summary>
        /// Sends request to the data source.
        /// </summary>
        public void SendRequest(string method, HttpDataSourceResponseType responseType)
        {
            InternalSendRequest(null, NoContent, null, method, false, responseType);
        }

        /// <summary>
        /// Sends request to the data source.
        /// </summary>
        public void SendRequest(string relativeUrlPath, byte[] data, string method, HttpDataSourceResponseType responseType)
        {
            InternalSendRequest(relativeUrlPath, data != null && data.Length > 0 ? BinaryContent : NoContent, data, method, false, responseType);
        }

        /// <summary>
        /// Sends request to the data source.
        /// </summary>
        public void SendRequest(string relativeUrlPath, string data, string method, HttpDataSourceResponseType responseType)
        {
            if (string.IsNullOrEmpty(data))
                InternalSendRequest(relativeUrlPath, NoContent, null, method, false, responseType);
            else
                InternalSendRequest(relativeUrlPath, data, GetSerializedData(data), method, false, responseType);
        }

        /// <summary>
        /// Sends asynchronous request to the data source.
        /// </summary>
        public void SendRequestAsync(string method, HttpDataSourceResponseType responseType)
        {
            InternalSendRequest(null, NoContent, null, method, true, responseType);
        }

        /// <summary>
        /// Sends asynchronous request to the data source.
        /// </summary>
        public void SendRequestAsync(string relativeUrlPath, byte[] data, string method, HttpDataSourceResponseType responseType)
        {
            InternalSendRequest(relativeUrlPath, data != null && data.Length > 0 ? BinaryContent : NoContent, data, method, true, responseType);
        }

        /// <summary>
        /// Sends asynchronous request to the data source.
        /// </summary>
        public void SendRequestAsync(string relativeUrlPath, string data, string method, HttpDataSourceResponseType responseType)
        {
            if (string.IsNullOrEmpty(data))
                InternalSendRequest(relativeUrlPath, NoContent, null, method, true, responseType);
            else
                InternalSendRequest(relativeUrlPath, data, GetSerializedData(data), method, true, responseType);
        }

        /// <summary>
        /// Sends request to the data source.
        /// </summary>
        private void InternalSendRequest(string relativeUrlPath, string dataDescription, byte[] data, string method, bool asynchronous, HttpDataSourceResponseType responseType)
        {
            if (dataDescription == null)
                throw new ArgumentOutOfRangeException("dataDescription");
            if (_request != null)
                throw new InvalidOperationException("Another request is being processed. Call Cancel() method first.");
            if (method != MethodPost && method != MethodGet)
                throw new InvalidOperationException("Invalid method used. Try 'POST' or 'GET'.");

            //////////////////////
            // FILL HTTP request:
            DateTime now = DateTime.Now;
            string uri = string.IsNullOrEmpty(relativeUrlPath) ? _url : _url + relativeUrlPath;
            HttpWebRequest webRequest = CreateHttpWebRequest(uri, method, now, GetSerializedDataLength(data));

            SetRequest(webRequest);

            if (asynchronous)
            {
                //////////////////////
                // Format data to sent:
                var dataRequest = new AsyncDataRequest(webRequest, dataDescription, data, responseType);

                if (data != null && data.Length > 0)
                {
                    webRequest.BeginGetRequestStream(AsyncWebRequestStreamCallback, dataRequest);
                }
                else
                {
                    DebugLog.WriteCoreLine(string.Format(CultureInfo.InvariantCulture, "<--- Starting request (at: {0}): {1}",
                        now, webRequest.RequestUri.AbsoluteUri));

                    //////////////////////
                    // Send request:
                    try
                    {
                        webRequest.BeginGetResponse(AsyncWebResponseCallback, dataRequest);
                    }
                    catch (WebException ex)
                    {
                        if (ex.Status != WebExceptionStatus.RequestCanceled)
                            ProcessResponse(webRequest, null, responseType);
                    }
                }
            }
            else
            {
                //////////////////////
                // Format data to sent:
                if (data != null && data.Length > 0)
                {
                    DebugLog.WriteCoreLine(string.Format(CultureInfo.InvariantCulture, "<--- Sending request (length: {0} bytes, at: {1}): {2} ({3})",
                                                dataDescription != null ? dataDescription.Length : 0, now, webRequest.RequestUri.AbsoluteUri, ContentType));
                    DebugLog.WriteCoreLine(dataDescription ?? string.Empty);

                    if (WriteRequestData(webRequest, null, data))
                        return;
                }
                else
                {
                    DebugLog.WriteCoreLine(string.Format(CultureInfo.InvariantCulture, "<--- Starting request (at: {0}): {1}",
                        DateTime.Now, webRequest.RequestUri.AbsoluteUri));
                }

                //////////////////////
                // Send request and process response:
                HttpWebResponse response;
                bool canProcess = true;

                try
                {
                    response = GetResponse(webRequest);
                }
                catch (WebException ex)
                {
                    canProcess = ex.Status != WebExceptionStatus.RequestCanceled;
                    response = (HttpWebResponse)ex.Response;
                }

                if (canProcess)
                    ProcessResponse(webRequest, response, responseType);
            }
        }

        private HttpWebRequest CreateHttpWebRequest(string uri, string method, DateTime now, long contentLength)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);

            if (NetworkCredential != null)
            {
#if !PocketPC
                webRequest.UseDefaultCredentials = false;
#endif
                webRequest.Credentials = NetworkCredential;
            }

#if !PocketPC && !WINDOWS_PHONE
            webRequest.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
            webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
#endif
            webRequest.Method = method;
            webRequest.Accept = AcceptContentType;
            webRequest.ContentType = ContentType;
            webRequest.UserAgent = UserAgent;

#if WINDOWS_PHONE
            webRequest.Headers[HeaderSentAt] = now.Ticks.ToString();
            webRequest.AllowAutoRedirect = false;
            webRequest.AllowReadStreamBuffering = false;
#else
            webRequest.Headers.Add(HeaderSentAt, now.Ticks.ToString(CultureInfo.InvariantCulture));
            webRequest.ContentLength = contentLength;
            webRequest.KeepAlive = false;
            webRequest.Timeout = Timeout;
            webRequest.ReadWriteTimeout = Timeout;
            webRequest.Pipelined = false;
#endif

            // append additional headers:
            if (_headers != null)
            {
#if WINDOWS_PHONE
                foreach (KeyValuePair<string, string> header in _headers)
                    webRequest.Headers[header.Key] = header.Value;
#else
                foreach (KeyValuePair<string, string> header in _headers)
                {
                    if (string.Compare("referer", header.Key, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        webRequest.Referer = header.Value;
                        continue;
                    }

#if !NET_2_COMPATIBLE
                    if (string.Compare("host", header.Key, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        webRequest.Host = header.Value;
                        continue;
                    }
#endif
                    webRequest.Headers.Add(header.Key, header.Value);
                }
#endif
            }

            AppendAdditionalInfo(webRequest);

            return webRequest;
        }

        private HttpWebRequest SetRequest(HttpWebRequest request)
        {
            return Interlocked.Exchange(ref _request, request);
        }

        private bool TryCompleteRequest(HttpWebRequest request)
        {
            return Interlocked.CompareExchange(ref _request, null, request) == request;
        }

        private static HttpStatusCode GetHttpStatusCode(WebExceptionStatus status)
        {
            if (status == WebExceptionStatus.Timeout)
                return HttpStatusCode.RequestTimeout;

            return HttpStatusCode.BadRequest;
        }

        /// <summary>
        /// Gets the response object in synchronous way.
        /// </summary>
        private HttpWebResponse GetResponse(HttpWebRequest request)
        {
#if WINDOWS_PHONE
            responseReceived = false;

            IAsyncResult asyncResult = null;
            try
            {
                asyncResult = request.BeginGetResponse(AsyncPhoneWebResponseCallback, request);
            }
            catch (WebException)
            {
                return null;
            }

            // HACK: this is a fake behaviour, as Windows Phone 7 doesn't support synchronous web requests,
            // that's why the current thread must poll for result:
            while (!responseReceived)
            {
                System.Threading.Thread.Sleep(50);
            }

            return (HttpWebResponse)request.EndGetResponse(asyncResult);
#else
            return (HttpWebResponse)request.GetResponse();
#endif
        }

        /// <summary>
        /// Gets the request stream in synchronous way.
        /// </summary>
        private Stream GetRequestStream(HttpWebRequest request)
        {
#if WINDOWS_PHONE
            requestStreamReceived = false;
            IAsyncResult asyncResult = null;

            try
            {
                asyncResult = request.BeginGetRequestStream(AsyncPhoneWebRequestStreamCallback, request);
            }
            catch (WebException)
            {
                return null;
            }

            // HACK: take a look on comment of GetResponse() method...
            while (!requestStreamReceived)
            {
                System.Threading.Thread.Sleep(50);
            }

            return request.EndGetRequestStream(asyncResult);
#else
            return request.GetRequestStream();
#endif
        }

#if WINDOWS_PHONE

        private volatile bool responseReceived;
        private volatile bool requestStreamReceived;

        private void AsyncPhoneWebResponseCallback(IAsyncResult result)
        {
            // update indication that response is received:
            responseReceived = true;
        }

        private void AsyncPhoneWebRequestStreamCallback(IAsyncResult result)
        {
            // update indication that request stream is available to write:
            requestStreamReceived = true;
        }

#endif

        private Encoding GetResponseEncoding(HttpWebResponse response)
        {
            if (_responseEncoding != null)
                return _responseEncoding;

#if WINDOWS_PHONE
            string encodingName = response.Headers[HttpRequestHeader.ContentEncoding];
#else
            string encodingName = string.IsNullOrEmpty(response.ContentEncoding) ? response.CharacterSet : response.ContentEncoding;
#endif
            return (string.IsNullOrEmpty(encodingName) ? _defaultEncoding : Encoding.GetEncoding(encodingName));
        }

        /// <summary>
        /// Cancels current operation.
        /// Throws an exception when nothing is actually processed.
        /// </summary>
        public void Cancel()
        {
            HttpWebRequest request = Interlocked.Exchange(ref _request, null);

            if (request != null)
            {
                DebugLog.WriteCoreLine("Aborting HTTP request!");
                request.Abort();
            }
        }

        /// <summary>
        /// Gets an indication if there is an ongoing request.
        /// </summary>
        public bool IsActive
        {
            get { return _request != null; }
        }

        /// <summary>
        /// Gets or sets the content type of message sent.
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the content type accepted by data source.
        /// </summary>
        public string AcceptContentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the network login credentials.
        /// </summary>
        public NetworkCredential NetworkCredential
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timeout for next request (ms).
        /// </summary>
        public int Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user-agent name.
        /// </summary>
        public string UserAgent
        {
            get;
            set;
        }

        /// <summary>
        /// Adds or replaces additional header sent via this connection with next request.
        /// </summary>
        public void AddHeader(string name, string value)
        {
            if (_headers == null)
                _headers = new Dictionary<string, string>();

            if (_headers.ContainsKey(name))
                _headers[name] = value;
            else
                _headers.Add(name, value);
        }

        /// <summary>
        /// Removes header with specified name.
        /// </summary>
        public void RemoveHeader(string name)
        {
            if (_headers != null)
            {
                _headers.Remove(name);
            }
        }

        private void AsyncWebRequestStreamCallback(IAsyncResult asyncResult)
        {
            AsyncDataRequest asyncState = (AsyncDataRequest)asyncResult.AsyncState;

            // this method was called only, when data should be populated together with the request:
            if (asyncState == null || asyncState.Data == null || asyncState.Data.Length == 0)
                throw new InvalidOperationException("asyncResult");

            HttpWebRequest webRequest = asyncState.Request;
            string dataDescription = asyncState.DataDescription;
            DateTime now = DateTime.Now;

            if (WriteRequestData(webRequest, asyncResult, asyncState.Data))
                return;

            DebugLog.WriteCoreLine(string.Format(CultureInfo.InvariantCulture, "<--- Sending request (length: {0} bytes, at: {1}): {2} ({3})",
                                        dataDescription != null ? dataDescription.Length : 0, now, webRequest.RequestUri.AbsoluteUri, ContentType));
            DebugLog.WriteCoreLine(dataDescription ?? string.Empty);

            //////////////////////
            // Send request:
            try
            {
                webRequest.BeginGetResponse(AsyncWebResponseCallback, asyncState);
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.RequestCanceled)
                    ProcessResponse(webRequest, null, asyncState.ResponseType);
            }
        }

        private void AsyncWebResponseCallback(IAsyncResult asyncResult)
        {
            AsyncDataRequest asyncState = (AsyncDataRequest)asyncResult.AsyncState;
            HttpWebRequest request = asyncState.Request;
            HttpWebResponse response;
            bool canProcess = true;

            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asyncResult);
            }
            catch (WebException ex)
            {
                canProcess = ex.Status != WebExceptionStatus.RequestCanceled;
                response = (HttpWebResponse)ex.Response;
            }

            if (canProcess)
                ProcessResponse(request, response, asyncState.ResponseType);
        }

        /// <summary>
        /// Add new extensions to <see cref="HttpWebRequest"/> before sending to the server.
        /// </summary>
        protected virtual void AppendAdditionalInfo(HttpWebRequest request)
        {
        }

        /// <summary>
        /// Processes the content of response received in context of given request.
        /// </summary>
        protected void ProcessResponse(HttpWebRequest request, HttpWebResponse response, HttpDataSourceResponseType responseType)
        {
            // data reception failed or timeouted/cancelled?
            if (response == null)
            {
                ProcessFailedResponse(request);
                return;
            }

            ProcessSuccessfulResponse(request, response, responseType);
            response.Close();
        }

        private void ProcessSuccessfulResponse(HttpWebRequest request, HttpWebResponse response, HttpDataSourceResponseType responseType)
        {
            // process the data:
            HttpStatusCode responseStatusCode = response.StatusCode;
            string responseStatusDescription = string.IsNullOrEmpty(response.StatusDescription) ? response.StatusCode.ToString() : response.StatusDescription;
            string stringData;
            byte[] binaryData;
            Stream streamData;

            if (IsFailureCode(responseStatusCode))
            {
                DebugLog.WriteCoreLine(string.Format(CultureInfo.InvariantCulture, "---> Received response (length: {0} bytes, at: {1}, waiting: {2:F2} sec) with status: {3} ({4}, {5}, {6})",
                    GetContentLength(response), DateTime.Now, (DateTime.Now - new DateTime(long.Parse(request.Headers[HeaderSentAt]))).TotalSeconds,
                    response.StatusCode, (int)responseStatusCode, responseStatusDescription, response.ContentType));

                try
                {
                    ReadResponseData(response, responseType, out stringData, out binaryData, out streamData);
                    if (binaryData != null || streamData != null)
                        DebugLog.WriteCoreLine(BinaryContent);
                    else
                        DebugLog.WriteCoreLine(stringData ?? string.Empty);
                }
                catch
                {
                    stringData = null;
                    binaryData = null;
                    streamData = null;
                }

                if (TryCompleteRequest(request))
                    Event.Invoke(DataReceiveFailed, this, new HttpDataSourceEventArgs(this, responseStatusCode, responseStatusDescription, stringData, binaryData, streamData));
                else
                    DebugLog.WriteCoreLine("Ignoring reception due to previous request cancellation!");

                return;
            }

            try
            {
                ReadResponseData(response, responseType, out stringData, out binaryData, out streamData);

                long length = stringData != null ? stringData.Length : (binaryData != null ? binaryData.Length : GetContentLength(response));

                DebugLog.WriteCoreLine(string.Format(CultureInfo.InvariantCulture, "---> Received response (length: {0} bytes, at: {1}, waiting: {2:F2} sec) with status: {3} ({4}, {5}, {6})",
                    length, DateTime.Now, (DateTime.Now - new DateTime(long.Parse(request.Headers[HeaderSentAt]))).TotalSeconds,
                    response.StatusCode, (int)responseStatusCode, responseStatusDescription, response.ContentType));
                if (binaryData != null || streamData != null)
                    DebugLog.WriteCoreLine(BinaryContent);
                else
                    DebugLog.WriteCoreLine(stringData ?? string.Empty);
            }
            catch (Exception ex)
            {
                DebugLog.WriteCoreLine(string.Format("Response reception error: {0}", ex.Message));
                if (TryCompleteRequest(request))
                    Event.Invoke(DataReceiveFailed, this, new HttpDataSourceEventArgs(this, responseStatusCode, responseStatusDescription));
                else
                    DebugLog.WriteCoreLine("Ignoring reception due to previous request cancellation!");

                return;
            }

            if (TryCompleteRequest(request))
                Event.Invoke(DataReceived, this, new HttpDataSourceEventArgs(this, responseStatusCode, responseStatusDescription, stringData, binaryData, streamData));
            else
                DebugLog.WriteCoreLine("Ignoring reception due to previous request cancellation!");
        }

        private static bool IsFailureCode(HttpStatusCode statusCode)
        {
            return statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.Created && statusCode != HttpStatusCode.Accepted;
        }

        private void ProcessFailedResponse(HttpWebRequest request)
        {
            // release current request:
            if (TryCompleteRequest(request))
            {
                DebugLog.WriteCoreLine(string.Format(CultureInfo.InvariantCulture, "---> Response critical failure! Probably timed out or canceled (at: {0}).", DateTime.Now));
                Event.Invoke(DataReceiveFailed, this, new HttpDataSourceEventArgs(this, HttpStatusCode.ServiceUnavailable, null));
            }
            else
            {
                DebugLog.WriteCoreLine("Ignoring request failure due to previous request cancellation!");
            }
        }

        private static long GetContentLength(HttpWebResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            if (response.ContentLength >= 0)
                return response.ContentLength;

            if (response.Headers["Content-Length"] != null)
            {
                try
                {
                    return long.Parse(response.Headers["Content-Length"], NumberStyles.Integer, CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    DebugLog.WriteCoreException(ex);
                    throw;
                }
            }

            return -1L;
        }

        private void ReadResponseData(HttpWebResponse response, HttpDataSourceResponseType responseType, out string stringData, out byte[] binaryData, out Stream streamData)
        {
            stringData = null;
            binaryData = null;
            streamData = null;

            if (responseType == HttpDataSourceResponseType.AsString)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    stringData = ReadResponseStringData(responseStream, GetContentLength(response), GetResponseEncoding(response));
                }

                return;
            }

            if (responseType == HttpDataSourceResponseType.AsBinary)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    binaryData = ReadResponseBinaryData(responseStream, (int)GetContentLength(response));
                }
                return;
            }

            if (responseType == HttpDataSourceResponseType.AsRawStream)
            {
                streamData = response.GetResponseStream();
                return;
            }

            throw new ArgumentOutOfRangeException("responseType", "Not supported response type");
        }

        private bool WriteRequestData(HttpWebRequest request, IAsyncResult asyncResult, byte[] data)
        {
            Stream requestStream = null;

            try
            {
                requestStream = asyncResult != null ? request.EndGetRequestStream(asyncResult) : GetRequestStream(request);

                if (requestStream == null)
                    return true;

                WriteRequestData(requestStream, data);
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.RequestCanceled)
                    Event.Invoke(DataReceiveFailed, this, new HttpDataSourceEventArgs(this, GetHttpStatusCode(ex.Status), ex.Message));
                TryCompleteRequest(request);

                return true;
            }
            finally
            {
                if (requestStream != null)
                    requestStream.Close();
            }

            return false;
        }

        private byte[] GetSerializedData(string data)
        {
            return _defaultEncoding.GetBytes(data);
        }

        private long GetSerializedDataLength(byte[] data)
        {
            if (data == null)
                return 0;

            return data.Length;
        }

        /// <summary>
        /// Reads data from given source.
        /// </summary>
        protected static string ReadToEnd(StreamReader reader, long maxLength)
        {
#if WINDOWS_PHONE
            // HACK: Some network streams on Windows Phone 7 have 'seek-support' non-implemented issue.
            // To fix it, try to read data in chunks manually:

            StringBuilder result = maxLength > 0 ? new StringBuilder((int)maxLength): new StringBuilder();
            char[] buffer = new char[1024];
            long totalRead = 0;
            int read;

            do
            {
                read = reader.Read(buffer, 0, buffer.Length);

                if (read == 0)
                    break;

                totalRead += read;
                result.Append(buffer, 0, read);
            }
            while (totalRead < maxLength);

            return result.ToString();
#else
            return reader.ReadToEnd();
#endif
        }

        /// <summary>
        /// Reads data from given source.
        /// </summary>
        protected static byte[] ReadToEnd(BinaryReader reader, int expectedLength)
        {
            const int DefaultChunk = 4 * 1024;
            byte[] result = new byte[expectedLength];
            int totalRead = 0;
            int read;
            int toRead;

            do
            {
                toRead = Math.Min(expectedLength - totalRead, DefaultChunk);
                read = reader.Read(result, totalRead, toRead);

                if (read == 0)
                    break;

                totalRead += read;
            }
            while (totalRead < expectedLength);

            if (totalRead != expectedLength)
                throw new InvalidOperationException("Invalid number of bytes read");

            return result;
        }

        #region Virtual Methods

        /// <summary>
        /// Writes data to the server as part of the request stream.
        /// </summary>
        protected virtual void WriteRequestData(Stream output, byte[] data)
        {
            output.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Reads data received from server and interprets according to given encoding.
        /// </summary>
        protected virtual byte[] ReadResponseBinaryData(Stream input, int contentLength)
        {
            if (input == null || !input.CanRead)
                return null;

            using (BinaryReader reader = new BinaryReader(input))
            {
                return ReadToEnd(reader, contentLength);
            }
        }

        /// <summary>
        /// Reads data received from server and interprets according to given encoding.
        /// </summary>
        protected virtual string ReadResponseStringData(Stream input, long contentLength, Encoding encoding)
        {
            if (input == null || !input.CanRead)
                return null;

            using (StreamReader reader = new StreamReader(input, encoding))
            {
                return ReadToEnd(reader, contentLength);
            }
        }

        #endregion
    }
}
