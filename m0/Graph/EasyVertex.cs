﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Store;
using m0;
using m0.Util;
using m0.ZeroCode;

namespace m0.Graph
{
    [Serializable]
    public class EasyVertex:VertexBase, IDisposable
    {        
        public override int UsageCounter
        {
            get
            {            
                return base.UsageCounter;
            }
            set
            {
                if(base.UsageCounter < value && UsageCounter == 0 && Store.DetachState == DetachStateEnum.Attached)
                        Store.StoreVertexIdentifier(this);

                if (base.UsageCounter > value && UsageCounter == 1 && Store.DetachState == DetachStateEnum.Attached)
                        Store.RemoveVertexIdentifier(this);

                base.UsageCounter = value;
            }
        }

        protected string _Identifier;
        
        public override string Identifier { get { return _Identifier; }}


        protected object _Value;

        public override object Value {
            get{
                return _Value;
            }
            set{
                _Value = value;

                FireChange(new VertexChangeEventArgs(VertexChangeType.ValueChanged, null));
            }
        }

        protected bool HasInheritance=false;

        protected int InheritanceCount = 0;

        protected IList<IEdge> _InEdges;

        public override IEnumerable<IEdge> InEdges
        {
            get
            {
                if (HasInheritance)
                {
                    List<IEdge> FullEdges = _InEdges.ToList();

                    foreach (IEdge e in _InEdges)
                        if (GeneralUtil.CompareStrings(e.Meta.Value, "$Inherits"))
                            FullEdges.AddRange(e.To);

                    return FullEdges;
                }
                else
                    return _InEdges;
            }
        }

        protected IList<IEdge> _OutEdges;

        public override IEnumerable<IEdge> OutEdges
        {
            get
            {
                if (HasInheritance)
                {
                    List<IEdge> FullEdges = _OutEdges.ToList();

                    foreach (IEdge e in _OutEdges)
                        if (GeneralUtil.CompareStrings(e.Meta.Value, "$Inherits"))
                            FullEdges.AddRange(e.To);

                    //return FullEdges;
                    return ZeroCodeEngine.RemoveDuplicates(FullEdges);
                }else
                    return _OutEdges;
            }
        }

        public override void AddInEdge(IEdge edge)
        {
            _InEdges.Add(edge);

            UsageCounter++;            

            //FireChange(new VertexChangeEventArgs(VertexChangeType.InEdgeAdded,edge));
            // not needed as for now
        }

        public override void DeleteInEdge(IEdge _edge)
        {
            IEdge edge = null;

            foreach (IEdge e in _OutEdges)
                if (e.Meta == _edge.Meta && e.To == _edge.To)
                    edge = e;

            if (_edge != null)
            {
                _InEdges.Remove(edge);

                UsageCounter--;
            }

            //FireChange(new VertexChangeEventArgs(VertexChangeType.InEdgeRemoved, edge));
            // not needed as for now
        }

        public override IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
            // LOG BEGIN
            /*
            if(metaVertex==null)
                MinusZero.Instance.Log(3,"AddEdge", GeneralUtil.EmptyIfNull(this.Value) + " [NULL] " + GeneralUtil.EmptyIfNull(destVertex.Value));
            else
                MinusZero.Instance.Log(3,"AddEdge", GeneralUtil.EmptyIfNull(this.Value) + " [" + GeneralUtil.EmptyIfNull(metaVertex.Value) + "] " + GeneralUtil.EmptyIfNull(destVertex.Value));
             */
            // LOG END

            EdgeBase ne = new EasyEdge(this, metaVertex, destVertex);

            _OutEdges.Add(ne);

            UsageCounter++;

            if (GeneralUtil.CompareStrings(ne.Meta.Value, "$Inherits"))
            {
                InheritanceCount++;

                HasInheritance = true;
            }

            FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, ne));

            return ne;
        }

        public override void DeleteEdge(IEdge _edge)
        {
            IEdge edge = null;

            foreach (IEdge e in _OutEdges)
                if(e.Meta==_edge.Meta && e.To==_edge.To)
                   edge = e;

            if (edge != null)
            {
                _OutEdges.Remove(edge);

                UsageCounter--;

                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                {
                    InheritanceCount--;

                    if (InheritanceCount == 0)
                        HasInheritance = false;
                }

                FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge));

                edge.To.DeleteInEdge(edge);
            }
            //else // becouse of inheritance this may happen
                //throw new Exception(_edge.Meta + " : " + _edge.To + " edge does not exist in given Vertex");
        }              

        public EasyVertex(IStore _store):base(_store)
        {
            _InEdges = new List<IEdge>();
            _OutEdges = new List<IEdge>();

            _Identifier = Guid.NewGuid().ToString();
        }

        public override IVertex Execute(IVertex inputVertex, IVertex expression)
        { 
            //
            MinusZero m0 = MinusZero.Instance;
            m0.Log(2,"Execute", "\"" + expression + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex ret= MinusZero.Instance.DefaultExecuter.Execute(this, inputVertex, expression);

            //
            m0.DoLog = prevDoLog;
            //

            return ret;
        }

        private static IDictionary<String, IVertex> ParseChache= new Dictionary<String,IVertex>();

        public override IVertex Get(string query)
        {
            //
            MinusZero m0=MinusZero.Instance;
            m0.Log(2,"Get", "\"" + query + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex queryVertex = null;
            IVertex parseError = null;

            if (ParseChache.ContainsKey(query))
                queryVertex = ParseChache[query];
            else            
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                parseError = MinusZero.Instance.DefaultParser.Parse(queryVertex, query);

                if (parseError == null)
                    ParseChache.Add(query, queryVertex);
            }
                            
            //
            m0.DoLog = prevDoLog;
            //

            if (parseError != null)
                return null;

            return MinusZero.Instance.DefaultExecuter.Get(this, queryVertex);            
        }

        public override IVertex GetAll(string query)
        {
            //
            MinusZero m0 = MinusZero.Instance;
            m0.Log(2, "GetAll", "\"" + query + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex queryVertex = null;
            IVertex parseError = null;

            if (ParseChache.ContainsKey(query))
                queryVertex = ParseChache[query];
            else
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                parseError = MinusZero.Instance.DefaultParser.Parse(queryVertex, query);

                if (parseError == null)
                    ParseChache.Add(query, queryVertex);
            }

            //
            m0.DoLog = prevDoLog;
            //

            if (parseError != null)
                return null;

            return MinusZero.Instance.DefaultExecuter.GetAll(this, queryVertex);
        }

        public void Dispose()
        {
            GraphUtil.RemoveAllEdges(this);                        
        }
    }
}
