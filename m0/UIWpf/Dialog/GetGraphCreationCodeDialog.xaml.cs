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

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for GetGraphCreationCodeDialog.xaml
    /// </summary>
    public partial class GetGraphCreationCodeDialog : UserControl
    {
        public override string ToString()
        {
            return "Graph generation code";
        }


        public GetGraphCreationCodeDialog(IVertex Vertex)
        {
            InitializeComponent();

            this.Content.Text=MinusZero.Instance.DefaultGraphCreationCodeGenerator.GraphCreationCodeGenerateAsString(Vertex);

            //this.Content.AppendText(MinusZero.Instance.DefaultGraphCreationCodeGenerator.GraphCreationCodeGenerateAsString(Vertex)); ;
        }
    }
}
