using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contest_2023_CeHui
{
    internal class CoreFunction
    {
        static public (bool flag, string message, List<Point>? data) ReadData(string fileName)
        {
            try
            {
                var data = new List<Point>();
                int count = -1;
                using (var sr = new StreamReader(fileName))
                {
                    var line = sr.ReadLine();
                    if (line != null && int.TryParse(line, out count) && count >= 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            line = sr.ReadLine();
                            if (line != null)
                            {
                                var items = line.Split(',');
                                var point = new Point()
                                {
                                    Name = items[0],
                                    X = Convert.ToDouble(items[1]),
                                    Y = Convert.ToDouble(items[2]),
                                    Z = Convert.ToDouble(items[3])
                                };
                                data.Add(point);
                            }
                        }
                    }
                    else
                    {
                        return (false, "读取数据文件失败，请检查数据", null);
                    }
                }
                return (true, $"已成功读取数据文件，共有{data.Count}个点", data);
            }
            catch
            {
                return (false, "读取文件数据失败，请检查数据", null);
            }
        }

        // 基于栅格投影的点云分割算法
        static public (bool flag, string message, Grid[,]? grid) Calc1(in PointCloud cloud, double dx, double dy)
        {
            var (flag, message, grid) = cloud.Raster(dx, dy);
            if (flag && grid != null)
            {
                foreach (var item in grid)
                    item.ComputateGeometricFeature();
                return (true, "已成功对点云进行基于栅格投影的点云分割", grid);
            }
            else
            {
                return (flag, message, null);
            }
        }

        // 随机抽样一致（RANSAC）平面分割
        static public (bool flag, string message) Calc2(in PointCloud cloud, double areaThreshold, double distanceThreshold)
        {
            // 最佳拟合平面 J1
            var insidePointName_J1 = new List<string>();
            var outsidePoint_J1 = new List<Point>();
            for (int i = 0; i < 300; i++)
            {
                // 三点共线检测
                if (Geometry.AreaOfTriangle(cloud.Points[3 * i + 0], cloud.Points[3 * i + 1], cloud.Points[3 * i + 2]) < areaThreshold)
                    continue;

                var plane = Geometry.PlaneEquation(cloud.Points[3 * i + 0], cloud.Points[3 * i + 1], cloud.Points[3 * i + 2]);
                // 内部点和外部点计算
                var insidePointName = new List<string>();
                var outsidePoint = new List<Point>();
                for (int j = 0; j < cloud.Points.Count; j++)
                {
                    if (j == (3 * i + 0) || j == (3 * i + 1) || j == (3 * i + 2))
                        continue;
                    if (Geometry.Distance(plane, cloud.Points[j]) < distanceThreshold)
                        insidePointName.Add(cloud.Points[j].Name);
                    else
                        outsidePoint.Add(cloud.Points[j]);
                }
                // 判断是否为最佳拟合平面 J1
                if (insidePointName_J1.Count < insidePointName.Count)
                {
                    insidePointName_J1 = insidePointName;
                    outsidePoint_J1 = outsidePoint;
                }
            }

            // 最佳拟合平面 J2
            var insidePointName_J2 = new List<string>();
            for (int i = 0; i < 80; i++)
            {
                // 三点共线检测
                if (Geometry.AreaOfTriangle(outsidePoint_J1[3 * i + 0], outsidePoint_J1[3 * i + 1], outsidePoint_J1[3 * i + 2]) < areaThreshold)
                    continue;

                var plane = Geometry.PlaneEquation(outsidePoint_J1[3 * i + 0], outsidePoint_J1[3 * i + 1], outsidePoint_J1[3 * i + 2]);
                // 内部点和外部点计算
                var insidePointName = new List<string>();
                for (int j = 0; j < outsidePoint_J1.Count; j++)
                {
                    if (j == (3 * i + 0) || j == (3 * i + 1) || j == (3 * i + 2))
                        continue;
                    if (Geometry.Distance(plane, outsidePoint_J1[j]) < distanceThreshold)
                        insidePointName.Add(outsidePoint_J1[j].Name);
                }
                if (insidePointName_J2.Count < insidePointName.Count)
                    insidePointName_J2 = insidePointName;
            }

            // 输出结果
            using (var sw = new StreamWriter("result.txt"))
            {
                foreach (var item in cloud.Points)
                {
                    var builder = new StringBuilder($"{item}");
                    // 首先判断是否为 J1 的内部点
                    if (insidePointName_J1.Contains(item.Name))
                        builder.Append(",J1");
                    else if (insidePointName_J2.Contains(item.Name))
                        builder.Append(",J2");
                    else
                        builder.Append(",0");
                    sw.WriteLine(builder.ToString());
                }
            }

            return (true, "已输出结果文件至程序根目录");
        }
    }
}
