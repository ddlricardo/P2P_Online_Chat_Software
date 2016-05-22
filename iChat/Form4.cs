/*
 * last edited by Du Delun
 * 2015/1/8 23:50
 * 重写窗口关闭和最小化函数
 * 添加鼠标拖拽移动窗口功能
 * 实现ListBox接收来自前一窗口的传值
 * 实现对用户双击选择的好友进行存储并传到下一窗口
 * 界面美化
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

namespace iChat
{
    public partial class Form4 : Form
    {
       
        int count = 0,group_num = 0;
        string usernanme;

        bool m_state;
        bool m_positive;
        string[] name_client = new string[20];
        string[] IP_client = new string[20];
        string[] m_name_client = new string [20];
        string[] m_IP_client = new string [20];

        public Form4(string username,string[] str1,string[] str2,int num)
        {
            InitializeComponent();
            this.usernanme = username;
           
            m_state=true;
            m_positive = true;
            int i = 0;
            group_num = 0;
            count = num;
            for (i = 0; i < num; i++)
            {
                name_client[i] = str1[i];
                IP_client[i] = str2[i];
                listBox1.Items.Add(name_client[i]);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int m_num = Frmcnct.Getcurrent_num();

            Form5 f5 = new Form5(usernanme, m_num, m_state, m_positive, m_IP_client, m_name_client, group_num);
            f5.Show();
            this.Close();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int i = 0;
            string tmp = listBox1.SelectedItem.ToString();
            for (i = 0; i < count; i++)
            {
                if (name_client[i] == tmp)
                {
                    m_name_client[group_num] = name_client[i];
                    m_IP_client[group_num] = IP_client[i];
                    listBox2.Items.Add(tmp);
                    group_num++;
                }
            }
        }

        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标签是否为左键
        private void Form4_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//如果按下的是鼠标左键
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Form4_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是按下的状态
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Form4_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)//如果鼠标左键是松开的状态
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }



    }
}
