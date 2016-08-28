using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using System.Windows.Threading;
using Emgu.CV.Structure;
using System.Windows;

namespace FaceReplacement
{
    class WebCamCard : PhotoCard
    {
        public WebCamCard(MainWindow mainWindow)
            : base(mainWindow)
        {
            InitializeComponent();
            cap = new Capture(0);
            dispatcherTimer = new DispatcherTimer();
            Loaded += new System.Windows.RoutedEventHandler(WebCamCard_Loaded);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            using (Image<Bgr, byte> nextFrame = cap.QueryFrame())
            {
                if (nextFrame != null)
                {
                    FullPhoto = Interop.IImageToBitmapSource(nextFrame);
                }
            }
        }
        
        private void WebCamCard_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SaveButton.Visibility = Visibility.Hidden;
            this.CaptureButton.Visibility = System.Windows.Visibility.Visible;
            this.EditContourButton.Visibility = System.Windows.Visibility.Hidden;
            this.EditingToolPanel.Visibility = System.Windows.Visibility.Hidden;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / 12.0); // 12 frames per seconds
            dispatcherTimer.Start();
        }
        protected override void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.RemoveFromPhotoListBox(this);
            cap.Dispose(); 
        }
        protected override void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            PhotoCard newPhotoCard = new PhotoCard(mainWindow) { FullPhoto = this.FullPhoto };
            int index = mainWindow.PhotoListBox.Items.IndexOf(this);
            mainWindow.RemoveFromPhotoListBox(index);
            mainWindow.AddToPhotoListBox(index, newPhotoCard);
            cap.Dispose(); 
        }
        private Capture cap;
        
        private DispatcherTimer dispatcherTimer;
    }
}