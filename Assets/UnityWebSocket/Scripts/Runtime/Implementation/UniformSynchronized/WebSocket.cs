using System;
using System.Collections.Generic;

namespace UnityWebSocket
{
    public class WebSocket : IWebSocket
    {
        public event EventHandler<OpenEventArgs> OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;
        public string Address { get { return _socket.Address; } }
        public WebSocketState ReadyState { get { return _socket.ReadyState; } }

        private readonly Uniform.WebSocket _socket;

        public WebSocket(string address)
        {
            _socket = new Uniform.WebSocket(address);
            _socket.OnOpen += (o, e) => HandleEvent(e);
            _socket.OnClose += (o, e) => HandleEvent(e);
            _socket.OnError += (o, e) => HandleEvent(e);
            _socket.OnMessage += (o, e) => HandleEvent(e);
        }

        private void HandleEvent(EventArgs eventArgs)
        {
            lock (eventQueueLock)
            {
                eventQueue.Enqueue(eventArgs);
            }
        }

        public void ConnectAsync()
        {
            WebSocketManager.Instance.Add(this);
            _socket.ConnectAsync();
        }

        public void CloseAsync()
        {
            _socket.CloseAsync();
        }

        public void SendAsync(string data)
        {
            _socket.SendAsync(data);
        }

        public void SendAsync(byte[] data)
        {
            _socket.SendAsync(data);
        }

        private readonly Queue<EventArgs> eventQueue = new Queue<EventArgs>();
        private readonly object eventQueueLock = new object();
        internal void Update()
        {
            EventArgs e;
            while (eventQueue.Count > 0)
            {
                lock (eventQueueLock)
                {
                    e = eventQueue.Dequeue();
                }

                if (e is CloseEventArgs)
                {
                    OnClose?.Invoke(this, e as CloseEventArgs);
                    WebSocketManager.Instance.Remove(this);
                }
                else if (e is OpenEventArgs)
                {
                    OnOpen?.Invoke(this, e as OpenEventArgs);
                }
                else if (e is MessageEventArgs)
                {
                    OnMessage?.Invoke(this, e as MessageEventArgs);
                }
                else if (e is ErrorEventArgs)
                {
                    OnError?.Invoke(this, e as ErrorEventArgs);
                }
            }
        }
    }
}
