using AvalonDock.Layout;
using BRToolBox.Class;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using IRW.LOG;
using Microsoft.Win32;
using Newtonsoft.Json;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;
using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Common.Step21;
using Xbim.Common.XbimExtensions;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc2x3.GeometricConstraintResource;
using Xbim.Ifc2x3.GeometricModelResource;
using Xbim.Ifc2x3.GeometryResource;
using Xbim.Ifc2x3.Interfaces;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.PropertyResource;
using Xbim.Ifc2x3.RepresentationResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using static Xbim.ModelGeometry.Scene.XbimPlacementTree;
using Color = SharpDX.Color;
using LineSegment = BRToolBox.Class.LineSegment;

namespace BRToolBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Model3D Model { get; set; }

        TeklaReader teklaReader = null;

        int ratio = 100;

        private XbimControl xbimControl = new XbimControl();

        Xbim3DModelContext context;

        DrawingControl3D Viewer3D = null;

        Viewport3DX ViewerDX = null;

        List<List<Vector3>> FacePointList = new List<List<Vector3>>();

        private string selectedModelGuid = string.Empty;

        private MeshGeometryModel3D selectedModel = null;

        private HelixToolkit.Wpf.SharpDX.Material selectedMaterial = null;

        private Dictionary<string, ArrayList> dicWeldCoordinate = new Dictionary<string, ArrayList>();

        #region ViewPort3DX Parameters


        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //11111111
            //2222
            //4444444
            //Test
            //master
            // attach window managment functions
            Closed += MainWindow_Closed;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            WindowState = WindowState.Maximized;

            ViewerDX = HXView3DX;

            // GDIFCViewer.Children.Add(xbimControl);

            //LoadPaint();

            // LoadPaintDX(HXView3DX,100);

            LoadModel();

        }

        private void LoadModel()
        {
            LoadTeklaModel(ViewerDX);

            LoadTeklaModelDX(ViewerDX);

            //DrawBeam();

            //CreateCameraPosition();

            ViewerDX.MouseDown3D += ViewerDX_MouseDown3D;

            ViewerDX.MouseDown += ViewerDX_MouseDown;



        }


        private void ViewerDX_MouseDown3D(object sender, RoutedEventArgs e)
        {
            if (e is MouseDown3DEventArgs args && args.HitTestResult != null)
            {
                string btn = ((MouseButtonEventArgs)((Mouse3DEventArgs)e).OriginalInputEventArgs).ChangedButton.ToString();

                if(btn == "Right")
                {
                    return;
                }

                object hit = args.HitTestResult.ModelHit;

                if(hit is MeshGeometryModel3D)
                {
                    MeshGeometryModel3D model3D = (MeshGeometryModel3D)args.HitTestResult.ModelHit;

                    if (!string.IsNullOrEmpty(selectedModelGuid))
                    {
                        if (selectedModelGuid != model3D.GUID.ToString())
                        {
                            selectedModel.Material = selectedMaterial;

                            selectedModel = model3D;

                            selectedMaterial = model3D.Material;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        selectedModel = model3D;

                        selectedMaterial = model3D.Material;

                        selectedModelGuid = model3D.GUID.ToString();
                    }

                    model3D.Material = new PhongMaterial()
                    {
                        AmbientColor = Color.Yellow,
                        EmissiveColor = Color.Yellow,
                        ReflectiveColor = Color.Yellow,
                        SpecularColor = Color.Yellow
                    };



                    //((HelixToolkit.Wpf.SharpDX.PhongMaterial)model3D.Material).EmissiveColor ReflectiveColor

                    //PhongMaterial phongMaterial = (PhongMaterial)model3D.Material;

                    //if(phongMaterial.EmissiveColor.Red == 0 && phongMaterial.EmissiveColor.Green == 0 && phongMaterial.EmissiveColor.Blue == 0)
                    //{
                    //    if(phongMaterial.ReflectiveColor.Red > 0 || phongMaterial.ReflectiveColor.Green > 0 || phongMaterial.ReflectiveColor.Blue > 0)
                    //    {
                    //        model3D.Material = new PhongMaterial() { ReflectiveColor = SharpDX.Color.Yellow };
                    //    }

                    //}
                    //else
                    //{
                    //    model3D.Material = new PhongMaterial() { EmissiveColor = SharpDX.Color.Yellow };
                    //}
                }

            }
        }


        private void ViewerDX_MouseDown(object sender, MouseButtonEventArgs e)
        {

            // Get the mouse's position relative to the viewport.
            System.Windows.Point mouse_pos = e.GetPosition(ViewerDX);

            // Perform the hit test.
            System.Windows.Media.HitTestResult result =
                VisualTreeHelper.HitTest(ViewerDX, mouse_pos);

            // Display information about the hit.
            RayMeshGeometry3DHitTestResult mesh_result =
                result as RayMeshGeometry3DHitTestResult;
            if (mesh_result == null) this.Title = "";
            else
            {
                // Display the name of the model.
                Title = mesh_result.ModelHit.GetName();

                // Display more detail about the hit.
                Console.WriteLine("Distance: " +
                    mesh_result.DistanceToRayOrigin);
                Console.WriteLine("Point hit: (" +
                    mesh_result.PointHit.ToString() + ")");

                Console.WriteLine("Triangle:");
                System.Windows.Media.Media3D.MeshGeometry3D mesh = mesh_result.MeshHit;
                Console.WriteLine("    (" +
                    mesh.Positions[mesh_result.VertexIndex1].ToString()
                        + ")");
                Console.WriteLine("    (" +
                    mesh.Positions[mesh_result.VertexIndex2].ToString()
                        + ")");
                Console.WriteLine("    (" +
                    mesh.Positions[mesh_result.VertexIndex3].ToString()
                        + ")");
            }



            IList<HelixToolkit.Wpf.SharpDX.HitTestResult> hits = ViewerDX.FindHits(e.GetPosition(ViewerDX));
            if (hits.Count > 0)
            {
                Dictionary<string, double> hitElements = new Dictionary<string, double>();

                foreach (HelixToolkit.Wpf.SharpDX.HitTestResult hit in hits.Where(h => h.IsValid))
                {
                    HelixToolkit.Wpf.SharpDX.Geometry3D geometry = hit.Geometry;
                    hitElements.Add(geometry.GUID.ToString(), hit.Distance);

                    double minDistance = hitElements.First().Value;
                    string id_of_hit_element = hitElements.First().Key;

                    foreach (KeyValuePair<string, double> hmin in hitElements)
                    {
                        if (hmin.Value < minDistance)
                        {
                            minDistance = hmin.Value;
                            id_of_hit_element = hmin.Key;
                        }
                    }
                }
            }

            ////射线检测是否会发生Hit
            //System.Windows.Point mousePos = e.GetPosition(ViewerDX);
            //PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            //System.Windows.Media.HitTestResult result = VisualTreeHelper.HitTest(ViewerDX, mousePos);
            //RayMeshGeometry3DHitTestResult rayMeshResult = result as RayMeshGeometry3DHitTestResult;
            //if (rayMeshResult != null)
            //{
            //    System.Windows.Media.Media3D.MeshGeometry3D mesh = new System.Windows.Media.Media3D.MeshGeometry3D();
            //    mesh.Positions.Add(rayMeshResult.MeshHit.Positions[rayMeshResult.VertexIndex1]);
            //    mesh.Positions.Add(rayMeshResult.MeshHit.Positions[rayMeshResult.VertexIndex2]);
            //    mesh.Positions.Add(rayMeshResult.MeshHit.Positions[rayMeshResult.VertexIndex3]);
            //    mesh.TriangleIndices.Add(0);
            //    mesh.TriangleIndices.Add(1);
            //    mesh.TriangleIndices.Add(2);
            //    System.Windows.Media.Media3D.GeometryModel3D marker = new System.Windows.Media.Media3D.GeometryModel3D(mesh, new System.Windows.Media.Media3D.DiffuseMaterial(Brushes.Blue));
            //    //...add marker to the scene...
            //}
        }



 
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            CloseAndDeleteTemporaryFiles();
        }
        private void CloseAndDeleteTemporaryFiles()
        {
            try
            {
                if (Model != null)
                {
                    xbimControl.ModelProvider.ObjectInstance = null;
                    xbimControl.ModelProvider.Refresh();
                }

            }
            finally
            {

            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {


            // 获取内部坐标系元素
             Viewer3D = DrawingControl;




        }

        public Visibility AnyErrors
        {
            get
            {
                return NumErrors > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public int NumErrors { get; private set; }

        public Visibility AnyWarnings
        {
            get
            {
                return NumWarnings > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public int NumWarnings { get; private set; }


        public IPersistEntity SelectedItem
        {
            get => (IPersistEntity)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }


        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(IPersistEntity), typeof(MainWindow),
                                        new UIPropertyMetadata(null, OnSelectedItemChanged));


        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mw = d as MainWindow;
            if (mw != null && e.NewValue is IPersistEntity)
            {
                IPersistEntity label = (IPersistEntity)e.NewValue;
                mw.EntityLabel.Text = label != null ? "#" + label.EntityLabel : "";
            }
            else if (mw != null)
            {
                mw.EntityLabel.Text = "";
            }
        }

        private void LoadTeklaModel(Viewport3DX viewport3D)
        {
            teklaReader = new TeklaReader(viewport3D);
            teklaReader.LoadModel();
        }

        /// <summary>
        /// Loadeds the paint.
        /// </summary>
        //private void LoadPaint()
        //{
        //    // Create a teklaModel group
        //    Model3DGroup modelGroup = new Model3DGroup();
        //    // Create a mesh builder and add a box to it
        //    HelixToolkit.Wpf.MeshBuilder meshBuilder = new HelixToolkit.Wpf.MeshBuilder(false, false);

        //    meshBuilder.AddBox(new Point3D(0, 0, 1), 1, 2, 0.5);
        //    meshBuilder.AddBox(new Rect3D(0, 0, 1.2, 0.5, 1, 0.4));

        //    // Create a mesh from the builder (and freeze it)
        //    System.Windows.Media.Media3D.MeshGeometry3D mesh = meshBuilder.ToMesh(true);

        //    // Create some materials
        //    System.Windows.Media.Media3D.Material greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
        //    System.Windows.Media.Media3D.Material redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
        //    System.Windows.Media.Media3D.Material blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
        //    System.Windows.Media.Media3D.Material insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);

        //    // Add 3 models to the group (using the same mesh, that's why we had to freeze it)
        //    modelGroup.Children.Add(new System.Windows.Media.Media3D.GeometryModel3D { Geometry = mesh, Material = greenMaterial, BackMaterial = insideMaterial });
        //   // modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(-2, 0, 0), Material = redMaterial, BackMaterial = insideMaterial });
        //   // modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(2, 0, 0), Material = blueMaterial, BackMaterial = insideMaterial });

        //    // 创建一个模型视图器
        //    ModelVisual3D modelVisual = new ModelVisual3D
        //    {
        //        Content = modelGroup
        //    };

        //    // 将模型视图器添加到视口3D的视图集合中
        //    viewport3D.Children.Add(modelVisual);
        //}

        /// <summary>
        /// SHarpDX
        /// .
        /// </summary>
        private void LoadPaintDX(Viewport3DX viewport3DX)
        {
            HelixToolkit.Wpf.SharpDX.OrthographicCamera camera = new HelixToolkit.Wpf.SharpDX.OrthographicCamera()
            {
                Width = 300, // 设置相机的宽度，与模型的宽度一致
                Position = new Point3D(0, 0, 10000) // 设置相机的位置，根据模型的尺寸和位置进行调整
            };

            // 将OrthographicCamera设置为视图相机
            viewport3DX.Camera = camera;

            viewport3DX.BackgroundColor = Colors.Black;
            viewport3DX.EffectsManager = new DefaultEffectsManager();
            viewport3DX.ShowCoordinateSystem = true;
            viewport3DX.ShowFrameRate = true;

            viewport3DX.Items.Add(new DirectionalLight3D() { Direction = new Vector3D(-1, -1, -1) });
            viewport3DX.Items.Add(new AmbientLight3D() { Color = System.Windows.Media.Color.FromArgb(255, 50, 50, 50) });

            // 网格
            LineMaterialGeometryModel3D lineMaterialGeometryModel3D = new LineMaterialGeometryModel3D()
            {
                Geometry = LineBuilder.GenerateGrid(),
                IsRendering = true,
                Material = new LineMaterial() { Color = Colors.Gray, TextureScale = 0.4 },
                Transform = new TranslateTransform3D(-5, -1, -5),
            };
            viewport3DX.Items.Add(lineMaterialGeometryModel3D);

            SceneNodeGroupModel3D sceneNodeGroup = new SceneNodeGroupModel3D();
            viewport3DX.Items.Add(sceneNodeGroup);


    


            HelixToolkit.Wpf.SharpDX.MeshBuilder b1 = new HelixToolkit.Wpf.SharpDX.MeshBuilder();
            HelixToolkit.Wpf.SharpDX.MeshBuilder b2 = new HelixToolkit.Wpf.SharpDX.MeshBuilder();
            HelixToolkit.Wpf.SharpDX.MeshBuilder b3 = new HelixToolkit.Wpf.SharpDX.MeshBuilder();


            // 上下截面    缩小100倍  WI300-15-20*300
            b1.AddBox(new Vector3(0, 0, 0), 3f, 0.2f, 10f);
            b2.AddBox(new Vector3(0, 2.8f, 0), 3f, 0.2f, 10f);
            // 中间梁
            b3.AddBox(new Vector3(0, 1.4f, 0), 0.15f, 2.6f, 10f);

            MeshGeometryModel3D mesh1 = RenderObject(b1.ToMeshGeometry3D(), Colors.Red.ToColor4());

            viewport3DX.Items.Add(mesh1);

            MeshGeometryModel3D mesh2 = RenderObject(b2.ToMeshGeometry3D(), Colors.Red.ToColor4());
            viewport3DX.Items.Add(mesh2);

            MeshGeometryModel3D mesh3 = RenderObject(b3.ToMeshGeometry3D(), Colors.Blue.ToColor4());
            viewport3DX.Items.Add(mesh3);



            //HelixToolkit.Wpf.SharpDX.MeshGeometry3D meshGeometry3D_1 = b1.ToMeshGeometry3D();
            //MeshGeometryModel3D meshGeometryModel3D_1 = new MeshGeometryModel3D
            //{
            //    Geometry = meshGeometry3D_1,
            //    Transform = new TranslateTransform3D(0, 0, 0)
            //};
            //PhongMaterial material = new PhongMaterial();
            //material = PhongMaterials.Red;
            //meshGeometryModel3D_1.Material = material;

            //meshGeometryModel3D_1.IsTransparent = true;

            //viewport3DX.Items.Add(meshGeometryModel3D_1);
        }

        /// <summary>
        /// SHarpDX
        /// .
        /// </summary>
        private void LoadPaintDX(Viewport3DX viewport3DX, int rate)
        {
            //HelixToolkit.Wpf.SharpDX.OrthographicCamera camera = new HelixToolkit.Wpf.SharpDX.OrthographicCamera()
            //{
            //    Width = 300, // 设置相机的宽度，与模型的宽度一致
            //    Position = new Point3D(0, 0, 10000) // 设置相机的位置，根据模型的尺寸和位置进行调整
            //};

            //// 将OrthographicCamera设置为视图相机
            //viewport3DX.Camera = camera;


            viewport3DX.BackgroundColor = Colors.Black;
            viewport3DX.EffectsManager = new DefaultEffectsManager();
            viewport3DX.ShowCoordinateSystem = true;
            viewport3DX.ShowFrameRate = true;
            viewport3DX.Items.Add(new DirectionalLight3D() { Direction = new Vector3D(-1, -1, -1) });
            viewport3DX.Items.Add(new AmbientLight3D() { Color = System.Windows.Media.Color.FromArgb(255, 50, 50, 50) });

            // 网格
            LineMaterialGeometryModel3D lineMaterialGeometryModel3D = new LineMaterialGeometryModel3D()
            {
                Geometry = LineBuilder.GenerateGrid(),
                IsRendering = true,
                Material = new LineMaterial() { Color = Colors.Gray, TextureScale = 0.4 },
                Transform = new TranslateTransform3D(-1, -1, -1),

            };
            viewport3DX.Items.Add(lineMaterialGeometryModel3D);

            SceneNodeGroupModel3D sceneNodeGroup = new SceneNodeGroupModel3D();
            viewport3DX.Items.Add(sceneNodeGroup);




            //判断模型是否为空,准备重建模型
            if (teklaReader.ModelBeamObjs != null)
            {
                //Task.Run(() => Parallel.ForEach(teklaReader.ModelObjs, item =>
                //{
                //    // Update the progress bar on the Synchronization Context that owns this Form.
                //   // this.Invoke(new Action(() => this.progressBar1.Value++));
                //}));

                foreach (ModelObject obj in teklaReader.ModelBeamObjs)
                {
                    if (obj is Beam beamObject)
                    {
                        //起点坐标转换
                        Vector3 startPoint = new Vector3((float)beamObject.StartPoint.X, (float)beamObject.StartPoint.Y, (float)beamObject.StartPoint.Z);
                        //终点坐标转换
                        Vector3 endPoint = new Vector3((float)beamObject.EndPoint.X, (float)beamObject.EndPoint.Y, (float)beamObject.EndPoint.Z);

                        string sizeInfo = ((Part)obj).Profile.ProfileString;

                        //获取属性   WI300-15-20*300 h-s-t * b
                        float[] sizeArray = ParseString(sizeInfo);


                        if (sizeInfo.StartsWith("HN"))
                        {
                            //HN400*200*8*13 h*b*s*t
                            // float h, float s, float t1, float t2, float b1, float b2
                            DrawGong(
                                viewport3DX,
                                startPoint,
                                endPoint,
                                sizeArray[0],
                                sizeArray[2],
                                sizeArray[3],
                                sizeArray[3],
                                sizeArray[1],
                                sizeArray[1],
                                rate);
                        }
                        if (sizeInfo.StartsWith("WI"))
                        {
                            // WI300-15-20*300 h-s-t * b
                            DrawGong(
                                viewport3DX,
                                startPoint,
                                endPoint,
                                sizeArray[0],
                                sizeArray[1],
                                sizeArray[2],
                                sizeArray[2],
                                sizeArray[3],
                                sizeArray[3],
                                rate);
                        }


                    }
                }
            }

        }




        private void LoadTeklaModelDX(Viewport3DX viewport3DX, int shrinkRate = 100)
        {
            //HelixToolkit.Wpf.SharpDX.OrthographicCamera camera = new HelixToolkit.Wpf.SharpDX.OrthographicCamera()
            //{
            //    Width = 300, // 设置相机的宽度，与模型的宽度一致
            //    Position = new Point3D(0, 0, 10000) // 设置相机的位置，根据模型的尺寸和位置进行调整
            //};

            //// 将OrthographicCamera设置为视图相机
            //viewport3DX.Camera = camera;


            viewport3DX.BackgroundColor = Colors.Black;
            viewport3DX.EffectsManager = new DefaultEffectsManager();
            viewport3DX.ShowCoordinateSystem = true;
            viewport3DX.ShowFrameRate = true;
            //viewport3DX.Items.Add(new DirectionalLight3D() { Direction = new Vector3D(-1, -1, -1) });
            //viewport3DX.Items.Add(new AmbientLight3D() { Color = System.Windows.Media.Color.FromArgb(255, 50, 50, 50) });
            DirectionalLight3D directionalLight3D = new DirectionalLight3D
            {
                Color = Colors.White,
                Direction = new Vector3D(-2, -5, -2)
            };
            AmbientLight3D ambientLight3D = new AmbientLight3D
            {
                Color = Colors.White
            };

            viewport3DX.Items.Add(ambientLight3D);
            viewport3DX.Items.Add(directionalLight3D);

            // 网格
            LineMaterialGeometryModel3D lineMaterialGeometryModel3D = new LineMaterialGeometryModel3D()
            {
                Geometry = LineBuilder.GenerateGrid(),
                IsRendering = true,
                Material = new LineMaterial() { Color = Colors.Gray, TextureScale = 0.4 },
                Transform = new TranslateTransform3D(-1, -1, -1),

            };
            viewport3DX.Items.Add(lineMaterialGeometryModel3D);

            SceneNodeGroupModel3D sceneNodeGroup = new SceneNodeGroupModel3D();
            viewport3DX.Items.Add(sceneNodeGroup);





            //判断模型是否为空,准备重建模型
            if (teklaReader.TotalModelObjs != null)
            {

                //string totalJson =  JsonConvert.SerializeObject(teklaReader.TotalModelObjs);

                //List<ModelObject> ls = JsonConvert.DeserializeObject<List<ModelObject>>(totalJson);


                foreach (ModelObject obj in teklaReader.ModelBeamObjs)
                {
                    string sizeInfo = ((Part)obj).Profile.ProfileString;

                    if(obj is PolyBeam)
                    {
                        PolyBeam polyBeam = obj as PolyBeam;

                        GetSolidFace(polyBeam.GetSolid(), sizeInfo, "BEAM");
                    }
                    else if(obj is Beam)
                    {
                        Beam beam = obj as Beam;

                        GetSolidFace(beam.GetSolid(), sizeInfo, "BEAM");
                    }
                    else
                    {

                    }
                    
                }

                foreach(ModelObject obj in teklaReader.ModelContourPlateObjs)
                {
                    ContourPlate plate = obj as ContourPlate;

                    string sizeInfo = ((Part)obj).Profile.ProfileString;

                    GetSolidFace(plate.GetSolid(), sizeInfo, "PLATE");

                }

                foreach (ModelObject obj in teklaReader.ModelWeldObjs)
                {
                    PolygonWeld weld = obj as PolygonWeld;

                    dicWeldCoordinate.Add(weld.ReferenceText, weld.Polygon.Points);

                    GetSolidFace(weld.GetSolid(), string.Empty, "WELD", weld.ReferenceText);

                }

            }

 
            //为焊缝标注方向
            foreach(var dic in dicWeldCoordinate)
            {
                ArrayList ar = dic.Value;

                Tekla.Structures.Geometry3d.Point p1 = (Tekla.Structures.Geometry3d.Point)ar[0];

                Tekla.Structures.Geometry3d.Point p2 = (Tekla.Structures.Geometry3d.Point)ar[1];

                DrawLineWithArrow(
                    new Vector3 { X = (float)p1.X / ratio, Y = (float)p1.Y / ratio, Z = (float)p1.Z / ratio },
                    new Vector3 { X = (float)p2.X / ratio, Y = (float)p2.Y / ratio, Z = (float)p2.Z / ratio },
                    Colors.Red);
            }

           

        }


        public void DrawLineWithArrow(Vector3 point1, Vector3 point2, System.Windows.Media.Color color, int count = 3)
        {
            List<Vector3> pointList = DivideLineEqually(point1, point2, count);

            for (int i = 0; i < pointList.Count - 1; i++)
            {
                HelixToolkit.Wpf.SharpDX.MeshBuilder meshBuilder = new HelixToolkit.Wpf.SharpDX.MeshBuilder(false, false, false);
                meshBuilder.AddArrow(pointList[i], pointList[i + 1], 0.1, 3, 10);

                MeshGeometryModel3D mesh1 = RenderObject(
                     meshBuilder.ToMeshGeometry3D(),
                     color.ToColor4());

                ViewerDX.Items.Add(mesh1);
            }

    
        }

        public List<Vector3> DivideLineEqually(Vector3 point1, Vector3 point2,int count)
        {
            // 计算P1和P2之间的距离
            float distance = Vector3.Distance(point1, point2);

            // 计算每段的长度
            float segmentLength = distance / count;

            List<Vector3> dividedPoints = new List<Vector3>
            {

                // 添加起点P1
                point1
            };

            // 计算并添加三个中间点
            for (int i = 1; i < count; i++)
            {
                // 计算当前中间点的位置
                float t = i * segmentLength / distance;
                Vector3 interpolatedPoint = Vector3.Lerp(point1, point2, t);

                // 添加中间点
                dividedPoints.Add(interpolatedPoint);
            }

            // 添加终点P2
            dividedPoints.Add(point2);

            return dividedPoints;
        }




        private void GetSolidFace(
            Solid solid,
            string sizeInfo = "",
            string partType = "WELD",
            string referenceText = "")
        {
            //获取属性   WI300-15-20*300 h-s-t * b
            float[] sizeArray = ParseString(sizeInfo);

            FaceEnumerator plateFaceEnum = solid.GetFaceEnumerator();

            Dictionary<string, List<Tekla.Structures.Geometry3d.Point>> dicFacePointList = new Dictionary<string, List<Tekla.Structures.Geometry3d.Point>>();

            Dictionary<string, Tekla.Structures.Geometry3d.Vector> dicFaceNormalList = new Dictionary<string, Tekla.Structures.Geometry3d.Vector>();

            int faceID = 1;

            bool isCreateWeldTag = false;

            //获取face
            while (plateFaceEnum.MoveNext())
            {
                Face faceCurrent = plateFaceEnum.Current;

                List<Tekla.Structures.Geometry3d.Point> pointList = new List<Tekla.Structures.Geometry3d.Point>();

                if (plateFaceEnum.Current is Face MyFace)
                {
                    LoopEnumerator myLoopEnum = MyFace.GetLoopEnumerator();
                    while (myLoopEnum.MoveNext())
                    {
                        if (myLoopEnum.Current is Loop MyLoop)
                        {
                            VertexEnumerator myVertexEnum = MyLoop.GetVertexEnumerator();
                            while (myVertexEnum.MoveNext())
                            {
                                Tekla.Structures.Geometry3d.Point myVertex = myVertexEnum.Current;
                                if (myVertex != null && !pointList.Contains(myVertex))
                                {
                                    pointList.Add(myVertex);
                                }
                            }
                        }
                    }

                    List<Vector3> points = new List<Vector3>();

                    foreach (Tekla.Structures.Geometry3d.Point p in pointList)
                    {
                        Vector3 vector3 = new Vector3()
                        {
                            X = (float)p.X / ratio,
                            Y = (float)p.Y / ratio,
                            Z = (float)p.Z / ratio
                        };

                        points.Add(vector3);
                    }
                    FacePointList.Add(points);

                  

                    Random rnd = new Random();
                    byte[] b = new byte[3];
                    rnd.NextBytes(b);
                    System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(b[0], b[1], b[2]);

                    //Beam 两头特别处理
                    if (points.Count == 12)
                    {

                        //有12个三维的坐标P1(x,y,z)到P12(x,y,z),挑出两组坐标, 每组4个, 顺序排列,
                        //组成一个平面四边形, 长为b, 宽为t,  t和b为已知的一个实数.

                        float b1 = sizeArray[3] / ratio; // 已知的长
                        float t1 = sizeArray[2] / ratio;  // 已知的宽

                        for (int i = 0; i < points.Count; i++)
                        {
                            for (int j = i + 1; j < points.Count; j++)
                            {
                                for (int k = j + 1; k < points.Count; k++)
                                {
                                    for (int l = k + 1; l < points.Count; l++)
                                    {
                                        Vector3 p1 = points[i];
                                        Vector3 p2 = points[j];
                                        Vector3 p3 = points[k];
                                        Vector3 p4 = points[l];

                                        // 检查是否组成平面四边形
                                        if (IsQuadrilateral(p1, p2, p3, p4))
                                        {
                                            // 计算平面四边形的长和宽
                                            float length = CalculateDistance(p1, p2);
                                            float width = CalculateDistance(p2, p3);

                                            // 判断长度和宽度是否符合要求
                                            if ((Math.Abs(length - b1) < 0.0001 && Math.Abs(width - t1) < 0.0001) ||

                                                (Math.Abs(length - t1) < 0.0001 && Math.Abs(width - b1) < 0.0001))
                                            {
                                                List<Vector3> ls = new List<Vector3>
                                                {
                                                    p1,
                                                    p2,
                                                    p3,
                                                    p4
                                                };
                                                AddBeamPointManually(ls, color);
                                                // 找到符合条件的两组坐标
                                               // Console.WriteLine("找到一组坐标：");
                                               // Console.WriteLine($"({p1.X}, {p1.Y}, {p1.Z}), ({p2.X}, {p2.Y}, {p2.Z}), ({p3.X}, {p3.Y}, {p3.Z}), ({p4.X}, {p4.Y}, {p4.Z})");
                                            }

                                
                                        }
                                    }
                                }
                            }
                        }




                    }
                else
                {
                    HelixToolkit.Wpf.SharpDX.MeshBuilder meshBuilder = new HelixToolkit.Wpf.SharpDX.MeshBuilder(false, false, false);

                    meshBuilder.AddPolygon(points);

                    MeshGeometryModel3D mesh = null;

                    if (partType == "WELD")
                    {
                        mesh = RenderObject(
                                                meshBuilder.ToMeshGeometry3D(),
                                                Colors.LightBlue.ToColor4());
                        if (!isCreateWeldTag)
                        {
                            Vector3 centerPoint = CalculateCenterPoint(points[0], points[1], points[2], points[3]);

                            Vector3 normal = new Vector3();
                            normal.X = (float)faceCurrent.Normal.X;
                            normal.Y = (float)faceCurrent.Normal.Y;
                            normal.Z = (float)faceCurrent.Normal.Z;
                            Vector3 centerPointAbove = GeneratePositionAboveCenter(centerPoint, normal, 0.5);

                                CreateTextInfo(referenceText, centerPointAbove, Color.Red, Color.Black);




                                // 创建一个线条几何模型
                                LineBuilder lineBilder = new LineBuilder();

                            lineBilder.AddLine(centerPoint, centerPointAbove);



                                LineGeometry3D mesh1 = lineBilder.ToLineGeometry3D();
                                LineGeometryModel3D arrowMeshModel = new LineGeometryModel3D
                            {
                                Geometry = mesh1,
                                Color = Colors.White,
                                IsHitTestVisible = false
                            };

           
                            ViewerDX.Items.Add(arrowMeshModel);

                            isCreateWeldTag = true;
                        }

                    }
                    else
                    {
                        mesh = partType == "PLATE"
                            ? RenderObject(meshBuilder.ToMeshGeometry3D(), Colors.Purple.ToColor4())
                            : partType == "BEAM"? RenderObject(meshBuilder.ToMeshGeometry3D(), Colors.Green.ToColor4())
                                                        : RenderObject(meshBuilder.ToMeshGeometry3D(), color.ToColor4());

                        // CreateTextInfo("weld",points[0] ,SharpDX.Color.Red, SharpDX.Color.Black);
                    }

                    ViewerDX.Items.Add(mesh);
                }

    
     

                }
                dicFacePointList.Add($"{faceCurrent.OriginPartId}_{faceID}", pointList);

                dicFaceNormalList.Add($"{faceCurrent.OriginPartId}_{faceID}", faceCurrent.Normal);

                faceID++;


            }
        }


        private bool IsQuadrilateral(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            // 计算p1到p2、p2到p3、p3到p4和p4到p1之间的向量
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p3 - p2;
            Vector3 v3 = p4 - p3;
            Vector3 v4 = p1 - p4;

            // 检查是否是平行四边形，即向量v1与v3的长度和向量v2与v4的长度相等
            if (Math.Abs(v1.Length() - v3.Length()) < 0.0001 && Math.Abs(v2.Length() - v4.Length()) < 0.0001)
            {
                // 检查是否是直角矩形，即向量v1与v2的点积为0
                if (Math.Abs(Vector3.Dot(v1, v2)) < 0.0001)
                {
                    // 是矩形
                    return true;
                }
            }

            // 不是矩形
            return false;
        }

        private float CalculateDistance(Vector3 p1, Vector3 p2)
        {
            // 计算两点之间的距离
            return Vector3.Distance(p1, p2);
        }

        public Vector3 CalculateCenterPoint(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            double centerX = (point1.X + point2.X + point3.X + point4.X) / 4;
            double centerY = (point1.Y + point2.Y + point3.Y + point4.Y) / 4;
            double centerZ = (point1.Z + point2.Z + point3.Z + point4.Z) / 4;

            return new Vector3 { X = (float)centerX, Y = (float)centerY, Z = (float)centerZ };
        }

        // 根据法向量生成位于中心点上方指定距离的位置坐标
        public Vector3 GeneratePositionAboveCenter(Vector3 centerPoint, Vector3 normal, double distance)
        {
            // 根据法向量的长度将其归一化
            normal.Normalize();

            // 根据法向量和距离计算位于中心点上方的位置坐标
            double positionX = centerPoint.X + (normal.X * distance);
            double positionY = centerPoint.Y + (normal.Y * distance);
            double positionZ = centerPoint.Z + (normal.Z * distance);

            return new Vector3 { X = (float)positionX, Y = (float)positionY, Z = (float)positionZ };
        }


        private List<List<Vector3>> GetFaces(List<Vector3> vertices, float h, float s, float t, float b)
        {
            List<List<Vector3>> faces = new List<List<Vector3>>();

            List<Vector3> listGroup = new List<Vector3>();

            List<Vector3> listGroup2 = new List<Vector3>();

            List<Vector3> list = new List<Vector3>();

            list.AddRange(vertices);

            Dictionary<Tuple<Vector3, Vector3>, float> distances = new Dictionary<Tuple<Vector3, Vector3>, float>();

            for (int i = 0; i < vertices.Count(); i++)
            {
                for (int j = i + 1; j < vertices.Count(); j++)
                {
                    float distance = (float)Math.Round(Vector3.Distance(vertices[j], vertices[i]), 3);

                    float[] arr = new float[] { b, t, h - 2 * t, s};
                    if (arr.Contains(distance))
                    {
                        if(!distances.TryGetValue(Tuple.Create(vertices[i], vertices[j]),out float v))
                        {
                            distances.Add(Tuple.Create(vertices[j], vertices[i]), distance);
                        }
                    }
                }
            }

            foreach(KeyValuePair<Tuple<Vector3, Vector3>, float> dic in distances)
            {
                Tuple<Vector3, Vector3> p1 = dic.Key;
                foreach(KeyValuePair<Tuple<Vector3, Vector3>, float> dic1 in distances)
                {
                    Tuple<Vector3, Vector3> p2 = dic1.Key;

                    Vector3 p11 = p1.Item1;
                    Vector3 p12 = p1.Item2;
                    Vector3 p21 = p2.Item1;
                    Vector3 p22 = p2.Item2;

                    float d1 = (float)Math.Round(Vector3.Distance(p11, p21));
                    float d2 = (float)Math.Round(Vector3.Distance(p11, p22));

                    float d3 = (float)Math.Round(Vector3.Distance(p12, p21));
                    float d4 = (float)Math.Round(Vector3.Distance(p12, p22));

                

                    float[] arr1 = new float[] { b, t};

                    float[] arr2 = new float[] { h - 2 * t, s };


                }
            }





            List<Tuple<Vector3, Vector3>> candidates = distances.Where(kvp =>
            kvp.Value.Equals(t) ||
            kvp.Value.Equals(b)).Select(kvp => kvp.Key).ToList();


            List<Tuple<Vector3, Vector3>> candidates2 = distances.Where(
                kvp =>
        kvp.Value.Equals(s) ||
                    kvp.Value.Equals(h - 2 * t))
                .Select(kvp => kvp.Key)
                .ToList();

            List<List<Vector3>> validSets = new List<List<Vector3>>();

            while (candidates.Count > 0)
            {
                List<Vector> groups = new List<Vector>();
                List<Tuple<Vector3, Vector3>> set = new List<Tuple<Vector3, Vector3>>
                {
                    candidates.First()
                };
                _ = candidates.Remove(set[set.Count - 1]);

                while (candidates.Count > 0)
                {
                    bool foundMatch = false;

                    foreach (Tuple<Vector3, Vector3> candidate in candidates)
                    {
                        Vector3 sitem1 = set.Last().Item1;
                        Vector3 sitem2 = set.Last().Item2;
                        Vector3 citem1 = candidate.Item1;
                        Vector3 citem2 = candidate.Item2;

                        float d1 = (float)Math.Round(Vector3.Distance(sitem1, citem1));
                        float d2 = (float)Math.Round(Vector3.Distance(sitem1, citem2));

                        float d3 = (float)Math.Round(Vector3.Distance(sitem2, citem1));
                        float d4 = (float)Math.Round(Vector3.Distance(sitem2, citem2));

                        float[] arr = new float[] { b, t, 0 };

                        float[] arr1 = new float[] { h - 2 * t, s, 0 };
                         
                        if (arr.Contains(d1) && arr.Contains(d2) && arr.Contains(d3) && arr.Contains(d4))
                        {
                            if (!listGroup.Contains(sitem1))
                            {
                                listGroup.Add(sitem1);
                            }
                            if (!listGroup.Contains(sitem2))
                            {
                                listGroup.Add(sitem2);
                            }
                            if (!listGroup.Contains(citem1))
                            {
                                listGroup.Add(citem1);
                            }
                            if (!listGroup.Contains(citem2))
                            {
                                listGroup.Add(citem2);
                            }

                            set.Add(candidate);
                            _ = candidates.Remove(candidate);
                            foundMatch = true;
                            break;
                        }

                        //if (arr1.Contains(d1) && arr1.Contains(d2) && arr1.Contains(d3) && arr1.Contains(d4))
                        //{
                        //    if (!listGroup2.Contains(sitem1))
                        //    {
                        //        listGroup2.Add(sitem1);
                        //    }
                        //    if (!listGroup2.Contains(p12))
                        //    {
                        //        listGroup2.Add(p12);
                        //    }
                        //    if (!listGroup2.Contains(p21))
                        //    {
                        //        listGroup2.Add(p21);
                        //    }
                        //    if (!listGroup2.Contains(p22))
                        //    {
                        //        listGroup2.Add(p22);
                        //    }
                        //    set.Add(candidate);
                        //    _ = candidates.Remove(candidate);
                        //    foundMatch = true;
                        //    break;
                        //}
                    }

                    if (!foundMatch)
                    {
                        break;
                    }

                    if (listGroup.Count == 4)
                    {
                        bool exists = validSets.Any(sublist => sublist.SequenceEqual(listGroup));

                        validSets.Add(listGroup);
                   
                        listGroup = new List<Vector3>();
                        //validSets.Add(set);
                    }
                }
            }

            listGroup = new List<Vector3>();

            foreach (List<Vector3> vs in validSets)
            {
                foreach(Vector3 p in vs)
                {
                    listGroup.Add(p);
                }
            }

            List<Vector3> difference = vertices.Except(listGroup).ToList();

            return validSets;
        }


        private void AddBeamPointManually(List<Vector3> points, System.Windows.Media.Color color)
        {
            HelixToolkit.Wpf.SharpDX.MeshBuilder meshBuilder = new HelixToolkit.Wpf.SharpDX.MeshBuilder(false, false);
            meshBuilder.AddPolygon(points);
            MeshGeometryModel3D mesh1 = RenderObject(
                 meshBuilder.ToMeshGeometry3D(),
                 color.ToColor4());
            ViewerDX.Items.Add(mesh1);
        }



        private void CreateCameraPosition()
        {
            List<WeldStructure> weldList = teklaReader.WeldList;

            foreach(WeldStructure weld in weldList)
            {
                //var p = teklaReader.CreateCameraPosition(
                //    new Tekla.Structures.Geometry3d.Point(weld.StartPoint.X / ratio, weld.StartPoint.Y / ratio, weld.StartPoint.Z / ratio),
                //     new Tekla.Structures.Geometry3d.Point(weld.EndPoint.X / ratio, weld.EndPoint.Y / ratio, weld.EndPoint.Z / ratio),
                //    45,
                //    100,
                //    weld.NormalList[0],
                //    weld.NormalList[1]);

                Tekla.Structures.Geometry3d.Point p = weld.CameraPoint;

                HelixToolkit.Wpf.SharpDX.MeshBuilder meshBuilder = new HelixToolkit.Wpf.SharpDX.MeshBuilder();

                // AddBox(Vector3 center, double xlength, double ylength, double zlength)
                meshBuilder.AddBox(new Vector3() { X = (float)p.X / ratio, Y = (float)p.Y / ratio, Z = (float)p.Z / ratio }, 0.1, 0.1, 0.1);

                Random rnd = new Random();
                byte[] b = new byte[3];
                rnd.NextBytes(b);
                System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(b[0], b[1], b[2]);

                //MeshGeometryModel3D mesh = RenderObject(
                //        meshBuilder.ToMeshGeometry3D(),
                //        Colors.Red.ToColor4());
                MeshGeometryModel3D mesh = RenderObject(
                      meshBuilder.ToMeshGeometry3D(),
                      color.ToColor4());

                ViewerDX.Items.Add(mesh);

            }
        }



        private void CreateTextInfo(string info, Vector3 position, SharpDX.Color fontColor, SharpDX.Color bkColor)
        {
            BillboardSingleText3D axisTxt = new BillboardSingleText3D()
            {
                TextInfo = new TextInfo(info, position),
                FontColor = fontColor,
                FontSize = 20,
                BackgroundColor = bkColor,
                FontStyle = FontStyles.Italic,
                Padding = new Thickness(2)
            };

            BillboardTextModel3D axisTxtModel = new BillboardTextModel3D
            {
                Geometry = axisTxt
            };
            ViewerDX.Items.Add(axisTxtModel);


        }


        /// <summary>
        /// Parses the string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Single[].</returns>
        private float[] ParseString(string input)
        {
            string pattern = @"\d+"; // 匹配连续的数字

            MatchCollection matches = Regex.Matches(input, pattern);

            float[] sizeData = new float[matches.Count];
            int i = 0;
            foreach (Match match in matches)
            {
                sizeData[i] = Convert.ToSingle(match.Value);
                i++;
            }


            return sizeData;
        }


        private void DrawBeam(int listIndex)
        {
            if(listIndex > FacePointList.Count - 1)
                return;
           //foreach(List<Vector3> p in FacePointList)
            //{
               List<Vector3> p = FacePointList[listIndex];

            HelixToolkit.Wpf.SharpDX.MeshBuilder meshBuilder = new HelixToolkit.Wpf.SharpDX.MeshBuilder();

            //if (p.Count > 4 && p.Count % 4 == 0)
            //{
            //    List<Vector3> p1 = new List<Vector3>();

            //    int index = 1;

            //    for(int i = 0; i < p.Count; i++)
            //    {
            //        p1.Add(p[i]);

            //        if (index % 4 == 0)
            //        {
            //            meshBuilder.AddPolygon(p1);
            //            p1 = new List<Vector3>();

            //            index = 0;
            //        }


            //        index++;
            //    }

            //}
            //else
            //{
            //    meshBuilder.AddPolygon(p);
            //}

            meshBuilder.AddPolygon(p);



            Random rnd = new Random();
            byte[] b = new byte[3];
            rnd.NextBytes(b);
            System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(b[0], b[1], b[2]);

            //MeshGeometryModel3D mesh = RenderObject(
            //        meshBuilder.ToMeshGeometry3D(),
            //        Colors.Red.ToColor4());
            MeshGeometryModel3D mesh = RenderObject(
                  meshBuilder.ToMeshGeometry3D(),
                  color.ToColor4());

            ViewerDX.Items.Add(mesh);
           // }
             
        }


        /// <summary>
        /// 画工字钢
        /// </summary>
        /// <param name="viewport3DX">The viewport3 dx.</param>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="h">The h. 高度</param>
        /// <param name="s">The s. 中间梁厚度</param>
        /// <param name="t1">The t1.上梁厚度</param>
        /// <param name="t2">The t2.下梁厚度</param>
        /// <param name="b1">The b1.上梁宽度</param>
        /// <param name="b2">The b2. 下梁宽度</param>
        /// <param name="rate">The rate. 缩小倍数</param>
        private void DrawGong(Viewport3DX viewport3DX, Vector3 startPoint, Vector3 endPoint, float h, float s, float t1, float t2, float b1, float b2, int rate = 100)
        {
            ratio = rate;
            HelixToolkit.Wpf.SharpDX.MeshBuilder meshBuilder1 = new HelixToolkit.Wpf.SharpDX.MeshBuilder();
            HelixToolkit.Wpf.SharpDX.MeshBuilder meshBuilder2 = new HelixToolkit.Wpf.SharpDX.MeshBuilder();
            HelixToolkit.Wpf.SharpDX.MeshBuilder meshBuilder3 = new HelixToolkit.Wpf.SharpDX.MeshBuilder();

            //长度

            float len = (float)Math.Sqrt(
                Math.Pow(startPoint.X - endPoint.X, 2) +
                    Math.Pow(startPoint.Y - endPoint.Y, 2) +
                    Math.Pow(startPoint.Z - endPoint.Z, 2));


            Vector3 newStartPoint = new Vector3(startPoint.X / rate, startPoint.Y / rate, startPoint.Z / rate);
            _ = new Vector3(endPoint.X / rate, endPoint.Y / rate, endPoint.Z / rate);

            // AddBox(Vector3 center, double xlength, double ylength, double zlength)

            //判断方向
            _ = endPoint.X - startPoint.X;
            _ = endPoint.Y - startPoint.Y;
            _ = endPoint.Z - endPoint.Z;


            // 在立方体表面添加文字
            TextVisual3D textVisual = new TextVisual3D();
            textVisual.Text = "UP";
            textVisual.Position = new Point3D(startPoint.X, startPoint.Y, startPoint.Z); // 文字的位置
            textVisual.FontSize = 10; // 文字的大小
            textVisual.Foreground = Brushes.White; // 文字的颜色




            // 上下截面    缩小倍率  WI300-15-20*300
            meshBuilder1.AddBox(newStartPoint, h / rate, t1 / rate, len / rate);
            //上下间隔
            startPoint.Y = h / rate - t1 / rate;
            meshBuilder2.AddBox(newStartPoint, h / rate, t2 / rate, len / rate);
            // 中间梁
            meshBuilder3.AddBox(
                new Vector3(newStartPoint.X, (h / rate - (t1 / rate)) / 2, newStartPoint.Z),
                s / rate,
                (h / rate) - (t1 / rate) - (t2 / rate),
                len / rate);

            MeshGeometryModel3D mesh1 = RenderObject(meshBuilder1.ToMeshGeometry3D(), Colors.Yellow.ToColor4());
            viewport3DX.Items.Add(mesh1);


            MeshGeometryModel3D mesh2 = RenderObject(meshBuilder2.ToMeshGeometry3D(), Colors.Red.ToColor4());
            viewport3DX.Items.Add(mesh2);

            MeshGeometryModel3D mesh3 = RenderObject(meshBuilder3.ToMeshGeometry3D(), Colors.Blue.ToColor4());
            viewport3DX.Items.Add(mesh3);
        }



        private MeshGeometryModel3D RenderObject(HelixToolkit.Wpf.SharpDX.MeshGeometry3D meshGeometry3D, Color4 materialColor)
        {

            HelixToolkit.Wpf.SharpDX.MeshGeometry3D mesh = meshGeometry3D;
            MeshGeometryModel3D meshGeometryModel3D = new MeshGeometryModel3D
            {
                Geometry = mesh,

                //Transform = new TranslateTransform3D(0, 0, 0),
                //Material = new PhongMaterial
                //{
                //    DiffuseColor = new Color4(1, 0, 0, 1), // 设置为红色
                //    AmbientColor = new Color4(1, 0, 0, 1) // 设置为红色
                //}
            };

            PhongMaterial material = new PhongMaterial()
            {
                DiffuseColor = materialColor,
                AmbientColor = materialColor
            };


            // meshGeometryModel3D.Material = materialColor;

            meshGeometryModel3D.Material = material;

            meshGeometryModel3D.IsTransparent = false;

            return meshGeometryModel3D;
        }




  

        private void BTNOpenFile_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个OpenFileDialog对象
            OpenFileDialog openFileDialog = new OpenFileDialog
            {

                // 设置对话框的标题和过滤器
                Title = "选择文件",
                Filter = "IFC Files|*.ifc;*.ifczip;*.ifcxml|Xbim Files|*.xbim"
            };

            // 打开对话框并获取用户选择的文件路径
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                // 在这里处理选中的文件路径
                LoadXbimFile(filePath);

            }
        }

        private void dlg_FileSaveAs(object sender, CancelEventArgs e)
        {
            SaveFileDialog dlg = sender as SaveFileDialog;
            if (dlg == null)
            {
                return;
            }

            FileInfo fInfo = new FileInfo(dlg.FileName);
            try
            {
                if (fInfo.Exists)
                {
                    // the user has been asked to confirm deletion previously
                    fInfo.Delete();
                }
                //if (Model != null)
                //{
                //    Model.SaveAs(dlg.FileName);
                //    SetOpenedModelFileName(dlg.FileName);
                //    var s = Path.GetExtension(dlg.FileName);
                //    if (string.IsNullOrWhiteSpace(s))
                //    {
                //        return;
                //    }

                //    var extension = s.ToLowerInvariant();
                //    if (extension != "xbim" || string.IsNullOrWhiteSpace(_temporaryXbimFileName))
                //    {
                //        return;
                //    }

                //    File.Delete(_temporaryXbimFileName);
                //    _temporaryXbimFileName = null;
                //}
                //else
                //{
                //    throw new Exception("Invalid Model Server");
                //}
            }
            catch (Exception except)
            {
                _ = MessageBox.Show(except.Message, "Error Saving as", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void LoadXbimFile(string fileName)
        {
            // GetLocation(fileName);


            XbimEditorCredentials editor = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "xbim developer",
                ApplicationFullName = "xbim toolkit",
                ApplicationIdentifier = "xbim",
                ApplicationVersion = "4.0",
                EditorsFamilyName = "Santini Aichel",
                EditorsGivenName = "Johann Blasius",
                EditorsOrganisationName = "Independent Architecture"
            };

            IfcStore model = IfcStore.Open(fileName, editor);


            // 获取所有需要解析的元素集合，这里以IfcProduct为例
            IEnumerable<IfcProduct> products = model.Instances.OfType<IfcProduct>();

            if (model.GeometryStore.IsEmpty)
            {

                // Create the geometry using the XBIM GeometryEngine
                try
                {
                    context = new Xbim3DModelContext(model);

                    _ = context.CreateContext();


                    // 获取所有连接到Weld的IFC元素

                    IEnumerable<IGrouping<Type, IIfcRelConnects>> connectedElements = model.Instances.OfType<IIfcRelConnects>().GroupBy(e => e.GetType());


                    foreach (IGrouping<Type, IIfcRelConnects> element in connectedElements)
                    {
                        if (element is IIfcColumn)
                        {
                            IIfcColumn column = element as IIfcColumn;

                            IIfcObjectPlacement c = column.ObjectPlacement;

                            // 处理尺寸信息...
                        }
                        else if (element is IIfcBeam)
                        {
                            IIfcBeam beam = element as IIfcBeam;
                            //double width = beam.OverallWidth;
                            //double height = beam.OverallHeight;
                            // 处理尺寸信息...
                        }
                        else if (element is IIfcSlab)
                        {
                            IIfcSlab slab = element as IIfcSlab;
                            //double length = slab.XDim;
                            //double width = slab.YDim;
                            //double thickness = slab.Thickness;
                            // 处理尺寸信息...
                        }
                        else if (element is IIfcWall)
                        {
                            IIfcWall wall = element as IIfcWall;
                            //double width = wall.OverallWidth;
                            //double height = wall.OverallHeight;
                            // 处理尺寸信息...
                        }
                        else
                        {
                            // 处理其他类型的元素...
                        }
                    }


                    //// 创建正方体的几何图形
                    //var meshBuilder = new HelixToolkit.Wpf.MeshBuilder();
                    //meshBuilder.AddBox(new Point3D(0, 0, 0), 1, 1, 1);

                    //// 将正方体移动到位置 (10, 0, 0)
                    //var transformMatrix = new XbimMatrix3D(1, 0, 0, 10, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
                    //var transformedMeshGeometry = meshBuilder.ToMesh().Transform(transformMatrix);

                    //// 创建正方体的材质
                    //var material = MaterialHelper.CreateMaterial(Colors.Red);

                    //// 创建正方体的几何图形对象并添加到 DrawingControl3D 中
                    //var cubeModel = new System.Windows.Media.Media3D.GeometryModel3D(new MeshGeometry3D(transformedMeshGeometry.Positions, transformedMeshGeometry.TriangleIndices), material);
                    //var cubeGroup = new ModelVisual3D { Content = cubeModel };
                    //DrawingControl.Viewport.Children.Add(cubeGroup);


                    //IEnumerable<int> matchingElements = from i in model.Instances.OfType<IIfcRelDefinesByProperties>()
                    //                                        // Find all properties in Pset called 'Dimensions' with 'Area' > 20.5
                    //                                    where i.RelatingPropertyDefinition is IIfcPropertySet
                    //                                    let pset = i.RelatingPropertyDefinition as IIfcPropertySet
                    //                                    where pset.Name == "Dimensions"
                    //                                    let property = pset.HasProperties.OfType<IIfcPropertySingleValue>().FirstOrDefault(v => v.Name == "Area")
                    //                                    where property?.NominalValue is IExpressRealType
                    //                                    where ((IExpressRealType)property.NominalValue).Value > 20.5d
                    //                                    // Now get the elements
                    //                                    from element in i.RelatedObjects
                    //                                    select element.EntityLabel;





                    Viewer3D.Model = model;
                    //SpatialControl.Model = model;
                    //PropertiesControl.Model = model;

                    // AddCubeToViewport(DrawingControl, 0, 0, 0, 0.5,0.1,1.0, new SolidColorBrush(Colors.White)); // 在3D坐标(0,0,0)位置添加一个尺寸为0.5的立方体

                    //AddCubeToViewport(DrawingControl, 0, 0, 0, 0, 0.5, 1.0, new SolidColorBrush(Colors.Red)); // 在3D坐标(0,0,0)位置添加一个尺寸为0.5的立方体

                    //AddCubeToViewport(DrawingControl, 1, 0, 0, 0, 0.5, 1.0, new SolidColorBrush(Colors.Red));


                    //AddCubeToViewport(Viewer3D, 0, 0, 0, 0.05); // 在3D坐标(0,0,0)位置添加一个尺寸为0.5的立方体

                    ////AddTextToDrawingControl(DrawingControl, "1", 0, 0, 0);

                    //AddCubeToViewport(Viewer3D, 1, 0, 0, 0.05);

                    //AddCubeToViewport(Viewer3D, -1, 0, 0, 0.05);

                    //AddCubeToViewport(Viewer3D, 0, 1, 0, 0.05);

                    //AddCubeToViewport(Viewer3D, 0, -1, 0, 0.05);

                    //AddCubeToViewport(Viewer3D, 0, 0, 1, 0.05);

                    //AddCubeToViewport(Viewer3D, 0, 0, -1, 0.05);


                    // AddCameraToViewport(DrawingControl, 1, 0.5, new SolidColorBrush(Colors.Red));

                    //SpatialControl.ItemsSource = xbimTreeview.svList;
                    //SpatialControl.Model = (IfcStore)model.Instances;

                    // TODO: SaveAs(xbimFile); // so we don't re-process every time
                }
                catch (Exception geomEx)
                {
                    throw geomEx;
                }
            }




        }






        // 刷新树状列表数据的方法
        void RefreshTreeViewData(IfcProduct product)
        {
            //// 根据需要刷新树状列表的数据
            //// 这里可以根据元素的属性进行解析并更新树状节点的子节点
            //// 例如，获取product的属性并将其添加为子节点
            //TreeViewItem item = new TreeViewItem();
            //item.Header = "属性名称";
            //item.Items.Add("属性值");

            //// 清空树状节点的子节点并添加新的子节点
            //item.Items.Clear();
            //item.Items.Add("新的属性值");

            //// 更新树状列表数据
            //// 这里需要根据实际情况找到对应的树状节点并进行更新
            //// 例如，可以通过遍历TreeView的Items属性找到对应的节点
            //foreach (TreeViewItem node in TVSelected.Items)
            //{
            //    if (node.Header.ToString() == product.Name)
            //    {
            //        node.Items.Clear();
            //        node.Items.Add(item);
            //        break;
            //    }
            //}
        }



        private System.Windows.Media.Media3D.MeshGeometry3D CreateCubeMesh(double x, double y, double z, double size, double scale)
        {
            // 调整顶点坐标
            Point3DCollection positions = new Point3DCollection()
    {
        new Point3D(x * scale, y * scale, z * scale),
        new Point3D((x + size) * scale, y * scale, z * scale),
        new Point3D(x * scale, (y + size) * scale, z * scale),
        new Point3D((x + size) * scale, (y + size) * scale, z * scale),
        new Point3D(x * scale, y * scale, (z + size) * scale),
        new Point3D((x + size) * scale, y * scale, (z + size) * scale),
        new Point3D(x * scale, (y + size) * scale, (z + size) * scale),
        new Point3D((x + size) * scale, (y + size) * scale, (z + size) * scale)
    };


            Int32Collection triangleIndices = new Int32Collection(new int[]
            {
        0,1,2,2,1,3, // front face
        5,4,7,7,4,6, // back face
        4,0,6,6,0,2, // left face
        1,5,3,3,5,7, // right face
        4,5,0,0,5,1, // top face
        2,3,6,6,3,7  // bottom face
            });

            System.Windows.Media.Media3D.MeshGeometry3D mesh = new System.Windows.Media.Media3D.MeshGeometry3D();
            mesh.Positions = positions;
            mesh.TriangleIndices = triangleIndices;

            return mesh;
        }

        private void AddCubeToViewport(DrawingControl3D drawingControl3D, double x, double y, double z, double size, double scale, double opacity, SolidColorBrush brush)
        {
            System.Windows.Media.Media3D.MeshGeometry3D cubeMesh = CreateCubeMesh(x, y, z, size, scale);
            System.Windows.Media.Media3D.DiffuseMaterial material = new System.Windows.Media.Media3D.DiffuseMaterial(brush);

            // 设置透明度
            material.Brush.Opacity = opacity;

            System.Windows.Media.Media3D.GeometryModel3D geometryModel3D = new System.Windows.Media.Media3D.GeometryModel3D(cubeMesh, material);

            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = geometryModel3D;

            drawingControl3D.Viewport.Children.Add(modelVisual3D);
        }


        private void AddCubeToViewport(DrawingControl3D drawingControl3D, double x, double y, double z, double size, double scale = 1)
        {
            System.Windows.Media.Media3D.MeshGeometry3D cubeMesh = CreateCubeMesh(x, y, z, size, scale);
            System.Windows.Media.Media3D.DiffuseMaterial material = new System.Windows.Media.Media3D.DiffuseMaterial(new SolidColorBrush(Colors.Red));

            System.Windows.Media.Media3D.GeometryModel3D geometryModel3D = new System.Windows.Media.Media3D.GeometryModel3D(cubeMesh, material);

            ModelVisual3D modelVisual3D = new ModelVisual3D
            {
                Content = geometryModel3D
            };

            drawingControl3D.Viewport.ShowCoordinateSystem = true;

            drawingControl3D.Viewport.CoordinateSystemLabelForeground = Brushes.White;

            drawingControl3D.Viewport.Children.Add(modelVisual3D);
        }



        private System.Windows.Media.Media3D.MeshGeometry3D CreateCameraMesh(double size, double scale)
        {
            // 定义摄像头的顶点坐标
            Point3DCollection positions = new Point3DCollection()
    {
        new Point3D(-1, -0.5, 0),
        new Point3D(1, -0.5, 0),
        new Point3D(1, 0.5, 0),
        new Point3D(-1, 0.5, 0),
        new Point3D(0, 0, 1)
    };

            // 定义摄像头的三角形面片
            Int32Collection indices = new Int32Collection()
    {
        0, 1, 4,
        1, 2, 4,
        2, 3, 4,
        3, 0, 4,
        0, 1, 2, 0, 2, 3
    };

            // 缩放和平移几何形状
            for (int i = 0; i < positions.Count; i++)
            {
                positions[i] = new Point3D(positions[i].X * scale * size, positions[i].Y * scale * size, positions[i].Z * scale * size);
            }

            // 创建MeshGeometry3D对象
            System.Windows.Media.Media3D.MeshGeometry3D mesh = new System.Windows.Media.Media3D.MeshGeometry3D
            {
                Positions = positions,
                TriangleIndices = indices
            };

            return mesh;
        }

        private void AddCameraToViewport(DrawingControl3D drawingControl3D, double size, double scale, SolidColorBrush brush)
        {
            System.Windows.Media.Media3D.MeshGeometry3D cubeMesh = CreateCameraMesh(size, scale);
            System.Windows.Media.Media3D.DiffuseMaterial material = new System.Windows.Media.Media3D.DiffuseMaterial(brush);

            // 设置透明度
            material.Brush.Opacity = 1;

            System.Windows.Media.Media3D.GeometryModel3D geometryModel3D = new System.Windows.Media.Media3D.GeometryModel3D(cubeMesh, material);

            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = geometryModel3D;

            drawingControl3D.Viewport.Children.Add(modelVisual3D);
        }




        private void LoadModelTree(string ifcName, IEnumerable<IIfcElement> elements)
        {
            _ = new TreeViewItem
            {
                Header = ifcName
            };

            //    //  
            //    TreeViewItem Child1Item = new TreeViewItem();
            ////Child1Item.Header = "Child One";
            //ParentItem.Items.Add(Child1Item);
            ////  
            //TreeViewItem Child2Item = new TreeViewItem();
            //Child2Item.Header = "Child Two";
            //ParentItem.Items.Add(Child2Item);
            //TreeViewItem SubChild1Item = new TreeViewItem();
            //SubChild1Item.Header = "Sub Child One";
            //Child2Item.Items.Add(SubChild1Item);
            //TreeViewItem SubChild2Item = new TreeViewItem();
            //SubChild2Item.Header = "Sub Child Two";
            //Child2Item.Items.Add(SubChild2Item);
            //TreeViewItem SubChild3Item = new TreeViewItem();
            //SubChild3Item.Header = "Sub Child Three";
            //Child2Item.Items.Add(SubChild3Item);
            ////  
            //TreeViewItem Child3Item = new TreeViewItem();
            //Child3Item.Header = "Child Three";
            //ParentItem.Items.Add(Child3Item);
        }

        private void GetLocation(string fileName)
        {
            //XmlDocument xml = new XmlDocument();
            //XmlNode statement = xml.CreateXmlDeclaration("1.0", "utf-8", "");
            //xml.AppendChild(statement);
            ////创建根节点  
            //XmlElement root = xml.CreateElement("Root");
            //xml.AppendChild(root);
            //XmlNode RootNode = xml.SelectSingleNode("Root");

            //var model = IfcStore.Open(fileName);
            //var context = new Xbim3DModelContext(model);
            //context.CreateContext();
            //var instances = context.ShapeInstances();
            //var products = model.Instances.OfType<IIfcProduct>();
            //foreach (var product in products)
            //{
            //    if (product.Representation != null)
            //    {
            //        XmlNode ProductNode = xml.CreateElement(product.ExpressType + product.EntityLabel.ToString());
            //        RootNode.AppendChild(ProductNode);
            //        var instance = instances.FirstOrDefault(r => r.IfcProductLabel == product.EntityLabel);
            //        //var coordinate = GetGlobalLocation(instance);

            //        //XmlElement coordinateX = xml.CreateElement("X");
            //        //XmlElement coordinateY = xml.CreateElement("Y");
            //        //XmlElement coordinateZ = xml.CreateElement("Z");
            //        //coordinateX.InnerText = coordinate.X.ToString("f2");
            //        //coordinateY.InnerText = coordinate.Y.ToString("f2");
            //        //coordinateZ.InnerText = coordinate.Z.ToString("f2");
            //        //ProductNode.AppendChild(coordinateX);
            //        //ProductNode.AppendChild(coordinateY);
            //        //ProductNode.AppendChild(coordinateZ);
            //    }
            //}
            //xml.Save("Location.xml");
            //if (File.Exists("Location.xml"))
            //{
            //    Console.WriteLine("成功");
            //}




            IfcStore model = IfcStore.Open(fileName);

            List<XbimPlacementNode> rootNodes = new List<XbimPlacementNode>();
            List<IfcLocalPlacement> localPlacements = model.Instances.OfType<IfcLocalPlacement>(true).ToList();
            Dictionary<int, XbimPlacementNode> Nodes = new Dictionary<int, XbimPlacementNode>();
            foreach (IfcLocalPlacement placement in localPlacements)
            {
                Nodes.Add(placement.EntityLabel, new XbimPlacementNode(placement));
            }

            foreach (IfcLocalPlacement localPlacement in localPlacements)
            {
                if (localPlacement.PlacementRelTo != null) //resolve parent
                {
                    XbimPlacementNode xbimPlacement = Nodes[localPlacement.EntityLabel];
                    XbimPlacementNode xbimPlacementParent = Nodes[localPlacement.PlacementRelTo.EntityLabel];
                    xbimPlacement.Parent = xbimPlacementParent;
                    xbimPlacementParent.Children.Add(xbimPlacement);
                }
                else
                {
                    rootNodes.Add(Nodes[localPlacement.EntityLabel]);
                }
            }



        }






        private void AutoScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sc = sender as ScrollViewer;

            int rows = 10000;

            if (rows < 100)
            {
                rows = 100;
            }

            if (TBLOG.Text.Length > rows)
            {
                TBLOG.Text = string.Empty;
            }


            if (TBLOG.Text.Length > rows)
            {
                TBLOG.Text = string.Empty;
            }

            if (e.ExtentHeightChange > 0)
            {
                sc.ScrollToEnd();
            }
        }


        /// <summary>
        /// Prints the PLC information.
        /// </summary>
        /// <param name="info">The information.</param>
        public static void PrintInfo(object info)
        {
            try
            {
                string strFlag = info.ToString().Split(':')[0];

                string strinfo = info.ToString().Split(':')[1];

                strinfo = strinfo.Replace("OP70", "OP80");

                switch (strFlag)
                {
                    case "INFO":
                        ((MainWindow)Application.Current.MainWindow).TBLOG.Inlines
                            .Add(
                                new Run($"[{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]  {strinfo}")
                                {
                                    Foreground = Brushes.LightGreen
                                });
                        break;
                    case "WARNNING":
                        ((MainWindow)Application.Current.MainWindow).TBLOG.Inlines
                            .Add(
                                new Run($"[{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]  {strinfo}")
                                {
                                    Foreground = Brushes.LightYellow
                                });
                        break;
                    case "ERROR":
                        ((MainWindow)Application.Current.MainWindow).TBLOG.Inlines
                            .Add(
                                new Run($"[{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]  {strinfo}")
                                {
                                    Foreground = Brushes.MediumVioletRed
                                });
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ((MainWindow)Application.Current.MainWindow).TBLOG.Inlines
                    .Add(
                        new Run($"[{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {info}")
                        {
                            Foreground = Brushes.White
                        });
                _ = SimpleLog.Error($"PrintInfo 发生异常,{ex}");
            }
        }


        public static void WriteLogToFile(string loginfo, int loglevel = 999)
        {
            if (loglevel == 999)
            {
                _ = SimpleLog.Info(loginfo);
            }
            else
            {
                _ = SimpleLog.Error(loginfo);
            }
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void OnLayoutRootPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            LayoutContent activeContent = ((LayoutRoot)sender).ActiveContent;
            if (e.PropertyName == "ActiveContent")
            {
                Debug.WriteLine(string.Format("ActiveContent-> {0}", activeContent));
            }
        }



        //private bool _camChanged;
        private void SpatialControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // _camChanged = false;
            //    DrawingControl.Viewport.Camera.Changed += Camera_Changed;
            //    DrawingControl.ZoomSelected();
            //    DrawingControl.Viewport.Camera.Changed -= Camera_Changed;
            //    if (!_camChanged)
            //        DrawingControl.ClipBaseSelected(0.15);
        }

        void Camera_Changed(object sender, EventArgs e)
        {
            //  _camChanged = true;
        }

        private void CommandBoxEval(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //CommandBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowErrors(object sender, MouseButtonEventArgs e)
        {

        }


        private TreeViewData BuildTreeViewData(IIfcProduct product)
        {
            TreeViewData treeData = new TreeViewData();
            treeData.Name = $"{product.ExpressType.ExpressName} #{product.EntityLabel}";
            treeData.IfcEntityLabel = product.EntityLabel.ToString();
            treeData.IfcType = product.ExpressType.ExpressName;
            // 填充其他IFC元素属性

            foreach (IIfcObjectDefinition child in product.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
            {
                IIfcProduct childProduct = child as IIfcProduct;
                if (childProduct != null)
                {
                    TreeViewData childNode = BuildTreeViewData(childProduct);
                    treeData.Children.Add(childNode);
                }
            }

            return treeData;
        }


        private TreeViewData BuildTreeViewData(List<WeldStructure> ls)
        {
            TreeViewData treeData = new TreeViewData();
            treeData.Name = "焊缝数据";
 
            // 填充其他IFC元素属性

            //foreach (var ws in ls)
            //{
                
            //    if (ws != null)
            //    {
            //        TreeViewData childNode = BuildTreeViewData(childProduct);
            //        treeData.Children.Add(childNode);
            //    }
            //}

            return treeData;
        }


        private void DrawingControl_SelectedEntityChanged(object sender, SelectionChangedEventArgs e)
        {
            List<int> selectedIds = new List<int>();
            if(Viewer3D == null)
            {
                return;
            }
            foreach (IPersistEntity item in Viewer3D.Selection)
            {
                // 获取所选组件的 ID
                if (item.Model != null)
                {
                    TVSelect.ItemsSource = null;
                    List<IfcObjectNode> rootNodes = new List<IfcObjectNode>();


                    IEnumerable<XbimShapeInstance> instances = context.ShapeInstances();

                    double weldLength = 0.0;
                    IEnumerable<IfcPropertySet> propertySets = ((IfcObject)item).PropertySets;
                    foreach (IfcPropertySet propertySet in propertySets)
                    {
                        // 判断属性集合是否是`IIfcPropertySet`类型
                        if (propertySet is IIfcPropertySet ifcPropertySet)
                        {
                            // 获取属性集合中的属性列表
                            IItemSet<IIfcProperty> properties = ifcPropertySet.HasProperties;

                            string ss = ((IfcRoot)((Xbim.Common.Collections.ProxyItemSet<Xbim.Ifc2x3.PropertyResource.IfcProperty, IIfcProperty>)properties).OwningEntity).FriendlyName;

                            IfcObjectNode rootNode = new IfcObjectNode
                            {
                                Name = ss,
                                Properties = new List<IfcPropertyNode>()
                            };


                            foreach (IIfcProperty property in properties)
                            {
                                // 获取属性名称
                                string propertyName = property.Name;

                                // 获取属性值
                                string propertyValue = string.Empty;
                                string propertyDesc = property.Description?.ToString();

                                if (property is IIfcPropertySingleValue singleValueProperty)
                                {
                                    propertyValue = singleValueProperty.NominalValue?.ToString();
                                }
                                else if (property is IIfcPropertyTableValue tableValueProperty)
                                {
                                    propertyValue = "Table Value";
                                    // 这里可以进一步处理表格值的数据
                                }
                                else if (property is IIfcPropertyEnumeratedValue enumeratedValueProperty)
                                {
                                    propertyValue = "Enumerated Value";
                                    // 这里可以进一步处理枚举值的数据
                                }
                                else if (property is IIfcPropertyBoundedValue boundedValueProperty)
                                {
                                    propertyValue = "Bounded Value";
                                    // 这里可以进一步处理有界值的数据
                                }
                                // 还可以根据需要处理其他类型的属性

                                if (propertySet.Name.ToString() == "Tekla Weld Common" && ((IfcRoot)item).Name == "Weld" &&
                                    property.Name.ToString().ToUpper() == "LENGTH")
                                {
                                    weldLength = Convert.ToDouble(propertyValue);
                                }


                                IfcPropertyNode propertyNode = new IfcPropertyNode
                                {
                                    PropertyName = propertyName,
                                    PropertyValue = propertyValue
                                };
                                rootNode.Properties.Add(propertyNode);

                            }

                            rootNodes.Add(rootNode);

                        }






                    }


                    if (((IfcRoot)item).Name == "Weld")
                    {

                        // 获取焊缝对象
                        //Weld weld = item as Weld; // 假设 selectedObject 是你选择的焊缝对象

                        //// 获取焊缝的两个连接部件
                        //Part mainPart = weld.MainObject as Part;
                        //Part secondaryPart = weld.SecondaryObject as Part;





                        IfcObjectNode rootNodeWeld = new IfcObjectNode
                        {
                            Name = "焊缝",
                            Properties = new List<IfcPropertyNode>()
                        };


                        IfcPropertyNode propertyNodeWeld = new IfcPropertyNode
                        {
                            PropertyName = "Length",
                            PropertyValue = weldLength.ToString()
                        };
                        rootNodeWeld.Properties.Add(propertyNodeWeld);

                        XbimShapeInstance instance = instances.FirstOrDefault(x => x.IfcProductLabel == item.EntityLabel);
                        XbimVector3D global_pos = instance.Transformation.Translation;


                        //var geometry = context.ShapeGeometry(instance);
                        //var data = ((IXbimShapeGeometryData)geometry).ShapeData;

                        propertyNodeWeld = new IfcPropertyNode
                        {
                            PropertyName = "中心",
                            PropertyValue = $"{Math.Round(global_pos.X, 2)}, {Math.Round(global_pos.Y, 2)}, {Math.Round(global_pos.Z, 2)}"
                        };

                        rootNodeWeld.Properties.Add(propertyNodeWeld);

                        //法向坐标
                        List<XbimVector3D> p = ((IfcLocalPlacement)((IfcProduct)item).ObjectPlacement).RelativePlacement.P;

                        // 创建 LineSegment 对象并设置属性值
                        LineSegment lineSegment = new LineSegment
                        {
                            Length = weldLength,
                            CenterPoint = new double[] { global_pos.X, global_pos.Y, global_pos.Z },
                            XDirection = new double[] { p[0].X, p[0].Y, p[0].Z },
                            YDirection = new double[] { p[1].X, p[1].Y, p[1].Z },
                            ZDirection = new double[] { p[2].X, p[2].Y, p[2].Z }
                        };

                        // 获取线段的两个端点坐标
                        double[] endPoint1 = lineSegment.GetEndPoint1();
                        double[] endPoint2 = lineSegment.GetEndPoint2();


                        propertyNodeWeld = new IfcPropertyNode
                        {
                            PropertyName = "端点1",
                            PropertyValue = $"{string.Join(", ", endPoint1)} "
                        };

                        rootNodeWeld.Properties.Add(propertyNodeWeld);

                        propertyNodeWeld = new IfcPropertyNode
                        {
                            PropertyName = "端点2",
                            PropertyValue = $"{string.Join(", ", endPoint2)}"
                        };

                        rootNodeWeld.Properties.Add(propertyNodeWeld);

                        rootNodes.Add(rootNodeWeld);
                    }
                    else
                    {
                        IfcObjectNode rootNodeWeld = new IfcObjectNode
                        {
                            Name = "中心坐标",
                            Properties = new List<IfcPropertyNode>()
                        };


                        IfcPropertyNode propertyNodeWeld = new IfcPropertyNode
                        {
                            PropertyName = "Length",
                            PropertyValue = weldLength.ToString()
                        };
                        rootNodeWeld.Properties.Add(propertyNodeWeld);

                        XbimShapeInstance instance = instances.FirstOrDefault(x => x.IfcProductLabel == item.EntityLabel);
                        XbimVector3D global_pos = instance.Transformation.Translation;


                        //var geometry = context.ShapeGeometry(instance);
                        //var data = ((IXbimShapeGeometryData)geometry).ShapeData;

                        propertyNodeWeld = new IfcPropertyNode
                        {
                            PropertyName = "中心",
                            PropertyValue = $"{Math.Round(global_pos.X, 2)}, {Math.Round(global_pos.Y, 2)}, {Math.Round(global_pos.Z, 2)}"
                        };
                        rootNodeWeld.Properties.Add(propertyNodeWeld);
                        rootNodes.Add(rootNodeWeld);
                    }

                    TVSelect.ItemsSource = rootNodes;

                }
            }

        }

        private void DrawingControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

     
        private void HXView3DX_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //DrawBeam(listIndex);

            //listIndex++;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadModel();
        }

        private void HXView3DX_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {


        }

        private void HXView3DX_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //HelixToolkit.Wpf.SharpDX.PerspectiveCamera camera = ViewerDX.Camera as HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

            //// 计算相机的缩放系数
            //double zoomFactor = e.Delta > 0 ? 1.1 : 0.9; // 根据滚轮滚动方向调整缩放系数

            //// 修改相机的缩放系数
            //camera.FieldOfView *= zoomFactor;

            //// 修改字体的缩放系数，使其与相机的缩放系数保持一致
            //double textScale = 1 / zoomFactor;
            //billboardText.Scale *= new Vector3D(textScale, textScale, textScale);
        }
    }



    public class IfcObjectNode : DependencyObject
    {
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(IfcObjectNode));

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(List<IfcPropertyNode>), typeof(IfcObjectNode));

        public List<IfcPropertyNode> Properties
        {
            get { return (List<IfcPropertyNode>)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }
    }

    public class IfcPropertyNode : DependencyObject
    {
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(IfcPropertyNode));

        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyValueProperty =
            DependencyProperty.Register("PropertyValue", typeof(string), typeof(IfcPropertyNode));

        public string PropertyValue
        {
            get { return (string)GetValue(PropertyValueProperty); }
            set { SetValue(PropertyValueProperty, value); }
        }
    }


    public class TreeViewData
    {
        public string Name { get; set; }
        public string IfcEntityLabel { get; set; }
        public string IfcType { get; set; }
        // 添加其他你希望显示的IFC元素属性

        public List<TreeViewData> Children { get; set; } = new List<TreeViewData>();
    }

    public class ProperityDataClass
    {
        public string strName { get; set; }

        public string strValue { get; set; }

        public string strDesc { get; set; }
    }
}
