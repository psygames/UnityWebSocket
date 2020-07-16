using System;
using System.Collections.Generic;

namespace UnityWebSocket.Synchronized
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

        public WebSocket()
        {
            _socket = new Uniform.WebSocket();

            _socket.OnOpen += (o, e) =>
            {
                lock (eventArgsQueue) { eventArgsQueue.Enqueue(e); }
            };
            _socket.OnClose += (o, e) =>
            {
                lock (eventArgsQueue) { eventArgsQueue.Enqueue(e); }
            };
            _socket.OnError += (o, e) =>
            {
                lock (eventArgsQueue) { eventArgsQueue.Enqueue(e); }
            };
            _socket.OnMessage += (o, e) =>
            {
                lock (eventArgsQueue) { eventArgsQueue.Enqueue(e); }
            };

            UnityWebSocketManager.Instance.Add(this);

#if UNITY_EDITOR
            UnityWebSocketDebuger.Create(this);
#endif
        }


        public void SendAsync(string data, Action completed = null)
        {
#if UNITY_EDITOR
            _socket.SendAsync(data, () =>
            {
                UnityWebSocketDebuger.Send(this, data);
                if (completed != null)
                {
                    lock (sendCallbackQueue)
                    {
                        sendCallbackQueue.Enqueue(completed);
                    }
                }
            });
#else

            if (completed != null)
            {
                _socket.SendAsync(data, () =>
                {
                    lock (sendCallbackQueue)
                    {
                        sendCallbackQueue.Enqueue(completed);
                    }
                });
            }
            else
            {
                _socket.SendAsync(data);
            }
#endif
        }

        public void SendAsync(byte[] data, Action completed = null)
        {
#if UNITY_EDITOR
            _socket.SendAsync(data, () =>
            {
                UnityWebSocketDebuger.Send(this, data);
                if (completed != null)
                {
                    lock (sendCallbackQueue)
                    {
                        sendCallbackQueue.Enqueue(completed);
                    }
                }
            });
#else

            if (completed != null)
            {
                _socket.SendAsync(data, () =>
                {
                    lock (sendCallbackQueue)
                    {
                        sendCallbackQueue.Enqueue(completed);
                    }
                });
            }
            else
            {
                _socket.SendAsync(data);
            }
#endif
        }

        public void ConnectAsync(string address)
        {
            _socket.ConnectAsync(address);
        }

        public void CloseAsync()
        {
            _socket.CloseAsync();
        }

        private readonly Queue<EventArgs> eventArgsQueue = new Queue<EventArgs>();
        private readonly Queue<Action> sendCallbackQueue = new Queue<Action>();
        public void Update()
        {
            while (sendCallbackQueue.Count > 0)
            {
                Action callback;
                lock (sendCallbackQueue)
                {
                    callback = sendCallbackQueue.Dequeue();
                }
                if (callback != null)
                {
                    callback.Invoke();
                }
            }


            while (eventArgsQueue.Count > 0)
            {
                EventArgs e;
                lock (eventArgsQueue)
                {
                    e = eventArgsQueue.Dequeue();
                }

                if (e is CloseEventArgs && OnClose != null)
                {
                    OnClose.Invoke(this, e as CloseEventArgs);
                }
                else if (e is OpenEventArgs && OnOpen != null)
                {
                    OnOpen.Invoke(this, e as OpenEventArgs);
                }
                else if (e is MessageEventArgs && OnMessage != null)
                {
                    OnMessage.Invoke(this, e as MessageEventArgs);
                }
                else if (e is ErrorEventArgs && OnError != null)
                {
                    OnError.Invoke(this, e as ErrorEventArgs);
                }
            }
        }
    }
}
