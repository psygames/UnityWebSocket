var WebSocketJS =
{
    $RECEIVER_NAME: {},
    $OPEN_METHOD_NAME: {},
    $CLOSE_METHOD_NAME: {},
    $MESSAGE_METHOD_NAME: {},
    $ERROR_METHOD_NAME: {},
    $webSocketMap: {},

    $Initialize: function()
    {
        webSocketMap = new Map();
        RECEIVER_NAME = "[WebSocketReceiver]";
        OPEN_METHOD_NAME = "OnOpen";
        CLOSE_METHOD_NAME = "OnClose";
        MESSAGE_METHOD_NAME = "OnMessage";
        ERROR_METHOD_NAME = "OnError";
    },

    // call by unity
    ConnectJS: function(addressaPtr)
    {
        if(!(webSocketMap instanceof Map))
            Initialize();

        var address = Pointer_stringify(addressaPtr);
        if(webSocketMap.has(address))
        {
            OnError(address, "Duplicated address: " + address);
            return;
        }

        var webSocket = new WebSocket(address);
        webSocket.onmessage = function(e)
        {
            if (e.data instanceof Blob)
                OnMessage(address, 2, e.data);
            else if(typeof e.data == 'string')
                OnMessage(address, 1, e.data);
            else
                OnError(address, "onmessage can not recognize msg type !");
        };

        webSocket.onopen = function(e)
        {
            OnOpen(address);
        };

        webSocket.onclose = function(e)
        {
            OnClose(address, e.code, e.reason, e.wasClean);
        };

        webSocket.onerror = function(e)
        {
            // can not catch the error reason, only use for debug.
            // see this page  https://stackoverflow.com/questions/18803971/websocket-onerror-how-to-read-error-description
            OnError(address, "a websocket error occured.");
        };

        webSocketMap.set(address, webSocket);
    },

    // call by unity
    SendJS: function (addressPtr, msgPtr, length)
    {
        var address = Pointer_stringify(addressPtr);
        if(webSocketMap.has(address))
            webSocketMap.get(address).send(HEAPU8.buffer.slice(msgPtr, msgPtr + length));
        else
            OnError(address, "send msg binary with a WebSocket not Instantiated");
    },

    // call by unity
    SendStrJS: function (addressPtr, msgPtr)
    {
        var address = Pointer_stringify(addressPtr);
        var msg = Pointer_stringify(msgPtr);
        if(webSocketMap.has(address))
            webSocketMap.get(address).send(msg);
        else
            OnError(address, "send msg string with a WebSocket not Instantiated");
    },

    // call by unity
    CloseJS: function (addressPtr)
    {
        var address = Pointer_stringify(addressPtr);
        if(webSocketMap.has(address))
            webSocketMap.get(address).close();
        else
            OnError(address, "close with a WebSocket not Instantiated");
    },

    // call by unity
    GetReadyStateJS: function (addressPtr)
    {
        var address = Pointer_stringify(addressPtr);
        if(!(webSocketMap instanceof Map))
            return 0;
        if(webSocketMap.has(address))
             return webSocketMap.get(address).readyState;
        return 0;
    },

    $OnMessage: function(address, opcode, data)
    {
        var combinedMsg = address + "_" + opcode + "_";
        if(opcode == 2) // blob data
        {
            var reader = new FileReader();
            reader.addEventListener("loadend", function()
            {
                // data format to hex string
                var array = new Uint8Array(reader.result);
                for(var i = 0; i < array.length; i++)
                {
                    var b = array[i];
                    if(b < 16)
                        combinedMsg += "0" + b.toString(16);
                    else
                        combinedMsg += b.toString(16);
                }
                SendMessage(RECEIVER_NAME, MESSAGE_METHOD_NAME, combinedMsg);
            });
            reader.readAsArrayBuffer(data);
        }
        else // string data
        {
            combinedMsg += data;
            SendMessage(RECEIVER_NAME, MESSAGE_METHOD_NAME, combinedMsg);
        }
    },

    $OnOpen: function(address)
    {
        SendMessage(RECEIVER_NAME, OPEN_METHOD_NAME, address);
    },

    $OnClose: function(address, code, reason, wasClean)
    {
        if(webSocketMap.get(address))
            webSocketMap.delete(address);
        var combinedMsg = address + "_" + code + "_" + reason + "_" + wasClean;
        SendMessage(RECEIVER_NAME, CLOSE_METHOD_NAME, combinedMsg);
    },

    $OnError: function(address, errorMsg)
    {
        var combinedMsg =  address + "_" + errorMsg;
        SendMessage(RECEIVER_NAME, ERROR_METHOD_NAME, combinedMsg);
    },
};

// Auto add to depends
autoAddDeps(WebSocketJS, '$RECEIVER_NAME');
autoAddDeps(WebSocketJS, '$OPEN_METHOD_NAME');
autoAddDeps(WebSocketJS, '$CLOSE_METHOD_NAME');
autoAddDeps(WebSocketJS, '$MESSAGE_METHOD_NAME');
autoAddDeps(WebSocketJS, '$ERROR_METHOD_NAME');
autoAddDeps(WebSocketJS, '$webSocketMap');
autoAddDeps(WebSocketJS, '$Initialize');
autoAddDeps(WebSocketJS, '$OnMessage');
autoAddDeps(WebSocketJS, '$OnOpen');
autoAddDeps(WebSocketJS, '$OnClose');
autoAddDeps(WebSocketJS, '$OnError');
mergeInto(LibraryManager.library, WebSocketJS);
