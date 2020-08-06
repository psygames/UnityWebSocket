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
        private bool IsCtsCancel { get { return cts == null || cts.IsCancellationRequested; } }

        public WebSocket(string address)
        {
            this.Address = address;
        }

        public void ConnectAsync()
        {
            if (cts != null || socket != null)
            {
                HandleError(new Exception("socket is busy."));
                return;
            }
            cts = new CancellationTokenSource();
            socket = new ClientWebSocket();
            Task.Run(ConnectThread);
        }

        public void CloseAsync()
        {
            Task.Run(CloseThread);
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

        private async Task ConnectThread()
        {
            // UnityEngine.Debug.Log("Connect Thread Start ...");

            try
            {
                var uri = new Uri(Address);
                await socket.ConnectAsync(uri, cts.Token);
            }
            catch (Exception e)
            {
                HandleError(e);
                HandleClose((ushort)CloseStatusCode.Abnormal, e.Message);
                SocketDispose();
                return;
            }

            try
            {
                LongRunningTask(SendThread);
                LongRunningTask(ReceiveThread);
            }
            catch (Exception e)
            {
                HandleError(e);
            }

            HandleOpen();

            // UnityEngine.Debug.Log("Connect Thread Stop !");
        }

        private async void CloseThread()
        {
            // UnityEngine.Debug.Log("Close Thread Start ...");

            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", cts.Token);
            }
            catch { }

            // UnityEngine.Debug.Log("Close Thread Stop !");
        }

        private async void DisposeThread()
        {
            // UnityEngine.Debug.Log("Dispose Thread Start ...");

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
                SocketDispose();
            }

            // UnityEngine.Debug.Log("Dispose Thread Stop !");
        }

        private bool isSendThreadRunning;
        private async Task SendThread()
        {
            // UnityEngine.Debug.Log("Send Thread Start ...");

            try
            {
                isSendThreadRunning = true;
                SendBuffer buffer = null;
                while (!IsCtsCancel)
                {
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

            // UnityEngine.Debug.Log("Send Thread Stop !");
        }

        private bool isReceiveThreadRunning;
        private async Task ReceiveThread()
        {
            // UnityEngine.Debug.Log("Receive Thread Start ...");

            string closeReason = "";
            ushort closeCode = 0;
            bool isClosed = false;
            try
            {
                isReceiveThreadRunning = true;

                var buffer = new byte[1024 * 1024];
                var bufferCount = 0;
                var segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
                while (!IsCtsCancel && !isClosed)
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
                            isClosed = true;
                            closeCode = (ushort)result.CloseStatus;
                            closeReason = result.CloseStatusDescription;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(e);
                closeCode = (ushort)CloseStatusCode.Abnormal;
                closeReason = e.Message;
            }
            finally
            {
                isReceiveThreadRunning = false;
            }

            cts.Cancel();
            await Task.Run(DisposeThread);
            HandleClose(closeCode, closeReason);

            // UnityEngine.Debug.Log("Receive Thread Stop !");
        }

        private void LongRunningTask(Func<Task> function)
        {
            Task.Factory.StartNew(function, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void SocketDispose()
        {
            cts.Dispose();
            socket.Dispose();
            cts = null;
            socket = null;
        }

        private readonly Queue<SendBuffer> sendCaches = new Queue<SendBuffer>();

        class SendBuffer
        {
            public ArraySegment<byte> buffer;
            public Action callback;
            public WebSocketMessageType type;
        }

        private void HandleOpen()
        {
            // UnityEngine.Debug.Log("OnOpen");
            try
            {
                OnOpen?.Invoke(this, new OpenEventArgs());
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        private void HandleMessage(Opcode opcode, byte[] rawData)
        {
            // UnityEngine.Debug.Log("OnMessage: " + opcode);
            try
            {
                OnMessage?.Invoke(this, new MessageEventArgs(opcode, rawData));
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        private void HandleClose(ushort code, string reason)
        {
            // UnityEngine.Debug.Log("OnClose: " + code + " " + reason);
            try
            {
                OnClose?.Invoke(this, new CloseEventArgs(code, reason));
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        private void HandleError(Exception exception)
        {
            // UnityEngine.Debug.Log("OnError: " + exception.Message);
            OnError?.Invoke(this, new ErrorEventArgs(exception.Message));
        }
    }
}
#endif
