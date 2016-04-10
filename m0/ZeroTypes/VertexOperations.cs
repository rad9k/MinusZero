using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.ZeroTypes
{
    public class VertexOperations
    {
        public static void DeleteEdge(IVertex source, IVertex metaVertex, IVertex toVertex)
        {
            GraphUtil.DeleteEdge(source, metaVertex, toVertex);
        }

        public static IVertex GetChildEdges(IVertex metaVertex)
        {                        
            IVertex edgeTarget = metaVertex.Get("$EdgeTarget:");
            if (edgeTarget != null)
                return GetChildEdges(edgeTarget);

            IVertex ret = m0.MinusZero.Instance.CreateTempVertex();

            foreach (IEdge e in metaVertex)
            {
                if(GeneralUtil.CompareStrings(e.Meta,"$VertexTarget"))
                    ret.AddEdge(null,m0.MinusZero.Instance.Root.Get(@"System\Meta\UML\Vertex\$EdgeTarget"));
                else
                    //if (!GeneralUtil.CompareStrings(e.Meta, "$Is") && !GeneralUtil.CompareStrings(e.Meta, "$Inherits")) // to be extanded
                    if (GeneralUtil.CompareStrings(e.Meta, "$Empty")||((string)e.Meta.Value)[0] != '$') // is extanded
                         ret.AddEdge(null,e.To);
            }

            return ret;
        }

        public static IVertex DoFilter(IVertex baseVertex, IVertex FilterQuery)
        {
            return baseVertex.GetAll((string)FilterQuery.Value);
        }

        public static bool InheritanceCompare(IVertex baseVertex, string toCompare)
        {
            if (GeneralUtil.CompareStrings(baseVertex.Value, toCompare))
                return true;

            foreach (IEdge e in baseVertex.GetAll("$Inherits:"))
                if (InheritanceCompare(e.To, toCompare))
                    return true;

            return false;
        }

        public static IVertex TestIfNewEdgeValid(IVertex baseVertex, IVertex metaVertex, IVertex toVertex)
        {        
            int MaxCardinality=GraphUtil.GetIntegerValue(metaVertex.Get(@"$MaxCardinality:"));
                        
            if (MaxCardinality != -1 && MaxCardinality!=GraphUtil.NullInt)
            {
                int cnt=0;

                foreach (IEdge e in baseVertex)
                    if (e.Meta == metaVertex)
                        cnt++;

                if ((cnt + 1) > MaxCardinality)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();

                    v.Value = "Source vertex allready contains $MaxCardinality count of edges of desired meta.";

                    return v;
                }
            }

            int MaxTargetCardinality = GraphUtil.GetIntegerValue(metaVertex.Get(@"$MaxTargetCardinality:"));

            if (MaxTargetCardinality != -1 && MaxTargetCardinality != GraphUtil.NullInt)
            {
                int cnt = 0;

                foreach (IEdge e in toVertex.InEdges)
                    if (e.Meta == metaVertex)
                        cnt++;

                if ((cnt + 1) > MaxCardinality)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();

                    v.Value = "Target vertex allready contains $MaxTargetCardinality count of in edges of desired meta.";

                    return v;
                }
            }

            return null;
        }

        public static IEdge AddEdgeOrVertexByMeta(IVertex baseVertex, IVertex metaVertex, IVertex toVertex, bool showDialog, Point position)
        {
            if (metaVertex.Get(@"$VertexTarget:") != null)
            {                
                IVertex n=VertexOperations.AddInstance(baseVertex,metaVertex);

                IEdge e = new EasyEdge(baseVertex, metaVertex, n);

                n.AddEdge(MinusZero.Instance.Root.Get(@"System\Meta\UML\Vertex\$EdgeTarget"), toVertex);

                if(showDialog)
                    MinusZero.Instance.DefaultShow.EditDialog(e, position);

                return e;
            }
            else
            {
                return baseVertex.AddEdge(metaVertex, toVertex); ;
            }
        }

        public static IVertex AddInstance(IVertex baseVertex,IVertex metaVertex, IVertex edgeVertex){

            IVertex nv = baseVertex.AddVertex(edgeVertex, null);

            nv.AddEdge(MinusZero.Instance.Root.Get(@"System\Meta\UML\Vertex\$Is"), metaVertex);

            IVertex children = metaVertex.GetAll("{$MinCardinality:1}");

            foreach (IEdge child in children)
            {
                if(child.To.Get("DefaultValue:")!=null)
                    nv.AddEdge(child.To, child.To.Get("DefaultValue:"));
                else
                    nv.AddVertex(child.To, null);
            }

            return nv;
        }

        public static IVertex AddInstance(IVertex baseVertex, IVertex metaVertex)
        {
            return AddInstance(baseVertex, metaVertex, metaVertex);
        }
    }
}
