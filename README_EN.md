
[(中文版)](README.md)

<div align=center>
  <img src="https://s1.ax1x.com/2020/08/21/dYIAQU.png" width=20%/>
</div>

## **Online Demo**

- **[https://psygames.github.io/UnityWebSocket/](https://psygames.github.io/UnityWebSocket/)**


## **Quick Start**

### **Requirements**

- Unity 2018.3 or higher.

### **Installation**

- **Install via Package Manager (Recommended)**

  Open Window/Package Manager in the Unity menu bar, click the `+` icon at the top left, select `Add package from git URL...`, enter `https://github.com/psygames/UnityWebSocket.git#upm` and confirm.
  
- **Install via Unity Package**

  Download the latest version of `UnityWebSocket.unitypackage` from the [Releases](https://github.com/psygames/UnityWebSocket/releases) page, then import the package into your project.

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

- For more usage, refer to the [UnityWebSocketDemo.cs](Assets/UnityWebSocket/Demo/UnityWebSocketDemo.cs) example code in the project.

- Menus
  - Tools -> UnityWebSocket, version update check, bug report, etc.

- Unity Define Symbols(Optional):
  - `UNITY_WEB_SOCKET_LOG` Open internal log info.
