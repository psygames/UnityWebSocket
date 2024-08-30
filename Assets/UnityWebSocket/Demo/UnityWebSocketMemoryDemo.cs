using System.Collections;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.Profiling;

namespace UnityWebSocket.Demo
{
    public class UnityWebSocketMemoryDemo : MonoBehaviour
    {
        public string address = "wss://echo.websocket.events";

        private IWebSocket socket;
        private int receiveCount = 0;

        private void Start()
        {
            StartCoroutine(RoutineTest());
        }

        static readonly byte[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray().Select(c => (byte)c).ToArray();
        private PooledBuffer RandomString(int length)
        {
            PooledBuffer buffer = PooledBuffer.Create(Opcode.Text);
            for (int i = 0; i < length; i++)
            {
                buffer.Write(chars[Random.Range(0, chars.Length)], i);
            }

            return buffer;
        }

        private IEnumerator RoutineTest()
        {
            while (true)
            {
                if (socket == null)
                {
                    socket = new WebSocket(address);
                    socket.OnOpen += Socket_OnOpen;
                    socket.OnMessage += Socket_OnMessage;
                    socket.OnClose += Socket_OnClose;
                    socket.OnError += Socket_OnError;
                    socket.ConnectAsync();
                }

                socket.ConnectAsync();

                while (socket.ReadyState != WebSocketState.Open)
                {
                    yield return null;
                }

                var delay = new WaitForSeconds(0.1f);
                var sendCount = 0;
                while (socket.ReadyState == WebSocketState.Open)
                {
                    var message = RandomString(Random.Range(1, 512));

                    socket.SendAsync(message);
                    sendCount += 1;
                    yield return null;

                    if (sendCount % 20 == 0)
                    {
                        // PooledBuffer.LogStatus();
                    }
                }
            }
        }

        private void Socket_OnOpen(object sender, OpenEventArgs e)
        {
            Debug.Log(string.Format("Connected: {0}", address));
        }

        private void Socket_OnMessage(object sender, PooledBuffer e)
        {
            // if (e.IsBinary)
            // {
            //     AddLog(string.Format("Receive Bytes ({1}): {0}", e.Data, e.Length));
            // }
            // else if (e.IsText)
            // {
            //     AddLog(string.Format("Receive: {0}", e.Data));
            // }

            // Profiler.BeginSample("Socket_OnMessage Log");
            // Debug.Log(string.Format("Receive: {0}", e.Data));
            // Profiler.EndSample();

            if (receiveCount % 20 == 0)
            {
                // PooledBuffer.LogStatus();
            }

            e.Dispose();

            receiveCount += 1;
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            Debug.Log(string.Format("Error: {0}", e.Message));
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
