namespace Ninja.WebSockets.Internal
{
    internal class WebSocketReadCursor
    {
        public WebSocketFrame WebSocketFrame { get; private set; }

        // Number of bytes read in the last read operation
        public int NumBytesRead { get; private set; }

        // Number of bytes remaining to read before we are done reading the entire frame
        public int NumBytesLeftToRead { get; private set; }

        public WebSocketReadCursor(WebSocketFrame frame, int numBytesRead, int numBytesLeftToRead)
        {
            WebSocketFrame = frame;
            NumBytesRead = numBytesRead;
            NumBytesLeftToRead = numBytesLeftToRead;
        }
    }
}
