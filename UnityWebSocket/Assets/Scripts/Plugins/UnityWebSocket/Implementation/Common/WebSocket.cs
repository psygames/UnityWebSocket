#if !NET_LEGACY
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;


namespace UnityWebSocket.Common
{
    /// <summary>
    /// <para>WebSocket indicate a network connection.</para>
    /// <para>It can be connecting, connected, closing or closed state. </para>
    /// <para>You can send and receive messages by using it.</para>
    /// <para>Register receive callback for handling received messages.</para>
    /// <para>WebSocket 表示一个网络连接，</para>
    /// <para>它可以是 connecting connected closing closed 状态，</para>
    /// <para>可以发送和接收消息，</para>
    /// <para>接收消息处理的地方注册消息回调即可。</para>
    /// </summary>
    public class WebSocket : IWebSocket
    {
        public string Address { get; private set; }

        public WebSocketState ReadyState
        {
            get
            {
                if (socket == null)
                    return WebSocketState.Closed;
                switch (socket.State)
                {
                    case System.Net.WebSockets.WebSocketState.Aborted:
                    case System.Net.WebSockets.WebSocketState.Closed:
                    case System.Net.WebSockets.WebSocketState.CloseReceived:
                    case System.Net.WebSockets.WebSocketState.None:
                        return WebSocketState.Closed;
                    case System.Net.WebSockets.WebSocketState.CloseSent:
                        return WebSocketState.Closing;
                    case System.Net.WebSockets.WebSocketState.Connecting:
                        return WebSocketState.Connecting;
                    case System.Net.WebSockets.WebSocketState.Open:
                        return WebSocketState.Open;
                }
                return WebSocketState.Closed;
            }
        }

        public event EventHandler OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;

        private ClientWebSocket socket;
        private CancellationTokenSource cts;

        private void TaskNew(Func<Task> function)
        {
            Task.Factory.StartNew(function, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void ConnectAsync(string address)
        {
            if (cts != null || socket != null)
            {
                HandleOnError("socket is busy.");
                return;
            }
            this.Address = address;
            var uri = new Uri(Address);
            cts = new CancellationTokenSource();
            socket = new ClientWebSocket();
            TaskNew(ConnectThread);
        }


        private async Task ConnectThread()
        {
            try
            {
                var uri = new Uri(Address);
                await socket.ConnectAsync(uri, cts.Token);
                HandleOnOpen();
                TaskNew(SendThread);
                await ReceiveThread();
            }
            catch (Exception e)
            {
                HandleOnError(e.Message);
            }
        }

        public void CloseAsync()
        {
            TaskNew(CloseThread);
        }

        private async Task CloseThread()
        {
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", cts.Token);
                while (!cts.IsCancellationRequested || isSendThreadRunning || isReceiveThreadRunning)
                {
                    Thread.Sleep(1);
                }
                cts.Dispose();
                socket.Dispose();
                cts = null;
                socket = null;
            }
            catch (Exception e)
            {
                HandleOnError(e.Message);
            }
        }

        private Queue<SendBuffer> sendCaches = new Queue<SendBuffer>();

        class SendBuffer
        {
            public ArraySegment<byte> buffer;
            public Action<bool> callback;
            public WebSocketMessageType type;
        }

        public void SendAsync(byte[] data, Action<bool> completed)
        {
            var sendBuffer = new SendBuffer
            {
                buffer = new ArraySegment<byte>(data),
                callback = completed,
                type = WebSocketMessageType.Binary
            };
            lock (sendCaches)
            {
                sendCaches.Enqueue(sendBuffer);
            }
        }

        public void SendAsync(string text, Action<bool> completed)
        {
            var data = Encoding.UTF8.GetBytes(text);
            var sendBuffer = new SendBuffer
            {
                buffer = new ArraySegment<byte>(data),
                callback = completed,
                type = WebSocketMessageType.Text
            };
            lock (sendCaches)
            {
                sendCaches.Enqueue(sendBuffer);
            }
        }

        private bool isSendThreadRunning;
        private async Task SendThread()
        {
            isSendThreadRunning = true;
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    SendBuffer buffer = null;
                    if (sendCaches.Count <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    lock (sendCaches)
                    {
                        buffer = sendCaches.Dequeue();
                    }
                    await socket.SendAsync(buffer.buffer, buffer.type, true, cts.Token);
                    buffer.callback?.Invoke(true);
                }
            }
            catch (Exception e)
            {
                HandleOnError(e.Message);
            }

            isSendThreadRunning = false;
        }

        private bool isReceiveThreadRunning;
        private async Task ReceiveThread()
        {
            isReceiveThreadRunning = true;

            var buffer = new byte[1024 * 1024];
            var bufferCount = 0;
            var segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
            while (!cts.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(segment, cts.Token);
                bufferCount += result.Count;
                segment = new ArraySegment<byte>(buffer, bufferCount, buffer.Length - bufferCount);

                if (!result.EndOfMessage)
                    continue;

                byte[] data = new byte[bufferCount];
                for (int i = 0; i < bufferCount; i++)
                {
                    data[i] = buffer[i];
                }

                bufferCount = 0;
                segment = new ArraySegment<byte>(buffer, 0, buffer.Length);

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    HandleOnMessage(Opcode.Binary, data);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    HandleOnMessage(Opcode.Text, data);
                }
                else
                {
                    cts.Cancel();
                    HandleOnClose((ushort)result.CloseStatus, result.CloseStatusDescription, true);
                }
            }

            isReceiveThreadRunning = false;
        }


        private void HandleOnOpen()
        {
            OnOpen?.Invoke(this, EventArgs.Empty);
        }

        private void HandleOnMessage(Opcode opcode, byte[] rawData)
        {
            OnMessage?.Invoke(this, new MessageEventArgs(opcode, rawData));
        }

        private void HandleOnClose(ushort code, string reason, bool wasClean)
        {
            OnClose?.Invoke(this, new CloseEventArgs(code, reason, wasClean));
        }

        private void HandleOnError(string msg)
        {
            OnError?.Invoke(this, new ErrorEventArgs(msg));
        }
    }
}
#endif
