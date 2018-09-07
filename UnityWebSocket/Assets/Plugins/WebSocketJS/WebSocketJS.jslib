
﻿var WebSocketJS =
{
	$RECEIVER_NAME:{},
	$OPEN_METHOD_NAME:{},
	$CLOSE_METHOD_NAME:{},
	$RECEIVE_METHOD_NAME:{},
	$RECEIVE_STRING_METHOD_NAME:{},
	$ERROR_METHOD_NAME:{},
	$webSocketMap: {},

	$Initialize: function()
	{
		webSocketMap = new Map();
		RECEIVER_NAME = "WebSocketReceiver";
		OPEN_METHOD_NAME = "OnOpen";
		CLOSE_METHOD_NAME = "OnClose";
		RECEIVE_METHOD_NAME = "OnReceive";
		RECEIVE_STRING_METHOD_NAME = "OnReceiveString";
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
				OnMessage(address, e.data);
			else if(typeOf event.data === String) {、
    		OnMessageString(address, e.data);
  		else
				OnError(address, "msg is not a blob instance");
		};

		webSocket.onopen = function(e)
		{
			OnOpen(address);
		};

		webSocket.onclose = function(e)
		{
			OnClose(address);
			if(e.code != 1000)
			{
				if(e.reason != null && e.reason.length > 0)
					OnError(address, e.reason);
				else
					OnError(address, GetCloseReason(e.code));
			}
		};

		webSocket.onerror = function(e)
		{
			// can not catch the error reason, only use for debug.
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
		if(webSocketMap.has(address))
			webSocketMap.get(address).readyState;
		else
			OnError(address, "get readyState with a WebSocket not Instantiated");
	},

	$OnMessageString: function(address, str)
	{
		SendMessage(RECEIVER_NAME, RECEIVE_STRING_METHOD_NAME, str);
	},

	$OnMessage: function(address, blobData)
	{
		var reader = new FileReader();
		reader.addEventListener("loadend", function()
		{
			// format : address_data, (address and data split with "_")
			// the data format is hex string
			var msg = address + "_";
			var array = new Uint8Array(reader.result);
			for(var i = 0; i < array.length; i++)
			{
				var b = array[i];
				if(b < 16)
					msg += "0" + b.toString(16);
				else
					msg += b.toString(16);
			}
			SendMessage(RECEIVER_NAME, RECEIVE_METHOD_NAME, msg);
		});
		reader.readAsArrayBuffer(blobData);
	},

	$OnOpen: function(address)
	{
		SendMessage(RECEIVER_NAME, OPEN_METHOD_NAME, address);
	},

	$OnClose: function(address, code, reason, wasClean)
	{
		if(webSocketMap.get(address))
			webSocketMap.delete(address);
		SendMessage(RECEIVER_NAME, CLOSE_METHOD_NAME, address, code, reason, wasClean);
	},

	$OnError: function(address, errorMsg)
	{
		var combinedMsg =  address + "_" + errorMsg;
		SendMessage(RECEIVER_NAME, ERROR_METHOD_NAME ,combinedMsg);
	},

	$GetCloseReason: function(code)
	{
		var error = "";
		switch (code)
		{
			case 1001:
				error = "Endpoint going away.";
				break;
			case 1002:
				error = "Protocol error.";
				break;
			case 1003:
				error = "Unsupported message.";
				break;
			case 1005:
				error = "No status.";
				break;
			case 1006:
				error = "Abnormal disconnection.";
				break;
			case 1009:
				error = "Data frame too large.";
				break;
			default:
				error = "Error Code " + code;
				break;
		}
		return error;
	},

};

// Auto add to depends
autoAddDeps(WebSocketJS, '$RECEIVER_NAME');
autoAddDeps(WebSocketJS, '$OPEN_METHOD_NAME');
autoAddDeps(WebSocketJS, '$CLOSE_METHOD_NAME');
autoAddDeps(WebSocketJS, '$RECEIVE_STRING_METHOD_NAME');
autoAddDeps(WebSocketJS, '$RECEIVE_METHOD_NAME');
autoAddDeps(WebSocketJS, '$webSocketMap');
autoAddDeps(WebSocketJS, '$Initialize');
autoAddDeps(WebSocketJS, '$OnMessage');
autoAddDeps(WebSocketJS, '$OnMessageString');
autoAddDeps(WebSocketJS, '$OnOpen');
autoAddDeps(WebSocketJS, '$OnClose');
autoAddDeps(WebSocketJS, '$OnError');
autoAddDeps(WebSocketJS, '$GetCloseReason');
mergeInto(LibraryManager.library, WebSocketJS);
