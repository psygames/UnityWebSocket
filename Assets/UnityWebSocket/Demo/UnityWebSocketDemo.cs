using UnityEngine;

namespace UnityWebSocket.Demo
{
    public class UnityWebSocketDemo : MonoBehaviour
    {
        public string address = "ws://127.0.0.1:8080";
        public string sendText = "Hello World!";
        public bool logMessage = true;

        private IWebSocket socket;

        private string log = "";
        private int sendCount;
        private int receiveCount;
        private Vector2 scrollPos;

        private void OnGUI()
        {
            var scale = Screen.width / 800f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            var width = GUILayout.Width(Screen.width / scale - 10);

            WebSocketState state = socket == null ? WebSocketState.Closed : socket.ReadyState;

            GUILayout.Label("SDK Version: " + Settings.VERSION, width);
            var stateColor = state == WebSocketState.Closed ? "red" : state == WebSocketState.Open ? "#11ff11" : "#aa4444";
            var richText = new GUIStyle() { richText = true };
            GUILayout.Label(string.Format(" <color=white>State:</color> <color={1}>{0}</color>", state, stateColor), richText);

            GUI.enabled = state == WebSocketState.Closed;
            GUILayout.Label("Address: ", width);
            address = GUILayout.TextField(address, width);

            GUILayout.BeginHorizontal();
            GUI.enabled = state == WebSocketState.Closed;
            if (GUILayout.Button(state == WebSocketState.Connecting ? "Connecting..." : "Connect"))
            {
                socket = new WebSocket(address);
                socket.OnOpen += Socket_OnOpen;
                socket.OnMessage += Socket_OnMessage;
                socket.OnClose += Socket_OnClose;
                socket.OnError += Socket_OnError;
                AddLog(string.Format("Connecting..."));
                socket.ConnectAsync();
            }

            GUI.enabled = state == WebSocketState.Open;
            if (GUILayout.Button(state == WebSocketState.Closing ? "Closing..." : "Close"))
            {
                AddLog(string.Format("Closing..."));
                socket.CloseAsync();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Text: ");
            sendText = GUILayout.TextArea(sendText, GUILayout.MinHeight(50), width);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Send") && !string.IsNullOrEmpty(sendText))
            {
                socket.SendAsync(sendText);
                AddLog(string.Format("Send: {0}", sendText));
                sendCount += 1;
            }
            if (GUILayout.Button("Send Bytes") && !string.IsNullOrEmpty(sendText))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(sendText);
                socket.SendAsync(bytes);
                AddLog(string.Format("Send Bytes ({1}): {0}", sendText, bytes.Length));
                sendCount += 1;
            }
            if (GUILayout.Button("Send x100") && !string.IsNullOrEmpty(sendText))
            {
                for (int i = 0; i < 100; i++)
                {
                    var text = (i + 1).ToString() + ". " + sendText;
                    socket.SendAsync(text);
                    AddLog(string.Format("Send: {0}", text));
                    sendCount += 1;
                }
            }
            if (GUILayout.Button("Send Bytes x100") && !string.IsNullOrEmpty(sendText))
            {
                for (int i = 0; i < 100; i++)
                {
                    var text = (i + 1).ToString() + ". " + sendText;
                    var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                    socket.SendAsync(bytes);
                    AddLog(string.Format("Send Bytes ({1}): {0}", text, bytes.Length));
                    sendCount += 1;
                }
            }

            GUILayout.EndHorizontal();

            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            logMessage = GUILayout.Toggle(logMessage, "Log Message");
            GUILayout.Label(string.Format("Send Count: {0}", sendCount));
            GUILayout.Label(string.Format("Receive Count: {0}", receiveCount));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Clear"))
            {
                log = "";
                receiveCount = 0;
                sendCount = 0;
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(Screen.height / scale - 270), width);
            GUILayout.Label(log);
            GUILayout.EndScrollView();
        }

        private void AddLog(string str)
        {
            if (!logMessage) return;
            log += str + "\n";
            if (log.Length > 4 * 1024)
            {
                log = log.Substring(2 * 1024);
            }
            scrollPos.y = 10000;
        }

        private void Socket_OnOpen(object sender, OpenEventArgs e)
        {
            AddLog(string.Format("Connected: {0}", address));
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                AddLog(string.Format("Receive Bytes ({1}): {0}", e.Data, e.RawData.Length));
            }
            else if (e.IsText)
            {
                AddLog(string.Format("Receive: {0}", e.Data));
            }
            receiveCount += 1;
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            AddLog(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            AddLog(string.Format("Error: {0}", e.Message));
        }

        private void OnApplicationQuit()
        {
            if (socket != null && socket.ReadyState != WebSocketState.Closed)
            {
                socket.CloseAsync();
            }
        }
    }
}
