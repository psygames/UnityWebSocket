using System;

namespace UnityWebSocket
{
    public interface IWebSocket
    {
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
        void ConnectAsync(string address);

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
        void CloseAsync();

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
        void SendAsync(byte[] data, Action<bool> completed);

        /// <summary>
        /// Sends the specified data using the WebSocket connection.
        /// </summary>
        /// <param name="text">
        /// A <see cref="string"/> that represents the text data to send.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The current state of the connection is not Open.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="text"/> could not be UTF-8-encoded.
        /// </exception>
        void SendAsync(string text, Action<bool> completed);

        /// <summary>
        /// get the address which to connect.
        /// </summary>
        string Address { get; }

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
        WebSocketState ReadyState { get; }

        /// <summary>
        /// Occurs when the WebSocket connection has been established.
        /// </summary>
        event EventHandler OnOpen;

        /// <summary>
        /// Occurs when the WebSocket connection has been closed.
        /// </summary>
        event EventHandler<CloseEventArgs> OnClose;

        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> gets an error.
        /// </summary>
        event EventHandler<ErrorEventArgs> OnError;

        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> receives a message.
        /// </summary>
        event EventHandler<MessageEventArgs> OnMessage;
    }
}