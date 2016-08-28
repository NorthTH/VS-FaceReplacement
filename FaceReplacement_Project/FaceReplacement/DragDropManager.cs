using System;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using FaceReplacement.Engine;
using WPF.JoshSmith.Controls.Utilities;
namespace FaceReplacement
{
    public class DragDropManager
    {
        public DragDropManager(UIElement referenceElement)
        {
            this.adornedElement = referenceElement;
            layer = AdornerLayer.GetAdornerLayer(adornedElement);
            if (layer == null)
            {
                throw new Exception("Failed to extract adorner layer. This may occur because of uninitialized reference element");
            }
        }
        public void ShowAdorner(object sender, DragEventArgs e)
        {
            Face faceData = e.Data.GetData(typeof(Face)) as Face;
            adorner = new FaceDragDropAdorner(adornedElement, faceData);
            layer.Add(adorner);
        }

        public void HideAdorner(object sender, DragEventArgs e)
        {
            HideAdorner();
        }

        public void HideAdorner()
        {
            if (adorner != null)
            {
                layer.Remove(adorner);
            }
            adorner = null;
        }

        private void UpdateAdorner(object sender, GiveFeedbackEventArgs e)
        {
            if (adorner != null)
            {
                adorner.Offset = MouseUtilities.GetMousePosition(adornedElement);
                layer.Update();
            }
        }

        private void Start()
        {
            localDragStart = MouseUtilities.GetMousePosition(adornedElement);
        }

        public Vector Dragged
        {
            get
            {
                return localDragStart - MouseUtilities.GetMousePosition(adornedElement);
            }
        }
        public void DoDragDrop(UIElement workingElement, object data)
        {
            DragDrop.AddGiveFeedbackHandler(workingElement, UpdateAdorner);
            DragDrop.DoDragDrop(workingElement, data, DragDropEffects.Move);
            DragDrop.RemoveGiveFeedbackHandler(workingElement, UpdateAdorner);
            HideAdorner();
        }
        private Point localDragStart;
        private UIElement adornedElement;
        private AdornerLayer layer;
        private FaceDragDropAdorner adorner;
    }
}