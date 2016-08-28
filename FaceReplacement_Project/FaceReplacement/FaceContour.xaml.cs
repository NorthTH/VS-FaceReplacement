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
using System.Windows.Media.Animation;
using FaceReplacement.Engine;

namespace FaceReplacement
{
    /// <summary>
    /// Interaction logic for FaceContour.xaml
    /// </summary>
    public partial class FaceContour : UserControl
    {
        public FaceContour()
        {
            InitializeComponent();
            isEditingMode = false;
            for (int i = 0; i < totalPoint; i++)
            {
                Ellipse newEllipse = new Ellipse()
                {
                    Stroke = Brushes.SkyBlue,
                    Fill = new SolidColorBrush(Color.FromArgb(0x80, 0x87, 0xCE, 0xEB)),
                    Width = thumbRadius * 2 + 1,
                    Height = thumbRadius * 2 + 1
                };
                newEllipse.MouseMove += Contour_MouseMove;
                newEllipse.MouseUp += Contour_MouseUp;
                newEllipse.PreviewMouseLeftButtonDown += Contour_PreviewMouseLeftButtonDown;
                thumbsLayer.Children.Add(newEllipse);
            }
        }

        private void CloseEditingMode_Storyboard_Completed(object sender, EventArgs e)
        {
            (this.Resources["CloseEditingMode"] as Storyboard).Stop();
        }

        public void Contour_MouseMove(object sender, MouseEventArgs e)
        {
            if (isEditingMode && currentState != DragState.Normal)
            {
                Matrix opMatrix = new Matrix();
                Point mousePosition = e.GetPosition(this);

                if (mousePosition.X < 0) mousePosition.X = 0;
                if (mousePosition.X > BoundaryWidth) mousePosition.X = BoundaryWidth;
                if (mousePosition.Y < 0) mousePosition.Y = 0;
                if (mousePosition.Y > BoundaryHeight) mousePosition.Y = BoundaryHeight;

                int selectedThumbIndex = Points.IndexOf(currentPoint);

                switch (currentState)
                {
                    case DragState.Translate:
                        opMatrix.Translate(mousePosition.X - currentPoint.X, mousePosition.Y - currentPoint.Y);
                        break;
                    case DragState.Scale:
                        Point pivot = Points[(selectedThumbIndex + totalPoint / 2) % totalPoint];

                        Vector faceHorizontal = Points[totalPoint / 4] - Points[(totalPoint * 3) / 4];
                        Vector faceVertical = Points[totalPoint / 2] - Points[0];

                        Vector baseVector = pivot - currentPoint;
                        Vector newVector = pivot - mousePosition;

                        if (Math.Abs(newVector.X) < Math.Abs(baseVector.X / 2) || Math.Abs(newVector.Y) < Math.Abs(baseVector.Y / 2))
                        {
                            return;
                        }

                        double vectorXRadianAngleFromXAxis = Vector.AngleBetween(new Vector(1, 0), faceHorizontal) / 180.0 * Math.PI;

                        double baseVectorRadianAngleFromVectorX = Vector.AngleBetween(faceHorizontal, baseVector) / 180.0 * Math.PI;
                        double newVectorRadianAngleFromVectorX = Vector.AngleBetween(faceHorizontal, newVector) / 180.0 * Math.PI;

                        double scaleX = (newVector.Length * Math.Cos(newVectorRadianAngleFromVectorX)) / (baseVector.Length * Math.Cos(baseVectorRadianAngleFromVectorX));
                        double scaleY = (newVector.Length * Math.Sin(newVectorRadianAngleFromVectorX)) / (baseVector.Length * Math.Sin(baseVectorRadianAngleFromVectorX));

                        double baseRadianAngle = Vector.AngleBetween(newVector, baseVector) / 180.0 * Math.PI;
                        double scale = newVector.Length / baseVector.Length;

                        if (selectedThumbIndex % 4 != 0)
                        {
                            opMatrix.ScaleAt(
                                Euclidean(scaleX * Math.Cos(vectorXRadianAngleFromXAxis), scaleY * Math.Sin(vectorXRadianAngleFromXAxis)),
                                Euclidean(scaleX * Math.Sin(vectorXRadianAngleFromXAxis), scaleY * Math.Cos(vectorXRadianAngleFromXAxis)),
                                pivot.X, pivot.Y);
                        }
                        else if (selectedThumbIndex % (totalPoint / 2) != 0)
                        {
                            opMatrix.ScaleAt(
                                scaleX * Math.Cos(vectorXRadianAngleFromXAxis),
                                1.0,
                                pivot.X, pivot.Y);
                        }
                        else
                        {
                            opMatrix.ScaleAt(
                                1.0,
                                scaleY * Math.Cos(vectorXRadianAngleFromXAxis),
                                pivot.X, pivot.Y);
                        }
                        break;
                    case DragState.Rotate:
                        baseVector = contourCenter - currentPoint;
                        newVector = contourCenter - mousePosition;
                        baseRadianAngle = Vector.AngleBetween(newVector, baseVector) / 180.0 * Math.PI;
                        opMatrix.RotateAt(-baseRadianAngle * 180.0 / Math.PI, contourCenter.X, contourCenter.Y);
                        break;
                }

                Point[] mainfeatures = { RelativeLeftEyePosition, RelativeRightEyePosition, RelativeMouthPosition };
                opMatrix.Transform(mainfeatures);
                
                if ((mainfeatures[0] - mainfeatures[1]).Length < RotateCenter.Width ||
                    (mainfeatures[1] - mainfeatures[2]).Length < RotateCenter.Width ||
                    (mainfeatures[2] - mainfeatures[0]).Length < RotateCenter.Width)
                {
                    return;
                }

                RelativeLeftEyePosition = mainfeatures[0];
                RelativeRightEyePosition = mainfeatures[1];
                RelativeMouthPosition = mainfeatures[2];

                this.Points = FaceContourGenerator.GetContourPoints(RelativeLeftEyePosition, RelativeRightEyePosition, RelativeMouthPosition);
                switch (currentState)
                {
                    case DragState.Translate:
                    case DragState.Rotate:
                        currentPoint = mousePosition;
                        break;
                    case DragState.Scale:
                        if (selectedThumbIndex > -1)
                        {
                            currentPoint = Points[selectedThumbIndex];
                        }
                        else
                        {
                            currentState = DragState.Normal;
                        }
                        break;
                }
            }
        }

        public void Contour_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentState = DragState.Normal;
        }

        public void Contour_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode)
            {
                Point mousePosition = e.GetPosition(this);
                double minDistance = double.MaxValue;
                double sumX = 0.0, sumY = 0.0;

                foreach (Point point in Contour.Points)
                {
                    if ((mousePosition - point).Length < minDistance && (mousePosition - point).Length < thumbRadius)
                    {
                        currentState = DragState.Scale;
                        minDistance = (mousePosition - point).Length;
                        currentPoint = point;
                    }
                    sumX += point.X;
                    sumY += point.Y;
                }

                contourCenter = new Point(sumX / totalPoint, sumY / totalPoint);

                HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, mousePosition);
                bool isMove = hitTestResult != null;

                if ((mousePosition - contourCenter).Length < RotateCenter.Width / 4)
                {
                    currentState = DragState.Translate;
                    currentPoint = mousePosition;
                }
                else if (currentState == DragState.Normal && isMove)
                {
                    currentState = DragState.Rotate;
                    currentPoint = mousePosition;
                }
            }
        }
        
        public double Euclidean(double a, double b)
        {
            return Math.Sqrt(a * a + b * b);
        }
        
        public void HideContour()
        {
            Contour.Visibility = Visibility.Hidden;
            (this.Resources["ContourHilightColorAnimation"] as Storyboard).Stop();
        }

        public void ShowContour()
        {
            Contour.Visibility = Visibility.Visible;
            (this.Resources["ContourHilightColorAnimation"] as Storyboard).Begin();
        }

        public void TriggerEditingMode()
        {
            isEditingMode = true;
            (this.Resources["OpenEditingMode"] as Storyboard).Begin();
            (this.Resources["ContourHilightColorAnimation"] as Storyboard).Begin();
        }

        public void TriggerNormalMode()
        {
            isEditingMode = false;
            (this.Resources["ContourHilightColorAnimation"] as Storyboard).Stop();
            (this.Resources["OpenEditingMode"] as Storyboard).Stop();
            (this.Resources["CloseEditingMode"] as Storyboard).Begin();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);
            if (mousePosition.X < 0 ||
            mousePosition.X > BoundaryWidth ||
            mousePosition.Y < 0 ||
            mousePosition.Y > BoundaryHeight)
                currentState = DragState.Normal;
        }

        public double BoundaryHeight
        {
            get { return (double)GetValue(BoundaryHeightProperty); }
            set { SetValue(BoundaryHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BoundaryHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoundaryHeightProperty =
            DependencyProperty.Register("BoundaryHeight", typeof(double), typeof(FaceContour), new UIPropertyMetadata(double.NaN));

        public double BoundaryWidth
        {
            get { return (double)GetValue(BoundaryWidthProperty); }
            set { SetValue(BoundaryWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BoundaryWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoundaryWidthProperty =
            DependencyProperty.Register("BoundaryWidth", typeof(double), typeof(FaceContour), new UIPropertyMetadata(double.NaN));

        private DragState currentState
        {
            get { return (DragState)GetValue(CurrentStateProperty); }
            set
            {
                switch (value)
                {
                    case DragState.Normal:
                        RotateArrow.Visibility = Visibility.Visible;
                        TranslateArrow.Visibility = Visibility.Visible;
                        thumbsLayer.Visibility = Visibility.Visible;
                        RotateCenter.Visibility = Visibility.Visible;
                        break;
                    case DragState.Rotate:
                        RotateArrow.Visibility = Visibility.Visible;
                        TranslateArrow.Visibility = Visibility.Hidden;
                        thumbsLayer.Visibility = Visibility.Hidden;
                        RotateCenter.Visibility = Visibility.Visible;
                        break;
                    case DragState.Scale:
                        RotateArrow.Visibility = Visibility.Hidden;
                        TranslateArrow.Visibility = Visibility.Hidden;
                        thumbsLayer.Visibility = Visibility.Hidden;
                        RotateCenter.Visibility = Visibility.Hidden;
                        break;
                    case DragState.Translate:
                        RotateArrow.Visibility = Visibility.Hidden;
                        TranslateArrow.Visibility = Visibility.Visible;
                        thumbsLayer.Visibility = Visibility.Hidden;
                        RotateCenter.Visibility = Visibility.Visible;
                        break;
                }
                SetValue(CurrentStateProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for CurrentState.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty CurrentStateProperty =
            DependencyProperty.Register("CurrentState", typeof(DragState), typeof(FaceContour), new UIPropertyMetadata(DragState.Normal));

        public PointCollection Points
        {
            get { return Contour.Points; }
            set
            {
                if (Contour.Points != null) Contour.Points.IndexOf(currentPoint);
                Contour.Points = value;
                double sumX = 0.0, sumY = 0.0;
                for (int i = 0; i < totalPoint; i++)
                {
                    Canvas.SetLeft(thumbsLayer.Children[i], value[i].X - thumbRadius);
                    Canvas.SetTop(thumbsLayer.Children[i], value[i].Y - thumbRadius);
                    sumX += value[i].X;
                    sumY += value[i].Y;
                }
                Point center = new Point(sumX / totalPoint, sumY / totalPoint);
                RotateCenter.Height = RotateCenter.Width = 64.0;// (Contour.Points[totalPoint / 2] - center).Length;
                Canvas.SetLeft(RotateCenter, center.X - RotateCenter.Width / 2);
                Canvas.SetTop(RotateCenter, center.Y - RotateCenter.Height / 2);
            }
        }

        public Point RelativeLeftEyePosition
        {
            get
            {
                return new Point(Canvas.GetLeft(LeftEye) + thumbRadius, Canvas.GetTop(LeftEye) + thumbRadius);
            }
            set
            {
                Canvas.SetLeft(LeftEye, value.X - thumbRadius);
                Canvas.SetTop(LeftEye, value.Y - thumbRadius);
            }
        }

        public Point RelativeRightEyePosition
        {
            get
            {
                return new Point(Canvas.GetLeft(RightEye) + thumbRadius, Canvas.GetTop(RightEye) + thumbRadius);
            }
            set
            {
                Canvas.SetLeft(RightEye, value.X - thumbRadius);
                Canvas.SetTop(RightEye, value.Y - thumbRadius);
            }
        }

        public Point RelativeMouthPosition
        {
            get
            {
                return new Point(Canvas.GetLeft(Mouth) + thumbRadius, Canvas.GetTop(Mouth) + thumbRadius);
            }
            set
            {
                Canvas.SetLeft(Mouth, value.X - thumbRadius);
                Canvas.SetTop(Mouth, value.Y - thumbRadius);
            }
        }

        private bool isEditingMode;

        private const int thumbRadius = 4;

        private const int totalPoint = 16;

        private Point contourCenter;

        private Point currentPoint;

        private enum DragState
        {
            Normal, Scale, Translate, Rotate
        }
    }
}
