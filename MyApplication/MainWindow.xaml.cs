using Microsoft.Win32;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace MyApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Serializable]
    public class CanvasItem
    {
        public string Type { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public SerializableThickness Margin { get; set; }
        public ColorData FillColor { get; set; }
        public double Opacity { get; set; }
        public string Content { get; set; }
        public string ImageUri { get; set; }
        public HorizontalAlignment? HorizontalAlignment { get; internal set; }
        public VerticalAlignment? VerticalAlignment { get; internal set; }
    }

    [Serializable]
    public class SessionData
    {
        public List<CanvasItem> Items { get; set; }
    }

    


    [Serializable]
    public class ColorData
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public ColorData(SolidColorBrush solidColorBrush)
        {
        }

        public ColorData(Color color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public Color ToColor()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }

    [Serializable]
    public class SerializableThickness : ISerializable
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }

        public SerializableThickness() { }

        public SerializableThickness(Thickness thickness)
        {
            Left = thickness.Left;
            Top = thickness.Top;
            Right = thickness.Right;
            Bottom = thickness.Bottom;
        }

        public Thickness ToThickness()
        {
            return new Thickness(Left, Top, Right, Bottom);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Left", Left);
            info.AddValue("Top", Top);
            info.AddValue("Right", Right);
            info.AddValue("Bottom", Bottom);
        }

        protected SerializableThickness(SerializationInfo info, StreamingContext context)
        {
            Left = info.GetDouble("Left");
            Top = info.GetDouble("Top");
            Right = info.GetDouble("Right");
            Bottom = info.GetDouble("Bottom");
        }
    }

    [Serializable]
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private Rectangle rectangle;
        private bool isDragging;
        private Point clickPosition;
        Button myButton;
        Button saveSession;
        Button loadSession;
        private Uri imageURI;
        public MainWindow()
        {
            InitializeComponent();
            myButton = new Button();
            saveSession = new Button();
            loadSession = new Button();
        }

        private void SaveSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Binary files (.bin)|*.bin|All files (*.*)|*.*";
                saveFileDialog.Title = "Save session data";
                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        var sessionData = new SessionData { Items = new List<CanvasItem>() };

                        // Store the data for each canvas item in the session data.
                        foreach (var item in canvas.Children.OfType<FrameworkElement>())
                        {
                            var type = item.GetType().Name;
                            if (type == "Rectangle" || type == "Button" || type == "Image")
                            {
                                var canvasItem = new CanvasItem
                                {
                                    Type = type,
                                    Left = Canvas.GetLeft(item),
                                    Top = Canvas.GetTop(item),
                                    Width = item.Width,
                                    Height = item.Height,
                                    FillColor = new ColorData((item as Shape)?.Fill is SolidColorBrush solidColorBrush ? solidColorBrush.Color : Colors.Transparent),
                                    Opacity = item.Opacity,
                                    Content = (item as Button)?.Content as string,
                                    ImageUri = (item as Image)?.Source?.ToString(),
                                    HorizontalAlignment = (item as Button)?.HorizontalAlignment,
                                    VerticalAlignment = (item as Button)?.VerticalAlignment,
                                    Margin = (item as Button)?.Margin != null ? new SerializableThickness((item as Button).Margin) : new SerializableThickness()
                                };
                                sessionData.Items.Add(canvasItem);
                            }
                        }
                        // Serialize the session data and write it to the file stream.
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(stream, sessionData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred in SaveSession_Click: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void LoadSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Binary files (.bin)|*.bin|All files (*.*)|*.*";
                openFileDialog.Title = "Load session data";
                if (openFileDialog.ShowDialog() == true)
                {
                    var filePath = openFileDialog.FileName;
                    using (var stream = new FileStream(filePath, FileMode.Open))
                    {
                        var formatter = new BinaryFormatter();
                        stream.Position = 0;
                        var sessionData = formatter.Deserialize(stream) as SessionData;

                        // Remove existing canvas items
                        canvas.Children.Clear();

                        // Add the canvas items stored in the session data
                        foreach (var item in sessionData.Items)
                        {
                            if (item.Type == "Rectangle")
                            {
                                var rect = new Rectangle
                                {
                                    Width = item.Width,
                                    Height = item.Height,
                                    Fill = new SolidColorBrush(item.FillColor.ToColor()),
                                    Opacity = item.Opacity,
                                };
                                
                                /*
                                rect.Loaded += Rectangle_Loaded;
                                rect.MouseRightButtonDown += Rectangle_MouseRightDown;
                                rect.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                                rect.MouseMove += Rectangle_MouseMove;
                                rect.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
                                */
                                Canvas.SetLeft(rect, item.Left);
                                Canvas.SetTop(rect, item.Top);
                                canvas.Children.Add(rect);

                            }
                            else if (item.Type == "Button")
                            {
                                var button = new Button
                                {
                                    Margin = item.Margin.ToThickness(),
                                    Content = item.Content,
                                    HorizontalAlignment = item.HorizontalAlignment ?? HorizontalAlignment.Left,
                                    VerticalAlignment = item.VerticalAlignment ?? VerticalAlignment.Top,
                                    Opacity = item.Opacity,
                                    Width = item.Width,
                                    Height = item.Height

                                };
                                
                                Canvas.SetLeft(button, item.Left);
                                Canvas.SetTop(button, item.Top);
                                canvas.Children.Add(button);
                            }
                            else if (item.Type == "Image")
                            {
                                var image = new Image
                                {
                                    Width = item.Width,
                                    Height = item.Height,
                                    Opacity = item.Opacity,
                                    Source = new BitmapImage(new Uri(item.ImageUri)),
                                    Stretch = Stretch.Fill
                                };
                                Canvas.SetLeft(image, item.Left);
                                Canvas.SetTop(image, item.Top);
                                canvas.Children.Add(image);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred in LoadSession_Click: {ex.Message}");
            }

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //get position of mouse on window
                startPoint = e.GetPosition(canvas);

                rectangle = new Rectangle
                {
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 2,
                    Opacity = 0.5,
                    Fill = Brushes.Red,
                };

                rectangle.Loaded += Rectangle_Loaded;
                rectangle.MouseRightButtonDown += Rectangle_MouseRightDown;
                rectangle.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                rectangle.MouseMove += Rectangle_MouseMove;
                rectangle.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;

                Canvas.SetLeft(rectangle, startPoint.X);
                Canvas.SetTop(rectangle, startPoint.Y);

                canvas.Children.Add(rectangle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred in Image_MouseDown: {ex.Message}");
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                rectangle = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred in Image_MouseUp: {ex.Message}");
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                //when mouse is released
                if (e.LeftButton == MouseButtonState.Released || rectangle == null)
                    return;
                // Get the current position of the mouse on the canvas
                Point currentPosition = e.GetPosition(canvas);

                double rectangleTopLeftX = Math.Min(currentPosition.X, startPoint.X);
                double rectangleTopLeftY = Math.Min(currentPosition.Y, startPoint.Y);

                double width = Math.Abs(currentPosition.X - startPoint.X);
                double height = Math.Abs(currentPosition.Y - startPoint.Y);

                // Checking if the rectangle is inside the image
                if (rectangleTopLeftX < 0) rectangleTopLeftX = 0;
                if (rectangleTopLeftY < 0) rectangleTopLeftY = 0;

                if (rectangleTopLeftX + width > image.Width) width = image.Width - rectangleTopLeftX;
                if (rectangleTopLeftY + height > image.Height) height = image.Height - rectangleTopLeftY;

                rectangle.Width = width;
                rectangle.Height = height;

                Canvas.SetLeft(rectangle, rectangleTopLeftX);
                Canvas.SetTop(rectangle, rectangleTopLeftY);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine($"A NullReferenceException occurred in Image_MouseMove: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"An InvalidOperationException occurred in Image_MouseMove: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"An ArgumentException occurred in Image_MouseMove: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred in Image_MouseMove: {ex.Message}");
            }

        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas == null || image == null)
            {
                throw new NullReferenceException("Canvas or image is null in Rectangle_MouseMove method.");
            }

            Rectangle rectangle = sender as Rectangle;
            if (rectangle == null)
            {
                throw new ArgumentException("Sender is not a Rectangle object in Rectangle_MouseMove method.");
            }

            if (isDragging)
            {
                // Get the position of the mouse relative to the canvas.
                Point currentPosition = e.GetPosition(canvas);

                double rectangleTopLeftX = Math.Min(currentPosition.X, clickPosition.X);
                double rectangleTopLeftY = Math.Min(currentPosition.Y, clickPosition.Y);

                double width = Math.Abs(currentPosition.X - clickPosition.X);
                double height = Math.Abs(currentPosition.Y - clickPosition.Y);

                // Calculate the offset from the position where the mouse was clicked.
                double offsetX = currentPosition.X - clickPosition.X;
                double offsetY = currentPosition.Y - clickPosition.Y;

                // Update the position of the rectangle using the Canvas.Left and Canvas.Top attached properties.
                Point rectangleNewPosition = new Point(Canvas.GetLeft(rectangle) + offsetX, Canvas.GetTop(rectangle) + offsetY);
                if (rectangleNewPosition.X < 0) rectangleNewPosition.X = 0;
                if (rectangleNewPosition.Y < 0) rectangleNewPosition.Y = 0;
                if (rectangleNewPosition.X + rectangle.Width > image.Width) rectangleNewPosition.X = image.Width - rectangle.Width;
                if (rectangleNewPosition.Y + rectangle.Height > image.Height) rectangleNewPosition.Y = image.Height - rectangle.Height;

                if (double.IsNaN(rectangleNewPosition.X) || double.IsNaN(rectangleNewPosition.Y))
                {
                    throw new InvalidOperationException("Rectangle New Positon is NaN.");
                }
                Canvas.SetLeft(rectangle, rectangleNewPosition.X);
                Canvas.SetTop(rectangle, rectangleNewPosition.Y);

                // Remember the current position as the new click position.
                clickPosition = currentPosition;
            }
        }

        private void Rectangle_MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Rectangle rectangle = (Rectangle)sender;

                // Create a new dropdown context menu
                ContextMenu menu = new ContextMenu();

                // Create a new "Change Color" menu item
                MenuItem changeColorItem = new MenuItem();
                changeColorItem.Header = "Change Color";
                changeColorItem.Click += (s, args) =>
                {
                    try
                    {
                        // Show a color dialog to select a new color
                        System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
                        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            // Set the Fill property of the rectangle to the selected color
                            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                            rectangle.Fill = new SolidColorBrush(color);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that may occur while changing the color of the rectangle
                        MessageBox.Show($"An error occurred while changing the color of the rectangle: {ex.Message}");
                    }
                };
                menu.Items.Add(changeColorItem);

                // Create a new "Remove Rectangle" menu item
                MenuItem removeItem = new MenuItem();
                removeItem.Header = "Remove Rectangle";
                removeItem.Click += (s, args) =>
                {
                    try
                    {
                        // Remove the rectangle from the canvas
                        canvas.Children.Remove(rectangle);
                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that may occur while removing the rectangle from the canvas
                        MessageBox.Show($"An error occurred while removing the rectangle: {ex.Message}");
                    }
                };
                menu.Items.Add(removeItem);

                // Set the context menu of the rectangle to the new context menu
                rectangle.ContextMenu = menu;
            }
            catch (Exception ex)
            {
                // Handle any exception that may occur while creating the context menu
                MessageBox.Show($"An error occurred while creating the context menu: {ex.Message}");
            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Rectangle))
            {
                // Handling the error condition appropriately
                return;
            }
            Rectangle rectangle = (Rectangle)sender;
            // Set the isDragging flag to true and remember the position where the mouse was clicked.
            isDragging = true;
            clickPosition = e.GetPosition(canvas);

            // Change the cursor to a hand cursor to indicate that the rectangle can be dragged.
            rectangle.Cursor = Cursors.Hand;

            // Capture the mouse so that mouse events are still handled even if the mouse leaves the rectangle.
            rectangle.CaptureMouse();
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Rectangle))
            {
                // Handling the error condition appropriately
                return;
            }
            Rectangle rectangle = (Rectangle)sender;
            // Set the isDragging flag to false and restore the default cursor.
            isDragging = false;
            rectangle.Cursor = Cursors.Arrow;

            // Release the mouse capture.
            rectangle.ReleaseMouseCapture();
        }

        private void Save_ImageButton(object sender, RoutedEventArgs e)
        {
            try
            {
                canvas.Children.Remove(myButton);

                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)image.ActualWidth, (int)image.ActualHeight + 50, 96, 96, PixelFormats.Pbgra32);

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

                // Show the dialog and save the image if the user clicks on the button
                if (dialog.ShowDialog() == true)
                {
                    using (Stream stream = dialog.OpenFile())
                    {
                        encoder.Save(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving the image: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                canvas.Children.Add(myButton);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
                // Open one or more files
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image files|*.bmp;*.jpg;*.png";
                if (dialog.ShowDialog() == true)
                {
                    imageURI = new Uri(dialog.FileName);
                    image.Source = new BitmapImage(imageURI);
                    image.Stretch = Stretch.Fill;
                }

                // Set the button's properties
                myButton.Content = "Save Image";
                myButton.Margin = new Thickness(10);
                myButton.Click += Save_ImageButton; 

                
                saveSession.Content = "Save Session";
                saveSession.Margin = new Thickness(100,10,30,30);
                saveSession.Click += SaveSession_Click;


                loadSession.Content = "Load Session";
                loadSession.Margin = new Thickness(200, 10, 30, 30);
                loadSession.Click += LoadSession_Click;


                // Add the button to the canvas
                canvas.Children.Add(myButton);
                canvas.Children.Add(saveSession);
                canvas.Children.Add(loadSession);

            
            
        }


        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer.GetAdornerLayer(image).Add(
                new ResizeAdorner(rectangle, image, 
                new Point(2, 2)));
        }
    }

    [Serializable]
    public class ResizeAdorner : Adorner
    {
        VisualCollection AdornerVisuals;

        // Declaring thumbs for corners of rectangle
        Thumb topLeftThumb, bottomRightThumb, bottomLeftThumb, topRightThumb; 

        // Declaring thumbs for sides of rectangle
        Thumb topEdgeThumb, bottomEdgeThumb, leftEdgeThumb, rightEdgeThumb;

        Image image;
        Point imageTopLeft;

        public ResizeAdorner(UIElement adornedElement, Image image1, Point topleft) : base(adornedElement)
        {
            AdornerVisuals = new VisualCollection(this);
            image = image1;
            imageTopLeft = topleft;

            // Initialising all thumbs 
            topLeftThumb = new Thumb() { Background = Brushes.Coral, Opacity = 0.5, Height = 7, Width = 7 };
            bottomRightThumb = new Thumb() { Background = Brushes.Coral, Opacity = 0.5, Height = 7, Width = 7 };
            bottomLeftThumb = new Thumb() { Background = Brushes.Coral, Opacity = 0.5, Height = 7, Width = 7 };
            topRightThumb = new Thumb() { Background = Brushes.Coral, Opacity = 0.5, Height = 7, Width = 7 };

            topEdgeThumb = new Thumb() { Background = Brushes.Coral, Opacity = 0.5, Height = 7, Width = 7 };
            bottomEdgeThumb = new Thumb() { Background = Brushes.Coral, Opacity = 0.5, Height = 7, Width = 7 };
            leftEdgeThumb = new Thumb() { Background = Brushes.Coral, Opacity = 0.5, Height = 7, Width = 7 };
            rightEdgeThumb = new Thumb() { Background = Brushes.Coral, Opacity = 0.5, Height = 7, Width = 7 };

            // Creating EventHandlers for all thumbs
            topLeftThumb.DragDelta += TopLeft_ThumbDragDelta;
            bottomRightThumb.DragDelta += BottomRight_ThumbDragDelta;
            bottomLeftThumb.DragDelta += BottomLeft_ThumbDragDelta;
            topRightThumb.DragDelta += TopRight_ThumbDragDelta;

            topEdgeThumb.DragDelta += TopEdge_ThumbDragDelta;
            bottomEdgeThumb.DragDelta += BottomEdge_ThumbDragDelta;
            leftEdgeThumb.DragDelta += LeftEdge_ThumbDragDelta;
            rightEdgeThumb.DragDelta += RightEdge_ThumbDragDelta;

            AdornerVisuals.Add(topLeftThumb);
            AdornerVisuals.Add(bottomRightThumb);
            AdornerVisuals.Add(bottomLeftThumb);
            AdornerVisuals.Add(topRightThumb);

            AdornerVisuals.Add(topEdgeThumb);
            AdornerVisuals.Add(bottomEdgeThumb);
            AdornerVisuals.Add(leftEdgeThumb);
            AdornerVisuals.Add(rightEdgeThumb);

        }

        
        private bool Is_OutOfBounds(Point rectangleTopLeft, Point newDimensions)
        {
            // Checking if the rectangle is going out of the image boundaries
            if (rectangleTopLeft.X < imageTopLeft.X || rectangleTopLeft.Y < imageTopLeft.Y ||
                rectangleTopLeft.Y + newDimensions.Y > imageTopLeft.Y + image.ActualHeight ||
                rectangleTopLeft.X + newDimensions.X > imageTopLeft.X + image.ActualWidth)
            {
                return true;
            }
            return false;
        }

        private void RightEdge_ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                var rectangle = (FrameworkElement)AdornedElement;

                // Adjusting height and width of rectangle
                double newWidth = rectangle.Width + e.HorizontalChange < 0 ? 0 : rectangle.Width + e.HorizontalChange;

                // Checking if the rectangle goes out of the image boundaries
                if (!Is_OutOfBounds(new Point(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle)),
                                    new Point(newWidth, rectangle.Height)))
                {
                    rectangle.Width = newWidth;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception occurred: " + ex.Message);
            }
        }


        private void LeftEdge_ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                var rectangle = (FrameworkElement)AdornedElement;

                // Adjusting height and width of rectangle
                double deltaHorizontal = Math.Min(e.HorizontalChange, rectangle.Width - rectangle.MinWidth);
                double newWidth = rectangle.Width - deltaHorizontal;

                // Checking if the rectangle goes out of the image boundaries
                if (!Is_OutOfBounds(new Point(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle)),
                                    new Point(newWidth, rectangle.Height)))
                {
                    rectangle.Width -= deltaHorizontal;
                    Canvas.SetLeft(rectangle, Canvas.GetLeft(rectangle) + deltaHorizontal);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }


        private void TopEdge_ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                var rectangle = (FrameworkElement)AdornedElement;

                // Adjusting height and width of rectangle
                double deltaVertical = Math.Min(e.VerticalChange, rectangle.Height - rectangle.MinHeight);
                double newHeight = rectangle.Height - deltaVertical;

                // Checking if the rectangle goes out of the image boundaries
                if (!Is_OutOfBounds(new Point(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle)),
                                    new Point(rectangle.Width, newHeight)))
                {
                    rectangle.Height -= deltaVertical;
                    Canvas.SetTop(rectangle, Canvas.GetTop(rectangle) + deltaVertical);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred in TopEdge_ThumbDragDelta: " + ex.Message);
            }
        }


        private void BottomEdge_ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                var rectangle = (FrameworkElement)AdornedElement;

                // Adjusting height and width of rectangle
                double newHeight = rectangle.Height + e.VerticalChange < 0 ? 0 : rectangle.Height + e.VerticalChange;

                // Checking if the rectangle goes out of the image boundaries
                if (!Is_OutOfBounds(new Point(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle)),
                                    new Point(rectangle.Width, newHeight)))
                {
                    rectangle.Height = newHeight;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in BottomEdge_ThumbDragDelta: " + ex.Message);
            }
        }

        private void BottomRight_ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                var rectangle = (FrameworkElement)AdornedElement;

                // Adjusting height and width of rectangle
                double newHeight = rectangle.Height + e.VerticalChange < 0 ? 0 : rectangle.Height + e.VerticalChange;
                double newWidth = rectangle.Width + e.HorizontalChange < 0 ? 0 : rectangle.Width + e.HorizontalChange;

                // Checking if the rectangle goes out of the image boundaries
                if (!Is_OutOfBounds(new Point(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle)),
                                    new Point(newWidth, newHeight)))
                {
                    rectangle.Height = newHeight;
                    rectangle.Width = newWidth;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in BottomRight_ThumbDragDelta: {ex.Message}");
            }
        }

        private void TopLeft_ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                var rectangle = (FrameworkElement)AdornedElement;

                // Adjusting height and width of rectangle
                double deltaHorizontal = Math.Min(e.HorizontalChange, rectangle.Width - rectangle.MinWidth);
                double deltaVertical = Math.Min(e.VerticalChange, rectangle.Height - rectangle.MinHeight);
                double newWidth = rectangle.Width - deltaHorizontal;
                double newHeight = rectangle.Height - deltaVertical;

                // Checking if the rectangle goes out of the image boundaries
                if (!Is_OutOfBounds(new Point(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle)),
                                    new Point(rectangle.Width, newHeight)))
                {
                    rectangle.Height = newHeight;
                    rectangle.Width = newWidth;
                    Canvas.SetLeft(rectangle, Canvas.GetLeft(rectangle) + deltaHorizontal);
                    Canvas.SetTop(rectangle, Canvas.GetTop(rectangle) + deltaVertical);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred in the TopLeft_ThumbDragDelta method: {ex.Message}");
            }
        }

        private void BottomLeft_ThumbDragDelta(object sender, DragDeltaEventArgs e) //bottom left
        {
            try
            {
                var rectangle = (FrameworkElement)AdornedElement;

                // Adjusting height and width of rectangle
                double deltaHorizontal = Math.Min(e.HorizontalChange, rectangle.Width - rectangle.MinWidth);
                double newWidth = rectangle.Width - deltaHorizontal;
                double newHeight = rectangle.Height + e.VerticalChange < 0 ? 0 : rectangle.Height + e.VerticalChange;

                // Checking if the rectangle goes out of the image boundaries
                if (!Is_OutOfBounds(new Point(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle)),
                                    new Point(newWidth, newHeight)))
                {
                    rectangle.Width = newWidth;
                    rectangle.Height = newHeight;
                    Canvas.SetLeft(rectangle, Canvas.GetLeft(rectangle) + deltaHorizontal);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }


        private void TopRight_ThumbDragDelta(object sender, DragDeltaEventArgs e) //bottom left
        {
            try
            {
                var rectangle = (FrameworkElement)AdornedElement;

                // Adjusting height and width of rectangle
                double deltaVertical = Math.Min(e.VerticalChange, rectangle.Height - rectangle.MinHeight);
                double newWidth = rectangle.Width + e.HorizontalChange < 0 ? 0 : rectangle.Width + e.HorizontalChange;
                double newHeight = rectangle.Height - deltaVertical;

                // Checking if the rectangle goes out of the image boundaries
                if (!Is_OutOfBounds(new Point(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle)),
                                    new Point(newWidth, newHeight)))
                {
                    rectangle.Width = newWidth;
                    rectangle.Height = newHeight;
                    Canvas.SetTop(rectangle, Canvas.GetTop(rectangle) + deltaVertical);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }


        protected override Visual GetVisualChild(int index)
        {
            return AdornerVisuals[index];
        }

        protected override int VisualChildrenCount => AdornerVisuals.Count;
       
        protected override Size ArrangeOverride(Size finalSize)
        {

            topLeftThumb.Arrange(new Rect(-5, -5, 10, 10));
            bottomRightThumb.Arrange(new Rect(AdornedElement.DesiredSize.Width - 5, AdornedElement.DesiredSize.Height - 5, 10, 10)); 
            bottomLeftThumb.Arrange(new Rect(- 5, AdornedElement.DesiredSize.Height - 5, 10, 10)); 
            topRightThumb.Arrange(new Rect(AdornedElement.DesiredSize.Width - 5, - 5, 10, 10)); 

            topEdgeThumb.Arrange(new Rect((AdornedElement.DesiredSize.Width / 2) - 5, -5, 10, 10));
            bottomEdgeThumb.Arrange(new Rect((AdornedElement.DesiredSize.Width / 2) - 5, AdornedElement.DesiredSize.Height - 5, 10, 10));
            rightEdgeThumb.Arrange(new Rect(AdornedElement.DesiredSize.Width-5, (AdornedElement.DesiredSize.Height / 2) - 5, 10, 10));
            leftEdgeThumb.Arrange(new Rect(-5, (AdornedElement.DesiredSize.Height / 2) - 5, 10, 10));

            return base.ArrangeOverride(finalSize);
        }
    }
}