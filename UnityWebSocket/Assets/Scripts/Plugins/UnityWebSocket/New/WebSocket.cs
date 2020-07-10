using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityWebSocket
{
    /// <summary>
    /// For All Platform
    /// </summary>
    public class WebSocketNew
    {
        #region ClientWebSocket

        readonly ClientWebSocket _webSocket = new ClientWebSocket();
        readonly CancellationToken _cancellation = new CancellationToken();

        public async void WebSocket()
        {
            try
            {
                var url = "ws://echo.websocket.org";
                await _webSocket.ConnectAsync(new Uri(url), _cancellation);
                var bsend = new byte[1024];
                await _webSocket.SendAsync(new ArraySegment<byte>(bsend), WebSocketMessageType.Binary, true, _cancellation); //发送数据

                while (true)
                {
                    var result = new byte[1024];
                    await _webSocket.ReceiveAsync(new ArraySegment<byte>(result), new CancellationToken());//接受数据
                    // var lastbyte = ByteCut(result, 0x00);
                    var str = Encoding.UTF8.GetString(result, 0, result.Length);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        public static async void EchoClient()
        {
            //WebSocket socket = new ClientWebSocket();
            //WebSocket.CreateClientWebSocket()
            ClientWebSocket socket = new ClientWebSocket();
            Uri uri = new Uri("ws://localhost:50430/websocket/mytest.k");
            var cts = new CancellationTokenSource();
            await socket.ConnectAsync(uri, cts.Token);

            Console.WriteLine(socket.State);

            Task.Factory.StartNew(
                async () =>
                {
                    var rcvBytes = new byte[128];
                    var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                    while (true)
                    {
                        WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(rcvBuffer, cts.Token);
                        byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                        string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                        Console.WriteLine("Received: {0}", rcvMsg);
                    }
                }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            while (true)
            {
                var message = Console.ReadLine();
                if (message == "Bye")
                {
                    cts.Cancel();
                    return;
                }
                byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                var sendBuffer = new ArraySegment<byte>(sendBytes);
                await
                    socket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true,
                                     cancellationToken: cts.Token);
            }

        }
    }

}
