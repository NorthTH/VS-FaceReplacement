using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using dnAnalytics.LinearAlgebra;

namespace FaceReplacement.Engine
{
    class ColorTransfer
    {
        public static Bitmap Adjust(Face targetHead, Bitmap transformedFacePatch, bool[,] mask)
        {
            if (DoColorTransfer)
            {
                Bitmap colorAdjustedFacePatch = Transfer(transformedFacePatch, mask, targetHead.OriginalPhoto);
                transformedFacePatch.Dispose();
                transformedFacePatch = colorAdjustedFacePatch;
            }
            return transformedFacePatch;
        }
        private static Bitmap Transfer(Bitmap source, bool[,] mask, Bitmap target)
        {
            int width = source.Width;
            int height = source.Height;

            DenseMatrix sourceMaskedPixels = byteToDenseMatrix(extractMaskedPixels(source, mask));
            DenseMatrix targetMaskedPixels = byteToDenseMatrix(extractMaskedPixels(target, mask));
            int totalMaskedPixels = sourceMaskedPixels.Columns;

            if (useLabColorSpace)
            {
                sourceMaskedPixels = RGBToLAB(sourceMaskedPixels);
                targetMaskedPixels = RGBToLAB(targetMaskedPixels);
            }

            double[][] sourceIntensities = new double[channels][];
            double[][] targetIntensities = new double[channels][];

            // calculate statistics
            double[] sourceStd = new double[channels];
            double[] sourceMean = new double[channels];
            double[] targetStd = new double[channels];
            double[] targetMean = new double[channels];

            for (int channel = 0; channel < channels; channel++)
            {
                sourceIntensities[channel] = new double[totalMaskedPixels];
                targetIntensities[channel] = new double[totalMaskedPixels];

                for (int pixel = 0; pixel < totalMaskedPixels; pixel++)
                {
                    sourceIntensities[channel][pixel] = sourceMaskedPixels[channel, pixel];
                    targetIntensities[channel][pixel] = targetMaskedPixels[channel, pixel];
                }

                bool doColorDistributionTransform = CloneColorDistribution;
                if (useLabColorSpace && onlyLabLuminance)
                {
                    doColorDistributionTransform &= (channel == Luminance);
                }

                if (doColorDistributionTransform)
                {
                    sourceIntensities[channel] = forceValuesDistribution(sourceIntensities[channel], targetIntensities[channel]);
                }
                else
                {
                    dnAnalytics.Statistics.DescriptiveStatistics sourceStatistics = new dnAnalytics.Statistics.DescriptiveStatistics(sourceIntensities[channel]);
                    sourceMean[channel] = sourceStatistics.Mean;
                    sourceStd[channel] = sourceStatistics.StandardDeviation;

                    dnAnalytics.Statistics.DescriptiveStatistics targetStatistics = new dnAnalytics.Statistics.DescriptiveStatistics(targetIntensities[channel]);
                    targetMean[channel] = targetStatistics.Mean;
                    targetStd[channel] = targetStatistics.StandardDeviation;
                }
            }

            // do color transformation
            DenseMatrix outputPixels = new DenseMatrix(channels, totalMaskedPixels);

            for (int channel = 0; channel < channels; channel++)
            {
                for (int pixel = 0; pixel < totalMaskedPixels; pixel++)
                {
                    if (CloneColorDistribution) // bypass color transformation
                    {
                        outputPixels[channel, pixel] = sourceIntensities[channel][pixel];
                    }
                    else
                    {
                        outputPixels[channel, pixel] = (sourceIntensities[channel][pixel] - sourceMean[channel]) * targetStd[channel] / sourceStd[channel] + targetMean[channel];
                    }
                }
            }

            if (useLabColorSpace)
            {
                outputPixels = LABToRGB(outputPixels);
            }

            int maskedPixelIndex = 0;
            Bitmap output = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y])
                    {
                        output.SetPixel(x, y, Color.FromArgb(
                            FaceBlender.byteClamp(outputPixels[Red, maskedPixelIndex]),
                            FaceBlender.byteClamp(outputPixels[Green, maskedPixelIndex]),
                            FaceBlender.byteClamp(outputPixels[Blue, maskedPixelIndex])
                            ));
                        maskedPixelIndex++;
                    }
                    else
                    {
                        output.SetPixel(x, y, source.GetPixel(x, y));
                    }
                }
            }

            return output;
        }
        private static DenseMatrix byteToDenseMatrix(byte[,] input)
        {
            DenseMatrix output = new DenseMatrix(channels, input.GetLength(1));
            for (int channel = 0; channel < channels; channel++)
            {
                for (int pixel = 0; pixel < input.GetLength(1); pixel++)
                {
                    output[channel, pixel] = input[channel, pixel];
                }
            }
            return output;
        }
        private static byte[,] denseMatrixToByte(DenseMatrix input)
        {
            byte[,] output = new byte[channels, input.Columns];
            for (int channel = 0; channel < channels; channel++)
            {
                for (int pixel = 0; pixel < input.Columns; pixel++)
                {
                    output[channel, pixel] = FaceBlender.byteClamp(input[channel, pixel]);
                }
            }
            return output;
        }
        private static byte[,] extractMaskedPixels(Bitmap source, bool[,] mask)
        {
            List<Color> pixels = new List<Color>();

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    if (mask[x, y])
                    {
                        pixels.Add(source.GetPixel(x, y));
                    }
                }
            }

            byte[,] output = new byte[channels, pixels.Count];

            for (int pixel = 0; pixel < pixels.Count; pixel++)
            {
                output[Red, pixel] = pixels[pixel].R;
                output[Green, pixel] = pixels[pixel].G;
                output[Blue, pixel] = pixels[pixel].B;
            }

            return output;
        }
        private static double[] forceValuesDistribution(double[] sourceValues, double[] targetValues)
        {
            Dictionary<double, int> valueCount = new Dictionary<double, int>();
            for (int value = 0; value < sourceValues.Length; value++)
            {
                if (!valueCount.Keys.Contains(sourceValues[value]))
                {
                    valueCount[sourceValues[value]] = 0;
                }
                valueCount[sourceValues[value]]++;
            }

            Dictionary<double, double> valueToPercentile = new Dictionary<double, double>();

            // calculate percentile index at each intensity
            double[] values = valueCount.Keys.ToArray();
            Array.Sort(values);

            int currentCumulativePixelCount = 0;
            foreach (double value in values) // foreach intensity
            {
                currentCumulativePixelCount += valueCount[value];
                valueToPercentile[value] = currentCumulativePixelCount * 100.0 / sourceValues.Length;
            }

            targetValues = targetValues.Clone() as double[];
            Array.Sort(targetValues);

            double[] percentiles = valueToPercentile.Values.ToArray();
            Array.Sort(percentiles);
            // prepare map table for each percentile index
            Dictionary<double, double> valueToIntensity = new Dictionary<double, double>();
            foreach (double percentile in percentiles)
            {
                valueToIntensity[percentile] = getValueFromPercentile(targetValues, percentile);
            }

            double[] output = new double[sourceValues.Length];
            for (int value = 0; value < sourceValues.Length; value++)
            {
                output[value] = valueToIntensity[valueToPercentile[sourceValues[value]]];
            }

            return output;
        }
        private static double getValueFromPercentile(double[] array, double percentile)
        {
            double index = (array.Length - 1) * percentile / 100.0;
            int upperIndex = (int)Math.Ceiling(index);
            int lowerIndex = (int)Math.Floor(index);
            if (upperIndex == lowerIndex)
            {
                return array[lowerIndex];
            }
            else
            {
                // interpolate
                return (array[lowerIndex] * (index - lowerIndex) + array[upperIndex] * (upperIndex - index));
            }
        }
        private static void initializeConstants()
        {
            RGB2lms = new DenseMatrix(3, 3);
            RGB2lms[0, 0] = 0.3811;
            RGB2lms[0, 1] = 0.5783;
            RGB2lms[0, 2] = 0.0402;
            RGB2lms[1, 0] = 0.1967;
            RGB2lms[1, 1] = 0.7244;
            RGB2lms[1, 2] = 0.0782;
            RGB2lms[2, 0] = 0.0241;
            RGB2lms[2, 1] = 0.1288;
            RGB2lms[2, 2] = 0.8444;

            DenseMatrix LMS2lab01 = new DenseMatrix(3, 3);
            LMS2lab01[0, 0] = 1 / Math.Sqrt(3);
            LMS2lab01[0, 1] = 0;
            LMS2lab01[0, 2] = 0;
            LMS2lab01[1, 0] = 0;
            LMS2lab01[1, 1] = 1 / Math.Sqrt(6);
            LMS2lab01[1, 2] = 0;
            LMS2lab01[2, 0] = 0;
            LMS2lab01[2, 1] = 0;
            LMS2lab01[2, 2] = 1 / Math.Sqrt(2);

            DenseMatrix LMS2lab02 = new DenseMatrix(3, 3);
            LMS2lab02[0, 0] = 1;
            LMS2lab02[0, 1] = 1;
            LMS2lab02[0, 2] = 1;
            LMS2lab02[1, 0] = 1;
            LMS2lab02[1, 1] = 1;
            LMS2lab02[1, 2] = -2;
            LMS2lab02[2, 0] = 1;
            LMS2lab02[2, 1] = -1;
            LMS2lab02[2, 2] = 0;

            LMS2lab = LMS2lab01 * LMS2lab02;

            DenseMatrix lab2LMS01 = new DenseMatrix(3, 3);
            lab2LMS01[0, 0] = 1;
            lab2LMS01[0, 1] = 1;
            lab2LMS01[0, 2] = 1;
            lab2LMS01[1, 0] = 1;
            lab2LMS01[1, 1] = 1;
            lab2LMS01[1, 2] = -1;
            lab2LMS01[2, 0] = 1;
            lab2LMS01[2, 1] = -2;
            lab2LMS01[2, 2] = 0;

            DenseMatrix lab2LMS02 = new DenseMatrix(3, 3);
            lab2LMS02[0, 0] = Math.Sqrt(3) / 3;
            lab2LMS02[0, 1] = 0;
            lab2LMS02[0, 2] = 0;
            lab2LMS02[1, 0] = 0;
            lab2LMS02[1, 1] = Math.Sqrt(6) / 6;
            lab2LMS02[1, 2] = 0;
            lab2LMS02[2, 0] = 0;
            lab2LMS02[2, 1] = 0;
            lab2LMS02[2, 2] = Math.Sqrt(2) / 2;

            lab2LMS = lab2LMS01 * lab2LMS02;

            lms2RGB = new DenseMatrix(3, 3);
            lms2RGB[0, 0] = 4.4679;
            lms2RGB[0, 1] = -3.5873;
            lms2RGB[0, 2] = 0.1193;
            lms2RGB[1, 0] = -1.2186;
            lms2RGB[1, 1] = 2.3809;
            lms2RGB[1, 2] = -0.1624;
            lms2RGB[2, 0] = 0.0497;
            lms2RGB[2, 1] = -0.2439;
            lms2RGB[2, 2] = 1.2045;
        }
        private static DenseMatrix LABToRGB(DenseMatrix lab)
        {
            if (lab2LMS == null)
            {
                initializeConstants();
            }

            DenseMatrix lms = lab2LMS * lab;

            for (int channel = 0; channel < channels; channel++)
            {
                for (int pixel = 0; pixel < lab.Columns; pixel++)
                {
                    if (lms[channel, pixel] == 0.0)
                    {
                        lms[channel, pixel] = 1 / double.MaxValue;
                    }

                    lms[channel, pixel] = Math.Pow(10.0, lms[channel, pixel]);
                }
            }

            DenseMatrix RGB = lms2RGB * lms;

            return RGB;
        }
        private static DenseMatrix RGBToLAB(DenseMatrix input)
        {
            if (RGB2lms == null)
            {
                initializeConstants();
            }

            DenseMatrix lms = RGB2lms * input;

            for (int channel = 0; channel < channels; channel++)
            {
                for (int pixel = 0; pixel < input.Columns; pixel++)
                {
                    if (lms[channel, pixel] == 0)
                    {
                        lms[channel, pixel] = 1.0 / double.MaxValue;
                    }
                    lms[channel, pixel] = Math.Log10(lms[channel, pixel]);
                }
            }

            DenseMatrix lab = LMS2lab * lms;

            return lab;
        }

        private static DenseMatrix RGB2lms, LMS2lab, lab2LMS, lms2RGB;

        public static bool DoColorTransfer = true;
        public static bool CloneColorDistribution = true;
        public static bool useLabColorSpace = false; 
        public static bool onlyLabLuminance = false;
        
        private const int channels = 3;
        private const int Red = 0, Green = 1, Blue = 2; // ordered as constant matrices
        private const int Luminance = 0, Alpha = 1, Beta = 2; // ordered as constant matrices
    }
}