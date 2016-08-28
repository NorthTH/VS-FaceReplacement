using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceReplacement.Engine
{
    class ContourFeature
    {        
        public static ContourFeature Parse(string word)
        {
            string[] numbers = word.Split('|');
            ContourFeature output = new ContourFeature();
            output.RelativeRadianAngle = double.Parse(numbers[0]);
            output.RelativeMagnitude = double.Parse(numbers[1]);
            return output;
        }

        public override string ToString()
        {
            return RelativeRadianAngle + "|" + RelativeMagnitude;
        }
        
        public double RelativeMagnitude;
        
        public double RelativeRadianAngle;
    }
}