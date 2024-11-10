using UnityEngine;
using UnityEngine.UI;

namespace UnityWebSocket.Demo
{
    public class PerformanceTest : MonoBehaviour
    {
        public string address = "wss://echo.websocket.events";
        public string sendText = "Hello UnityWebSocket!";
        public Button btn;

        private IWebSocket socket;

        private void Awake()
        {
            // btn.onClick.AddListener(ButtonClick);
        }

        public void ButtonClick()
        {
            // AddLog("111");
        }

        private void AddLog(string str)
        {

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

        private int frame = 0;
        private float time = 0;
        private float fps = 0;
        private void Update()
        {
            frame += 1;
            time += Time.deltaTime;
            if (time >= 0.5f)
            {
                fps = frame / time;
                frame = 0;
                time = 0;
            }
        }
    }
}
