using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace FaceReplacement.Engine
{
    class FaceContourGenerator
    {
        public static bool[,] convertBitmapToMask(System.Drawing.Bitmap bitmap)
        {
            const byte maskThreshold = 127;
            const int columns = 4; // color offsets
            int rows = bitmap.Width * columns;
            bool[,] mask = new bool[bitmap.Width, bitmap.Height];
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* data = (byte*)(bitmapData.Scan0);
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int i = y * rows + x * columns;

                        if (data[i + A] > maskThreshold)
                        {
                            mask[x, y] = true;
                        }
                        else
                        {
                            mask[x, y] = false;
                        }
                    }
                }
            }
            bitmap.UnlockBits(bitmapData);
            return mask;
        }
        public static bool[,] convertMaskToBorder(bool[,] mask)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            bool[,] border = new bool[width, height];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //border[x, y] = false; // default flag

                    if (mask[x, y]) // is inside boundary
                    {
                        // check neighbor pixels

                        //check above pixel
                        if (!mask[x, y - 1])
                        {
                            border[x, y] = true;
                            continue;
                        }
                        //check below pixel
                        if (!mask[x, y + 1])
                        {
                            border[x, y] = true;
                            continue;
                        }
                        //check left pixel
                        if (!mask[x - 1, y])
                        {
                            border[x, y] = true;
                            continue;
                        }
                        //check right pixel
                        if (!mask[x + 1, y])
                        {
                            border[x, y] = true;
                            continue;
                        }
                    }
                }
            }
            return border;
        }
        public static bool FaceFeaturesParser()
        {
            #region read file

            // read file
            string[] lines = System.IO.File.ReadAllLines("Resources/averaged_seam.txt");
            //string[] lines = System.IO.File.ReadAllLines("Resources/seam_trainsetv2.txt");
            List<TrainingSample> samples = new List<TrainingSample>();
            for (int line = 0; line < lines.Length; line++)
            {
                if (lines[line].StartsWith("FILE:: "))
                {
                    TrainingSample sample = new TrainingSample();
                    sample.FileName = lines[line].Remove(0, "FILE:: ".Length);

                    line++;
                    sample.LeftEye = Point.Parse(lines[line].Remove(0, "LEFT:: ".Length));

                    line++;
                    sample.RightEye = Point.Parse(lines[line].Remove(0, "RIGHT:: ".Length));

                    line++;
                    sample.Mouth = Point.Parse(lines[line].Remove(0, "MOUTH:: ".Length));

                    line++;
                    string[] words = lines[line].Remove(0, "SEAM:: ".Length).Split(' ');
                    foreach (string word in words)
                    {
                        sample.ContourFeatures.Add(Point.Parse(word));
                    }
                    samples.Add(sample);
                }
                else if (lines[line].StartsWith("AVERAGED:: "))
                {
                    string[] words = lines[line].Remove(0, "AVERAGED:: ".Length).Split(' ');

                    averagedFeatures.Clear();
                    foreach (string word in words)
                    {
                        averagedFeatures.Add(ContourFeature.Parse(word));
                    }
                    return true;
                }
            }

            #endregion

            #region extract relative properties in case not averaged yet
            List<List<ContourFeature>> SamplesFeatures = new List<List<ContourFeature>>();
            foreach (TrainingSample sample in samples)
            {
                Vector XAxis = new Vector(1, 0);
                Point sampleCenter = new Point((sample.LeftEye.X + sample.RightEye.X) / 2, (sample.LeftEye.Y + sample.RightEye.Y) / 2);
                Vector horizontalVector = sample.RightEye - sample.LeftEye;
                Vector verticalVector = (sample.Chin - sample.Forehead) + (sample.Mouth - sampleCenter);
                Vector baseDirectionVector = new Vector(horizontalVector.X + verticalVector.Y, horizontalVector.Y + verticalVector.X); // average of horizontalVector and verticalVector (not concern the length)
                double degreeAngle = Vector.AngleBetween(XAxis, baseDirectionVector);

                Matrix rotateToXYAxis = new Matrix();
                rotateToXYAxis.RotateAt(-degreeAngle, sampleCenter.X, sampleCenter.Y);
                Point[] features = sample.ContourFeatures.ToArray();
                rotateToXYAxis.Transform(features);

                double relativeWidth = horizontalVector.Length;
                double relativeHeight = (sample.Mouth - sampleCenter).Length;

                List<ContourFeature> ContourFeatures = new List<ContourFeature>();
                for (int featureIndex = 0; featureIndex < features.Length; featureIndex++)
                {
                    ContourFeature feature = new ContourFeature();
                    Vector v = features[featureIndex] - sampleCenter;
                    v.Y /= relativeHeight / relativeWidth;

                    feature.RelativeMagnitude = v.Length / relativeWidth;
                    feature.RelativeRadianAngle = Vector.AngleBetween(XAxis, v) / 180.0 * Math.PI;

                    // within -0.5PI to 1.5PI
                    while (feature.RelativeRadianAngle < -0.5 * Math.PI) feature.RelativeRadianAngle += 2.0 * Math.PI;
                    while (feature.RelativeRadianAngle > 1.5 * Math.PI) feature.RelativeRadianAngle -= 2.0 * Math.PI;

                    ContourFeatures.Add(feature);
                }
                SamplesFeatures.Add(ContourFeatures);
            }

            if (SamplesFeatures.Count == 0)
            {
                return false;
            }
            else
            {
                #region find average for each feature
                for (int feature = 0; feature < TrainingSample.TotalContourFeatures; feature++)
                {
                    List<double> featureRadianAngles = new List<double>();
                    List<double> featureMagnitudes = new List<double>();
                    for (int sample = 0; sample < SamplesFeatures.Count; sample++)
                    {
                        // rearrange by feature
                        featureRadianAngles.Add(SamplesFeatures[sample][feature].RelativeRadianAngle);
                        featureMagnitudes.Add(SamplesFeatures[sample][feature].RelativeMagnitude);
                    }
                    averagedFeatures.Add(new ContourFeature()
                    {
                        RelativeRadianAngle = featureRadianAngles.Average(),
                        RelativeMagnitude = featureMagnitudes.Average()
                    });
                }
                #endregion

                #region find average between left and right side
                const int top = 0, bottom = TrainingSample.TotalContourFeatures / 2;
                for (int feature = top + 1; feature < bottom; feature++)
                {
                    int oppositeFeature = TrainingSample.TotalContourFeatures - feature;
                    double RelativeRadianAngle = (averagedFeatures[feature].RelativeRadianAngle + (Math.PI - averagedFeatures[oppositeFeature].RelativeRadianAngle)) / 2.0;
                    double RelativeMagnitude = (averagedFeatures[feature].RelativeMagnitude + averagedFeatures[oppositeFeature].RelativeMagnitude) / 2.0;
                    averagedFeatures[feature].RelativeRadianAngle = RelativeRadianAngle;
                    averagedFeatures[feature].RelativeMagnitude = RelativeMagnitude;
                    averagedFeatures[oppositeFeature].RelativeRadianAngle = Math.PI - RelativeRadianAngle;
                    averagedFeatures[oppositeFeature].RelativeMagnitude = RelativeMagnitude;
                }

                averagedFeatures[top].RelativeRadianAngle = -0.5 * Math.PI;
                averagedFeatures[bottom].RelativeRadianAngle = 0.5 * Math.PI;
                #endregion
            }
            #endregion

            return true;
        }
        public static PointCollection GetContourPoints(Point leftEye, Point rightEye, Point mouth)
        {
            Point[] points = new Point[TrainingSample.TotalContourFeatures];

            if (averagedFeatures == null)
            {
                averagedFeatures = new List<ContourFeature>();
                if (!FaceFeaturesParser())
                {
                    averagedFeatures = null;
                    throw new Exception("Features parsing failed");
                }
            }

            Point faceCenter = new Point((rightEye.X + leftEye.X) / 2.0, (rightEye.Y + leftEye.Y) / 2.0);
            Vector baseVector = rightEye - leftEye;
            double featuresWidth = baseVector.Length;
            double featuresHeight = (mouth - faceCenter).Length;

            for (int featureIndex = 0; featureIndex < TrainingSample.TotalContourFeatures; featureIndex++)
            {
                double radianAngle = averagedFeatures[featureIndex].RelativeRadianAngle;
                Vector featureVector = new Vector(
                    averagedFeatures[featureIndex].RelativeMagnitude * featuresWidth * Math.Cos(radianAngle),
                    averagedFeatures[featureIndex].RelativeMagnitude * featuresWidth * Math.Sin(radianAngle)
                    );
                featureVector.Y *= featuresHeight / featuresWidth;
                points[featureIndex] = faceCenter + featureVector;
            }

            Vector XAxis = new Vector(1, 0);
            double baseRadianAngle = Vector.AngleBetween(XAxis, baseVector) / 180.0 * Math.PI;
            Matrix rotateFromXYAxes = new Matrix();
            rotateFromXYAxes.RotateAt(baseRadianAngle * 180.0 / Math.PI, faceCenter.X, faceCenter.Y);
            rotateFromXYAxes.Transform(points);

            PointCollection output = new PointCollection();
            foreach (Point p in points) { output.Add(p); }
            return output;
        }
        public static bool[,] RenderBooleanMask(System.Drawing.PointF[] faceContour, System.Drawing.Size size, System.Drawing.Drawing2D.Matrix transformMatrix)
        {
            transformMatrix.TransformPoints(faceContour);

            System.Drawing.Bitmap maskBitmap = RenderMask(faceContour, size.Width, size.Height);
            bool[,] mask = convertBitmapToMask(maskBitmap);
            maskBitmap.Dispose(); maskBitmap = null;

            return mask;
        }
        public static System.Drawing.Bitmap RenderMask(System.Drawing.PointF[] faceContour, int width, int height)
        {
            System.Drawing.Bitmap mask = new System.Drawing.Bitmap(width, height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(mask);
            g.FillPolygon(System.Drawing.Brushes.White, faceContour);
            g.Dispose(); g = null;
            return mask;
        }

        private static List<ContourFeature> averagedFeatures;
        private const int A = 3, R = 2, G = 1, B = 0; // ordered as Format32bppArgb byte offset
    }
}