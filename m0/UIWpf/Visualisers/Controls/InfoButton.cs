using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using m0.Foundation;
using m0.UIWpf.Visualisers;
using m0.Graph;
using m0.ZeroTypes;
using System.Windows.Media;

namespace m0.UIWpf.Visualisers.Controls
{
    public class InfoButton:Button
    {
         public IEdge BaseEdge
        {
            get { return (IEdge)GetValue(BaseEdgeProperty); }
            set { SetValue(BaseEdgeProperty, value); }
        }

        public static readonly DependencyProperty BaseEdgeProperty =
            DependencyProperty.Register("BaseEdge", typeof(IEdge), typeof(InfoButton), new UIPropertyMetadata(BaseEdgeChangedCallback));

        public static void BaseEdgeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs _e)
        {
            InfoButton _this = (InfoButton)d;
            IEdge e = (IEdge)_e.NewValue;
        }

        public InfoButton()
        {
            Content = "i";
            BorderThickness = new Thickness(0);
            this.Margin = new Thickness(0);
            this.Padding = new Thickness(0);

            Brush ob = new SolidColorBrush();
            ob.Opacity = 1;
            Background = ob;
            Foreground = (Brush)FindResource("0ForegroundBrush"); 
        }

        protected DependencyObject getParentFormVisualiser(DependencyObject e)
        {
            if(e==null)
                return null;

            if(e is FormVisualiser)
                return e;

            return getParentFormVisualiser(VisualTreeHelper.GetParent(e));
        }

        protected override void OnClick(){
            FormVisualiser v=(FormVisualiser)getParentFormVisualiser(this);

            Edge.ReplaceEdgeEdges(v.Vertex.Get("BaseEdge:"), BaseEdge);
        }
        
        
    }
}
