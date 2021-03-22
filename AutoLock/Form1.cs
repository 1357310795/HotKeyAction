using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoLock
{
    public partial class Form1 : Form
    {

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        #region Member
        int KeyId;         //热键编号
        private IntPtr Handle;     //窗体句柄
        Form Window;     //热键所在窗体
        uint ControlKey;   //热键控制键
        uint Key;          //热键主键
        public delegate void OnHotKeyEventHandler();         //热键事件委托
        public event OnHotKeyEventHandler OnHotKey = null;   //热键事件
        static Hashtable KeyPair = new Hashtable();          //热键哈希表
        private const int WM_HOTKEY = 0x0312;                // 热键消息编号
        public enum KeyFlags                                 //控制键编码        
        {
            MOD_ALT = 0x1,
            MOD_CONTROL = 0x2,
            MOD_SHIFT = 0x4,
            MOD_WIN = 0x8
        }
        #endregion

        Boolean muted;

        public Form1()
        {
            InitializeComponent();
        }
        public void AnkHotKey(Form win, KeyFlags control, Keys key)
        {
            Handle = win.Handle;
            Window = win;
            ControlKey = (uint)control;
            Key = (uint)key;
            KeyId = (int)ControlKey + (int)Key * 10;
            if (KeyPair.ContainsKey(KeyId))
            {
                throw new Exception("热键已经被注册!");
            }
            //注册热键
            if (false == RegisterHotKey(Handle, KeyId, ControlKey, Key))
            {
                throw new Exception("热键注册失败!");
            }
            //添加这个热键索引
            KeyPair.Add(KeyId, this);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            AnkHotKey(this, KeyFlags.MOD_SHIFT, Keys.F5);
            notifyIcon1.Icon = this.Icon;
            notifyIcon1.ContextMenuStrip=contextMenuStrip1;
            notifyIcon1.Visible = true;
            Timer mini = new Timer();
            mini.Interval = 200;
            mini.Tick += Mini_Tick;
            mini.Start();
        }

        private void Mini_Tick(object sender, EventArgs e)
        {
            ((Timer)sender).Stop();
            WindowState = FormWindowState.Minimized;
        }

        //重写WndProc()方法，通过监视系统消息，来调用过程
        protected override void WndProc(ref Message m)//监视Windows消息
        {
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    foreach (Process p in Process.GetProcessesByName("msedge"))
                    {
                        Console.WriteLine(p.Id);
                        if(muted)
                            AudioController.AudioManager.SetApplicationVolume(p.Id, 100);
                        else
                            AudioController.AudioManager.SetApplicationVolume(p.Id, 0);
                        
                    }
                    muted = !muted;
                    Console.WriteLine("ok");
                    break;
            }
            base.WndProc(ref m); //将系统消息传递自父类的WndProc
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(Handle, KeyId);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if(WindowState == FormWindowState.Minimized)
                this.Hide();
        }
    }
}
