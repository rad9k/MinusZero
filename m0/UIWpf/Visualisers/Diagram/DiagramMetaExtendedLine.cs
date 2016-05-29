using m0.UIWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0.UIWpf.Visualisers.Diagram
{
    public class DiagramMetaExtendedLine: DiagramLine
    {
        public DiagramItemBase MetaDiagramItem;

        protected ArrowPolyline MetaLine = new ArrowPolyline();

        public override void SetPosition(double FromX, double FromY, double ToX, double ToY, bool isSelfRelation, double selfRelationX, double selfRelationY)
        {
            base.SetPosition(FromX, FromY, ToX, ToY, isSelfRelation, selfRelationX, selfRelationY);

            if (MetaDiagramItem == null)
                return;

            PointCollection pc = new PointCollection();
           
            if (isSelfRelation)
            {
            }
            else
            {
                Point p=new Point(FromX+((ToX-FromX)/2), FromY+((ToY-FromY)/2));
                pc.Add(p);
                pc.Add(MetaDiagramItem.GetLineAnchorLocation(null,p,1,1,false));

                MetaLine.Points = pc;
            }
            
            
        }

        public override void AddToCanvas()
        {
            base.AddToCanvas();

            if(Vertex.Get(@"BaseEdge:\Meta:")==null)
                return;

            foreach (DiagramItemBase i in Diagram.Items)
                if (i.Vertex.Get(@"BaseEdge:\To:") == Vertex.Get(@"BaseEdge:\Meta:"))
                    MetaDiagramItem = i;

            if(MetaDiagramItem!=null)
                Diagram.TheCanvas.Children.Add(MetaLine);
            
        }

        public DiagramMetaExtendedLine()
        {
            MetaLine.IsEndings = false;
            MetaLine.StrokeThickness = 1;
            MetaLine.Stroke = (Brush)LineEndings.FindResource("0ForegroundBrush");
            MetaLine.StrokeDashArray = new DoubleCollection(new double[] { 5, 3 });
        }

        protected override void UpdateLine()
        {
            base.UpdateLine();

            MetaLine.StrokeThickness = LineWidth;            
        }

        protected override void UpdateLineEnds()
        {
            base.UpdateLineEnds();

            MetaLine.Stroke = ForegroundColor;
        }

        public override void Highlight()
        {
            base.Highlight();
            
            MetaLine.Stroke = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            
            Panel.SetZIndex(MetaLine, 99999);
        }

        public override void Unhighlight()
        {
            base.Unhighlight();
            
            MetaLine.Stroke = ForegroundColor;
            
            Panel.SetZIndex(MetaLine, 0);
        }
    }
}
