using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BasicCommunication
{
    //tcp服务器
    public class TCPIPServer
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
        /// 是否打开连接
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
        public TCPIPServer()
        {


        }

        /// <summary>
        /// 有参构造
        /// </summary>
        /// <param name="ip">本机IP</param>
        /// <param name="port">本机端口 默认:23456</param>
        /// <param name="maxConn">最大连接数量 默认:20</param>
        /// <param name="buffSize">缓冲区大小 默认:1024*1024</param>
        public TCPIPServer(string ip = "127.0.0.1", int port = 23456, int maxConn = 20, int buffSize = 1024 * 1024)
        {
            this.ip = ip;
            this.port = port;
            this.maxConn = maxConn;
            this.buffSize = buffSize;
        }

        /// <summary>
        /// 获取当前连接的IP地址
        /// </summary>
        public List<string> Ls
        {
            get
            {
                lock (suo1)
                {
                    return new List<string>(ls);
                }
            }
        }



        /// <summary>
        /// 初始化,绑定到ip和端口,并且启动线程自动监听
        /// </summary>
        /// <returns>错误码 0:正常  1:ip为null 2:ip格式不对 3:以部署 4:未知错误</returns>
        public int Init()
        {
            int errorCode = 0;

            if (socket != null && socket.Connected)
            {
                return 3;
            }

            socket = new Socket(SocketType.Stream, ProtocolType.IP);
            sockets = new Dictionary<string, Socket>();

            ls = new List<string>();
            if (neiPairs == null)
            {
                neiPairs = new Dictionary<string, List<byte>>();
            }

            try
            {

                //绑定ip和端口
                socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                //最大连接数量
                socket.Listen(maxConn);


            }
            catch (ArgumentNullException ex)
            {
                errorCode = 1;
            }
            catch (FormatException ex)
            {
                errorCode = 2;
            }

            //创建监听线程
            Thread t = new Thread(Read);
            //设置为后台线程
            t.IsBackground = true;
            //是否监听设置为true
            atuoRun = true;
            //启动线程
            t.Start();
            //修改服务器状态为已打开
            IsOpen = true;

            return errorCode;
        }

        /// <summary>
        ///自动读取数据放入缓冲区
        /// </summary>
        void Read()
        {
            while (atuoRun)
            {
                Socket socketci = null;
                string ke = "";
                try
                {
                    socketci = socket.Accept();
                    ke = ((IPEndPoint)socketci.RemoteEndPoint).Address.ToString();
                    int lenc = ke.LastIndexOf(':') + 1;
                    ke = ke.Substring(lenc, ke.Length - lenc);
                    ke += ":"+port;
                    ls.Add(ke);
                    sockets.Add(ke, socketci);
                }
                catch (SocketException s)
                {
                    return;
                }
                catch (ArgumentException es)
                {
                    ls.RemoveAt(ls.Count - 1);
                    return;
                }



                Thread t = new Thread((object o) =>
                {

                    string keyIp = (string)o;
                    //数据连接
                    List<byte> bytes;
                    if (!neiPairs.ContainsKey(keyIp))
                    {
                        bytes = new List<byte>();
                        neiPairs.Add(keyIp, bytes);
                    }
                    else
                    {
                        neiPairs.TryGetValue(keyIp, out bytes);
                    }

                    byte[] buff = new byte[buffSize];

                    try
                    {
                        while (socketci.Connected)
                        {

                            lock (socket)
                            {
                                if (buff.Length != buffSize)
                                {
                                    buff = new byte[buffSize];
                                }
                            }


                            if (socketci.Poll(1000,SelectMode.SelectRead)) {
                                throw new Exception("客户端断开连接");
                            
                            }

                            int len = socketci.Receive(buff);
                            if (len != 0)
                            {
                                //录入数据到内缓冲区
                                lock (suo)
                                {
                                    for (int i = 0; i < len; i++)
                                    {
                                        bytes.Add(buff[i]);
                                    }
                                }

                            }
                        }


                    }
                    catch (Exception es)
                    {
                        if (socketci != null)
                        {
                            socketci.Close();
                        }
                    }
                    finally {
                        sockets.Remove(keyIp);
                        lock (suo1)
                        {
                            ls.Remove(keyIp);
                        }
                        lock (suo)
                        {
                            neiPairs.Remove(ke);
                        }

                    }
                    
                })
                { IsBackground = true };

                t.Start(ke);
            }

        }


        /// <summary>
        /// 读取服务器信息
        /// </summary>
        /// <param name="ipArg">ip地址</param>
        /// <returns>数据 null:进入数据有误</returns>
        public List<byte> waiRead(string ipArg , int potr)
        {
            this.port = potr;
            if (ipArg == null || ipArg == string.Empty || !neiPairs.ContainsKey(ipArg+":"+port) /*|| neiPairs[ipArg].Count == 0*/)
            {
                return new List<byte>();
            }
            List<byte> tempd;
            lock (suo)
            {
                tempd = new List<byte>(neiPairs[ipArg+":"+port]);

                neiPairs[ipArg + ":" + port].Clear();
            }


            return tempd;
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="ipArg">ip地址</param>
        /// <param name="en">Encoding对象</param>
        /// <returns></returns>
        public string waiRead(string ipArg,Encoding en) {
            return en.GetString(waiRead(ipArg,port).ToArray());
        }

        /// <summary>
        /// 根据IP写入数据
        /// </summary>
        /// <param name="ipArg"></param>
        /// <returns></returns>
        public bool waiWrite(string ipArg, byte[] by)
        {
            Socket so;
            if (ipArg == null || ipArg == string.Empty || by == null || !sockets.TryGetValue(ipArg, out so) || !so.Connected)
            {
                return false;
            }


            try
            {
                so.Send(by);
            }
            catch (SocketException ex)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Clare()
        {
            atuoRun = false;
            socket.Close();
            Thread.Sleep(2000);
            ls = null;
            neiPairs = null;
            sockets = null;
            socket = null;
        }
    }

    //tcp客户端
    public class TCPIPClient
    {
        /// <summary>
        /// 数据对象
        /// </summary>
        public Socket socket;
        /// <summary>
        /// 地址
        /// </summary>
        public string ip = "127.0.0.1";
        /// <summary>
        /// 端口
        /// </summary>
        public int port = 6666;
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int buffsize = 1024 * 1024;

        /// <summary>
        /// 初始化,连接到ip和端口
        /// </summary>
        /// <returns>错误码 0:正常  1:ip为null 2:ip格式不对 3:以部署 4:未连接上</returns>
        public int Conn()
        {
            int errorCode = 0;

            if (socket != null && socket.Connected)
            {
                return 3;
            }

            try
            {
                ManualResetEvent en = new ManualResetEvent(false);
                socket = new Socket(SocketType.Stream, ProtocolType.IP);

                socket.ReceiveTimeout = 5000;
                en.Reset();
                socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback((s) =>
                {
                    en.Set();
                }), socket);


                if (!en.WaitOne(3000))
                {
                    throw new FormatException("连接超时");
                }
            }
            catch (ArgumentNullException ex)
            {
                errorCode = 1;
            }
            catch (FormatException ex)
            {
                errorCode = 2;
            }
            catch (Exception ex)
            {
                errorCode = 4;
            }

            return errorCode
                
                
                
                
                
                
                
                
                
                
                ;
        }
        /// <summary>
        /// 根据IP写入数据
        /// </summary>
        /// <param name="ipArg"></param>
        /// <returns></returns>
        public bool waiWrite(byte[] by)
        {
            if (socket == null || !socket.Connected)
            {
                if (socket != null)
                {
                    int error = Conn();
                    if (error == 2)
                    {
                        throw new Exception("连接超时");
                    }
                }

                return false;
            }
            try
            {
                socket.Send(by);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns></returns>
        public byte[] read()
        {
            if (socket == null || !socket.Connected)
            {
                if (socket != null)
                {
                    int error = Conn();
                    if (error == 2)
                    {
                        throw new Exception("连接超时");
                    }
                }
                return null;
            }
            byte[] buff = new byte[buffsize];
            int len;
            try
            {
                len = socket.Receive(buff);
            }
            catch(Exception ex)
            {
                return null;
            }

            

            byte[] dd = new byte[len];
            for (int i = 0; i < len; i++)
            {
                dd[i] = buff[i];
            }
            return dd;
        }

        public void Clare()
        {
            socket.Close();
            socket = null;
        }
    }

    //HTTP
    public class HTTPClient
    {
        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Post(string url, string data, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(data);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = bytes.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader stream = new StreamReader(response.GetResponseStream(), encoding))
            {
                return stream.ReadToEnd();
            }
        }
    }
}
