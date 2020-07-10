using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.WebSockets;
using System.Threading;
using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;

public class TestNew : MonoBehaviour
{
    public string url = "ws://echo.websocket.org";

    public ClientWebSocket socket = new ClientWebSocket();
    CancellationTokenSource cts = new CancellationTokenSource();
    

    private void Awake()
    {
    }


    bool sendTrigger = false;
    string sendText = "";
    string message = "";
    Vector2 scrollPos = Vector2.zero;
    private void OnGUI()
    {
        if (GUILayout.Button("Connect"))
        {
            Task.Factory.StartNew(Connect, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        GUILayout.Label("Text: ");
        sendText = GUILayout.TextArea(sendText);

        if (GUILayout.Button("Send"))
        {
            sendTrigger = true;
        }

        if (GUILayout.Button("Close"))
        {
            Debug.Log("close");
            cts.Cancel();
        }

        GUILayout.Label("Message: ");
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.Label(message);
        GUILayout.EndScrollView();
    }

    private async Task Connect()
    {
        var uri = new Uri(url);
        Debug.LogError($"connect to: {url}");
        await socket.ConnectAsync(uri, cts.Token);
        Debug.LogError($"connected: {url}");
        await Task.Factory.StartNew(Receive, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        await Task.Factory.StartNew(Send, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async void Send()
    {
        while (true)
        {
            if (!sendTrigger)
            {
                Thread.Sleep(1);
                continue;
            }
            sendTrigger = false;
            byte[] sendBytes = Encoding.UTF8.GetBytes(sendText);
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            await socket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cts.Token);
            sendText = "";
        }
    }

    private async void Receive()
    {
        var rcvBytes = new byte[128];
        var rcvBuffer = new ArraySegment<byte>(rcvBytes);
        while (true)
        {
            WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(rcvBuffer, cts.Token);
            byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
            string rcvMsg = Encoding.UTF8.GetString(msgBytes);
            message += $"receive: {rcvMsg}\n";
        }
    }
}
