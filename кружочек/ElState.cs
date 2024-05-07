using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace кружочек
{
    public enum Direction_w
    {
        Left,
        Right,
        None
    }
    public enum Direction_h
    {
        Up,
        Down,
        None
    }
    public static class Dopnik
    {
        public static int Id;
        public static Random r_path = new Random((int)DateTime.Now.Ticks);
        private static double vector_mult(double ax, double ay, double bx, double by) //векторное произведение
        {
            return ax * by - bx * ay;
        }
        static void LineEquation(Point p1, Point p2, out double A, out double B, out double C)
        {
            A = p2.Y - p1.Y;
            B = p1.X - p2.X;
            C = -p1.X * (p2.Y - p1.Y) + p1.Y * (p2.X - p1.X);
        }
        public static bool areCrossing(Point p1, Point p2, Point p3, Point p4)//проверка пересечения
        {
            double v1 = vector_mult(p4.X - p3.X, p4.Y - p3.Y, p1.X - p3.X, p1.Y - p3.Y);
            double v2 = vector_mult(p4.X - p3.X, p4.Y - p3.Y, p2.X - p3.X, p2.Y - p3.Y);
            double v3 = vector_mult(p2.X - p1.X, p2.Y - p1.Y, p3.X - p1.X, p3.Y - p1.Y);
            double v4 = vector_mult(p2.X - p1.X, p2.Y - p1.Y, p4.X - p1.X, p4.Y - p1.Y);
            if ((v1 * v2) < 0 && (v3 * v4) < 0)
            {
                double a1, b1, c1, a2, b2, c2;
                LineEquation(p1, p2, out a1, out b1, out c1);
                LineEquation(p3, p4, out a2, out b2, out c2);
                Point p = CrossingPoint(a1, b1, c1, a2, b2, c2);
            }
            return false;
        }
        static Point CrossingPoint(double a1, double b1, double c1, double a2, double b2, double c2)
        {
            Point pt = new Point();
            double d = (double)(a1 * b2 - b1 * a2);
            double dx = (double)(-c1 * b2 + b1 * c2);
            double dy = (double)(-a1 * c2 + c1 * a2);
            pt.X = (int)(dx / d);
            pt.Y = (int)(dy / d);
            return pt;
        }
    }
    public class ElState
    {
        DoubleAnimation anim2;
        DoubleAnimation anim1;
        public bool need_ani = true;
        public ElState(Ellipse el, Canvas par, bool need_anim = true)
        {
            _id = ++Dopnik.Id;
            l = el;
            Max_target.X = par.ActualWidth;
            Max_target.Y = par.ActualHeight;
            l.LayoutUpdated += L_LayoutUpdated;
            anim1 = new DoubleAnimation();
            anim2 = new DoubleAnimation();
            need_ani = need_anim;
            owner = par;
            tar = new Ellipse() { Width = 15, Height = 15 };
            tar.Stroke = new SolidColorBrush(Colors.Black);
            tar.Fill = this.l.Fill;
            if (need_anim)
            {
                CalcNewPointsToMove();
            }
        }
        private Canvas owner;
        private Ellipse tar;
        //Пока с прошлой перерисовки не пройдет изменение координат на половину величины, не реагируем
        private bool resent_changed = false;
        public bool Resent_changed
        {
            get => resent_changed; set
            {
                resent_changed = value;
                if (value)
                {
                    Debug.WriteLine($"BALL {Id} is FRESES");
                    freeze_point.X = Left;
                    freeze_point.Y = Top;
                }
            }
        }
        Point freeze_point = new Point();
        private void L_LayoutUpdated(object sender, EventArgs e)
        {
            if (Left == cur_target.X || Top == cur_target.Y)
            {
                owner.Children.Remove(tar);
                CalcNewPointsToMove();
            }
        }

        public void Change_size_of_Target(Size s)
        {
            Max_target.X = s.Width;
            Max_target.Y = s.Height;
        }
        public double Left => Canvas.GetLeft(l);
        public double Top => Canvas.GetTop(l);
        public Point Center => new Point(Left / 2, Top / 2);
        public double Width => l.Width;
        public double Height => l.Height;
        private int _id;
        public int Id => _id;
        Point Max_target = new Point();

        private Direction_w cur_dir_w = Direction_w.None;
        private Direction_h cur_dir_h = Direction_h.None;

        Ellipse l { get; set; } = null;
        public Direction_w Cur_dir_w { get => cur_dir_w; set => cur_dir_w = value; }
        public Direction_h Cur_dir_h { get => cur_dir_h; set => cur_dir_h = value; }
        private Point cur_target;

        private void MoveTo(double time_anim)
        {
            anim1.From = Top;
            anim1.To = cur_target.Y;
            anim1.Duration = TimeSpan.FromSeconds(time_anim);
            anim2.From = Left;
            anim2.To = cur_target.X;
            anim2.Duration = TimeSpan.FromSeconds(time_anim);

            anim1.FillBehavior = FillBehavior.HoldEnd;
            anim2.FillBehavior = FillBehavior.HoldEnd;
            l.BeginAnimation(Canvas.LeftProperty, anim2);///, HandoffBehavior.SnapshotAndReplace);
            l.BeginAnimation(Canvas.TopProperty, anim1);//, HandoffBehavior.SnapshotAndReplace);
        }
        private void GetNewDirection(Direction_h dop_h = Direction_h.None, Direction_w dop_w = Direction_w.None, ElState Stat_obj = null)
        {
            if (dop_h != Direction_h.None && dop_w != Direction_w.None)
            {
                switch (cur_dir_h)
                {
                    case Direction_h.Up:
                        switch (cur_dir_w)
                        {
                            case Direction_w.Left:
                                if (dop_h == Direction_h.Up)
                                {
                                    if (dop_w == Direction_w.Left)
                                    {
                                        //dop_w = Direction_w.Right;
                                        cur_dir_h = Direction_h.Down;
                                    }
                                    else if (dop_w == Direction_w.Right)
                                    {
                                        //dop_w = Direction_w.Left;
                                        cur_dir_w = Direction_w.Right;
                                    }
                                }
                                else if (dop_h == Direction_h.Down)
                                {
                                    if (dop_w == Direction_w.Left)
                                    {
                                        //dop_h = Direction_h.Up;
                                        cur_dir_h = Direction_h.Down;
                                    }
                                    else if (dop_w == Direction_w.Right)
                                    {
                                        //dop_h = Direction_h.Up;
                                        cur_dir_h = Direction_h.Down;
                                    }
                                }
                                break;
                            case Direction_w.Right:
                                if (dop_h == Direction_h.Up)
                                {
                                    if (dop_w == Direction_w.Left)
                                    {
                                        // dop_w = Direction_w.Right;
                                        cur_dir_w = Direction_w.Left;
                                    }
                                    else if (dop_w == Direction_w.Right)
                                    {
                                        //dop_w = Direction_w.Left;
                                        cur_dir_h = Direction_h.Down;
                                    }
                                }
                                else if (dop_h == Direction_h.Down)
                                {
                                    if (dop_w == Direction_w.Left)
                                    {
                                        //dop_h = Direction_h.Up;
                                        cur_dir_h = Direction_h.Down;
                                    }
                                    else if (dop_w == Direction_w.Right)
                                    {
                                        //dop_h = Direction_h.Up;
                                        cur_dir_h = Direction_h.Down;
                                    }
                                }
                                break;
                        }
                        break;
                    case Direction_h.Down:
                        switch (cur_dir_w)
                        {
                            case Direction_w.Left:
                                if (dop_h == Direction_h.Up)
                                {
                                    if (dop_w == Direction_w.Left)
                                    {
                                        // dop_h = Direction_h.Down;
                                        cur_dir_h = Direction_h.Up;
                                    }
                                    else if (dop_w == Direction_w.Right)
                                    {
                                        // dop_w = Direction_w.Left;
                                        cur_dir_w = Direction_w.Right;
                                    }
                                }
                                else if (dop_h == Direction_h.Down)
                                {
                                    if (dop_w == Direction_w.Left)
                                    {
                                        //dop_h = Direction_h.Up;
                                        cur_dir_w = Direction_w.Right;
                                    }
                                    else if (dop_w == Direction_w.Right)
                                    {
                                        //dop_w = Direction_w.Left;
                                        cur_dir_w = Direction_w.Right;
                                    }
                                }
                                break;
                            case Direction_w.Right:
                                if (dop_h == Direction_h.Up)
                                {
                                    if (dop_w == Direction_w.Left)
                                    {
                                        //dop_w = Direction_w.Right;
                                        cur_dir_h = Direction_h.Up;
                                    }
                                    else if (dop_w == Direction_w.Right)
                                    {
                                        //dop_h = Direction_h.Down;
                                        cur_dir_h = Direction_h.Up;
                                    }
                                }
                                else if (dop_h == Direction_h.Down)
                                {
                                    if (dop_w == Direction_w.Left)
                                    {
                                        //dop_w = Direction_w.Right;
                                        //cur_dir_w = Direction_w.Left;
                                        cur_dir_h = Direction_h.Up;
                                    }
                                    else if (dop_w == Direction_w.Right)
                                    {
                                        //dop_h = Direction_h.Up;
                                        //cur_dir_w = Direction_w.Left;
                                        cur_dir_h = Direction_h.Up;
                                    }
                                }
                                break;
                        }
                        break;
                }
            }
            else
            {
                if(Stat_obj != null)
                {
                    //ну почти, надо еще знать с какой стороны заход на шар. интересная тема. может пригодится
                    //подумать что придумать
                    switch (cur_dir_w)
                    {
                        case Direction_w.Left:
                            switch (cur_dir_h)
                            {
                                case Direction_h.Up:
                                    break;
                                case Direction_h.Down:
                                    cur_dir_w = Direction_w.Right;
                                    break;
                            }
                            break;
                        case Direction_w.Right:
                            switch (cur_dir_h)
                            {
                                case Direction_h.Up:
                                    cur_dir_h = Direction_h.Down;
                                    break;
                                case Direction_h.Down:
                                    cur_dir_w = Direction_w.Left;
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    if (cur_dir_w == Direction_w.None)
                        cur_dir_w = (Direction_w)Dopnik.r_path.Next(0, 1);
                    else
                        switch (cur_dir_w)
                        {
                            case Direction_w.Left:
                                if (cur_target.X <= l.Width) //уперлись в границу слева
                                    cur_dir_w = Direction_w.Right;
                                break;
                            case Direction_w.Right:
                                if (cur_target.X >= Max_target.X - l.Width) //уперлись в границу справа
                                    cur_dir_w = Direction_w.Left;
                                break;
                        }
                    if (cur_dir_h == Direction_h.None)
                        cur_dir_h = (Direction_h)Dopnik.r_path.Next(0, 1);
                    else
                        switch (cur_dir_h)
                        {
                            case Direction_h.Up:
                                if (cur_target.Y <= l.Height) //уперлись в границу сверху
                                    cur_dir_h = Direction_h.Down;
                                break;
                            case Direction_h.Down:
                                if (cur_target.Y >= Max_target.Y - l.Height) //уперлись в границу снизу
                                    cur_dir_h = Direction_h.Up;
                                break;
                        }
                }
                //тут можно поменять для стационарного объекта, что он типо стена

            }
        }
        double curr_angle = 0.0;
        public void CalcNewPointsToMove(Direction_h dop_h = Direction_h.None, Direction_w dop_w = Direction_w.None, ElState Stat_obj = null)
        {
            if (!need_ani) return;

            //l.BeginAnimation(Canvas.LeftProperty, null, HandoffBehavior.Compose);
            //l.BeginAnimation(Canvas.TopProperty, null, HandoffBehavior.Compose);
            //1. определяем лево или право
            //2. если лево берем 0 по х, по y берем рандом от 0 до (top+hight)
            //3. если право берем ширину и рандом
            //4. когда доехали до точки - ширина высота круга (30) расщитываем новую точку движения в противоположенную

            //МЕНЯЕМ НАПРАВЛЕНИЕ ЕСЛИ УПЕРЛИСЬ В ГРАНИЦУ
            GetNewDirection(dop_h, dop_w);
            //ЕСЛИ ВНИЗ ИЛИ ВПРАВО, уменьшим границы МАКС на величину шара
            Point _Max_target = new Point(Max_target.X, Max_target.Y);
            double kat;
            double max_angle;
            if (cur_dir_w == Direction_w.Right)
                _Max_target.X -= l.Width;
            if (cur_dir_h == Direction_h.Down)
                _Max_target.Y -= l.Height;

            if (cur_dir_w == Direction_w.Left)
            {
                kat = Left;
                cur_target.X = 0;
            }
            else
            {
                kat = _Max_target.X - Left; //+ l.Width / 2;
                cur_target.X = _Max_target.X;
            }

            if (dop_h == Direction_h.None || dop_w == Direction_w.None)
            {
                curr_angle = Dopnik.r_path.Next(0, 89) + (Dopnik.r_path.Next(0, 99) / 100.0);
            }
            else
            {
                //int pause = 0;
                owner.Children.Remove(tar);
            }
            if (cur_dir_h == Direction_h.Up)
                max_angle = Math.Atan(Top / kat) / Math.PI * 180;
            else
                max_angle = Math.Atan((_Max_target.Y - Top) /*- l.Height)*/ / kat) / Math.PI * 180;

            if (max_angle < 0) max_angle = 0;


            // double alfa = Dopnik.r_path.Next(0, 89) + (Dopnik.r_path.Next(0, 99) / 100.0); //рандомим угол
            double tar_kat = kat * Math.Tan(curr_angle * Math.PI / 180);
            if (curr_angle < max_angle)
            {
                //первый спопсоб для простого

                //таргет_катет = катет * tg(угла)
                //катет = если влево, то координата от левой стороны, если вправо двигаемся то от правой стороны
                //точка по х = если влево то мин, если вправо то макс
                //точка по y = если вверх, то длина от у0 до координаты - таргет катет
                //              если вниз, то длина от ymax до координаты - таргет катет
                if (cur_dir_h == Direction_h.Up)
                    cur_target.Y = Top - tar_kat;
                else
                    cur_target.Y = Top + tar_kat;
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
                if (cur_dir_h == Direction_h.Up)
                {
                    tar_kat = (tar_kat - Top) / Math.Tan(curr_angle * Math.PI / 180);
                    cur_target.Y = 0;
                }
                else
                {
                    tar_kat = Math.Abs(Top + tar_kat - _Max_target.Y) / Math.Tan(curr_angle * Math.PI / 180);
                    cur_target.Y = _Max_target.Y;
                }
                if (cur_dir_w == Direction_w.Left)
                {
                    cur_target.X = 0 + tar_kat;
                }
                else
                {
                    cur_target.X = _Max_target.X - tar_kat;
                }
            }

            double path = Math.Sqrt(Math.Pow(Left + l.Width / 2 - cur_target.X, 2) + Math.Pow(Top + l.Height / 2 - cur_target.Y, 2));

            Canvas.SetLeft(tar, cur_target.X);
            Canvas.SetTop(tar, cur_target.Y);
            owner.Children.Add(tar);
            double speed = 300;
            double time = path / speed;

            MoveTo(time);

        }
    }


}
