using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BasicCommunication;
using System.Threading;

namespace TCP
{
    public partial class Form1 : Form
    {
        TCPIPServer Server = new TCPIPServer();
        string Log_message;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Server = new TCPIPServer("127.0.0.1",10010);
            //Server = new TCPIPServer();
            Server.Init();
            richTextBox1.Text = DateTime.Now.ToString() + "       " + "服务器连接成功";
            Log_message += Log_message + richTextBox1.Text;
            Thread t = new Thread(R);
            t.Start();
            if (button1.Text == "连接服务器")
            {
                button1.Text = "打开服务器";
            }
        }

        public void R() 
        {
            string message;
            List<byte> a;
            bool t = true;
            while (t)
            {
                if (Server.Ls != null)
                {
                    this.Invoke(new Action(() =>
                    {
                        richTextBox1.Text = DateTime.Now.ToString() + "       " + "客户端已经连接";
                        Log_message = Log_message + richTextBox1.Text;
                    }));
                    t = false;
                }
            }
            
            while (Server.Ls != null)
            {
                a = Server.waiRead("127.0.0.1", 10010);
                if (a.Count != 0)
                {
                    message = System.Text.Encoding.Default.GetString(a.ToArray());
                    this.Invoke(new Action(() =>
                    {
                        richTextBox1.Text = Log_message +
                                            DateTime.Now.ToString() + "      " + message + Environment.NewLine;
                        Log_message = richTextBox1.Text;
                    }));
                }
                if (Server.Ls == null)
                {
                    this.Invoke(new Action(() =>
                    {
                        richTextBox1.Text = Log_message +
                                            DateTime.Now.ToString() + "      " + "客户端已断开,请重新连接" + Environment.NewLine;
                        Log_message = richTextBox1.Text;
                    }));
                    return;
                }
            
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string message =null ;
            int a = 1;
            int b = 1;
            int c = 0; ;

            for (int i = 1; i <= 9; i++)
            {
                for (int k = 1; k <= 10-i; k++)
                {
                    c = a * b;
                    message += a + "*" + b + "=" +c + "   ";
                    b++;
                }
                message +=  Environment.NewLine;
                a++;
                b = i+1;
            }
            MessageBox.Show(message);
        }
    }
}

//V1.1
