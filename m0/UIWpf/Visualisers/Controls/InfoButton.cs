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
using System.Windows.Media.Imaging;

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
            Image i = new Image();
            BitmapImage b = new BitmapImage(new Uri("mag.gif", UriKind.Relative));
            int q = b.PixelHeight; // will not load without this
            i.Source = b;            
            
            Content = i;

            this.Style = (Style)Application.Current.FindResource("TransparentStyle");

            BorderThickness = new Thickness(0);
            this.Margin = new Thickness(0);
            this.Padding = new Thickness(0);

        
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
