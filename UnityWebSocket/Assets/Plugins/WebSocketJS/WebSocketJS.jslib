
﻿var WebSocketJS =
{
	$RECEIVER_NAME:{},
	$OPEN_METHOD_NAME:{},
	$CLOSE_METHOD_NAME:{},
	$RECEIVE_METHOD_NAME:{},
	$webSocketMap: {},

	Initialize: function()
	{
		webSocketMap = new Map();
		RECEIVER_NAME = "WebSocketReceiver";
		OPEN_METHOD_NAME = "OnOpen";
		CLOSE_METHOD_NAME = "OnClose";
		RECEIVE_METHOD_NAME = "OnReceive";
		ERROR_METHOD_NAME = "OnError";

		alert("Inited");
	}

	ConnectJS: function(bAddress)
	{
		if(webSocketMap == null)
			Initialize();

		var address = Pointer_stringify(bAddress);
		var webSocket = null;
		if(!webSocketMap.has(address))
		{
		  webSocket = new WebSocket(address);
			webSocketMap.set(address, webSocket);
		}
		else
		{
			webSocket = webSocketMap.get(address);
		}

		webSocket.onmessage = function (e)
		{
			if (e.data instanceof Blob)
				OnMessage(address, e.data);
			else
				OnError(address, "msg not a blob instance");
		};

		webSocket.onopen = function(e)
		{
			OnOpen(address);
		};

		webSocket.onclose = function(e)
		{
			OnClose(address);
		};
	},

	SendJS: function (address, msg, length)
	{
		if(webSocketMap.has(address))
			webSocketMap.get(address).send(HEAPU8.buffer.slice(msg, msg + length));
		else
			OnError(address, "send msg with a WebSocket not Instantiated");
	},

	CloseJS: function (address)
	{
		if(webSocketMap.has(address))
			webSocket.close();
		else
			OnError(address, "close with a WebSocket not Instantiated");
	},

	OnMessage: function(address, blobData)
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

	OnOpen: function(address)
	{
		SendMessage(RECEIVER_NAME, OPEN_METHOD_NAME, address);
	},

	OnClose: function(address)
	{
		SendMessage(RECEIVER_NAME, CLOSE_METHOD_NAME , address);
	},

	OnError: function(address, errorMsg)
	{
		var combinedMsg =  address + "_" + errorMsg;
		SendMessage(RECEIVER_NAME, ERROR_METHOD_NAME ,combinedMsg);
	},

};

autoAddDeps(WebSocketJS, '$webSocketMap');
mergeInto(LibraryManager.library, WebSocketJS);
