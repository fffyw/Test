using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xbim.Common;
using Xbim.Ifc;

namespace BRToolBox
{
    /// <summary>
    /// Interaction logic for XbimControl.xaml
    /// </summary>
    public partial class XbimControl : UserControl
    {
        public XbimControl()
        {
            InitializeComponent();
            ModelProvider = new ObjectDataProvider { IsInitialLoadEnabled = false };
        }


             public static readonly DependencyProperty ModelProviderProperty = DependencyProperty.Register("ModelProvider", typeof(ObjectDataProvider), typeof(XbimControl));

        #region DataContext

        public ObjectDataProvider ModelProvider
        {
            get { return (ObjectDataProvider)GetValue(ModelProviderProperty); }
            private set { SetValue(ModelProviderProperty, value); }
        }

        #endregion

        public IfcStore Model
        {
            get { return ModelProvider.ObjectInstance as IfcStore; }
        }

        public IPersistEntity SelectedElement
        {
            get => DrawingControl.SelectedEntity;
            set => DrawingControl.SelectedEntity = value;
        }

        public Xbim.Presentation.DrawingControl3D.SelectionBehaviours SelectionBehaviour
        {
            get => DrawingControl.SelectionBehaviour;
            set => DrawingControl.SelectionBehaviour = value;
        }

        public Xbim.Presentation.EntitySelection Selection
        {
            get => DrawingControl.Selection;
            set => DrawingControl.Selection = value;
        }

        public delegate void SelectionChangedHandler(object sender, SelectionChangedEventArgs e);

        public event SelectionChangedEventHandler SelectionChanged;

        private void DrawingControl_SelectedEntityChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }
    }
}
