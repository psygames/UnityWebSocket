using System;
using System.Collections.Generic;
using System.Text;

namespace YLWebSocket
{
    /// <summary>
    /// 将数据打包和解包
    /// 需要服务器客户端打包解析方式对应（推荐 protobuffer）
    /// 这里只处理 byte[] <---> UTF8 的转换
    /// </summary>
    public  class SimpleMessagePackTool
    {
        public static byte[] Pack(string msg)
        {
            return System.Text.Encoding.UTF8.GetBytes(msg);
        }

        public static string Unpack(byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
