using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace FaceReplacement.Engine
{
    class TrainingSample
    {
        public TrainingSample()
        {
            ContourFeatures = new PointCollection();
        }

        public Point Chin
        {
            get { return ContourFeatures[TotalContourFeatures / 2]; }
        }

        public Point Forehead
        {
            get { return ContourFeatures[0]; }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("FILE:: " + FileName + "\r\n");
            output.Append("LEFT:: " + LeftEye.ToString() + "\r\n");
            output.Append("RIGHT:: " + RightEye.ToString() + "\r\n");
            output.Append("MOUTH:: " + Mouth.ToString() + "\r\n");
            output.Append("SEAM:: ");
            foreach (Point feature in ContourFeatures)
            {
                output.Append(feature.ToString() + " ");
            }
            return output.ToString().Trim();
        }

        public PointCollection ContourFeatures;
        
        public string FileName;

        public Point Mouth;

        public Point LeftEye;

        public Point RightEye;

        public const int TotalContourFeatures = 16;
    }
}
