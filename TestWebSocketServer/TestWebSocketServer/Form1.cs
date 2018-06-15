using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TestWebSocketServer
{
    public partial class Form1 : Form
    {
        public static Form1 instance;

        public Form1()
        {
            InitializeComponent();
            instance = this;
        }

        private Dictionary<string, WebSocketServer> servers = new Dictionary<string, WebSocketServer>();
        private Dictionary<string, string> logs = new Dictionary<string, string>();

        private void start_Click(object sender, EventArgs e)
        {
            int spIndex = address.Text.LastIndexOf('/');
            string addr = address.Text.Substring(0, spIndex);
            string path = address.Text.Substring(spIndex);
            if (servers.ContainsKey(addr))
            {
                MessageBox.Show("Duplicate Addr -> " + addr);
                return;
            }

            var server = new WebSocketServer(addr);
            server.WaitTime = TimeSpan.FromSeconds(2);
            server.AddWebSocketService<MessageHandle>(path);
            server.Start();
            servers.Add(addr, server);
            listBox1.Items.Add(address.Text);
            Log(addr, "start listening...");
        }

        private delegate void DelegatePrintText(string str, System.Drawing.Color color);
        public void PrintText(string str, System.Drawing.Color color)
        {
            if (receive.InvokeRequired)
            {
                DelegatePrintText du = new DelegatePrintText(PrintText);
                receive.Invoke(du, new object[] { str, color });
            }
            else
            {
                RtbAppend(str, color);
            }
        }

        public void Log(string addr, string str)
        {
            PrintText("[" + addr + "]" + "\n" + str + "\n", System.Drawing.Color.Black);
        }

        private void RtbAppend(string strInput, System.Drawing.Color fontColor)
        {
            int p1 = receive.TextLength;            //取出未添加时的字符串长度
            receive.AppendText(strInput);           //保留每行的所有颜色 
            int p2 = strInput.Length;               //取出要添加的文本的长度 
            receive.Select(p1, p2);                 //选中要添加的文本 
            receive.SelectionColor = fontColor;     //设置要添加的文本的字体色 
            receive.Focus();
        }

    }

    public class MessageHandle : WebSocketBehavior
    {
        private string addr { get { return base.Context.Host; } }

        protected override void OnOpen()
        {
            Form1.instance.Log(addr, "Client Connected: " + ID);
            SendMessage("Connect at " + DateTime.Now);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            string msg = Encoding.UTF8.GetString(e.RawData);
            Form1.instance.Log(addr, "Receive From :" + ID + "\n" + msg);
            SendMessage("Got [" + msg + "] at " + DateTime.Now);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Form1.instance.Log(addr, "Client Closed :" + ID);
            Sessions.CloseSession(ID);
        }

        public void SendMessage(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            Sessions.SendToAsync(data, ID, (a) => { });
        }
    }
}
