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
    public partial class NewVertexBySchema : UserControl
    {
        public override string ToString()
        {
            return "New Vertex by Schema";
        }


        IVertex Vertex, MetaVertex;

        public NewVertexBySchema(IVertex _Vertex, IVertex _MetaVertex)
        {
            InitializeComponent();

            Vertex = _Vertex;
            MetaVertex = _MetaVertex;

            NewButton.Content = "New " + MetaVertex.Value + " Vertex";

            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            Content.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Vertex.AddVertex(MetaVertex, this.Content.Text);

            MinusZero.Instance.DefaultShow.CloseWindowByContent(this);
        }
    }
}
