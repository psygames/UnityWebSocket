
﻿var WebSocketJS =
{
	$webSocket: {},
	ConnectJS: function(bAddress)
	{
		//unity receiver name and handle method name.
		var RECEIVER_NAME = "WebSocketReceiver";
		var OPEN_METHOD_NAME = "OnOpen";
		var CLOSE_METHOD_NAME = "OnClose";
		var RECEIVE_METHOD_NAME = "OnReceive";

		var address = Pointer_stringify(bAddress);
		webSocket = new WebSocket(address);
		webSocket.onmessage = function (e)
		{
			if (e.data instanceof Blob)
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
				reader.readAsArrayBuffer(e.data);
			}
			else
			{
				alert("msg not a blob instance");
			}
		};

		webSocket.onopen = function(e)
		{
			SendMessage(RECEIVER_NAME, OPEN_METHOD_NAME, address);
		};

		webSocket.onclose = function(e)
		{
			SendMessage(RECEIVER_NAME, CLOSE_METHOD_NAME , address);
		};
	},

	SendJS: function (msg, length)
	{
		webSocket.send(HEAPU8.buffer.slice(msg, msg + length));
	},

	CloseJS: function ()
	{
		webSocket.close();
	},

	AlertJS: function (bMsg)
	{
		var msg = Pointer_stringify(bMsg);
		alert(msg);
	}
};

autoAddDeps(WebSocketJS, '$webSocket');
mergeInto(LibraryManager.library, WebSocketJS);
