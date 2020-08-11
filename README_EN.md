
[(中文版)](README.md)

### Demo Online Test

- [http://39.105.150.229/UnityWebSocket/](http://39.105.150.229/UnityWebSocket/)

### UnityWebSocket Usage

#### 1. Download the latest version

- [https://github.com/y85171642/UnityWebSocket/releases](https://github.com/y85171642/UnityWebSocket/releases)

#### 2. Usage:

- Import UnityWebSocket.unitypackage in Unity, Require:

      * Require Scripting Runtime Version = .Net 4.x
      * Require WebGL LinkerTarger = asm.js or Both

- Easy to use WebSocket

  ```csharp
  // the namespace
  using UnityWebSocket;

  // create instance
  WebSocket scoket = new WebSocket();

  // register callback
  scoket.OnOpen += OnOpen;
  scoket.OnClose += OnClose;
  scoket.OnMessage += OnMessage;
  socket.OnError += OnError;

  // connect
  string address = "ws://echo.websocket.org";
  socket.ConnectAsync(address);

  // send data (tow ways)
  socket.SendAsync(str); // send String data
  socket.SendAsync(bytes); // send byte[] data

  // close connection
  socket.CloseAsync();
  ```

- more detail usage, see the [Example.cs](UnityWebSocket/Assets/UnityWebSocket/Example/Example.cs) code in project。

#### 3. Attention(Warning)

- there are many **WebSocket** class in different namespace, use in different situations.

  namespace | platform | sync style |  description  
  -|-|-|-
  UnityWebSocket | all | synchronized(no block) | **[recommend]** no need consider the problem by using unity component in asynchronized callback.
  UnityWebSocket.Uniform | all | asynchronized | consider the problem by using unity component in asynchronized callback.
  UnityWebSocket.WebGL | WebGL only | asynchronized | only run in WebGL platform.
  UnityWebSocket.NoWebGL | WebGL except | asynchronized  | only run in not WebGL platforms.

#### 4. WebGL Module Introduction

- WebSocket.jslib, syntax follow to [asm.js](http://www.ruanyifeng.com/blog/2017/09/asmjs_emscripten.html)。

      Path: Plugins/WebGL/WebSocket.jslib
      Fucntion：Unity will deploy it to web js runtime on WebGL platform.

- Example Scene

      Function: Example how to use UnityWebSocket.

#### 5. WebSocket Server

- use Official Echo Test Server. refer to [Echo Test](http://www.websocket.org/echo.html).
