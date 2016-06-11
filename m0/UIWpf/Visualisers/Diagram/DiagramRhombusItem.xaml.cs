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
    /// Interaction logic for DiagramRhombusItem.xaml
    /// </summary>
    public partial class DiagramRhombusItem : DiagramRectangleItemBase
    {

        public override void SetBackAndForeground()
        {
            this.Foreground = ForegroundColor;
            this.Rhombus.Stroke = ForegroundColor;
            this.Rhombus.Fill = BackgroundColor;            
        }

        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();


            if(LineWidth!=-1&&LineWidth!=0)
                this.Rhombus.StrokeThickness = LineWidth;
        }

        public DiagramRhombusItem()
        {
            InitializeComponent();
        }
    }
}
