
[(English)](README_EN.md)

## 在线示例

- [http://39.105.150.229/UnityWebSocket/](http://39.105.150.229/UnityWebSocket/)

## 安装

#### 需要

- Unity 2018.3 或更高。
- 无其他SDK依赖。

#### 使用 OpenUPM 安装

- SDK已上传至 [OpenUPM](https://openupm.com).
您可以使用 [openupm-cli](https://github.com/openupm/openupm-cli) 进行安装。
  ```
  openupm add com.psygame.unitywebsocket
  ```


#### 使用 Git 安装

- 在您的项目根路径的 Packages 文件夹中找到 manifest.json 文件，参考如下方式进行修改:
  ```js
  {
   "dependencies": {
   "com.psygame.unitywebsocket": "https://github.com/psygame/UnityWebSocket.git",
   ...
   },
  }
  ```

- 可通过修改链接后缀 `#{version}` 来安装对应版本.
  * 示例： `"com.psygame.unitywebsocket": "https://github.com/psygame/UnityWebSocket.git#2.2.0",`


- 或使用 [UpmGitExtension](https://github.com/mob-sakai/UpmGitExtension) 来安装SDK。

#### 使用 Unity Package 安装

-  在 [Releases](https://github.com/psygame/UnityWebSocket/releases) 页面中，下载对应版本的 UnityWebSocket.unitypackage 安装包。
- 在您的项目中导入安装包。


## 使用方法

- 需要如下设置：

      * 需要 Scripting Runtime Version = .Net 4.x
      * 需要 WebGL LinkerTarger = asm.js or Both

- 使用 WebSocket

  ```csharp
  // 命名空间
  using UnityWebSocket;

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

- 详细使用方法可参考项目中的 [UnityWebSocketTest.cs](Tests/UnityWebSocketTest.cs) 示例代码。

#### 3. 注意（Warning）

- 插件中多个命名空间中存在 **WebSocket** 类，适用不同环境。

  命名空间 | 平台 | 方式 |  说明  
  -|-|-|-
  UnityWebSocket | 全平台 | 同步(无阻塞) | **[推荐]** 无需考虑异步回调使用 Unity 组件的问题。
  UnityWebSocket.Uniform | 全平台 | 异步 | 需要考虑异步回调使用 Unity 组件的问题。
  UnityWebSocket.WebGL | WebGL平台 | 异步 | 仅支持WebGL平台下的通信。
  UnityWebSocket.NoWebGL | 非WebGL平台 | 异步  | 仅支持非WebGL平台下的通信。

#### 4. WebGL 模块说明

- WebSocket.jslib 语法格式需要遵循 [asm.js](http://www.ruanyifeng.com/blog/2017/09/asmjs_emscripten.html)。

      路径：Plugins/WebGL/WebSocket.jslib
      作用：Unity发布WebGL版本会将其加入到js运行库中。

- Example 场景

      作用：WebSocket的使用方法示例。

#### 5. WebSocket 服务器

- 使用官方提供的 Echo Test 服务器。参考 [Echo Test](http://www.websocket.org/echo.html)。

#### 6. QQ 交流群
- 1126457634  >>> [入群通道](https://qm.qq.com/cgi-bin/qm/qr?k=KcexYJ9aYwogFXbj2aN0XHH5b2G7ICmd) <<<
