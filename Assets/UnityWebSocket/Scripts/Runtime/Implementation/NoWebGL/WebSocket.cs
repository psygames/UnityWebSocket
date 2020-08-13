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
        private bool isSendThreadRunning;
        private bool isReceiveThreadRunning;

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
            var sendBuffer = SpawnBuffer(WebSocketMessageType.Binary, data, completed);
            PushBuffer(sendBuffer);
        }

        public void SendAsync(string text, Action completed = null)
        {
            var data = Encoding.UTF8.GetBytes(text);
            var sendBuffer = SpawnBuffer(WebSocketMessageType.Text, data, completed);
            PushBuffer(sendBuffer);
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

            LongRunningTask(SendThread);
            LongRunningTask(ReceiveThread);

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

            while (!IsCtsCancel || isSendThreadRunning || isReceiveThreadRunning)
            {
                await Task.Delay(1);
            }

            SocketDispose();

            // UnityEngine.Debug.Log("Dispose Thread Stop !");
        }

        private async Task SendThread()
        {
            // UnityEngine.Debug.Log("Send Thread Start ...");

            try
            {
                isSendThreadRunning = true;
                SendBuffer buffer = null;
                while (!IsCtsCancel)
                {
                    if (sendBuffers.Count <= 0)
                    {
                        await Task.Delay(1);
                        continue;
                    }
                    buffer = PopBuffer();
                    if (!IsCtsCancel)
                    {
                        await socket.SendAsync(buffer.buffer, buffer.type, true, cts.Token);
                        HandleSent(buffer.callback);
                    }
                    ReleaseBuffer(buffer);

                    // UnityEngine.Debug.Log("SendBuffers: " + sendBuffers.Count + ", PoolelBuffers: " + pooledSendBuffers.Count);
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
            finally
            {
                isSendThreadRunning = false;
                while (sendBuffers.Count > 0)
                {
                    ReleaseBuffer(PopBuffer());
                }
            }

            // UnityEngine.Debug.Log("Send Thread Stop !");
        }

        private async Task ReceiveThread()
        {
            // UnityEngine.Debug.Log("Receive Thread Start ...");

            var bufferCap = 1024;
            var buffer = new byte[bufferCap];
            var received = 0;

            string closeReason = "";
            ushort closeCode = 0;
            bool isClosed = false;

            try
            {
                isReceiveThreadRunning = true;
                var segment = new ArraySegment<byte>(buffer);

                while (!IsCtsCancel && !isClosed)
                {
                    WebSocketReceiveResult result = await socket.ReceiveAsync(segment, cts.Token);
                    received += result.Count;

                    if (received >= buffer.Length && !result.EndOfMessage)
                    {
                        bufferCap = bufferCap * 2;
                        var newBuffer = new byte[bufferCap];
                        Array.Copy(buffer, newBuffer, buffer.Length);
                        buffer = newBuffer;
                        newBuffer = null;
                        // UnityEngine.Debug.Log("Expand Receive Buffer to " + bufferCap);
                    }

                    if (!result.EndOfMessage)
                    {
                        segment = new ArraySegment<byte>(buffer, received, buffer.Length - received);
                        continue;
                    }

                    byte[] data = new byte[received];
                    for (int i = 0; i < received; i++)
                    {
                        data[i] = buffer[i];
                    }

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
                    received = 0;
                    segment = new ArraySegment<byte>(buffer);
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
                buffer = null;
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

        private readonly Queue<SendBuffer> sendBuffers = new Queue<SendBuffer>();
        private readonly Queue<SendBuffer> pooledSendBuffers = new Queue<SendBuffer>();

        class SendBuffer
        {
            public WebSocketMessageType type;
            public ArraySegment<byte> buffer;
            public Action callback;
        }

        private void PushBuffer(SendBuffer sendBuffer)
        {
            lock (sendBuffers)
            {
                sendBuffers.Enqueue(sendBuffer);
            }
        }

        private SendBuffer PopBuffer()
        {
            SendBuffer buffer;
            lock (sendBuffers)
            {
                buffer = sendBuffers.Dequeue();
            }
            return buffer;
        }

        private void ReleaseBuffer(SendBuffer sendBuffer)
        {
            sendBuffer.buffer = default;
            sendBuffer.callback = null;
            lock (pooledSendBuffers)
            {
                pooledSendBuffers.Enqueue(sendBuffer);
            }
        }

        private SendBuffer SpawnBuffer(WebSocketMessageType type, byte[] bytes, Action callback)
        {
            SendBuffer sendBuffer = null;
            if (pooledSendBuffers.Count <= 0)
            {
                sendBuffer = new SendBuffer
                {
                    type = WebSocketMessageType.Text,
                    buffer = new ArraySegment<byte>(bytes),
                    callback = callback
                };
                return sendBuffer;
            }

            lock (pooledSendBuffers)
            {
                sendBuffer = pooledSendBuffers.Dequeue();
            }

            sendBuffer.type = type;
            sendBuffer.buffer = new ArraySegment<byte>(bytes);
            sendBuffer.callback = callback;

            return sendBuffer;
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

        private void HandleSent(Action action)
        {
            // UnityEngine.Debug.Log("OnOpen");

            try
            {
                action?.Invoke();
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
