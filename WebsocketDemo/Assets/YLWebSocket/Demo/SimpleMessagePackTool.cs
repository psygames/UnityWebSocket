using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YLWebSocket
{
    /// <summary>
    /// <para>pack and unpack the network message.</para>
    /// <para>need use the same pack method with server.(suggest use protobuffer)</para>
    /// <para>here for the byte[] --- UTF8 convert</para>
    /// <para>将数据打包和解包</para>
    /// <para>需要服务器客户端打包解析方式对应（推荐 protobuffer）</para>
    /// <para>这里只处理 byte[] --- UTF8 的转换</para>
    /// </summary>
    public class SimpleMessagePackTool
    {
        public static byte[] Pack(string msg)
        {
            return Encoding.UTF8.GetBytes(msg);
        }

        public static string Unpack(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
