using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using m0.Foundation;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;

namespace m0.UIWpf.Dialog
{

    public partial class NewDiagram : UserControl
    {
        public override string ToString()
        {

            return "New Diagram";
        }

        IVertex Vertex;

        public NewDiagram(IVertex _Vertex)
        {
            InitializeComponent();

            Vertex = _Vertex;
            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            //Keyboard.Focus(Content);
            //FocusManager.SetFocusedElement(this,Content);
            Content.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            IVertex dv = VertexOperations.AddInstance(Vertex, MinusZero.Instance.Root.Get(@"System\Meta\Visualiser\Class:Diagram"));

            if (dv != null)
            {
                dv.Value = Content.Text;

                GraphUtil.ReplaceEdge(dv, "CreationPool", Vertex);
            }

            MinusZero.Instance.DefaultShow.CloseWindowByContent(this);
        }
    }
}
