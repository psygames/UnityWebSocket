using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityWebSocket
{
    /// <summary>
    /// For All Platform
    /// </summary>
    public class WebSocket : IWebSocket
    {
        #region Public Events
        /// <summary>
        /// Occurs when the WebSocket connection has been established.
        /// </summary>
        public event EventHandler onOpen;

        /// <summary>
        /// Occurs when the WebSocket connection has been closed.
        /// </summary>
        public event EventHandler<CloseEventArgs> onClose;

        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> gets an error.
        /// </summary>
        public event EventHandler<ErrorEventArgs> onError;

        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> receives a message.
        /// </summary>
        public event EventHandler<MessageEventArgs> onMessage;
        #endregion

#if UNITY_WEBGL && !UNITY_EDITOR
        public string address { get { return m_rawSocket.address; } }
        public WebSocketState readyState { get { return m_rawSocket.readyState; } }

        WebSocketJslib.WebSocket m_rawSocket = null;
        public WebSocket(string address)
        {
            m_rawSocket = new WebSocketJslib.WebSocket(address);
            m_rawSocket.onOpen += (o, e) =>
            {
                if (onOpen != null)
                    onOpen(this, EventArgs.Empty);
            };
            m_rawSocket.onClose += (o, e) =>
            {
                if (onClose != null)
                    onClose(this, new CloseEventArgs(e.Code, e.Reason, e.WasClean));
            };
            m_rawSocket.onError += (o, e) =>
            {
                if (onError != null)
                    onError(this, new ErrorEventArgs(e.Message, e.Exception));
            };
            m_rawSocket.onMessage += (o, e) =>
            {
                if (onMessage != null)
                    onMessage(this, new MessageEventArgs((Opcode)e.Opcode, e.RawData));
            };
        }

        public void Connect()
        {
            m_rawSocket.Connect();
        }

        public void Send(byte[] data)
        {
            m_rawSocket.Send(data);
        }

        public void Send(string data)
        {
            m_rawSocket.Send(data);
        }

        public void Close()
        {
            m_rawSocket.Close();
        }

        public void ConnectAsync()
        {
            Connect();
        }

        public void CloseAsync()
        {
            Close();
        }

        public void SendAsync(byte[] data, Action<bool> completed)
        {
            Send(data);
            completed(true);
        }
#else
        /// <summary>
        /// get the address which to connect.
        /// </summary>
        public string address { get { return m_rawSocket.Url.AbsoluteUri; } }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <value>
        ///   <para>
        ///   One of the <see cref="WebSocketState"/> enum values.
        ///   </para>
        ///   <para>
        ///   It indicates the current state of the connection.
        ///   </para>
        ///   <para>
        ///   The default value is <see cref="WebSocketState.Connecting"/>.
        ///   </para>
        /// </value>
        public WebSocketState readyState { get { return (WebSocketState)m_rawSocket.ReadyState; } }

        private WebSocketSharp.WebSocket m_rawSocket = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket"/> class with
        /// <paramref name="url"/> and optionally <paramref name="protocols"/>.
        /// </summary>
        /// <param name="url">
        ///   <para>
        ///   A <see cref="string"/> that specifies the URL to which to connect.
        ///   </para>
        ///   <para>
        ///   The scheme of the URL must be ws or wss.
        ///   </para>
        ///   <para>
        ///   The new instance uses a secure connection if the scheme is wss.
        ///   </para>
        /// </param>
        /// <param name="protocols">
        ///   <para>
        ///   An array of <see cref="string"/> that specifies the names of
        ///   the subprotocols if necessary.
        ///   </para>
        ///   <para>
        ///   Each value of the array must be a token defined in
        ///   <see href="http://tools.ietf.org/html/rfc2616#section-2.2">
        ///   RFC 2616</see>.
        ///   </para>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="url"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   <paramref name="url"/> is an empty string.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   <paramref name="url"/> is an invalid WebSocket URL string.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   <paramref name="protocols"/> contains a value that is not a token.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   <paramref name="protocols"/> contains a value twice.
        ///   </para>
        /// </exception> 
        public WebSocket(string address)
        {
            m_rawSocket = new WebSocketSharp.WebSocket(address);
            m_rawSocket.OnOpen += (o, e) =>
            {
                if (onOpen != null)
                    onOpen(this, EventArgs.Empty);
            };
            m_rawSocket.OnClose += (o, e) =>
            {
                if (onClose != null)
                    onClose(this, new CloseEventArgs(e.Code, e.Reason, e.WasClean));
            };
            m_rawSocket.OnError += (o, e) =>
            {
                if (onError != null)
                    onError(this, new ErrorEventArgs(e.Message, e.Exception));
            };
            m_rawSocket.OnMessage += (o, e) =>
            {
                if (onMessage != null)
                    onMessage(this, new MessageEventArgs((Opcode)e.Opcode, e.RawData));
            };

            if (m_rawSocket.IsSecure)
            {
                m_rawSocket.SslConfiguration.EnabledSslProtocols = (System.Security.Authentication.SslProtocols)((int)m_rawSocket.SslConfiguration.EnabledSslProtocols | 192 | 768 | 3072);
            }
        }

        /// <summary>
        /// Establishes a connection.
        /// </summary>
        /// <remarks>
        /// This method does nothing if the connection has already been established.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   <para>
        ///   This instance is not a client.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   The close process is in progress.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   A series of reconnecting has failed.
        ///   </para>
        /// </exception>
        public void Connect()
        {
            m_rawSocket.Connect();
        }

        /// <summary>
        /// Sends the specified data using the WebSocket connection.
        /// </summary>
        /// <param name="data">
        /// An array of <see cref="byte"/> that represents the binary data to send.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the connection is not Open.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="data"/> is <see langword="null"/>.
        /// </exception>
        public void Send(byte[] data)
        {
            m_rawSocket.Send(data);
        }

        /// <summary>
        /// Sends the specified data using the WebSocket connection.
        /// </summary>
        /// <param name="data">
        /// A <see cref="string"/> that represents the text data to send.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the connection is not Open.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="data"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="data"/> could not be UTF-8-encoded.
        /// </exception>
        public void Send(string data)
        {
            m_rawSocket.Send(data);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <remarks>
        /// This method does nothing if the current state of the connection is
        /// Closing or Closed.
        /// </remarks>
        public void Close()
        {
            m_rawSocket.Close();
        }

        /// <summary>
        /// Establishes a connection asynchronously.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///   This method does not wait for the connect process to be complete.
        ///   </para>
        ///   <para>
        ///   This method does nothing if the connection has already been
        ///   established.
        ///   </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   <para>
        ///   This instance is not a client.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   The close process is in progress.
        ///   </para>
        ///   <para>
        ///   -or-
        ///   </para>
        ///   <para>
        ///   A series of reconnecting has failed.
        ///   </para>
        /// </exception>
        public void ConnectAsync()
        {
            m_rawSocket.ConnectAsync();
        }

        /// <summary>
        /// Closes the connection asynchronously.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///   This method does not wait for the close to be complete.
        ///   </para>
        ///   <para>
        ///   This method does nothing if the current state of the connection is
        ///   Closing or Closed.
        ///   </para>
        /// </remarks>
        public void CloseAsync()
        {
            m_rawSocket.CloseAsync();
        }

        /// <summary>
        /// Sends the specified data asynchronously using the WebSocket connection.
        /// </summary>
        /// <remarks>
        /// This method does not wait for the send to be complete.
        /// </remarks>
        /// <param name="data">
        /// An array of <see cref="byte"/> that represents the binary data to send.
        /// </param>
        /// <param name="completed">
        ///   <para>
        ///   An <c>Action&lt;bool&gt;</c> delegate or <see langword="null"/>
        ///   if not needed.
        ///   </para>
        ///   <para>
        ///   The delegate invokes the method called when the send is complete.
        ///   </para>
        ///   <para>
        ///   <c>true</c> is passed to the method if the send has done with
        ///   no error; otherwise, <c>false</c>.
        ///   </para>
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the connection is not Open.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="data"/> is <see langword="null"/>.
        /// </exception>
        public void SendAsync(byte[] data, Action<bool> completed)
        {
            m_rawSocket.SendAsync(data, completed);
        }
#endif

    }
}
