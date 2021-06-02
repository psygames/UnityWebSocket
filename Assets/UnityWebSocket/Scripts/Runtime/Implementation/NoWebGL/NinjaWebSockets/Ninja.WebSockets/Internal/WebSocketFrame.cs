using System;
using System.Net.WebSockets;

namespace Ninja.WebSockets.Internal
{
    internal class WebSocketFrame
    {
        public bool IsFinBitSet { get; private set; }

        public WebSocketOpCode OpCode { get; private set; }

        public int Count { get; private set; }

        public WebSocketCloseStatus? CloseStatus { get; private set; }

        public string CloseStatusDescription { get; private set; }

        public ArraySegment<byte> MaskKey { get; private set; }

        public WebSocketFrame(bool isFinBitSet, WebSocketOpCode webSocketOpCode, int count, ArraySegment<byte> maskKey)
        {
            IsFinBitSet = isFinBitSet;
            OpCode = webSocketOpCode;
            Count = count;
            MaskKey = maskKey;
        }

        public WebSocketFrame(bool isFinBitSet, WebSocketOpCode webSocketOpCode, int count, WebSocketCloseStatus closeStatus, string closeStatusDescription, ArraySegment<byte> maskKey) : this(isFinBitSet, webSocketOpCode, count, maskKey)
        {
            CloseStatus = closeStatus;
            CloseStatusDescription = closeStatusDescription;
        }
    }
}
