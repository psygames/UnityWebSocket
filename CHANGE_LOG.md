## 版本记录

#### v2.2
- 拆分 Git 库，添加对 PackageManager 支持，添加对 OpenUpm 的支持。

#### v2.1
- 修改WebGL平台Unity - JS通信方式，使用回调方式替换SendMessage方式。
- WebGL平台内存管理，内存优化，主动 malloc 和 free 内存。
- 非WebGL平台内存管理，内存优化，使用对象池方式重复利用对象。
- 添加 辅助工具，包括：Help，About，Check Updates 等。
- 添加 Assembly Define 支持，规范项目依赖，非必要接口使用 internal 关键字不予暴露。
- 目录结构重新整理，使用根路径下唯一UnityWebSocket文件夹作为插件唯一存放位置。

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
