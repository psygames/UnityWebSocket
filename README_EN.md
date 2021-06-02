
[(中文版)](README.md)

<div align=center>
  <img src="https://s1.ax1x.com/2020/08/21/dYIAQU.png" width=20%/>
</div>

## **Online Demo**

- **[https://psygames.github.io/UnityWebSocket/](https://psygames.github.io/UnityWebSocket/)**


## **Quick Start**

### **Installation**

- Download an `UnityWebSocket.unitypackage` file from [Releases](https://github.com/psygames/UnityWebSocket/releases) page.
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

- more detail usage, see the [UnityWebSocketDemo.cs](Assets/UnityWebSocket/Demo/UnityWebSocketDemo.cs) code in project.
  
- Unity Define Symbols:
  - `UNITY_WEB_SOCKET_LOG` open internal log info.
  - `UNITY_WEB_SOCKET_SHARP` use third-party plugin [websocket-sharp](https://github.com/sta/websocket-sharp).
  - `UNITY_WEB_SOCKET_NINJA` use third-party plugin [Ninja.WebSockets](https://github.com/ninjasource/Ninja.WebSockets) 

### **Attention(Warning)**

- there are many **WebSocket** class in different namespace, use in different situations.

  namespace | platform | sync style |  description  
  -|-|-|-
  UnityWebSocket | all | synchronized(no block) | **[recommend]** no need consider the problem by using unity component in asynchronized callback.
  UnityWebSocket.Uniform | all | asynchronized | consider the problem by using unity component in asynchronized callback.
  UnityWebSocket.WebGL | WebGL only | asynchronized | only run in WebGL platform.
  UnityWebSocket.NoWebGL | WebGL except | asynchronized  | only run in not WebGL platforms.
