
[(中文版)](README.md)

## Demo 线上测试地址
- [http://39.105.150.229/UnityWebSocketDemo/](http://39.105.150.229/UnityWebSocketDemo/)

## UnityWebSocket 使用

### 1. [最新版本下载](https://github.com/y85171642/UnityWebSocket/releases)

### 2. 使用方法：


- 在 Unity 中导入 UnityWebSocket.unitypackage

      需要 Scripting Runtime Version = .Net 4.x

      需要 WebGL LinkerTarger = asm.js or Both

- 使用 WebSocket

  ```csharp
  // 命名空间
  using UnityWebSocket;
  using UnityWebSocket.Synchronized;

  // 创建实例
  WebSocket scoket = new WebSocket();

  // 注册回调
  scoket.OnOpen += OnOpen;
  scoket.OnClose += OnClose;
  scoket.OnMessage += OnMessage;
  socket.OnError += OnError;

  // 连接
  string address = "ws://echo.websocket.org";
  socket.ConnectAsync(address);

  // 发送数据（两种发送方式）
  socket.SendAsync(str); // 发送类型 String 类型数据
  socket.SendAsync(bytes); // 发送 byte[] 类型数据

  // 关闭连接
  socket.CloseAsync();
  ```

- 详细使用方法可参考项目中的 [Example](UnityWebSocket/Assets/Scripts/Plugins/UnityWebSocket/Example/TestWebSocket.cs) 示例代码。

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
- Unity2018 以上版本需要修改WebGL平台 Publishing Settings -> Linker Target 为 asm.js 或 Both。
- 插件中多个命名空间中存在 **WebSocket** 类，适用不同环境。

  命名空间 | 平台 | 方式 |  说明  
  -|-|-|-
  UnityWebSocket.Synchronized | 全平台 | 同步(无阻塞) | **[推荐]** 无需考虑异步回调使用 Unity 组件的问题。
  UnityWebSocket.Uniform | 全平台 | 异步 | 需要考虑异步回调使用 Unity 组件的问题。
  UnityWebSocket.WebGL | WebGL平台 | 异步 | 仅支持WebGL平台下的通信。
  UnityWebSocket.NoWebGL | 非WebGL平台 | 异步  | 仅支持非WebGL平台下的通信。

### 5. WebSocket服务器
- 使用官方提供的 Echo Test 服务器。参考 [Echo Test](http://www.websocket.org/echo.html)。

### 6. 版本记录

#### v2.0
- 移除 websocket-sharp 插件，使用 .Net 4.x 内置的 ClientWebSocket 作为非 WebGL 平台下 WebSocket 插件。
- 添加**同步方式**的WebSocket ，使用者不必再考虑**异步回调**中使用 Unity 组件的问题。

#### v1.3.2
- 修复 非ssl连接，使用sslConfiguration bug。

#### v1.3.1
- 修复 Tls error，添加默认协议 Tls，Tls11，Tls12。

#### v1.3
- 移除服务器Demo，改用 [websocket-sharp](http://www.websocket.org/echo.html) 官方提供的测试服务器。
- 添加 PlayerSetting -> Linker Target 属性检测。

#### v1.2.2 - pre
- support for wss(ssl) 支持SSL协议格式（更新了websocket-sharp源码）。
- 服务器Demo尚未支持 SSL。会在未来版本支持。

#### v1.2.1
- fix 非WebGL平台打包兼容BUG（屏蔽websocket-jslib部分代码）。

#### v1.2
- 重构代码，规范代码，模块整理。
- 规范接口，参考websocket-sharp结构，使用EventHandler方式处理事件。
- 添加了字符串数据收发的支持。
- jslib中添加了获取socket.readyState的方法。
- jslib中的SendMessage参数整理。
- fix some Bugs.

#### v1.1
- 多平台支持，使用websocket-sharp 开源插件。
- 完善项目命名空间，目录结构。
- WebSocket增加异步连接发送方法。（webgl平台下仍调用同步方式）
- 添加开发分支，git管理方式调整。

#### v1.0
- 支持单客户端同时创建多个不同WebSocket链接。
- 添加OnError错误回调。错误码对应错误原因，参考jslib文件。
- 删除Alert功能（与WebSocket无关，按需求自行添加即可）。
- Close Event Code 作为链接断开错误信息处理。
- jslib 内容完善，增加Map管理websocket实例。
- 修改 State 枚举对应到WebSocket ReadyState。
- 添加 Release Demo Build 文件。
