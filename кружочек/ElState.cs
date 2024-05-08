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

        private static Point GetVector(Point cur1, Point tar1)
        {
            return new Point(tar1.X - cur1.X, tar1.Y - cur1.Y);
        }
        private static double Scal_mult_of_vec(Point vec1, Point vec2)
        {
            return vec1.X * vec2.X + vec1.Y * vec2.Y;
        }
        private static double Module_of_vec(Point vec)
        {
            return Math.Sqrt(Math.Pow(vec.X, 2) + Math.Pow(vec.Y, 2));
        }
        public static double GetAngle(Point cur, Point tar)
        {
            double scal = Scal_mult_of_vec(cur, tar);
            double mod1 = Module_of_vec(cur);
            double mod2 = Module_of_vec(tar);
            var x = scal / (mod1 * mod2);
            if (Double.IsNaN(x))
                x = 0;
            if (x > 1d)
                x = 0.99;
            else if (x < -1d)
                x = -0.99;
            return Math.Acos(x) / Math.PI * 180;

            //// Получим косинус угла по формуле
            //double cos = (cur.X * tar.X + cur.Y * tar.Y)
            //        / (Math.Sqrt(Math.Pow(cur.X, 2) + Math.Pow(cur.Y, 2))
            //        * Math.Sqrt(Math.Pow(tar.X, 2) + Math.Pow(tar.Y, 2)));
            //// Вернем arccos полученного значения (в радианах!)
            //return Math.Acos(cos) * 180 / Math.PI;
        }
        public static double UngleOfVectors(Point cur1, Point tar1, Point cur2, Point tar2)
        {
            Point vec1 = GetVector(cur1, tar1);
            Point vec2 = GetVector(cur2, tar2);
            double scal = Scal_mult_of_vec(vec1, vec2);
            double mod1 = Module_of_vec(vec1);
            double mod2 = Module_of_vec(vec2);
            var x = scal / (mod1 * mod2);
            if (Double.IsNaN(x))
                x = 0;
            if (x > 1d)
                x = 0.99;
            else if (x < -1d)
                x = -0.99;
            return Math.Acos(x) / Math.PI * 180;

        }

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
            anim1.FillBehavior = FillBehavior.HoldEnd;
            anim2.FillBehavior = FillBehavior.HoldEnd;
            need_ani = need_anim;
            owner = par;
            tar = new Ellipse() { Width = 15, Height = 15 };
            tar.Stroke = new SolidColorBrush(Colors.Black);
            tar.Fill = this.l.Fill;
            tar_last = new Ellipse() { Width = 15, Height = 15 };
            tar_last.Stroke = new SolidColorBrush(Colors.Black);
            tar_last.Fill = this.l.Fill;
            tar_last.Opacity = 0.5;
            if (need_anim)
            {
                CalcNewPointsToMove();
            }
        }
        private Canvas owner;
        private Ellipse tar;
        private Ellipse tar_last;
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
            if (Left == Cur_target.X || Top == Cur_target.Y)
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
        public Point Cur_target { get => cur_target; set => cur_target = value; }

        private Point cur_target;

        public void MoveTo(double time_anim)
        {
            //var q = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            anim1.From = Top;
            anim1.BeginTime = TimeSpan.FromMilliseconds(0);
            anim1.To = Cur_target.Y;
            anim1.Duration = TimeSpan.FromSeconds(time_anim);
            anim2.From = Left;
            anim2.To = Cur_target.X;
            anim2.BeginTime = TimeSpan.FromMilliseconds(0);
            anim2.Duration = TimeSpan.FromSeconds(time_anim);
            l.BeginAnimation(Canvas.LeftProperty, anim2);///, HandoffBehavior.SnapshotAndReplace);
            l.BeginAnimation(Canvas.TopProperty, anim1);//, HandoffBehavior.SnapshotAndReplace);
        }
        private void GetNewDirection(double? angle_on_target = null)//Direction_h dop_h = Direction_h.None, Direction_w dop_w = Direction_w.None)
        {
            #region help
            //if (dop_h != Direction_h.None && dop_w != Direction_w.None)
            //{
            //    switch (cur_dir_h)
            //    {
            //        case Direction_h.Up:
            //            switch (cur_dir_w)
            //            {
            //                case Direction_w.Left:
            //                    if (dop_h == Direction_h.Up)
            //                    {
            //                        if (dop_w == Direction_w.Left)
            //                        {
            //                            //dop_w = Direction_w.Right;
            //                            cur_dir_h = Direction_h.Down;
            //                        }
            //                        else if (dop_w == Direction_w.Right)
            //                        {
            //                            //dop_w = Direction_w.Left;
            //                            cur_dir_w = Direction_w.Right;
            //                        }
            //                    }
            //                    else if (dop_h == Direction_h.Down)
            //                    {
            //                        if (dop_w == Direction_w.Left)
            //                        {
            //                            //dop_h = Direction_h.Up;
            //                            cur_dir_h = Direction_h.Down;
            //                        }
            //                        else if (dop_w == Direction_w.Right)
            //                        {
            //                            //dop_h = Direction_h.Up;
            //                            cur_dir_h = Direction_h.Down;
            //                        }
            //                    }
            //                    break;
            //                case Direction_w.Right:
            //                    if (dop_h == Direction_h.Up)
            //                    {
            //                        if (dop_w == Direction_w.Left)
            //                        {
            //                            // dop_w = Direction_w.Right;
            //                            cur_dir_w = Direction_w.Left;
            //                        }
            //                        else if (dop_w == Direction_w.Right)
            //                        {
            //                            //dop_w = Direction_w.Left;
            //                            cur_dir_h = Direction_h.Down;
            //                        }
            //                    }
            //                    else if (dop_h == Direction_h.Down)
            //                    {
            //                        if (dop_w == Direction_w.Left)
            //                        {
            //                            //dop_h = Direction_h.Up;
            //                            cur_dir_h = Direction_h.Down;
            //                        }
            //                        else if (dop_w == Direction_w.Right)
            //                        {
            //                            //dop_h = Direction_h.Up;
            //                            cur_dir_h = Direction_h.Down;
            //                        }
            //                    }
            //                    break;
            //            }
            //            break;
            //        case Direction_h.Down:
            //            switch (cur_dir_w)
            //            {
            //                case Direction_w.Left:
            //                    if (dop_h == Direction_h.Up)
            //                    {
            //                        if (dop_w == Direction_w.Left)
            //                        {
            //                            // dop_h = Direction_h.Down;
            //                            cur_dir_h = Direction_h.Up;
            //                        }
            //                        else if (dop_w == Direction_w.Right)
            //                        {
            //                            // dop_w = Direction_w.Left;
            //                            cur_dir_w = Direction_w.Right;
            //                        }
            //                    }
            //                    else if (dop_h == Direction_h.Down)
            //                    {
            //                        if (dop_w == Direction_w.Left)
            //                        {
            //                            //dop_h = Direction_h.Up;
            //                            cur_dir_w = Direction_w.Right;
            //                        }
            //                        else if (dop_w == Direction_w.Right)
            //                        {
            //                            //dop_w = Direction_w.Left;
            //                            cur_dir_w = Direction_w.Right;
            //                        }
            //                    }
            //                    break;
            //                case Direction_w.Right:
            //                    if (dop_h == Direction_h.Up)
            //                    {
            //                        if (dop_w == Direction_w.Left)
            //                        {
            //                            //dop_w = Direction_w.Right;
            //                            cur_dir_h = Direction_h.Up;
            //                        }
            //                        else if (dop_w == Direction_w.Right)
            //                        {
            //                            //dop_h = Direction_h.Down;
            //                            cur_dir_h = Direction_h.Up;
            //                        }
            //                    }
            //                    else if (dop_h == Direction_h.Down)
            //                    {
            //                        if (dop_w == Direction_w.Left)
            //                        {
            //                            //dop_w = Direction_w.Right;
            //                            //cur_dir_w = Direction_w.Left;
            //                            cur_dir_h = Direction_h.Up;
            //                        }
            //                        else if (dop_w == Direction_w.Right)
            //                        {
            //                            //dop_h = Direction_h.Up;
            //                            //cur_dir_w = Direction_w.Left;
            //                            cur_dir_h = Direction_h.Up;
            //                        }
            //                    }
            //                    break;
            //            }
            //            break;
            //    }
            //}
            #endregion
            if (angle_on_target != null)
            {
                double ang = (double)angle_on_target;

                if (curr_angle >= ang)
                {
                    // 1 двигаюсь вниз вправо
                    // 2 двигаюсь вниз влево
                    // 3 двигаюсь вверх влево
                    // 4 двигаюсь вверх вправо
                    //1 ушел вниз влево
                    //2 ушел вниз вправо
                    //3 ушел вниз вправо
                    //4 ушел вверх влево
                    switch (Cur_dir_h)
                    {
                        case Direction_h.Up:
                            switch (Cur_dir_w)
                            {
                                case Direction_w.Left:
                                    //3
                                    Cur_dir_h = Direction_h.Down;
                                    Cur_dir_w = Direction_w.Right;
                                    break;
                                case Direction_w.Right:
                                    //4
                                    Cur_dir_w = Direction_w.Left;
                                    break;
                            }
                            break;
                        case Direction_h.Down:
                            switch (Cur_dir_w)
                            {
                                case Direction_w.Left:
                                    //2
                                    Cur_dir_w = Direction_w.Right;
                                    break;
                                case Direction_w.Right:
                                    //1
                                    Cur_dir_w = Direction_w.Left;
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    // 1 двигаюсь вниз вправо
                    // 2 двигаюсь вниз влево
                    // 3 двигаюсь вверх влево
                    // 4 двигаюсь вверх вправо
                    //1 ушел вверх вправо
                    //2 ушел вверх влево
                    //3 ушел вниз влево
                    //4 ушул вниз вправо
                    switch (Cur_dir_h)
                    {
                        case Direction_h.Up:
                            switch (Cur_dir_w)
                            {
                                case Direction_w.Left:
                                case Direction_w.Right:
                                    //3
                                    //4
                                    Cur_dir_h = Direction_h.Down;
                                    break;
                            }
                            break;
                        case Direction_h.Down:
                            switch (Cur_dir_w)
                            {
                                case Direction_w.Left:
                                case Direction_w.Right:
                                    //2
                                    //1
                                    Cur_dir_h = Direction_h.Up;
                                    break;
                            }
                            break;
                    }
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
                            if (Cur_target.X <= l.Width) //уперлись в границу слева
                                cur_dir_w = Direction_w.Right;
                            break;
                        case Direction_w.Right:
                            if (Cur_target.X >= Max_target.X - l.Width) //уперлись в границу справа
                                cur_dir_w = Direction_w.Left;
                            break;
                    }
                if (cur_dir_h == Direction_h.None)
                    cur_dir_h = (Direction_h)Dopnik.r_path.Next(0, 1);
                else
                    switch (cur_dir_h)
                    {
                        case Direction_h.Up:
                            if (Cur_target.Y <= l.Height) //уперлись в границу сверху
                                cur_dir_h = Direction_h.Down;
                            break;
                        case Direction_h.Down:
                            if (Cur_target.Y >= Max_target.Y - l.Height) //уперлись в границу снизу
                                cur_dir_h = Direction_h.Up;
                            break;
                    }
                //тут можно поменять для стационарного объекта, что он типо стена

            }
        }
        double curr_angle = 0.0;
        public void CalcNewPointsToMove(ElState el1 = null) //Direction_h dop_h = Direction_h.None, Direction_w dop_w = Direction_w.None)
        {
            if (!need_ani) return;
            double? angle_on_target = null;
            double last_angle = curr_angle;
            Direction_h last_dir_h = Cur_dir_h;
            Direction_w last_dir_w = Cur_dir_w;
            double popravka_alfa = 0d;
            //ТЕСТИРУЕМ ПОЛУЧЕНИЯ УРОВНЕНИЯ ПРЯМОЙ
            if (el1 != null)
            {
                var q = Dopnik.UngleOfVectors(new Point(Left, Top), Cur_target, new Point(el1.Left, el1.Top), el1.Cur_target);
                //считаем угол между векторами и получаем поправку к углу отражения
                angle_on_target = Dopnik.GetAngle(Center, el1.Center);
                //Определить кто является основным объектом (целится в другой объект)
                //теоретически так должно быть всегда, но есть ситуации когда один догоняет другой в жопу.
                //и тогда того, кого догнали направление не меняет
                //Debug.WriteLine($"{angle_on_target}");
                //// 1 сверху слева двигаюсь вниз вправо
                //// 2 сверху справа двигаюсь вниз влево
                //// 3 снизу справа двигаюсь вверх влево
                //// 4 снизу слева двигаюсь вверх вправо
                //if(curr_angle >= angle_on_target)
                //{
                //    //1 ушел вниз влево
                //    //2 ушел вниз вправо
                //    //3 ушел вниз вправо
                //    //4 ушел вверх влево
                //}
                //else
                //{
                //    //1 ушел вверх вправо
                //    //2 ушел вверх влево
                //    //3 ушел вверх влево
                //    //4 ушул вниз вправо
                //}

                popravka_alfa = q - curr_angle;
                curr_angle += popravka_alfa;
                if (curr_angle > 90)
                    curr_angle -= 90;
            }
            //МЕНЯЕМ НАПРАВЛЕНИЕ ЕСЛИ УПЕРЛИСЬ В ГРАНИЦУ
            GetNewDirection(angle_on_target);
            //ЕСЛИ ВНИЗ ИЛИ ВПРАВО, уменьшим границы МАКС на величину шара
            Point _Max_target = new Point(Max_target.X, Max_target.Y);
            double kat;
            double max_angle;
            if (cur_dir_w == Direction_w.Right)
                _Max_target.X -= l.Width;
            if (cur_dir_h == Direction_h.Down)
                _Max_target.Y -= l.Height;
            var _Cur_target = Cur_target;
            if (cur_dir_w == Direction_w.Left)
            {
                kat = Left;
                _Cur_target.X = 0;
            }
            else
            {
                kat = _Max_target.X - Left; //+ l.Width / 2;
                _Cur_target.X = _Max_target.X;
            }

            if (el1 == null)
                curr_angle = Dopnik.r_path.Next(0, 89) + (Dopnik.r_path.Next(0, 99) / 100.0);
            else
            {
                //owner.Children.Remove(tar);
                //Canvas.SetLeft(tar_last, Canvas.GetLeft(tar));
                //Canvas.SetTop(tar_last, Canvas.GetTop(tar));
            }

            //if (dop_h == Direction_h.None || dop_w == Direction_w.None)
            //{
            //    curr_angle = Dopnik.r_path.Next(0, 89) + (Dopnik.r_path.Next(0, 99) / 100.0);
            //}
            //else
            //{
            //    //int pause = 0;
            //    owner.Children.Remove(tar);
            //}
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
                    _Cur_target.Y = Top - tar_kat;
                else
                    _Cur_target.Y = Top + tar_kat;
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
                if (Cur_dir_h == Direction_h.Up)
                {
                    tar_kat = (tar_kat - Top) / Math.Tan(curr_angle * Math.PI / 180);
                    _Cur_target.Y = 0;
                }
                else
                {
                    tar_kat = Math.Abs(Top + tar_kat - _Max_target.Y) / Math.Tan(curr_angle * Math.PI / 180);
                    _Cur_target.Y = _Max_target.Y;
                }
                if (Cur_dir_w == Direction_w.Left)
                {
                    _Cur_target.X = 0 + tar_kat;
                }
                else
                {
                    _Cur_target.X = _Max_target.X - tar_kat;
                }
            }
            Cur_target = _Cur_target;
            double path = Math.Sqrt(Math.Pow(Left + l.Width / 2 - _Cur_target.X, 2) + Math.Pow(Top + l.Height / 2 - _Cur_target.Y, 2));
            double speed = 100;
            time_ = path / speed;
            Canvas.SetLeft(tar_last, Canvas.GetLeft(tar));
            Canvas.SetTop(tar_last, Canvas.GetTop(tar));
            Canvas.SetLeft(tar, _Cur_target.X);
            Canvas.SetTop(tar, _Cur_target.Y);

            if (!owner.Children.Contains(tar))
                owner.Children.Add(tar);
            if (!owner.Children.Contains(tar_last))
                owner.Children.Add(tar_last);
            if (el1 != null)
            {
                Debug.WriteLine($"EL {Id} LastDirW = {Enum.GetName(typeof(Direction_w), last_dir_w)} LastDirH = {Enum.GetName(typeof(Direction_h), last_dir_h)}" +
                    $" HCurDirW = {Enum.GetName(typeof(Direction_w), cur_dir_w)} CurDirH = {Enum.GetName(typeof(Direction_h), cur_dir_h)},\n" +
                    $" AngleBefore = {String.Format("{0:0.000}",last_angle)}, AngleCorrect = {String.Format("{0:0.000}", curr_angle)} " +
                    $"AngToObj = {String.Format("{0:0.000}", angle_on_target)}, Popravka = {String.Format("{0:0.000}", popravka_alfa)}");
                anim1.BeginTime = null;
                l.BeginAnimation(Canvas.TopProperty, anim1);
                anim2.BeginTime = null;
                l.BeginAnimation(Canvas.LeftProperty, anim2);
            }
            else
                MoveTo(time_);
            //БАГИ
            //1. недостаточный угол корректировки (или слишком большой)


        }
        public double time_ = 0;
    }


}
