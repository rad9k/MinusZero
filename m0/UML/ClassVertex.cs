using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;

namespace m0.UML
{
    public class ClassVertex
    {
        public static void AddAllAttributesVertexes(IVertex ObjectVertex){
            IVertex AttributeVertexes = ObjectVertex.GetAll(@"$Is:\Attribute:");

            foreach (IEdge e in AttributeVertexes)
                ObjectVertex.AddVertex(e.To, null);
        }

        public static void AddIsClassAndAllAttributes(IVertex ObjectVertex, IVertex ClassVertex)
        {
            IVertex smuv = MinusZero.Instance.Root.Get(@"System\Meta\UML\Vertex");

            ObjectVertex.AddEdge(smuv.Get("$Is"), ClassVertex);

            AddAllAttributesVertexes(ObjectVertex);
        }
    }
}
