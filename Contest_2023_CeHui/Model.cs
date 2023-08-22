using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Contest_2023_CeHui
{
    internal struct Point
    {
        public string Name;
        public double X;
        public double Y;
        public double Z;

        public Point()
        {
            this.Name = string.Empty;
            this.X = this.Y = this.Z = 0;
        }

        public override readonly string ToString() => $"{this.Name},{this.X:f3},{this.Y:f3},{this.Z:f3}";
    }

    internal class BoundBox
    {
        public Point Min;
        public Point Max;
        public Point Size { get { return new Point() { Name = "Size", X = Max.X - Min.X, Y = Max.Y - Min.Y, Z = Max.Z - Min.Z }; } }

        public BoundBox()
        {
            this.Max = new Point() { Name = "Max" };
            this.Min = new Point() { Name = "Min" };
        }

        public void ComputeBoundBox(in List<Point> points)
        {
            if (points.Count == 0)
                return;

            var bbMax = points[0];
            bbMax.Name = "Max";
            var bbMin = points[0];
            bbMin.Name = "Min";

            foreach (var item in points)
            {
                if (item.X < bbMin.X)
                    bbMin.X = item.X;
                else if (item.X > bbMax.X)
                    bbMax.X = item.X;

                if (item.Y < bbMin.Y)
                    bbMin.Y = item.Y;
                else if (item.Y > bbMax.Y)
                    bbMax.Y = item.Y;

                if (item.Z < bbMin.Z)
                    bbMin.Z = item.Z;
                else if (item.Z > bbMax.Z)
                    bbMax.Z = item.Z;
            }
            this.Max = bbMax;
            this.Min = bbMin;
        }
    }

    internal class Grid
    {
        public List<Point> Points;
        public BoundBox Box;
        public double AverageZ;
        public double DifferenceZ;
        public double VarianceZ;

        public Grid()
        {
            this.Points = new List<Point>();
            this.Box = new BoundBox();
            this.AverageZ = this.DifferenceZ = this.VarianceZ = 0.0;
        }

        public void ComputateGeometricFeature()
        {
            if (this.Points.Count == 0)
                return;

            this.Box.ComputeBoundBox(in this.Points);

            for (int i = 0; i < this.Points.Count; i++)
                this.AverageZ += this.Points[i].Z;
            this.AverageZ /= this.Points.Count;

            this.DifferenceZ = this.Box.Size.Z;

            for (int i = 0; i < this.Points.Count; i++)
                this.VarianceZ += Math.Pow((this.Points[i].Z - this.AverageZ), 2);
            this.VarianceZ /= this.Points.Count;
        }
    }

    internal class PointCloud
    {
        public List<Point> Points { get; }
        public BoundBox Box { get; }

        public PointCloud(List<Point> cloud)
        {
            this.Points = cloud;
            this.Box = new BoundBox();
            this.Box.ComputeBoundBox(in cloud);
        }

        public (bool flag, string message, Grid[,]? grid) Raster(double dx, double dy)
        {
            if (dx <= 0 || dy <= 0)
                return (false, "给定栅格单元的长或宽有误，请检查数据", null);

            var countX = (int)Math.Floor(this.Box.Size.X / dx) + 1;
            var countY = (int)Math.Floor(this.Box.Size.Y / dy) + 1;
            var grid = new Grid[countX, countY];
            for (int i = 0; i < countX; i++)
            {
                for (int j = 0; j < countY; j++)
                    grid[i, j] = new Grid();
            }

            foreach (var item in this.Points)
            {
                var i = (int)Math.Floor((item.X - this.Box.Min.X) / dx);
                var j = (int)Math.Floor((item.Y - this.Box.Min.Y) / dy);
                grid[i, j].Points.Add(item);
            }
            return (true, "已将点云进行二维栅格化操作", grid);

        }
    }

    internal class Geometry
    {
        static public double Distance(Point a, Point b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
        static public double AreaOfTriangle(Point a, Point b, Point c)
        {
            var ab = Distance(a, b);
            var bc = Distance(b, c);
            var ca = Distance(c, a);
            var p = (ab + bc + ca) / 2;
            return Math.Sqrt(p * (p - ab) * (p - bc) * (p - ca));
        }
        static public double[] PlaneEquation(Point a, Point b, Point c)
        {
            var A = (b.Y - a.Y) * (c.Z - a.Z) - (c.Y - a.Y) * (b.Z - a.Z);
            var B = (b.Z - a.Z) * (c.X - a.X) - (c.Z - a.Z) * (b.X - a.X);
            var C = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
            var D = -A * a.X - B * a.Y - C * a.Z;
            return new double[] { A, B, C, D };
        }
        static public double Distance(double[] plane, Point a) => Math.Abs(plane[0] * a.X + plane[1] * a.Y + plane[2] * a.Z + plane[3]) / Math.Sqrt(Math.Pow(plane[0], 2) + Math.Pow(plane[1], 2) + Math.Pow(plane[2], 2));
        static public Point Projection(double[] plane, Point a)
        {
            var m = Math.Sqrt(Math.Pow(plane[0], 2) + Math.Pow(plane[1], 2) + Math.Pow(plane[2], 2));
            var x = (Math.Pow(plane[1], 2) + ((Math.Pow(plane[2], 2)) * a.X) - plane[0] * (plane[1] * a.Y + plane[2] * a.Z + plane[3])) / m;
            var y = (Math.Pow(plane[2], 2) + ((Math.Pow(plane[0], 2)) * a.Y) - plane[1] * (plane[0] * a.X + plane[2] * a.Z + plane[3])) / m;
            var z = (Math.Pow(plane[0], 2) + ((Math.Pow(plane[1], 2)) * a.Z) - plane[2] * (plane[0] * a.X + plane[1] * a.Y + plane[3])) / m;
            return new Point() { Name = "Project", X = x, Y = y, Z = z };
        }
    }
}
