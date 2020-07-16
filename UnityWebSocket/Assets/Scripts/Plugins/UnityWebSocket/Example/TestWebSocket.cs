using UnityEngine;
using UnityWebSocket;
using UnityWebSocket.Synchronized;

public class TestWebSocket : MonoBehaviour
{
    public string url = "ws://echo.websocket.org";
    private WebSocket socket;

    private void Awake()
    {
        socket = new WebSocket();
        socket.OnOpen += Socket_OnOpen;
        socket.OnMessage += Socket_OnMessage;
        socket.OnClose += Socket_OnClose;
        socket.OnError += Socket_OnError;
    }

    private void Socket_OnOpen(object sender, System.EventArgs e)
    {
        message += string.Format("Connected: {0}\n", url);
    }

    private void Socket_OnMessage(object sender, MessageEventArgs e)
    {
        message += string.Format("Received: {0}\n", e.Data);
        messageCount += 1;
    }

    private void Socket_OnClose(object sender, CloseEventArgs e)
    {
        message += string.Format("Closed: {0}\n", e.Reason);
    }

    private void Socket_OnError(object sender, ErrorEventArgs e)
    {
        message += string.Format("Error: {0}\n", e.Message);
    }

    string sendText = "";
    string message = "";
    int messageCount;
    Vector2 scrollPos;

    private void OnGUI()
    {
        var scale = Screen.width / 800f;
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        var width = GUILayout.Width(Screen.width / scale - 10);

        GUILayout.Label(string.Format("State: {0}", socket.ReadyState), width);
        GUILayout.Label("URL: ", width);
        url = GUILayout.TextField(url, width);

        GUILayout.BeginHorizontal();
        GUI.enabled = socket.ReadyState == WebSocketState.Closed;
        if (GUILayout.Button("Connect"))
        {
            socket.ConnectAsync(url);
        }

        GUI.enabled = socket.ReadyState == WebSocketState.Open;
        if (GUILayout.Button("Close"))
        {
            socket.CloseAsync();
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Text: ");
        sendText = GUILayout.TextArea(sendText, GUILayout.MinHeight(50), width);

        if (GUILayout.Button("Send"))
        {
            if (!string.IsNullOrEmpty(sendText))
            {
                socket.SendAsync(sendText, () =>
                {
                    message += string.Format("Send: {0}\n", sendText);
                });
            }
        }

        GUI.enabled = true;
        GUILayout.Label(string.Format("Message({0}): ", messageCount));
        if (GUILayout.Button("Clear"))
        {
            message = "";
            messageCount = 0;
        }

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(Screen.height / scale - 250), width);
        GUILayout.Label(message);
        GUILayout.EndScrollView();
    }
}