using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using FaceReplacement.Engine;
using System.Windows.Media.Animation;

namespace FaceReplacement
{
    /// <summary>
    /// Interaction logic for PhotoCard.xaml
    /// </summary>
    public partial class PhotoCard : UserControl
    {
        public PhotoCard(MainWindow mainWindow)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                Content = new Border()
                {
                    Width = 400,
                    Height = 300,
                    Child = new Label() { Content = "FaceInstance\nin Design Mode" }
                };
            }
            else
            {
                InitializeComponent();
                this.mainWindow = mainWindow;
            }
        }

        public BitmapSource FullPhoto
        {
            get { return this.DataContext as BitmapSource; }
            set
            {
                //value.Freeze();
                this.DataContext = value;
                if (this is WebCamCard)
                {
                    // do nothing for webcamcard
                }
                else
                {
                    using (System.Drawing.Bitmap face = Interop.BitmapSourceToBitmap(value))
                    {
                        Faces = FaceDetector.DetectFace(face);
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (this.Resources["CloseEditingMode"] as System.Windows.Media.Animation.Storyboard).Begin(); 
            foreach (UIElement element in FacesLayer.Children)
            {
                FaceInstance faceInstance = element as FaceInstance;
                if (faceInstance!=null)
                {
                    faceInstance.CurrentMode = FaceInstance.Mode.Normal;
                }
            }
        }

        protected virtual void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // close inside faces
            List<FaceInstance> faceInstances = new List<FaceInstance>();
            foreach (UIElement element in FacesLayer.Children)
            {
                FaceInstance faceInstance = element as FaceInstance;
                if (faceInstance != null)
                {
                    faceInstances.Add(faceInstance);
                }
            }
            FacesLayer.Children.Clear();
            foreach (FaceInstance faceInstance in faceInstances)
            {
                faceInstance.Dispose();
            }
            faceInstances.Clear();

            // close this photo
            this.FullImage.Source = null;
            mainWindow.RemoveFromPhotoListBox(this);
            this.DataContext = null;
            (this.Resources["OpenEditingMode"] as Storyboard).Remove();
            (this.Resources["CloseEditingMode"] as Storyboard).Remove();

            GC.Collect();
        }

        private void EditContourButton_Click(object sender, RoutedEventArgs e)
        {
            (this.Resources["OpenEditingMode"] as System.Windows.Media.Animation.Storyboard).Begin();
            foreach (UIElement element in FacesLayer.Children)
            {
                FaceInstance faceInstance = element as FaceInstance;
                if (faceInstance != null)
                {
                    faceInstance.CurrentMode = FaceInstance.Mode.Editing;
                }
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            // save contour
            foreach (UIElement element in FacesLayer.Children)
            {
                FaceInstance faceInstance = element as FaceInstance;
                if (faceInstance != null)
                {
                    faceInstance.ApplyContour();
                }
            }
            CancelButton_Click(sender, e); // back to Replacement Mode
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const double FXSpareSpace = 1.00;
            const int borderSpace = 40;

            base.OnRender(drawingContext);

            double desiredRatio = grid.ActualHeight * FXSpareSpace / grid.ActualWidth;

            if (mainWindow.PhotoListBox.ActualHeight < grid.ActualHeight * FXSpareSpace + borderSpace)
            {
                FlippingPlane.Width = (mainWindow.PhotoListBox.ActualHeight - borderSpace) / desiredRatio;
                FlippingPlane.Height = mainWindow.PhotoListBox.ActualHeight - borderSpace;
            }
            else
            {
                FlippingPlane.Width = grid.ActualHeight * FXSpareSpace / desiredRatio;
                FlippingPlane.Height = grid.ActualHeight * FXSpareSpace;
            }

            Point3DCollection points = FlippingPlane.Resources["pyramidButtonPts"] as Point3DCollection;

            Point3D topLeft = points[0];
            Point3D topRight = points[1];
            Point3D bottomRight = points[2];
            Point3D bottomLeft = points[3];

            topLeft.Y = grid.ActualHeight / grid.ActualWidth;
            topRight.Y = grid.ActualHeight / grid.ActualWidth;
            bottomRight.Y = -grid.ActualHeight / grid.ActualWidth;
            bottomLeft.Y = -grid.ActualHeight / grid.ActualWidth;

            points[0] = topLeft;
            points[1] = topRight;
            points[2] = bottomRight;
            points[3] = bottomLeft;
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.InvalidateVisual();
        }
        
        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in FacesLayer.Children)
            {
                FaceInstance faceInstance = element as FaceInstance;
                if (faceInstance != null)
                {
                    faceInstance.RevertContour();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (System.Drawing.Bitmap outputBitmap = Interop.BitmapSourceToBitmap(this.FullPhoto))
            {
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(outputBitmap);
                foreach (UIElement element in FacesLayer.Children)
                {
                    FaceInstance faceInstance = element as FaceInstance;
                    if (faceInstance != null)
                    {
                        System.Drawing.Bitmap fullMaskedPhoto = faceInstance.FaceData.GetFullMaskedPhoto();
                        g.DrawImageUnscaled(fullMaskedPhoto, new System.Drawing.Point());
                        fullMaskedPhoto.Dispose(); fullMaskedPhoto = null;
                    }
                }
                g.Dispose(); g = null;

                string pathBackup = System.IO.Directory.GetCurrentDirectory();

                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                const int BMP = 1, JPG = 2, GIF = 3, TIFF = 4, PNG = 5;
                saveFileDialog.Filter = "BMP (*.bmp)|*.bmp|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff|PNG (*.png)|*.png";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.Drawing.Imaging.ImageFormat format = null;
                    switch (saveFileDialog.FilterIndex)
                    {
                        case BMP: format = System.Drawing.Imaging.ImageFormat.Bmp;
                            saveFileDialog.DefaultExt = "*.bmp";
                            break;
                        case JPG: format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            saveFileDialog.DefaultExt = "*.jpg";
                            break;
                        case GIF: format = System.Drawing.Imaging.ImageFormat.Gif;
                            saveFileDialog.DefaultExt = "*.gif";
                            break;
                        case TIFF: format = System.Drawing.Imaging.ImageFormat.Tiff;
                            saveFileDialog.DefaultExt = "*.tiff";
                            break;
                        case PNG: format = System.Drawing.Imaging.ImageFormat.Png;
                            saveFileDialog.DefaultExt = "*.png";
                            break;
                    }
                    System.Drawing.Bitmap saveBitmap = new System.Drawing.Bitmap(outputBitmap);
                    saveBitmap.Save(saveFileDialog.FileName, format);
                    saveBitmap.Dispose(); saveBitmap = null;
                }
                System.IO.Directory.SetCurrentDirectory(pathBackup);
            }
        }
        
        private Face[] Faces
        {
            set
            {
                FacesLayer.Children.Clear();
                foreach (Face face in value)
                {
                    FaceInstance faceInstance = new FaceInstance(mainWindow, face);
                    Canvas.SetTop(faceInstance, face.RegionBox.Top);
                    Canvas.SetLeft(faceInstance, face.RegionBox.Left);
                    faceInstance.AllowDrop = true;
                    FacesLayer.Children.Add(faceInstance);
                }
            }
        }

        protected MainWindow mainWindow;

        protected virtual void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            //do nothing for the base class
        }

        private void userControl_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (UIElement element in this.FacesLayer.Children)
            {
                FaceInstance face = element as FaceInstance;
                if (face != null)
                {
                    face.UserControl_MouseMove(sender, e);
                }
            }
        }
    }
}