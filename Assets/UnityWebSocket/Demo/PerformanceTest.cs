using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebSocket.Demo
{
    public class PerformanceTest : MonoBehaviour
    {
        public string address = "wss://echo.websocket.events";
        public string sendText = "Hello UnityWebSocket!";
        public Button btn;

        private WebSocket socket;

        private void Awake()
        {
            btn.onClick.AddListener(ButtonClick);
        }

        public void ButtonClick()
        {
            StartCoroutine(TestCase1());
        }

        private void AddLog(string str)
        {
            Debug.Log(str);
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

        static WaitForSeconds wait5000 = new WaitForSeconds(5);
        static WaitForSeconds wait1000 = new WaitForSeconds(1);
        static WaitForSeconds wait100 = new WaitForSeconds(0.1f);
        static WaitForSeconds wait10 = new WaitForSeconds(0.01f);
        private IEnumerator TestCase1()
        {
            socket = new WebSocket(address);
            socket.ConnectAsync();
            yield return wait5000;
            byte[] data = System.Text.Encoding.UTF8.GetBytes(sendText);
            for (int i = 0; i < 100; i++)
            {
                socket.SendAsync(data);
                yield return wait100;
            }
            socket.CloseAsync();
        }
    }
}
