using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityWebSocket
{
    public class MessageEventArgs : EventArgs
    {
        private string _data;
        private bool _isSetDataStr;

        internal MessageEventArgs(Opcode opcode, byte[] rawData)
        {
            Opcode = opcode;
            RawData = rawData;
        }

        /// <summary>
        /// Gets the opcode for the message.
        /// </summary>
        /// <value>
        /// <see cref="Opcode.Text"/>, <see cref="Opcode.Binary"/>,
        /// or <see cref="Opcode.Ping"/>.
        /// </value>
        internal Opcode Opcode { get; private set; }

        /// <summary>
        /// Gets the message data as a <see cref="string"/>.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the message data if its type is
        /// text or ping and if decoding it to a string has successfully done;
        /// otherwise, <see langword="null"/>.
        /// </value>
        public string Data
        {
            get
            {
                SetData();
                return _data;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the message type is binary.
        /// </summary>
        /// <value>
        /// <c>true</c> if the message type is binary; otherwise, <c>false</c>.
        /// </value>
        public bool IsBinary
        {
            get
            {
                return Opcode == Opcode.Binary;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the message type is ping.
        /// </summary>
        /// <value>
        /// <c>true</c> if the message type is ping; otherwise, <c>false</c>.
        /// </value>
        public bool IsPing
        {
            get
            {
                return Opcode == Opcode.Ping;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the message type is text.
        /// </summary>
        /// <value>
        /// <c>true</c> if the message type is text; otherwise, <c>false</c>.
        /// </value>
        public bool IsText
        {
            get
            {
                return Opcode == Opcode.Text;
            }
        }

        /// <summary>
        /// Gets the message data as an array of <see cref="byte"/>.
        /// </summary>
        /// <value>
        /// An array of <see cref="byte"/> that represents the message data.
        /// </value>
        public byte[] RawData { get; private set; }

        private void SetData()
        {
            if (_isSetDataStr || Opcode == Opcode.Binary)
            {
                _isSetDataStr = true;
                return;
            }

            _data = Encoding.UTF8.GetString(RawData);
            _isSetDataStr = true;
        }
    }
}