using System;
using UnityEngine;
using UnityWebSocket;

public class Example : MonoBehaviour
{
    public string url = "ws://echo.websocket.org";
    private IWebSocket socket;

    string sendText = "测试Test123 \\/*1#&^`";
    string log = "";
    int sendCount;
    int receiveCount;
    Vector2 scrollPos;
    bool logMessage = true;
    bool showLog = true;

    private void OnGUI()
    {
        var scale = Screen.width / 800f;
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        var width = GUILayout.Width(Screen.width / scale - 10);

        if (socket == null)
        {
            GUILayout.Label("sdk version: 2.0.1", width);
            if (GUILayout.Button("Init with Synchronized"))
            {
                socket = new UnityWebSocket.Synchronized.WebSocket(url);
            }

            if (GUILayout.Button("Init with Uniform"))
            {
                socket = new UnityWebSocket.Uniform.WebSocket(url);
            }

#if UNITY_EDITOR || !UNITY_WEBGL
#if !NET_LEGACY
            if (GUILayout.Button("Init with NoWebGL"))
            {
                socket = new UnityWebSocket.NoWebGL.WebSocket(url);
            }
#endif
#else
            if (GUILayout.Button("Init with WebGL"))
            {
                socket = new UnityWebSocket.WebGL.WebSocket(url);
            }
#endif
            if (socket != null)
            {
                socket.OnOpen += Socket_OnOpen;
                socket.OnMessage += Socket_OnMessage;
                socket.OnClose += Socket_OnClose;
                socket.OnError += Socket_OnError;
            }
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("State: {0}", socket.ReadyState));
        if (GUILayout.Button("Dispose"))
        {
            if (socket.ReadyState != WebSocketState.Closed)
                socket.CloseAsync();
            socket = null;
            return;
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("URL: ", width);
        url = GUILayout.TextField(url, width);

        GUILayout.BeginHorizontal();
        GUI.enabled = socket.ReadyState == WebSocketState.Closed;
        if (GUILayout.Button(socket.ReadyState == WebSocketState.Connecting ? "Connecting..." : "Connect"))
        {
            AddLog(string.Format("Connecting...\n"));
            socket.ConnectAsync();
        }

        GUI.enabled = socket.ReadyState == WebSocketState.Open;
        if (GUILayout.Button(socket.ReadyState == WebSocketState.Closing ? "Closing..." : "Close"))
        {
            AddLog(string.Format("Closing...\n"));
            socket.CloseAsync();
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Text: ");
        sendText = GUILayout.TextArea(sendText, GUILayout.MinHeight(50), width);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Send"))
        {
            if (!string.IsNullOrEmpty(sendText))
            {
                socket.SendAsync(sendText, () =>
                {
                    if (logMessage)
                        AddLog(string.Format("Send: {0}\n", sendText));
                    sendCount += 1;
                });
            }
        }
        if (GUILayout.Button("Send Bytes"))
        {
            if (!string.IsNullOrEmpty(sendText))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(sendText);
                socket.SendAsync(bytes, () =>
                {
                    if (logMessage)
                        AddLog(string.Format("Send Bytes ({1}): {0}\n", sendText, bytes.Length));
                    sendCount += 1;
                });
            }
        }
        if (GUILayout.Button("Send Bytes *1000"))
        {
            if (!string.IsNullOrEmpty(sendText))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(sendText);
                for (int i = 0; i < 1000; i++)
                {
                    socket.SendAsync(bytes, () =>
                    {
                        if (logMessage)
                            AddLog(string.Format("Send Bytes ({1}): {0}\n", sendText, bytes.Length));
                        sendCount += 1;
                    });
                }
            }
        }
        GUILayout.EndHorizontal();

        GUI.enabled = true;
        GUILayout.BeginHorizontal();
        showLog = GUILayout.Toggle(showLog, "Show Log");
        logMessage = GUILayout.Toggle(logMessage, "Log Message");
        GUILayout.Label(string.Format("Send ({0}): ", sendCount));
        GUILayout.Label(string.Format("Receive ({0}): ", receiveCount));
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear"))
        {
            log = "";
            receiveCount = 0;
            sendCount = 0;
        }

        if (showLog)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(Screen.height / scale - 250), width);
            GUILayout.Label(log);
            GUILayout.EndScrollView();
        }
    }

    private void AddLog(string str)
    {
        log += str;
    }

    private void Socket_OnOpen(object sender, OpenEventArgs e)
    {
        AddLog(string.Format("Connected: {0}\n", url));
    }

    private void Socket_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
        {
            if (logMessage)
                AddLog(string.Format("Receive Bytes ({1}): {0}\n", e.Data, e.RawData.Length));
        }
        else if (e.IsText)
        {
            if (logMessage)
                AddLog(string.Format("Receive: {0}\n", e.Data));
        }
        receiveCount += 1;
    }

    private void Socket_OnClose(object sender, CloseEventArgs e)
    {
        AddLog(string.Format("Closed, StatusCode: {0}, Reason: {1}\n", e.StatusCode, e.Reason));
    }

    private void Socket_OnError(object sender, ErrorEventArgs e)
    {
        AddLog(string.Format("Error: {0}\n", e.Message));
    }

}