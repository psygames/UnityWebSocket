using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WebSocketSharp.Server;

namespace UnityWebSocket.Editor
{
    internal class TestServerWindow : EditorWindow
    {
        static TestServerWindow window = null;
        [MenuItem("Tools/UnityWebSocket/Test Server", priority = 100)]
        internal static void Open()
        {
            if (window != null)
            {
                window.Close();
            }

            window = GetWindow<TestServerWindow>(true, "Test Server");
            window.minSize = window.maxSize = new Vector2(600, 500);
            window.Show();
        }

        private void OnDestroy()
        {
            if (server != null && server.IsListening)
            {
                server.Stop();
            }
        }

        private WebSocketServer server;
        private List<string> logs = new List<string>();
        private Vector2 scroll;
        private bool needRepaint;
        private string address = "ws://0.0.0.0";

        private void OnGUI()
        {
            Color lastColor = GUI.color;
            window = this;
            bool isStart = server != null && server.IsListening;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(isStart);
            EditorGUILayout.LabelField("Listening on:", GUILayout.Width(80));
            address = EditorGUILayout.TextField(address);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Clear Logs", GUILayout.Width(120)))
            {
                logs.Clear();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, "box");
            foreach (var log in new List<string>(logs))
            {
                EditorGUILayout.LabelField(log);
            }
            EditorGUILayout.EndScrollView();

            if (!isStart)
            {
                GUI.color = Color.green;
                if (GUILayout.Button("Start", GUILayout.Height(30)))
                {
                    server = new WebSocketServer();
                    server.AddWebSocketService<TestServer>("/");
                    server.Start();
                }
            }
            else
            {
                GUI.color = Color.red;
                if (GUILayout.Button("Stop", GUILayout.Height(30)))
                {
                    server.Stop();
                }
            }

            GUI.color = lastColor;
        }

        private void OnInspectorUpdate()
        {
            if (needRepaint && window != null)
            {
                window.Repaint();
                needRepaint = false;
            }
        }

        internal static void Log(string log)
        {
            if (window == null) return;
            window.logs.Add(log);
            window.needRepaint = true;
        }

        public class TestServer : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                Log(ID + ": Connected");
            }

            protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
            {
                if (e.IsBinary)
                    Log(ID + ": Received: bytes(" + e.RawData.Length + ")");
                else
                    Log(ID + ": Received: " + e.Data + "");
                Send(e.RawData);
            }

            protected override void OnClose(WebSocketSharp.CloseEventArgs e)
            {
                Log(ID + ": Closed, Code: " + e.Code + ", Reason: " + e.Reason);
            }

            protected override void OnError(WebSocketSharp.ErrorEventArgs e)
            {
                Log(ID + ": Error, Message: " + e.Message);
            }
        }
    }
}
