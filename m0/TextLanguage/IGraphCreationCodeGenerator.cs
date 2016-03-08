using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;

namespace m0.TextLanguage
{
    public interface IGraphCreationCodeGenerator
    {
        string GraphCreationCodeGenerateAsString(IVertex graphRoot);

        void GraphCreationCodeGenerate(IVertex graphRoot, IVertex generationRoot);

    }
}
