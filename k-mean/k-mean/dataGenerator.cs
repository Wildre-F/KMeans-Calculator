using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace k_mean
{
    public class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Cluster { get; set; } = -1;
    }

    public class DataGenerator
    {
        public static List<DataPoint> Generate(int count)
        {
            Random rnd = new Random();
            List<DataPoint> points = new List<DataPoint>();
            
            for (int i = 0; i < count; i++)
            {
                int x = rnd.Next(0,501);
                int y = rnd.Next(0, 501);
                DataPoint p = new DataPoint();
                p.X = x;
                p.Y = y;
                points.Add(p);


            }

            return points;
            
        }
    }
}
