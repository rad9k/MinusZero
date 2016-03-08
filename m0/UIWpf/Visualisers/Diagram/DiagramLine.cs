using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0.UIWpf.Visualisers.Diagram
{
    public class DiagramLine: DiagramLineBase, IPlatformClass
    {
        private IVertex _Vertex;
        public override IVertex Vertex { get { return _Vertex; }
            set {
                _Vertex = value;

                VertexUpdated();

                PlatformClass.RegisterVertexChangeListeners(Vertex, new VertexChange(VertexChange));
            }
        }

        private void VertexChange(object sender, VertexChangeEventArgs args)
        {
            if (args.Type == VertexChangeType.EdgeAdded && GeneralUtil.CompareStrings(args.Edge.Meta.Value,"IsDashed")){
                if(GeneralUtil.CompareStrings(Vertex.Get("IsDashed:"),"True"))
                     Line.StrokeDashArray = new DoubleCollection(new double[] { 5, 3 });
            }

            if (args.Type == VertexChangeType.EdgeAdded && (GeneralUtil.CompareStrings(args.Edge.Meta.Value,"StartAnchor") || GeneralUtil.CompareStrings(args.Edge.Meta.Value,"EndAnchor")))
            {
                string StartAnchor = (string)GraphUtil.GetValue(Vertex.Get(@"StartAnchor:"));
                string EndAnchor = (string)GraphUtil.GetValue(Vertex.Get(@"EndAnchor:"));

                if (StartAnchor == "Arrow")
                {
                    LineEndings.StartEnding = LineEndEnum.Arrow;
                    Line.StartEnding = LineEndEnum.Arrow;
                }

                if (EndAnchor == "Arrow")
                {
                    LineEndings.EndEnding = LineEndEnum.Arrow;
                    Line.EndEnding = LineEndEnum.Arrow;
                }

                if (StartAnchor == "Triangle")
                {
                    LineEndings.StartEnding = LineEndEnum.Triangle;
                    Line.StartEnding = LineEndEnum.Triangle;

                    FillBrush = (Brush)LineEndings.FindResource("0BackgroundBrush");
                    HighlightFillBrush = (Brush)LineEndings.FindResource("0BackgroundBrush");  
                }

                if (EndAnchor == "Triangle")
                {
                    LineEndings.EndEnding = LineEndEnum.Triangle;
                    Line.EndEnding = LineEndEnum.Triangle;

                    FillBrush = (Brush)LineEndings.FindResource("0BackgroundBrush");
                    HighlightFillBrush = (Brush)LineEndings.FindResource("0BackgroundBrush"); 
                }

                if (StartAnchor == "FilledTriangle")
                {
                    LineEndings.StartEnding = LineEndEnum.FilledTriangle;
                    Line.StartEnding = LineEndEnum.FilledTriangle;

                    FillBrush = (Brush)LineEndings.FindResource("0ForegroundBrush");
                    HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
                }

                if (EndAnchor == "FilledTriangle")
                {
                    LineEndings.EndEnding = LineEndEnum.FilledTriangle;
                    Line.EndEnding = LineEndEnum.FilledTriangle;

                    FillBrush = (Brush)LineEndings.FindResource("0ForegroundBrush");
                    HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
                }

                if (StartAnchor == "Diamond")
                {
                    LineEndings.StartEnding = LineEndEnum.Diamond;
                    Line.StartEnding = LineEndEnum.Diamond;

                    FillBrush = (Brush)LineEndings.FindResource("0BackgroundBrush");
                    HighlightFillBrush = (Brush)LineEndings.FindResource("0BackgroundBrush");
                }

                if (EndAnchor == "Diamond")
                {
                    LineEndings.EndEnding = LineEndEnum.Diamond;
                    Line.EndEnding = LineEndEnum.Diamond;

                    FillBrush = (Brush)LineEndings.FindResource("0BackgroundBrush");
                    HighlightFillBrush = (Brush)LineEndings.FindResource("0BackgroundBrush");
                }

                if (StartAnchor == "FilledDiamond")
                {
                    LineEndings.StartEnding = LineEndEnum.FilledDiamond;
                    Line.StartEnding = LineEndEnum.FilledDiamond;

                    FillBrush = (Brush)LineEndings.FindResource("0ForegroundBrush");
                    HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
                }

                if (EndAnchor == "FilledDiamond")
                {
                    LineEndings.EndEnding = LineEndEnum.FilledDiamond;
                    Line.EndEnding = LineEndEnum.FilledDiamond;

                    FillBrush = (Brush)LineEndings.FindResource("0ForegroundBrush");
                    HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
                }


                if (FillBrush != null)
                    LineEndings.Fill = FillBrush;
            }
        }

        Brush FillBrush=null;
        Brush HighlightFillBrush = null;

        private void VertexUpdated(){
            if (Vertex.Get(@"Definition:Inheritence") != null) // not to display $Inherits
                return;

            if (Vertex.Get(@"BaseEdge:\Meta:\$VertexTarget:")!=null)
            {
                IVertex v=Vertex.Get(@"BaseEdge:\To:");
                if(v.Value!=null&&!GeneralUtil.CompareStrings(v.Value,"$Empty"))
                      Label.Text = (string)v.Value;
            }
            else
            {
                IVertex v = Vertex.Get(@"BaseEdge:\Meta:");
                if (v.Value != null && !GeneralUtil.CompareStrings(v.Value, "$Empty"))
                    Label.Text = (string)v.Value;
            }        
        }

        protected ArrowPolyline LineEndings=new ArrowPolyline();
        protected ArrowPolyline Line = new ArrowPolyline();

        protected TextBlock Label = new TextBlock();

        public override void SetPosition(double FromX, double FromY, double ToX, double ToY, bool isSelfRelation, double selfRelationX, double selfRelationY)
        {
            PointCollection pc = new PointCollection();

            pc.Add(new Point(FromX, FromY));

            if (isSelfRelation)
            {
                pc.Add(new Point(FromX, selfRelationY));
                pc.Add(new Point(selfRelationX,  selfRelationY));
                pc.Add(new Point(selfRelationX, ToY));

                Canvas.SetLeft(Label,  selfRelationX + 3);
                Canvas.SetTop(Label,  selfRelationY);
            }
            else
            {
                Canvas.SetLeft(Label, FromX + ((ToX - FromX) / 2));
                Canvas.SetTop(Label, FromY + ((ToY - FromY) / 2));
            }

            pc.Add(new Point(ToX, ToY));

            LineEndings.Points = pc;
            Line.Points = pc;
        }

        public override void AddToCanvas()
        {
            Diagram.TheCanvas.Children.Add(LineEndings);
            Diagram.TheCanvas.Children.Add(Line);
           // Diagram.TheCanvas.Children.Add(Label);
        }

        public override void RemoveFromCanvas()
        {
            Diagram.TheCanvas.Children.Remove(LineEndings);
            Diagram.TheCanvas.Children.Remove(Line);
            Diagram.TheCanvas.Children.Remove(Label);
        }

        public override void Highlight()
        {
            IsHighlighted = true;

            LineEndings.Stroke = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            Line.Stroke = (Brush)LineEndings.FindResource("0LightHighlightBrush");

            if (HighlightFillBrush != null)            
                LineEndings.Fill = HighlightFillBrush;
                            

            Label.Foreground = (Brush)LineEndings.FindResource("0HighlightBrush");

            Panel.SetZIndex(LineEndings, 99999);
            Panel.SetZIndex(Label, 99999);   
        }

        public override void Unhighlight()
        {
            IsHighlighted = false;

            LineEndings.Stroke = (Brush)LineEndings.FindResource("0ForegroundBrush");
            Line.Stroke = (Brush)LineEndings.FindResource("0ForegroundBrush");

            if (FillBrush != null)
                LineEndings.Fill = FillBrush;

            Label.Foreground = (Brush)LineEndings.FindResource("0ForegroundBrush");

            Panel.SetZIndex(LineEndings, 0);
            Panel.SetZIndex(Label, 0); 
        }

        public DiagramLine(){
            LineEndings.IsEndings = true;
            LineEndings.StrokeThickness = 1;
            LineEndings.Stroke = (Brush)LineEndings.FindResource("0ForegroundBrush");

            LineEndings.ArrowLength = 15;
            LineEndings.ArrowAngle = 60;

            Line.IsEndings = false;
            Line.StrokeThickness = 1;
            Line.Stroke = (Brush)LineEndings.FindResource("0ForegroundBrush");

            Line.ArrowLength = 15;
            Line.ArrowAngle = 60;

            Label.Foreground = (Brush)LineEndings.FindResource("0ForegroundBrush");
        }
    }
}

