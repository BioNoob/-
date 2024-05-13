using System;
using System.Collections.Generic;
using System.Linq;

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
            //Point buf = new Point(Start);
            //Start.X -= 1;
            //End = new Point(buf);
            //End.X += 1;
            //y - y0 = k(x-x0)
            Angle = angl;
            //B = -1 * K * Start.X + Start.Y;
            //B = Start.Y;//Dopnik.MaxTarget.Y;
            //попробовать сместить центр на точку в которой стоим. тогда B будет 0 
            B = 0;
            Line buf = new Line(new Point(0, 0), null);

            Line _left = new Line(new Point(0 - Dopnik.MaxTarget.X / 2 + Start.X, Dopnik.MaxTarget.Y / 2 + Start.Y), 
                new Point(0 - Dopnik.MaxTarget.X / 2 + Start.X, 0 - Dopnik.MaxTarget.Y / 2 + Start.Y));


            Line _top = new Line(new Point(0 - Dopnik.MaxTarget.X / 2 + Start.X, Dopnik.MaxTarget.Y / 2 + Start.Y),
                new Point(Dopnik.MaxTarget.X / 2 + Start.X, Dopnik.MaxTarget.Y / 2 + Start.Y));
            Line _bot = new Line(new Point(0 - Dopnik.MaxTarget.X / 2 + Start.X, 0 - Dopnik.MaxTarget.Y / 2 + Start.Y),
                new Point(Dopnik.MaxTarget.X / 2 + Start.X, 0 - Dopnik.MaxTarget.Y / 2 + Start.Y));
            Line _right = new Line(new Point(Dopnik.MaxTarget.X / 2 + Start.X, Dopnik.MaxTarget.Y / 2 + Start.Y),
                new Point(Dopnik.MaxTarget.X / 2 + Start.X, 0 - Dopnik.MaxTarget.Y / 2 + Start.Y));

            buf.intersect(_left, out Point crossLef);
            buf.intersect(_top, out Point crossTop);
            buf.intersect(_right, out Point crossRig);
            buf.intersect(_bot, out Point crossBot);
            List<Point> lstl = new List<Point>() { crossBot, crossTop, crossLef, crossRig }.Where(t => t != null).ToList();
            if (lstl.Count == 2)
            {
                switch (_H)
                {
                    case Direction_h.Up:
                        if (K > 0)
                            End = crossTop == null ? new Point(crossRig) : new Point(crossTop);
                        else
                            End = crossBot == null ? new Point(crossLef) : new Point(crossBot);
                        break;
                    case Direction_h.Down:
                        if (K < 0)
                            End = crossTop == null ? new Point(crossLef) : new Point(crossTop);
                        else
                            End = crossBot == null ? new Point(crossRig) : new Point(crossBot);
                        break;
                }
                GetCanon();
            }
            else
                throw new Exception();

        }
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
        public double UngleOfLines(Line ln2)
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
