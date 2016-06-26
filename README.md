# YLWebSocket
Unity Lightweight WebSocket for WebGL Platform.
#Unity WebSocket 使用

###1.下载 [YLWebSocket.unitypackage]()。

###2.使用Unity导入package。

- WebSocket.jslib 

路径：Plugins/YLWebSocket/WebSocketJS.jslib

作用：Unity发布WebGL版本会将其加入到js运行库中。

- WebSocket.cs

作用：作为一个WebSocket连接。

- WebSocketManager.cs

作用：创建、管理WebSocket的使用，并且负责接收、分发多个WebSocket消息。

- Demo场景

作用：WebSocket的使用方法示例。

- SimpleMessagePackTool.cs

作用：简单的将 UTF8字符串 和 byte[] 之间相互转换。

###3.使用方法：

- 创建WebSocket实例

```csharp 

string address = "ws://127.0.0.1:8730/test";

WebSocket scoket = WebSocketManager.instance.GetSocket(address);

```

- 注册回调

```


socket.onConnected += OnConnected;

socket.onClosed += OnClosed;

socket.onReceived += OnReceived;//接收数据类型byte[]

```

- 连接

```

socket.Connect();

```

- 发送数据

```

socket.Send(data);//发送数据类型byte[]

```

- 关闭连接

```

socket.Close();

```

###4.下载 [服务器Demo]()

- 提供简单的WebSocket消息收发

- 使用了开源项目 [websocket-sharp]()

