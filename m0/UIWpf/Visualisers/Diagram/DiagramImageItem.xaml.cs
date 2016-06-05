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

namespace m0.UIWpf.Visualisers.Diagram
{
    /// <summary>
    /// Interaction logic for DiagramImageItem.xaml
    /// </summary>
    public partial class DiagramImageItem : DiagramRectangleItemBase
    {
        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();
            
            BitmapImage b = new BitmapImage(new Uri("images\\"+Vertex.Get("Filename:"), UriKind.Relative));
            int q = b.PixelHeight; // will not load without this
            Image.Source = b; 
        }

        public DiagramImageItem()
        {
            InitializeComponent();
        }
    }
}
