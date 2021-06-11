using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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

        private HttpServer server;
        private List<string> logs = new List<string>();
        private Vector2 scroll;
        private bool needRepaint;
        private int port = 5963;
        private bool secure = true;

        private void OnGUI()
        {
            Color lastColor = GUI.color;
            window = this;
            bool isStart = server != null && server.IsListening;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(isStart);
            EditorGUILayout.LabelField("Listening on port:", GUILayout.Width(110));
            port = EditorGUILayout.IntField(port, GUILayout.Width(80));
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            EditorGUILayout.LabelField("Secure:", GUILayout.Width(60));
            secure = EditorGUILayout.Toggle(secure);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Logs", GUILayout.Width(120)))
            {
                logs.Clear();
            }
            GUILayout.Label("");
            if (GUILayout.Button("Test On Browser", GUILayout.Width(140)))
            {
                if (secure)
                    Application.OpenURL("https://localhost:" + port);
                else
                    Application.OpenURL("http://localhost:" + port);
            }
            EditorGUILayout.EndHorizontal();
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
                    server = new HttpServer(port, secure);
                    server.Log.Level = WebSocketSharp.LogLevel.Trace;
                    if (secure)
                    {
                        var certPath = "Assets/UnityWebSocket/Scripts/Editor/cert.pfx";
                        var certPwd = "123456";
                        server.SslConfiguration.ServerCertificate =
                            new X509Certificate2(certPath, certPwd);
                        server.SslConfiguration.EnabledSslProtocols =
                        SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Ssl2;
                    }
                    // Set the document root path.
                    server.DocumentRootPath = "Assets/UnityWebSocket/Scripts/Editor/";

                    // Set the HTTP GET request event.
                    server.OnGet += (sender, e) =>
                    {
                        var req = e.Request;
                        var res = e.Response;

                        var path = req.RawUrl;
                        if (path == "/")
                            path += "index.html";

                        byte[] contents;
                        if (!e.TryReadFile(path, out contents))
                        {
                            res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
                            return;
                        }

                        if (path.EndsWith(".html"))
                        {
                            res.ContentType = "text/html";
                            res.ContentEncoding = Encoding.UTF8;
                        }
                        else if (path.EndsWith(".js"))
                        {
                            res.ContentType = "application/javascript";
                            res.ContentEncoding = Encoding.UTF8;
                        }

                        res.ContentLength64 = contents.LongLength;
                        res.Close(contents, true);
                    };
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
                {
                    Log(ID + ": Received: bytes(" + e.RawData.Length + ")");
                    Send(e.RawData);
                }
                else
                {
                    Log(ID + ": Received: " + e.Data + "");
                    Send(e.Data);
                }
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
