using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace 热电比拟
{
    /// <summary>
    /// WinDengwen.xaml 的交互逻辑
    /// </summary>
    public partial class WinDengwen : Window
    {
        TextBlock[,] tbldy = new TextBlock[12, 16];
        double[,] dy;
        double[,] wd;
        public WinDengwen(double[,] dianya)
        {
            InitializeComponent();
            dy = dianya;
            wd = new double[12, 16];
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
            //double t1 = 30;
            double t2 = 0;
            double derte = 3;
            double C = 0.1;
            #region 实现电压温度的转换
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (i > 5 && j > 5) break;
                    //电压温度转换关系
                    wd[i, j] = t2 + dy[i, j] / C;
                    tbldy[i, j].Text = wd[i, j].ToString("f2");
                }
            }
            #endregion

            drawline(24);
            drawline(18);
            drawline(12);
            //drawline(6);

        }
        private void drawline( double lt1)
        {

            //画曲线
            Polyline line1 = new Polyline();
            line1.Stroke = Brushes.Red;
            line1.StrokeThickness = 2;
            PointCollection points1 = new PointCollection();
            int jg = 4;//每4个像素间隙画一个点
            int w = 40;//每个方格的宽度


            double x1 = 0.0;
            for (int k = w * 16; k > 0; k = k - jg)
            {//k为x坐标值
                for (int i = 1; i < 12; i++)
                {//查找每一列方格值
                    int kw = k / w - 1;//画点所在的列数
                    if ((i > 5 && kw > 5) || kw < 1) break;
                    double kx = (double)(k % w) / w;//点在方格内x方向的比例
                    double yx0span, yx1span;//点在方格几y方向的温度范围
                    yx0span = wd[i - 1, kw - 1] - wd[i, kw - 1];
                    yx1span = wd[i - 1, kw] - wd[i, kw];

                    double y0 = wd[i - 1, kw - 1] - kx * (wd[i - 1, kw - 1] - wd[i - 1, kw]);
                    double y1 = wd[i, kw - 1] - kx * (wd[i, kw - 1] - wd[i, kw]);
                    //if (kw < 4) break;
                    if (lt1 > y1)
                    {//若在这方格时
                        double x = k - w;
                        double y;
                        if ((wd[i - 1, kw] - wd[i, kw]) < 0.001) y = (i + 1) * w;
                        else
                        {
                            y = (i + 1) * w - w * (lt1 - y1) / (y0 - y1); //(kx*yx0span +(1-kx)*yx1span);
                        }
                        if (y0 - y1 < 1) jg = 1;
                        //else y = (i + 1) * w - w * (lt1 - wd[i, kw]) / (wd[i - 1, kw] - wd[i, kw]);
                        if (k == w * 16)
                        {//加一个起点
                            Point p0 = new Point(k, y);
                            points1.Add(p0);
                            TextBlock tblt1 = new TextBlock();
                            cvdwx.Children.Add(tblt1);
                            tblt1.Text = lt1.ToString() + "℃";
                            Canvas.SetLeft(tblt1, k + 5);
                            Canvas.SetTop(tblt1, y - 10);

                        }
                        x1 = x;
                        Point p = new Point(x, y);
                        points1.Add(p);
                        break;
                    }
                }
            }
            Point p1 = new Point(x1, w * 12);
            points1.Add(p1);
            line1.Points = points1;
            cvdwx.Children.Add(line1);


        }




    }
}
