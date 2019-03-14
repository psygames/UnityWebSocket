using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityWebSocket
{
    public interface IWebSocket
    {
        void Connect();
        void Send(byte[] data);
        void Send(string data);
        void Close();
        void ConnectAsync();
        void CloseAsync();
        void SendAsync(byte[] data, Action<bool> completed);
        string address { get; }
        WebSocketState readyState { get; }
        event EventHandler onOpen;
        event EventHandler<CloseEventArgs> onClose;
        event EventHandler<ErrorEventArgs> onError;
        event EventHandler<MessageEventArgs> onMessage;
    }
}
