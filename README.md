
# Demo 线上测试地址
- [http://47.100.28.149/UnityWebSocketDemo/](http://47.100.28.149/UnityWebSocketDemo/)

# UnityWebSocket 使用

### 1. [最新版本下载](https://github.com/y85171642/UnityWebSocket/releases/latest)

### 2. 使用方法：
- 导入 UnityWebSocket.unitypackage

- 创建WebSocket实例

  ```csharp
  // 创建实例
  string address = "ws://127.0.0.1:8730/test";
  WebSocket scoket = new WebSocket(address);

  // 注册回调
  scoket.onOpen += OnOpen;
  scoket.onClose += OnClose;
  scoket.onReceive += OnReceive;

  // 连接
  socket.Connect();

  // 发送数据
  socket.Send(data);//发送数据类型byte[]

  // 关闭连接
  socket.Close();
  ```

### 3. 功能说明
- WebSocket.jslib

        路径：Plugins/WebSocketJS/WebSocketJS.jslib
        作用：Unity发布WebGL版本会将其加入到js运行库中。

- WebSocket.cs

        作用：WebSocket连接，可同时创建多个不同连接。

- WebSocketReceiver.cs

        作用：与jslib交互，负责收发多个WebSocket消息。
        该脚本在使用WebSocket时会自动加载到场景中，并添加为DonDestroyOnLoad。

- Demo场景

        作用：WebSocket的使用方法示例。

### 4. 注意(Warning)
- WebSocket的命名空间是 UnityWebSocket 不要用错了 :) 。
- WebSocket的 onOpen、OnClose、OnReceive 回调都发生在网络线程中，回调处理函数不能直接修改主线程中的Unity组件内容，需要在主线程中加消息处理队列，缓存网络消息后，再在主线程中处理消息包。
- WebGL平台下，需要发布到Tomcat等服务器上运行。
- ServerDemo 是用于Demo测试版本的WebSocket服务器，兼容所有Release版本的Demo。
- v1.1 后版本有使用websocket-sharp插件，如果本地已使用该插件，可自行修改或删除。

### 5. WebSocket服务器
- 项目发布完成后，需要一个WebSocket服务器收发消息，以下是Demo版本对应的服务器。
- [服务器Demo下载](https://github.com/y85171642/UnityWebSocket/tree/master/Release/Server)
- 提供简单的WebSocket消息收发
- 使用了开源项目 [websocket-sharp](https://github.com/sta/websocket-sharp)

### 6. 版本记录
#### v1.0
- 支持单客户端同时创建多个不同WebSocket链接。
- 添加OnError错误回调。错误码对应错误原因，参考jslib文件。
- 删除Alert功能（与WebSocket无关，按需求自行添加即可）。
- Close Event Code 作为链接断开错误信息处理。
- jslib 内容完善，增加Map管理websocket实例。
- 修改 State 枚举对应到WebSocket ReadyState。
- 添加 Release Demo Build 文件。

#### v1.1
- 多平台支持，使用websocket-sharp 开源插件。
- 完善项目命名空间，目录结构。
- WebSocket增加异步连接发送方法。（webgl平台下仍调用同步方式）
- 添加开发分支，git管理方式调整。
