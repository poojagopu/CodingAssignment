using Microsoft.Win32;
using System;
using System.IO;
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
        private bool isDragging;
        private Point clickPosition;
        Button myButton;
        public MainWindow()
        {
            InitializeComponent();
            myButton = new Button();
        }
        private void Save_ImageButton(object sender, RoutedEventArgs e)
        {
            canvas.Children.Remove(myButton);
            // Create a new RenderTargetBitmap of the same size as the image
            //Point imageTopLeft = new Point(Canvas.GetLeft(image1), Canvas.GetTop(image1));
            //Point imageSize = new Point(image1.ActualWidth, image1.Height);
            //Canvas.SetTop(canvas, Canvas.GetTop(image1));
            //Canvas.SetLeft(canvas, Canvas.GetLeft(image1));
            //canvas.RenderTransform = new TranslateTransform(0, -50);
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)image1.ActualWidth, (int)image1.ActualHeight+50, 96, 96, PixelFormats.Pbgra32);
            
            // Render the canvas to the bitmap
            bitmap.Render(canvas);

            // Create a new PngBitmapEncoder
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            // Add the bitmap to the encoder
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            // Create a new file dialog for saving the image
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PNG Image (.png)|.png";
            dialog.FileName = "image.png";

            // Show the dialog and save the image if the user clicks OK
            if (dialog.ShowDialog() == true)
            {
                using (Stream stream = dialog.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
            canvas.Children.Add(myButton);
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
            rect.MouseRightButtonDown += Rect_MouseRightDown;
            rect.MouseLeftButtonDown += rectangle_MouseLeftButtonDown;
            rect.MouseMove += rectangle_MouseMove;
            rect.MouseLeftButtonUp += rectangle_MouseLeftButtonUp;


            // left property represents the distance between the left side of a control and its parent container Canvas
            // top property represents the distance between the top of a control and its parent container Canvas

            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.Y);

            canvas.Children.Add(rect);
        }

        private void rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;
            if (isDragging)
            {
                // Get the position of the mouse relative to the canvas.
                Point currentPosition = e.GetPosition(canvas);

                double x = Math.Min(currentPosition.X, clickPosition.X);
                double y = Math.Min(currentPosition.Y, clickPosition.Y);

                double width = Math.Abs(currentPosition.X - clickPosition.X);
                double height = Math.Abs(currentPosition.Y - clickPosition.Y);

                // checking if the rectangle is inside the image
                if (x < 0 || y < 0 || (x + width > image1.Width) || (y + height > image1.Height))
                {
                    return;
                }

                // Calculate the offset from the position where the mouse was clicked.
                double offsetX = currentPosition.X - clickPosition.X;
                double offsetY = currentPosition.Y - clickPosition.Y;

                // Update the position of the rectangle using the Canvas.Left and Canvas.Top attached properties.
                Canvas.SetLeft(rect, Canvas.GetLeft(rect) + offsetX);
                Canvas.SetTop(rect, Canvas.GetTop(rect) + offsetY);

                // Remember the current position as the new click position.
                clickPosition = currentPosition;
            }
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
        private void Rect_MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;

            // Create a new context menu
            ContextMenu menu = new ContextMenu();

            // Create a new "Change Color" menu item
            MenuItem changeColorItem = new MenuItem();
            changeColorItem.Header = "Change Color";
            changeColorItem.Click += (s, args) =>
            {
                // Show a color dialog to select a new color
                System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Set the Fill property of the rectangle to the selected color
                    Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                    rect.Fill = new SolidColorBrush(color);
                }
            };
            menu.Items.Add(changeColorItem);

            // Create a new "Remove Rectangle" menu item
            MenuItem removeItem = new MenuItem();
            removeItem.Header = "Remove Rectangle";
            removeItem.Click += (s, args) =>
            {
                // Remove the rectangle from the canvas
                canvas.Children.Remove(rect);
            };
            menu.Items.Add(removeItem);

            // Set the context menu of the rectangle to the new context menu
            rect.ContextMenu = menu;
        }

        private void rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;
            // Set the isDragging flag to true and remember the position where the mouse was clicked.
            isDragging = true;
            clickPosition = e.GetPosition(canvas);

            // Change the cursor to a hand cursor to indicate that the rectangle can be dragged.
            rect.Cursor = Cursors.Hand;

            // Capture the mouse so that mouse events are still handled even if the mouse leaves the rectangle.
            rect.CaptureMouse();
        }

        private void rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;
            // Set the isDragging flag to false and restore the default cursor.
            isDragging = false;
            rect.Cursor = Cursors.Arrow;

            // Release the mouse capture.
            rect.ReleaseMouseCapture();
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
                image1.Stretch = Stretch.Fill;
            }

            // Set the button's properties
            myButton.Content = "Save Image";
            myButton.Margin = new Thickness(10);
            myButton.Click += Save_ImageButton; // assign an event handler for the Click event

            // Add the button to a canvas
            canvas.Children.Add(myButton);
        }

        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer.GetAdornerLayer(image1).Add(new ResizeAdorner(rect, image1));


        }
    }

    public class ResizeAdorner : Adorner
    {
        VisualCollection AdornerVisuals;
        Thumb thumb1, thumb2, thumb3, thumb4;
        Thumb te, be, le, re;

        /* thumb1 - topleft
         * thumb2 - bottomright
         * thumb3 - bottomleft
         * thumb4 - topright
         */

        Rectangle Rec;
        Image image;

        public ResizeAdorner(UIElement adornedElement, Image image1) : base(adornedElement)
        {
            AdornerVisuals = new VisualCollection(this);
            image = image1;
            thumb1 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
            thumb2 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
            thumb3 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
            thumb4 = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };

            te = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
            be = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
            le = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };
            re = new Thumb() { Background = Brushes.Coral, Height = 10, Width = 10 };


            Rec = new Rectangle() { Stroke = Brushes.Coral, StrokeThickness = 2, StrokeDashArray = { 3, 2 } };


            thumb1.DragDelta += Thumb1_DragDelta;
            thumb2.DragDelta += Thumb2_DragDelta;
            thumb3.DragDelta += Thumb3_DragDelta;
            thumb4.DragDelta += Thumb4_DragDelta;

            te.DragDelta += Te_DragDelta;
            be.DragDelta += Be_DragDelta;
            le.DragDelta += Le_DragDelta;
            re.DragDelta += Re_DragDelta;

            AdornerVisuals.Add(Rec);
            AdornerVisuals.Add(thumb1);
            AdornerVisuals.Add(thumb2);
            AdornerVisuals.Add(thumb3);
            AdornerVisuals.Add(thumb4);

            AdornerVisuals.Add(te);
            AdornerVisuals.Add(be);
            AdornerVisuals.Add(le);
            AdornerVisuals.Add(re);



        }

        private void Re_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var ele = (FrameworkElement)AdornedElement;
            ele.Width = ele.Width + e.HorizontalChange < 0 ? 0 : ele.Width + e.HorizontalChange;
        }

        private void Le_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var ele = (FrameworkElement)AdornedElement;
            double deltaHorizontal = Math.Min(e.HorizontalChange, ele.Width - ele.MinWidth);
            ele.Width -= deltaHorizontal;

            Canvas.SetLeft(ele, Canvas.GetLeft(ele) + deltaHorizontal);
        }

        private void Te_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var ele = (FrameworkElement)AdornedElement;
            double deltaVertical = Math.Min(e.VerticalChange, ele.Height - ele.MinHeight);
            ele.Height -= deltaVertical;

            Canvas.SetTop(ele, Canvas.GetTop(ele) + deltaVertical);
        }

        private void Be_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var ele = (FrameworkElement)AdornedElement;
            ele.Height = ele.Height + e.VerticalChange < 0 ? 0 : ele.Height + e.VerticalChange;

        }

        private void Thumb2_DragDelta(object sender, DragDeltaEventArgs e)// bottom right
        {
            var ele = (FrameworkElement)AdornedElement;
            ele.Height = ele.Height + e.VerticalChange < 0 ? 0 : ele.Height + e.VerticalChange;
            ele.Width = ele.Width + e.HorizontalChange < 0 ? 0 : ele.Width + e.HorizontalChange;

        }

        private void Thumb1_DragDelta(object sender, DragDeltaEventArgs e) //top left
        {
            var ele = (FrameworkElement)AdornedElement;

            // Adjust width and left
           double deltaHorizontal = Math.Min(e.HorizontalChange, ele.Width - ele.MinWidth);
            ele.Width -= deltaHorizontal;

            Canvas.SetLeft(ele, Canvas.GetLeft(ele) + deltaHorizontal);

            // Adjust height and top
            double deltaVertical = Math.Min(e.VerticalChange, ele.Height - ele.MinHeight);
            ele.Height -= deltaVertical;

            Canvas.SetTop(ele, Canvas.GetTop(ele) + deltaVertical);
        }


        private void Thumb3_DragDelta(object sender, DragDeltaEventArgs e) //bottom left
        {
            var ele = (FrameworkElement)AdornedElement;

            // Adjust width and left
            double deltaHorizontal = Math.Min(e.HorizontalChange, ele.Width - ele.MinWidth);
            ele.Width -= deltaHorizontal;

            Canvas.SetLeft(ele, Canvas.GetLeft(ele) + deltaHorizontal);

            // Adjust height and top
            ele.Height = ele.Height + e.VerticalChange < 0 ? 0 : ele.Height + e.VerticalChange;

        }

        private void Thumb4_DragDelta(object sender, DragDeltaEventArgs e) //bottom left
        {
            var ele = (FrameworkElement)AdornedElement;

            // Adjust width and left
            ele.Width = ele.Width + e.HorizontalChange < 0 ? 0 : ele.Width + e.HorizontalChange;

            // Adjust height and top
            double deltaVertical = Math.Min(e.VerticalChange, ele.Height - ele.MinHeight);
            ele.Height -= deltaVertical;

            Canvas.SetTop(ele, Canvas.GetTop(ele) + deltaVertical);

        }


        protected override Visual GetVisualChild(int index)
        {
            return AdornerVisuals[index];
        }

        protected override int VisualChildrenCount => AdornerVisuals.Count;



        protected override Size ArrangeOverride(Size finalSize)
        {

            Rec.Arrange(new Rect(-2.5, -2.5, AdornedElement.DesiredSize.Width + 5, AdornedElement.DesiredSize.Height + 5));
            thumb1.Arrange(new Rect(-5, -5, 10, 10));// top left
            thumb2.Arrange(new Rect(AdornedElement.DesiredSize.Width - 5, AdornedElement.DesiredSize.Height - 5, 10, 10)); //bottom right
            thumb3.Arrange(new Rect(- 5, AdornedElement.DesiredSize.Height - 5, 10, 10)); //bottom right
            thumb4.Arrange(new Rect(AdornedElement.DesiredSize.Width - 5, - 5, 10, 10)); //bottom right

            te.Arrange(new Rect((AdornedElement.DesiredSize.Width / 2) - 5, -5, 10, 10));
            be.Arrange(new Rect((AdornedElement.DesiredSize.Width / 2) - 5, AdornedElement.DesiredSize.Height - 5, 10, 10));
            re.Arrange(new Rect(AdornedElement.DesiredSize.Width-5, (AdornedElement.DesiredSize.Height / 2) - 5, 10, 10));
            le.Arrange(new Rect(-5, (AdornedElement.DesiredSize.Height / 2) - 5, 10, 10));


            return base.ArrangeOverride(finalSize);
        }

    }
}