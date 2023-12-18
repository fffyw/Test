using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace BRToolBox
{
    public class TextControl3D : UserControl
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        private TextBlock textBlock;

        public TextControl3D()
        {
            textBlock = new TextBlock
            {
                Text = "我的文字"
            };

            Content = textBlock;
        }

        public void UpdatePosition(Point3D position)
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;
            _ = new ModelVisual3D
            {
                Transform = new TranslateTransform3D(X, Y, Z),
                Content = this
            };

            //添加到DrawingControl3D里
        }

        public static implicit operator Model3D(TextControl3D v)
        {
            throw new NotImplementedException();
        }
    }
}
