/*
 * last edited by Du Delun
 * 2014/12/29 21:50
 * 实现登录功能
 * 重写窗口关闭和最小化函数
 * 实现背景图片随机显示功能
 * 2014/12/30 18:25
 * 添加鼠标拖拽移动窗口功能
 * 实现登录窗口和好友列表跳转功能
 * 解决不同窗口使用同一socket的问题
 * 2015/12/31 22:57
 * 添加窗口淡入淡出功能
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

namespace iChat
{
    public partial class Form1 : Form
    {
        string username;
        string password;
        public Form1()
        {
            InitializeComponent();
        }

        private void usernametext_TextChanged(object sender, EventArgs e)
        {
            username = usernametext.Text;
        }

        private void passwordtext_TextChanged(object sender, EventArgs e)
        {
            password = passwordtext.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int port = 8000;
                string host = "166.111.180.60";
                //创建终结点EndPoint
                IPAddress ip = IPAddress.Parse(host);
                //IPAddress ipp = new IPAddress("166.111.180.60");
                IPEndPoint ipe = new IPEndPoint(ip, port);//把ip和端口转化为IPEndpoint实例
                //创建socket并连接到服务器
                Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                c.Connect(ipe);//连接到服务器
                //向服务器发送信息
                string sendStr = username + "_" + password;
                byte[] bs = Encoding.ASCII.GetBytes(sendStr);//把字符串编码为字节
                c.Send(bs, bs.Length, 0);//发送信息
                //接受从服务器返回的信息
                string recvStr = "";
                byte[] recvBytes = new byte[1024]; 
                int bytes; 
                bytes = c.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 
                recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                if (recvStr == "lol"){
                    for (double d = 1; d > 0; d -= 0.02)
                    {
                        System.Threading.Thread.Sleep(1);
                        Application.DoEvents();
                        this.Opacity = d;
                        this.Refresh();
                    }
                    this.Hide();
                    Form2 f2 = new Form2(username,c);
                    f2.ShowDialog();
                    this.Close();
                }
                else if (recvStr == "Incorrect login No." || (username+"_").Length != 11)
                    MessageBox.Show("用户名错误，请重新输入！","登录失败");
                else
                    MessageBox.Show("密码错误，请重新输入！", "登录失败");
            }
            catch (SocketException ee)
            {
                Console.WriteLine("SocketException:{0}", ee);
            }
        }

        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标签是否为左键
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//如果按下的是鼠标左键
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是按下的状态
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是松开的状态
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Random r = new Random();
            this.BackgroundImage = Image.FromFile(r.Next(5).ToString() + ".jpg");
            for (double d = 0.01; d < 1; d += 0.04)
            {
                System.Threading.Thread.Sleep(1);
                Application.DoEvents();
                this.Opacity = d;
                this.Refresh();
            }
            this.Hide();
            this.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (double d = 1; d > 0; d -= 0.04)
            {
                System.Threading.Thread.Sleep(1);
                Application.DoEvents();
                this.Opacity = d;
                this.Refresh();
            }
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("密码：net2014", "密码提示");
        }
    }
}