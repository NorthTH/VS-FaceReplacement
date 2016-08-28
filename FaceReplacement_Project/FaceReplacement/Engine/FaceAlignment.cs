using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FaceReplacement.Engine
{
    class FaceAlignment
    {
        public static Bitmap Align(Face targetHead, Face sourceFace, System.Drawing.Drawing2D.Matrix transformMatrix)
        {
            Bitmap transformedFacePatch = new Bitmap(targetHead.OriginalPhoto.Width, targetHead.OriginalPhoto.Height);
            Graphics g = Graphics.FromImage(transformedFacePatch);
            g.Transform = transformMatrix;
            g.DrawImage(sourceFace.OriginalPhoto, new PointF(0, 0));
            g.Dispose(); g = null;

            return transformedFacePatch;
        }

        public static Matrix GetTransformMatrix(Face targetHead, Face sourceFace)
        {
            double rotateAngle = targetHead.BaseDirectionAngle - sourceFace.BaseDirectionAngle;
            double scaleRatioX = targetHead.ResolutionX / sourceFace.ResolutionX;
            double scaleRatioY = targetHead.ResolutionY / sourceFace.ResolutionY;

            Matrix transformMatrix = new Matrix();
            transformMatrix.Translate((float)targetHead.AbsoluteFacePivotPosition.X, (float)targetHead.AbsoluteFacePivotPosition.Y); // fourth
            transformMatrix.Rotate((float)rotateAngle); // third
            transformMatrix.Scale((float)scaleRatioX, (float)scaleRatioY); // second
            transformMatrix.Translate((float)-sourceFace.AbsoluteFacePivotPosition.X, (float)-sourceFace.AbsoluteFacePivotPosition.Y); // first

            return transformMatrix;
        }
    }
}
