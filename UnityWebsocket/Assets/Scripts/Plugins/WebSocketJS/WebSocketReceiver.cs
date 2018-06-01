using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WebSocketJS
{
    class WebSocketReceiver : MonoBehaviour
    {
        public static WebSocketReceiver instance { get; private set; }

        private Dictionary<string, Action> m_openActions = new Dictionary<string, Action>();
        private Dictionary<string, Action> m_closeActions = new Dictionary<string, Action>();
        private Dictionary<string, Action<byte[]>> m_receiveActions = new Dictionary<string, Action<byte[]>>();

        void Awake()
        {
            instance = this;
        }


        public void AddListener(string address, Action onOpen, Action onClose, Action<byte[]> onReceive)
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
        /// jslib will call this method on message received (Method Name Must Be Keep Unchanged).
        /// </summary>
        /// <param name="address_data">address_data(hex string)</param>
        private void OnReceive(string address_data)
        {
            string address;
            byte[] data;
            MessageTranslator(address_data, out address, out data);

            if (m_receiveActions.ContainsKey(address) && m_receiveActions[address] != null)
            {
                m_receiveActions[address].Invoke(data);
            }
        }

        /// <summary>
        /// jslib will call this method on connection open (Method Name Must Be Keep Unchanged).
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
        /// jslib will call this method on connection closed (Method Name Must Be Keep Unchanged).
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
        /// Message Translator
        /// Data Format : "address_data", (data is hex string)
        /// address and data split with "_", it's deal with the jslib 
        /// </summary>
        private void MessageTranslator(string address_data, out string address, out byte[] data)
        {
            int splitIndex = address_data.LastIndexOf("_");
            address = address_data.Substring(0, splitIndex);
            string hexData = address_data.Substring(splitIndex + 1);
            data = new byte[hexData.Length / 2];
            for (int i = 0; i < data.Length; i++)
            {
                string hex = hexData.Substring(i * 2, 2);
                data[i] = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
        }
    }
}
