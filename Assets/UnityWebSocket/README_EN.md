
[(中文版)](README.md)

<div align=center>
  <img src="https://s1.ax1x.com/2020/08/21/dYIAQU.png" width=20%/>
</div>

[![openupm](https://img.shields.io/npm/v/com.psygame.unitywebsocket?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.psygame.unitywebsocket/)

## **Online Demo**

- **[https://psygame.github.io/UnityWebSocket/](https://psygame.github.io/UnityWebSocket/)**


## **Quick Start**

### **Requirements**

- Unity 2018.3 or later
- No other SDK are required

### **Installation**

- **Using OpenUPM**

  This package is available on [OpenUPM](https://openupm.com/packages/com.psygame.unitywebsocket/). You can install it via [openupm package installer](https://package-installer.glitch.me/v1/installer/OpenUPM/com.psygame.unitywebsocket?registry=https%3A%2F%2Fpackage.openupm.com).

- **Using Git**

  Find the manifest.json file in the Packages folder of your project and edit it to look like this:
  ```js
  {
   "dependencies": {
   "com.psygame.unitywebsocket": "https://github.com/psygame/UnityWebSocket.git",
   ...
   },
  }
  ```

  To update the package, change suffix `#{version}` to the target version.
  * e.g. `"com.psygame.unitywebsocket": "https://github.com/psygame/UnityWebSocket.git#2.3.0",`

- **Using Unity Package**

  Download an `UnityWebSocket.unitypackage` file from [Releases](https://github.com/psygame/UnityWebSocket/releases) page.
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

  // send data (two ways)
  socket.SendAsync(str); // send string data
  socket.SendAsync(bytes); // send byte[] data

  // close connection
  socket.CloseAsync();
  ```

- more detail usage, see the [UnityWebSocketDemo.cs](Samples~/Demo/UnityWebSocketDemo.cs) code in project。


### **Attention(Warning)**

- there are many **WebSocket** class in different namespace, use in different situations.

  namespace | platform | sync style |  description  
  -|-|-|-
  UnityWebSocket | all | synchronized(no block) | **[recommend]** no need consider the problem by using unity component in asynchronized callback.
  UnityWebSocket.Uniform | all | asynchronized | consider the problem by using unity component in asynchronized callback.
  UnityWebSocket.WebGL | WebGL only | asynchronized | only run in WebGL platform.
  UnityWebSocket.NoWebGL | WebGL except | asynchronized  | only run in not WebGL platforms.
