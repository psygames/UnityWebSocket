
[(English)](README_EN.md)

### Demo 线上测试地址

- [http://39.105.150.229/UnityWebSocket/](http://39.105.150.229/UnityWebSocket/)

### UnityWebSocket 使用

#### 1. 下载最新版本

- [https://github.com/y85171642/UnityWebSocket/releases](https://github.com/y85171642/UnityWebSocket/releases)

#### 2. 使用方法：

- 在 Unity 中导入 UnityWebSocket.unitypackage，需要：

      * 需要 Scripting Runtime Version = .Net 4.x
      * 需要 WebGL LinkerTarger = asm.js or Both

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

#### 3. 注意（Warning）

- 插件中多个命名空间中存在 **WebSocket** 类，适用不同环境。

  命名空间 | 平台 | 方式 |  说明  
  -|-|-|-
  UnityWebSocket.Synchronized | 全平台 | 同步(无阻塞) | **[推荐]** 无需考虑异步回调使用 Unity 组件的问题。
  UnityWebSocket.Uniform | 全平台 | 异步 | 需要考虑异步回调使用 Unity 组件的问题。
  UnityWebSocket.WebGL | WebGL平台 | 异步 | 仅支持WebGL平台下的通信。
  UnityWebSocket.NoWebGL | 非WebGL平台 | 异步  | 仅支持非WebGL平台下的通信。

#### 4. WebGL 模块说明

- WebSocket.jslib 语法格式需要遵循 [asm.js](http://www.ruanyifeng.com/blog/2017/09/asmjs_emscripten.html)。

      路径：Plugins/WebSocketJS/WebSocketJS.jslib
      作用：Unity发布WebGL版本会将其加入到js运行库中。

- WebSocketReceiver.cs

      作用：与 jslib 交互，负责收发多个WebSocket消息。
      该脚本在使用WebSocket时会自动加载到场景中，并添加到DonDestroyOnLoad。

- Example 场景

      作用：WebSocket的使用方法示例。

#### 5. WebSocket 服务器

- 使用官方提供的 Echo Test 服务器。参考 [Echo Test](http://www.websocket.org/echo.html)。

#### 6. QQ 交流群
- 1126457634  >>> [入群通道](https://qm.qq.com/cgi-bin/qm/qr?k=KcexYJ9aYwogFXbj2aN0XHH5b2G7ICmd) <<<
