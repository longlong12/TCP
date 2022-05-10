using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer_using
{
    public class TcpServer 
    {
        /// <summary>
        /// 服务器端口
        /// </summary>
        Socket socket;

        /// <summary>
        /// 客户端数组
        /// </summary>
        Dictionary<string, Socket> sockets;

        /// <summary>
        /// 当前连接的ip地址
        /// </summary>
        List<string> ls;

        /// <summary>
        /// ip地址
        /// </summary>
        public string ip;
        /// <summary>
        /// 端口
        /// </summary>
        public int port;
        /// <summary>
        /// 最大连接数量
        /// </summary>
        public int maxConn;

        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int buffSize;

        /// <summary>
        /// 是否监听客户端
        /// </summary>
        bool atuoRun = false;

        /// <summary>
        /// 是否打开连接  服务器状态
        /// </summary>
        public bool IsOpen = false;

        /// <summary>
        /// 读取数据锁
        /// </summary>
        object suo = new object();

        /// <summary>
        /// 访问IP锁
        /// </summary>
        object suo1 = new object();


        /// <summary>
        /// 内部缓冲区
        /// </summary>
        Dictionary<string, List<byte>> neiPairs;

        /// <summary>
        /// 无参构造
        /// </summary>
        public  TcpServer() 
        {

        }

        /// <summary>
        /// 有参构造
        /// </summary>
        /// <param name="ip"> 服务器IP 默认：本机</param>
        /// <param name="port">服务器端口 默认：10086</param>
        /// <param name="maxconn">最大连接数 默认：20</param>
        /// <param name="buffsize">缓冲区大小 默认：1024*1024</param>
        public TcpServer(string ip="127.0.0.1", int port = 10086, int maxconn = 20 , int buffsize = 1024*1024) 
        {
            this.ip = ip;
            this.port = port;
            this.maxConn = maxconn;
            this.buffSize = buffsize;
        }

        public int Statr()
        {
            sockets = new Dictionary<string, Socket>();
            int errorCode = 0;
            if (socket != null && socket.Connected)
            {
                return 3;
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Bind(iPEndPoint);
            socket.Listen(maxConn);
            Thread t = new Thread(read);
            t.Start();
            atuoRun = true;
            IsOpen = true;

            return errorCode;
        }

        void read() 
        {
            

        }

        public string read_string()
        {
            string message;
            while (atuoRun)
            {
                Socket socket_read = socket.Accept();
                //socket_read.RemoteEndPoint();
                message = ((IPEndPoint)socket_read.RemoteEndPoint).Address.ToString();
                //sockets.Add(message, socket_read);
                return message;
            }
            return null;
        }
    }
}
