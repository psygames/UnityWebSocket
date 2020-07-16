using System.Collections.Generic;
using UnityEngine;

namespace UnityWebSocket.Synchronized
{
    public class UnityWebSocketManager : MonoBehaviour
    {
        private const string rootName = "[UnityWebSocketManager]";
        private static UnityWebSocketManager _instance;
        public static UnityWebSocketManager Instance
        {
            get
            {
                if (_instance == null)
                    AutoCreateInstance();
                return _instance;
            }
        }

        internal UnityWebSocketManager()
        { }


        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }


        public static void AutoCreateInstance()
        {
            GameObject go = GameObject.Find("/"+ rootName);
            if (go == null)
            {
                go = new GameObject(rootName);
            }

            if (go.GetComponent<UnityWebSocketManager>() == null)
            {
                go.AddComponent<UnityWebSocketManager>();
            }
        }

        private readonly List<WebSocket> sockets = new List<WebSocket>();

        public void Add(WebSocket socket)
        {
            sockets.Add(socket);
        }

        public void Remove(WebSocket socket)
        {
            sockets.Remove(socket);
        }

        private void Update()
        {
            foreach (var ws in sockets)
            {
                ws.Update();
            }
        }
    }
}