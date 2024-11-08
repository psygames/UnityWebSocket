#if !NET_LEGACY && (UNITY_EDITOR || !UNITY_WEBGL)
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebSocket
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    internal class WebSocketManager : MonoBehaviour
    {
        private const string rootName = "[UnityWebSocket]";
        private static WebSocketManager _instance;
        public static WebSocketManager Instance
        {
            get
            {
                if (!_instance) CreateInstance();
                return _instance;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static void CreateInstance()
        {
            GameObject go = GameObject.Find("/" + rootName);
            if (!go) go = new GameObject(rootName);
            _instance = go.GetComponent<WebSocketManager>();
            if (!_instance) _instance = go.AddComponent<WebSocketManager>();
#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
            UnityEditor.Compilation.CompilationPipeline.compilationStarted -= OnCompilationStarted;
            UnityEditor.Compilation.CompilationPipeline.compilationStarted += OnCompilationStarted;
#endif
        }

        private readonly List<WebSocket> sockets = new List<WebSocket>();

        public void Add(WebSocket socket)
        {
            if (!sockets.Contains(socket))
                sockets.Add(socket);
        }

        public void Remove(WebSocket socket)
        {
            if (sockets.Contains(socket))
                sockets.Remove(socket);
        }

        private void Update()
        {
            if (sockets.Count <= 0) return;
            for (int i = sockets.Count - 1; i >= 0; i--)
            {
                sockets[i].Update();
            }
        }

#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
        private static void OnCompilationStarted(object obj)
        {
            if (_instance != null)
            {
                _instance.SocketAbort();
            }
        }
#endif

        private void OnApplicationQuit()
        {
            SocketAbort();
        }

        private void SocketAbort()
        {
            for (int i = sockets.Count - 1; i >= 0; i--)
            {
                sockets[i].Abort();
            }
        }
#endif
    }
}
#endif