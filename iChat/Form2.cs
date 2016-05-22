/*
 * last edited by Du Delun
 * 2015/1/1 18:50
 * 重写窗口关闭和最小化函数
 * 添加鼠标拖拽移动窗口功能
 * 添加好友列表本地存储功能
 * 好友查询、添加功能
 * 添加好友列表刷新功能
 * 2015/1/3 11:25
 * ListBox控件中条目添加图片
 * ListBox控件中条目的背景色进行设置
 * 界面美化
 * 2015/1/4 15:57
 * 添加窗口贴边隐藏
 * 添加上线/下线切换功能
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace iChat
{
    public partial class Form2 : Form
    {
        string user;
        int flag = 0;
        Socket s;
        int current_num;
        string IP_client;
        int num_client;
        bool ACK;
        bool positive;
        string name_client;
        int  mygroupnum ;
        int groupnum ;
        string []name_group=new string [20];
        int SERVERPOST = 8000;
        Thread threadserver;
      
        private Color RowBackColorAlt = Color.FromArgb(224, 224, 224);
        private Color RowBackColorSel = Color.FromArgb(150, 200, 250);
        public Form2(string str, Socket c)
        {
            InitializeComponent();
            s = c;
            user = str;
            label1.Text = user;
            comboBox1.Items.Add("在线");
            comboBox1.Items.Add("离线");
            comboBox1.SelectedIndex = 0;
            check_friend();
            threadserver = new Thread(MainServer);
            threadserver.IsBackground = true;
            runServer();
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
                    //AddressFamily.InterNetwork表示此IP为IPv4
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

        private void MainServer()
        {
            string serverHost = GetLocalIP();//ipEntry.AddressList[1].ToString();//转换为string类
            ///创建终结点（EndPoint）
            IPAddress ip = IPAddress.Parse(serverHost);//把ip地址字符串转换为IPAddress类型的实例
            IPEndPoint ipe = new IPEndPoint(ip, SERVERPOST);//用指定的端口和ip初始化IPEndPoint类的新实例
            //创建socket并开始监听
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建一个socket对像，如果用udp协议，则要用SocketType.Dgram类型的套接字
            s.Bind(ipe);//绑定EndPoint对像（2000端口和ip地址）
            while (true)
            {
                s.Listen(1000);//开始监听              
                ///接受到client连接，为此连接建立新的socket，并接受信息
                Socket temp = s.Accept();//为新建连接创建新的socket
                string recvStr = "";
                byte[] recvBytes = new byte[1024];
                int bytes;
                bytes = temp.Receive(recvBytes, recvBytes.Length, 0);//从客户端接受信息
                recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);//2012011447+num
                string[] str = recvStr.Split(new Char[] { '+' });
                current_num = Frmcnct.Getcurrent_num();
                //  string IP_client = ((IPEndPoint)(s.RemoteEndPoint)).Address.ToString();  
                IP_client = str[2];
                num_client = int.Parse(str[1]);
                positive = false;
                name_client = str[0];
                string type = str[3];
                //public Connect(int m_num, bool m_state, bool m_positive, string m_IP_client,  string m_name_client，int m_num_client,ACK)
                // Connect C0 = new Connect(user,current_num, true, false, IP_client,  str[0]，num_client,);
                if (str[3] == "Single")
                {
                    if (MessageBox.Show("是否同意来自" + str[0] + "的会话请求？", "会话请求", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        ACK = true;
                        temp.Close();
                    }
                    else
                    {
                        ACK = false;
                        temp.Close();
                    }
                    MethodInvoker mi = new MethodInvoker(this.ShowForm);
                    this.BeginInvoke(mi);
                }
                else
                {
                    if (MessageBox.Show("是否同意来自" + str[0] + "发起的群聊请求？", "群聊请求", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        ACK = true;
                        temp.Close();
                    }
                    else
                    {
                        ACK = false;
                        temp.Close();
                    }
                   mygroupnum = int.Parse(str[4]);
                  groupnum = int.Parse(str[5]);
                  for (int j = 1; j < groupnum;j++ )
                  {
                      name_group[j]=str[j+5];
                  }
                    name_group[0]=str[0];
                    MethodInvoker mi = new MethodInvoker(this.ShowForm_group);
                    this.BeginInvoke(mi);
                }

            }
            s.Close();
        }


        private void ShowForm()
        {
            Form3 f3 = new Form3(user, current_num, true, positive, IP_client, name_client, num_client, ACK);


            if (ACK == true)
            {
             f3.Show();
            }
            else
            { f3.Close();}

        }

        private void ShowForm_group()
        {
            Form6 f6 = new Form6(user, current_num, true, positive, IP_client, name_client, num_client, ACK, mygroupnum,groupnum, name_group);


            if (ACK == true)
            {
                f6.Show();
            }
            else
            { f6.Close();}

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string file = user + ".txt";
            string friend = textBox1.Text;
            if ((friend + "_").Length != 11)
                MessageBox.Show("输入错误，请重新输入！","添加好友");
            else
            {
                FileStream fin = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader brin = new StreamReader(fin, Encoding.Default);
                string sf = brin.ReadToEnd();
                sf = sf + "\r\n" + friend + "\r\n";
                brin.Close();
                fin.Close();
                FileStream fout = new FileStream(file, FileMode.Create, FileAccess.Write);
                StreamWriter brout = new StreamWriter(fout, Encoding.Default);
                brout.Write(sf);
                brout.Close();
                fout.Close();
                MessageBox.Show("添加好友成功！", "添加好友");
            }
            if (flag == 0)
                check_friend();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            check_friend();
        }

        private void check_friend()
        {
            if (flag == 1)
            {
                FriendList.Items.Clear();
                MessageBox.Show("您已下线，无法查看好友状态！", "Sorry");
            }
            else
            {
                string file = user + ".txt";
                if (File.Exists(file) != true)
                {

                    FileStream fout = new FileStream(file, FileMode.Create, FileAccess.Write);
                    StreamWriter brout = new StreamWriter(fout);
                    brout.Close();
                    fout.Close();

                }
                FileStream fin = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader brin = new StreamReader(fin, Encoding.Default);
                string friend = "";
                string recvStr = "";
                FriendList.Items.Clear();
                while (friend != null)
                {
                    friend = brin.ReadLine();
                    if (friend != null && !friend.Equals(""))
                    {
                        try
                        {
                            string sendStr = "q" + friend;
                            byte[] bs = Encoding.ASCII.GetBytes(sendStr);//把字符串编码为字节
                            s.Send(bs, bs.Length, 0);//发送信息
                            recvStr = "";
                            byte[] recvBytes = new byte[1024];
                            int bytes;
                            bytes = s.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 
                            recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                            if (recvStr == "n" || recvStr == "")
                                FriendList.Items.Add(friend + "(离线)");
                            else
                                FriendList.Items.Add(friend + "(在线)");
                        }
                        catch (SocketException ee)
                        {
                            Console.WriteLine("SocketException:{0}", ee);
                        }
                    }
                }
                brin.Close();
                fin.Close();
            }
        }


        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标签是否为左键
        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//如果按下的是鼠标左键
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是按下的状态
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是松开的状态
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }

        private void FriendList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string[] status = FriendList.SelectedItem.ToString().Split(new Char[] { '(', ')' });
            if (status[1] == "离线")
                MessageBox.Show("好友不在线，无法聊天！", "会话失败");
            else
            {
                    string sendStr = "q" + status[0];
                    string recvStr = "";
                    byte[] bs = Encoding.ASCII.GetBytes(sendStr);//把字符串编码为字节
                    s.Send(bs, bs.Length, 0);//发送信息
                    recvStr = "";
                    byte[] recvBytes = new byte[1024];
                    int bytes;
                    bytes = s.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 
                    recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);//对方IP
                    // public Connect(int m_num, bool m_state, bool m_positive, string m_IP_client, string m_name_client)
                    current_num = Frmcnct.Getcurrent_num();
                    // Connect C1 = new Connect(user,current_num, true, true, recvStr,status[0]);
                    bool ACK = true;//主动发起

                    Form3 f3 = new Form3(user, current_num, true, true, recvStr, status[0], 0, ACK);
                    if (Frmcnct.reply == false)
                    {
                        f3.Close();
                        Frmcnct.reply = true;
                    }
                    else
                        f3.Show();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (flag == 0)
            {
                try
                {
                    string sendStr = "logout" + user;
                    byte[] bs = Encoding.ASCII.GetBytes(sendStr);//把字符串编码为字节
                    s.Send(bs, bs.Length, 0);//发送信息
                    string recvStr = "";
                    byte[] recvBytes = new byte[1024];
                    int bytes;
                    bytes = s.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 
                    recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                }
                catch (SocketException ee)
                {
                    Console.WriteLine("SocketException:{0}", ee);
                }
            }
            Application.Exit();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (flag == 1)
            {
                FriendList.Items.Clear();
                MessageBox.Show("您已下线，无法查看好友状态！", "Sorry");
            }
            else
            {
                try
                {
                    string friendname = textBox1.Text;
                    string sendStr = "q" + friendname;
                    string recvStr = "";
                    byte[] bs = Encoding.ASCII.GetBytes(sendStr);//把字符串编码为字节
                    s.Send(bs, bs.Length, 0);//发送信息
                    byte[] recvBytes = new byte[1024];
                    int bytes;
                    bytes = s.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 
                    recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                    if (recvStr == "n" || recvStr == "" || (friendname + "_").Length != 11)
                        MessageBox.Show(friendname + "离线！", "好友状态");
                    else
                        MessageBox.Show(friendname + "在线！", "好友状态");
                }
                catch (SocketException ee)
                {
                    Console.WriteLine("SocketException:{0}", ee);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sendStr = "";
            if (comboBox1.Text == "离线")
            {
                if (flag == 0)
                {
                    try
                    {
                        sendStr = "logout" + user;
                        byte[] bs = Encoding.ASCII.GetBytes(sendStr);//把字符串编码为字节
                        s.Send(bs, bs.Length, 0);//发送信息
                        string recvStr = "";
                        byte[] recvBytes = new byte[1024];
                        int bytes;
                        bytes = s.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 
                        recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                        if (recvStr == "loo")
                        {
                            flag = 1;
                            s.Close();
                            MessageBox.Show("下线成功！", "下线");
                        }
                        else
                            MessageBox.Show("您不在线上！", "下线");
                    }
                    catch (SocketException ee)
                    {
                        Console.WriteLine("SocketException:{0}", ee);
                    }
                }
                else
                    MessageBox.Show("您不在线上！", "下线");
            }
            else if (comboBox1.Text == "在线")
            {
                if (flag == 1)
                {
                    try
                    {
                        int port = 8000;
                        string host = "166.111.180.60";
                        IPAddress ip = IPAddress.Parse(host);
                        IPEndPoint ipe = new IPEndPoint(ip, port);//把ip和端口转化为IPEndpoint实例
                        s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        s.Connect(ipe);//连接到服务器
                        sendStr = user + "_net2014";
                        byte[] bs = Encoding.ASCII.GetBytes(sendStr);//把字符串编码为字节
                        s.Send(bs, bs.Length, 0);//发送信息
                        string recvStr = "";
                        byte[] recvBytes = new byte[1024];
                        int bytes;
                        bytes = s.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 
                        recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                        if (recvStr == "lol")
                        {
                            flag = 0;
                            MessageBox.Show("上线成功！", "上线");
                        }
                        else
                            MessageBox.Show("上线失败！", "上线");
                    }
                    catch (SocketException ee)
                    {
                        Console.WriteLine("SocketException:{0}", ee);
                    }
                }
            }
        }

        private void FriendList_DrawItem(object sender, DrawItemEventArgs e)
        {
            Brush myBrush = Brushes.Black;
            string str = "";
            string[] status;
            str = FriendList.Items[e.Index].ToString();
            status = str.Split(new Char[] { '(', ')' });
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                myBrush = new SolidBrush(RowBackColorSel);
            else if (status[1] == "离线")
                myBrush = new SolidBrush(RowBackColorAlt);
            else
                myBrush = new SolidBrush(Color.White);
            e.Graphics.FillRectangle(myBrush, e.Bounds);
            e.DrawFocusRectangle();//焦点框
            Image image = Image.FromFile("touxiang.jpg");
            Graphics g = e.Graphics;
            Rectangle bounds = e.Bounds;
            Rectangle imageRect = new Rectangle(
                bounds.X,
                bounds.Y,
                bounds.Height,
                bounds.Height);
            Rectangle textRect = new Rectangle(
                imageRect.Right,
                bounds.Y,
                bounds.Width - imageRect.Right,
                bounds.Height);

            if (image != null)
            {
                g.DrawImage(
                    image,
                    imageRect,
                    0,
                    0,
                    image.Width,
                    image.Height,
                    GraphicsUnit.Pixel);
            }
            StringFormat strFormat = new StringFormat();
            strFormat.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(FriendList.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), textRect, strFormat); 
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer StopRectTimer = new System.Windows.Forms.Timer();
            StopRectTimer.Tick += new EventHandler(timer1_Tick);
            StopRectTimer.Interval = 100;//时间
            StopRectTimer.Enabled = true;
        }

        internal AnchorStyles StopAanhor = AnchorStyles.None;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Bounds.Contains(Cursor.Position))
            {
                switch (this.StopAanhor)
                {
                    case AnchorStyles.Top:
                        this.Location = new Point(this.Location.X, 0);
                        break;
                    case AnchorStyles.Left:
                        this.Location = new Point(0, this.Location.Y);
                        break;
                    case AnchorStyles.Right:
                        this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, this.Location.Y);
                        break;
                }
            }
            else
            {
                switch (this.StopAanhor)
                {
                    case AnchorStyles.Top:
                        this.Location = new Point(this.Location.X, (this.Height - 2) * (-1));
                        break;
                    case AnchorStyles.Left:
                        this.Location = new Point((-1) * (this.Width - 2), this.Location.Y);
                        break;
                    case AnchorStyles.Right:
                        this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 2, this.Location.Y);
                        break;
                }
            }
        }

        private void mStopAnthor()
        {
            if (this.Top <= 0)
                StopAanhor = AnchorStyles.Top;
            else if (this.Left <= 0)
                StopAanhor = AnchorStyles.Left;
            else if (this.Left >= Screen.PrimaryScreen.Bounds.Width - this.Width)
                StopAanhor = AnchorStyles.Right;
            else
                StopAanhor = AnchorStyles.None;
        }

        private void Form2_LocationChanged(object sender, EventArgs e)
        {
            this.mStopAnthor();
        }

        void runServer()
        {
            string stringState = threadserver.ThreadState.ToString();

            switch (stringState)
            {
                case "Background, Unstarted": //第一次启动
                    try
                    {
                        threadserver.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "Running":    //正在运行,此状态删掉亦可
                    break;
                case "Suspended": //挂起则恢复运行
                    //threadserver.Resume();
                    break;
                case "Stopped": //线程已停止则重新启动
                    threadserver = new Thread(MainServer);
                    threadserver.Start();
                    break;
                default: //什么都不做
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //查一下在线好友全都传进去
            int i = 0,count = 0;
            string[] name_client = new string [20];
            string[] IP_client = new string[20];
            string file = user + ".txt";
            if (File.Exists(file) != true)
            {

                FileStream fout = new FileStream(file, FileMode.Create, FileAccess.Write);
                StreamWriter brout = new StreamWriter(fout);
                brout.Close();
                fout.Close();

            }
            FileStream fin = new FileStream(file, FileMode.Open, FileAccess.Read);
            StreamReader brin = new StreamReader(fin, Encoding.Default);
            string friend = "";
            string recvStr = "";
            FriendList.Items.Clear();
            while (friend != null)
            {
                friend = brin.ReadLine();
                if (friend != null && !friend.Equals(""))
                {
                    try
                    {
                        string sendStr = "q" + friend;
                        byte[] bs = Encoding.ASCII.GetBytes(sendStr);//把字符串编码为字节
                        s.Send(bs, bs.Length, 0);//发送信息
                        recvStr = "";
                        byte[] recvBytes = new byte[1024];
                        int bytes;
                        bytes = s.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 
                        recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                        if (recvStr == "n" || recvStr == "")
                            FriendList.Items.Add(friend + "(离线)");
                        else
                        {
                            FriendList.Items.Add(friend + "(在线)");
                            name_client[i] = friend;
                            IP_client[i] = recvStr;
                            i++;
                        }

                    }
                    catch (SocketException ee)
                    {
                        Console.WriteLine("SocketException:{0}", ee);
                    }
                }
            }
            brin.Close();
            fin.Close();

            count = i;
            Form4 f4 = new Form4(user,name_client,IP_client,count);
            f4.Show();
        }

    }
}