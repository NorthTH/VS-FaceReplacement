using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace FaceReplacement.Engine
{
    public delegate Bitmap BitmapProcessingDelegate(Bitmap target, bool[,] mask, Bitmap source);

    class FaceBlender
    {
        public static BitmapProcessingDelegate Clone = new BitmapProcessingDelegate(mvcClone);

        public static byte byteClamp(double value)
        {
            if (value > 255)
                return 255;
            else if (value < 0)
                return 0;
            else
                return (byte)value;
        }
        
        public static Bitmap mvcClone(Bitmap backgroundBitmap, bool[,] mask, Bitmap foregroundBitmap)
        {
            double[,][] lambda = new double[foregroundBitmap.Width, foregroundBitmap.Height][];

            bool[,] border = FaceContourGenerator.convertMaskToBorder(mask);
            System.Windows.Point[] P = getCounterClockWisePoints(border);
            // Compute the mean-value coordinates of x w.r.t. P
            for (int y = 1; y < backgroundBitmap.Height - 1; y++)
            {
                for (int x = 1; x < backgroundBitmap.Width - 1; x++)
                {
                    if (mask[x, y])
                    {
                        lambda[x, y] = mvc(new System.Windows.Point(x, y), P);
                    }
                }
            }

            // Compute differences along the boundary
            int[][] diff = new int[P.Length][];
            for (int i = 0; i < P.Length; i++)
            {
                diff[i] = new int[] {
                    backgroundBitmap.GetPixel((int)P[i].X, (int)P[i].Y).B - foregroundBitmap.GetPixel((int)P[i].X, (int)P[i].Y).B,
                    backgroundBitmap.GetPixel((int)P[i].X, (int)P[i].Y).G - foregroundBitmap.GetPixel((int)P[i].X, (int)P[i].Y).G,
                    backgroundBitmap.GetPixel((int)P[i].X, (int)P[i].Y).R - foregroundBitmap.GetPixel((int)P[i].X, (int)P[i].Y).R};
            }

            // evaluate the mean-value interpolant at x
            Bitmap f = new Bitmap(foregroundBitmap.Width, foregroundBitmap.Height);
            double[,][] r = new double[foregroundBitmap.Width, foregroundBitmap.Height][];
            for (int y = 1; y < backgroundBitmap.Height - 1; y++)
            {
                for (int x = 1; x < backgroundBitmap.Width - 1; x++)
                {
                    if (mask[x, y])
                    {
                        r[x, y] = new double[3];
                        for (int i = 0; i < lambda[x, y].Length; i++)
                        {
                            r[x, y][R] += lambda[x, y][i] * diff[i][R];
                            r[x, y][G] += lambda[x, y][i] * diff[i][G];
                            r[x, y][B] += lambda[x, y][i] * diff[i][B];
                        }
                        f.SetPixel(x, y, Color.FromArgb(
                            byteClamp(foregroundBitmap.GetPixel(x, y).R + r[x, y][R]),
                            byteClamp(foregroundBitmap.GetPixel(x, y).G + r[x, y][G]),
                            byteClamp(foregroundBitmap.GetPixel(x, y).B + r[x, y][B]))
                            );
                    }
                }
            }
            return f;
        }
        
        public static Bitmap poissonClone(Bitmap backgroundBitmap, bool[,] mask, Bitmap foregroundBitmap)
        {
            int width = backgroundBitmap.Width;
            int height = backgroundBitmap.Height;
            int[][,] div = deriveSecondOrder(foregroundBitmap);
            bool[,] border = FaceContourGenerator.convertMaskToBorder(mask);

            // Build mapping from (x,y) to variables
            int maskedPixelsCount = 0; // variable indexer

            // Key is a pixel address, y * width + x
            // Value is the order number
            Dictionary<int, int> maskedPixels = new Dictionary<int, int>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y] && !border[x, y]) // Masked pixel
                    {
                        int currentPixelAddress = y * width + x;
                        maskedPixels[currentPixelAddress] = maskedPixelsCount;
                        maskedPixelsCount++;
                    }
                }
            }

            if (maskedPixelsCount == 0)
            {
                return backgroundBitmap;
            }

            // Build the matrix
            // ----------------
            // We solve Ax = b for all 3 channels at once

            // Create the sparse matrix, we have at least 5 non-zero elements per column
            dnAnalytics.LinearAlgebra.SparseMatrix pAt = new dnAnalytics.LinearAlgebra.SparseMatrix(maskedPixelsCount, maskedPixelsCount);
            dnAnalytics.LinearAlgebra.DenseMatrix known = new dnAnalytics.LinearAlgebra.DenseMatrix(maskedPixelsCount, 3);

            // Populate matrix
            maskedPixelsCount = 0;
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (mask[x, y] && !border[x, y])
                    {
                        int currentPixelAddress = y * width + x;

                        int[] colorValue = new int[3];
                        colorValue[B] = div[B][x, y];
                        colorValue[G] = div[G][x, y];
                        colorValue[R] = div[R][x, y];

                        if (border[x, y - 1]) // above
                        {
                            SubtractColors(colorValue, backgroundBitmap.GetPixel(x, y - 1));
                        }
                        else
                        {
                            pAt[maskedPixels[currentPixelAddress - width], maskedPixelsCount] = 1.0;
                        }

                        if (border[x - 1, y]) // left
                        {
                            SubtractColors(colorValue, backgroundBitmap.GetPixel(x - 1, y));
                        }
                        else
                        {
                            pAt[maskedPixels[currentPixelAddress - 1], maskedPixelsCount] = 1.0;
                        }

                        pAt[maskedPixels[currentPixelAddress], maskedPixelsCount] = -4.0; // itself

                        if (border[x + 1, y]) // right
                        {
                            SubtractColors(colorValue, backgroundBitmap.GetPixel(x + 1, y));
                        }
                        else
                        {
                            pAt[maskedPixels[currentPixelAddress + 1], maskedPixelsCount] = 1.0;
                        }

                        if (border[x, y + 1]) // below
                        {
                            SubtractColors(colorValue, backgroundBitmap.GetPixel(x, y + 1));
                        }
                        else
                        {
                            pAt[maskedPixels[currentPixelAddress + width], maskedPixelsCount] = 1.0;
                        }

                        int pixelOrder = maskedPixels[y * width + x];
                        known[pixelOrder, B] = colorValue[B] / 255.0;
                        known[pixelOrder, G] = colorValue[G] / 255.0;
                        known[pixelOrder, R] = colorValue[R] / 255.0;

                        maskedPixelsCount++;
                    }
                }
            }

            //dnAnalytics.LinearAlgebra.Solvers.Direct.LUSolver solver = new dnAnalytics.LinearAlgebra.Solvers.Direct.LUSolver();
            //dnAnalytics.LinearAlgebra.Solvers.Iterative.TFQMR solver = new dnAnalytics.LinearAlgebra.Solvers.Iterative.TFQMR(); // 76.991
            //dnAnalytics.LinearAlgebra.Solvers.Iterative.GPBiCG solver = new dnAnalytics.LinearAlgebra.Solvers.Iterative.GPBiCG();//70.293
            dnAnalytics.LinearAlgebra.Solvers.Iterative.BiCgStab solver = new dnAnalytics.LinearAlgebra.Solvers.Iterative.BiCgStab();//66.378

            dnAnalytics.LinearAlgebra.Matrix unknown = solver.Solve(pAt, known);

            // Convert solution vector back to image
            Bitmap outputBitmap = new Bitmap(backgroundBitmap.Width, backgroundBitmap.Height);
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (mask[x, y] && !border[x, y])
                    {
                        int pixelIndex = y * width + x;
                        int pixelOrder = maskedPixels[pixelIndex];
                        // Clamp RGB values
                        outputBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(clamp(unknown[pixelOrder, R]), clamp(unknown[pixelOrder, G]), clamp(unknown[pixelOrder, B])));
                    }
                }
            }
            return outputBitmap;
        }
        
        private static byte clamp(double n)
        {
            if (n < 0.0)
                return 0;
            else if (n > 1.0)
                return 255;
            else
                return (byte)(n * 255);
        }
        
        private static int[][,] deriveSecondOrder(Bitmap originalBitmap)
        {

            const int Column = 4;
            int width = originalBitmap.Width, height = originalBitmap.Height;
            System.Drawing.Imaging.BitmapData dataOriBitmap = originalBitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            int Row = dataOriBitmap.Width * Column;

            int[][,] firstOrderU = { new int[width, height], new int[width, height], new int[width, height] };
            int[][,] firstOrderV = { new int[width, height], new int[width, height], new int[width, height] };
            unsafe
            {
                byte* originalBitmapData = (byte*)(dataOriBitmap.Scan0);
                for (int y = 0; y < dataOriBitmap.Height - 1; y++)
                {
                    for (int x = 0; x < dataOriBitmap.Width - 1; x++)
                    {
                        int i = y * Row + x * Column;

                        firstOrderU[B][x, y] = originalBitmapData[i + Column + B] - originalBitmapData[i + B];
                        firstOrderU[G][x, y] = originalBitmapData[i + Column + G] - originalBitmapData[i + G];
                        firstOrderU[R][x, y] = originalBitmapData[i + Column + R] - originalBitmapData[i + R];

                        firstOrderV[B][x, y] = originalBitmapData[i + Row + B] - originalBitmapData[i + B];
                        firstOrderV[G][x, y] = originalBitmapData[i + Row + G] - originalBitmapData[i + G];
                        firstOrderV[R][x, y] = originalBitmapData[i + Row + R] - originalBitmapData[i + R];
                    }
                }
            }
            originalBitmap.UnlockBits(dataOriBitmap);

            int[][,] secondOrder = { new int[width, height], new int[width, height], new int[width, height] };

            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    secondOrder[B][x, y] = firstOrderU[B][x, y] - firstOrderU[B][x - 1, y] + firstOrderV[B][x, y] - firstOrderV[B][x, y - 1];
                    secondOrder[G][x, y] = firstOrderU[G][x, y] - firstOrderU[G][x - 1, y] + firstOrderV[G][x, y] - firstOrderV[G][x, y - 1];
                    secondOrder[R][x, y] = firstOrderU[R][x, y] - firstOrderU[R][x - 1, y] + firstOrderV[R][x, y] - firstOrderV[R][x, y - 1];
                }
            }
            return secondOrder;
        }

        private static System.Windows.Point[] getCounterClockWisePoints(bool[,] border)
        {
            bool[,] available = border.Clone() as bool[,];
            List<System.Windows.Point> pointList = new List<System.Windows.Point>();
            int currentX = 0, currentY = 0;
            int nextX = 0, nextY = 0;
            bool borderLeft = true;

            for (int y = available.GetLength(1) - 2; y > 0; y--)
            {
                for (int x = available.GetLength(0) - 2; x > 0; x--)
                {
                    if (available[x, y])
                    {
                        currentX = x;
                        currentY = y;

                        break;
                    }
                }
                if (currentX != 0 && currentY != 0)
                {
                    break;
                }
            }

            do
            {
                pointList.Add(new System.Windows.Point(currentX, currentY));
                available[currentX, currentY] = false;

                //check right pixel
                if (available[currentX + 1, currentY] == true)
                {
                    nextX = currentX + 1;
                    nextY = currentY;
                }
                //check above right pixel
                else if (available[currentX + 1, currentY - 1] == true)
                {
                    nextX = currentX + 1;
                    nextY = currentY - 1;
                }
                //check above pixel
                else if (available[currentX, currentY - 1] == true)
                {
                    nextX = currentX;
                    nextY = currentY - 1;
                }
                //check above left pixel
                else if (available[currentX - 1, currentY - 1] == true)
                {
                    nextX = currentX - 1;
                    nextY = currentY - 1;
                }
                //check left pixel
                else if (available[currentX - 1, currentY] == true)
                {
                    nextX = currentX - 1;
                    nextY = currentY;
                }
                //check left below pixel
                else if (available[currentX - 1, currentY + 1] == true)
                {
                    nextX = currentX - 1;
                    nextY = currentY + 1;
                }
                //check below pixel 
                else if (available[currentX, currentY + 1] == true)
                {
                    nextX = currentX;
                    nextY = currentY + 1;
                }
                //check right below pixel
                else if (available[currentX + 1, currentY + 1] == true)
                {
                    nextX = currentX + 1;
                    nextY = currentY + 1;
                }
                else
                {
                    borderLeft = false;
                }
                currentX = nextX;
                currentY = nextY;
            }
            while (borderLeft);

            return pointList.ToArray();
        }
        
        private static double[] mvc(System.Windows.Point x, System.Windows.Point[] P)
        {
            double[] w = new double[P.Length];
            double[] alpha = new double[P.Length];

            for (int i = 0; i < P.Length; i++)
            {
                alpha[i % P.Length] = System.Windows.Vector.AngleBetween(P[(i + 1) % P.Length] - x, P[i % P.Length] - x) / 180 * Math.PI;
            }

            for (int i = 0; i < P.Length; i++)
            {
                double magnitude = (P[(i + 1) % P.Length] - x).Length;
                if (magnitude != 0.0)
                {
                    w[(i + 1) % P.Length] = (
                        Math.Tan(alpha[i % P.Length] / 2) +
                        Math.Tan(alpha[(i + 1) % P.Length] / 2)
                        ) / (P[(i + 1) % P.Length] - x).Length;
                }
                else
                {
                    w[(i + 1) % P.Length] = double.MaxValue;
                }
            }

            double[] lambda = new double[P.Length];
            double sumW = w.Sum();

            for (int i = 0; i < P.Length; i++)
            {
                lambda[i] = w[i] / sumW;
            }
            return lambda;
        }

        private static void SubtractColors(int[] left, System.Drawing.Color right)
        {
            left[R] -= right.R;
            left[G] -= right.G;
            left[B] -= right.B;
        }

        private const int A = 3, R = 2, G = 1, B = 0; // ordered as Format32bppArgb byte offset
    }
}