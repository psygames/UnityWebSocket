
# Demo 线上测试地址
- [http://47.100.28.149/UnityWebSocketDemo/](http://47.100.28.149/UnityWebSocketDemo/)

# DEV FOR MULTI PLATFORMS
# UnityWebSocket 使用

### 1. 最新Release版本下载 [UnityWebSocket.unitypackage](https://github.com/y85171642/UnityWebSocket/blob/master/Release/UnityWebSocket.unitypackage?raw=true)。

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
        该脚本在使用WebSocket时会自动加载到场景中。

- Demo场景

        作用：WebSocket的使用方法示例。


### 4. 发布
- 需要将Unity项目切换为WebGL平台，并Build。

- 将生成好的项目文件发布至Tomcat，启动Tomcat，在浏览器中打开相应链接路径。（例如：http://127.0.0.1/UnityWebSocketDemo/index.html ）

### 5. WebSocket服务器
- 项目发布完成后，需要一个WebSocket服务器收发消息，以下是Demo版本对应的服务器。

- [服务器Demo下载](https://github.com/y85171642/UnityWebSocket/tree/master/Release/Demo/Server)

- 提供简单的WebSocket消息收发

- 使用了开源项目 [websocket-sharp](https://github.com/sta/websocket-sharp)

### 6. 注意(Warning)
- 切换场景时，请不要卸载WebSocketReceiver。如果卸载了，请销毁WebSocket连接，并重新创建。

### 7. Change Log（v1.0）
- 支持单客户端同时创建多个不同WebSocket链接。
- 添加OnError错误回调。错误码对应错误原因，参考jslib文件。
- 删除Alert功能（与WebSocket无关，按需求自行添加即可）。
- Close Event Code 作为链接断开错误信息处理。
- jslib 内容完善，增加Map管理websocket实例。
- 修改 State 枚举对应到WebSocket ReadyState。
- 添加 Release Demo Build 文件。
