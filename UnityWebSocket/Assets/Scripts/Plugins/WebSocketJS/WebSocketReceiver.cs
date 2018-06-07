using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WebSocketJS
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
        /// <param name="address_data">address_data(hex string)</param>
        private void OnReceive(string address_data)
        {
            string address;
            string hexStr;
            SplitAddressData(address_data, out address, out hexStr);
            byte[] data = HexStrToBytes(hexStr);
            if (m_receiveActions.ContainsKey(address) && m_receiveActions[address] != null)
            {
                m_receiveActions[address].Invoke(data);
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
            SplitAddressData(address_data, out address, out errorMsg);
            if (m_errorActions.ContainsKey(address) && m_errorActions[address] != null)
            {
                m_errorActions[address].Invoke(errorMsg);
            }
        }

        /// <summary>
        /// Split address_data with '_'
        /// </summary>
        /// <param name="address_data">split with '_'</param>
        /// <param name="address"></param>
        /// <param name="data"></param>
        private void SplitAddressData(string address_data, out string address, out string data)
        {
            int splitIndex = address_data.LastIndexOf("_");
            address = address_data.Substring(0, splitIndex);
            data = address_data.Substring(splitIndex + 1);
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
