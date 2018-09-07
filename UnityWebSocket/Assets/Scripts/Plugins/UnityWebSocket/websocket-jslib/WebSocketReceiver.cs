using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WebSocketJslib
{
    public class WebSocketReceiver : MonoBehaviour
    {
        public static WebSocketReceiver instance { get; private set; }

        private Dictionary<string, Action> m_openActions = new Dictionary<string, Action>();
        private Dictionary<string, Action> m_closeActions = new Dictionary<string, Action>();
        private Dictionary<string, Action<byte[]>> m_receiveActions = new Dictionary<string, Action<byte[]>>();
        private Dictionary<string, Action<string>> m_errorActions = new Dictionary<string, Action<string>>();

        private WebSocketReceiver()
        { }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        public static void AutoCreateInstance()
        {
            GameObject go = GameObject.Find("/WebSocketReceiver");
            if (go == null)
            {
                go = new GameObject("WebSocketReceiver");
            }

            if (go.GetComponent<WebSocketReceiver>() == null)
            {
                go.AddComponent<WebSocketReceiver>();
            }

            instance = go.GetComponent<WebSocketReceiver>();
        }

        public void AddListener(string address, Action onOpen, Action onClose, Action<byte[]> onReceive, Action<string> onError)
        {
            if (!m_openActions.ContainsKey(address))
                m_openActions.Add(address, null);
            m_openActions[address] = onOpen;

            if (!m_closeActions.ContainsKey(address))
                m_closeActions.Add(address, null);
            m_closeActions[address] = onClose;

            if (!m_receiveActions.ContainsKey(address))
                m_receiveActions.Add(address, null);
            m_receiveActions[address] = onReceive;

            if (!m_errorActions.ContainsKey(address))
                m_errorActions.Add(address, null);
            m_errorActions[address] = onError;
        }

        public void RemoveListener(string address)
        {
            if (m_openActions.ContainsKey(address))
                m_openActions.Remove(address);

            if (m_closeActions.ContainsKey(address))
                m_closeActions.Remove(address);

            if (m_receiveActions.ContainsKey(address))
                m_receiveActions.Remove(address);
        }

        /// <summary>
        /// jslib will call this method on message received.
        /// </summary>
        /// <param name="address_data">address_opcode_data(hex string)</param>
        private void OnReceive(string address_opcode_data)
        {
            string[] spData;
            SplitData(address_opcode_data, 3, out spData);
            byte[] data = HexStrToBytes(spData[2]);
            if (m_receiveActions.ContainsKey(spData[0]) && m_receiveActions[spData[0]] != null)
            {
                m_receiveActions[spData[0]].Invoke(data);
            }
        }

        /// <summary>
        /// jslib will call this method on connection open.
        /// </summary>
        /// <param name="address">address</param>
        private void OnOpen(string address)
        {
            if (m_openActions.ContainsKey(address) && m_openActions[address] != null)
            {
                m_openActions[address].Invoke();
            }
        }

        /// <summary>
        /// jslib will call this method on connection closed.
        /// </summary>
        /// <param name="address">address</param>
        private void OnClose(string address)
        {
            if (m_closeActions.ContainsKey(address) && m_closeActions[address] != null)
            {
                m_closeActions[address].Invoke();
            }
        }

        /// <summary>
        /// jslib will call this method on error.
        /// </summary>
        /// <param name="address_data">address_data(text string)</param>
        private void OnError(string address_data)
        {
            string address;
            string errorMsg;
            SplitData(address_data, out address, out errorMsg);
            if (m_errorActions.ContainsKey(address) && m_errorActions[address] != null)
            {
                m_errorActions[address].Invoke(errorMsg);
            }
        }

        private void SplitData(string rawData, int length, out string[] retData)
        {
            retData = new string[length];
            int i = -1;
            int lastIndex = 0;
            while (++i < length)
            {
                var index = rawData.IndexOf('_', lastIndex);
                retData[i] = rawData.Substring(lastIndex, index - lastIndex);
                lastIndex = index + 1;
            }
        }

        private byte[] HexStrToBytes(string hexStr)
        {
            byte[] data = new byte[hexStr.Length / 2];
            for (int i = 0; i < data.Length; i++)
            {
                string hex = hexStr.Substring(i * 2, 2);
                data[i] = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
            return data;
        }
    }
}
