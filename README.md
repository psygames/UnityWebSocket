
# Demo 线上测试地址
- [http://47.100.28.149/UnityWebSocketDemo/](http://47.100.28.149/UnityWebSocketDemo/)
- WebSocket服务器(ServerDemo)监听地址：

        ws://47.100.28.149:8758/test
        ws://47.100.28.149:8759/test
        ws://47.100.28.149:8760/test

# UnityWebSocket 使用

### 1. [最新版本下载](https://github.com/y85171642/UnityWebSocket/releases)

### 2. 使用方法：
- 导入 UnityWebSocket.unitypackage

- 创建WebSocket实例

  ```csharp
  // 命名空间
  using UnityWebSocket;

  // 创建实例
  string address = "ws://127.0.0.1:8730/test";
  WebSocket scoket = new WebSocket(address);

  // 注册回调
  scoket.onOpen += OnOpen;
  scoket.onClose += OnClose;
  scoket.onMessage += OnMessage;
  socket.onError += OnError;

  // 连接
  socket.Connect();

  // 发送数据
  socket.Send(str); // 发送类型String数据
  socket.Send(bytes); // 发送byte[]类型数据

  // 关闭连接
  socket.Close();
  ```

- 详细使用方法可参考项目中的Example示例，或参考 [websocket-sharp](https://github.com/sta/websocket-sharp) 的使用方法。

### 3. 模块说明
- WebSocket.jslib
语法格式需要遵循 [asm.js](http://www.ruanyifeng.com/blog/2017/09/asmjs_emscripten.html)。

        路径：Plugins/WebSocketJS/WebSocketJS.jslib
        作用：Unity发布WebGL版本会将其加入到js运行库中。

- WebSocket.cs

        作用：WebSocket连接，可同时创建多个不同连接。
        已经支持全平台使用。

- WebSocketReceiver.cs

        作用：与jslib交互，负责收发多个WebSocket消息。
        该脚本在使用WebSocket时会自动加载到场景中，并添加到DonDestroyOnLoad。

- Example场景

        作用：WebSocket的使用方法示例。

### 4. 注意(Warning)
- Unity2018 以上版本需要修改WebGL平台 Publishing Settings -> Linker Target 为 asm.js。
- WebSocket的命名空间是 UnityWebSocket ，项目中有多个命名空间存在WebSocket类，不要用错了 :) 。
- WebSocket的 onOpen、OnClose、OnMessage、OnError 回调都发生在网络线程中，回调处理函数不能直接修改主线程中的Unity组件内容，需要在主线程中加消息处理队列（需要加锁），缓存网络消息后，再在主线程中处理消息包。
- WebGL平台下，暂时不能使用异步连接、关闭、发送，接口仍然使用的同步方式。
- WebGL平台下，需要将打包好的文件，发布到Tomcat等服务器上运行。
- ServerDemo 是用于示例版本的WebSocket测试服务器，需要使用对应的版本。
- v1.1 后版本加入了websocket-sharp插件（源码），如果你的项目已包含该插件，可自行删除或修改。

### 5. WebSocket服务器
- 提供简单的WebSocket消息收发
- 每个版本都包含对应的服务器（ServerDemo）。
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

#### v1.2
- 重构代码，规范代码，模块整理。
- 规范接口，参考websocket-sharp结构，使用EventHandler方式处理事件。
- 添加了字符串数据收发的支持。
- jslib中添加了获取socket.readyState的方法。
- jslib中的SendMessage参数整理。
- fix some Bugs.

#### v1.2.1
- fix 非WebGL平台打包兼容BUG（屏蔽websocket-jslib部分代码）
