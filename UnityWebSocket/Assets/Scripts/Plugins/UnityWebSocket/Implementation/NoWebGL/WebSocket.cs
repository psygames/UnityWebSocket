#if !NET_LEGACY
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;


namespace UnityWebSocket.NoWebGL
{
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
                    case System.Net.WebSockets.WebSocketState.Closed:
                    case System.Net.WebSockets.WebSocketState.None:
                        return WebSocketState.Closed;
                    case System.Net.WebSockets.WebSocketState.CloseReceived:
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

        public event EventHandler<OpenEventArgs> OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;

        private ClientWebSocket socket;
        private CancellationTokenSource cts;
        private readonly CancellationTokenSource closeCts = new CancellationTokenSource();
        private bool IsCtsCancel { get { return cts == null || cts.IsCancellationRequested; } }

        private void TaskNew(Func<Task> function, CancellationTokenSource _cts = null)
        {
            if (_cts == null)
                _cts = cts;
            Task.Factory.StartNew(function, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void ConnectAsync(string address)
        {
            if (cts != null || socket != null)
            {
                HandleError(new Exception("socket is busy."));
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
                HandleOpen();
                TaskNew(SendThread);
                await ReceiveThread();
            }
            catch (Exception e)
            {
                HandleError(e);
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
                if (!IsCtsCancel)
                {
                    cts.Cancel();
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        private async Task WaitForClose()
        {
            try
            {
                while (!IsCtsCancel || isSendThreadRunning || isReceiveThreadRunning)
                {
                    await Task.Delay(1);
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
            finally
            {
                cts.Dispose();
                socket.Dispose();
                cts = null;
                socket = null;
            }
        }

        private readonly Queue<SendBuffer> sendCaches = new Queue<SendBuffer>();

        class SendBuffer
        {
            public ArraySegment<byte> buffer;
            public Action callback;
            public WebSocketMessageType type;
        }

        public void SendAsync(byte[] data, Action completed = null)
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

        public void SendAsync(string text, Action completed = null)
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
            try
            {
                isSendThreadRunning = true;
                while (!IsCtsCancel)
                {
                    SendBuffer buffer = null;
                    if (sendCaches.Count <= 0)
                    {
                        await Task.Delay(1);
                        continue;
                    }
                    lock (sendCaches)
                    {
                        buffer = sendCaches.Dequeue();
                    }
                    if (!IsCtsCancel)
                    {
                        await socket.SendAsync(buffer.buffer, buffer.type, true, cts.Token);
                        buffer.callback?.Invoke();
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
            finally
            {
                isSendThreadRunning = false;
            }
        }

        private bool isReceiveThreadRunning;
        private async Task ReceiveThread()
        {
            try
            {
                isReceiveThreadRunning = true;

                var buffer = new byte[1024 * 1024];
                var bufferCount = 0;
                var segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
                while (!IsCtsCancel)
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
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            HandleMessage(Opcode.Binary, data);
                            break;
                        case WebSocketMessageType.Text:
                            HandleMessage(Opcode.Text, data);
                            break;
                        case WebSocketMessageType.Close:
                            cts.Cancel();
                            TaskNew(WaitForClose, closeCts);
                            HandleClose((ushort)result.CloseStatus, result.CloseStatusDescription);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
            finally
            {
                isReceiveThreadRunning = false;
            }
        }


        private void HandleOpen()
        {
            OnOpen?.Invoke(this, new OpenEventArgs());
        }

        private void HandleMessage(Opcode opcode, byte[] rawData)
        {
            OnMessage?.Invoke(this, new MessageEventArgs(opcode, rawData));
        }

        private void HandleClose(ushort code, string reason)
        {
            OnClose?.Invoke(this, new CloseEventArgs(code, reason));
        }

        private void HandleError(Exception exception)
        {
            UnityEngine.Debug.LogError(exception.Message + "\n" + exception.StackTrace);
            OnError?.Invoke(this, new ErrorEventArgs(exception.Message));
        }
    }
}
#endif
