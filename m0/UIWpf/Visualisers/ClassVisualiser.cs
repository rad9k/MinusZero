using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.UML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using System.Windows.Media;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using m0.UIWpf.Commands;
using System.Windows;


namespace m0.UIWpf.Visualisers
{
    class ClassVisualiser : TextBlock, IPlatformClass, IDisposable, IHasLocalizableEdges
    {
        public ClassVisualiser()
        {
            MinusZero mz = MinusZero.Instance;            

            if (mz != null && mz.IsInitialized)
            {
                Padding = new Thickness(3);

                Vertex = mz.CreateTempVertex();

                Vertex.Value = "ClassVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributes(Vertex, mz.Root.Get(@"System\Meta\Visualiser\ClassVisualiser"));

                ClassVertex.AddIsClassAndAllAttributes(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));         

                
                this.AllowDrop = false;
                

                this.Loaded += new RoutedEventHandler(OnLoad);
            }
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!UIWpf.HasParentsGotContextMenu(this))
                this.ContextMenu = new m0ContextMenu(this);
        }

        private void UpdateBaseEdge()
        {
            IVertex bv = Vertex.Get(@"BaseEdge:\To:");

            if (bv != null && bv.Value != null /*&& ((String)bv.Value)!="$Empty"*/){
                StringBuilder sb=new StringBuilder();

                bool isFirst=true;

                foreach(IEdge e in bv.GetAll(@"Attribute:")){
                    if(isFirst==false)
                        sb.Append("\n");
                    else
                        isFirst=false;

                    sb.Append(e.To.Value);

                    if (e.To.Get("$EdgeTarget:") != null)
                        sb.Append(" : " + e.To.Get(@"$EdgeTarget:"));

                    string cardinalites = ClassVertex.GetStringCardinalities(e.To);

                    if(cardinalites!="")
                        sb.Append(" "+cardinalites);
                }

                this.Text = sb.ToString();
            }
            else
                this.Text = "Ø";
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();                        

            if ((sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To"))
                || (sender == Vertex.Get(@"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged))            
                UpdateBaseEdge();            
        }        

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                if (_Vertex != null)
                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                UpdateBaseEdge();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        public IVertex GetEdgeByLocation(System.Windows.Point point)
        {
            return Vertex.Get(@"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(System.Windows.FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public System.Windows.FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }
    }
}
