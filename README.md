## Coding Assignment

This project is a WPF application that allows the user to select and load an image, draw one or more rectangles on the image, resize and move the rectangle(s), change their color, and save the changes to a new image.

## Features

- Load an Image
    
    Click the Load Image button to select and load an image. The image will be displayed on the canvas.

- Draw Rectangle(s) on the Image

    Click and drag the mouse on the canvas to draw a rectangle.
    The size of the rectangle depends on how much the user drags the mouse. Only allow drawing inside the image.

- Change the Rectangle Color

    Click any rectangle to select it.
    Right-click the rectangle and select 'color change' from dropdown and then select a color from the color palette to change the rectangle's color.

- Resize the Rectangle(s)

    Hover over any corner or side of the rectangle until the mouse cursor changes to a resize icon.
    Click and drag the mouse to resize the rectangle.

- Move the Rectangle(s)
   
    Click and hold a rectangle to select it.
    Move the mouse to move the selected rectangle.
    Release the mouse to drop the rectangle at the new location.

- Delete any Rectangle
    
    Click a rectangle to select it.
    Right-click the rectangle and select 'delete rectangle' from dropdown and then press the Delete option to delete the selected rectangle.

- Save the Changes to a New Image
    
    Click the Save Image button to save the changes made on the canvas to a new image file.
    The new image will be saved to our local.

- Additional Feature - Save session

    - Click the "Save Session" and select a location for to store binary file using a file dialog box. 
    - The data for each canvas item (i.e. Rectangle, Button, and Image) is stored in a session data object. The session data object is then serialized and written to the binary file.

- Additional Feature - Load previous sessions
    - Click the "load session" button to select a binary file using a file dialog box and the data for each canvas item is deserialized from the file and added to the canvas. 
    - The existing canvas items are first removed before adding the items stored in the session data. The code also handles the case where the user has drawn rectangles on the canvas and loaded them from a saved session. 
    - Additionally, it allows the user to open and update a previously saved image file.

## How to Use

- Launch the application by double-clicking the WPFImageEditor.exe file.
- Click the Load Image button to load an image.
- Draw one or more rectangles on the image by clicking and dragging the mouse on the canvas.
- Change the color of a rectangle by selecting from dropdown on right-clicking to access the color palette.
- Resize the rectangle(s) by clicking and dragging on any corner or side of the rectangle.
- Move the rectangle(s) by clicking and dragging on a selected rectangle.
- Delete any rectangle from dropdown on right-clicking it and pressing the Delete option.
- Click the Save Image button to save the changes to a new image file.
- Click the "Save Session" button to save the current session to a binary file. Select the location to store the binary file in the file dialog box.
- Click "Load Session" to restore the image and rectangles of the saved session, which will be retrieved from the binary file.

## Installation

The application is implemented using C# in Visual Studio 2019 with .NET Framework 4.8 and WPF.

    
## Implementation Details

The following are the implementation details of the different features:

- Load an Image
    - Used the OpenFileDialog class to allow the user to select an image file.
    - Used the BitmapImage class to load the image into a System.Windows.Controls.Image control.

- Draw Rectangle(s) on the Image
    - Used the MouseDown and MouseUp events of the System.Windows.Controls.Image control to handle the drawing of the rectangle.
    - Used the Rectangle class to draw a rectangle with a transparent border and a red fill with less opacity.
    - Used the Canvas.SetLeft and Canvas.SetTop attached properties to set the position of the rectangle on the canvas.

- Change the Rectangle Color
    - Used the MouseRightButtonDown event of the rectangle to handle the color change from the dropdown created.
    - Used the ColorDialog class to allow the user to select a new color for the rectangle. 

- Resize the Rectangle(s)
    - Used the Adorner class to handle the resizing of the rectangle.
    - Hovered over any corner or side of the rectangle until the mouse cursor changes to a resize icon.
    - Clicked and dragged the mouse to resize the rectangle.

- Move the Rectangle(s)
    - Used the RectangleMouseLeftButtonDown, RectangleMouseMove, and RectangleMouseLeftButtonUp events of the rectangle to handle the events of the rectangle to handle the moving of the rectangle.

    - Used the Canvas.SetLeft and Canvas.SetTop attached properties to set the new position of the rectangle on the canvas.

- Delete any Rectangle

    - Used the MouseRightButtonDown event of the rectangle to handle the deletion from the dropdown created.
    - Used the Canvas.Children.Remove method to remove the selected rectangle from the canvas.

- Save the Changes to a New Image

    - Used the SaveFileDialog class to allow the user to select the location and name of the new image file.
    - Used the RenderTargetBitmap class to render the canvas to a bitmap.
    - Used the BitmapEncoder class to encode the bitmap into an image file and save it to the selected location.

- Save session 
    - This function attempts to save the data of the current session to a file. 
    - It opens a SaveFileDialog to allow the user to select the location and name of the file. 
    - If a file is selected, the function creates a FileStream and uses it to serialize the data of the current session into a SessionData object. 
    - It then writes the object to the file using BinaryFormatter. If any exception occurs during the process, the function catches it and prints an error message to the console.

- Load Session
    - The LoadSession_Click function allows the user to load the data of a previously saved session. 
    - It opens an OpenFileDialog to allow the user to select the file to be loaded. If a file is selected, the function creates a FileStream and uses BinaryFormatter to deserialize the data in the file into a SessionData object. 
    - It then removes any existing canvas items and adds the canvas items stored in the SessionData object to the canvas. 
    - If any exception occurs during the process, the function catches it and prints an error message to the console.
    
##Resources

- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/?view=netdesktop-7.0
- https://www.youtube.com/watch?v=ddVXKMpWGME
- https://learn.microsoft.com/en-us/dotnet/standard/events/
- https://www.c-sharpcorner.com/UploadFile/201fc1/handling-events-in-wpf-in-an-easy-and-short-hand-way/
- https://www.c-sharpcorner.com/UploadFile/mahesh/using-xaml-image-in-wpf/
- https://learn.microsoft.com/en-us/dotnet/csharp/
- https://www.devcoons.com/c-wpf-canvas-move-shapes-around/
