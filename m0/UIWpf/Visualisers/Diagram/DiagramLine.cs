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

                PlatformClass.RegisterVertexChangeListeners(Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges", "ForegroundColor", "BackgroundColor" });
            }
        }

        private void VertexChange(object sender, VertexChangeEventArgs args)
        {
            if ((args.Type == VertexChangeType.EdgeAdded && (GeneralUtil.CompareStrings(args.Edge.Meta.Value,"IsDashed")||GeneralUtil.CompareStrings(args.Edge.Meta.Value,"LineWidth")))
                || (args.Type == VertexChangeType.ValueChanged && (sender == Vertex.Get(@"IsDashed:")||sender == Vertex.Get(@"LineWidth:"))))
            {
                UpdateLine();
            }

            if ((args.Type == VertexChangeType.EdgeAdded && (GeneralUtil.CompareStrings(args.Edge.Meta.Value,"StartAnchor") || GeneralUtil.CompareStrings(args.Edge.Meta.Value,"EndAnchor")))
                || (args.Type == VertexChangeType.ValueChanged && (sender == Vertex.Get(@"StartAnchor:")||sender == Vertex.Get(@"EndAnchor:"))))
            {
                UpdateLineEnds();
            }

            if ((args.Type == VertexChangeType.EdgeAdded && (GeneralUtil.CompareStrings(args.Edge.Meta.Value, "BackgroundColor") || GeneralUtil.CompareStrings(args.Edge.Meta.Value, "ForegroundColor")))
                || (args.Type == VertexChangeType.ValueChanged && (
                  sender == Vertex.Get(@"BackgroundColor:") || sender == Vertex.Get(@"BackgroundColor:\Red:") || sender == Vertex.Get(@"BackgroundColor:\Green:") || sender == Vertex.Get(@"BackgroundColor:\Blue:") || sender == Vertex.Get(@"BackgroundColor:\Opacity:") ||
                   sender == Vertex.Get(@"ForegroundColor:") || sender == Vertex.Get(@"ForegroundColor:\Red:") || sender == Vertex.Get(@"ForegroundColor:\Green:") || sender == Vertex.Get(@"ForegroundColor:\Blue:") || sender == Vertex.Get(@"ForegroundColor:\Opacity:")       
                )))
            {
                UpdateLineEnds();
            }
        }

        private void UpdateLine()
        {
            if (GraphUtil.GetDoubleValue(Vertex.Get("LineWidth:")) != GraphUtil.NullDouble)
                LineWidth = GraphUtil.GetDoubleValue(Vertex.Get("LineWidth:"));
            else
                LineWidth = 1;

            Line.StrokeThickness = LineWidth;
            LineEndings.StrokeThickness = LineWidth;

            if (GeneralUtil.CompareStrings(Vertex.Get("IsDashed:"), "True"))
                Line.StrokeDashArray = new DoubleCollection(new double[] { 5, 3 });
            else
                Line.StrokeDashArray = null;
        }

        private void UpdateLineEnds()
        {
            if (Vertex.Get("BackgroundColor:") != null)
                BackgroundColor = UIWpf.GetBrushFromColorVertex(Vertex.Get("BackgroundColor:"));
            else
                BackgroundColor = (Brush)Line.FindResource("0BackgroundBrush");

            if (Vertex.Get("ForegroundColor:") != null)
                ForegroundColor = UIWpf.GetBrushFromColorVertex(Vertex.Get("ForegroundColor:"));
            else
                ForegroundColor = (Brush)Line.FindResource("0ForegroundBrush");

            LineEndings.Stroke = ForegroundColor;
            Line.Stroke = ForegroundColor;
            Label.Foreground = ForegroundColor;


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

                FillBrush = BackgroundColor;
                HighlightFillBrush = BackgroundColor;
            }

            if (EndAnchor == "Triangle")
            {
                LineEndings.EndEnding = LineEndEnum.Triangle;
                Line.EndEnding = LineEndEnum.Triangle;

                FillBrush = BackgroundColor;
                HighlightFillBrush = BackgroundColor;
            }

            if (StartAnchor == "FilledTriangle")
            {
                LineEndings.StartEnding = LineEndEnum.FilledTriangle;
                Line.StartEnding = LineEndEnum.FilledTriangle;

                FillBrush = ForegroundColor;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (EndAnchor == "FilledTriangle")
            {
                LineEndings.EndEnding = LineEndEnum.FilledTriangle;
                Line.EndEnding = LineEndEnum.FilledTriangle;

                FillBrush = ForegroundColor;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (StartAnchor == "Diamond")
            {
                LineEndings.StartEnding = LineEndEnum.Diamond;
                Line.StartEnding = LineEndEnum.Diamond;

                FillBrush = BackgroundColor;
                HighlightFillBrush = BackgroundColor;
            }

            if (EndAnchor == "Diamond")
            {
                LineEndings.EndEnding = LineEndEnum.Diamond;
                Line.EndEnding = LineEndEnum.Diamond;

                FillBrush = BackgroundColor;
                HighlightFillBrush = BackgroundColor;
            }

            if (StartAnchor == "FilledDiamond")
            {
                LineEndings.StartEnding = LineEndEnum.FilledDiamond;
                Line.StartEnding = LineEndEnum.FilledDiamond;

                FillBrush = ForegroundColor;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (EndAnchor == "FilledDiamond")
            {
                LineEndings.EndEnding = LineEndEnum.FilledDiamond;
                Line.EndEnding = LineEndEnum.FilledDiamond;

                FillBrush = ForegroundColor;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }


            if (FillBrush != null)
                LineEndings.Fill = FillBrush;
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
            Diagram.TheCanvas.Children.Add(Label);
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

            LineEndings.Stroke = ForegroundColor;
            Line.Stroke = ForegroundColor;

            if (FillBrush != null)
                LineEndings.Fill = FillBrush;

            Label.Foreground = ForegroundColor;

            Panel.SetZIndex(LineEndings, 0);
            Panel.SetZIndex(Label, 0); 
        }

        public DiagramLine(){
            ForegroundColor = (Brush)Line.FindResource("0ForegroundBrush");
            BackgroundColor = (Brush)Line.FindResource("0BackgroundBrush");

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

