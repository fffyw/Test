// ***********************************************************************
// Assembly         : WeldingTool
// Author           : Administrator
// Created          : 11-02-2023
//
// Last Modified By : Administrator
// Last Modified On : 11-02-2023
// ***********************************************************************
// <copyright file="WeldStructure.cs" company="">
//     Copyright ©  2023
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Point = Tekla.Structures.Geometry3d.Point;

namespace BRToolBox.Class
{
    /// <summary>
    /// Class WeldStructure.
    /// </summary>
    public class WeldStructure
    {
        /// <summary>
        /// Gets or sets the weld identifier.
        /// </summary>
        /// <value>The weld identifier.</value>
        public int WeldId { get; set; }

        /// <summary>
        /// 焊缝与组件接触面的face的顶点坐标信息,对应组件接触面的信息
        /// </summary>
        /// <value>The weld face point.</value>
        public List<Tuple<string, List<Point>,List<Point>>> FaceWeldAndPartInfo { get; set; }

        /// <summary>
        /// Gets or sets the normal list.
        /// </summary>
        /// <value>The normal list.</value>
        public List<Vector> NormalList { get; set; }

        /// <summary>
        /// Gets or sets the polygon weld.
        /// </summary>
        /// <value>The polygon weld.</value>
        public PolygonWeld PolygonWeld { get; set; }

        /// <summary>
        /// Gets or sets the weld main part.
        /// </summary>
        /// <value>The weld main part.</value>
        public ModelObject WeldMainPart { get; set; }

        /// <summary>
        /// Gets or sets the weld second part.
        /// </summary>
        /// <value>The weld second part.</value>
        public ModelObject WeldSecondPart { get; set; }

        /// <summary>
        /// Gets or sets the plate thick1.
        /// </summary>
        /// <value>The plate thick1.</value>
        public double PlateThick { get; set; }

 
        /// <summary>
        /// Gets or sets the start point.
        /// </summary>
        /// <value>The start point.</value>
        public Point StartPoint { get; set; }

        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        /// <value>The end point.</value>
        public Point EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the centra point.
        /// </summary>
        /// <value>The centra point.</value>
        public Point CentraPoint { get; set; }


        /// <summary>
        /// Gets or sets the camera point.
        /// </summary>
        /// <value>The camera point.</value>
        public Point CameraPoint { get; set; }

        /// <summary>
        /// Gets or sets the length of the weld.
        /// </summary>
        /// <value>The length of the weld.</value>
        public double WeldLength { get; set; }

        //SizeAbove
        /// <summary>
        /// Gets or sets the width of the weld.
        /// </summary>
        /// <value>The width of the weld.</value>
        public double WeldWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is around weld.
        /// </summary>
        /// <value><c>true</c> if this instance is around weld; otherwise, <c>false</c>.</value>
        public bool IsAroundWeld { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is connect assemblies.
        /// </summary>
        /// <value><c>true</c> if this instance is connect assemblies; otherwise, <c>false</c>.</value>
        public bool IsConnectAssemblies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [weld effective throat above].
        /// </summary>
        /// <value><c>true</c> if [weld effective throat above]; otherwise, <c>false</c>.</value>
        public bool WeldEffectiveThroatAbove { get; set; }

        /// <summary>
        /// Gets or sets the type of the weld intermittent.
        /// </summary>
        /// <value>The type of the weld intermittent.</value>
        public int  WeldIntermittentType { get; set; }

        //WELD_PROCESS_NONE: 无焊接工艺。
        //WELD_PROCESS_SAW: 电子束焊接。
        //WELD_PROCESS_MIGMAG: 气体保护焊接。
        //WELD_PROCESS_TIG: 氩弧焊接。
        //WELD_PROCESS_STICK: 手工电弧焊接。
        //WELD_PROCESS_SUBMERGED: 焊接半自动埋弧焊。
        /// <summary>
        /// Gets or sets the type of the weld process.
        /// </summary>
        /// <value>The type of the weld process.</value>
        public string  WeldProcessType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is shop weld.
        /// </summary>
        /// <value><c>true</c> if this instance is shop weld; otherwise, <c>false</c>.</value>
        public bool IsShopWeld { get; set; }

        /// <summary>
        /// Gets or sets the weld unit identifier.
        /// </summary>
        /// <value>The weld unit identifier.</value>
        public int WeldUnitID { get; set; }

        /// <summary>
        /// Gets or sets the weld unit order.
        /// </summary>
        /// <value>The weld unit order.</value>
        public int WeldUnitOrder { get; set; }

        /// <summary>
        /// Gets or sets the process identifier.
        /// </summary>
        /// <value>The process identifier.</value>
        public int ProcessID { get; set; }

        /// <summary>
        /// Gets or sets the type of the weld.
        /// </summary>
        /// <value>The type of the weld.</value>
        public string WeldType { get; set; }

        /// <summary>
        /// Gets or sets the weld reference text.
        /// </summary>
        /// <value>The weld reference text.</value>
        public string WeldReferenceText { get; set; }
    }

    public class WeldStructureLight
    {
        /// <summary>
        /// Gets or sets the weld identifier.
        /// </summary>
        /// <value>The weld identifier.</value>
        public int WeldId { get; set; }


        /// <summary>
        /// Gets or sets the normal list.
        /// </summary>
        /// <value>The normal list.</value>
        public List<Vector> NormalList { get; set; }


        /// <summary>
        /// Gets or sets the plate thick1.
        /// </summary>
        /// <value>The plate thick1.</value>
        public double PlateThick { get; set; }


        /// <summary>
        /// Gets or sets the start point.
        /// </summary>
        /// <value>The start point.</value>
        public Point StartPoint { get; set; }

        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        /// <value>The end point.</value>
        public Point EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the centra point.
        /// </summary>
        /// <value>The centra point.</value>
        public Point CentraPoint { get; set; }


        /// <summary>
        /// Gets or sets the camera point.
        /// </summary>
        /// <value>The camera point.</value>
        public Point CameraPoint { get; set; }

        /// <summary>
        /// Gets or sets the length of the weld.
        /// </summary>
        /// <value>The length of the weld.</value>
        public double WeldLength { get; set; }

        //SizeAbove
        /// <summary>
        /// Gets or sets the width of the weld.
        /// </summary>
        /// <value>The width of the weld.</value>
        public double WeldWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is around weld.
        /// </summary>
        /// <value><c>true</c> if this instance is around weld; otherwise, <c>false</c>.</value>
        public bool IsAroundWeld { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is connect assemblies.
        /// </summary>
        /// <value><c>true</c> if this instance is connect assemblies; otherwise, <c>false</c>.</value>
        public bool IsConnectAssemblies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [weld effective throat above].
        /// </summary>
        /// <value><c>true</c> if [weld effective throat above]; otherwise, <c>false</c>.</value>
        public bool WeldEffectiveThroatAbove { get; set; }

        /// <summary>
        /// Gets or sets the type of the weld intermittent.
        /// </summary>
        /// <value>The type of the weld intermittent.</value>
        public int WeldIntermittentType { get; set; }

        //WELD_PROCESS_NONE: 无焊接工艺。
        //WELD_PROCESS_SAW: 电子束焊接。
        //WELD_PROCESS_MIGMAG: 气体保护焊接。
        //WELD_PROCESS_TIG: 氩弧焊接。
        //WELD_PROCESS_STICK: 手工电弧焊接。
        //WELD_PROCESS_SUBMERGED: 焊接半自动埋弧焊。
        /// <summary>
        /// Gets or sets the type of the weld process.
        /// </summary>
        /// <value>The type of the weld process.</value>
        public string WeldProcessType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is shop weld.
        /// </summary>
        /// <value><c>true</c> if this instance is shop weld; otherwise, <c>false</c>.</value>
        public bool IsShopWeld { get; set; }

        /// <summary>
        /// Gets or sets the weld unit identifier.
        /// </summary>
        /// <value>The weld unit identifier.</value>
        public int WeldUnitID { get; set; }

        /// <summary>
        /// Gets or sets the weld unit order.
        /// </summary>
        /// <value>The weld unit order.</value>
        public int WeldUnitOrder { get; set; }

        /// <summary>
        /// Gets or sets the process identifier.
        /// </summary>
        /// <value>The process identifier.</value>
        public int ProcessID { get; set; }

        /// <summary>
        /// Gets or sets the type of the weld.
        /// </summary>
        /// <value>The type of the weld.</value>
        public string WeldType { get; set; }

        /// <summary>
        /// Gets or sets the weld reference text.
        /// </summary>
        /// <value>The weld reference text.</value>
        public string WeldReferenceText { get; set; }
    }
}
