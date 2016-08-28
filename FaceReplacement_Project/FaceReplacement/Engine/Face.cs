using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.ComponentModel;

namespace FaceReplacement.Engine
{
    public class Face : DependencyObject, IDisposable
    {
        public Face(int id, Bitmap fullPhoto, Bitmap displayPhoto, System.Drawing.Rectangle regionBox, System.Windows.Point relativeLeftEyePosition, System.Windows.Point relativeRightEyePosition, System.Windows.Point relativeMouthPosition, bool IsOriginal)
        {
            this.id = id;
            this.originalPhoto = fullPhoto;
            this.displayPhoto = displayPhoto;
            this.RegionBox = regionBox;
            this.relativeLeftEyePosition = relativeLeftEyePosition;
            this.relativeRightEyePosition = relativeRightEyePosition;
            this.relativeMouthPosition = relativeMouthPosition;
            this.IsOriginal = IsOriginal;
            Thumbnail = getThumbnail();
        }

        public Face CloneOriginal()
        {
            Bitmap clonedPhoto = new Bitmap(this.originalPhoto);
            Face output = new Face(this.id, clonedPhoto, clonedPhoto, this.RegionBox, this.relativeLeftEyePosition, this.relativeRightEyePosition, this.relativeMouthPosition, true);
            return output;
        }
        public Bitmap GetFullMaskedPhoto()
        {
            Bitmap image = new Bitmap((int)this.originalPhoto.Width, (int)this.originalPhoto.Height);
            Graphics g = Graphics.FromImage(image);
            g.DrawImage(this.displayPhoto, new PointF(0, 0));
            g.Dispose(); g = null;
            using (Bitmap mask = FaceContourGenerator.RenderMask(Interop.WPFPointCollectionToPointFArray(AbsoluteContour), (int)this.originalPhoto.Width, (int)this.originalPhoto.Height))
            {
                System.Drawing.Imaging.BitmapData dataBitmap = image.LockBits(new System.Drawing.Rectangle(0, 0, (int)image.Width, (int)image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData dataMask = mask.LockBits(new System.Drawing.Rectangle(0, 0, (int)image.Width, (int)image.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                const int alphaChannel = 3, channels = 4;
                unsafe
                {
                    byte* imgMask = (byte*)(dataMask.Scan0);
                    byte* imgBitmap = (byte*)(dataBitmap.Scan0);
                    for (int i = 0; i < dataBitmap.Height; i++)
                    {
                        for (int j = 0; j < dataBitmap.Width; j++)
                        {
                            imgBitmap[alphaChannel] = (byte)(imgBitmap[alphaChannel] * (imgMask[alphaChannel] / 255));

                            imgMask += channels;
                            imgBitmap += channels;
                        }
                    }
                }
                mask.UnlockBits(dataMask);
                image.UnlockBits(dataBitmap);
            }
            return image;
        }
        public void InvalidateContour()
        {
            contour = null;
        }
        public void RestoreOriginal()
        {
            if (!this.IsOriginal)
            {
                MessageBoxResult response = MessageBox.Show("Do you want to restore this face to original face", "Face Replacement", MessageBoxButton.YesNo);
                if (response == MessageBoxResult.Yes)
                {
                    this.displayPhoto = this.originalPhoto;
                    this.IsOriginal = true;
                }
                UpdateThumbnail();
            }
        }
        public void UpdateThumbnail()
        {
            Thumbnail = getThumbnail();
        }
        private BitmapSource getThumbnail()
        {
            System.Windows.Point maxX = RelativeContour[0];
            System.Windows.Point maxY = RelativeContour[0];
            foreach (System.Windows.Point p in RelativeContour)
            {
                if (p.X > maxX.X)
                {
                    maxX = p;
                }
                if (p.Y > maxY.Y)
                {
                    maxY = p;
                }
            }

            Int32Rect croppingBox = new Int32Rect(this.RegionBox.X, this.RegionBox.Y, (int)Math.Max(maxX.X, RegionBox.Width), (int)Math.Max(maxY.Y, RegionBox.Height));
            Bitmap fullMaskedPhoto = this.GetFullMaskedPhoto();

            BitmapSource output = new CroppedBitmap(Interop.BitmapToBitmapSource(fullMaskedPhoto), croppingBox);
            fullMaskedPhoto.Dispose(); fullMaskedPhoto = null;
            return output;
        }

        public PointCollection AbsoluteContour
        {
            get { return FaceContourGenerator.GetContourPoints(AbsoluteLeftEyePosition, AbsoluteRightEyePosition, AbsoluteMouthPosition); }
        }
        public System.Windows.Point AbsoluteFacePivotPosition
        {
            get { return RelativeFacePivotPosition + RegionOffset; }
        }
        public System.Windows.Point AbsoluteLeftEyePosition
        {
            get { return relativeLeftEyePosition + this.RegionOffset; }
            set
            {
                this.relativeLeftEyePosition = value - this.RegionOffset;
                InvalidateContour();
            }
        }
        public System.Windows.Point AbsoluteMouthPosition
        {
            get { return relativeMouthPosition + this.RegionOffset; }
            set
            {
                this.relativeMouthPosition = value - this.RegionOffset;
                InvalidateContour();
            }
        }
        public System.Windows.Point AbsoluteRightEyePosition
        {
            get { return relativeRightEyePosition + this.RegionOffset; }
            set
            {
                this.relativeRightEyePosition = value - this.RegionOffset;
                InvalidateContour();
            }
        }

        // Returns:
        //     The angle, in degrees, from horizontal line.
        public double BaseDirectionAngle
        {
            get
            {
                Vector XAxis = new Vector(1, 0);
                return Vector.AngleBetween(XAxis, (relativeRightEyePosition - relativeLeftEyePosition) /*+ (Mouth - Center)*/);
            }
        }
        public Bitmap DisplayPhoto
        {
            set
            {
                this.displayPhoto = value;
            }
        }
        public int Id
        {
            get { return id; }
        }
        public Bitmap OriginalPhoto
        {
            get { return originalPhoto; }
        }
        public Vector RegionOffset
        {
            get
            {
                return new Vector(this.RegionBox.Left, this.RegionBox.Top);
            }
        }
        public PointCollection RelativeContour
        {
            get
            {
                if (contour == null)
                {
                    contour = FaceContourGenerator.GetContourPoints(relativeLeftEyePosition, relativeRightEyePosition, relativeMouthPosition);
                }
                return contour;
            }
        }
        public System.Windows.Point RelativeFacePivotPosition
        {
            get { return new System.Windows.Point((relativeLeftEyePosition.X + relativeRightEyePosition.X) / 2, (relativeLeftEyePosition.Y + relativeRightEyePosition.Y) / 2); }
        }
        public System.Windows.Point RelativeLeftEyePosition
        {
            get { return relativeLeftEyePosition; }
            set
            {
                this.relativeLeftEyePosition = value;
                InvalidateContour();
            }
        }
        public System.Windows.Point RelativeMouthPosition
        {
            get { return relativeMouthPosition; }
            set
            {
                this.relativeMouthPosition = value;
                InvalidateContour();
            }
        }
        public System.Windows.Point RelativeRightEyePosition
        {
            get { return relativeRightEyePosition; }
            set
            {
                this.relativeRightEyePosition = value;
                InvalidateContour();
            }
        }
        public double ResolutionX
        {
            get { return (relativeRightEyePosition - relativeLeftEyePosition).Length; }
        }
        public double ResolutionY
        {
            get { return (relativeMouthPosition - RelativeFacePivotPosition).Length; }
        }
        public BitmapSource Thumbnail
        {
            get { return (BitmapSource)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Thumbnail.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(BitmapSource), typeof(Face), new UIPropertyMetadata(new BitmapImage()));

        public bool IsOriginal;
        public System.Drawing.Rectangle RegionBox;
        
        private PointCollection contour;
        private Bitmap displayPhoto;
        private int id;
        private Bitmap originalPhoto;
        private System.Windows.Point relativeLeftEyePosition;
        private System.Windows.Point relativeMouthPosition;
        private System.Windows.Point relativeRightEyePosition;

        #region IDisposable Members

        public void Dispose()
        {
            this.originalPhoto.Dispose(); this.originalPhoto = null;
            this.displayPhoto.Dispose(); this.displayPhoto = null;
        }

        #endregion
    }
}