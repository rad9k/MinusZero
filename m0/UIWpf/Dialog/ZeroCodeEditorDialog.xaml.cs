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
using m0.Graph;
using m0.ZeroTypes;

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for ZeroCodeEditorDialog.xaml
    /// </summary>
    public partial class ZeroCodeEditorDialog : UserControl
    {
        IVertex baseVertex;

        public override string ToString()
        {
            return "Zero code editor";
        }

        public ZeroCodeEditorDialog(IVertex baseVertex)
        {
            MinusZero z = MinusZero.Instance;

            InitializeComponent();

            this.baseVertex = baseVertex;

            IVertex SchemaEdge = MinusZero.Instance.CreateTempVertex();
            Edge.AddEdgeEdgesOnlyTo(SchemaEdge, z.Root.Get(@"System\Meta"));
            GraphUtil.ReplaceEdge(this.Schema.Vertex.Get("BaseEdge:"), "To", SchemaEdge);

            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            Content.Focus();
        }

        private void Parse_Click(object sender, RoutedEventArgs e)
        {
            MinusZero z = MinusZero.Instance;

            GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get("BaseEdge:"),"To",z.Empty);            

            IVertex res=z.DefaultParser.Parse(baseVertex,this.Content.Text);

            if(res!=null)
                GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get("BaseEdge:"), "To", res);

            e.Handled = true;
        }
        
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            MinusZero z = MinusZero.Instance;

            GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get("BaseEdge:"),"To",z.Empty);            

            IVertex expressionAsVertex = z.CreateTempVertex();

            IVertex res=z.DefaultParser.Parse(expressionAsVertex, Content.Text);

            if (res != null)
            {
                GraphUtil.ReplaceEdge(this.Resoult.Vertex.Get("BaseEdge:"), "To", res);
                return;
            }

            res=z.DefaultExecuter.Execute(baseVertex, this.Schema.Vertex.Get(@"BaseEdge:\To:\To:"), expressionAsVertex);

            if (res != null)            
                GraphUtil.ReplaceEdge(this.Resoult.Vertex, "BaseVertex", res);
            
        }
    }
}
