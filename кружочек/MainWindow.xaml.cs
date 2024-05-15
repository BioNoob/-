using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Random r_color;
        public MainWindow()
        {
            InitializeComponent();
            r_color = new Random((int)DateTime.Now.Ticks);
            this.LayoutUpdated += MainWindow_LayoutUpdated;
        }
        //int frames = 0;
        //List<ElState> el_to_changed = new List<ElState>();
        List<KeyValuePair<int, int>> pares = new List<KeyValuePair<int, int>>();
        List<KeyValuePair<int, int>> to_del = new List<KeyValuePair<int, int>>();
        private void MainWindow_LayoutUpdated(object sender, EventArgs e)
        {
            /*
Сделать историю 
Кто с кем сталкивался (два айди)
Смотреть, внутри фрейма, если их позиция "отлипла" на величину больше поисковой
То удалять из истории
             */
            foreach (var el1 in lst)
            {
                foreach (var el2 in lst.Where(t => t.Id != el1.Id))
                {
                    if (pares.Contains(new KeyValuePair<int, int>(el1.Id, el2.Id)) || pares.Contains(new KeyValuePair<int, int>(el2.Id, el1.Id)))
                    {
                        continue;
                    }
                    Rect r1 = new Rect(el1.Left, el1.Top, el1.Width + 2, el1.Height + 2);
                    Rect r2 = new Rect(el2.Left, el2.Top, el2.Width + 2, el2.Height + 2);
                    if (r1.IntersectsWith(r2))
                    {
                        pares.Add(new KeyValuePair<int, int>(el1.Id, el2.Id));
                        //el2.CalcNewPointsToMove(el1);//, el1.Cur_dir_h, el1.Cur_dir_w);
                        el2.Use_old_coord = true;
                        //el1.CalcNewPointsToMove(el2);//, buf_h, buf_w);
                    }
                }
            }
            to_del.Clear();
            foreach (var item in pares)
            {
                var el1 = lst.Single(t => t.Id == item.Key);
                var el2 = lst.Single(t => t.Id == item.Value);
                Rect r1 = new Rect(el1.Left, el1.Top, el1.Width + 2, el1.Height + 2);
                Rect r2 = new Rect(el2.Left, el2.Top, el2.Width + 2, el2.Height + 2);
                if (!r1.IntersectsWith(r2))
                {
                    to_del.Add(item);
                }
            }
            to_del.ForEach(t => pares.Remove(t));


            //foreach (var item in lst.Where(t => !t.Resent_changed))
            //{
            //    var a = lst.Where(t => t.Id != item.Id && !t.Resent_changed).Where(t => Math.Abs(t.Center.X - item.Center.X) < 16 && Math.Abs(t.Center.Y - item.Center.Y) < 16);
            //    if (a.Any())
            //    {
            //        var xx = new List<ElState>();
            //        var z = a.First();
            //        xx.Add(item);
            //        xx.Add(z);
            //        pares.Add(xx);
            //        Direction_w buf_w = z.Cur_dir_w;
            //        Direction_h buf_h = z.Cur_dir_h;
            //        z.CalcNewPointsToMove(item.Cur_dir_h, item.Cur_dir_w);
            //        item.CalcNewPointsToMove(buf_h, buf_w);
            //        z.Resent_changed = true;
            //        item.Resent_changed = true;
            //    }
            //}

        }

        private void JopaCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse l = new Ellipse() { Width = Dopnik.EllipseSize.X, Height = Dopnik.EllipseSize.Y };
            l.Stroke = new SolidColorBrush(Colors.Black);
            l.Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(r_color.Next(0, 255)),
         Convert.ToByte(r_color.Next(0, 255)), Convert.ToByte(r_color.Next(0, 255))));
            var pos = e.GetPosition(JopaCanvas);
            Canvas.SetLeft(l, pos.X - l.Width / 2);
            Canvas.SetTop(l, pos.Y - l.Height / 2);
            JopaCanvas.Children.Add(l);
            var x = new ElState(l, JopaCanvas);
            //x.Drawed += X_Drawed;
            lst.Add(x);

        }
        private void JopaCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse l = new Ellipse() { Width = Dopnik.EllipseSize.X, Height = Dopnik.EllipseSize.Y };
            l.Stroke = new SolidColorBrush(Colors.Black);
            l.Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(r_color.Next(0, 255)),
         Convert.ToByte(r_color.Next(0, 255)), Convert.ToByte(r_color.Next(0, 255))));
            var pos = e.GetPosition(JopaCanvas);
            Canvas.SetLeft(l, pos.X - l.Width / 2);
            Canvas.SetTop(l, pos.Y - l.Height / 2);
            JopaCanvas.Children.Add(l);
            var x = new ElState(l, JopaCanvas, false);
            x.Cur_dir_h = Direction_h.None;
            //x.Cur_dir_w = Direction_w.None;
            lst.Add(x);
        }
        private void JopaCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //lst.ForEach(t => t.Change_size_of_Target(e.NewSize));
            if (e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0)
                return;
            Dopnik.ReInitStatic_bySize(new Point(e.NewSize.Width, e.NewSize.Height));
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //lst.ForEach()
        }

        private List<ElState> lst = new List<ElState>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lst.ForEach(t => t.MoveTo(t.time_));
        }
        bool statinit = false;
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (statinit)
                return;
            Dopnik.InitStatic(new Point(JopaCanvas.ActualWidth, JopaCanvas.ActualHeight), 30, 30);

        }
    }
}
