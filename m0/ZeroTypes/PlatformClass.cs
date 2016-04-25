using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Util;
using m0.Graph;

namespace m0.ZeroTypes
{
    class PlatformClassVertexChangeListener{
        public List<string> WatchList = new List<string>();

        public event VertexChange Change;

        public virtual Delegate[] GetChangeDelegateInvocationList()
        {
            return Change.GetInvocationList();
        }

        public IVertex PlatformClassVertex;

        public void Listener(object sender, VertexChangeEventArgs e){
            if ((sender == PlatformClassVertex) && (e.Type==VertexChangeType.EdgeAdded) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value,"$Is")))
            {
                IVertex AttributeVertexes = PlatformClassVertex.GetAll(@"$Is:{$Inherits:$PlatformClass}\Selector:");

                foreach (IEdge ed in AttributeVertexes)
                    if (e.Edge.Meta == ed.To)
                    {
                        GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(e.Edge.To, this.Listener);
                        if(GeneralUtil.CompareStrings(e.Edge.Meta.Value,"BaseEdge"))
                            foreach(IEdge ee in e.Edge.To)
                                GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(ee.To, this.Listener);                                
                    }
            }

            if ((sender == PlatformClassVertex.Get("SelectedEdges:")) && (e.Type == VertexChangeType.EdgeAdded) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value,"$Is")))
                GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(e.Edge.To, this.Listener);

            if ((sender == PlatformClassVertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))
                GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(e.Edge.To, this.Listener);                                        
            
            if ((sender == PlatformClassVertex) && (e.Type == VertexChangeType.EdgeRemoved) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))
            {
                IVertex AttributeVertexes = PlatformClassVertex.GetAll(@"$Is:{$Inherits:$PlatformClass}\Selector:");

                foreach (IEdge ed in AttributeVertexes)
                    if (e.Edge.Meta == ed.To)
                    {
                        e.Edge.To.Change -= new VertexChange(this.Listener);

                        if (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge"))
                            foreach (IEdge ee in e.Edge.To)
                                ee.To.Change -= new VertexChange(this.Listener);
                    }                
            }

            if ((sender == PlatformClassVertex.Get("SelectedEdges:")) && (e.Type == VertexChangeType.EdgeRemoved) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))
                e.Edge.To.Change -= new VertexChange(this.Listener);                                

            if(Change!=null)
                Change(sender, e);
        }
    }

    public class PlatformClass
    {
        public static IPlatformClass CreatePlatformObject(IVertex Vertex)
        {
            if (Vertex.Get("$Is:Class") != null)
            {
                String classname = (string)Vertex.Get("$PlatformClassName:").Value;

                return (IPlatformClass)Activator.CreateInstance(Type.GetType(classname), null);
            }
            else
            {
                String classname = (string)Vertex.Get(@"$Is:{$Inherits:$PlatformClass}\$PlatformClassName:").Value;

                IPlatformClass pc=(IPlatformClass)Activator.CreateInstance(Type.GetType(classname), null);

                pc.Vertex = Vertex;

                return pc;
            }
        }

        public static void RegisterVertexChangeListeners(IVertex PlatformClassVertex, VertexChange action, string[] watchList){
            PlatformClassVertexChangeListener listener=new PlatformClassVertexChangeListener();
            listener.PlatformClassVertex = PlatformClassVertex;
            listener.Change += action;


            PlatformClassVertex.Change += new VertexChange(listener.Listener);

            IVertex AttributeVertexes = PlatformClassVertex.GetAll(@"$Is:{$Inherits:$PlatformClass}\Selector:");

            foreach (IEdge e in AttributeVertexes)
            {
                foreach (IEdge ee in PlatformClassVertex.GetAll(e.To.Value + ":"))
                {
                    GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(ee.To, listener.Listener);

                    if (GeneralUtil.CompareStrings(ee.Meta.Value, "BaseEdge"))                    
                        foreach (IEdge eee in ee.To)
                            GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(eee.To, listener.Listener);                             
                }
            }
        }

        public static void RemoveVertexChangeListeners(IVertex PlatformClassVertex, VertexChange action)
        {
            RemoveVertexChangeListeners_ForVertex(PlatformClassVertex,PlatformClassVertex, action);

            IVertex AttributeVertexes = PlatformClassVertex.GetAll(@"$Is:{$Inherits:$PlatformClass}\Selector:");

            foreach (IEdge e in AttributeVertexes)
            {
                foreach (IEdge ee in PlatformClassVertex.GetAll(e.To.Value + ":"))                    
                    RemoveVertexChangeListeners_ForVertex(e.To, PlatformClassVertex,action);

                foreach (IEdge ee in PlatformClassVertex.GetAll(e.To.Value + @":\"))
                    RemoveVertexChangeListeners_ForVertex(e.To, PlatformClassVertex, action);
            }
        }

        private static void RemoveVertexChangeListeners_ForVertex(IVertex Vertex, IVertex PlatformClassVertex, VertexChange action)
        {
            Delegate[] delegates=Vertex.GetChangeDelegateInvocationList();

            if(delegates!=null)
            foreach (Delegate d in delegates)                            
                if (d.Target is PlatformClassVertexChangeListener)
                {
                    PlatformClassVertexChangeListener list = (PlatformClassVertexChangeListener)d.Target;

                    if (list.PlatformClassVertex == PlatformClassVertex)
                    {
                        list.Change -= action;

                        Vertex.Change -= list.Listener;
                    }
                }                
            }            
        
    }
}
