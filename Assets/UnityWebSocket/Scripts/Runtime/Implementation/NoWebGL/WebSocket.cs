#if !NET_LEGACY && (UNITY_EDITOR || !UNTIY_WEBGL)
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.IO;

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
        private bool IsCtsCanceled { get { return cts == null || cts.IsCancellationRequested; } }
        private bool isSending;

        #region APIs
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
            Task.Run(ConnectTask);
        }

        public void CloseAsync()
        {
            Task.Run(CloseTask);
        }

        public void SendAsync(byte[] data)
        {
            if (IsCtsCanceled) return;
            var buffer = new SendBuffer(data, WebSocketMessageType.Binary);
            SendAsyncBuffer(buffer);
        }

        public void SendAsync(string text)
        {
            if (IsCtsCanceled) return;
            var data = Encoding.UTF8.GetBytes(text);
            var buffer = new SendBuffer(data, WebSocketMessageType.Text);
            SendAsyncBuffer(buffer);
        }
        #endregion


        private async Task ConnectTask()
        {
            Log("Connect Task Begin ...");

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

            HandleOpen();

            Log("Connect Task End !");

            await ReceiveTask();
        }

        private async Task CloseTask()
        {
            Log("Close Task Begin ...");

            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", cts.Token);
            }
            catch (Exception e)
            {
                HandleError(e);
            }

            Log("Close Task End !");
        }

        class SendBuffer
        {
            public byte[] data;
            public WebSocketMessageType type;
            public SendBuffer(byte[] data, WebSocketMessageType type)
            {
                this.data = data;
                this.type = type;
            }
        }

        private object sendQueueLock = new object();
        private Queue<SendBuffer> sendQueue = new Queue<SendBuffer>();

        private void SendAsyncBuffer(SendBuffer buffer)
        {
            if (isSending)
            {
                lock (sendQueueLock)
                {
                    sendQueue.Enqueue(buffer);
                }
            }
            else
            {
                isSending = true;
                sendQueue.Enqueue(buffer);
                Task.Run(SendTask);
            }
        }

        private async Task SendTask()
        {
            Log("Send Task Begin ...");

            try
            {
                SendBuffer buffer = null;
                while (!IsCtsCanceled)
                {
                    if (sendQueue.Count <= 0) break;
                    lock (sendQueueLock)
                    {
                        buffer = sendQueue.Dequeue();
                    }
                    await socket.SendAsync(new ArraySegment<byte>(buffer.data), buffer.type, true, cts.Token);
                    Log("Send Queue Size: " + sendQueue.Count);
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
            finally
            {
                isSending = false;
            }

            if (IsCtsCanceled)
            {
                sendQueue.Clear();
            }

            Log("Send Task End !");
        }

        private async Task ReceiveTask()
        {
            Log("Receive Task Begin ...");

            string closeReason = "";
            ushort closeCode = 0;
            bool isClosed = false;
            var segment = new ArraySegment<byte>(new byte[8192]);

            try
            {
                while (!IsCtsCanceled && !isClosed)
                {
                    var ms = new MemoryStream();
                    var result = await socket.ReceiveAsync(segment, cts.Token);
                    ms.Write(segment.Array, 0, result.Count);
                    if (!result.EndOfMessage) continue;
                    var data = ms.ToArray();
                    ms.Close();
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

            cts.Cancel();

            Log("Wait For Close ...");

            while (!IsCtsCanceled || isSending)
            {
                await Task.Delay(10);
            }

            Log("Wait For Close End !");

            HandleClose(closeCode, closeReason);
            SocketDispose();

            Log("Receive Task End !");
        }

        private void SocketDispose()
        {
            cts.Dispose();
            socket.Dispose();
            cts = null;
            socket = null;
        }

        private void HandleOpen()
        {
            Log("OnOpen");
            OnOpen?.Invoke(this, new OpenEventArgs());
        }

        private void HandleMessage(Opcode opcode, byte[] rawData)
        {
            Log("OnMessage: " + opcode + "(" + rawData.Length + ")");
            OnMessage?.Invoke(this, new MessageEventArgs(opcode, rawData));
        }

        private void HandleClose(ushort code, string reason)
        {
            Log("OnClose: " + reason + "(" + code + ")");
            OnClose?.Invoke(this, new CloseEventArgs(code, reason));
        }

        private void HandleError(Exception exception)
        {
            Log("OnError: " + exception.Message);
            OnError?.Invoke(this, new ErrorEventArgs(exception.Message));
        }

        [System.Diagnostics.Conditional("UNITY_WEB_SOCKET_LOG")]
        private void Log(string msg)
        {
            UnityEngine.Debug.Log($"<color=yellow>[UnityWebSocket]</color>" +
                $"<color=green>[T-{Thread.CurrentThread.ManagedThreadId:D3}]</color>" +
                $"<color=red>[{DateTime.Now.TimeOfDay}]</color>" +
                $" {msg}");
        }
    }
}
#endif
