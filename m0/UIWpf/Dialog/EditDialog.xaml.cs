using m0.Foundation;
using m0.ZeroTypes;
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
using System.Windows.Shapes;

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class EditDialog : Window
    {
        public EditDialog(IEdge baseEdge)
        {
            InitializeComponent();

            Owner = m0Main.Instance;

            this.Title = "new " + baseEdge.Meta.Value + " dialog";

            Edge.ReplaceEdgeEdges(FormVisuliser.Vertex.Get("BaseEdge:"), baseEdge);

            ShowDialog();
        }
    }
}
