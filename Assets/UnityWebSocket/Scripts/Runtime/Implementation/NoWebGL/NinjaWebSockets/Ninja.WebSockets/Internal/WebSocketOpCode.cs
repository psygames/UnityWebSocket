#if !NET_LEGACY && (UNITY_EDITOR || !UNTIY_WEBGL)
namespace Ninja.WebSockets.Internal
{
    internal enum WebSocketOpCode
    {
        ContinuationFrame = 0,
        TextFrame = 1,
        BinaryFrame = 2,
        ConnectionClose = 8,
        Ping = 9,
        Pong = 10
    }
}

#endif
