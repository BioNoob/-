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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace кружочек
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        Storyboard s;
        Random r_color;
        public MainWindow()
        {
            InitializeComponent();
            s = new Storyboard();
            r_color = new Random((int)DateTime.Now.Ticks);
        }

        private void JopaCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse l = new Ellipse() { Width = 30, Height = 30 };
            l.Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(r_color.Next(0, 255)),
         Convert.ToByte(r_color.Next(0, 255)), Convert.ToByte(r_color.Next(0, 255))));
            var pos = e.GetPosition(JopaCanvas);
            Canvas.SetLeft(l, pos.X - l.Width / 2);
            Canvas.SetTop(l, pos.Y - l.Height / 2);
            System.Diagnostics.Debug.WriteLine($"{pos.X} + {pos.Y}");
            JopaCanvas.Children.Add(l);
            lst.Add(new ElState(l, JopaCanvas));
        }



        private List<ElState> lst = new List<ElState>();
        public class ElState
        {
            public static Random r_path = new Random((int)DateTime.Now.Ticks);
            DoubleAnimation anim2;
            DoubleAnimation anim1;
            public ElState(Ellipse el, Canvas par)
            {
                l = el;
                Max_target.X = par.ActualWidth;
                Max_target.Y = par.ActualHeight;
                CalcNewPointsToMove();
            }
            enum Direction_w
            {
                Left,
                Right,
                None
            }
            enum Direction_y
            {
                Up,
                Down,
                None
            }

            Point Max_target = new Point();

            Direction_w cur_dir_w = Direction_w.None;
            Direction_y cur_dir_h = Direction_y.None;

            double k = 0d;
            Ellipse l { get; set; } = null;
            bool anim1_compl
            {
                get => anim1_compl1;
                set
                {
                    if (anim2_compl && value)
                    {
                        CalcNewPointsToMove();
                    }
                    else
                        anim1_compl1 = value;
                }
            }
            bool anim2_compl
            {
                get => anim2_compl1;
                set
                {
                    if (anim1_compl && value)
                    {
                        CalcNewPointsToMove();
                    }
                    else
                        anim2_compl1 = value;
                }
            }
            private bool anim1_compl1 = false;
            private bool anim2_compl1 = false;
            private Point cur_target;

            public void MoveTo()
            {
                var top = Canvas.GetTop(l);
                var left = Canvas.GetLeft(l);
                //TranslateTransform trans = new TranslateTransform();
                //l.RenderTransform = trans;
                string dir_y = cur_dir_h == Direction_y.Up ? "UP" : "DOWN";
                string dir_x = cur_dir_w == Direction_w.Right ? "RIGHT" : "LEFT";
                System.Diagnostics.Debug.WriteLine($"FROM {left}:{top} TO {cur_target.X}:{cur_target.Y}, DIR = {dir_x} {dir_y}");
                anim1 = new DoubleAnimation(top, cur_target.Y, TimeSpan.FromSeconds(2));
                anim2 = new DoubleAnimation(left, cur_target.X, TimeSpan.FromSeconds(2));
                anim1.Completed += Anim1_Completed;
                anim2.Completed += Anim2_Completed;
                l.BeginAnimation(Canvas.LeftProperty, anim2);
                l.BeginAnimation(Canvas.TopProperty, anim1);
                //l.BeginAnimation(TranslateTransform.XProperty, anim1);
                //l.BeginAnimation(TranslateTransform.YProperty, anim2);
            }

            private void CalcNewPointsToMove()
            {
                //1. определяем лево или право
                //2. если лево берем 0 по х, по y берем рандом от 0 до (top+hight)
                //3. если право берем ширину и рандом
                //4. когда доехали до точки - ширина высота круга (30) расщитываем новую точку движения в противоположенную

                //МЕНЯЕМ НАПРАВЛЕНИЕ ЕСЛИ УПЕРЛИСЬ В ГРАНИЦУ
                if (cur_dir_w == Direction_w.None)
                    cur_dir_w = (Direction_w)r_path.Next(0, 1);
                else
                    switch (cur_dir_w)
                    {
                        case Direction_w.Left:
                            if(cur_target.X <= l.Width) //уперлись в границу слева
                                cur_dir_w = Direction_w.Right;
                            break;
                        case Direction_w.Right:
                            if (cur_target.X >= Max_target.X - l.Width) //уперлись в границу справа
                                cur_dir_w = Direction_w.Left;
                            break;
                    }
                if (cur_dir_h == Direction_y.None)
                    cur_dir_h = (Direction_y)r_path.Next(0, 1);
                else
                    switch (cur_dir_h)
                    {
                        case Direction_y.Up:
                            if (cur_target.Y <= l.Height) //уперлись в границу сверху
                                cur_dir_h = Direction_y.Down;
                            break;
                        case Direction_y.Down:
                            if (cur_target.Y >= Max_target.Y - l.Height) //уперлись в границу снизу
                                cur_dir_h = Direction_y.Up;
                            break;
                    }

                /*
                 * 
                 * y = kx
                 * k = pos.x / pos.y
                 * x = -10 y =
                 * x =  10 y =
                 * y = 10  x =
                 * y = -10 x =
                 * анализируем что из этого вылетело из диапазона
                 * анализируем куда двигаемся (в какую сторону)
                 * выбираем результат 
                */
                // ПРИМЕР 30 на 30 зона
                // мист = от -15 до 15
                //Rect myst = new Rect();//(Max_target.X / 2 * -1, Max_target.Y / 2, Max_target.X, Max_target.Y);//new Point(Max_target.X / 2, Max_target.Y / 2), new Point(Max_target.X / 2 * -1, Max_target.Y / 2 * -1));
                //перестраиваем центр коордиант в 0.0 от - до +
                double b_myst = Max_target.Y / 2 * -1;
                double l_myst = Max_target.X / 2 * -1;

                double t_myst = Max_target.Y / 2;
                double r_myst = Max_target.X / 2;


                double x_1, y_1, x_2, y_2 = 0;
                var top = Canvas.GetTop(l) + l.Height / 2;
                var top_dif = Max_target.Y - top; //cмещение от конца рамки
                var top_rev = b_myst + top_dif; //координата Y точки по новой системе координат

                var left = Canvas.GetLeft(l) + l.Width / 2;
                var left_dif = Max_target.X - left; //cмещение от конца рамки
                var left_rev = r_myst - left_dif; //координата X точки по новой системе координат

                double alfa = r_path.Next(1, 89); //рандомим угол

                if(alfa < 45)
                {
                    //первый спопсоб для простого

                    //таргет_катет = катет * tg(угла)
                    //катет = если влево, то координата от левой стороны, если вправо двигаемся то от правой стороны
                    //точка по х = если влево то мин, если вправо то макс
                    //точка по y = если вверх, то длина от у0 до координаты - таргет катет
                    //              если вниз, то длина от ymax до координаты - таргет катет
                    double tar_kat = 0d;
                    double kat = 0d;
                    if(cur_dir_w == Direction_w.Left)
                    {
                        kat = Math.Abs(l_myst) - Math.Abs(left_rev);
                        tar_kat = kat * Math.Tan(alfa * Math.PI / 180);
                        cur_target.X = l_myst;
                        if (cur_dir_h == Direction_y.Up)
                            cur_target.Y = top_rev - tar_kat;
                        else
                            cur_target.Y = top_rev + tar_kat;
                    }


                }
                else
                {
                    //втрой способ
                    //таргет_катет_промежуток = катет * tg(угла)
                    //катет = если влево, то координата от левой стороны, если вправо двигаемся то от правой стороны
                    //ищем катет для игрека. угол2 = 90 - угол
                    //(таргет катет промежуток - катет)*tg(угол2)
                    //точка по х = если влево то мин + катет игрек, если вправо то макс - катет игрек
                    //точка по y = если вверх, 0
                    //              если вниз, макс
                }


                //k = top_rev / left_rev; //коэф наклона

                k = r_path.Next(-100,100) + r_path.Next(1,99) / 100d;

                y_1 = k * r_myst; //targ 1
                y_2 = k * l_myst;
                x_1 = t_myst / k;
                x_2 = b_myst / k;
                bool use_y = false;

                if (y_1 > 0 && y_1 < t_myst)
                    use_y = true;
                else if (y_1 < 0 && y_1 > b_myst)
                    use_y = true;
                else if (x_1 > 0 && x_1 < r_myst)
                    use_y = false;
                else if (x_1 < 0 && x_1 > l_myst)
                    use_y = false;
                cur_target.X = 0;
                cur_target.Y = 0;
                if (cur_dir_w == Direction_w.Left && cur_dir_h == Direction_y.Up)
                {
                    if (use_y)
                    {
                        cur_target.X = l_myst;
                        if (y_1 >= y_2)
                            cur_target.Y = y_1;
                        else
                            cur_target.Y = y_2;
                    }
                    else
                    {
                        cur_target.Y = t_myst;
                        if (x_1 <= x_2)
                            cur_target.X = x_1;
                        else
                            cur_target.X = x_2;
                    }
                }
                else if (cur_dir_w == Direction_w.Left && cur_dir_h == Direction_y.Down)
                {
                    if (use_y)
                    {
                        cur_target.X = l_myst;
                        if (y_1 <= y_2)
                            cur_target.Y = y_1;
                        else
                            cur_target.Y = y_2;
                    }
                    else
                    {
                        cur_target.Y = b_myst;
                        if (x_1 <= x_2)
                            cur_target.X = x_1;
                        else
                            cur_target.X = x_2;
                    }
                }
                else if (cur_dir_w == Direction_w.Right && cur_dir_h == Direction_y.Up)
                {
                    if (use_y)
                    {
                        cur_target.X = r_myst;
                        if (y_1 >= y_2)
                            cur_target.Y = y_1;
                        else
                            cur_target.Y = y_2;
                    }
                    else
                    {
                        cur_target.Y = t_myst;
                        if (x_1 >= x_2)
                            cur_target.X = x_1;
                        else
                            cur_target.X = x_2;
                    }
                }
                else if (cur_dir_w == Direction_w.Right && cur_dir_h == Direction_y.Down)
                {
                    if (use_y)
                    {
                        cur_target.X = r_myst;
                        if (y_1 <= y_2)
                            cur_target.Y = y_1;
                        else
                            cur_target.Y = y_2;
                    }
                    else
                    {
                        cur_target.Y = b_myst;
                        if (x_1 >= x_2)
                            cur_target.X = x_1;
                        else
                            cur_target.X = x_2;
                    }
                }


                cur_target.X = Max_target.X / 2 + cur_target.X; //Math.Abs(cur_target.X) * 2;
                cur_target.Y = Max_target.Y / 2 - cur_target.Y;//Math.Abs(cur_target.Y) * 2;
                if (cur_target.X == Max_target.X)
                    cur_target.X -= l.Width;
                if (cur_target.Y == Max_target.Y)
                    cur_target.Y -= l.Height;
                if ((Max_target.Y - cur_target.Y) < l.Height)
                    cur_target.Y = l.Height;
                if ((Max_target.X - cur_target.X) < l.Width)
                    cur_target.X = l.Width;
                //cur_target.X += l.Width;
                //cur_target.Y += l.Height;
                //if (cur_target.X < 0) cur_target.X = 0;
                //if (cur_target.Y < 0) cur_target.Y = 0;
                anim2_compl = false;
                anim1_compl = false;
                MoveTo();

            }
            private void Anim2_Completed(object sender, EventArgs e)
            {
                anim2_compl = true;
            }

            private void Anim1_Completed(object sender, EventArgs e)
            {
                anim1_compl = true;
            }
        }
    }
}
