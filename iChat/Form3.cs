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
 * 2015/1/5 22:50
 * 重写窗口关闭和最小化函数
 * 添加鼠标拖拽移动窗口功能
 * 2015/1/7 18:30
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
    public partial class Form3 : Form
    {

        public int clientPort;
        public int clientPortFile;
        public int clientPortShake;
        public bool isConnect = false;

        string myname;
        int num;//序号，当前发起的第几个序号
        bool state;//true为已经使用，false为未使用
        int serverPort;
        int serverPortFile;
        int serverPortShake;
        bool positive;//true为主动发起连接，false为被动接收连接
        int num_client;//被动开始聊天时用于得知对方的端口号信息
        string IP_client;//对方IP
        string name_client;//对方用户名
        bool ACK;

        Thread threadClient;
        Thread threadServer;
        Thread threadClientFile;
        Thread threadServerFile;
        Thread threadClientShake;
        Thread threadServerShake;
        public ImageListPopup ilp;



        public Form3(string usernanme, int m_num, bool m_state, bool m_positive, string m_IP_client, string m_name_client, int m_num_client, bool ACK)
        {

            InitializeComponent();
            myname = usernanme;
            num = m_num;
            state = m_state;
            positive = m_positive;
            IP_client = m_IP_client;
            serverPort = 8001 + 4 * num;
            serverPortFile = 8002 + 4 * num;
            serverPortShake = 8003 + 4 * num;
            num_client = m_num_client;
            name_client = m_name_client;
            this.ACK = ACK;
            label1.Text = name_client;

            threadClient = new Thread(P2PClient);
            threadServer = new Thread(P2PServer);
            threadClientFile = new Thread(P2PClientFile);
            threadServerFile = new Thread(P2PServerFile);
            threadClientShake = new Thread(P2PClientShake);
            threadServerShake = new Thread(P2PServerShake);
            shake_hands();
            runServer();
            runServerFile();
            runShake();
            threadClient.IsBackground = true;
            threadServer.IsBackground = true;
            threadClientFile.IsBackground = true;
            threadServerFile.IsBackground = true;
            threadClientShake.IsBackground = true;
            threadServerShake.IsBackground = true;

            //发送表情
            imageList1 = new ImageList();
            imageList1.ImageSize = new Size(24, 24);
            imageList1.ColorDepth = ColorDepth.Depth24Bit;       //32位的带alpha通道的可以直接透明 
            imageList1.Images.AddStrip(new Bitmap(GetType(), "emoticons.bmp"));  //加载资源表情图片           
            imageList1.TransparentColor = Color.FromArgb(255, 0, 255);

            ilp = new ImageListPopup();
            ilp.Init(imageList1, 8, 8, 10, 2);   //水平、垂直线间距，表情显示的列和行 
            ilp.ItemClick += new ImageListPopupEventHandler(OnItemClicked); 

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
                    string sendStr = richTextBox2.Rtf;//聊天发送的文字

                    byte[] bs = Encoding.UTF8.GetBytes(sendStr);//把字符串编码为字节

                    c.Send(bs, bs.Length, 0);//发送信息
                }));
                ///接受从服务器返回的信息
                string recvStr = "";
                byte[] recvBytes = new byte[16384];
                int bytes;
                bytes = c.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息
                recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
                if (recvStr == "succeed")
                {
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
                }
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
                /**/
                ///给client端返回信息
                richTextBox1.Invoke(new Action(() =>
                {
                    richTextBox1.AppendText(name_client + " " + DateTime.Now.ToString() + "\n");
                    richTextBox1.SelectedRtf = recvStr;
                    richTextBox1.AppendText("\n");
                    richTextBox1.ScrollToCaret();
                }));
                //textbox 中添加一行
                string sendStr = "succeed";
                byte[] bs = Encoding.UTF8.GetBytes(sendStr);
                temp.Send(bs, bs.Length, 0);//返回信息给客户端
                temp.Close();
                s.Close();
            }
        }
        private void P2PClientFile()
        {
            try
            {
            ///创建终结点EndPoint
            IPAddress ip = IPAddress.Parse(IP_client);
            //IPAddress ipp = new IPAddress("127.0.0.1");
            IPEndPoint ipe = new IPEndPoint(ip, clientPortFile);//把ip和端口转化为IPEndpoint实例
            ///创建socket并连接到服务器
            Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
            //Console.WriteLine("Conneting…");
            c.Connect(ipe);//连接到服务器

            string filename_access;
            string filename;

            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filename_access = dialog.FileName;
                filename = dialog.SafeFileName;
                ///向服务器发送信息
                byte[] f_name = Encoding.UTF8.GetBytes(filename);//把字符串编码为字节
                //Console.WriteLine("Send Message");
                c.Send(f_name, f_name.Length, 0);//发送信息
                ///接受从服务器返回的信息
                string recvStr = "";
                byte[] recvBytes = new byte[1024];
                int bytes;
                bytes = c.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息
                recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
                if (recvStr == "yes")
                {
                    //下面是发文件
                    FileStream fs = File.Open(filename_access, FileMode.Open);
                    byte[] reads = new byte[4098];
                    BinaryReader binaryReader = new BinaryReader(fs);

                    int count;
                    while (0 != (count = binaryReader.Read(reads, 0, 4098)))
                        c.Send(reads, count, SocketFlags.None);
                    MessageBox.Show("文件发送成功！", "文件发送");
                    //   c.Send(reads);
                    ///一定记着用完socket后要关闭
                    c.Close();
                    binaryReader.Close();
                    fs.Close();
                }
                else
                {
                    MessageBox.Show("对方拒绝了您发送文件的请求。", "文件发送");
                }
            }
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
        private void P2PServerFile()
        {
            while (true)
            {
                //server

                //获取本机IP地址
                string serverHost = GetLocalIP();//转换为string类
                /**/
                ///创建终结点（EndPoint）
                IPAddress ip = IPAddress.Parse(serverHost);//把ip地址字符串转换为IPAddress类型的实例
                IPEndPoint ipe = new IPEndPoint(ip, serverPortFile);//用指定的端口和ip初始化IPEndPoint类的新实例
                /**/
                ///创建socket并开始监听
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建一个socket对像，如果用udp协议，则要用SocketType.Dgram类型的套接字
                s.Bind(ipe);//绑定EndPoint对像（2000端口和ip地址）
                s.Listen(5);//开始监听
                /**/
                ///接受到client连接，为此连接建立新的socket，并接受信息
                Socket temp = s.Accept();//为新建连接创建新的socket

                string filename_access = "";
                byte[] receiveBuffer = new byte[4098];
                string default_filename = "";
                byte[] d_filename = new byte[1024];
                int bytes;
                bytes = temp.Receive(d_filename, d_filename.Length, 0);//从客户端接受文件名
                default_filename += Encoding.UTF8.GetString(d_filename, 0, bytes);
                string extension = Path.GetExtension(default_filename);//后缀名
                string filename = Path.GetFileNameWithoutExtension(default_filename);//无后缀名和路径的单纯文件名
                //加做弹窗选择是否选择接受名为。。。的文件
                //发送yes
                string sendStr_yes = "";
                if (MessageBox.Show("是否接收来自" + name_client + "的名为" + "'" + default_filename + "'" + "的文件？", "文件接收", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    sendStr_yes = "yes";
                    byte[] bs = Encoding.UTF8.GetBytes(sendStr_yes);
                    temp.Send(bs, bs.Length, 0);//返回信息给客户端
                    //接收文件

                    //filename = default_filename;
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.FileName = filename;
                    sfd.Filter = @"ALL|";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string localFilePath = sfd.FileName.ToString(); //获得文件路径 

                        filename_access += localFilePath + extension;
                        int count;
                        FileStream fi = File.Create(filename_access);
                        BinaryWriter binaryWriter = new BinaryWriter(fi);
                        while (0 != (count = temp.Receive(receiveBuffer, 4098, SocketFlags.None)))
                            binaryWriter.Write(receiveBuffer, 0, count);
                        MessageBox.Show("文件接收成功！", "文件接收");
                        fi.Close();
                        binaryWriter.Close();
                        temp.Close();
                    }
                }
                else
                    sendStr_yes = "no"; ;
                s.Close();
            }
        }
        private void P2PClientShake()
        {
            try
            {

                ///创建终结点EndPoint
                IPAddress ip = IPAddress.Parse(IP_client);
                IPEndPoint ipe = new IPEndPoint(ip, clientPortShake);//把ip和端口转化为IPEndpoint实例
                ///创建socket并连接到服务器
                Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                c.Connect(ipe);//连接到服务器

                ///向服务器发送信息

                string sendStr = "\\";//聊天发送的文字

                byte[] bs = Encoding.UTF8.GetBytes(sendStr);//把字符串编码为字节

                c.Send(bs, bs.Length, 0);//发送信息

                ///接受从服务器返回的信息
                string recvStr = "";
                byte[] recvBytes = new byte[1024];
                int bytes;
                bytes = c.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息
                recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
                if (recvStr == "succeed")
                {
                    window();
                }

                ///一定记着用完socket后要关闭
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
        private void P2PServerShake()
        {
            while (true)
            {
                //server

                //获取本机IP地址
                string serverHost = GetLocalIP();//ipEntry.AddressList[1].ToString();//转换为string类
                /**/
                ///创建终结点（EndPoint）
                IPAddress ip = IPAddress.Parse(serverHost);//把ip地址字符串转换为IPAddress类型的实例
                IPEndPoint ipe = new IPEndPoint(ip, serverPortShake);//用指定的端口和ip初始化IPEndPoint类的新实例
                /**/
                ///创建socket并开始监听
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建一个socket对像，如果用udp协议，则要用SocketType.Dgram类型的套接字
                s.Bind(ipe);//绑定EndPoint对像（2000端口和ip地址）

                s.Listen(5);//开始监听

                /**/
                ///接受到client连接，为此连接建立新的socket，并接受信息
                Socket temp = s.Accept();//为新建连接创建新的socket
                string recvStr = "";
                byte[] recvBytes = new byte[1024];
                int bytes;
                bytes = temp.Receive(recvBytes, recvBytes.Length, 0);//从客户端接受信息
                recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
                /**/
                ///给client端返回信息
                if (recvStr == "\\")
                {
                    window();
                }
                //textbox 中添加一行
                string sendStr = "succeed";
                byte[] bs = Encoding.UTF8.GetBytes(sendStr);
                temp.Send(bs, bs.Length, 0);//返回信息给客户端
                temp.Close();
                s.Close();
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

        private void button3_Click(object sender, EventArgs e)
        {
            string stringState = threadClientFile.ThreadState.ToString();

            switch (stringState)
            {
                case "Background, Unstarted": //第一次启动
                    try
                    {
                        threadClientFile.SetApartmentState(ApartmentState.STA);
                        threadClientFile.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "Running":    //正在运行,此状态删掉亦可
                    break;
                case "Suspended": //挂起则恢复运行
                    threadClientFile.Resume();
                    break;
                case "Stopped": //线程已停止则重新启动
                    threadClientFile = new Thread(P2PClientFile);
                    threadClientFile.SetApartmentState(ApartmentState.STA);
                    threadClientFile.Start();
                    break;
                default: //什么都不做
                    break;
            }
        }

        void runServerFile()
        {
            string stringState = threadServerFile.ThreadState.ToString();

            switch (stringState)
            {
                case "Unstarted": //第一次启动
                    try
                    {
                        threadServerFile.SetApartmentState(ApartmentState.STA);
                        threadServerFile.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "Running":    //正在运行,此状态删掉亦可
                    break;
                case "Suspended": //挂起则恢复运行
                    threadServerFile.Resume();
                    break;
                case "Stopped": //线程已停止则重新启动
                    threadServerFile = new Thread(P2PServerFile);
                    threadServerFile.SetApartmentState(ApartmentState.STA);
                    threadServerFile.Start();
                    break;
                default: //什么都不做
                    break;
            }
        }

        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标签是否为左键
        private void Form3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//如果按下的是鼠标左键
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Form3_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是按下的状态
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Form3_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是松开的状态
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string stringState = threadClientShake.ThreadState.ToString();

            switch (stringState)
            {
                case "Background, Unstarted": //第一次启动
                    try
                    {
                        threadClientShake.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "Running":    //正在运行,此状态删掉亦可
                    break;
                case "Suspended": //挂起则恢复运行

                    break;
                case "Stopped": //线程已停止则重新启动
                    threadClientShake = new Thread(P2PClientShake);
                    threadClientShake.Start();
                    break;
                default: //什么都不做
                    break;
            }
        }


        private void shake_hands()
        {
            if (positive == false)//被动
            {
                clientPort = 8001 + 4 * num_client;
                clientPortFile = 8002 + 4 * num_client;
                clientPortShake = 8003 + 4 * num_client;
                IPAddress ip = IPAddress.Parse(IP_client);
                IPEndPoint ipe = new IPEndPoint(ip, clientPort + 3);//把ip和端口转化为IPEndpoint实例
                ///创建socket并连接到服务器
                Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                c.Connect(ipe);//连接到服务器
                if (ACK == true)
                {
                    string sendStr = "order+ACK" + "+" + num.ToString();//聊天发送的文字
                    byte[] bs = Encoding.UTF8.GetBytes(sendStr);//把字符串编码为字节
                    c.Send(bs, bs.Length, 0);//发送信息
                    isConnect = true;
                    c.Close();
                }
                else
                {
                    string sendStr = "order+NAK" + "+" + num.ToString();//聊天发送的文字
                    byte[] bs = Encoding.UTF8.GetBytes(sendStr);//把字符串编码为字节
                    c.Send(bs, bs.Length, 0);//发送信息
                    isConnect = true;
                    c.Close();
                }

            }
            else//主动发起
            {
                string serverHost = GetLocalIP();//ipEntry.AddressList[1].ToString();//转换为string类
                /**/
                ///创建终结点（EndPoint）
                IPAddress ip = IPAddress.Parse(serverHost);//把ip地址字符串转换为IPAddress类型的实例
                IPEndPoint ipe = new IPEndPoint(ip, serverPort + 3);//用指定的端口和ip初始化IPEndPoint类的新实例
                /**/
                ///创建socket并开始监听
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建一个socket对像，如果用udp协议，则要用SocketType.Dgram类型的套接字
                s.Bind(ipe);//绑定EndPoint对像（2000端口和ip地址）

                s.Listen(5);//开始监听
                

                IPAddress ip1 = IPAddress.Parse(IP_client);
                IPEndPoint ipe1 = new IPEndPoint(ip1, 8000);//把ip和端口转化为IPEndpoint实例
                ///创建socket并连接到服务器
                Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                c.Connect(ipe1);//连接到服务器

                string sendStr = myname + "+" + num.ToString() + "+" + serverHost + "+" + "Single";//聊天发送的文字
                byte[] bs = Encoding.UTF8.GetBytes(sendStr);//把字符串编码为字节
                c.Send(bs, bs.Length, 0);//发送信息                    
                c.Close();

                
                /**/
                ///接受到client连接，为此连接建立新的socket，并接受信息
                Socket temp = s.Accept();//为新建连接创建新的socket
                string recvStr = "";//order+ACK" + "+" + m_num.ToString()
                byte[] recvBytes = new byte[1024];
                int bytes;
                bytes = temp.Receive(recvBytes, recvBytes.Length, 0);//从客户端接受信息
                recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
                string[] str = recvStr.Split(new Char[] { '+' });
                if (str[1] == "ACK")
                {
                    num_client = int.Parse(str[2]);
                    clientPort = 8001 + 4 * num_client;
                    clientPortFile = 8002 + 4 * num_client;
                    clientPortShake = 8003 + 4 * num_client;
                    isConnect = true;
                    temp.Close();
                    s.Close();
                }
                else
                {
                    MessageBox.Show("对方不同意您的会话请求，连接失败！", "会话请求", MessageBoxButtons.OK);
                    Frmcnct.reply = false;
                    isConnect = false;
                }

            }

        }

        void runShake()
        {
            string stringState = threadServerShake.ThreadState.ToString();

            switch (stringState)
            {
                case "Unstarted": //第一次启动
                    try
                    {
                        threadServerShake.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "Running":    //正在运行,此状态删掉亦可
                    break;
                case "Suspended": //挂起则恢复运行
                    break;
                case "Stopped": //线程已停止则重新启动
                    threadServerShake = new Thread(P2PServerShake);
                    threadServerShake.Start();
                    break;
                default: //什么都不做
                    break;
            }
        }
        
        /************************************************************************/
        /* 选择了表情                                                                                          */
        /************************************************************************/
        public void OnItemClicked(object sender, ImageListPopupEventArgs e)
        {
            Image img = imageList1.Images[e.SelectedItem];
            Clipboard.SetDataObject(img);
            richTextBox2.ReadOnly = false;
            richTextBox2.Paste(DataFormats.GetFormat(DataFormats.Bitmap));
        }
        /************************************************************************/
        /* 表情按钮点击                                                                                     */
        /************************************************************************/
        private void button4_Click(object sender, EventArgs e)
        {
            Point pt = PointToScreen(new Point(button4.Left, button4.Top));
            ilp.Show(pt.X, pt.Y - 40);
        }

        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (/*e.Modifiers == Keys.Control && */e.KeyCode == Keys.Enter)
            {
                /*string stringState = threadClient.ThreadState.ToString();

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
                }*/
                this.button1_Click(sender, e);//触发button事件 
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Random r = new Random();
            this.BackgroundImage = Image.FromFile(r.Next(4).ToString() + "_chat.jpg");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
