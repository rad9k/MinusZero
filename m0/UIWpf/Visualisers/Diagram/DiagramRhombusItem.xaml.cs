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
            this.Text.Foreground = ForegroundColor;
            this.Foreground = ForegroundColor;
            this.Rhombus.Stroke = ForegroundColor;
            this.Rhombus.Fill = BackgroundColor;            
        }

        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();

            if (Vertex.Get(@"BaseEdge:\To:").Value != null)
                this.Text.Text = Vertex.Get(@"BaseEdge:\To:").Value.ToString();
            else
                this.Text.Text = "Ø";

            if(LineWidth!=-1&&LineWidth!=0)
                this.Rhombus.StrokeThickness = LineWidth;
        }

        public DiagramRhombusItem()
        {
            InitializeComponent();
        }        

        
        public override void Select()
        {
            base.Select();

            this.Text.Foreground = (Brush)FindResource("0BackgroundBrush");
            this.Rhombus.Stroke = (Brush)FindResource("0BackgroundBrush");
            this.Rhombus.Fill = (Brush)FindResource("0SelectionBrush");            

            this.Rhombus.Cursor = Cursors.ScrollAll;
        }

        public override void Unselect()
        {
            base.Unselect();

            SetBackAndForeground();

            this.Rhombus.Cursor = Cursors.Arrow;
        }

        public override void Highlight()
        {
            base.Highlight();

            this.Text.Foreground = (Brush)FindResource("0HighlightForegroundBrush");
            this.Rhombus.Stroke = (Brush)FindResource("0HighlightForegroundBrush");
            this.Rhombus.Fill = (Brush)FindResource("0HighlightBrush");            
        }

        public override void Unhighlight()
        {
            SetBackAndForeground();

            base.Unhighlight();
        }

        public override Point GetLineAnchorLocation(DiagramItemBase toItem, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart)
        {
            Point p = new Point();

            Point pTo = new Point();

            if (toItem != null)
            {
                pTo.X = Canvas.GetLeft(toItem) + toItem.ActualWidth / 2;
                pTo.Y = Canvas.GetTop(toItem) + toItem.ActualHeight / 2;
            }
            else
                pTo = toPoint;

            double tX = Canvas.GetLeft(this) + this.ActualWidth / 2;
            double tY = Canvas.GetTop(this) + this.ActualHeight / 2;

            double testX = pTo.X - tX;
            double testY = pTo.Y - tY;

            if (testX == 0) testX = 0.001;
            if (testY == 0) testY = 0.001;

            if (toItemDiagramLinesCount > 1)
            {
                if (toItem == this)
                {
                    if (isSelfStart)
                    {
                        p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                        p.Y = tY - this.ActualHeight / 2;

                        return p;
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = Canvas.GetTop(this) + (((double)(toItemDiagramLinesCount - toItemDiagramLinesNumber)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        return p;
                    }
                }

                if (testY <= 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                    p.Y = tY - this.ActualHeight / 2;
                }

                if (testY > 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                    p.Y = tY + this.ActualHeight / 2;
                }

                if (testX >= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + this.ActualWidth / 2;
                    p.Y = Canvas.GetTop(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);
                }

                if (testX <= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - this.ActualWidth / 2;
                    p.Y = Canvas.GetTop(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);
                }
            }
            else
            {
                if (toItem == this)
                {
                    if (isSelfStart)
                    {
                        p.X = tX;
                        p.Y = tY - this.ActualHeight / 2;

                        return p;
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = tY;

                        return p;
                    }
                }

                if (testY <= 0 && testX <=0)
                {
                    p.X = tX - (this.ActualHeight / 2 * testX / testY);
                    p.Y = tY - (this.ActualHeight / 2 * testY / testX);
                }

             /*   if (testY > 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + (this.ActualHeight / 2 * testX / testY);
                    p.Y = tY + this.ActualHeight / 2;
                }

                if (testX >= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + this.ActualWidth / 2;
                    p.Y = tY + (this.ActualWidth / 2 * testY / testX);
                }

                if (testX <= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - this.ActualWidth / 2;
                    p.Y = tY - (this.ActualWidth / 2 * testY / testX);
                }*/
            }

            return p;
        }

    }
}
