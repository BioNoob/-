using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public const double EPS = 1e-9;
        public const double Speed = 100;
        public static Point MaxTarget { get; set; } = new Point();
        public static Point EllipseSize { get; set; }
        public static Point GetVector(Point cur1, Point tar1)
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
        public static double UngleOfVectors(Point cur1, Point tar1, Point cur2, Point tar2)
        {
            Point vec1 = GetVector(cur1, tar1);
            Point vec2 = GetVector(cur2, tar2);
            double scal = Scal_mult_of_vec(vec1, vec2);
            double mod1 = Module_of_vec(vec1);
            double mod2 = Module_of_vec(vec2);
            var x = scal / (mod1 * mod2);
            if (double.IsNaN(x))
                x = 0;
            if (x > 1d)
                x = 0.99;
            else if (x < -1d)
                x = -0.99;
            return Math.Acos(x) / Math.PI * 180;

        }
        //получаем уравнение прямой
        public static void GetCanon(Point start, Point end, out double k, out double b)
        {
            /* x-xa / xb-xa = y - ya / yb - ya*/
            double ax = end.X - start.X;
            double ay = end.Y - start.Y;
            k = ay / ax;
            b = -1 * (((ay * start.X) - (start.Y * ax)) / ax);
        }

        public static double det(double a, double b, double c, double d)
        {
            return a * d - b * c;
        }
        //точка пересечения двух прямых заданных уравнением
        //можно использовать для нахождения пересечения, а не через метод прямоугольников
        public static bool intersect(double k1, double b1, double k2, double b2, out Point res)
        {
            double bb1 = -1 * b1;
            double bb2 = -1 * b2;
            double zn = det(k1, 1, k2, 1);
            if (Math.Abs(zn) < EPS)
            {
                res = null;
                return false;
            }
            res = new Point();
            res.X = -det(b1, 1, b2, 1) / zn;
            res.Y = -det(k1, bb1, k2, bb2) / zn;
            return true;
        }


        public static void GetNewLine(Line ln, Line orto)
        {
            double angl = ln.AngleOfLines(orto);
            double acute, obtuse = 0; //острый и тупой угол
            if (angl > 90)
            {
                obtuse = angl;
                acute = 180d - angl;
            }
            else
            {
                obtuse = 180d - angl;
                acute = angl;
            }
            double answacute = acute * 2; //новый угол движения от пересечения
            double answobtuse = obtuse - acute;
            //теперь нужно определить в какую сторону ехать. в тупую или в острую
            //и в какую четверть мы приедем
            Line nw = ln.Copy();
            nw.Angle = answacute; //?
            //intersect(nw.K, nw.B, kLeft, bLeft, out Point crossLef);
            //intersect(nw.K, nw.B, kTop, bTop, out Point crossTop);
            //intersect(nw.K, nw.B, kRight, bRight, out Point crossRig);
            //intersect(nw.K, nw.B, kBottom, bBottom, out Point crossBot);
            ////по идее будет одна точка, хотя хуй его знает
            //List<Point> lstl = new List<Point>() { crossBot, crossTop, crossLef, crossRig }.Where(t => t != null).ToList();
        }

        /// <summary>
        /// Вернуть вектор для линии границы
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Line GetOrtoLineFromBorder(Point pt)//double angle_from, Point st1, Point end1, Point st2, Point end2)
        {
            if (pt.X == 0)
                return new Line(pt, new Point(MaxTarget.X, pt.Y));
            else if (pt.X == MaxTarget.X)
                return new Line(pt, new Point(0, pt.Y));
            else if (pt.Y == 0)
                return new Line(pt, new Point(pt.X, MaxTarget.Y));
            else if (pt.Y == MaxTarget.Y)
                return new Line(pt, new Point(pt.X, 0));
            else
                return null;
        }

        //    /*
        //     * 1. получили уравнение прямой GetCanon
        //     * 2. получили уравнение прямой GetCanon
        //     * 3. нашли точку пересечения intersect
        //     * 4. строим прямую от точки пересчения к границе (как обычный вектор) (в нужном направлении...) ага.. и под нужным углом
        //     * 5. ищем угол между прямой 1. и 4. угол + угол = новый угол движения для шарика после отражения
        //     * 6. ищем угол между прямой 2. и 4. = новый угол движения для шарика после отражения, но с учетом того, что одно из двух направлений будет занято первым шаром (5.)
        //     * 7. строим две прямые новые для движения
        //     */
        //    //КСТА k=tg(alfa)
        //    //если k > 0 то вверх едем
        //    //если k < 0 то вниз
        //    //alfa = atan(k);
        //    //знач направление определим тоже по точкам, но вручную сравнением
        //    //1
        //    //GetCanon(st1, end1, out double k1, out double b1);
        //    //2
        //    //GetCanon(st2, end2, out double k2, out double b2);
        //    //3
        //    Point answ = new Point();
        //    Line orto = null;
        //    if (intersect(ln1.K, ln1.B, ln2.K, ln2.B, out answ))
        //    {
        //        //пересечение
        //        //Будем искать точки пересечения 
        //        //y=kx+b знаем y с точки, знаем х с точки, знаем К через тангенс
        //        //-b=-y+kx
        //        //b=y-kx
        //        double _nk = Math.Tan((90 - ln1.Angle) * Math.PI / 180);
        //        double _nb = answ.Y - _nk * answ.X;
        //        //получим две точки пересчения минимум. Выяснить какие (можно через провреку по К но лень)
        //        intersect(_nk, _nb, kLeft, bLeft, out Point crossLef);
        //        intersect(_nk, _nb, kTop, bTop, out Point crossTop);
        //        intersect(_nk, _nb, kRight, bRight, out Point crossRig);
        //        intersect(_nk, _nb, kBottom, bBottom, out Point crossBot);
        //        List<Point> ln = new List<Point>() { crossBot, crossTop, crossLef, crossRig }.Where(t => t != null).ToList();
        //        if (ln.Count == 2)
        //        {
        //            orto = new Line(ln[0], ln[1]);
        //        }
        //        else
        //        {
        //            //error больше чем две точки или меньше
        //            throw new Exception();
        //        }

        //    }
        //    else
        //    {
        //        //нет пересечения
        //        throw new Exception();
        //    }
        //    return orto;
        //}

        //public static Line Top;
        //public static Line Left;
        //public static Line Bottom;
        //public static Line Right;
        public static void ReInitStatic_bySize(Point MaxCoord)
        {
            MaxTarget.X = MaxCoord.X - EllipseSize.X;
            MaxTarget.Y = MaxCoord.Y - EllipseSize.Y;

            //Top = new Line(new Point(0, 0), new Point(MaxTarget.X, 0));
            //Left = new Line(new Point(0, 0), new Point(0, MaxTarget.Y));
            //Right = new Line(new Point(MaxTarget.X, 0), new Point(MaxTarget.X, MaxTarget.Y));
            //Bottom = new Line(new Point(0, MaxTarget.Y), new Point(MaxTarget.X, MaxTarget.Y));
        }
        public static void InitStatic(Point MaxCoord, double ElWidth, double ElHigth)
        {
            EllipseSize = new Point(ElWidth, ElHigth);
            ReInitStatic_bySize(MaxCoord);

        }
        public static Point CreateEndPoint(Direction_h _H, Direction_w _W, Point stPoint, double angl)
        {
            //GetNewDirection(angle_on_target);
            double kat;
            double max_angle;
            //ВОПРОС ВЫБОРА НАПРАВЛЕНИЯ для вектора..
            //if (_W == Direction_w.Right)
            //    _Max_target.X -= l.Width;
            //if (_H == Direction_h.Down)
            //    _Max_target.Y -= l.Height;
            double Left = stPoint.X;
            double Top = stPoint.Y;
            Point Cur_target = new Point();
            if (_W == Direction_w.Left)
            {
                kat = stPoint.X;
                Cur_target.X = 0;
            }
            else
            {
                kat = MaxTarget.X - Left; //+ l.Width / 2;
                Cur_target.X = MaxTarget.X;
            }
            if (_H == Direction_h.Up)
                max_angle = Math.Atan(Top / kat) / Math.PI * 180;
            else
                max_angle = Math.Atan((MaxTarget.Y - Top) /*- l.Height)*/ / kat) / Math.PI * 180;

            if (max_angle < 0) max_angle = 0;
            double tar_kat = kat * Math.Tan(angl * Math.PI / 180);
            if (angl < max_angle)
            {
                if (_H == Direction_h.Up)
                    Cur_target.Y = Top - tar_kat;
                else
                    Cur_target.Y = Top + tar_kat;
            }
            else
            {
                if (_H == Direction_h.Up)
                {
                    tar_kat = (tar_kat - Top) / Math.Tan(angl * Math.PI / 180);
                    Cur_target.Y = 0;
                }
                else
                {
                    tar_kat = Math.Abs(Top + tar_kat - MaxTarget.Y) / Math.Tan(angl * Math.PI / 180);
                    Cur_target.Y = MaxTarget.Y;
                }
                if (_W == Direction_w.Left)
                {
                    Cur_target.X = 0 + tar_kat;
                }
                else
                {
                    Cur_target.X = MaxTarget.X - tar_kat;
                }
            }
            return Cur_target;
        }

    }
    public class ElState
    {
        private DoubleAnimation anim2;
        private DoubleAnimation anim1;
        private Line curLine;
        private Line ortLine;
        //private double curr_angle = 0.0;
        private Canvas owner;
        private Ellipse tar;
        private Ellipse tar_last;
        private int _id;
        //private Direction_w cur_dir_w = Direction_w.None;
        private Direction_h cur_dir_h = Direction_h.None;
        private bool need_ani = true;
        //private Point Max_target = new Point();
        private Ellipse l { get; set; } = null;

        public double time_ { get; set; }
        public double Left => Canvas.GetLeft(l);
        public double Top => Canvas.GetTop(l);
        public Point Center => new Point(Left / 2, Top / 2);
        public double Width => l.Width;
        public double Height => l.Height;
        public int Id => _id;
       // public Direction_w Cur_dir_w { get => cur_dir_w; set => cur_dir_w = value; }
        public Direction_h Cur_dir_h { get => cur_dir_h; set => cur_dir_h = value; }

        public Line CurLine { get => curLine; set => curLine = value; }

        //public Point Cur_target { get => cur_target; set => cur_target = value; }
        //public Point Last_target
        //{
        //    get
        //    {
        //        if (last_target is null)
        //        {
        //            last_target = cur_target;
        //        }
        //        return last_target;
        //    }
        //    set => last_target = value;
        //}
        public bool Use_old_coord { get; set; } = false;
        public ElState(Ellipse el, Canvas par, bool need_anim = true)
        {
            _id = ++Dopnik.Id;
            l = el;
            l.LayoutUpdated += L_LayoutUpdated;
            anim1 = new DoubleAnimation();
            anim2 = new DoubleAnimation();
            anim1.FillBehavior = FillBehavior.HoldEnd;
            anim2.FillBehavior = FillBehavior.HoldEnd;
            need_ani = need_anim;
            owner = par;
            tar = new Ellipse() { Width = l.Width / 2, Height = l.Height / 2 };
            tar.Stroke = new SolidColorBrush(Colors.Black);
            tar.Fill = l.Fill;
            tar_last = new Ellipse() { Width = l.Width / 2, Height = l.Height / 2 };
            tar_last.Stroke = new SolidColorBrush(Colors.Black);
            tar_last.Fill = l.Fill;
            tar_last.Opacity = 0.5;
            CurLine = new Line(new Point(Left, Top), null);
            //Cur_target = new Point();
            //Last_target = new Point(Left, Top);
            if (need_anim)
            {
                CalcNewPointToMove();
            }
        }

        private void L_LayoutUpdated(object sender, EventArgs e)
        {
            if (Left == CurLine.End.X || Top == CurLine.End.Y)
            {
                //owner.Children.Remove(tar);
                CalcNextPointToMove();
            }
        }
        //public void Change_size_of_Target(Size s)
        //{
        //    Max_target.X = s.Width;
        //    Max_target.Y = s.Height;
        //}
        public void MoveTo(double time_anim)
        {
            //var q = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            anim1.From = Top;
            anim1.BeginTime = TimeSpan.FromMilliseconds(0);
            anim1.To = CurLine.End.Y;
            anim1.Duration = TimeSpan.FromSeconds(time_anim);
            anim2.From = Left;
            anim2.To = CurLine.End.X;
            anim2.BeginTime = TimeSpan.FromMilliseconds(0);
            anim2.Duration = TimeSpan.FromSeconds(time_anim);
            l.BeginAnimation(Canvas.LeftProperty, anim2);///, HandoffBehavior.SnapshotAndReplace);
            l.BeginAnimation(Canvas.TopProperty, anim1);//, HandoffBehavior.SnapshotAndReplace);
        }
        private void GetNewDirection()//double? angle_on_target = null)//Direction_h dop_h = Direction_h.None, Direction_w dop_w = Direction_w.None)
        {

            //if (angle_on_target != null)
            //{
            //    double ang = (double)angle_on_target;

            //    if (Curr_Angle >= ang)
            //    {
            //        // 1 двигаюсь вниз вправо
            //        // 2 двигаюсь вниз влево
            //        // 3 двигаюсь вверх влево
            //        // 4 двигаюсь вверх вправо
            //        //1 ушел вниз влево
            //        //2 ушел вниз вправо
            //        //3 ушел вниз вправо
            //        //4 ушел вверх влево
            //        switch (Cur_dir_h)
            //        {
            //            case Direction_h.Up:
            //                switch (Cur_dir_w)
            //                {
            //                    case Direction_w.Left:
            //                        //3
            //                        Cur_dir_h = Direction_h.Down;
            //                        Cur_dir_w = Direction_w.Right;
            //                        break;
            //                    case Direction_w.Right:
            //                        //4
            //                        Cur_dir_w = Direction_w.Left;
            //                        break;
            //                }
            //                break;
            //            case Direction_h.Down:
            //                switch (Cur_dir_w)
            //                {
            //                    case Direction_w.Left:
            //                        //2
            //                        Cur_dir_w = Direction_w.Right;
            //                        break;
            //                    case Direction_w.Right:
            //                        //1
            //                        Cur_dir_w = Direction_w.Left;
            //                        break;
            //                }
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        // 1 двигаюсь вниз вправо
            //        // 2 двигаюсь вниз влево
            //        // 3 двигаюсь вверх влево
            //        // 4 двигаюсь вверх вправо
            //        //1 ушел вверх вправо
            //        //2 ушел вверх влево
            //        //3 ушел вниз влево
            //        //4 ушул вниз вправо
            //        switch (Cur_dir_h)
            //        {
            //            case Direction_h.Up:
            //                switch (Cur_dir_w)
            //                {
            //                    case Direction_w.Left:
            //                    case Direction_w.Right:
            //                        //3
            //                        //4
            //                        Cur_dir_h = Direction_h.Down;
            //                        break;
            //                }
            //                break;
            //            case Direction_h.Down:
            //                switch (Cur_dir_w)
            //                {
            //                    case Direction_w.Left:
            //                    case Direction_w.Right:
            //                        //2
            //                        //1
            //                        Cur_dir_h = Direction_h.Up;
            //                        break;
            //                }
            //                break;
            //        }
            //    }
            //}
            //else
            {
                //if (cur_dir_w == Direction_w.None)
                //    cur_dir_w = (Direction_w)Dopnik.r_path.Next(0, 1);
                //else
                //    switch (cur_dir_w)
                //    {
                //        case Direction_w.Left:
                //            if (CurLine.End.X <= l.Width) //уперлись в границу слева
                //                cur_dir_w = Direction_w.Right;
                //            break;
                //        case Direction_w.Right:
                //            if (CurLine.End.X >= Dopnik.MaxTarget.X - l.Width) //уперлись в границу справа
                //                cur_dir_w = Direction_w.Left;
                //            break;
                //    }
                if (cur_dir_h == Direction_h.None)
                    cur_dir_h = (Direction_h)Dopnik.r_path.Next(0, 1);
                else
                    switch (cur_dir_h)
                    {
                        case Direction_h.Up:
                            if (CurLine.End.Y <= l.Height) //уперлись в границу сверху
                                cur_dir_h = Direction_h.Down;
                            break;
                        case Direction_h.Down:
                            if (CurLine.End.Y >= Dopnik.MaxTarget.Y - l.Height) //уперлись в границу снизу
                                cur_dir_h = Direction_h.Up;
                            break;
                    }
                //тут можно поменять для стационарного объекта, что он типо стена

            }
        }
        public void CalcNewPointToMove()
        {
            if (!need_ani) return;
            GetNewDirection();
            CurLine.CreateEndPoint(Dopnik.r_path.Next(0, 179) + (Dopnik.r_path.Next(0, 99) / 100.0), Cur_dir_h);
            MoveTo(CurLine.Time);
        }
        //если пересечение от границ то используем этот метод. Если пересечение с другим ветором то через метод из Line
        public void CalcNextPointToMove()
        {
            Line t = Dopnik.GetOrtoLineFromBorder(CurLine.End);
            double angl = CurLine.AngleOfLines(t);
            if (angl > 90)
                angl = 180 - angl;
            //angl *= 2;
            //test
            angl += 90;
            GetNewDirection();
            CurLine.Start = CurLine.End;
            CurLine.CreateEndPoint(angl, cur_dir_h);
            MoveTo(CurLine.Time);
        }
        //public void CalcNewPointsToMove(ElState el1 = null) //Direction_h dop_h = Direction_h.None, Direction_w dop_w = Direction_w.None)
        //{
        //    if (!need_ani) return;

        //    double? angle_on_target = null;
        //    //double last_angle = curr_angle;
        //    Last_Angle = Curr_Angle;
        //    Direction_h last_dir_h = Cur_dir_h;
        //    Direction_w last_dir_w = Cur_dir_w;
        //    double popravka_alfa = 0d;
        //    //ТЕСТИРУЕМ ПОЛУЧЕНИЯ УРОВНЕНИЯ ПРЯМОЙ
        //    if (el1 != null)
        //    {
        //        //расчет идет неверно! если это вторая точка в расчете, потому что считает от уже рассчитанной новой точки, а не от исходной
        //        Point p = null;
        //        if (el1.Use_old_coord)
        //            p = el1.Last_target;
        //        else
        //            p = el1.Cur_target;

        //        double angl_sec;//угол на перпендикуляр относительно которого считаем рекошет
        //        //Это значит что ел уже пересчитали
        //        if (el1.Use_old_coord)
        //        {
        //            angl_sec = el1.Last_Angle;
        //        }
        //        else
        //        {
        //            angl_sec = el1.Curr_Angle;
        //        }
        //        //double popr = Curr_Angle - angl_sec; //-11.9 = 35.2 - 47.1
        //        double ugol_padenia = 90d - Curr_Angle; //если в стенку 90 - 47.1 = 42.9 \\ 90-28.1 = 61.9
        //        double buf = ugol_padenia - angl_sec; //42.9 - 35.2 = 7.7 \\ 61.9-81.5= -19.6
        //        Curr_Angle += buf * 2; // 28.1 + -19.6*2 = 

        //        //ange = 47.1 + 7.7*2 = 62.5
        //        //Curr_Angle = ugol_padenia + popr;


        //        el1.Use_old_coord = false;



        //        var q = Dopnik.UngleOfVectors(new Point(Left, Top), Cur_target, new Point(el1.Left, el1.Top), p);
        //        angle_on_target = q;
        //        //popravka_alfa = q - Curr_Angle;
        //        //Curr_Angle += popravka_alfa;
        //        //if (Curr_Angle > 90)
        //        //    Curr_Angle -= 90;
        //    }
        //    //МЕНЯЕМ НАПРАВЛЕНИЕ ЕСЛИ УПЕРЛИСЬ В ГРАНИЦУ
        //    GetNewDirection(angle_on_target);
        //    //ЕСЛИ ВНИЗ ИЛИ ВПРАВО, уменьшим границы МАКС на величину шара
        //    double kat;
        //    double max_angle;

        //    //ДУМАЮ ТУТ НАДО ДЕЛАТЬ БЕЗ ДИРОВ ЭТИХ, А ЧИСТО НА ОБЩЕМ УГЛУ
        //    //искать четверть по градусам, брать катет в четверти
        //    //а потом угол докладывать обратно

        //    Point _Max_target = new Point(Max_target.X, Max_target.Y);
        //    if (cur_dir_w == Direction_w.Right)
        //        _Max_target.X -= l.Width;
        //    if (cur_dir_h == Direction_h.Down)
        //        _Max_target.Y -= l.Height;
        //    if (cur_dir_w == Direction_w.Left)
        //    {
        //        kat = Left;
        //        Cur_target.X = 0;
        //    }
        //    else
        //    {
        //        kat = _Max_target.X - Left; //+ l.Width / 2;
        //        Cur_target.X = _Max_target.X;
        //    }

        //    //если нету от кого считать, считаем рандомный угол
        //    if (el1 == null)
        //    {
        //        Curr_Angle = Dopnik.r_path.Next(0, 89) + (Dopnik.r_path.Next(0, 99) / 100.0);
        //    }


        //    if (cur_dir_h == Direction_h.Up)
        //        max_angle = Math.Atan(Top / kat) / Math.PI * 180;
        //    else
        //        max_angle = Math.Atan((_Max_target.Y - Top) /*- l.Height)*/ / kat) / Math.PI * 180;

        //    if (max_angle < 0) max_angle = 0;


        //    // double alfa = Dopnik.r_path.Next(0, 89) + (Dopnik.r_path.Next(0, 99) / 100.0); //рандомим угол
        //    double tar_kat = kat * Math.Tan(Curr_Angle * Math.PI / 180);
        //    if (Curr_Angle < max_angle)
        //    {
        //        //первый спопсоб для простого

        //        //таргет_катет = катет * tg(угла)
        //        //катет = если влево, то координата от левой стороны, если вправо двигаемся то от правой стороны
        //        //точка по х = если влево то мин, если вправо то макс
        //        //точка по y = если вверх, то длина от у0 до координаты - таргет катет
        //        //              если вниз, то длина от ymax до координаты - таргет катет
        //        if (cur_dir_h == Direction_h.Up)
        //            Cur_target.Y = Top - tar_kat;
        //        else
        //            Cur_target.Y = Top + tar_kat;
        //    }
        //    else
        //    {
        //        //втрой способ
        //        //таргет_катет_промежуток = катет * tg(угла)
        //        //катет = если влево, то координата от левой стороны, если вправо двигаемся то от правой стороны
        //        //ищем катет для игрека. угол2 = 90 - угол
        //        //(таргет катет промежуток - катет)*tg(угол2)
        //        //точка по х = если влево то мин + катет игрек, если вправо то макс - катет игрек
        //        //точка по y = если вверх, 0
        //        //              если вниз, макс
        //        if (Cur_dir_h == Direction_h.Up)
        //        {
        //            tar_kat = (tar_kat - Top) / Math.Tan(Curr_Angle * Math.PI / 180);
        //            Cur_target.Y = 0;
        //        }
        //        else
        //        {
        //            tar_kat = Math.Abs(Top + tar_kat - _Max_target.Y) / Math.Tan(Curr_Angle * Math.PI / 180);
        //            Cur_target.Y = _Max_target.Y;
        //        }
        //        if (Cur_dir_w == Direction_w.Left)
        //        {
        //            Cur_target.X = 0 + tar_kat;
        //        }
        //        else
        //        {
        //            Cur_target.X = _Max_target.X - tar_kat;
        //        }
        //    }

        //    //имеем угол и координаты. ищем угол (глобальный) перпендикуляра к этому катету.. лол он на удивление равен углу падения

        //    double path = Math.Sqrt(Math.Pow(Left + l.Width / 2 - Cur_target.X, 2) + Math.Pow(Top + l.Height / 2 - Cur_target.Y, 2));
        //    double speed = 100;
        //    time_ = path / speed;

        //    Canvas.SetLeft(tar_last, Canvas.GetLeft(tar));
        //    Canvas.SetTop(tar_last, Canvas.GetTop(tar));
        //    Canvas.SetLeft(tar, Cur_target.X);
        //    Canvas.SetTop(tar, Cur_target.Y);

        //    if (!owner.Children.Contains(tar))
        //        owner.Children.Add(tar);
        //    if (!owner.Children.Contains(tar_last))
        //        owner.Children.Add(tar_last);
        //    if (el1 != null)
        //    {
        //        Debug.WriteLine($"EL {Id} LastDirW = {Enum.GetName(typeof(Direction_w), last_dir_w)} LastDirH = {Enum.GetName(typeof(Direction_h), last_dir_h)}" +
        //            $" HCurDirW = {Enum.GetName(typeof(Direction_w), cur_dir_w)} CurDirH = {Enum.GetName(typeof(Direction_h), cur_dir_h)},\n" +
        //            $" AngleBefore = {String.Format("{0:0.000}", Last_Angle)}, AngleCorrect = {String.Format("{0:0.000}", Curr_Angle)} " +
        //            $"AngToObj = {String.Format("{0:0.000}", angle_on_target)}, Popravka = {String.Format("{0:0.000}", popravka_alfa)}\n" +
        //            $"LastL = {String.Format("{0:0.000}", Last_target.X)}, LastT = {String.Format("{0:0.000}", Last_target.Y)},\n" +
        //            $"CurL = {String.Format("{0:0.000}", Left)}, CurT = {String.Format("{0:0.000}", Top)},\n" +
        //            $"TarX = {String.Format("{0:0.000}", Cur_target.X)}, TarY = {String.Format("{0:0.000}", Cur_target.Y)},\n " +
        //            $"Max_angle = {String.Format("{0:0.000}", max_angle)}, Tar_Kat = {String.Format("{0:0.000}", tar_kat)}");
        //        anim1.BeginTime = null;
        //        l.BeginAnimation(Canvas.TopProperty, anim1);
        //        anim2.BeginTime = null;
        //        l.BeginAnimation(Canvas.LeftProperty, anim2);

        //    }
        //    else
        //        MoveTo(time_);
        //    Last_target = new Point(Cur_target);
        //    //БАГИ
        //    //1. недостаточный угол корректировки (или слишком большой)
        //}
    }


}
