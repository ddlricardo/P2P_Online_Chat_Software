/*
 * last edited by TAN Fei
 * 2015/1/5 9:21
 * 添加窗口抖动 Window()函数
 * 添加窗口抖动功能
 * 去除server按钮
 * 2015/1/5 10:34
 * 添加时间
 * 添加表情发送
 * 2015/1/5 13:07
 * 添加线程关闭
 * last edited by Du Delun
 * 2015/1/9 00:15
 * 重写窗口关闭和最小化函数
 * 添加鼠标拖拽移动窗口功能
 * 实现ListBox接收来自前一窗口的传值
 * 实现背景图片随机显示功能
 * 界面美化
 * 2015/1/9 02:15
 * 设置窗口Tab键顺序
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using CustomUIControls;

namespace iChat
{
    public partial class Form6 : Form
    {

        public int clientPort;

        public bool isConnect = false;

        string myname;
        int num;//序号，当前发起的第几个序号
        bool state;//true为已经使用，false为未使用
        int serverPort;
        int mynumofgroup;
        int numgroup;
        bool positive;//true为主动发起连接，false为被动接收连接
        int num_client;//被动开始聊天时用于得知对方的端口号信息
        string IP_client;//对方IP
        string name_client;//对方用户名
        string[] name_group = new string[20];
        bool ACK;
        string localIP;
        Thread threadClient;
        Thread threadServer;

        public ImageListPopup ilp;


        public Form6(string usernanme, int m_num, bool m_state, bool m_positive, string m_IP_client, string m_name_client, int m_num_client, bool ACK,int mynumofgroup,int numgroup,string []name_group)
        {

            InitializeComponent();
            myname = usernanme;
            num = m_num;
            state = m_state;
            positive = m_positive;
            IP_client = m_IP_client;
            serverPort = 8001 + 4 * num;

            num_client = m_num_client;
            name_client = m_name_client;
            this.ACK = ACK;
            this.mynumofgroup = mynumofgroup;
            this.numgroup = numgroup;
            threadClient = new Thread(P2PClient);
            threadServer = new Thread(P2PServer);
            for (int j = 0; j < numgroup;j++ )
            {
                this.name_group[j] = name_group[j];
                listBox1.Items.Add(name_group[j]);
            }
            shake_hands();
            runServer();

            threadClient.IsBackground = true;
            threadServer.IsBackground = true;

            localIP = GetLocalIP();
        }


        private void P2PClient()
        {
            try
            {
                ///创建终结点EndPoint
                IPAddress ip = IPAddress.Parse(IP_client);
                IPEndPoint ipe = new IPEndPoint(ip, clientPort);//把ip和端口转化为IPEndpoint实例
                ///创建socket并连接到服务器
                Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                c.Connect(ipe);//连接到服务器

                ///向服务器发送信息
                richTextBox2.Invoke(new Action(() =>
                {
                    string sendStr = mynumofgroup+"$"+myname+"$"+richTextBox2.Rtf;//聊天发送的文字
                    
                    byte[] bs = Encoding.UTF8.GetBytes(sendStr);//把字符串编码为字节

                    c.Send(bs, bs.Length, 0);//发送信息
                }));

               
                    richTextBox1.Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText(myname + " " + DateTime.Now.ToString() + "\n");
                        richTextBox1.SelectedRtf = richTextBox2.Rtf;
                        richTextBox1.AppendText("\n");
                        
                        richTextBox1.ScrollToCaret();
                    }));
                    richTextBox2.Invoke(new Action(() =>
                    {
                        richTextBox2.Clear();
                        richTextBox2.Focus();
                    }));
               

                //一定记着用完socket后要关闭
                c.Close();
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show("argumentNullException:" + e.Message);
            }
            catch (SocketException e)
            {
                MessageBox.Show(/*"SocketException:" + e.Message+*/"对方已下线");
            }

        }


        private void P2PServer()
        {
            while (true)
            {
                //获取本机IP地址
                string serverHost = GetLocalIP();//ipEntry.AddressList[1].ToString();//转换为string类
                /**/
                ///创建终结点（EndPoint）
                IPAddress ip = IPAddress.Parse(serverHost);//把ip地址字符串转换为IPAddress类型的实例
                IPEndPoint ipe = new IPEndPoint(ip, serverPort);//用指定的端口和ip初始化IPEndPoint类的新实例
                /**/
                ///创建socket并开始监听
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建一个socket对像，如果用udp协议，则要用SocketType.Dgram类型的套接字
                s.Bind(ipe);//绑定EndPoint对像（2000端口和ip地址）
                s.Listen(5);//开始监听
                /**/
                ///接受到client连接，为此连接建立新的socket，并接受信息
                Socket temp = s.Accept();//为新建连接创建新的socket
                string recvStr = "";
                byte[] recvBytes = new byte[16384];
                int bytes;
                bytes = temp.Receive(recvBytes, recvBytes.Length, 0);//从客户端接受信息
                recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
                string realname = recvStr.Substring(0, 10);
                richTextBox1.Invoke(new Action(() =>
                {
                    richTextBox1.AppendText(realname+ " " + DateTime.Now.ToString() + "\n");
                    richTextBox1.SelectedRtf = recvStr.Substring(11);
                    richTextBox1.AppendText("\n");
                    //richTextBox1.Focus();
                    richTextBox1.ScrollToCaret();
                }));
                //textbox 中添加一行
//                 string sendStr = "succeed";
//                 byte[] bs = Encoding.UTF8.GetBytes(sendStr);
//                 temp.Send(bs, bs.Length, 0);//返回信息给客户端
                temp.Close();
                s.Close();
            }
        }


        private void shake_hands()//作为被动方
        {

            clientPort = 8001 + 4 * num_client;          
            IPAddress ip = IPAddress.Parse(IP_client);
            IPEndPoint ipe = new IPEndPoint(ip, clientPort + 3);//把ip和端口转化为IPEndpoint实例
            ///创建socket并连接到服务器
            Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
            c.Connect(ipe);//连接到服务器
            if (ACK == true)
            {
                string sendStr = myname + "+" + "ACK" + "+" + num.ToString() + "+" + localIP + "+" + "Group" + "+" + mynumofgroup.ToString();//聊天发送的文字
                byte[] bs = Encoding.UTF8.GetBytes(sendStr);//把字符串编码为字节
                c.Send(bs, bs.Length, 0);//发送信息
                isConnect = true;
                c.Close();
            }
            else
            {
                string sendStr = myname + "NAK" + num.ToString() + localIP + "+" + "Group" + numgroup.ToString();//聊天发送的文字
                byte[] bs = Encoding.UTF8.GetBytes(sendStr);//把字符串编码为字节
                c.Send(bs, bs.Length, 0);//发送信息
                isConnect = true;
                c.Close();
            }

        }


        public static string GetLocalIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取本机IP出错:" + ex.Message);
                return "";
            }
        }


        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (/*e.Modifiers == Keys.Control && */e.KeyCode == Keys.Enter)
            {
                string stringState = threadClient.ThreadState.ToString();

                switch (stringState)
                {
                    case "Background, Unstarted": //第一次启动
                        try
                        {
                            threadClient.Start();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        break;
                    case "Running":    //正在运行,此状态删掉亦可

                        break;
                    case "Suspended": //挂起则恢复运行
                        threadClient.Resume();
                        break;
                    case "Stopped": //线程已停止则重新启动
                        threadClient = new Thread(P2PClient);
                        threadClient.Start();
                        break;
                    default: //什么都不做
                        break;
                }
            }
        }

        void window()
        {
            int recordx = this.Left;
            int recordy = this.Top;
            Random random = new Random();
            this.Invoke(new Action(() =>
            {


                for (int i = 0; i < 150; i++)
                {
                    int x = random.Next(100);
                    int y = random.Next(100);
                    if (x % 2 == 0)
                    {
                        this.Left = this.Left + x / 25;
                    }
                    else
                    {
                        this.Left = this.Left - x / 25;
                    }
                    if (y % 2 == 0)
                    {
                        this.Top = this.Top + y / 25;
                    }
                    else
                    {
                        this.Top = this.Top - y / 25;
                    }
                    System.Threading.Thread.Sleep(1);
                }
                this.Left = recordx;
                this.Top = recordy;
            }));
        }



        private void button1_Click(object sender, EventArgs e)
        {
            string stringState = threadClient.ThreadState.ToString();

            switch (stringState)
            {
                case "Background, Unstarted": //第一次启动
                    try
                    {
                        threadClient.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "Running":    //正在运行,此状态删掉亦可

                    break;
                case "Suspended": //挂起则恢复运行
                    threadClient.Resume();
                    break;
                case "Stopped": //线程已停止则重新启动
                    threadClient = new Thread(P2PClient);
                    threadClient.Start();
                    break;
                default: //什么都不做
                    break;
            }
        }

        void runServer()
        {
            string stringState = threadServer.ThreadState.ToString();

            switch (stringState)
            {
                case "Unstarted": //第一次启动
                    try
                    {
                        threadServer.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "Running":    //正在运行,此状态删掉亦可
                    break;
                case "Suspended": //挂起则恢复运行
                    threadServer.Resume();
                    break;
                case "Stopped": //线程已停止则重新启动
                    threadServer = new Thread(P2PServer);
                    threadServer.Start();
                    break;
                default: //什么都不做
                    break;
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Random r = new Random();
            this.BackgroundImage = Image.FromFile(r.Next(4).ToString() + "_chat.jpg");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string stringState = threadClient.ThreadState.ToString();

            switch (stringState)
            {
                case "Background, Unstarted": //第一次启动
                    try
                    {
                        threadClient.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "Running":    //正在运行,此状态删掉亦可

                    break;
                case "Suspended": //挂起则恢复运行
                    threadClient.Resume();
                    break;
                case "Stopped": //线程已停止则重新启动
                    threadClient = new Thread(P2PClient);
                    threadClient.Start();
                    break;
                default: //什么都不做
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            Random r = new Random();
            this.BackgroundImage = Image.FromFile(r.Next(4).ToString() + "_chat.jpg");
        }

        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标签是否为左键

        private void Form6_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//如果按下的是鼠标左键
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Form6_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是按下的状态
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Form6_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是松开的状态
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }
    }
}

