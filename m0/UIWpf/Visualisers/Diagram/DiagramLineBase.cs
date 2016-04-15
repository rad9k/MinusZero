﻿using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace m0.UIWpf.Visualisers.Diagram
{
    public class DiagramLineBase
    {
        public Diagram Diagram;

        public DiagramItemBase FromDiagramItem;

        public DiagramItemBase ToDiagramItem;

        public virtual IVertex Vertex { get; set; }

        public bool IsHighlighted;

        public double LineWidth;

        public Brush BackgroundColor;

        public Brush ForegroundColor;

        public virtual void SetPosition(double FromX, double FromY, double ToX, double ToY, bool isSelfRelation, double selfRelationX, double selfRelationY)
        {
        }

        public virtual void AddToCanvas()
        {
        }

        public virtual void RemoveFromCanvas()
        {
        }

        public virtual void Highlight()
        {
        }

        public virtual void Unhighlight()
        {
        }


    }
}
