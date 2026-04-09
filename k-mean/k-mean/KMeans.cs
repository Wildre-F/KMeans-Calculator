using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace k_mean
{
    public class Centroid
    {
        public double X { get; set; }
        public double Y { get; set; }
    }


    public class KMeans
    {
        public List<IterationLog> Log = new List<IterationLog>();

        public IterationLog StepAndLog(int iterationNumber)
        {
            // calculate distances before assigning
            double[,] distances = new double[_points.Count, _centroids.Count];
            for (int i = 0; i < _points.Count; i++)
                for (int j = 0; j < _centroids.Count; j++)
                    distances[i, j] = Math.Sqrt(
                        Math.Pow(_points[i].X - _centroids[j].X, 2) +
                        Math.Pow(_points[i].Y - _centroids[j].Y, 2)) / 500.0;

            AssignClusters();

            int[] assignments = _points.Select(p => p.Cluster).ToArray();

            RecalculateCentroids();

            var log = new IterationLog
            {
                Iteration = iterationNumber,
                Distances = distances,
                Assignments = assignments,
                NewCentroids = _centroids.Select(c => new Centroid
                { X = c.X, Y = c.Y }).ToList()
            };

            Log.Add(log);
            return log;
        }

        private List<DataPoint> _points;
        private List<Centroid> _centroids;

        public KMeans(List<DataPoint> points, int k)
        {
            _points = points;
            _centroids = new List<Centroid>();
            Random random = new Random();

            for (int i = 0; i < k; i++)
            {
                int randomIndex = random.Next(points.Count);
                DataPoint chosenPoint = _points[randomIndex];
                Centroid c = new Centroid();
                c.X = chosenPoint.X;
                c.Y = chosenPoint.Y;
                _centroids.Add(c);
            }
            
           
        }

        public void AssignClusters()
        {
            foreach (DataPoint point in _points)
            {
                double minDistance = double.MaxValue;
                int closestCentroid = 0;

                for (int i = 0; i < _centroids.Count; i++)
                {
                    double distance = Math.Sqrt(Math.Pow(point.X - _centroids[i].X, 2) + Math.Pow(point.Y - _centroids[i].Y, 2));
                    if (distance < minDistance) 
                    { 
                        minDistance = distance; closestCentroid = i;
                    }
                    
                }

                point.Cluster = closestCentroid;
            }
        }

        public void RecalculateCentroids()
        {
            for (int i = 0; i < _centroids.Count; i++)
            {
                List<DataPoint> clusterPoints = _points.Where(p => p.Cluster == i).ToList();

                if (clusterPoints.Count > 0)
                {
                    _centroids[i].X = clusterPoints.Average(p => p.X);
                    _centroids[i].Y = clusterPoints.Average(p => p.Y);
                   
                }
            }
        }

        public void Run(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                AssignClusters();
                RecalculateCentroids();
            }
        }

        public bool HasConverged(List<Centroid> oldCentroids)
        {
            for (int i = 0; i < _centroids.Count; i++)
            {
                if (Math.Abs(_centroids[i].X - oldCentroids[i].X) > 0.01 ||
                    Math.Abs(_centroids[i].Y - oldCentroids[i].Y) > 0.01)
                    return false;
            }
            return true;
        }

        public void SetCentroid(int index, double x, double y)
        {
            _centroids[index].X = x;
            _centroids[index].Y = y;
        }



        public List<DataPoint> Points => _points;
        public List<Centroid> Centroids => _centroids;
    }

    public class IterationLog
    {
        public int Iteration { get; set; }
        public double[,] Distances { get; set; }
        public int[] Assignments { get; set; }
        public List<Centroid> NewCentroids { get; set; }
    }
}
