using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityWebSocket;
using System;

public class TestWebSocket : MonoBehaviour
{
    public Button newSocketBtn;
    public Button connentBtn;
    public Button closeBtn;
    public Button sendBtn;
    public Text contentText;
    public InputField addressInput;
    public InputField messageInput;
    public Transform entryRoot;
    public Button entryTemplate;
    public Image currentSelectBg;
    public Text currentSelectText;

    public GameObject logPanel;
    public Text logText;
    public Button logPanelCloseBtn;

    private Dictionary<string, WebSocketEntry> m_sockets = new Dictionary<string, WebSocketEntry>();
    private WebSocketEntry m_selectedEntry;

    private void Awake()
    {
        newSocketBtn.onClick.AddListener(NewSocket);
        connentBtn.onClick.AddListener(Connect);
        closeBtn.onClick.AddListener(Close);
        sendBtn.onClick.AddListener(Send);
        logPanelCloseBtn.onClick.AddListener(OnClickCloseLog);
        entryTemplate.gameObject.SetActive(false);
        logPanel.gameObject.SetActive(false);
    }

    public void NewSocket()
    {
        string addr = addressInput.text;
        if (m_sockets.ContainsKey(addr))
        {
            Log("Duplicate address " + addr);
            return;
        }

        WebSocketEntry entry = new WebSocketEntry(addr);
        m_sockets.Add(addr, entry);

        Button entryItem = GameObject.Instantiate(entryTemplate);
        entryItem.GetComponentInChildren<Text>().text = addr;
        entryItem.gameObject.SetActive(true);
        entryItem.transform.SetParent(entryRoot);
        entryItem.transform.localScale = Vector3.one;
        entryItem.transform.localRotation = Quaternion.identity;
        entryItem.onClick.AddListener(() => { OnEntryItemClick(entry); });
    }

    private void OnEntryItemClick(WebSocketEntry entry)
    {
        m_selectedEntry = entry;
    }

    public void Connect()
    {
        if (m_selectedEntry == null)
            return;

        m_selectedEntry.Connect();
    }

    public void Close()
    {

        if (m_selectedEntry == null)
            return;
        m_selectedEntry.Close();
    }

    public void Send()
    {
        if (m_selectedEntry == null)
            return;
        m_selectedEntry.Send(messageInput.text);
    }

    public void Log(string log)
    {
        logPanel.SetActive(true);
        logText.text = log;
    }

    private void OnClickCloseLog()
    {
        logPanel.SetActive(false);
    }

    void Update()
    {
        var text = "";
        var addr = "请选择服务器地址";
        var state = WebSocket.State.Closed;

        if (m_selectedEntry != null)
        {
            text = m_selectedEntry.content;
            state = m_selectedEntry.socket.state;
            addr = m_selectedEntry.socket.address;
        }
        contentText.text = text;
        currentSelectText.text = addr;
        currentSelectBg.color = GetStateColor(state);
    }


    private Color GetStateColor(WebSocket.State state)
    {
        switch (state)
        {
            case WebSocket.State.Closed:
                return Color.grey;
            case WebSocket.State.Closing:
                return Color.cyan;
            case WebSocket.State.Connecting:
                return Color.yellow;
            case WebSocket.State.Connected:
                return Color.green;
        }
        return Color.white;
    }


    class WebSocketEntry
    {
        public WebSocket socket { get; private set; }
        public string content { get; private set; }

        public WebSocketEntry(string address)
        {
            socket = new WebSocket(address);
            socket.onOpen += OnOpen;
            socket.onClose += OnClose;
            socket.onReceive += OnReceive;
            socket.onError += OnError;
        }

        public void Connect()
        {
            if (socket == null || socket.state != WebSocket.State.Closed)
                return;

            socket.Connect();
        }


        public void Close()
        {
            if (socket.state == WebSocket.State.Connecting
                || socket.state == WebSocket.State.Connected)
            {
                socket.Close();
            }
        }

        public void Send(string text)
        {
            if (socket.state == WebSocket.State.Connected)
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
                socket.Send(data);
                content += "[SEND] " + text + "\n";
            }
        }


        private void OnOpen()
        {
            content += "[INFO] Connected\n";
        }

        private void OnClose()
        {
            content += "[INFO] Closed\n";
        }

        private void OnReceive(byte[] data)
        {
            content += "[RECEIVE] " + System.Text.Encoding.UTF8.GetString(data) + "\n";
        }

        private void OnError(string errMsg)
        {
            content += "[ERROR] " + errMsg + "\n";
        }


        Vector3[] CreatePoint(Vector3 start, Vector3 end, float distance)
        {
            Vector3[] retPoint = new Vector3[4];

            Vector3 v3 = Vector3.Cross(end - start, Vector3.up);

            retPoint[0] = start + v3 * distance;
            retPoint[1] = start - v3 * distance;
            retPoint[3] = end + v3 * distance;
            retPoint[2] = end - v3 * distance;

            return retPoint;
        }

    }


}


