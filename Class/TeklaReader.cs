using HelixToolkit.Wpf.SharpDX;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;
using Line = Tekla.Structures.Geometry3d.Line;
using ModelObject = Tekla.Structures.Model.ModelObject;
using Point = Tekla.Structures.Geometry3d.Point;
using Vector = Tekla.Structures.Geometry3d.Vector;

namespace BRToolBox.Class
{
    public class TeklaReader
    {
        // 创建Tekla Structures模型对象
        Model teklaModel = null;
        public List<ModelObject> TotalModelObjs = new List<ModelObject>();
        public List<ModelObject> ModelBeamObjs = new List<ModelObject>();
        public List<ModelObject> ModelWeldObjs = new List<ModelObject>();
        public List<ModelObject> ModelContourPlateObjs = new List<ModelObject>();

        public string JsonWeldData = string.Empty;


        public List<WeldStructure> WeldList = new List<WeldStructure>();

        private Viewport3DX Viewer3DX = null;

        public TeklaReader(Viewport3DX viewport3DX)
        {
            Viewer3DX = viewport3DX;

            teklaModel = new Model();
        }

        public void LoadModel()
        {

            // 连接到Tekla Structures模型
            bool success = teklaModel.GetConnectionStatus();

            if (success)
            {
                // 连接成功，可以继续操作模型
                Tekla.Structures.Model.ModelInfo modelInfo = teklaModel.GetInfo();
                _ = modelInfo.ModelName;
            }
            else
            {
                // 连接失败，无法操作模型
                _ = MessageBox.Show("请打开Tekla数模");
                return;
            }


     
            // Get all teklaModel objects:

            ModelObjectEnumerator modelObjectEnum = teklaModel.GetModelObjectSelector().GetAllObjects();

            ModelObjectEnumerator.AutoFetch = true;

            if (modelObjectEnum != null)
            {
                modelObjectEnum.Reset();

                foreach (ModelObject modelObject in modelObjectEnum)
                {
                    TotalModelObjs.Add(modelObject);

                    if (IsBeam(modelObject as Part))
                    {
                        ModelBeamObjs.Add(modelObject);
                    }
                    if(IsPlate(modelObject as Part))
                    {
                        ModelContourPlateObjs.Add(modelObject);
                    }

                    if (modelObject is PolygonWeld weld2)
                    {
                        ModelWeldObjs.Add(weld2);
                    }

                    if (modelObject is PolygonWeld weld1)
                    {
                        // Tekla.Structures.Model.Weld weld = modelObject as Tekla.Structures.Model.Weld;

                       // ModelWeldObjs.Add(modelObject);

                        PolygonWeld weld = modelObject as PolygonWeld;


                        #region Weld 属性说明
                        //var weld = modelObject as Tekla.Structures.Model.Weld;

                        //当AroundWeld的值为false时，表示不绕过基础焊缝对象进行构件建模。这意味着构件将被建模以覆盖基础焊缝对象，并且将被视为单个实体。
                        //相反，当AroundWeld的值为true时，表示绕过基础焊缝对象进行构件建模。这意味着构件将被切割成两部分，围绕基础焊缝对象进行建模。这种
                        //方法通常用于处理具有复杂连接和焊接情况的结构，以便更好地描述它们的几何形状和材料特性。通过设置AroundWeld属性，可以在Tekla 
                        //Structures软件中明确指定是否绕过基础焊缝对象进行构件建模。这有助于提供准确的结构模型，并支持后续的设计、施工和维护过程。
                        _ = weld.AroundWeld;

                        //当ConnectAssemblies的值为false时，表示不进行构件组件的连接。这意味着构件将保持独立状态，不会通过焊接、螺栓或其他方式与其他构件连接在一起。
                        //相反，当ConnectAssemblies的值为true时，表示进行构件组件的连接。这意味着构件将通过适当的连接方法（如焊接或螺栓）与其他构件进行连接。连接的
                        //方式和方法将根据具体的设计和施工要求来确定。
                        _ = weld.ConnectAssemblies;

                        //ContourAbove的意思是，该基础焊缝对象没有上方的轮廓。其他的枚举成员包括ContourBelow、ContourBoth等，可以分别表示下方轮廓、上下轮廓等不同的轮廓
                        //类型。这些成员可以用于指定基础焊缝对象的轮廓类型，以便在绘图和报告中正确显示焊缝的形状和尺寸信息。

                        _ = weld.ContourAbove;

                        //焊缝的有效喉厚度是指焊缝两侧金属之间的最小距离，通常用于评估焊接连接的强度和可靠性。在焊缝设计中，有效喉厚度是一个重要的参数，影响焊缝的强度和承载能力。
                        //通过设置EffectiveThroatAbove属性的值，可以在Tekla Structures软件中明确指定基础焊缝对象上方的有效喉厚度。这有助于准确地描述焊缝的特征，并支持后续的
                        //结构分析和设计计算。
                        _ = weld.EffectiveThroatAbove;

                        //在Tekla Structures软件中，LengthAbove是一个浮点数属性，用于定义基础焊缝对象上方的长度。
                        //具体来说，在给定的输入中，LengthAbove的值被设置为100，表示基础焊缝对象上方的长度为100单位。该属性通常用于描述接头焊缝等连接部件的特征，
                        //它可以影响焊缝的强度和承载能力等性能指标。通过设置适当的LengthAbove属性值，可以在Tekla Structures软件中明确指定基础焊缝对象上方的长度，
                        //并支持后续的结构分析和设计计算。需要注意的是，在设置LengthAbove属性时，应根据具体的设计要求和规范来确定适当的长度值，并与相关的工程师或
                        //设计团队进行沟通，以确保焊缝的设计满足要求并符合安全性和质量要求。
                        _ = weld.LengthAbove;

                        //WeldNDTInspectionEnum枚举类型定义了用于指定基础焊缝对象的不同无损检测类型的值。其中，WELD_NDT_INSPECTION_NONE表示无无损检测，即没有对焊缝进行
                        //任何无损检测。
                        _ = weld.NDTInspection;

                        //在Tekla Structures软件中，IntermittentType是一个枚举类型属性，用于定义基础焊缝对象的间歇性焊接类型。
                        //在给定的输入中，IntermittentType的值被设置为CHAIN_INTERMITTENT，表示间歇性焊接类型为"CHAIN_INTERMITTENT"。该类型通常用于描述一系列间隔焊缝连接，
                        //焊缝之间存在一定的间距。IntermittentType属性的可选值由WeldIntermittentTypeEnum枚举类型定义，它包括多种间歇性焊接类型，如以下示例：
                        //NO_INTERMITTENT: 没有间歇性焊接。
                        //RANDOM_INTERMITTENT: 随机间歇性焊接。
                        //CHAIN_INTERMITTENT: 链式间歇性焊接。
                        //STAGGERED_INTERMITTENT: 交错间歇性焊接。
                        //OTHER: 其他类型的间歇性焊接。
                        //通过指定适当的IntermittentType属性值，可以在Tekla Structures软件中明确指定基础焊缝对象的间歇性焊接类型。这有助于更准确地描述焊缝的几何形状和连接方式，
                        //并支持后续的结构分析和设计计算。需要注意的是，在设置IntermittentType属性时，应根据具体的焊接要求和规范来选择适当的类型，并与相关的工程师或设计团队进行
                        //沟通，以确保焊缝的设计满足要求并符合安全性和质量要求
                        _ = weld.IntermittentType;

                        //在Tekla Structures软件中，ProcessType是一个枚举类型属性，用于定义基础焊缝对象所采用的焊接工艺类型。
                        //在给定的输入中，ProcessType的值被设置为WELD_PROCESS_NONE，表示焊接工艺类型为"无"。该值通常用于表示该焊缝对象没有采用特定的焊接工艺，
                        //或者工艺信息不适用于当前场景。
                        //ProcessType属性的可选值由WeldProcessTypeEnum枚举类型定义，它包括多种焊接工艺类型，如以下示例：
                        //WELD_PROCESS_NONE: 无焊接工艺。
                        //WELD_PROCESS_SAW: 电子束焊接。
                        //WELD_PROCESS_MIGMAG: 气体保护焊接。
                        //WELD_PROCESS_TIG: 氩弧焊接。
                        //WELD_PROCESS_STICK: 手工电弧焊接。
                        //WELD_PROCESS_SUBMERGED: 焊接半自动埋弧焊。
                        //通过指定适当的ProcessType属性值，可以在Tekla Structures软件中明确指定基础焊缝对象所采用的焊接工艺类型。这有助于更准确地描述焊缝的特征，
                        //并支持后续的结构分析和设计计算。需要注意的是，在设置ProcessType属性时，应根据具体的焊接要求和规范来选择适当的类型，以确保焊缝的设计满足
                        //要求并符合安全性和质量要求。
                        _ = weld.ProcessType;


                        //当ShopWeld的值为true时，表示该焊缝是车间焊接。这意味着焊缝将在车间环境中进行焊接操作，而不是在现场进行。车间焊接通常具有更好的控制和条件，可以提供更
                        //高的焊接质量和效率。相反，当ShopWeld的值为false时，表示该焊缝是现场焊接。现场焊接是指焊缝将在建筑工地或现场环境中进行焊接。现场焊接可能受到天气、环境
                        //条件和其他因素的限制，因此焊接质量和效率可能相对较低。
                        _ = weld.ShopWeld;

                        // 具体来说，SizeAbove表示基础焊缝对象上方的尺寸值。这个尺寸通常用于描述焊缝的宽度、厚度或其他相关尺寸参数。单位可能取决于所使用的工程单位制。
                        //在这种情况下，SizeAbove的值为10，意味着基础焊缝对象的上方尺寸为10。然而，请注意，具体的尺寸定义和含义可能会因不同的焊缝类型和设计要求而有所变化。
                        _ = weld.SizeAbove;

                        //WeldTypeEnum枚举类型定义了不同类型的焊缝，包括角焊缝（fillet weld）、对接焊缝（butt weld）等。其中，WELD_TYPE_FILLET表示角焊缝类型。
                        //角焊缝（fillet weld）是一种连接两个相交构件的焊缝类型，常用于连接拐角处或T形接头处。它的形状类似于一个直角三角形，将两个构件的侧面连接在一起。
                        //通过将TypeAbove属性设置为WELD_TYPE_FILLET，可以在Tekla Structures软件中明确指定基础焊缝对象上方的焊缝类型为角焊缝。这有助于准确描述焊缝的类
                        //型，并在绘图和报告中正确显示相关信息。
                        _ = weld.TypeAbove;

                        //当ConnectAssemblies的值为false时，表示不进行构件组件的连接。这意味着构件将保持独立状态，不会通过焊接、螺栓或其他方式与其他构件连接在一起。
                        //相反，当ConnectAssemblies的值为true时，表示进行构件组件的连接。这意味着构件将通过适当的连接方法（如焊接或螺栓）与其他构件进行连接。连接的
                        //方式和方法将根据具体的设计和施工要求来确定。通过设置ConnectAssemblies属性，可以在Tekla Structures软件中明确指定是否进行构件组件的连接，
                        //并根据需要进行相应的建模和分析。这有助于提供准确的结构模型，并支持后续的设计、施工和维护过程。
                        _ = weld.ConnectAssemblies;
                        #endregion


                        if (weld != null)
                        {
                            WeldStructure ws = new WeldStructure();

                            ArrayList pointArray = weld1.Polygon.Points;

                            ws.IsShopWeld = weld.ShopWeld;

                            if (weld.ShopWeld)
                            {
                                ws.WeldId = weld.Identifier.ID;

                                ws.PolygonWeld = weld;

                                if (pointArray.Count > 1)
                                {
                                    Point p1 = (Point)pointArray[0];

                                    Point p2 = (Point)pointArray[1];
                                    //中心点
                                    Point p = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, (p1.Z + p2.Z) / 2);
                                    //长度
                                    double distance = Math.Sqrt(
                                        Math.Pow((p2.X - p1.X), 2) +
                                            Math.Pow((p2.Y - p1.Y), 2) +
                                            Math.Pow((p2.Z - p1.Z), 2));

                                    ws.StartPoint = p1;

                                    ws.EndPoint = p2;

                                    ws.CentraPoint = p;

                                    ws.WeldLength = distance;
                                }
                            }

                            ws.WeldMainPart = weld.MainObject;

                            ws.WeldSecondPart = weld.SecondaryObject;

                            ws.WeldWidth = weld.SizeAbove;

                            ws.WeldType = weld.TypeAbove.ToString();

                            // ws.WeldUnitOrder =  

                            // 获取焊缝的连接部件
                            ModelObject mainPart = weld.MainObject;

                            ModelObject secondaryPart = weld.SecondaryObject;

                            ws.WeldProcessType = weld.ProcessType.ToString();

                            ws.WeldReferenceText = weld.ReferenceText;

                            Solid weldSolid = weld.GetSolid();

                            FaceEnumerator weldFaceEnum = weldSolid.GetFaceEnumerator();


                            Dictionary<string, List<Point>> dicFacePointList = new Dictionary<string, List<Point>>();

                            Dictionary<string, Vector> dicFaceNormalList = new Dictionary<string, Vector>();

                            int faceID = 1;

                            //获取焊缝face
                            while (weldFaceEnum.MoveNext())
                            {
                                Face faceCurrent = weldFaceEnum.Current;

                                List<Point> pointList = new List<Point>();

                                if (weldFaceEnum.Current is Face MyFace)
                                {
                                    LoopEnumerator myLoopEnum = MyFace.GetLoopEnumerator();
                                    while (myLoopEnum.MoveNext())
                                    {
                                        if (myLoopEnum.Current is Loop MyLoop)
                                        {
                                            VertexEnumerator myVertexEnum = MyLoop.GetVertexEnumerator();
                                            while (myVertexEnum.MoveNext())
                                            {
                                                Point myVertex = myVertexEnum.Current;
                                                if (myVertex != null && !pointList.Contains(myVertex))
                                                {
                                                    pointList.Add(myVertex);
                                                }
                                            }
                                        }
                                    }
                                }

                                dicFacePointList.Add($"{faceCurrent.OriginPartId}_{faceID}", pointList);

                                dicFaceNormalList.Add($"{faceCurrent.OriginPartId}_{faceID}", faceCurrent.Normal);

                                faceID++;
                            }

                            ws.NormalList = new List<Vector>();

                            ws.FaceWeldAndPartInfo = new List<Tuple<string, List<Point>, List<Point>>>();

                            //主
                            GetSurfaceNormal(mainPart as Part, ws, dicFacePointList, dicFaceNormalList);
                            //辅
                            GetSurfaceNormal(secondaryPart as Part, ws, dicFacePointList, dicFaceNormalList);


                            //测试计算
                            ws.CameraPoint = CreateCameraPosition(
                                ws.StartPoint,
                                ws.EndPoint,
                                45,
                                150,
                                ws.NormalList[0],
                                ws.NormalList[1]
                                );

                            WeldList.Add(ws);
                        }



                    }


                }
            }


            //Tekla.Structures.Model.UI.ModelObjectSelector modelObjectSelector = new Tekla.Structures.Model.UI.ModelObjectSelector();

            //ModelObjectEnumerator modelObjectEnumerator = modelObjectSelector.GetSelectedObjects();

            //ListPoints(modelObjectEnumerator);


            JsonWeldData = JsonConvert.SerializeObject(WeldList); 

          

        }


        /// <summary>Gets the surface normal.</summary>
        /// <param name="part">The part.</param>
        /// <param name="ws">The ws.</param>
        /// <param name="dicWeldFacePointList">The dic weld face point list.</param>
        /// <param name="dicWeldFaceNormalList">The dic weld face normal list.</param>
        private void GetSurfaceNormal(Part part, WeldStructure ws, Dictionary<string, List<Point>> dicWeldFacePointList, Dictionary<string, Vector> dicWeldFaceNormalList)
        {
            if (part != null)
            {
                if (IsBeam(part))
                {
                    Beam b = part as Beam;
                    //Point sPoint = b.StartPoint;
                    //Point ePoint = b.EndPoint;
                    ////尺寸信息
                    //string profile = b.Profile.ProfileString;

                    //// 计算梁的长度
                    //double length = Math.Round(Math.Sqrt(Math.Pow(ePoint.X - sPoint.X, 2) + Math.Pow(ePoint.Y - sPoint.Y, 2) + Math.Pow(ePoint.Z - sPoint.Z, 2)), 2);

                    ws.NormalList.Add(GetPartNormal(b, dicWeldFacePointList, dicWeldFaceNormalList, ws));

                }

                if (IsPlate(part))
                {
                    ContourPlate p = part as ContourPlate;

                    // 使用正则表达式匹配最后一组连续的数字
                    Match match = Regex.Match(p.Profile.ProfileString, @"\d+$");

                    if (match.Success)
                    {
                        ws.PlateThick = Convert.ToInt32(match.Value);
  
                    }
 

                    ws.NormalList.Add(GetPartNormal(p, dicWeldFacePointList, dicWeldFaceNormalList, ws));
                }
            }
        }







        /// <summary>Creates the camera position.</summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="angleDegrees">The angle degrees.</param>
        /// <param name="distance">The distance.</param>
        /// <param name="normalVector1">The normal vector1.</param>
        /// <param name="normalVector2">The normal vector2.</param>
        /// <returns>Point.</returns>
        /// TODO Edit XML Comment Template for CreateCameraPosition
        public Point CreateCameraPosition(Point startPoint, Point endPoint,double angleDegrees, double distance, Vector normalVector1, Vector normalVector2)
        {
            // 将角度转换为弧度
            double angleRadians = angleDegrees * Math.PI / 180;


            // 计算从起点到终点的向量
            Vector directionVector = new Vector(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z);

            // 计算与给定面法线垂直的向量
            Vector normalToPlaneVector = normalVector1.Cross(GetPerpendicularVector(normalVector2));

            // 计算方向向量的平行部分
            Vector parallelComponent = Vector.Dot(directionVector, normalToPlaneVector) * normalToPlaneVector;

            // 计算与给定角度成45度的单位向量
            Vector rotatedUnitVector = new Vector(Math.Cos(angleRadians), Math.Sin(angleRadians), 0);

            // 计算与给定距离成比例的向量
            Vector scaledVector = rotatedUnitVector * distance;

            // 计算所需点的坐标
            Point requiredPoint = new Point(startPoint.X + parallelComponent.X + scaledVector.X,
                                            startPoint.Y + parallelComponent.Y + scaledVector.Y,
                                            startPoint.Z + parallelComponent.Z + scaledVector.Z);

            return requiredPoint;
 
        }


        /// <summary>
        /// 根据给定向量, 创建垂直向量
        /// </summary>
        /// <param name="normalVector">The normal vector.</param>
        /// <returns>Vector.</returns>
        private Vector GetPerpendicularVector(Vector normalVector)
        {
            // 创建一个任意的向量
            Vector arbitraryVector = new Vector(1, 0, 0);

            // 计算与给定向量垂直的向量
            Vector perpendicularVector = normalVector.Cross(arbitraryVector);

            // 如果垂直向量为零向量，则使用不同的任意向量
            if (perpendicularVector.GetLength() == 0)
            {
                arbitraryVector = new Vector(0, 1, 0);
                perpendicularVector = normalVector.Cross(arbitraryVector);
            }

            return perpendicularVector;
        }

        /// <summary>
        /// 基于焊缝接触的面, 计算焊缝到模型的距离为0, 则这个面就是于焊缝相接的面,取出对应的法向
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        private Vector GetPartNormal(Part part, Dictionary<string, List<Point>> dicWeldFacePointList, Dictionary<string, Vector> dicFaceNormalList, WeldStructure ws)
        {
            Solid solid = part.GetSolid();

            _ = new TransformationPlane(part.GetCoordinateSystem());

            Face currentFace = null;


            ArrayList myfaceNormalList = new ArrayList();

            FaceEnumerator MyFaceEnum = solid.GetFaceEnumerator();

            List<Point> pointList = null;

            while (MyFaceEnum.MoveNext())
            {
                pointList = new List<Point>();
                _ = MyFaceEnum.Current;
                if (MyFaceEnum.Current is Face MyFace)
                {
                    currentFace = MyFace;

                    _ = myfaceNormalList.Add(MyFace.Normal);

                    LoopEnumerator myLoopEnum = MyFace.GetLoopEnumerator();
                    while (myLoopEnum.MoveNext())
                    {
                        if (myLoopEnum.Current is Loop MyLoop)
                        {
                            VertexEnumerator myVertexEnum = MyLoop.GetVertexEnumerator();
                            while (myVertexEnum.MoveNext())
                            {
                                Point myVertex = myVertexEnum.Current;

                                if (myVertex != null)
                                {
                                    if (!pointList.Contains(myVertex))
                                    {
                                        pointList.Add(myVertex);
                                    }
                                   
                                }
                            }
                        }
                    }
                }

                double x = 0.0, y = 0.0, z = 0.0;

                foreach(Point p in pointList)
                {
                    x += p.X;
                    y += p.Y;
                    z += p.Z;
                }

                // 计算面的中心点
                Point centerPoint = new Point(Math.Round(x / pointList.Count,1), Math.Round(y/ pointList.Count,1), Math.Round(z / pointList.Count,1));

                // 创建向量对象作为法线方向
                Vector planeNormal = new Vector(currentFace.Normal.X, currentFace.Normal.Y, currentFace.Normal.Z);

                // 创建 GeometricPlane 对象
                GeometricPlane currentPlane = new GeometricPlane(centerPoint, planeNormal);
                foreach (KeyValuePair<string, List<Point>> dic in dicWeldFacePointList)
                {
                    double totalDistance = 0.0;

                    string weldFaceID = dic.Key;

                    foreach (Point p in dic.Value)
                    {
                        double dis = Distance.PointToPlane(p, currentPlane);

                        totalDistance += dis;
                    }

                    if (totalDistance == 0)
                    {
                        Vector partNormal = currentFace.Normal;

                        Vector weldNormal = dicFaceNormalList[weldFaceID];

                        // 计算这两个 Normal 对象的点积,法向相反

                        double dotProduct = Vector.Dot(partNormal, weldNormal);
                        //考虑可能有误差,放点容差
                        if(dotProduct < 1e-9)
                        {
                            MinimumBoundingRectangle3D rectangle3D = new MinimumBoundingRectangle3D();

                            rectangle3D.GetMinRectangle(pointList);

                            ws.FaceWeldAndPartInfo.Add( new Tuple<string, List<Point>, List<Point>>(weldFaceID, dic.Value, pointList));

                            return partNormal;
                        }
                    }
                }

            }

            return new Vector(new Point(-1,-1,-1));
        }


        private Vector GetPartNormal(Part part, ArrayList weldNormalList)
        {

            Solid solid = part.GetSolid();
            _ = new ArrayList();

            FaceEnumerator MyFaceEnum = solid.GetFaceEnumerator();

            while (MyFaceEnum.MoveNext())
            {
                _ = MyFaceEnum.Current;
                if (MyFaceEnum.Current is Face MyFace)
                {

                    //Distance.PointToPlane
                    //  var d1 = Distance.PointToPoint(startPoint, pointA);
                    //   var d2 = Distance.PointToPoint(startPoint, pointA);

                   // myfaceNormalList.Add(MyFace.Normal);


                    if (weldNormalList.Contains(MyFace.Normal))
                    {
                        return MyFace.Normal;
                    }

                }
            }

            return new Vector();
        }




        /// <summary>
        /// 获取面的顶点坐标
        /// </summary>
        /// <param name="face">The face.</param>
        /// <returns>List&lt;Point&gt;.</returns>
        private static List<Point> GetFacePoint(Face face)
        {
            List<Point> pointlist = new List<Point>();
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            loopEnumerator.MoveNext();
            VertexEnumerator vertexEnumerator = loopEnumerator.Current.GetVertexEnumerator();
            while (vertexEnumerator.MoveNext())
            {
                Point current = vertexEnumerator.Current;
                if (!pointlist.Contains(current))
                {
                    pointlist.Add(current);
                }
            }
            ClearPolygonsLine(ref pointlist);
            return pointlist;
        }

        public static void ClearPolygonsLine(ref List<Point> pointlist, double EPSILON = 0.1)
        {
            for (int num = pointlist.Count - 1; num >= 0; num--)
            {
                Point point = GetPoint(pointlist, num);
                Point p = GetPoint(pointlist, num - 1);
                Point p2 = GetPoint(pointlist, num + 1);
                Line line = new Line(p, p2);
                if (Distance.PointToLine(point, line) < EPSILON)
                {
                    pointlist.RemoveAt(num);
                }
            }
        }

        public static Point GetPoint(List<Point> pointlist, int index)
        {
            if (index >= pointlist.Count)
            {
                index -= pointlist.Count;
            }
            else if (index < 0)
            {
                index += pointlist.Count;
            }
            try
            {
                return new Point(pointlist[index]);
            }
            catch (Exception)
            {
                return GetPoint(pointlist, index);
            }
        }

        private bool IsPlate(Part part)
        {
            return part is BentPlate || part is ContourPlate ||  part is LoftedPlate;
        }

        private bool IsBeam(Part part)
        {
            return part is Beam || part is PolyBeam || part is SpiralBeam;
        }


        public void ExportIFC()
        {
        } 
        public  void ListPoints(ModelObjectEnumerator modelObjectEnumerator)
        {
            List<Point> points = new List<Point>();

            while (modelObjectEnumerator.MoveNext())
            {
                if (modelObjectEnumerator.Current is Beam)
                {
                    Beam beam = (Beam)modelObjectEnumerator.Current;

                    if (!points.Contains(beam.StartPoint))
                    {
                        points.Add(beam.StartPoint);
                    }

                    if (!points.Contains(beam.EndPoint))
                    {
                        points.Add(beam.EndPoint);
                    }
                }
            }

            foreach (Point point in points)
            {
                Console.WriteLine(point);
            }
        }

    }


    public class userAssembly
    {
        public List<Assembly> modelassemblylist = new List<Assembly>();

        public string assemblypos;
    }
    public class WeldJson
    {
        public int workpieceId;

        public JsonCoc coord;

        //    public JsonCoc.JsonWeldSeam[] weldSeams;

        public JsonWeldUnit[] weldUnit;

        public JsonProcedure[] procedure;

        public int placehold;
    }
    public class JsonCoc
    {
        public JsonVector axisX;

        public JsonVector axisY;

        public JsonVector axisZ;

        public JsonPoint origin;

        public JsonCoc(CoordinateSystem coc)
        {
            if (coc != null)
            {
                origin = new JsonPoint(coc.Origin);
                axisX = new JsonVector(coc.AxisX);
                axisY = new JsonVector(coc.AxisY);
                Vector vector = Tekla.Structures.Geometry3d.Vector.Cross(coc.AxisX, coc.AxisY);
                axisZ = new JsonVector(vector);
            }
        }
    }
    public class JsonVector
    {
        public double x;

        public double y;

        public double z;

        public JsonVector(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public JsonVector()
        {
        }

        public JsonVector(Vector vector)
        {
            if (!(vector == null))
            {
                x = Math.Round(vector.X, 2);
                y = Math.Round(vector.Y, 2);
                z = Math.Round(vector.Z, 2);
            }
        }

        public static JsonVector operator *(JsonVector Vector, double Multiplier)
        {
            return new JsonVector
            {
                x = Multiplier * Vector.x,
                y = Multiplier * Vector.y,
                z = Multiplier * Vector.z
            };
        }

        public static JsonVector operator *(double Multiplier, JsonVector Vector)
        {
            return new JsonVector
            {
                x = Multiplier * Vector.x,
                y = Multiplier * Vector.y,
                z = Multiplier * Vector.z
            };
        }
    }
    public class JsonPoint
    {
        public double x;

        public double y;

        public double z;

        public JsonPoint(Point point)
        {
            if (!(point == null))
            {
                x = Math.Round(point.X, 2);
                y = Math.Round(point.Y, 2);
                z = Math.Round(point.Z, 2);
            }
        }
    }
    public class JsonWeldSeam
    {
        public int id;

        public int unitId;

        public int procId;

        public JsonPoint start;

        public JsonPoint end;

        public JsonVector[] planeNormal;

        public bool convex;

        public double[] plateThick;
    }
    public class JsonWeldUnit
    {
        public int id;

        public int weldType;

        public int procId;

        public int[] seams;
    }
    public class JsonProcedure
    {
        public int id;

        public JsonCoc coord;

        public JsonVector normal;

        public int[] unit;
    }
    public class StructuresData
    {
        public double ToleranceLineToFace = 2.0;

        public string WeldPrefix;

        public double MaxOffset = 20.0;

        public int CreatCocType;

        public int FirstOrderName;

        public int FirstOrderRound;

        public int FirstOrderType;

        public int SecOrderName;

        public int SecOrderRound;

        public int SecOrderType;

        public int ThirdOrderName;

        public int ThirdOrderRound;

        public int ThirdOrderType;

        public int DefaultNormal;

        public int DefulatVectorX;

        public int IsConsiderWeldDirection;

        public double LengthLimit;

        public int IsCreatMainWeld;

        public int ProcessCountType;

        public string AssemblyComment;

        public int IsCreatAngleWeld;

        public int IsCreatButtWeld;

        public int IsCreatLapWeld;

        public string PartNameList;

        public int IsCreatUnGroupWeld;

        public double WeldDisLimit;

        public int IsCreatSingleVerticalWeld;
    }

}
