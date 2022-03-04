
[(中文版)](README.md)

<div align=center>
  <img src="https://s1.ax1x.com/2020/08/21/dYIAQU.png" width=20%/>
</div>

## **Online Demo**

- **[https://psygames.github.io/UnityWebSocket/](https://psygames.github.io/UnityWebSocket/)**


## **Quick Start**

### **Installation**

- Download latest `UnityWebSocket.unitypackage` file from [Releases](https://github.com/psygames/UnityWebSocket/releases) page.
  Import it into your Unity project.


### **Usage**

- Easy to use

  ```csharp
  // the namespace
  using UnityWebSocket;

  // create instance
  string address = "ws://echo.websocket.org";
  WebSocket socket = new WebSocket(address);

  // register callback
  socket.OnOpen += OnOpen;
  socket.OnClose += OnClose;
  socket.OnMessage += OnMessage;
  socket.OnError += OnError;

  // connect
  socket.ConnectAsync();

  // send string data 
  socket.SendAsync(str);
  // or send byte[] data (suggested)
  socket.SendAsync(bytes); 

  // close connection
  socket.CloseAsync();
  ```

- More detail usages, see the [UnityWebSocketDemo.cs](Assets/UnityWebSocket/Demo/UnityWebSocketDemo.cs) code in project.

- Menus
  - Tools -> UnityWebSocket, version update check, bug report, etc.

- Unity Define Symbols(Optional):
  - `UNITY_WEB_SOCKET_LOG` Open internal log info.
  - `UNITY_WEB_SOCKET_ENABLE_ASYNC` Use network thread handle message (not WebGL platform).

