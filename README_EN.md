
[(中文版)](README.md)

## Online Demo

- [http://39.105.150.229/UnityWebSocket/](http://39.105.150.229/UnityWebSocket/)

## Installation

#### Requirement

- Unity 2018.3 or later
- No other SDK are required

#### Using OpenUPM

- This package is available on [OpenUPM](https://openupm.com).
You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
  ```
  openupm add com.psygame.unitywebsocket
  ```

#### Using Git

- Find the manifest.json file in the Packages folder of your project and edit it to look like this:
  ```js
  {
   "dependencies": {
   "com.psygame.unitywebsocket": "https://github.com/psygame/UnityWebSocket.git",
   ...
   },
  }
  ```

- To update the package, change suffix `#{version}` to the target version.
  * e.g. `"com.psygame.unitywebsocket": "https://github.com/psygame/UnityWebSocket.git#2.2.0",`

- Or, use [UpmGitExtension](https://github.com/mob-sakai/UpmGitExtension) to install and update the package.

#### Using Unity Package

- Download a UnityWebSocket.unitypackage file from [Releases](https://github.com/psygame/UnityWebSocket/releases) page.
- Import it into your Unity project.


## Usage:

- Require Settings:

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

- more detail usage, see the [UnityWebSocketTest.cs](Tests/UnityWebSocketTest.cs) code in project。

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
