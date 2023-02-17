using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private Rectangle rect;
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //get position of mouse on window
            startPoint = e.GetPosition(canvas);

            rect = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Opacity = 0.5,
                Fill = Brushes.Red,
                
            };
            // left property represents the distance between the left side of a control and its parent container Canvas
            // top property represents the distance between the top of a control and its parent container Canvas

            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.Y);

            canvas.Children.Add(rect);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            //when mouse is released
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;
            //get curr position of mouse
            Point pos = e.GetPosition(canvas);

            double x = Math.Min(pos.X, startPoint.X);
            double y = Math.Min(pos.Y, startPoint.Y);

            double width = Math.Abs(pos.X - startPoint.X);
            double height = Math.Abs(pos.Y - startPoint.Y);

            // checking if the rectangle is inside the image
            if (x < 0) x = 0;
            if (y < 0) y = 0;
  
            if (x + width > image1.Width) width = image1.Width - x;
            if (y + height > image1.Height) height = image1.Height - y;

            rect.Width = width;
            rect.Height = height;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rect = null;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //to open one or more files
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            if (dialog.ShowDialog() == true)
            {
                image1.Source=new BitmapImage(new Uri(dialog.FileName));
            }
        }
    }
}
