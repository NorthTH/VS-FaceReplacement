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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using Emgu.CV.Structure;
using Emgu.CV;
using FaceReplacement.Engine;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace FaceReplacement
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            photoList = new ObservableCollection<PhotoCard>();
            faceList = new ObservableCollection<FaceInstance>();
            PhotoListBox.ItemsSource = photoList;
            FaceListBox.ItemsSource = faceList;
        }

        public void AddToFaceListBox(ShelfedFaceInstance shelfedFaceInstance)
        {
            faceList.Add(shelfedFaceInstance);
            FaceListBox.ScrollIntoView(shelfedFaceInstance);
            (this.Resources["CollectFacesBlink"] as System.Windows.Media.Animation.Storyboard).Stop();
        }
        
        public void AddToPhotoListBox(PhotoCard photoCard)
        {
            AddToPhotoListBox(photoList.Count, photoCard);
        }
        
        public void AddToPhotoListBox(int index, PhotoCard photoCard)
        {
            photoList.Insert(index, photoCard);
            PhotoListBox.ScrollIntoView(photoCard);
            (this.Resources["PlacePhotosBlink"] as System.Windows.Media.Animation.Storyboard).Stop();
            if (photoList.Count == 1 && faceList.Count == 0)
            {
                (this.Resources["CollectFacesBlink"] as System.Windows.Media.Animation.Storyboard).Begin();
            }
        }
        
        private void AdjustColorCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ColorTransfer.DoColorTransfer = (bool)AdjustColorCheckBox.IsChecked;
            if (PreserveContrastCheckBox != null && LabCheckBox != null && LuminanceCheckBox != null)
            {
                if (ColorTransfer.DoColorTransfer)
                {
                    PreserveContrastCheckBox.Visibility = Visibility.Visible;
                    LabCheckBox.Visibility = Visibility.Visible;
                    LuminanceCheckBox.Visibility = ((bool)LabCheckBox.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    PreserveContrastCheckBox.Visibility = Visibility.Collapsed;
                    LabCheckBox.Visibility = Visibility.Collapsed;
                    LuminanceCheckBox.Visibility = Visibility.Collapsed;
                }
            }
        }
        
        private void CloseHelpButton_Click(object sender, RoutedEventArgs e)
        {
            (this.Resources["CloseHelp"] as System.Windows.Media.Animation.Storyboard).Begin();
        }
        
        private void FaceListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Face)))
            {
                dragDropManager.ShowAdorner(sender, e);
            }
        }
        
        private void FaceListBox_DragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Face)))
            {
                dragDropManager.HideAdorner(sender, e);
            }
        }
        
        private void FaceListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Face)))
            {
                Face face = e.Data.GetData(typeof(Face)) as Face;

                // prevent adding duplicated facee
                bool isDuplicated = false;
                foreach (FaceInstance faceInstance in faceList)
                {
                    if (faceInstance.FaceData.Id == face.Id)
                    {
                        isDuplicated = true;
                        break;
                    }
                }
                if (!isDuplicated && face.IsOriginal)
                {
                    ShelfedFaceInstance shelfedFaceInstance = new ShelfedFaceInstance(this, face);
                    AddToFaceListBox(shelfedFaceInstance);
                }
            }
        }
        
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            (this.Resources["OpenHelp"] as System.Windows.Media.Animation.Storyboard).Begin();
        }
        
        private void LabCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ColorTransfer.useLabColorSpace = (bool)LabCheckBox.IsChecked;
            if (LabCheckBox != null)
            {
                LuminanceCheckBox.Visibility = ((bool)LabCheckBox.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        private void loadFile(string fileName)
        {
            BitmapSource photo = null;
            try
            {
                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fileName))
                {
                    photo = Interop.BitmapToBitmapSource(bitmap);
                }
            }
            catch
            {
                MessageBox.Show(fileName + " cannot be loaded.");
            }
            if (photo != null)
            {
                PhotoCard photoCard = new PhotoCard(this) { FullPhoto = photo };
                AddToPhotoListBox(photoCard);
            }
        }
        
        private void LuminanceCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ColorTransfer.onlyLabLuminance = (bool)LuminanceCheckBox.IsChecked;
        }
        
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            GradientStopCollection gradientStops = new GradientStopCollection();
            if (this.WindowState == WindowState.Maximized)
            {
                ToolBar.CornerRadius = new CornerRadius(0.0);
                TranslucentBackground.CornerRadius = new CornerRadius(0.0);

                gradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0x00, 0x00), 0.0));
                gradientStops.Add(new GradientStop(Color.FromRgb(0x28, 0x30, 0x38), 1.0));
            }
            else if (this.WindowState == WindowState.Normal)
            {
                ToolBar.CornerRadius = new CornerRadius(4.0, 4.0, 0.0, 0.0);
                TranslucentBackground.CornerRadius = new CornerRadius(4.0);

                gradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x00, 0x00), 0.0));
                gradientStops.Add(new GradientStop(Color.FromArgb(0x80, 0x00, 0x10, 0x20), 1.0));
            }
            GradientBrush bg = new LinearGradientBrush(gradientStops)
            {
                StartPoint = new Point(0.5, 0.0),
                EndPoint = new Point(0.5, 1.0)
            };

            TranslucentBackground.Background = bg;
        }
        
        private void NextHelp_Click(object sender, RoutedEventArgs e)
        {
            helpIndex++;
            helpIndex %= 7;
            HelpContent.Source = Application.Current.Resources["h" + (helpIndex + 1)] as BitmapSource;
        }
        
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            string pathBackup = System.IO.Directory.GetCurrentDirectory();
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image files (*.bmp, *.jpg, *.gif, *.png)|*.bmp;*.jpg;*.gif;*.png";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && openFileDialog.CheckFileExists)
            {
                System.IO.Directory.SetCurrentDirectory(pathBackup);
                foreach (string fileName in openFileDialog.FileNames)
                {
                    loadFile(fileName);
                }
            }
        }
        
        private void OpenCameraButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in PhotoListBox.Items)
            {
                if (element is WebCamCard)
                {
                    MessageBox.Show("You are allowed to use only one camera at a time.", "Face Replacement", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                    return;
                }
            }
            try
            {
                photoList.Add(new WebCamCard(this));
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("There is no camera supported by this program.", "Face Replacement", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }
        
        private void PhotoListBox_Drop(object sender, DragEventArgs e)
        {
            DataObject data = e.Data as DataObject;
            if (data.ContainsFileDropList())
            {
                foreach (string fileName in data.GetFileDropList())
                {
                    loadFile(fileName);
                }
            }
        }
        
        private void PreferenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreferencePanel.Visibility == Visibility.Visible)
            {
                (this.Resources["ClosePreferences"] as System.Windows.Media.Animation.Storyboard).Begin();
            }
            else
            {
                (this.Resources["OpenPreferences"] as System.Windows.Media.Animation.Storyboard).Begin();
            }
        }
        
        private void PreserveContrastCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ColorTransfer.CloneColorDistribution = (bool)PreserveContrastCheckBox.IsChecked;
        }
        
        private void PreviousHelp_Click(object sender, RoutedEventArgs e)
        {
            helpIndex += 13;
            helpIndex %= 7;
            HelpContent.Source = Application.Current.Resources["h" + (helpIndex + 1)] as BitmapSource;
        }
        
        public void RemoveFromFaceListBox(ShelfedFaceInstance shelfedFaceInstance)
        {
            faceList.Remove(shelfedFaceInstance);
            if (faceList.Count == 0 && photoList.Count > 0)
            {
                (this.Resources["CollectFacesBlink"] as System.Windows.Media.Animation.Storyboard).Begin();
            }
        }
        
        public void RemoveFromPhotoListBox(int index)
        {
            photoList.RemoveAt(index);
        }
        
        public void RemoveFromPhotoListBox(PhotoCard photoCard)
        {
            photoList.Remove(photoCard);
            if (photoList.Count == 0)
            {
                (this.Resources["CollectFacesBlink"] as System.Windows.Media.Animation.Storyboard).Stop();
                (this.Resources["PlacePhotosBlink"] as System.Windows.Media.Animation.Storyboard).Begin();
            }
        }
        
        private void TuneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TuneSlider.Value == 0.0)
            {
                FaceBlender.Clone = new BitmapProcessingDelegate(FaceBlender.mvcClone);
            }
            else
            {
                FaceBlender.Clone = new BitmapProcessingDelegate(FaceBlender.poissonClone);
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
                TranslucentBackground.Visibility = Visibility.Visible;
                this.StateChanged += new EventHandler(MainWindow_StateChanged);
                MainWindow_StateChanged(sender, e);
            }
            catch (DllNotFoundException) // If Aero is not supported
            {
                GradientStopCollection gradientStops = new GradientStopCollection();
                gradientStops.Add(new GradientStop(Colors.White, 0.0));
                gradientStops.Add(new GradientStop(Color.FromRgb(0xC8, 0xF0, 0xF8), 0.25));
                gradientStops.Add(new GradientStop(Color.FromRgb(0x90, 0xC0, 0xF0), 0.5));
                gradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0x40, 0x80), 1.0));

                RadialGradientBrush bg = new RadialGradientBrush(gradientStops);
                bg.Center = new Point(0.5, 0.75);
                bg.GradientOrigin = new Point(0.5, 1.0);
                bg.RadiusX = 1.0;
                bg.RadiusY = 1.0;
                ToolBar.CornerRadius = new CornerRadius(0.0);
                this.Background = bg;
            }
            (this.Resources["PlacePhotosBlink"] as System.Windows.Media.Animation.Storyboard).Begin();
            dragDropManager = new DragDropManager(this.MainLayout);
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (UIElement element in PhotoListBox.Items)
            {
                element.InvalidateVisual();
            }
        }

        public List<FaceInstance> AllFaceInstances
        {
            get
            {
                List<FaceInstance> allFaceInstances = new List<FaceInstance>();
                allFaceInstances.AddRange(faceList);
                foreach (PhotoCard photo in photoList)
                {
                    foreach (UIElement element in photo.FacesLayer.Children)
                    {
                        FaceInstance faceInstance = element as FaceInstance;
                        if (faceInstance != null)
                        {
                            allFaceInstances.Add(faceInstance);
                        }
                    }
                }
                return allFaceInstances;
            }
        }

        public DragDropManager dragDropManager;

        private ObservableCollection<FaceInstance> faceList;

        private int helpIndex;

        private ObservableCollection<PhotoCard> photoList;
    }
}