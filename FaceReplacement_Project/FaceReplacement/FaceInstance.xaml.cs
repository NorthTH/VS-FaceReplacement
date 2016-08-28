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
using FaceReplacement.Engine;
using System.Threading;
using System.Drawing;
namespace FaceReplacement
{
    /// <summary>
    /// Interaction logic for FaceInstance.xaml
    /// </summary>
    public partial class FaceInstance : UserControl,IDisposable
    {
        public FaceInstance(MainWindow mainWindow, Face faceData)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                Content = new Border()
                {
                    Width = 96,
                    Height = 96,
                    Child = new Label() { Content = "FaceInstance\nin Design Mode" }
                };
            }
            else
            {
                InitializeComponent();
                this.mainWindow = mainWindow;
                this.FaceData = faceData.CloneOriginal();
            }
        }
        private Face faceData;

        public Face FaceData
        {
            get
            {
                return faceData;
            }
            set
            {
                faceData = value;
                this.DataContext = value;
            }
        }

        #region Drag and Drop

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragged = true;
            if (CurrentMode == Mode.Editing)
            {
                Contour.Contour_MouseMove(sender, e);
            }
        }

        public void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (CurrentMode == Mode.Normal && isDragged)
            {
                Vector dragged = mainWindow.dragDropManager.Dragged;
                if (Math.Abs(dragged.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(dragged.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    isDragged = false;
                    mainWindow.dragDropManager.DoDragDrop(this, this.faceData);
                    isDragged = false;
                }
            }
            else if (CurrentMode == Mode.Editing && isDragged)
            {
                Contour.Contour_MouseMove(sender, e);
            }
        }

        protected virtual void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Face)))
            {
                // do combining
                Face sourceFace = e.Data.GetData(typeof(Face)) as Face;
                if (sourceFace.IsOriginal && (sourceFace != this.faceData))
                {
                    Face targetHead = this.faceData;
                    foreach (FaceInstance faceInstance in mainWindow.AllFaceInstances)
                    {
                        if (faceInstance.DataContext == sourceFace)
                        {
                            faceInstance.IsWaiting = true;
                        }
                        if (faceInstance.DataContext == targetHead)
                        {
                            faceInstance.IsWaiting = true;
                        }
                    }
                    Combine(sourceFace, targetHead);
                }
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragged = false;
            Contour.Contour_MouseUp(sender, e);
        }

        private bool isDragged = false;

        #endregion

        private void FaceImage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is Face)
            {
                if (this is ShelfedFaceInstance)
                {
                    // do nothing
                }
                else
                {
                    Contour.RelativeLeftEyePosition = faceData.RelativeLeftEyePosition;
                    Contour.RelativeRightEyePosition = faceData.RelativeRightEyePosition;
                    Contour.RelativeMouthPosition = faceData.RelativeMouthPosition;

                    Contour.Points = faceData.RelativeContour;
                    this.Visibility = Visibility.Visible;
                }
            }
        }

        private void Combine(Face sourceFace, Face targetHead)
        {
            Thread thread = new Thread(delegate()
            {
                ReplaceWith(targetHead, sourceFace);
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new FinishCombineDelegate(FinishCombine), sourceFace, targetHead);
            });
            thread.Start();
        }
        public static void ReplaceWith(Face targetHead, Face sourceFace)
        {
            DateTime start = DateTime.Now;
            System.Drawing.Drawing2D.Matrix transformMatrix = FaceAlignment.GetTransformMatrix(targetHead, sourceFace);

            //Alignment
            System.Drawing.Bitmap transformedFacePatch = FaceAlignment.Align(targetHead, sourceFace, transformMatrix);

            //Contour Rendering
            bool[,] mask = FaceContourGenerator.RenderBooleanMask(Interop.WPFPointCollectionToPointFArray(sourceFace.AbsoluteContour), targetHead.OriginalPhoto.Size, transformMatrix);

            //Color Adjustment
            transformedFacePatch = ColorTransfer.Adjust(targetHead, transformedFacePatch, mask);

            //Blending
            Bitmap newBlendedImage = FaceBlender.Clone(targetHead.OriginalPhoto, mask, transformedFacePatch);
            transformedFacePatch.Dispose(); transformedFacePatch = null;

            //Display output
            targetHead.DisplayPhoto = newBlendedImage;
            targetHead.IsOriginal = false;

            DateTime stop = DateTime.Now;
            double duration = (stop - start).TotalSeconds;
        }

        public void FinishCombine(Face sourceFace, Face targetHead)
        {
            foreach (FaceInstance faceInstance in mainWindow.AllFaceInstances)
            {
                if (faceInstance.DataContext == sourceFace || faceInstance.DataContext == targetHead)
                {
                    faceInstance.IsWaiting = false;
                }
            }
            targetHead.UpdateThumbnail();
        }

        public void ApplyContour()
        {
            faceData.RelativeLeftEyePosition = Contour.RelativeLeftEyePosition;
            faceData.RelativeRightEyePosition = Contour.RelativeRightEyePosition;
            faceData.RelativeMouthPosition = Contour.RelativeMouthPosition;
        }

        public void RevertContour()
        {
            Contour.RelativeLeftEyePosition = faceData.RelativeLeftEyePosition;
            Contour.RelativeRightEyePosition = faceData.RelativeRightEyePosition;
            Contour.RelativeMouthPosition = faceData.RelativeMouthPosition;
            Contour.Points = faceData.RelativeContour;
        }

        public Mode CurrentMode
        {
            get { return currentMode; }
            set
            {
                currentMode = value;
                switch (currentMode)
                {
                    case Mode.Editing:
                        Contour.TriggerEditingMode();
                        ApplyContour(); // create contour backup
                        this.AllowDrop = false;
                        break;
                    case Mode.Normal:
                        Contour.TriggerNormalMode();
                        RevertContour(); // restore contour backup
                        this.AllowDrop = true;
                        break;
                }
            }
        }

        public enum Mode
        {
            Normal, // face replacement
            Editing // contour editing
        }

        protected MainWindow mainWindow;

        private Mode currentMode;

        public bool IsWaiting
        {
            get
            {
                return this.WaitingLayer.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    this.WaitingLayer.Visibility = Visibility.Visible;
                    this.IsHitTestVisible = false;
                }
                else
                {
                    this.WaitingLayer.Visibility = Visibility.Hidden;
                    this.IsHitTestVisible = true;
                }
            }
        }


        protected virtual void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // do nothing for FaceInstance
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (currentMode == Mode.Normal)
            {
                Contour.ShowContour();
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (currentMode == Mode.Normal)
            {
                Contour.HideContour();
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.faceData.RestoreOriginal();
        }


        protected virtual void userControl_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Face)))
            {
                mainWindow.dragDropManager.ShowAdorner(sender, e);
            }
        }

        protected virtual void userControl_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Face)))
            {
                mainWindow.dragDropManager.HideAdorner(sender,e);
            }
        }

        private void userControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
        }

        #region IDisposable Members

        public void Dispose()
        {   
            Face data = this.faceData;
            this.FaceData = null;
            data.Dispose(); data = null;
        }

        #endregion
    }

    delegate void FinishCombineDelegate(Face sourceFace, Face targetHead);
}