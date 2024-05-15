using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace кружочек
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point(Point p)
        {
            X = p.X;
            Y = p.Y;
        }
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
        public Point()
        {
            X = 0;
            Y = 0;
        }
        public override string ToString()
        {
            return $"{X}:{Y}";
        }
    }
    public class Line
    {
        private Point vector = null;
        private double vec_module;
        private Point center = null;

        public Point Start { get; set; }
        public Point End { get; set; }
        public Point Center { get => center == null ? center = new Point((Start.X + End.X) / 2, (Start.Y + End.Y) / 2) : center; }
        public Point Vector => vector == null ? vector = GetVector() : vector;
        public double Vec_Module => vec_module == 0 ? vec_module = Module_of_vec() : vec_module;
        public double Length => Vec_Module;
        public double Time => Length / Dopnik.Speed;
        public double K { get; set; }
        public double B { get; set; }
        public double Angle
        {
            get => Math.Atan(K) / Math.PI * 180;
            set
            {
                K = Math.Tan(value * Math.PI / 180);
                if (center != null)
                {
                    var t = Center; //для запуска с get вычисления
                    B = t.Y - K * t.X;
                }
            }
        }
        Point GetVector()
        {
            return new Point(End.X - Start.X, End.Y - Start.Y);
        }
        void GetCanon()
        {
            /* x-xa / xb-xa = y - ya / yb - ya*/
            if (End == null || Start == null)
                return;
            double ax = End.X - Start.X;
            double ay = End.Y - Start.Y;
            K = ay / ax;
            B = -1 * (((ay * Start.X) - (Start.Y * ax)) / ax);
        }
        double Module_of_vec()
        {
            return Math.Sqrt(Math.Pow(Vector.X, 2) + Math.Pow(Vector.Y, 2));
        }

        public Line(Point st, Point end)
        {
            Start = st;
            End = end;
            if (End == null || Start == null)
                return;
            GetCanon();
        }
        //задавать угол от 0 до 180
        public void CreateEndPoint(double angl, Direction_h _H)
        {
            Angle = angl - 90;
            B = -1 * K * Start.X + Start.Y;
            //B = Start.Y;//Dopnik.MaxTarget.Y;
            //B = 0;
            //y=kx+b
            //top = y=minY
            //left = x=minX
            //bot = y=maxY
            //rg = x=maxX

            double xtop = (0 - B) / K; //при у=0
            double xbot = (Dopnik.MaxTarget.Y - B) / K; //при у=макс.у
            double yleft = 0 * K + B; //при х=0
            double yRight = Dopnik.MaxTarget.X * K + B; //при х=макс.х

            switch (_H)
            {
                case Direction_h.Up:
                    if (K > 0)
                        End = xtop > 0 && xtop < Dopnik.MaxTarget.X ? new Point(xtop, 0) : new Point(Dopnik.MaxTarget.X, yRight);
                    else
                        End = xtop > 0 && xtop < Dopnik.MaxTarget.X ? new Point(xtop, 0) : new Point(0, yleft);
                    break;
                case Direction_h.Down:
                    if (K > 0)
                        End = xbot > 0 && xbot < Dopnik.MaxTarget.X ? new Point(xbot, Dopnik.MaxTarget.Y) : new Point(0, yleft);
                    else
                        End = xbot > 0 && xbot < Dopnik.MaxTarget.X ? new Point(xbot, Dopnik.MaxTarget.Y) : new Point(Dopnik.MaxTarget.X, yRight);
                    break;
            }

        }
        //public Line GetOrtoLineFromIntersect(Line ln2)
        //{
            //Point answ = new Point();
            //Line orto = null;
            ////if(ln2 != null)
            ////{
            //    //если пересечение не с 
            ////}
            ////if (intersect(ln2, out answ))
            ////{
            ////пересечение
            ////Будем искать точки пересечения 
            ////y=kx+b знаем y с точки, знаем х с точки, знаем К через тангенс
            ////-b=-y+kx
            ////b=y-kx
            //double _nk = Math.Tan((90 - Angle) * Math.PI / 180);
            //double _nb = answ.Y - _nk * answ.X;


            ////получим две точки пересчения минимум. Выяснить какие (можно через провреку по К но лень)
            //intersect(_nk, _nb, kLeft, bLeft, out Point crossLef);
            //intersect(_nk, _nb, kTop, bTop, out Point crossTop);
            //intersect(_nk, _nb, kRight, bRight, out Point crossRig);
            //intersect(_nk, _nb, kBottom, bBottom, out Point crossBot);
            //List<Point> ln = new List<Point>() { crossBot, crossTop, crossLef, crossRig }.Where(t => t != null).ToList();
            //if (ln.Count == 2)
            //{
            //    orto = new Line(ln[0], ln[1]);
            //}
            //else
            //{
            //    //error больше чем две точки или меньше
            //    throw new Exception();
            //}

            ////}
            ////else
            ////{
            ////нет пересечения
            ////throw new Exception();
            ////}
            //return orto;
        //}
        public bool intersect(Line ln2, out Point res)
        {
            double bb1 = -1 * B;
            double bb2 = -1 * ln2.B;
            double zn = Dopnik.det(K, 1, ln2.K, 1);
            if (Math.Abs(zn) < Dopnik.EPS)
            {
                res = null;
                return false;
            }
            res = new Point();
            res.X = -Dopnik.det(B, 1, ln2.B, 1) / zn;
            res.Y = -Dopnik.det(K, bb1, ln2.K, bb2) / zn;
            return true;
        }
        public void SetNewPointAsStart(Point st)
        {
            Start = st;
            GetCanon();
        }
        public void SetNewPointAsEnd(Point end)
        {
            End = end;
            GetCanon();
        }
        public Line Copy()
        {
            return new Line(Start, End);
        }
        public double Scal_mult_of_vec(Point vec2)
        {
            return Vector.X * vec2.X + Vector.Y * vec2.Y;
        }
        public double AngleOfLines(Line ln2)
        {
            double scal = Scal_mult_of_vec(ln2.Vector);
            var x = scal / (Vec_Module * ln2.Vec_Module);
            if (double.IsNaN(x))
                x = 0;
            if (x > 1d)
                x = 0.99;
            else if (x < -1d)
                x = -0.99;
            return Math.Acos(x) / Math.PI * 180;
        }
    }


}
