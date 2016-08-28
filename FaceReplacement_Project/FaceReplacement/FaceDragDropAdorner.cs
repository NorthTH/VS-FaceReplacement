using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using FaceReplacement.Engine;

namespace FaceReplacement
{
    public class FaceDragDropAdorner : Adorner
    {
        Face previewFace;
        ImageSource previewImage;
        Rect drawingRegion;
        public FaceDragDropAdorner(UIElement adornedElement,Face face)
            : base(adornedElement)
        {
            previewFace = face;
            previewImage = previewFace.Thumbnail;
            drawingRegion = new Rect(0, 0, 96, 96);
            this.Opacity = 0.75;
            this.IsHitTestVisible = false;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawImage(previewImage, drawingRegion);
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(Offset.X, Offset.Y));
            return result;
        }

        public Point Offset;
    }
}