using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FaceReplacement.Engine;

namespace FaceReplacement
{
    public class ShelfedFaceInstance : FaceInstance
    {
        public ShelfedFaceInstance(MainWindow mainWindow, Face faceData)
            : base(mainWindow, faceData)
        {
            FaceImage.Width = ShelfedFaceInstanceSize;
            FaceImage.Height = ShelfedFaceInstanceSize;
            CloseButton.Visibility = Visibility.Visible;
        }
       
        protected override void UserControl_Drop(object sender, DragEventArgs e)
        {
            // ShelfedFaceInstance do nothing when dropped
        }

        protected override void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.mainWindow.RemoveFromFaceListBox(this as ShelfedFaceInstance);
            mainWindow.dragDropManager.HideAdorner();
        }
        protected override void userControl_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            // ShelfedFaceInstance do nothing when drag enter
        }
        protected override void userControl_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            // ShelfedFaceInstance do nothing when drag leave
        }
        public const int ShelfedFaceInstanceSize = 96;
    }
}