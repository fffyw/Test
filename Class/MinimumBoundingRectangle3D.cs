using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace BRToolBox.Class
{
    public class MinimumBoundingRectangle3D
    {

        public void GetMinRectangle(List<Point> pointList)
        {
            List<Vector3> vectorList = new List<Vector3>();

            foreach(var point in pointList)
            {
                Vector3 vector = new Vector3(point.X, point.Y,point.Z);

                vectorList.Add(vector);
            }

            GetMinRectangle(vectorList);

           // _ = CreatPlate(pointList, 10);
        }


        private  void GetMinRectangle(List<Vector3> vertexList)
        {

            // 计算最小外接矩形
            Vector3 min = new Vector3(
                vertexList.Min(v => v.X),
                vertexList.Min(v => v.Y),
                vertexList.Min(v => v.Z)
            );

            Vector3 max = new Vector3(
                vertexList.Max(v => v.X),
                vertexList.Max(v => v.Y),
                vertexList.Max(v => v.Z)
            );

            // 最小外接矩形的尺寸和位置
            Vector3 size = max - min;
            Vector3 center = (max + min) * 0.5f;


            // 计算所有顶点
            Vector3[] vertices = new Vector3[8];
            vertices[0] = min;
            vertices[1] = new Vector3(Math.Round(max.X,1), Math.Round(min.Y), Math.Round(min.Z));
            vertices[2] = new Vector3(Math.Round(min.X), Math.Round(max.Y), Math.Round(min.Z));
            vertices[3] = new Vector3(Math.Round(max.X), Math.Round(max.Y), Math.Round(min.Z));
            vertices[4] = new Vector3(Math.Round(min.X), Math.Round(min.Y), Math.Round(max.Z));
            vertices[5] = new Vector3(Math.Round(max.X), Math.Round(min.Y), Math.Round(max.Z));
            vertices[6] = new Vector3(Math.Round(min.X), Math.Round(max.Y), Math.Round(max.Z));
            vertices[7] = max;

            Console.WriteLine("Min corner: " + min);
            Console.WriteLine("Max corner: " + max);
            Console.WriteLine("Size: " + size);
            Console.WriteLine("Center: " + center);
        }



        public static ContourPlate CreatPlate(List<Point> pointlist, double thick)
        {
            ContourPlate contourPlate = new ContourPlate();
            foreach (Point item in pointlist)
            {
                ContourPoint contourPoint = new ContourPoint(item, new Chamfer());
                contourPlate.AddContourPoint(contourPoint);
            }
            contourPlate.Position.Depth = Position.DepthEnum.MIDDLE;
            contourPlate.Class = "1";
            contourPlate.Profile.ProfileString = "PL" + thick;
            contourPlate.Material.MaterialString = "Q235B";
            try
            {
                _ = contourPlate.Insert();
                return contourPlate;
            }
            catch (Exception)
            {
                return null;
            }
        }


        // 用于模拟向量的结构体
        public struct Vector3
        {
            public double X, Y, Z;

            public Vector3(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public static Vector3 operator -(Vector3 a, Vector3 b)
            {
                return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            }

            public static Vector3 operator +(Vector3 a, Vector3 b)
            {
                return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            }

            public static Vector3 operator *(Vector3 v, float f)
            {
                return new Vector3(v.X * f, v.Y * f, v.Z * f);
            }
        }
    }
}
