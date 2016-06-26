using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using YLWebSocket;

public class TestWebSocket : MonoBehaviour
{
    public Text receive;
    public InputField address;
    public InputField message;

    private WebSocket m_scoket;
    private string m_currentReceiveStr;

    public void Connect()
    {
        m_scoket = WebSocketManager.instance.GetSocket(address.text);
        if (m_scoket.state == WebSocket.State.Closed)
        {
            m_scoket.onConnected += OnConnected;
            m_scoket.onClosed += OnClosed;
            m_scoket.onReceived += OnReceived;
            Debug.Log("connect to " + m_scoket.address);
            m_scoket.Connect();
        }
    }

    public void Close()
    {
        if (m_scoket.state == WebSocket.State.Connecting
            || m_scoket.state == WebSocket.State.Connected)
        {
            Debug.Log("close link " + m_scoket.address);
            m_scoket.Close();
        }
    }

    public void Send()
    {
        if (m_scoket.state == WebSocket.State.Connected)
            m_scoket.Send(SimpleMessagePackTool.Pack(message.text));
    }

    public void OnConnected()
    {
        Debug.Log("connected on " + m_scoket.address);
    }

    public void OnClosed()
    {
        m_scoket.onConnected -= OnConnected;
        m_scoket.onClosed -= OnClosed;
        m_scoket.onReceived -= OnReceived;
        Debug.Log("closed on " + m_scoket.address);
    }

    public void OnReceived(byte[] data)
    {
        /*
        Cause of the message receive in the network thread 
        so we should cached the message here.             
        suggest use message queue to cached the message 
        we just simple cached.                               
        这里因为多线程原因 要在网络线程里缓存接收到的数据      
        此处只做简单处理 建议使用消息队列
          */
        m_currentReceiveStr += "\n" + SimpleMessagePackTool.Unpack(data);
    }

    void Update()
    {
        receive.text += m_currentReceiveStr;
        m_currentReceiveStr = "";
    }
}
