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
using System.Windows.Controls.Primitives;


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

            rect.Loaded += Rectangle_Loaded;

            // Add event handler for changing the rectangle color
            rect.MouseDown += Rect_MouseDown;


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
        // learn the difference bwtween MouseButtonEventArgs and MouseEventArgs
        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;

            // Show a color dialog to select a new color
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Set the Fill property of the rectangle to the selected color
                Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                rect.Fill = new SolidColorBrush(color);

            }
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
                image1.Source = new BitmapImage(new Uri(dialog.FileName));
            }
        }

        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer.GetAdornerLayer(image1).Add(new ResizeAdorner(rect));


        }
    }

    public class ResizeAdorner : Adorner
    {
        VisualCollection AdornerVisuals;
        Thumb thumb1, thumb2;
        Rectangle Rec;

        public ResizeAdorner(UIElement adornedElement) : base(adornedElement)
        {
            AdornerVisuals = new VisualCollection(this);
            thumb1 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
            thumb2 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
            Rec = new Rectangle() { Stroke = Brushes.Coral, StrokeThickness = 2, StrokeDashArray = { 3, 2 } };


            thumb1.DragDelta += Thumb1_DragDelta;
            thumb2.DragDelta += Thumb2_DragDelta;

            AdornerVisuals.Add(Rec);
            AdornerVisuals.Add(thumb1);
            AdornerVisuals.Add(thumb2);


        }

        private void Thumb2_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var ele = (FrameworkElement)AdornedElement;
            ele.Height = ele.Height + e.VerticalChange < 0 ? 0 : ele.Height + e.VerticalChange;

            ele.Width = ele.Width + e.HorizontalChange < 0 ? 0 : ele.Width + e.HorizontalChange;

        }

        private void Thumb1_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var ele = (FrameworkElement)AdornedElement;
            ele.Height = ele.Height - e.VerticalChange < 0 ? 0 : ele.Height - e.VerticalChange;

            ele.Width = ele.Width - e.HorizontalChange < 0 ? 0 : ele.Width - e.HorizontalChange;
        }




        protected override Visual GetVisualChild(int index)
        {
            return AdornerVisuals[index];
        }

        protected override int VisualChildrenCount => AdornerVisuals.Count;



        protected override Size ArrangeOverride(Size finalSize)
        {

            Rec.Arrange(new Rect(-2.5, -2.5, AdornedElement.DesiredSize.Width + 5, AdornedElement.DesiredSize.Height + 5));
            thumb1.Arrange(new Rect(-5, -5, 10, 10));
            thumb2.Arrange(new Rect(AdornedElement.DesiredSize.Width - 5, AdornedElement.DesiredSize.Height - 5, 10, 10));



            return base.ArrangeOverride(finalSize);
        }



    }
}