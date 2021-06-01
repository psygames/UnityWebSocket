// ---------------------------------------------------------------------
// Copyright 2018 David Haig
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
// ---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ninja.WebSockets.Exceptions;
using Ninja.WebSockets.Internal;

namespace Ninja.WebSockets
{
    /// <summary>
    /// Web socket client factory used to open web socket client connections
    /// </summary>
    public class WebSocketClientFactory : IWebSocketClientFactory
    {
        private readonly Func<MemoryStream> _bufferFactory;
        private readonly IBufferPool _bufferPool;

        /// <summary>
        /// Initialises a new instance of the WebSocketClientFactory class without caring about internal buffers
        /// </summary>
        public WebSocketClientFactory()
        {
            _bufferPool = new BufferPool();
            _bufferFactory = _bufferPool.GetBuffer;
        }

        /// <summary>
        /// Initialises a new instance of the WebSocketClientFactory class with control over internal buffer creation
        /// </summary>
        /// <param name="bufferFactory">Used to get a memory stream. Feel free to implement your own buffer pool. MemoryStreams will be disposed when no longer needed and can be returned to the pool.</param>
        public WebSocketClientFactory(Func<MemoryStream> bufferFactory)
        {
            _bufferFactory = bufferFactory;
        }

        /// <summary>
        /// Connect with default options
        /// </summary>
        /// <param name="uri">The WebSocket uri to connect to (e.g. ws://example.com or wss://example.com for SSL)</param>
        /// <param name="token">The optional cancellation token</param>
        /// <returns>A connected web socket instance</returns>
        public async Task<WebSocket> ConnectAsync(Uri uri, CancellationToken token = default(CancellationToken))
        {
            return await ConnectAsync(uri, new WebSocketClientOptions(), token);
        }

        /// <summary>
        /// Connect with options specified
        /// </summary>
        /// <param name="uri">The WebSocket uri to connect to (e.g. ws://example.com or wss://example.com for SSL)</param>
        /// <param name="options">The WebSocket client options</param>
        /// <param name="token">The optional cancellation token</param>
        /// <returns>A connected web socket instance</returns>
        public async Task<WebSocket> ConnectAsync(Uri uri, WebSocketClientOptions options, CancellationToken token = default(CancellationToken))
        {
            Guid guid = Guid.NewGuid();
            string host = uri.Host;
            int port = uri.Port;
            string uriScheme = uri.Scheme.ToLower();
            bool useSsl = uriScheme == "wss" || uriScheme == "https";
            Stream stream = await GetStream(guid, useSsl, options.NoDelay, host, port, token);
            return await PerformHandshake(guid, uri, stream, options, token);
        }

        /// <summary>
        /// Connect with a stream that has already been opened and HTTP websocket upgrade request sent
        /// This function will check the handshake response from the server and proceed if successful
        /// Use this function if you have specific requirements to open a conenction like using special http headers and cookies
        /// You will have to build your own HTTP websocket upgrade request
        /// You may not even choose to use TCP/IP and this function will allow you to do that
        /// </summary>
        /// <param name="responseStream">The full duplex response stream from the server</param>
        /// <param name="secWebSocketKey">The secWebSocketKey you used in the handshake request</param>
        /// <param name="options">The WebSocket client options</param>
        /// <param name="token">The optional cancellation token</param>
        /// <returns></returns>
        public async Task<WebSocket> ConnectAsync(Stream responseStream, string secWebSocketKey, WebSocketClientOptions options, CancellationToken token = default(CancellationToken))
        {
            Guid guid = Guid.NewGuid();
            return await ConnectAsync(guid, responseStream, secWebSocketKey, options.KeepAliveInterval, options.SecWebSocketExtensions, options.IncludeExceptionInCloseResponse, token);
        }

        private async Task<WebSocket> ConnectAsync(Guid guid, Stream responseStream, string secWebSocketKey, TimeSpan keepAliveInterval, string secWebSocketExtensions, bool includeExceptionInCloseResponse, CancellationToken token)
        {
            Events.Log.ReadingHttpResponse(guid);
            string response = string.Empty;

            try
            {
                response = await HttpHelper.ReadHttpHeaderAsync(responseStream, token);
            }
            catch (Exception ex)
            {
                Events.Log.ReadHttpResponseError(guid, ex.ToString());
                throw new WebSocketHandshakeFailedException("Handshake unexpected failure", ex);
            }

            ThrowIfInvalidResponseCode(response);
            ThrowIfInvalidAcceptString(guid, response, secWebSocketKey);
            string subProtocol = GetSubProtocolFromHeader(response);
            return new WebSocketImplementation(guid, _bufferFactory, responseStream, keepAliveInterval, secWebSocketExtensions, includeExceptionInCloseResponse, true, subProtocol);
        }

        private string GetSubProtocolFromHeader(string response)
        {
            // make sure we escape the accept string which could contain special regex characters
            string regexPattern = "Sec-WebSocket-Protocol: (.*)";
            Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(response);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return null;            
        }

        private void ThrowIfInvalidAcceptString(Guid guid, string response, string secWebSocketKey)
        {
            // make sure we escape the accept string which could contain special regex characters
            string regexPattern = "Sec-WebSocket-Accept: (.*)";            
            Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            string actualAcceptString = regex.Match(response).Groups[1].Value.Trim();

            // check the accept string
            string expectedAcceptString = HttpHelper.ComputeSocketAcceptString(secWebSocketKey);
            if (expectedAcceptString != actualAcceptString)
            {
                string warning = string.Format($"Handshake failed because the accept string from the server '{expectedAcceptString}' was not the expected string '{actualAcceptString}'");
                Events.Log.HandshakeFailure(guid, warning);
                throw new WebSocketHandshakeFailedException(warning);
            }
            else
            {
                Events.Log.ClientHandshakeSuccess(guid);
            }
        }

        private void ThrowIfInvalidResponseCode(string responseHeader)
        {
            string responseCode = HttpHelper.ReadHttpResponseCode(responseHeader);
            if (responseCode == null)
            {
                throw new InvalidHttpResponseCodeException(null, null, responseHeader);
            }

            if (!responseCode.StartsWith("101 ", StringComparison.InvariantCultureIgnoreCase))
            {
                string[] lines = responseHeader.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++)
                {
                    // if there is more to the message than just the header
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        StringBuilder builder = new StringBuilder();
                        for (int j = i + 1; j < lines.Length - 1; j++)
                        {
                            builder.AppendLine(lines[j]);
                        }

                        string responseDetails = builder.ToString();
                        throw new InvalidHttpResponseCodeException(responseCode, responseDetails, responseHeader);
                    }
                }
            }
        }

        /// <summary>
        /// Override this if you need more fine grained control over the TLS handshake like setting the SslProtocol or adding a client certificate
        /// </summary>
        protected virtual void TlsAuthenticateAsClient(SslStream sslStream, string host)
        {
            sslStream.AuthenticateAsClient(host, null, SslProtocols.Tls12, true);
        }

        /// <summary>
        /// Override this if you need more control over how the stream used for the websocket is created. It does not event need to be a TCP stream
        /// </summary>
        /// <param name="loggingGuid">For logging purposes only</param>
        /// <param name="isSecure">Make a secure connection</param>
        /// <param name="noDelay">Set to true to send a message immediately with the least amount of latency (typical usage for chat)</param>
        /// <param name="host">The destination host (can be an IP address)</param>
        /// <param name="port">The destination port</param>
        /// <param name="cancellationToken">Used to cancel the request</param>
        /// <returns>A connected and open stream</returns>
        protected virtual async Task<Stream> GetStream(Guid loggingGuid, bool isSecure, bool noDelay, string host, int port, CancellationToken cancellationToken)
        {
            var tcpClient = new TcpClient();
            tcpClient.NoDelay = noDelay;
            IPAddress ipAddress;
            if (IPAddress.TryParse(host, out ipAddress))
            {
                Events.Log.ClientConnectingToIpAddress(loggingGuid, ipAddress.ToString(), port);
                await tcpClient.ConnectAsync(ipAddress, port);
            }
            else
            {
                Events.Log.ClientConnectingToHost(loggingGuid, host, port);
                await tcpClient.ConnectAsync(host, port);
            }

            cancellationToken.ThrowIfCancellationRequested();
            Stream stream = tcpClient.GetStream();

            if (isSecure)
            {
                SslStream sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                Events.Log.AttemtingToSecureSslConnection(loggingGuid);

                // This will throw an AuthenticationException if the certificate is not valid
                TlsAuthenticateAsClient(sslStream, host);
                Events.Log.ConnectionSecured(loggingGuid);
                return sslStream;
            }
            else
            {
                Events.Log.ConnectionNotSecure(loggingGuid);
                return stream;
            }
        }

        /// <summary>
        /// Invoked by the RemoteCertificateValidationDelegate
        /// If you want to ignore certificate errors (for debugging) then return true
        /// </summary>
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            Events.Log.SslCertificateError(sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        private static string GetAdditionalHeaders(Dictionary<string, string> additionalHeaders)
        {
            if (additionalHeaders == null || additionalHeaders.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                foreach (KeyValuePair<string, string> pair in additionalHeaders)
                {
                    builder.Append($"{pair.Key}: {pair.Value}\r\n");
                }

                return builder.ToString();
            }
        }

        private async Task<WebSocket> PerformHandshake(Guid guid, Uri uri, Stream stream, WebSocketClientOptions options, CancellationToken token)
        {
            Random rand = new Random();
            byte[] keyAsBytes = new byte[16];
            rand.NextBytes(keyAsBytes);
            string secWebSocketKey = Convert.ToBase64String(keyAsBytes);
            string additionalHeaders = GetAdditionalHeaders(options.AdditionalHttpHeaders);
            string handshakeHttpRequest = $"GET {uri.PathAndQuery} HTTP/1.1\r\n" +
                                          $"Host: {uri.Host}:{uri.Port}\r\n" +
                                           "Upgrade: websocket\r\n" +
                                           "Connection: Upgrade\r\n" +
                                          $"Sec-WebSocket-Key: {secWebSocketKey}\r\n" +
                                          $"Origin: http://{uri.Host}:{uri.Port}\r\n" +
                                          $"Sec-WebSocket-Protocol: {options.SecWebSocketProtocol}\r\n" +
                                          additionalHeaders +
                                           "Sec-WebSocket-Version: 13\r\n\r\n";

            byte[] httpRequest = Encoding.UTF8.GetBytes(handshakeHttpRequest);
            stream.Write(httpRequest, 0, httpRequest.Length);
            Events.Log.HandshakeSent(guid, handshakeHttpRequest);
            return await ConnectAsync(stream, secWebSocketKey, options, token);
        }
    }
}
