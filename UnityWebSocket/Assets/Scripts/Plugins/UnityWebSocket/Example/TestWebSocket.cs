using UnityEngine;
using UnityWebSocket;

public class TestWebSocket : MonoBehaviour
{
    public string url = "ws://echo.websocket.org";
    WebSocket socket;
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

    private void OnGUI()
    {
        var scale = Screen.width / 800f;
        GUI.matrix = Matrix4x4.TRS(new Vector3(200 * scale, 10, 0), Quaternion.identity, new Vector3(scale, scale, 1));

        GUILayout.Label(string.Format("State: {0}", socket.ReadyState));

        GUILayout.Label("URL:                                                   ");
        url = GUILayout.TextField(url);

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
        sendText = GUILayout.TextArea(sendText);

        if (GUILayout.Button("Send"))
        {
            socket.SendAsync(sendText, null);
        }

        GUI.enabled = true;
        GUILayout.Label(string.Format("Message({0}): ", messageCount));
        if (GUILayout.Button("Clear"))
        {
            message = "";
            messageCount = 0;
        }
        GUILayout.Label(message);

    }
}
