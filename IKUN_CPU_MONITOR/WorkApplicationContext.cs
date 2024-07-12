using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IKUN_CPU_MONITOR.Properties;
using System.Diagnostics;
using System.Resources;
using System.Drawing;
using System.ComponentModel;


namespace IKUN_CPU_MONITOR
{
    public class WorkApplicationContext : ApplicationContext
    {
        //CPU监测间隔  3s监测一次
        private const int CPU_TIMER_DEFAULT_INTERVAL = 3000;
        //动画默认间隔
        private int ANIMATE_TIMER_DEFAULT_INTERVAL = Settings.Default.Speed;
        private string icon = "";
        private string iconStyle = "";
        private float interval = Settings.Default.Speed;
        private int iconIndex = 0;
        private PerformanceCounter cpuUsage;
        private ToolStripMenuItem iconMenu;
        private ToolStripMenuItem styleMenu;
        private ToolStripMenuItem speedMenu;
        private NotifyIcon notifyIcon;
        private Icon[] icons;
        private Timer animateTimer = new Timer();
        private Timer cpuTimer = new Timer();
        private static readonly Dictionary<string, (int capacity, string fileName)> IconConfig = new Dictionary<string, (int capacity, string fileName)>
    {
        { "IKUN", (13, "ikun") },
        { "猫", (5, "cat") },
        { "马", (14, "horse") },
        { "鹦鹉", (9, "parrot") }
    };

        public float Interval { get => Interval1; set => Interval1 = value; }
        public float Interval1 { get => interval; set => interval = value; }
        public float Interval2 { get => interval; set => interval = value; }

        public WorkApplicationContext() {
            Settings.Default.Reload();
            icon = Settings.Default.Icon;
            iconStyle = Settings.Default.Style;

            Application.ApplicationExit += new EventHandler(OnApplicationExit);

            // 创建一个PerformanceCounter实例来监视所有CPU的平均使用率  
             cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            // 初始化计数器,第一次的值可能不准确,丢弃
            cpuUsage.NextValue();
            setIcon();
            initComponent();
            initTimer();
            playAnimate();
        }

        /// <summary>
        ///  初始化菜单选项
        /// </summary>
        private void initComponent() {
            iconMenu = new ToolStripMenuItem("图标", null, new ToolStripMenuItem[]
            {
                new ToolStripMenuItem("IKUN",null,switchChecked){ Checked = icon.Equals("IKUN") },
                new ToolStripMenuItem("猫",null,switchChecked){ Checked = icon.Equals("猫") },
                new ToolStripMenuItem("马",null,switchChecked){ Checked = icon.Equals("马") },
                new ToolStripMenuItem("鹦鹉",null,switchChecked){ Checked = icon.Equals("鹦鹉") }
            });

            styleMenu = new ToolStripMenuItem("颜色", null, new ToolStripMenuItem[] {
                new ToolStripMenuItem ("浅色",null,switchStyle){Checked = iconStyle.Equals("dark")},
                new ToolStripMenuItem ("深色",null,switchStyle){Checked = iconStyle.Equals("light")}
            });

            speedMenu = new ToolStripMenuItem("速度", null, new ToolStripMenuItem[] {
                new ToolStripMenuItem ("100",null,switchSpeed){Checked = ANIMATE_TIMER_DEFAULT_INTERVAL == 100},
                new ToolStripMenuItem ("200",null,switchSpeed){Checked = ANIMATE_TIMER_DEFAULT_INTERVAL == 200},
                new ToolStripMenuItem ("300",null,switchSpeed){Checked = ANIMATE_TIMER_DEFAULT_INTERVAL == 300}
            });



            //初始化右键菜单
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip(new Container());
            contextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                iconMenu,
                styleMenu,
                speedMenu,
                new ToolStripMenuItem("开机自启动",null,Startup){ Checked = Utils.IsStartupEnabled()},
                new ToolStripSeparator(),
                new ToolStripMenuItem($"IKUN v{Application.ProductVersion}")
                {
                    Enabled = false
                },
                new ToolStripMenuItem("作者：清梦")
                {
                    Enabled = false
                },
                new ToolStripMenuItem("退出", null, Exit)
            });

            //初始化通知图标
            notifyIcon = new NotifyIcon()
            {
                Icon = icons[0],
                ContextMenuStrip = contextMenuStrip,
                Text = "0.0%",
                Visible = true
            };

        }

        private void initTimer() {
            //初始化监测CPU Timer
            cpuTimer.Interval = CPU_TIMER_DEFAULT_INTERVAL;
            cpuTimer.Tick += new EventHandler(ObserveCPUTick);
            cpuTimer.Start();
            //初始化播放图片 Timer

            animateTimer.Interval = ANIMATE_TIMER_DEFAULT_INTERVAL;
            animateTimer.Tick += new EventHandler(playAnimateTick);
        }

        /// <summary>
        /// 图标菜单项点击事件
        /// </summary>
        private void switchChecked(object sender, EventArgs e) {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            UpdateCheckedState(item, iconMenu);
            icon = item.Text;
            setIcon();
        }

        /// <summary>
        /// 颜色菜单项点击事件
        /// </summary>
        private void switchStyle(object sender, EventArgs e) {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            iconStyle = item.Text.Equals("深色")?"light":"dark";
            UpdateCheckedState(item, styleMenu);
            setIcon();
        }

        /// <summary>
        /// 速度菜单项点击事件
        /// </summary>
        private void switchSpeed(object sender, EventArgs e) {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ANIMATE_TIMER_DEFAULT_INTERVAL = int.Parse(item.Text);
            UpdateCheckedState(item, speedMenu);
            //及时更新动画播放速度
            ObserveCPUTick(null, null);
        }

        /// <summary>
        ///  加载对应图标到icons数组中
        /// </summary>
        private void setIcon() {
            ResourceManager rm = Resources.ResourceManager;
            if (!IconConfig.TryGetValue(icon, out var config))
            {
                config = IconConfig["IKUN"]; // 默认配置
            }
            List<Icon> list = new List<Icon>(config.capacity);

            for (int i = 0; i < config.capacity; i++)
            {
                Icon temp = rm.GetObject($"{iconStyle}_{config.fileName}_{i}") as Icon;
                if (temp != null) {
                    list.Add(temp);
                }
            }
            icons = list.ToArray();
        }

        private void playAnimateTick(object sender, EventArgs e) {
            if (icons.Length <= iconIndex) iconIndex = 0;
            notifyIcon.Icon = icons[iconIndex];
            iconIndex = (iconIndex + 1) % icons.Length;
        }

        private void playAnimate() {
            animateTimer.Stop();
            animateTimer.Interval = (int)Interval;
            animateTimer.Start();
        }

        private void ObserveCPUTick(object sender, EventArgs e)
        {
            float cpuState = Math.Min(100, cpuUsage.NextValue());
            notifyIcon.Text = $"CPU: {cpuState:f1}%";  //获取的占用率可能超过100%
            Interval = ANIMATE_TIMER_DEFAULT_INTERVAL / (float)Math.Max(1.0f,cpuState / 5.0f);
            playAnimate();
        }

        /// <summary>
        ///  更新勾选状态 
        /// </summary>
        /// <param name="sender">被勾选的菜单项</param>
        /// <param name="menu">整体菜单项</param>
        private void UpdateCheckedState(ToolStripMenuItem sender, ToolStripMenuItem menu)
        {
            foreach (ToolStripMenuItem item in menu.DropDownItems)
            {
                item.Checked = false;
            }
            sender.Checked = true;
        }

        /// <summary>
        ///   设置开机自启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Startup(object sender, EventArgs e) {
            ToolStripMenuItem obj = sender as ToolStripMenuItem;
            Utils.SetStartup(obj.Checked);
            obj.Checked = !obj.Checked;
        }

        private void Exit(object sender, EventArgs e)
        {
            cpuUsage.Close();
            animateTimer.Stop();
            cpuTimer.Stop();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            Settings.Default.Icon = icon;
            Settings.Default.Style = iconStyle;
            Settings.Default.Save();
        }
    }
}
