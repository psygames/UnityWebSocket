#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityWebSocket.Synchronized
{
    public class UnityWebSocketDebuger : MonoBehaviour
    {
        [Header("WebSocket")]
        public string address;
        public WebSocketState state;

        [Header("Send Count")]
        public int sendCount;
        public int sendBytesCount;
        public int sendStringCount;
        public float sendCountSpeed;

        [Header("Send Size")]
        public int sendSize;
        public int sendBytesSize;
        public int sendStringSize;
        public float sendSizeSpeed;

        [Header("Receive Count")]
        public int receiveCount;
        public int receiveBytesCount;
        public int receiveStringCount;
        public float receiveCountSpeed;

        [Header("Receive Size")]
        public int receiveSize;
        public int receiveBytesSize;
        public int receiveStringSize;
        public float receiveSizeSpeed;

        private WebSocket socket;
        private float currentTime;

        private List<Tuple<float, int>> sendTimeCount = new List<Tuple<float, int>>();
        private List<Tuple<float, int>> sendTimeSize = new List<Tuple<float, int>>();
        private List<Tuple<float, int>> receiveTimeCount = new List<Tuple<float, int>>();
        private List<Tuple<float, int>> receiveTimeSize = new List<Tuple<float, int>>();

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            socket.OnMessage += Socket_OnMessage;
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            var _size = e.RawData.Length;
            if (e.IsBinary)
            {
                receiveBytesCount += 1;
                receiveBytesSize += _size;
            }
            else
            {
                receiveStringCount += 1;
                receiveStringSize += _size;
            }
            receiveCount += 1;
            receiveSize += _size;
            SyncAdd(receiveTimeCount, new Tuple<float, int>(currentTime, 1));
            SyncAdd(receiveTimeSize, new Tuple<float, int>(currentTime, _size));
        }

        private void OnSend(string message)
        {
            var _size = Encoding.UTF8.GetBytes(message).Length;
            sendStringCount += 1;
            sendStringSize += _size;
            sendCount += 1;
            sendSize += _size;
            SyncAdd(sendTimeCount, new Tuple<float, int>(currentTime, 1));
            SyncAdd(sendTimeSize, new Tuple<float, int>(currentTime, _size));
        }

        private void OnSend(byte[] bytes)
        {
            var _size = bytes.Length;
            sendBytesCount += 1;
            sendBytesSize += _size;
            sendCount += 1;
            sendSize += _size;
            SyncAdd(sendTimeCount, new Tuple<float, int>(currentTime, 1));
            SyncAdd(sendTimeSize, new Tuple<float, int>(currentTime, _size));
        }

        private float lastSpeedUpdateTime = 0;
        private void Update()
        {
            currentTime = Time.time;
            address = socket.Address == null ? "" : socket.Address;
            state = socket.ReadyState;

            if (Time.time - lastSpeedUpdateTime >= 1)
            {
                sendCountSpeed = CountSpeed(sendTimeCount);
                sendSizeSpeed = CountSpeed(sendTimeSize);
                receiveCountSpeed = CountSpeed(receiveTimeCount);
                receiveSizeSpeed = CountSpeed(receiveTimeSize);
                lastSpeedUpdateTime = Time.time;
            }
        }

        private void SyncAdd<T>(List<T> lst, T t)
        {
            lock (lst)
            {
                lst.Add(t);
            }
        }

        private float CountSpeed(List<Tuple<float, int>> lst)
        {
            int total = 0;
            float threshold = 2;
            lock (lst)
            {
                for (int i = lst.Count - 1; i >= 0; i--)
                {
                    if (lst[i].Item1 + threshold <= Time.time)
                    {
                        lst.RemoveAt(i);
                    }
                    else
                    {
                        total += lst[i].Item2;
                    }
                }
            }
            return total / threshold;
        }


        #region API
        private const string rootName = "[UnityWebSocketManager]";
        private static Dictionary<WebSocket, UnityWebSocketDebuger> sockets = new Dictionary<WebSocket, UnityWebSocketDebuger>();
        private static int index;

        public static void Create(WebSocket socket)
        {
            GameObject go = GameObject.Find("/" + rootName);
            if (go == null)
            {
                go = new GameObject(rootName);
            }

            var name = "socket_" + (++index).ToString();
            var comp = new GameObject(name).AddComponent<UnityWebSocketDebuger>();
            comp.transform.SetParent(go.transform, false);
            comp.socket = socket;
            sockets.Add(socket, comp);
        }

        public static void Destroy(WebSocket socket)
        {
            Destroy(sockets[socket].gameObject);
        }

        public static void Send(WebSocket socket, byte[] data)
        {
            sockets[socket].OnSend(data);
        }

        public static void Send(WebSocket socket, string data)
        {
            sockets[socket].OnSend(data);
        }

        #endregion
    }
}
#endif
