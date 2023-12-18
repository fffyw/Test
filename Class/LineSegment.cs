using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRToolBox.Class
{
    public class LineSegment
    {
        public double Length { get; set; }
        public double[] CenterPoint { get; set; }
        public double[] XDirection { get; set; }
        public double[] YDirection { get; set; }
        public double[] ZDirection { get; set; }

        public double[] GetEndPoint1()
        {
            double[] endPoint1 = new double[3];
            for (int i = 0; i < 3; i++)
            {
                endPoint1[i] = Math.Round(CenterPoint[i] - Length / 2 * XDirection[i], 2);
            }
            return endPoint1;
        }

        public double[] GetEndPoint2()
        {
            double[] endPoint2 = new double[3];
            for (int i = 0; i < 3; i++)
            {
                endPoint2[i] = Math.Round(CenterPoint[i] + Length / 2 * XDirection[i] , 2);
            }
            return endPoint2;
        }
    }
}
