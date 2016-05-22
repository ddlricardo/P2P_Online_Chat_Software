using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iChat
{
   public class Frmcnct
    {
        public static bool reply = true;
        public static bool[] state = new bool[80] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public static int max_num = 1;
        public static int current_num = 0;
        public static int Getcurrent_num()
        {
            int i;
            for (i = 0; i < max_num; i++)
            {
                if (Frmcnct.state[i] == false)
                {
                    current_num = i;
                    Frmcnct.state[i] = true;
                    return current_num;
                }
            }
            max_num++;
            if (max_num < 80)
            {
                Frmcnct.state[max_num] = true;
                return max_num;
            }
            else
            {
                MessageBox.Show("端口号已经分配完毕，请关闭一些窗口", "端口号错误");
                return max_num;
            }  
        }  
   }
}
