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
        List<List<ElState>> pares = new List<List<ElState>>();
        private void MainWindow_LayoutUpdated(object sender, EventArgs e)
        {
            //если не удалилсь друг от друга на расстояние больше в 1px то не учитывать их
            //а для этого нужно определить пары
            foreach (var item in lst.Where(t => !t.Resent_changed))
            {
                var a = lst.Where(t => t.Id != item.Id && !t.Resent_changed).Where(t => Math.Abs(t.Center.X - item.Center.X) < 16 && Math.Abs(t.Center.Y - item.Center.Y) < 16);
                if (a.Any())
                {
                    var xx = new List<ElState>();
                    var z = a.First();
                    xx.Add(item);
                    xx.Add(z);
                    pares.Add(xx);
                    Direction_w buf_w = z.Cur_dir_w;
                    Direction_h buf_h = z.Cur_dir_h;
                    z.CalcNewPointsToMove(item.Cur_dir_h, item.Cur_dir_w);
                    item.CalcNewPointsToMove(buf_h, buf_w);
                    z.Resent_changed = true;
                    item.Resent_changed = true;
                }
            }

        }

        private void JopaCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse l = new Ellipse() { Width = 30, Height = 30 };
            l.Stroke = new SolidColorBrush(Colors.Black);
            l.Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(r_color.Next(0, 255)),
         Convert.ToByte(r_color.Next(0, 255)), Convert.ToByte(r_color.Next(0, 255))));
            var pos = e.GetPosition(JopaCanvas);
            Canvas.SetLeft(l, pos.X - l.Width / 2);
            Canvas.SetTop(l, pos.Y - l.Height / 2);
            JopaCanvas.Children.Add(l);
            var x = new ElState(l, JopaCanvas);
            x.Drawed += X_Drawed;
            lst.Add(x);

        }
        private void JopaCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse l = new Ellipse() { Width = 30, Height = 30 };
            l.Stroke = new SolidColorBrush(Colors.Black);
            l.Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(r_color.Next(0, 255)),
         Convert.ToByte(r_color.Next(0, 255)), Convert.ToByte(r_color.Next(0, 255))));
            var pos = e.GetPosition(JopaCanvas);
            Canvas.SetLeft(l, pos.X - l.Width / 2);
            Canvas.SetTop(l, pos.Y - l.Height / 2);
            JopaCanvas.Children.Add(l);
            var x = new ElState(l, JopaCanvas, false);
            x.Cur_dir_h = Direction_h.Up;
            x.Cur_dir_w = Direction_w.Left;
            lst.Add(x);
        }
        private void X_Drawed(int id, double left, double top)
        {
            //Debug.WriteLine($"BALL {id} CHANGE!");
            //var a = lst.Where(t => t.Id != id && Math.Abs(t.Left - left) < 31 && Math.Abs(t.Top - top) < 31);
            //if (a.Any())
            //{
            //    Debug.WriteLine($"BALL {id} REQUEST CHANGE!");
            //    lst.Single(t => t.Id == id).CalcNewPointsToMove(a.ToList());
            //}
        }

        private void JopaCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lst.ForEach(t => t.Change_size_of_Target(e.NewSize));
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //lst.ForEach()
        }

        private List<ElState> lst = new List<ElState>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lst.Last().CalcNewPointsToMove();
        }


    }
}
