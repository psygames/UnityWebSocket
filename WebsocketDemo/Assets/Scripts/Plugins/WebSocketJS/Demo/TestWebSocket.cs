using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WebSocketJS;

public class TestWebSocket : MonoBehaviour
{
    public Text receive;
    public InputField address;
    public InputField message;

    private WebSocket m_scoket;
    private string m_content = "Receive:\n";

    private void Awake()
    {
        m_scoket = new WebSocket(address.text);
        m_scoket.onOpen += OnOpen;
        m_scoket.onClose += OnClose;
        m_scoket.onReceive += OnReceive;
    }

    public void Connect()
    {
        if (m_scoket.state == WebSocket.State.Closed)
        {
            m_scoket.Connect();
        }
    }

    public void Close()
    {
        if (m_scoket.state == WebSocket.State.Connecting
            || m_scoket.state == WebSocket.State.Connected)
        {
            m_scoket.Close();
        }
    }

    public void Send()
    {
        if (m_scoket.state == WebSocket.State.Connected)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message.text);
            m_scoket.Send(data);
        }
    }

    public void OnOpen()
    {

    }

    public void OnClose()
    {
        m_scoket.Alert(" socket closed : " + m_scoket.address);
        m_content += "Closed";
    }

    public void OnReceive(byte[] data)
    {
        m_content += System.Text.Encoding.UTF8.GetString(data) + "\n";
    }

    void Update()
    {
        receive.text = m_content;
    }

    private void OnDestroy()
    {
        if (m_scoket != null)
        {
            m_scoket.onOpen -= OnOpen;
            m_scoket.onClose -= OnClose;
            m_scoket.onReceive -= OnReceive;
        }
    }
}
