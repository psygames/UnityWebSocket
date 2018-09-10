
﻿var WebSocketJS =
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
		RECEIVER_NAME = "WebSocketReceiver";
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
			OnError(address, e.message);
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
			OnError(address, "send msg with a WebSocket not Instantiated");
	},

	// call by unity
	SendStrJS: function (addressPtr, msgPtr)
	{
		var address = Pointer_stringify(addressPtr);
		var msg = Pointer_stringify(msgPtr);
		if(webSocketMap.has(address))
			webSocketMap.get(address).send(msg);
		else
			OnError(address, "send msg with a WebSocket not Instantiated");
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
		var addr_opcode_data = address + "_" + opcode + "_";
		// blobData
		if(opcode == 2)
		{
			var reader = new FileReader();
			reader.addEventListener("loadend", function()
			{
				// format : address_data, (address and data split with "_")
				// the data format is hex string
				var array = new Uint8Array(reader.result);
				for(var i = 0; i < array.length; i++)
				{
					var b = array[i];
					if(b < 16)
						addr_opcode_data += "0" + b.toString(16);
					else
						addr_opcode_data += b.toString(16);
				}
				SendMessage(RECEIVER_NAME, MESSAGE_METHOD_NAME, addr_opcode_data);
			});
			reader.readAsArrayBuffer(data);
		}
		else
		{
			addr_opcode_data += data;
			SendMessage(RECEIVER_NAME, MESSAGE_METHOD_NAME, addr_opcode_data);
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
		SendMessage(RECEIVER_NAME, CLOSE_METHOD_NAME, address+"_"+code+"_"+reason+"_"+wasClean);
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
