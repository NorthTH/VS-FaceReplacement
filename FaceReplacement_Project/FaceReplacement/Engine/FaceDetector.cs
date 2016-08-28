using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace FaceReplacement.Engine
{
    class FaceDetector
    {
        public static Bitmap CropBitmap(Bitmap source,Rectangle region)
        {
            BitmapSource output = new CroppedBitmap(Interop.BitmapToBitmapSource(source), Interop.RectangleToInt32Rect(region));
            return Interop.BitmapSourceToBitmap(output);
        }
        public static Face[] DetectFace(Bitmap fullImage)
        {
            HaarCascade haar = new HaarCascade("Resources/haarcascade_frontalface_alt.xml");

            Image<Bgr, Byte> frame = new Image<Bgr, Byte>(fullImage);
            Image<Gray, byte> grayframe = frame.Convert<Gray, byte>();
            var detectedFaces =
                    grayframe.DetectHaarCascade(
                            haar, 1.1, 4,
                            HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                            new System.Drawing.Size(frame.Width / 16, frame.Height / 16)
                            )[0];
            grayframe.Dispose(); grayframe = null;
            frame.Dispose(); frame = null;

            List<Face> faces = new List<Face>();
            foreach (var face in detectedFaces)
            {
                Bitmap croppedBitmap = CropBitmap(fullImage, face.rect);
                System.Windows.Point[] eyes = DetectEye(croppedBitmap);
                System.Windows.Point mouth = DetectMouth(croppedBitmap);
                if (mouth == new System.Windows.Point(0, 0))
                {
                    Rectangle newRect = face.rect;
                    newRect.Height = face.rect.Height * 110 / 100;
                    croppedBitmap = CropBitmap(fullImage, newRect);
                    mouth = DetectMouth(croppedBitmap);
                }
                croppedBitmap.Dispose(); croppedBitmap = null;

                if (eyes.Length >= 2)
                {
                    System.Windows.Point left = eyes[0];
                    System.Windows.Point right = eyes[0];
                    for (int i = 1; i < eyes.Length; i++)
                    {
                        if (Math.Abs(left.X - (face.rect.Width * 30) / 100) > Math.Abs(eyes[i].X - (face.rect.Width * 30) / 100))
                            left = eyes[i];
                        if (Math.Abs(right.X - (face.rect.Width * 70) / 100) > Math.Abs(eyes[i].X - (face.rect.Width * 70) / 100))
                            right = eyes[i];
                    }
                    if (mouth == new System.Windows.Point())
                        mouth = new System.Windows.Point((left.X + right.X) / 2, (face.rect.Height * 90) / 100);

                    //frame.Draw(face.rect, new Bgr(0, double.MaxValue, 0), 3);
                    Vector regionBoxOffset = new Vector(face.rect.Left, face.rect.Top);
                    Bitmap clonedBitmap = new Bitmap(fullImage);
 
                    faces.Add(new Face(++id, clonedBitmap, clonedBitmap, face.rect, left, right, mouth, true));
                }
            }

            return faces.ToArray();
        }

        public static System.Windows.Point[] DetectEye(Bitmap cropImage)
        {
            using (Image<Bgr, Byte> frame = new Image<Bgr, Byte>(cropImage))
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, (int)(frame.Width * 2.0 / 11.0), (int)frame.Width, (int)(frame.Height * 1.0 / 3.0));
                frame.ROI = rect;
                HaarCascade haar = new HaarCascade("Resources/ojoD.xml");
                Image<Gray, byte> grayframe = frame.Convert<Gray, byte>();
                var eyes = grayframe.DetectHaarCascade(
                                haar, 1.1, 3,
                                HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                                new System.Drawing.Size(frame.Width / 4, frame.Height / 4)
                                )[0];
                grayframe.Dispose(); grayframe = null;

                List<System.Windows.Point> eyePos = new List<System.Windows.Point>();
                foreach (var eye in eyes)
                {
                    eyePos.Add(new System.Windows.Point(eye.rect.X + (eye.rect.Width / 2),
                                        eye.rect.Y + (int)(frame.Width / 5.5) + (eye.rect.Height / 2)));
                }
                if (eyes.Length == 1)
                    eyePos.Add(new System.Windows.Point(rect.Width - (eyes[0].rect.X + (eyes[0].rect.Width / 2)),
                                        eyes[0].rect.Y + (int)(frame.Width / 5.5) + (eyes[0].rect.Height / 2)));

                //frame.ROI = System.Drawing.Rectangle.Empty;

                return eyePos.ToArray();
            }
        }

        public static System.Windows.Point DetectMouth(Bitmap cropImage)
        {
            using (Image<Bgr, Byte> frame = new Image<Bgr, Byte>(cropImage))
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)(frame.Width * 1.0 / 5.0), (int)(frame.Height * 2.0 / 3.0), (int)(frame.Width * 3.0 / 5.0), (int)(frame.Height * 1.0 / 2.0));
                frame.ROI = rect;
                HaarCascade haar = new HaarCascade("Resources/Mouth.xml");
                Image<Gray, byte> grayframe = frame.Convert<Gray, byte>();
                var mouths = grayframe.DetectHaarCascade(
                                haar, 1.1, 3,
                                HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                                new System.Drawing.Size((int)(frame.Width / 1.8), (int)(frame.Height / 1.8))
                                )[0];
                grayframe.Dispose(); grayframe = null;

                if (mouths.Length == 0)
                    return new System.Windows.Point();

                int posX = 0, posY = 0;
                foreach (var mouth in mouths)
                {

                    posX += mouth.rect.X + rect.X + (mouth.rect.Width / 2);
                    posY += mouth.rect.Y + rect.Y + (mouth.rect.Height / 2);
                }
                //frame.ROI = System.Drawing.Rectangle.Empty;

                return new System.Windows.Point(posX / mouths.Length, posY / mouths.Length);
            }
        }
        
        private static int id = 0;
    }
}
