using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityWebSocket;
using System;

namespace UnityWebSocket.Example
{
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

        public GameObject messageBoxObj;
        public Text messagexBoxText;
        public Button messageBoxCloseBtn;

        private Dictionary<string, WebSocketEntry> m_sockets = new Dictionary<string, WebSocketEntry>();
        private WebSocketEntry m_selectedEntry;

        private void Awake()
        {
            newSocketBtn.onClick.AddListener(NewSocket);
            connentBtn.onClick.AddListener(Connect);
            closeBtn.onClick.AddListener(Close);
            sendBtn.onClick.AddListener(Send);
            messageBoxCloseBtn.onClick.AddListener(OnClickCloseMessageBox);
            entryTemplate.gameObject.SetActive(false);
            messageBoxObj.gameObject.SetActive(false);
        }

        public void NewSocket()
        {
            string addr = addressInput.text;
            if (m_sockets.ContainsKey(addr))
            {
                MessageBox("Duplicate address " + addr);
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
            entryItem.onClick.AddListener(() => { OnEntryItemClick(entryItem, entry); });

            if (m_selectedEntry == null)
            {
                ChangeSelected(entryItem);
                m_selectedEntry = entry;
            }
        }

        private void ChangeSelected(Button b)
        {
            foreach (Button btn in entryRoot.GetComponentsInChildren<Button>())
            {
                btn.image.color = b == btn ? Color.cyan : Color.white;
            }
        }

        private void OnEntryItemClick(Button sender, WebSocketEntry entry)
        {
            ChangeSelected(sender);
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

        public void MessageBox(string log)
        {
            messageBoxObj.SetActive(true);
            messagexBoxText.text = log;
        }

        private void OnClickCloseMessageBox()
        {
            messageBoxObj.SetActive(false);
        }

        void Update()
        {
            var text = "";
            var addr = "请选择服务器地址";
            var state = WebSocketState.Closed;

            if (m_selectedEntry != null)
            {
                text = m_selectedEntry.content;
                state = m_selectedEntry.socket.readyState;
                addr = m_selectedEntry.socket.address;
            }
            contentText.text = text;
            currentSelectText.text = addr;
            currentSelectBg.color = GetStateColor(state);
        }


        private Color GetStateColor(WebSocketState state)
        {
            switch (state)
            {
                case WebSocketState.Closed:
                    return Color.grey;
                case WebSocketState.Closing:
                    return Color.cyan;
                case WebSocketState.Connecting:
                    return Color.yellow;
                case WebSocketState.Open:
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
                socket.onMessage += OnReceive;
                socket.onError += OnError;
            }

            public void Connect()
            {
                if (socket == null
                    || socket.readyState == WebSocketState.Open
                    || socket.readyState == WebSocketState.Closing)
                    return;

                socket.Connect();
            }


            public void Close()
            {
                if (socket.readyState == WebSocketState.Connecting
                    || socket.readyState == WebSocketState.Open)
                {
                    socket.Close();
                }
            }

            public void Send(string text)
            {
                if (socket.readyState == WebSocketState.Open)
                {
                    socket.Send(text);
                    content += "[SEND] " + text + "\n";
                }
            }


            private void OnOpen(object sender, EventArgs e)
            {
                content += "[INFO] Connected\n";
            }

            private void OnClose(object sender, CloseEventArgs e)
            {
                if (e.statusCode != CloseStatusCode.NoStatus && e.statusCode != CloseStatusCode.Normal)
                    content += "[ERROR] " + e.Reason + " " + e.statusCode + "\n";
                else
                    content += "[INFO] Closed\n";
            }

            private void OnReceive(object sender, MessageEventArgs e)
            {
                content += "[RECEIVE] " + e.Data + "\n";
            }

            private void OnError(object sender, ErrorEventArgs e)
            {
                content += "[ERROR] " + e.Message + "\n";
            }
        }
    }
}