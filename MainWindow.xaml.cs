using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace 热电比拟
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
//程止方
        #region 导入动态库
        [DllImport("CHIDClass.dll")]
        private static extern int GetNumberOfDevice();
        [DllImport("CHIDClass.dll")]
        private static extern ushort GetPID(int i);
        [DllImport("CHIDClass.dll")]
        private static extern bool OpenDevice();
        [DllImport("CHIDClass.dll")]
        private static extern int GetBoardID();
        [DllImport("CHIDClass.dll")]
        private static extern void GetDllVer(byte[] v);
        [DllImport("CHIDClass.dll")]
        private static extern bool AnalogOutput(int ch, double val, int openClose);
        [DllImport("CHIDClass.dll")]
        private static extern double GetAnalog(int mode, int ch, int range);
        [DllImport("CHIDClass.dll")]
        private static extern bool GetArrayAnalog(int mode, int ch, int range, double[] arrayData);
        [DllImport("CHIDClass.dll")]
        private static extern bool GetALLChAnalog(int mode, int range, double[] arrayData);
        [DllImport("CHIDClass.dll")]
        private static extern bool GPIOOutput(int portName, int val);
        [DllImport("CHIDClass.dll")]
        private static extern bool GPIOOutputAll(int val);
        [DllImport("CHIDClass.dll")]
        private static extern int GPIOInput(int portName);
        [DllImport("CHIDClass.dll")]
        private static extern int GPIOInputAll();
        [DllImport("CHIDClass.dll")]
        private static extern bool PWM_Output(int fre, int dutyCyle, int ch, int isOpen);
        [DllImport("CHIDClass.dll")]
        private static extern bool PWM_Input(int infre, int[] data, int ch, int isOpen);
        [DllImport("CHIDClass.dll")]
        private static extern bool SetBoardID(int ID);
        [DllImport("CHIDClass.dll")]
        private static extern void GetBoardVersion(byte[] arrayVer);
        [DllImport("CHIDClass.dll")]
        private static extern bool ClearCounter(int i);
        [DllImport("CHIDClass.dll")]
        private static extern bool ReadCounter(int[] arrayData);
        #endregion
        /// <summary>
        /// 手动测试线程
        /// </summary>
        System.Threading.Thread dandianth;
        /// <summary>
        ///数据采集的线程
        /// </summary>
        System.Threading.Thread caijith;
        /// <summary>
        /// 实验流程进行的线程
        /// </summary>
        System.Threading.Thread liuchength;
        System.Threading.Thread Time;

        /// <summary>
        /// 分界线，小于此值表示未测量，大于此值表示正在测量
        /// </summary>
        double boud = 0.05;
        int rowstart = 0;
        int columnstart = 0;
        /// <summary>
        ///每个点的测量时间
        /// </summary>
        int timeshiyan = 3;
        /// <summary>
        ///每个点的间隔时间
        /// </summary>
        int timejiange = 3;
        /// <summary>
        /// 倒计时时间
        /// </summary>
        int timcout;
        /// <summary>
        ///是否开始倒计时
        /// </summary>
        bool timestart = false;
        /// <summary>
        /// 单点测量时记录的总数据个数
        /// </summary>
        int maxdian = 15;
        /// <summary>
        /// 当前时时采集的数据
        /// </summary>
        double NowV = 0.0;

        /// <summary>
        /// 是否开始实验
        /// </summary>
        bool teststart = false;
        /// <summary>
        /// 是否开始记录
        /// </summary>
        bool record = false;
        /// <summary>
        /// 间隙时间是否定时
        /// </summary>
        bool dsjg = false;
        /// <summary>
        /// 当前测试的点位置
        /// </summary>
        int srow, scolumn;
        Grid Nowgridk;
        TextBlock[,] tbldy = new TextBlock[12, 16];
        TextBlock nowtextblock;
        public MainWindow()
        {
            InitializeComponent();
            //TextBlock tbla1 = this.bta1.Template.FindName("Vdata", bta1) as TextBlock;//拿到模板中子控件的一种方法
            ////bta1.Template.FindName("Vdata");
            //tbla1.Text = (3.05).ToString();
        }
        /// <summary>
        /// 触点点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bta1_Click(object sender, RoutedEventArgs e)
        {
            if (teststart && (bool)sdrb.IsChecked)
            {

                Button bt = sender as Button;
                string s = bt.Tag as string;


                if (dandianth != null && dandianth.IsAlive)
                {
                    dandianth.Abort();
                    tbldy[srow, scolumn].Background = Brushes.AliceBlue;
                }
                dsjg = false;
                string[] rows = s.Split(',');
                srow = int.Parse(rows[0]);
                scolumn = int.Parse(rows[1]);

                dandianth = new System.Threading.Thread(testone);
                dandianth.Start();
                dandianth.IsBackground = true;

            }

        }

        /// <summary>
        /// 手动单点测试的时候的一个点
        /// </summary>
        /// <param name="o"></param>
        private void testone(object o)
        {
            if (teststart == false) return;
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {//更改前台界面的程序
                //对应闪烁显示动画
                tbldy[srow, scolumn].Background = Brushes.YellowGreen;

            });
            testpoit(srow, scolumn);
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {//更改前台界面的程序
                //对应闪烁显示动画
                tbldy[srow, scolumn].Background = Brushes.AliceBlue;

            });
        }
        /// <summary>
        /// 自动选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void zdcheck_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((bool)rb.IsChecked)
            {
                //if (stpjgsj != null) stpjgsj.Visibility = Visibility.Visible;
                if (dandianth != null && dandianth.IsAlive)
                {
                    dandianth.Abort();
                    tbldy[srow, scolumn].Background = Brushes.AliceBlue;
                }

                if (teststart)
                {
                    if (liuchength != null && liuchength.IsAlive)
                    {
                        liuchength.Abort();
                    }
                    if (tbxddjgsj.IsEnabled) dsjg = true;
                    else dsjg = false;
                    rowstart = srow;
                    columnstart = scolumn;
                    liuchength = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(test));
                    //liuchength.Priority = System.Threading.ThreadPriority.Highest;
                    liuchength.Start();//开启线程
                    //当关闭主窗体子线程无法关闭时，设置子线程的以下属性问题就解决了
                    liuchength.IsBackground = true;
                }

            }
        }
        /// <summary>
        /// 手动选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sdcheck_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((bool)rb.IsChecked)
            {
                //if (stpjgsj != null) stpjgsj.Visibility = Visibility.Collapsed;
                if (liuchength != null && liuchength.IsAlive)
                {
                    liuchength.Abort();
                    tbldy[srow, scolumn].Background = Brushes.AliceBlue;
                }

            }
        }
        /// <summary>
        /// 开始实验
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartCL_Click(object sender, RoutedEventArgs e)
        {

            if (btnStartCL.Content == "结束")
            {

                //if (liuchength != null && liuchength.IsAlive)
                //{
                //    liuchength.Abort();
                //}
                btnStartCL.Content = "测量";
                if ((bool)zdrb.IsChecked)
                {
                    if (nowtextblock != null) nowtextblock.Background = Brushes.AliceBlue;
                }
                else
                    tbldy[srow, scolumn].Background = Brushes.AliceBlue;
                teststart = false;//结束实验
            }
            else
            {

                btnStartCL.Content = "结束";

                teststart = true;//开始实验
                if ((bool)zdrb.IsChecked)
                {
                    if (liuchength != null && liuchength.IsAlive)
                    {
                        liuchength.Abort();
                    }

                    rowstart = 0;
                    columnstart = 0;
                    liuchength = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(test));
                    //liuchength.Priority = System.Threading.ThreadPriority.Highest;
                    liuchength.Start();//开启线程
                    //当关闭主窗体子线程无法关闭时，设置子线程的以下属性问题就解决了
                    liuchength.IsBackground = true;
                }
                else
                {
                    if (liuchength != null && liuchength.IsAlive)
                    {
                        liuchength.Abort();
                    }
                }

            }

        }

        /// <summary>
        /// 自动实验测量
        /// </summary>
        /// <param name="row">采集点的起始行</param>
        /// <param name="clomn">采集点的起始列</param>
        /// <param name="time">每个采集点的时间s</param>
        private void test(object o)
        {
            for (int i = rowstart; i < 12; i++)
            {
                if (i != rowstart)
                {
                    columnstart = 0;
                }
                for (int j = columnstart; j < 16; j++)
                {
                    srow = i; scolumn = j;
                    if (teststart == false) return;
                    if (i >= 5 && j >= 5) break;
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
                    {//更改前台界面的程序
                        //对应闪烁显示动画
                        tbldy[i, j].Background = Brushes.Yellow;

                    });
                    System.Threading.Thread.Sleep(100);

                    //if (caijith != null && caijith.IsAlive)
                    //{
                    //    caijith.Abort();
                    //}
                    testpoit(i, j);
                    //caijith = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(getone));
                    ////caijith.Priority = System.Threading.ThreadPriority.Highest;
                    //caijith.Start();
                    //caijith.IsBackground = true;
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
                    {//更改前台界面的程序
                        //对应闪烁显示动画
                        tbldy[i, j].Background = Brushes.AliceBlue;

                    });
                    System.Threading.Thread.Sleep(100);
                }
            }
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {
                btnStartCL.Content = "测量";
                teststart = false;//开始实验
                srow = 0;
                scolumn = 0;
            });


        }
        List<double> LVtest = new List<double>();
        /// <summary>
        /// 假测量方法
        /// </summary>
        /// <param name="row">点所在的行</param>
        /// <param name="column">点所在的列</param>
        private void testpoit1(int row, int column)
        {

            if (caijith != null && caijith.IsAlive)
            {
                caijith.Abort();
            }
            while (true)
            {//测试开始
                if (checkin()) break;
            }

            LVdata.Clear();
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {//更改前台界面的程序
                //对应闪烁显示动画
                nowtextblock = tbldy[row, column];
                Nowgridk = VisualTreeHelper.GetParent(tbldy[row, column]) as Grid;
                //闪烁显示
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0.1;//变化起始值
                da.To = 1;//变化结束值
                //da.RepeatBehavior = RepeatBehavior.Forever;//一直循环
                da.RepeatBehavior = new RepeatBehavior(timeshiyan * 1000 / 500);//循环n次
                //da.AutoReverse = true;    // 是否需要再反向进行一次        
                da.Duration = TimeSpan.FromMilliseconds(500);//动画进行一次的时间
                Nowgridk.BeginAnimation(Grid.OpacityProperty, da);

                // tbldy[row, column].Text = timeshiyan.ToString();
            });
            System.Threading.Thread.Sleep(100);
            timestart = true;
            timcout = timeshiyan;
            //System.Threading.Thread timeth = new System.Threading.Thread(tick);
            //timeth.Start();
            //timeth.IsBackground = true;


            int zq = 50;//采集周期
            //if (caijith != null && caijith.IsAlive)
            //{
            //    caijith.Abort();
            //}

            for (int k = 0; k < timeshiyan * 1000 / zq; k++)
            {

                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
                {
                    //更改前台界面的程序
                    tblsssj.Text = (row + column).ToString("F2");
                    tbljgsj.Text = (row + column).ToString("F2");
                });
                System.Threading.Thread.Sleep(zq);

            }

            //把测量结果放到对应的位置
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {
                tbldy[row, column].Text = tbljgsj.Text;
            });
            while (true)
            {//测试结束
                if (checkout()) break;
            }
            //caijith = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(getone));
            ////caijith.Priority = System.Threading.ThreadPriority.Highest;
            //caijith.Start();
            //caijith.IsBackground = true;

        }
        /// <summary>
        /// 对某一点处的电压进行采集
        /// </summary>
        /// <param name="row">点所在的行</param>
        /// <param name="column">点所在的列</param>
        private void testpoit(int row, int column)
        {

            if (caijith != null && caijith.IsAlive)
            {
                caijith.Abort();
            }
            while (true)
            {//测试开始
                if (checkin()) break;
            }
            LVdata.Clear();
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {//更改前台界面的程序
                //对应闪烁显示动画
                //闪烁显示
                DoubleAnimation da = new DoubleAnimation();
                da.From = 0.1;//变化起始值
                da.To = 1;//变化结束值
                //da.RepeatBehavior = RepeatBehavior.Forever;//一直循环
                da.RepeatBehavior = new RepeatBehavior(timeshiyan * 1000 / 500);//循环n次
                //da.AutoReverse = true;    // 是否需要再反向进行一次        
                da.Duration = TimeSpan.FromMilliseconds(500);//动画进行一次的时间
                nowtextblock = tbldy[row, column];
                Nowgridk = VisualTreeHelper.GetParent(tbldy[row, column]) as Grid;
                Nowgridk.BeginAnimation(Grid.OpacityProperty, da);
                //tbldy[row, column].Text = timeshiyan.ToString();
            });
            System.Threading.Thread.Sleep(30);
            timestart = true;
            timcout = timeshiyan;
            int zq = 100;//采集周期
            for (int k = 0; k < timeshiyan * 1000 / zq; k++)
            {
                int i;
                //采集卡的型号，现在没什么用
                ushort pid;
                OpenDevice();
                i = GetNumberOfDevice();

                if (i > 0)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        tblstatus.Text = "通信正常";
                        tblstatus.Background = Brushes.Green;
                    });
                    System.Threading.Thread.Sleep(50);
                    pid = GetPID(0);
                    int ch = 0;//采集的通道
                    int mode = 0;//采集模式，0为单点输入，1为差分输入
                    int range = 5;//采集的模拟量范围，从0到5分别为：±0.256V,±0.512V,±1.024V,±2.048V,±4.096V,±5.25V
                    double V = GetAnalog(mode, ch, range);//单通道采集
                    if (shishidata.Count > 4) shishidata.RemoveAt(0);
                    shishidata.Add(V);
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
                    {
                        //更改前台界面的程序
                        tblsssj.Text = V.ToString("F3");
                        tbljgsj.Text = dataaddcal(V).ToString("F3");
                    });
                    System.Threading.Thread.Sleep(zq);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        tblstatus.Text = "通信错误";
                        tblstatus.Background = Brushes.Red;
                    });
                    System.Threading.Thread.Sleep(50);
                }
            }

            //把测量结果放到对应的位置
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
            {
                tbldy[row, column].Text = tbljgsj.Text;
            });
            System.Threading.Thread.Sleep(20);
            while (true)
            {//测试结束
                if (checkout()) break;
            }
            caijith = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(getone));
            //caijith.Priority = System.Threading.ThreadPriority.Highest;
            caijith.Start();
            caijith.IsBackground = true;

        }
        /// <summary>
        /// 倒计时显示
        /// </summary>
        /// <param name="o"></param>
        private void tick()
        {
            while (true)
            {
                if (timestart)
                {
                    //倒计时
                    for (int t = timcout; t > 0; t--)
                    {

                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
                        {
                            if (nowtextblock != null) nowtextblock.Text = t.ToString();
                        });
                        System.Threading.Thread.Sleep(1000);//等待1秒                                                          
                    }
                    //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    //{
                    //    nowtextblock.Text = "0";
                    //});
                    timestart = false;
                }

            }
        }

        /// <summary>
        /// 检查是否已放在了触点上了
        /// </summary>
        /// <returns></returns>
        private bool checkin()
        {
            bool vacant = false;
            if (dsjg)
            {
                System.Threading.Thread.Sleep((int)(timejiange * 1000));
                vacant = true;
            }
            else
            {
                int i;
                //采集卡的型号，现在没什么用
                ushort pid;

                try {
                    OpenDevice();
                    i = GetNumberOfDevice(); }
                catch { i = 0; }
                if (i > 0)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        tblstatus.Text = "通信正常";
                        tblstatus.Background = Brushes.Green;
                    });
                    pid = GetPID(0);
                    int ch = 0;//采集的通道
                    int mode = 0;//采集模式，0为单点输入，1为差分输入
                    int range = 5;//采集的模拟量范围，从0到5分别为：±0.256V,±0.512V,±1.024V,±2.048V,±4.096V,±5.25V
                    NowV = GetAnalog(mode, ch, range);//单通道采集
                    if (shishidata.Count > 6) shishidata.RemoveAt(0);
                    shishidata.Add(NowV);
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
                    {
                        //更改前台界面的程序
                        if (teststart == false)
                        {
                            tbldycs.Text = NowV.ToString("F4");
                        }
                        else
                        {
                            tblsssj.Text = NowV.ToString("F2");
                        }


                    });
                    System.Threading.Thread.Sleep(200);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        tblstatus.Text = "通信错误";
                        tblstatus.Background = Brushes.Red;
                    });
                    System.Threading.Thread.Sleep(50);
                }
                if (shishidata.Count > 3)
                {
                    if (Math.Max(Math.Max(Math.Abs(shishidata[0] - shishidata[1]), Math.Abs(shishidata[1] - shishidata[2])), Math.Abs(shishidata[2] - shishidata[3])) > 0.2) vacant = true;
                }
                System.Threading.Thread.Sleep(500);
            }
            return vacant;
        }
        /// <summary>
        /// 检查是否已离开了触点
        /// </summary>
        /// <returns></returns>
        private bool checkout()
        {
            bool vacant = true;
            if (dsjg)
            {
                vacant = true;
            }
            else
            {
                OpenDevice();
                int i;
                //采集卡的型号，现在没什么用
                ushort pid;
                try { i = GetNumberOfDevice(); }
                catch { i = 0; }
                if (i > 0)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        tblstatus.Text = "通信正常";
                        tblstatus.Background = Brushes.Green;
                    });
                    System.Threading.Thread.Sleep(50);
                    pid = GetPID(0);
                    int ch = 0;//采集的通道
                    int mode = 0;//采集模式，0为单点输入，1为差分输入
                    int range = 5;//采集的模拟量范围，从0到5分别为：±0.256V,±0.512V,±1.024V,±2.048V,±4.096V,±5.25V
                    NowV = GetAnalog(mode, ch, range);//单通道采集
                    if (shishidata.Count > 5) shishidata.RemoveAt(0);
                    shishidata.Add(NowV);
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate()
                    {
                        //更改前台界面的程序
                        if (teststart == false)
                        {
                            tbldycs.Text = NowV.ToString("F4");
                        }
                        else
                        {
                            tblsssj.Text = NowV.ToString("F2");

                        }


                    });
                    System.Threading.Thread.Sleep(200);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        tblstatus.Text = "通信错误";
                        tblstatus.Background = Brushes.Red;
                    });
                    System.Threading.Thread.Sleep(50);
                }
                if (shishidata.Count > 3)
                {
                    //if (Math.Min(Math.Min(Math.Abs(shishidata[0] - shishidata[1]), Math.Abs(shishidata[1] - shishidata[2])), Math.Abs(shishidata[2] - shishidata[3])) > 0.3) vacant = true;
                    if (shishidata.Count > 3)
                    {
                        if (Math.Max(Math.Max(Math.Abs(shishidata[0] - shishidata[1]), Math.Abs(shishidata[1] - shishidata[2])), Math.Abs(shishidata[2] - shishidata[3])) >0.2) vacant = true;
                    }
                }
            }

            System.Threading.Thread.Sleep(500);
            return vacant;
        }


        List<double> LVdata = new List<double>();
        /// <summary>
        /// 时时添加数据处理
        /// </summary>
        /// <param name="d">新测的数据</param>
        /// <returns></returns>
        private double dataaddcal(double shishidata)
        {
            if (LVdata.Count > maxdian)
            {
                LVdata.RemoveAt(0);
            }
            LVdata.Add(shishidata);
            double data = 0.0;
            double residual = 0.1;

            List<List<double>> lldata = new List<List<double>>();
            List<double> data1 = new List<double>();
            lldata.Add(data1);
            int num = LVdata.Count;
            data1.Add(LVdata[0]);
            for (int i = 1; i < num; i++)
            {
                for (int j = 0; j < lldata.Count; j++)
                {
                    if (Math.Abs(lldata[j][0] - LVdata[i]) < residual)
                    {
                        lldata[j].Add(LVdata[i]);//添加到该列中去

                    }
                    else if (j == lldata.Count - 1)
                    {//没有相近就新加一列
                        List<double> d = new List<double>();
                        d.Add(LVdata[i]);
                        lldata.Add(d);
                    }
                }

            }
            int large = 0;
            for (int i = 1; i < lldata.Count; i++)
            {//找出数据最多的相同值
                if (lldata[large].Count < lldata[i].Count) large = i;
            }
            //求平均
            double all = 0.0;
            foreach (double dd in lldata[large])
                all += dd;

            data = all / lldata[large].Count;
            return data;
        }


        List<double> shishidata = new List<double>();

        /// <summary>
        ///时时采集显示数据
        /// </summary>
        /// <param name="ob"></param>
        public void getone(Object ob)
        {
            while (true)
            {
                int i;
                //采集卡的型号，现在没什么用
                ushort pid;
                OpenDevice();
                try { i = GetNumberOfDevice(); }
                catch { i = 0; }


                if (i > 0)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        tblstatus.Text = "通信正常";
                        tblstatus.Background = Brushes.Green;
                    });
                    System.Threading.Thread.Sleep(50);
                    pid = GetPID(0);
                    int ch = 0;//采集的通道
                    int mode = 0;//采集模式，0为单点输入，1为差分输入
                    int range = 5;//采集的模拟量范围，从0到5分别为：±0.256V,±0.512V,±1.024V,±2.048V,±4.096V,±5.25V
                    NowV = GetAnalog(mode, ch, range);//单通道采集
                    if (shishidata.Count > 6) shishidata.RemoveAt(0);
                    shishidata.Add(NowV);
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        //更改前台界面的程序
                        if (teststart == false)
                        {
                            tbldycs.Text = NowV.ToString("F4");
                        }
                        else
                        {
                            tblsssj.Text = NowV.ToString("F2");
                            //if (record)
                            //{
                            //    tbljgsj.Text = dataaddcal(NowV).ToString("F2");
                            //}
                        }


                    });
                    System.Threading.Thread.Sleep(200);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        tblstatus.Text = "通信错误";
                        tblstatus.Background = Brushes.Red;
                    });
                    System.Threading.Thread.Sleep(50);
                }

            }
        }
        public void getall()
        {
            while (true)
            {
                int ch = 0;//采集的通道
                int mode = 1;//采集模式，0为单点输入，1为差分输入
                int range = 5;//采集的模拟量范围，从0到5分别为：±0.256V,±0.512V,±1.024V,±2.048V,±4.096V,±5.25V
                double[] val1 = new double[32];

                mode = 0;
                range = 5;
                GetALLChAnalog(mode, range, val1);//所有通道同时采集

                //System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart());
                //caijith.Start();//开启线程
                //                // 当关闭主窗体子线程无法关闭时，设置子线程的以下属性问题就解决了
                //caijith.IsBackground = true;
                //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                //{
                //    //更改前台界面的程序
                //    tbldycs.Text = val.ToString("F4");
                //});
                System.Threading.Thread.Sleep(500);
            }
        }
        //private void Ceshi_Click(object sender, RoutedEventArgs e)
        //{
        //    if (caijith != null && caijith.IsAlive)
        //    {
        //        caijith.Abort();
        //    }
        //    caijith = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(getone));

        //    caijith.Start();//开启线程
        //                    //当关闭主窗体子线程无法关闭时，设置子线程的以下属性问题就解决了
        //    caijith.IsBackground = true;
        //    teststart = false;
        //}

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            int i;
            //采集卡的型号，现在没什么用
            ushort pid;

            i = GetNumberOfDevice();

            if (i > 0)
            {
                pid = GetPID(0);
                bt.Content = "设备已接";
                bt.Foreground = Brushes.Green;
            }
            else
            {
                bt.Content = "连接设备";
                bt.Foreground = Brushes.Red;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region
            tbldy[0, 0] = tbla1;
            tbldy[1, 0] = tbla2;
            tbldy[2, 0] = tbla3;
            tbldy[3, 0] = tbla4;
            tbldy[4, 0] = tbla5;
            tbldy[5, 0] = tbla6;
            tbldy[6, 0] = tbla7;
            tbldy[7, 0] = tbla8;
            tbldy[8, 0] = tbla9;
            tbldy[9, 0] = tbla10;
            tbldy[10, 0] = tbla11;
            tbldy[11, 0] = tbla12;

            tbldy[0, 1] = tblb1;
            tbldy[1, 1] = tblb2;
            tbldy[2, 1] = tblb3;
            tbldy[3, 1] = tblb4;
            tbldy[4, 1] = tblb5;
            tbldy[5, 1] = tblb6;
            tbldy[6, 1] = tblb7;
            tbldy[7, 1] = tblb8;
            tbldy[8, 1] = tblb9;
            tbldy[9, 1] = tblb10;
            tbldy[10, 1] = tblb11;
            tbldy[11, 1] = tblb12;

            tbldy[0, 2] = tblc1;
            tbldy[1, 2] = tblc2;
            tbldy[2, 2] = tblc3;
            tbldy[3, 2] = tblc4;
            tbldy[4, 2] = tblc5;
            tbldy[5, 2] = tblc6;
            tbldy[6, 2] = tblc7;
            tbldy[7, 2] = tblc8;
            tbldy[8, 2] = tblc9;
            tbldy[9, 2] = tblc10;
            tbldy[10, 2] = tblc11;
            tbldy[11, 2] = tblc12;

            tbldy[0, 3] = tbld1;
            tbldy[1, 3] = tbld2;
            tbldy[2, 3] = tbld3;
            tbldy[3, 3] = tbld4;
            tbldy[4, 3] = tbld5;
            tbldy[5, 3] = tbld6;
            tbldy[6, 3] = tbld7;
            tbldy[7, 3] = tbld8;
            tbldy[8, 3] = tbld9;
            tbldy[9, 3] = tbld10;
            tbldy[10, 3] = tbld11;
            tbldy[11, 3] = tbld12;

            tbldy[0, 4] = tble1;
            tbldy[1, 4] = tble2;
            tbldy[2, 4] = tble3;
            tbldy[3, 4] = tble4;
            tbldy[4, 4] = tble5;
            tbldy[5, 4] = tble6;
            tbldy[6, 4] = tble7;
            tbldy[7, 4] = tble8;
            tbldy[8, 4] = tble9;
            tbldy[9, 4] = tble10;
            tbldy[10, 4] = tble11;
            tbldy[11, 4] = tble12;

            tbldy[0, 5] = tblf1;
            tbldy[1, 5] = tblf2;
            tbldy[2, 5] = tblf3;
            tbldy[3, 5] = tblf4;
            tbldy[4, 5] = tblf5;
            tbldy[5, 5] = tblf6;
            tbldy[6, 5] = tblf7;
            tbldy[7, 5] = tblf8;
            tbldy[8, 5] = tblf9;
            tbldy[9, 5] = tblf10;
            tbldy[10, 5] = tblf11;
            tbldy[11, 5] = tblf12;

            tbldy[0, 6] = tblg1;
            tbldy[1, 6] = tblg2;
            tbldy[2, 6] = tblg3;
            tbldy[3, 6] = tblg4;
            tbldy[4, 6] = tblg5;
            tbldy[5, 6] = tblg6;

            tbldy[0, 7] = tblh1;
            tbldy[1, 7] = tblh2;
            tbldy[2, 7] = tblh3;
            tbldy[3, 7] = tblh4;
            tbldy[4, 7] = tblh5;
            tbldy[5, 7] = tblh6;

            tbldy[0, 8] = tbli1;
            tbldy[1, 8] = tbli2;
            tbldy[2, 8] = tbli3;
            tbldy[3, 8] = tbli4;
            tbldy[4, 8] = tbli5;
            tbldy[5, 8] = tbli6;

            tbldy[0, 9] = tblj1;
            tbldy[1, 9] = tblj2;
            tbldy[2, 9] = tblj3;
            tbldy[3, 9] = tblj4;
            tbldy[4, 9] = tblj5;
            tbldy[5, 9] = tblj6;

            tbldy[0, 10] = tblk1;
            tbldy[1, 10] = tblk2;
            tbldy[2, 10] = tblk3;
            tbldy[3, 10] = tblk4;
            tbldy[4, 10] = tblk5;
            tbldy[5, 10] = tblk6;

            tbldy[0, 11] = tbll1;
            tbldy[1, 11] = tbll2;
            tbldy[2, 11] = tbll3;
            tbldy[3, 11] = tbll4;
            tbldy[4, 11] = tbll5;
            tbldy[5, 11] = tbll6;

            tbldy[0, 12] = tblm1;
            tbldy[1, 12] = tblm2;
            tbldy[2, 12] = tblm3;
            tbldy[3, 12] = tblm4;
            tbldy[4, 12] = tblm5;
            tbldy[5, 12] = tblm6;

            tbldy[0, 13] = tbln1;
            tbldy[1, 13] = tbln2;
            tbldy[2, 13] = tbln3;
            tbldy[3, 13] = tbln4;
            tbldy[4, 13] = tbln5;
            tbldy[5, 13] = tbln6;

            tbldy[0, 14] = tblo1;
            tbldy[1, 14] = tblo2;
            tbldy[2, 14] = tblo3;
            tbldy[3, 14] = tblo4;
            tbldy[4, 14] = tblo5;
            tbldy[5, 14] = tblo6;

            tbldy[0, 15] = tblp1;
            tbldy[1, 15] = tblp2;
            tbldy[2, 15] = tblp3;
            tbldy[3, 15] = tblp4;
            tbldy[4, 15] = tblp5;
            tbldy[5, 15] = tblp6;


            #endregion
            #region 给个默认值
            //double[,] mr = new double[,]{{3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00,3.00 },
            //           { 3.00,2.90,2.81,2.71,2.62,2.55,2.49,2.45,2.43,2.42,2.41,2.41,2.40,2.40,2.40,2.40},
            //            {3.00,2.81,2.61,2.42,2.23,2.07,1.96,1.89,1.85,1.83,1.82,1.81,1.81,1.80,1.80,1.80},
            //{ 3.00,2.71,2.42,2.12,1.81,1.55,1.39,1.30,1.25,1.23,1.22,1.21,1.20,1.20,1.20,1.20},
            //                    { 3.00,2.62,2.23,1.82,1.36,0.91,0.74,0.67,0.64,0.62,0.61,0.61,0.60,0.60,0.60,0.60},
            //                        { 3.00,2.55,2.07,1.55,0.91,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00},
            //                            { 3.00,2.49,1.96,1.39,0.74,0.00,0,0,0,0,0,0,0,0,0,0},
            //                                { 3.00,2.45,1.89,1.30,0.67,0.00,0,0,0,0,0,0,0,0,0,0},
            //                                    { 3.00,2.43,1.85,1.26,0.64,0.00,0,0,0,0,0,0,0,0,0,0},
            //                                        { 3.00,2.42,1.83,1.23,0.62,0.00,0,0,0,0,0,0,0,0,0,0},
            //                                            { 3.00,2.41,1.82,1.22,0.61,0.00,0,0,0,0,0,0,0,0,0,0},
            //                                                { 3.00,2.41,1.82,1.22,0.61,0.00,0,0,0,0,0,0,0,0,0,0}};

            double[,] mr = new double[,]{{0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                       {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                        {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
            {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                                {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                                    {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                                        {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                                            {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                                               {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                                                    {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                                                        {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 },
                                                            {0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 } };

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (i > 5 && j > 5) break;
                    tbldy[i, j].Text = mr[i, j].ToString();
                }
            }
            #endregion

            timestart = false;
            timcout = 0;
            if (caijith != null && caijith.IsAlive)
            {
                caijith.Abort();
            }
            caijith = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(getone));
            //caijith.Priority = System.Threading.ThreadPriority.Highest;
            caijith.Start();//开启线程
            //当关闭主窗体子线程无法关闭时，设置子线程的以下属性问题就解决了
            caijith.IsBackground = true;

            //if (liuchength != null && liuchength.IsAlive)
            //{
            //    liuchength.Abort();
            //}
            //teststart = false;
            //liuchength = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(testone));
            //liuchength.Start();//开启线程
            //                   //当关闭主窗体子线程无法关闭时，设置子线程的以下属性问题就解决了
            //liuchength.IsBackground = true;


            if (Time != null && Time.IsAlive)
            {
                Time.Abort();
            }
            Time = new System.Threading.Thread(tick);
            //caijith.Priority = System.Threading.ThreadPriority.Highest;
            Time.Start();//开启线程当关闭主窗体子线程无法关闭时，设置子线程的以下属性问题就解决了
            Time.IsBackground = true;

        }

        private void btndengwen_Click(object sender, RoutedEventArgs e)
        {
            double[,] dy = new double[12, 16];
            for (int i = rowstart; i < 12; i++)
            {
                for (int j = columnstart; j < 16; j++)
                {
                    if (i > 5 && j > 5) break;
                    double.TryParse(tbldy[i, j].Text, out dy[i, j]);
                }
            }
            WinDengwen wdw = new WinDengwen(dy);
            wdw.ShowDialog();
            wdw.Owner = this;
        }
        private string tozimu(int i)
        {
            string s = "";
            switch (i)
            {
                case 0:
                    s = "A";
                    break;
                case 1:
                    s = "B";
                    break;
                case 2:
                    s = "C";
                    break;
                case 3:
                    s = "D";
                    break;
                case 4:
                    s = "E";
                    break;
                case 5:
                    s = "F";
                    break;
                case 6:
                    s = "G";
                    break;
                case 7:
                    s = "H";
                    break;
                case 8:
                    s = "I";
                    break;
                case 9:
                    s = "J";
                    break;
                case 10:
                    s = "K";
                    break;
                case 11:
                    s = "L";
                    break;
                case 12:
                    s = "M";
                    break;
                case 13:
                    s = "N";
                    break;
                case 14:
                    s = "O";
                    break;
                case 15:
                    s = "P";
                    break;
                case 16:
                    s = "Q";
                    break;
            }
            return s;
        }
        private void save_Click(object sender, RoutedEventArgs e)
        {

            #region NPOI插件导出到EXCEL
            SaveFileDialog sflg = new SaveFileDialog();
            sflg.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            sflg.Filter = "Excel(*.xls)|*.xls|Excel(*.xlsx)|*.xlsx";
            if (sflg.ShowDialog() == false)
            {
                return;
            }
            //this.gridView1.ExportToXls(sflg.FileName);


            //NPOI.xs book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            NPOI.SS.UserModel.IWorkbook book = null;
            //if (sflg.FilterIndex == 1)
            //{
            book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //}
            //else
            //{
            //   book = new NPOI.XS.UserModel.XSSFWorkbook();
            //}

            NPOI.SS.UserModel.ISheet sheet0 = book.CreateSheet("电势数据");


            // 添加数据
            int i = 0, j = 0;
            //sheet0cell.SetCellValue("β\\α");
            IRow sheet0Row0 = sheet0.CreateRow(0);
            for (i = 0; i < tbldy.GetLength(0); i++)
            {
                IRow sheet0Row = sheet0.CreateRow(i + 1);
                for (j = 0; j < tbldy.GetLength(1); j++)
                {
                    if (i > 5 && j > 5) break;
                    if (i == 0)
                    {

                        ICell sheet0cel0 = sheet0Row0.CreateCell(j + 1, CellType.STRING);
                        sheet0cel0.SetCellValue(tozimu(j));
                    }
                    if (j == 0)
                    {
                        ICell sheet0cel0 = sheet0Row.CreateCell(j, CellType.STRING);
                        sheet0cel0.SetCellValue((i + 1).ToString());
                    }
                    ICell sheet0cell = sheet0Row.CreateCell(j + 1, CellType.STRING);
                    sheet0cell.SetCellValue(tbldy[i, j].Text);
                }
            }

            // 写入 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            book = null;

            using (FileStream fs = new FileStream(sflg.FileName, FileMode.Create, FileAccess.Write))
            {
                byte[] data = ms.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Flush();
            }

            ms.Close();
            ms.Dispose();
            #endregion
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否清空数据重新测量", "警告", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                for (int i = rowstart; i < 12; i++)
                {
                    for (int j = columnstart; j < 16; j++)
                    {
                        if (i > 5 && j > 5) break;
                        tbldy[i, j].Text = "0";
                    }
                }
                scolumn = 0;
                srow = 0;

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            if (cbx.SelectedIndex == 0)
            {
                dsjg = true;
                if (tbxddjgsj != null) tbxddjgsj.IsEnabled = true;
            }
            else
            {
                dsjg = false;
                if (tbxddjgsj != null) tbxddjgsj.IsEnabled = false;
            }
        }

        private void tbxddjgsj_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbx = sender as TextBox;
            try
            {
                timejiange = int.Parse(tbx.Text);
                tbx.Foreground = Brushes.Black;
            }
            catch { tbx.Foreground = Brushes.Red; }
        }

        private void tbxddclsj_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbx = sender as TextBox;
            try
            {
                timeshiyan = int.Parse(tbx.Text);
                tbx.Foreground = Brushes.Black;
            }
            catch { tbx.Foreground = Brushes.Red; }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
