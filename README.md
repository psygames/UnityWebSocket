# UnityWebSocket 使用

### 1. 下载 [UnityWebSocket.unitypackage](https://github.com/y85171642/UnityWebSocket/blob/master/Release/UnityWebSocket.unitypackage?raw=true)。

### 2. 使用Unity导入package。
- WebSocket.jslib
        路径：Plugins/WebSocketJS/WebSocketJS.jslib
        作用：Unity发布WebGL版本会将其加入到js运行库中。

- WebSocket.cs
        作用：作为一个WebSocket连接。

- WebSocketReceiver.cs
        作用：与jslib交互，负责收发多个WebSocket消息。
        注意：该脚本必须挂在场景中作为根节点，且名为WebSocketReceiver的GameObject上，
        这样才能接收到jslib通过SendMessage方式发来消息 ！！！

- Demo场景
        作用：WebSocket的使用方法示例。


### 3. 使用方法：

- 创建WebSocket实例
  ```csharp
  string address = "ws://127.0.0.1:8730/test";
  WebSocket scoket = new WebSocket(address);
  ```

- 注册回调
  ```csharp
  scoket.onOpen += OnOpen;
  scoket.onClose += OnClose;
  scoket.onReceive += OnReceive;
  ```

- 连接
  ```csharp
  socket.Connect();
  ```

- 发送数据
  ```csharp
  socket.Send(data);//发送数据类型byte[]
  ```

- 关闭连接
  ```csharp
  socket.Close();
  ```

### 4. 发布
- 需要将Unity项目切换为WebGL平台，并Build。
- 将生成好的项目文件发布至Tomcat，启动Tomcat，在浏览器中打开相应链接路径。（例如：http://127.0.0.1/UnityWebSocketDemo/ ）

### 5. WebSocket服务器
- 项目发布完成后，需要一个WebSocket服务器收发消息，以下是Demo版本对应的服务器。
- [服务器Demo下载](https://github.com/y85171642/UnityWebSocket/blob/master/Release/TestWebSocketServer.exe?raw=true)
- 提供简单的WebSocket消息收发
- 使用了开源项目 [websocket-sharp](https://github.com/sta/websocket-sharp)

### 6. 再次提醒
` 注意 `：WebSocketReceiver 脚本必须挂在场景中作为根节点，且名为 WebSocketReceiver 的 GameObject 上，
这样才能接收到 jslib 通过 SendMessage 方式发来消息 ！！！
