
[(English)](README_EN.md)

<div align=center>
  <img src="https://s1.ax1x.com/2020/08/21/dYIAQU.png" width=20%/>
</div>

## **在线示例**

- **[https://psygames.github.io/UnityWebSocket/](https://psygames.github.io/UnityWebSocket/)**


## **快速开始**

### **安装方法**

  - 在 [Releases](https://github.com/psygames/UnityWebSocket/releases) 页面中，下载最新版本的 `UnityWebSocket.unitypackage` 安装包，然后导入到您的项目中。


### **使用方法**

- 代码示例

  ```csharp
  // 命名空间
  using UnityWebSocket;

  // 创建实例
  string address = "ws://echo.websocket.org";
  WebSocket socket = new WebSocket(address);

  // 注册回调
  socket.OnOpen += OnOpen;
  socket.OnClose += OnClose;
  socket.OnMessage += OnMessage;
  socket.OnError += OnError;

  // 连接
  socket.ConnectAsync();

  // 发送 string 类型数据
  socket.SendAsync(str); 

  // 或者 发送 byte[] 类型数据（建议使用）
  socket.SendAsync(bytes); 

  // 关闭连接
  socket.CloseAsync();
  ```

- 更多使用方法可参考项目中的 [UnityWebSocketDemo.cs](Assets/UnityWebSocket/Demo/UnityWebSocketDemo.cs) 示例代码。

- 功能菜单：
  - Tools -> UnityWebSocket，版本更新检测，问题反馈渠道等。

- Unity 编译宏（可选项）：
  - `UNITY_WEB_SOCKET_LOG` 打开底层日志输出。
  - `UNITY_WEB_SOCKET_ENABLE_ASYNC` 针对非WebGL平台使用异步线程处理消息（需自行处理跨线程访问Unity组件问题）。


### **QQ 交流群**
- 1126457634 >>> [入群通道](https://qm.qq.com/cgi-bin/qm/qr?k=KcexYJ9aYwogFXbj2aN0XHH5b2G7ICmd) <<<
