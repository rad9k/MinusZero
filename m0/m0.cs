using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Store;
using m0.Graph;
using m0.TextLanguage;
using m0.Store.FileSystem;
using m0.Util;

namespace m0
{
    public class MinusZero:IStoreUniverse, IDisposable
    {
        public static MinusZero Instance=new MinusZero();

        public bool IsInitialized = false;

        public AccessLevelEnum[] GetStoreDefaultAccessLevelList=new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions };

        
        IList<IStore> stores=new List<IStore>();

        public IList<IStore> Stores { get { return stores; } }


        IStore tempstore;

        public IStore TempStore { get { return tempstore; } }

        
        IVertex root;

        public IVertex Root { get { return root; } }

        
        IVertex metaEmpty;

        public IVertex MetaEmpty { get { return metaEmpty; } }

        IVertex empty;

        public IVertex Empty { get { return empty; } }

        IShow _DefaultShow;

        public IShow DefaultShow { get { return _DefaultShow; } }
        
        IParser _DefaultParser;

        public IParser DefaultParser { get { return _DefaultParser; } }


        IExecuter _DefaultExecuter;

        public IExecuter DefaultExecuter { get { return _DefaultExecuter; } }


        IVertex _DefaultLanguageMetaDefinition;

        public IVertex DefaultLanguageMetaDefinition { get { return _DefaultLanguageMetaDefinition; } }


        IVertex _MetaTextLanguageParsedTreeVertex;

        public IVertex MetaTextLanguageParsedTreeVertex {get {return _MetaTextLanguageParsedTreeVertex; } }

        
        IGraphCreationCodeGenerator _DefaultGraphCreationCodeGenerator;

        public IGraphCreationCodeGenerator DefaultGraphCreationCodeGenerator  { get { return _DefaultGraphCreationCodeGenerator; } }

        public bool IsGUIDragging { get; set; }


        public IVertex CreateTempVertex()
        {
            return new EasyVertex(this.tempstore);
        }

        void Bootstrap()
        {
            IStore rootstore = new MemoryStore("$-0$ROOT$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions });

            root = rootstore.Root;


            Stores.Clear();

            Stores.Add(rootstore);


            tempstore = new MemoryStore("$-0$TEMP$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions },true);


            metaEmpty = new IdentifiedVertex("$Empty", rootstore);

            MetaEmpty.Value = "$Empty";

            empty = new IdentifiedVertex("$EmptyVertex", rootstore);

            empty.Value = "";
        }

        void Init(){
            ZeroCode.ZeroCodeEngine zeroCodeEngine = new ZeroCode.ZeroCodeEngine();

            _DefaultShow = m0Main.Instance;

            _DefaultParser = zeroCodeEngine;

            _DefaultExecuter = zeroCodeEngine;

            _DefaultGraphCreationCodeGenerator = zeroCodeEngine;            
        }


        void CreateSystem()
        {
            IVertex system=Root.AddVertex(null, "System");

            // turned off for now
            // system.AddVertex(null,"Session").AddVertex(null,"Visualisers");

            IVertex meta = system.AddVertex(null, "Meta");
            
            IVertex tl=system.AddVertex(null, "TextLanguage");

            IVertex mtl = meta.AddVertex(null, "TextLanguage");

            IVertex sto = meta.AddVertex(null, "Store");


            // Meta\TextLanguage\Parser

            IVertex mtp=mtl.AddVertex(null,"Parser");

            IVertex ptmd=mtp.AddVertex(null, "PreviousTerminalMoveDown");

            IVertex mdtpnltoce = mtp.AddVertex(null, "MoveDownToPreviousContainerTerminalOrCretedEmpty");

            IVertex ct = mtp.AddVertex(null, "ContainerTerminal");


            // Meta\TextLanguage\ParsedTree

            IVertex mtpt = mtl.AddVertex(null,"ParsedTree");

            _MetaTextLanguageParsedTreeVertex = mtpt;

            IVertex empty=mtpt.AddVertex(null, "$EmptyContainerTerminal");
            empty.AddVertex(ct, null);


            // TextLanguage\ZeroCode

            IVertex zc=tl.AddVertex(null, "ZeroCode");

            _DefaultLanguageMetaDefinition = zc;
                       
            zc.AddVertex(null, ",");

            IVertex colon=zc.AddVertex(null, ":");
                colon.AddVertex(ptmd,1);
                colon.AddVertex(ct,null);

            zc.AddVertex(null, "\\");
            zc.AddVertex(null, "*");
            zc.AddVertex(null, "{").AddVertex(mdtpnltoce,null);
            zc.AddVertex(null, "}").AddVertex(mdtpnltoce,null);
            zc.AddVertex(null, "=");
            zc.AddVertex(null, "!=");
        }

        void CreateSystemMeta()
        {
            GeneralUtil.ParseAndExcute(Root.Get(@"System\Meta"), null, "{}");
        }

        void CreatePresentation()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Presentation{$Hide}}");
        }

        void CreateSystemMetaUml()
        {
            IVertex sm=Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, null, "{UML{Type{DefaultViewVisualiser,DefaultEditVisualiser,DefaultOpenVisualiser},Vertex{$Inherits,$Is,$EdgeTarget,$VertexTarget,$MinCardinality,$MaxCardinality,$MinTargetCardinality,$MaxTargetCardinality,$Group,$Section,Description,Comment,Author,Dependency},AtomType,Enum{EnumValue},Selector,Class{Attribute{MinValue,MaxValue,DefaultValue},Association,Aggregation,$PlatformClassName}}}");

            GeneralUtil.ParseAndExcute(sm.Get(@"UML\Selector"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            GeneralUtil.ParseAndExcute(sm.Get(@"UML\Enum\EnumValue"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");            

            sm.Get(@"UML\Vertex\$Is").AddVertex(sm.Get(@"Presentation\$Hide"),"");

            sm.Get(@"UML\Class").AddEdge(sm.Get("*$Inherits"), sm.Get(@"UML\Type"));

            sm.Get(@"UML\Class").AddEdge(null,sm.Get("*$Inherits"));
            
           // Root.Get(@"System\Meta\UML\Class\Attribute\OfType").AddEdge(null, sm.Get(@"UML\Type")); = $EdgeTarget
            //Root.Get(@"System\Meta\UML\Class\Attribute\SetOfType").AddEdge(null, sm.Get(@"UML\Type")); = $VertexTarget

            Root.Get(@"System\Meta\UML\Class\Attribute").AddEdge(sm.Get(@"*$Inherits"), sm.Get(@"UML\Selector"));
            Root.Get(@"System\Meta\UML\Class\Attribute").AddEdge(sm.Get(@"*$VertexTarget"), sm.Get(@"UML\Type"));

            Root.Get(@"System\Meta\UML\Class\Association").AddEdge(sm.Get(@"*$Inherits"), sm.Get(@"UML\Selector"));
            Root.Get(@"System\Meta\UML\Class\Association").AddEdge(sm.Get(@"*$VertexTarget"), sm.Get(@"UML\Class"));

            Root.Get(@"System\Meta\UML\Class\Aggregation").AddEdge(sm.Get(@"*$Inherits"), sm.Get(@"UML\Selector"));
            Root.Get(@"System\Meta\UML\Class\Aggregation").AddEdge(sm.Get(@"*$VertexTarget"), sm.Get(@"UML\Class"));


           // sm.Get(@"UML\Type").AddEdge(sm.Get("*$Inherits"),sm.Get(@"UML\Vertex"));    // do not want it at last for now        

            sm.Get(@"UML\AtomType").AddEdge(sm.Get("*$Inherits"),sm.Get(@"UML\Type"));
            sm.Get(@"UML\Enum").AddEdge(sm.Get("*$Inherits"),sm.Get(@"UML\Type"));            
        }

        void CreateSystemMetaZeroTypes()
        {
            IVertex sm = Root.Get(@"System\Meta");

            
            GeneralUtil.ParseAndExcute(sm, sm, "{ZeroTypes{AtomType:String,AtomType:Integer,AtomType:Decimal,AtomType:Float,AtomType:Boolean,Vertex:Vertex,Class:Edge{Attribute:From{$MinCardinality:0,$MaxCardinality:1},Attribute:Meta{$MinCardinality:1,$MaxCardinality:1},Attribute:To{$MinCardinality:1,$MaxCardinality:1}},Class:DateTime{Attribute:Year{$MinCardinality:1,$MaxCardinality:1},Attribute:Month{$MinCardinality:1,$MaxCardinality:1},Attribute:Day{$MinCardinality:1,$MaxCardinality:1},Attribute:Hour{$MinCardinality:1,$MaxCardinality:1},Attribute:Minute{$MinCardinality:1,$MaxCardinality:1},Attribute:Second{$MinCardinality:1,$MaxCardinality:1},Attribute:Millisecond{$MinCardinality:0,$MaxCardinality:1}},Enum:EnumBase,Class:$PlatformClass,Class:HasBaseEdge{Attribute:BaseEdge{$MinCardinality:1,$MaxCardinality:1}},Class:HasSelectedEdges{Attribute:SelectedEdges{$MinCardinality:1,$MaxCardinality:1}},Class:HasFilter{Attribute:FilterQuery{$MinCardinality:0,$MaxCardinality:1}},Class:Color{Attribute:Red{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Green{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Blue{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Opacity{MinValue:0,MaxValue:255,$MinCardinality:0,$MaxCardinality:1}}}}");

            sm.Get(@"ZeroTypes\String").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));
            sm.Get(@"ZeroTypes\Integer").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));
            sm.Get(@"ZeroTypes\Decimal").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));
            sm.Get(@"ZeroTypes\Float").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));
            sm.Get(@"ZeroTypes\Boolean").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));            
            sm.Get(@"ZeroTypes\Vertex").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Vertex"));            
            sm.Get(@"ZeroTypes\Edge").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\EnumBase").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Enum"));
            sm.Get(@"ZeroTypes\DateTime").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));            
            sm.Get(@"ZeroTypes\HasBaseEdge").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));            
            sm.Get(@"ZeroTypes\HasSelectedEdges").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\HasFilter").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\$PlatformClass").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Color").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));

            sm.Get(@"ZeroTypes\DateTime\Year").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Month").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Day").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Hour").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Minute").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Second").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Millisecond").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));

            sm.Get(@"ZeroTypes\Edge\From").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Vertex"));
            sm.Get(@"ZeroTypes\Edge\Meta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Vertex"));
            sm.Get(@"ZeroTypes\Edge\To").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Vertex"));
           
            sm.Get(@"ZeroTypes\HasBaseEdge\BaseEdge").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Edge"));
            sm.Get(@"ZeroTypes\HasBaseEdge\BaseEdge").AddVertex(sm.Get(@"*$Section"), "Base");

            sm.Get(@"ZeroTypes\HasSelectedEdges\SelectedEdges").AddEdge(sm.Get(@"*$VertexTarget"), sm.Get(@"ZeroTypes\Edge"));
            sm.Get(@"ZeroTypes\HasFilter\FilterQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));

            sm.Get(@"ZeroTypes\Color\Red").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\Color\Green").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\Color\Blue").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\Color\Opacity").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));

        }

        void CreateSystemMetaVisualiserDiagram()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Visualiser}");

            IVertex smv = Root.Get(@"System\Meta\Visualiser");

            GeneralUtil.ParseAndExcute(smv, sm, "{DiagramInternal{Class:DiagramItemBase{Attribute:Definition{$MinCardinality:1,$MaxCardinality:1},Attribute:PositionX{$MinCardinality:1,$MaxCardinality:1},Attribute:PositionY{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeX{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeY{$MinCardinality:1,$MaxCardinality:1},Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:DiagramLine{$MinCardinality:0,$MaxCardinality:-1},OptionEdge,OptionDiagramLineDefinition},Class:DiagramItemDefinition{Attribute:DirectVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Attribute:MetaVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Attribute:DiagramItemClass{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramItemVertex{$MinCardinality:0,$MaxCardinality:1},Attribute:InstanceCreation{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramLineDefinition{$MinCardinality:0,$MaxCardinality:-1},Attribute:DoNotShowInherited{$MinCardinality:0,$MaxCardinality:1}},Enum:InstanceCreationEnum{EnumValue:Instance,EnumValue:InstanceAndDirect,EnumValue:Direct},Class:DiagramLineBase{Attribute:Definition{$MinCardinality:1,$MaxCardinality:1},Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:ToDiagramItem{$MinCardinality:1,$MaxCardinality:1}},Class:DiagramLineDefinition{Attribute:EdgeTestQuery{$MinCardinality:1,$MaxCardinality:1},Attribute:ToDiagramItemTestQuery{$MinCardinality:0,$MaxCardinality:1},Attribute:DiagramLineClass{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramLineVertex{$MinCardinality:0,$MaxCardinality:1}},Class:DiagramImageItem{Attribute:Filename},Class:DiagramOvalItem,Class:DiagramRhombusItem,Class:DiagramRectangleItem{Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:RoundEdgeSize{MinValue:1,MaxValue:200,$MinCardinality:0,$MaxCardinality:1},Attribute:VisualiserClass{$MinCardinality:0,$MaxCardinality:1},Attribute:VisualiserVertex{$MinCardinality:0,$MaxCardinality:1}},Class:DiagramLine{Attribute:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}},Enum:LineEndEnum{EnumValue:Straight,EnumValue:Arrow,EnumValue:Triangle,EnumValue:FilledTriangle,EnumValue:Diamond,EnumValue:FilledDiamond},Class:DiagramMetaExtendedLine{Attribute:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}}}}");

            smv.Get(@"DiagramInternal\InstanceCreationEnum").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\EnumBase"));
            smv.Get(@"DiagramInternal\LineEndEnum").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\EnumBase"));

            smv.Get(@"DiagramInternal\DiagramItemBase").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramItemBase").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            smv.Get(@"DiagramInternal\DiagramItemBase").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            
            smv.Get(@"DiagramInternal\DiagramItemBase\Definition").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramItemDefinition"));
            IVertex definitionSection = smv.Get(@"DiagramInternal\DiagramItemBase\Definition").AddVertex(sm.Get(@"*$Section"), "Definition");
            
            smv.Get(@"DiagramInternal\DiagramItemBase\PositionX").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            IVertex positionAndSizeSection = smv.Get(@"DiagramInternal\DiagramItemBase\PositionX").AddVertex(sm.Get(@"*$Section"), "Position and size");

            smv.Get(@"DiagramInternal\DiagramItemBase\PositionY").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            smv.Get(@"DiagramInternal\DiagramItemBase\PositionY").AddEdge(sm.Get(@"*$Section"), positionAndSizeSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\SizeX").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            smv.Get(@"DiagramInternal\DiagramItemBase\SizeX").AddEdge(sm.Get(@"*$Section"), positionAndSizeSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\SizeY").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            smv.Get(@"DiagramInternal\DiagramItemBase\SizeY").AddEdge(sm.Get(@"*$Section"), positionAndSizeSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\LineWidth").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            IVertex lookSection = smv.Get(@"DiagramInternal\DiagramItemBase\LineWidth").AddVertex(sm.Get(@"*$Section"), "Look");

            smv.Get(@"DiagramInternal\DiagramItemBase\BackgroundColor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Color"));
            smv.Get(@"DiagramInternal\DiagramItemBase\BackgroundColor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\ForegroundColor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Color"));
            smv.Get(@"DiagramInternal\DiagramItemBase\ForegroundColor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\DiagramLine").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramLineBase"));
            smv.Get(@"DiagramInternal\DiagramItemBase\DiagramLine").AddEdge(sm.Get(@"*$Section"), sm.Get(@"ZeroTypes\HasBaseEdge\BaseEdge\$Section:"));
            


            smv.Get(@"DiagramInternal\DiagramItemDefinition").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DirectVertexTestQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\MetaVertexTestQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DiagramItemClass").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DiagramItemVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Vertex"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\InstanceCreation").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\InstanceCreationEnum"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DiagramLineDefinition").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramLineDefinition"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DoNotShowInherited").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));

            smv.Get(@"DiagramInternal\DiagramLineBase").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramLineBase").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            smv.Get(@"DiagramInternal\DiagramLineBase").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));

            smv.Get(@"DiagramInternal\DiagramLineBase\Definition").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramLineDefinition"));
            smv.Get(@"DiagramInternal\DiagramLineBase\Definition").AddEdge(sm.Get(@"*$Section"), definitionSection);
            
            smv.Get(@"DiagramInternal\DiagramLineBase\LineWidth").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            smv.Get(@"DiagramInternal\DiagramLineBase\LineWidth").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramLineBase\BackgroundColor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Color"));
            smv.Get(@"DiagramInternal\DiagramLineBase\BackgroundColor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramLineBase\ForegroundColor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Color"));
            smv.Get(@"DiagramInternal\DiagramLineBase\ForegroundColor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramLineBase\ToDiagramItem").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramLineBase\ToDiagramItem").AddEdge(sm.Get(@"*$Section"), sm.Get(@"ZeroTypes\HasBaseEdge\BaseEdge\$Section:"));

            smv.Get(@"DiagramInternal\DiagramLineDefinition").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\EdgeTestQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\ToDiagramItemTestQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\DiagramLineClass").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramLineBase"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\DiagramLineVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Vertex"));

            smv.Get(@"DiagramInternal\DiagramImageItem").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramImageItem").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramImageItem").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramImageItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            smv.Get(@"DiagramInternal\DiagramImageItem\Filename").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));

            smv.Get(@"DiagramInternal\DiagramOvalItem").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramOvalItem").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramOvalItem").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramOvalItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(@"DiagramInternal\DiagramRhombusItem").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramRhombusItem").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramRhombusItem").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramRhombusItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            
            
            smv.Get(@"DiagramInternal\DiagramRectangleItem").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramRectangleItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            smv.Get(@"DiagramInternal\DiagramRectangleItem\VisualiserClass").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"UML\Class"));
            IVertex visualiserSection = smv.Get(@"DiagramInternal\DiagramRectangleItem\VisualiserClass").AddVertex(sm.Get(@"*$Section"), "Visualiser");
            
            smv.Get(@"DiagramInternal\DiagramRectangleItem\VisualiserVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Vertex"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem\VisualiserVertex").AddEdge(sm.Get(@"*$Section"), visualiserSection);
            
            
            smv.Get(@"DiagramInternal\DiagramRectangleItem\RoundEdgeSize").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem\RoundEdgeSize").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramRectangleItem\ShowMeta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem\ShowMeta").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramLine").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramLine").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramLineBase"));
            smv.Get(@"DiagramInternal\DiagramLine").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramLine, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            smv.Get(@"DiagramInternal\DiagramLine\StartAnchor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*LineEndEnum"));
            smv.Get(@"DiagramInternal\DiagramLine\StartAnchor").AddEdge(sm.Get(@"*$Section"), lookSection);
            
            smv.Get(@"DiagramInternal\DiagramLine\EndAnchor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*LineEndEnum"));
            smv.Get(@"DiagramInternal\DiagramLine\EndAnchor").AddEdge(sm.Get(@"*$Section"), lookSection);
            
            smv.Get(@"DiagramInternal\DiagramLine\IsDashed").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*Boolean"));
            smv.Get(@"DiagramInternal\DiagramLine\IsDashed").AddEdge(sm.Get(@"*$Section"), lookSection);


            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramLineBase"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramMetaExtendedLine, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\StartAnchor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*LineEndEnum"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\StartAnchor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\EndAnchor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*LineEndEnum"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\EndAnchor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\IsDashed").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*Boolean"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\IsDashed").AddEdge(sm.Get(@"*$Section"), lookSection);
        }

        void CreateSystemMetaVisualiser()
        {
            IVertex sm = Root.Get(@"System\Meta");

            IVertex smv = Root.Get(@"System\Meta\Visualiser");

            GeneralUtil.ParseAndExcute(smv, sm, "{Enum:GridStyleEnum{EnumValue:None,EnumValue:Vertical,EnumValue:Horizontal,EnumValue:All,EnumValue:AllAndRound,EnumValue:Round},Class:FormVisualiser{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:ColumnNumber{$MinCardinality:0,$MaxCardinality:1},Attribute:MetaOnLeft{$MinCardinality:0,$MaxCardinality:1},Attribute:SectionsAsTabs{$MinCardinality:0,$MaxCardinality:1},Attribute:TableVisualiserVertex{$MinCardinality:0,$MaxCardinality:1}},Class:TableVisualiser{Attribute:ToShowEdgesMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Attribute:GridStyle{$MinCardinality:1,$MaxCardinality:1},Attribute:AlternatingRows{$MinCardinality:1,$MaxCardinality:1}},Class:TableFastVisualiser{Attribute:ToShowEdgesMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Attribute:GridStyle{$MinCardinality:1,$MaxCardinality:1},Attribute:AlternatingRows{$MinCardinality:1,$MaxCardinality:1}},Class:TreeVisualiser{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1}},Class:GraphVisualiser{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:VisualiserCircleSize{$MinCardinality:1,$MaxCardinality:1},Attribute:NumberOfCircles{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowInEdges{$MinCardinality:1,$MaxCardinality:1},Attribute:FastMode{$MinCardinality:1,$MaxCardinality:1},Attribute:MetaLabels{$MinCardinality:1,$MaxCardinality:1}},Class:ClassVisualiser,Class:StringVisualiser,Class:StringViewVisualiser,Class:VertexVisualiser,Class:EdgeVisualiser,Class:IntegerVisualiser,Class:DecimalVisualiser,Class:FloatVisualiser,Class:BooleanVisualiser,Class:EnumVisualiser,Class:Diagram{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1,MinValue:0,MaxValue:200,DefaultValue:100},Attribute:SizeX{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeY{$MinCardinality:1,$MaxCardinality:1},Attribute:Item{$MinCardinality:0,$MaxCardinality:-1,$Hide:},Attribute:CreationPool{$MinCardinality:1,$MaxCardinality:1}},Class:WrapVisualiser,Class:ListVisualiser{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:IsMetaRightAlign{$MinCardinality:1,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Attribute:GridStyle{$MinCardinality:1,$MaxCardinality:1}}}");

            sm.Get(@"Visualiser\GridStyleEnum").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\EnumBase"));

            sm.Get(@"Visualiser\FormVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\FormVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\FormVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.FormVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\FormVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\FormVisualiser\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\FormVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\FormVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\FormVisualiser\ColumnNumber").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\FormVisualiser\ColumnNumber").AddVertex(sm.Get(@"*MinValue"), 1);
            sm.Get(@"Visualiser\FormVisualiser\ColumnNumber").AddVertex(sm.Get(@"*MaxValue"), 8);
            sm.Get(@"Visualiser\FormVisualiser\MetaOnLeft").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\FormVisualiser\SectionsAsTabs").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\FormVisualiser\TableVisualiserVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Vertex"));
            sm.Get(@"UML\Class").AddEdge(sm.Get("UML*DefaultOpenVisualiser"), sm.Get(@"Visualiser\FormVisualiser"));
            
            sm.Get(@"Visualiser\WrapVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\WrapVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\WrapVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.WrapVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\WrapVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));

            sm.Get(@"Visualiser\ListVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\ListVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\ListVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\ListVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasFilter"));
            sm.Get(@"Visualiser\ListVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.ListVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\ListVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\ListVisualiser\ShowHeader").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\ListVisualiser\GridStyle").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Visualiser\GridStyleEnum"));
            sm.Get(@"Visualiser\ListVisualiser\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\ListVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\ListVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\ListVisualiser\IsMetaRightAlign").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\ListVisualiser\IsAllVisualisersEdit").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            
            sm.Get(@"Visualiser\TableVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\TableVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\TableVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\TableVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasFilter"));
            sm.Get(@"Visualiser\TableVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.TableVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\TableVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\TableVisualiser\ToShowEdgesMeta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Edge"));
            sm.Get(@"Visualiser\TableVisualiser\ShowHeader").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\TableVisualiser\GridStyle").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Visualiser\GridStyleEnum"));
            sm.Get(@"Visualiser\TableVisualiser\AlternatingRows").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\TableVisualiser\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\TableVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\TableVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\TableVisualiser\IsAllVisualisersEdit").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));

            sm.Get(@"Visualiser\TableFastVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\TableFastVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\TableFastVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\TableFastVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasFilter"));
            sm.Get(@"Visualiser\TableFastVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.TableFastVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\TableFastVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\TableFastVisualiser\ToShowEdgesMeta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Edge"));
            sm.Get(@"Visualiser\TableFastVisualiser\ShowHeader").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\TableFastVisualiser\GridStyle").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Visualiser\GridStyleEnum"));
            sm.Get(@"Visualiser\TableFastVisualiser\AlternatingRows").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\TableFastVisualiser\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\TableFastVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\TableFastVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\TableFastVisualiser\IsAllVisualisersEdit").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
           

            sm.Get(@"Visualiser\TreeVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\TreeVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\TreeVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\TreeVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.TreeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\TreeVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\TreeVisualiser\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\TreeVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\TreeVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);


            sm.Get(@"Visualiser\GraphVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\GraphVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\GraphVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));            
            sm.Get(@"Visualiser\GraphVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.GraphVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\GraphVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\GraphVisualiser\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\GraphVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\GraphVisualiser\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\GraphVisualiser\VisualiserCircleSize").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\GraphVisualiser\VisualiserCircleSize").AddVertex(sm.Get(@"*MinValue"), 50);
            sm.Get(@"Visualiser\GraphVisualiser\VisualiserCircleSize").AddVertex(sm.Get(@"*MaxValue"), 500);
            sm.Get(@"Visualiser\GraphVisualiser\NumberOfCircles").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\GraphVisualiser\NumberOfCircles").AddVertex(sm.Get(@"*MinValue"), 1);
            sm.Get(@"Visualiser\GraphVisualiser\NumberOfCircles").AddVertex(sm.Get(@"*MaxValue"), 10);
            sm.Get(@"Visualiser\GraphVisualiser\ShowInEdges").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\GraphVisualiser\FastMode").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\GraphVisualiser\MetaLabels").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));

            sm.Get(@"Visualiser\ClassVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\ClassVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\ClassVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.ClassVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\ClassVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));

            sm.Get(@"Visualiser\StringVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\StringVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\StringVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.StringVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\StringVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\String").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\StringVisualiser"));

            sm.Get(@"Visualiser\StringViewVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\StringViewVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\StringViewVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.StringViewVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\StringViewVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\String").AddEdge(sm.Get("UML*DefaultViewVisualiser"), sm.Get(@"Visualiser\StringViewVisualiser"));            
            

            sm.Get(@"Visualiser\VertexVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\VertexVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\VertexVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.VertexVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\VertexVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Vertex").AddEdge(sm.Get("UML*DefaultViewVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));
            sm.Get(@"ZeroTypes\Vertex").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));
            sm.Get(@"UML\Vertex\$EdgeTarget").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));
            sm.Get(@"UML\Vertex\$EdgeTarget").AddEdge(sm.Get("UML*DefaultViewVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));
            sm.Get(@"UML\Vertex\$VertexTarget").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));
            sm.Get(@"UML\Vertex\$VertexTarget").AddEdge(sm.Get("UML*DefaultViewVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));
            //sm.Get(@"UML\Class").AddEdge(sm.Get("UML*DefaultViewVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));
            sm.Get(@"UML\Class").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));

            sm.Get(@"Visualiser\EdgeVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\EdgeVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\EdgeVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.EdgeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\EdgeVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Edge").AddEdge(sm.Get("UML*DefaultViewVisualiser"), sm.Get(@"Visualiser\EdgeVisualiser"));
            sm.Get(@"ZeroTypes\Edge").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\EdgeVisualiser"));

            sm.Get(@"Visualiser\IntegerVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\IntegerVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));                        
            sm.Get(@"Visualiser\IntegerVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.IntegerVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\IntegerVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Integer").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\IntegerVisualiser"));

            sm.Get(@"Visualiser\DecimalVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\DecimalVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));            
            sm.Get(@"Visualiser\DecimalVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.DecimalVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\DecimalVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Decimal").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\DecimalVisualiser"));

            sm.Get(@"Visualiser\FloatVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\FloatVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));            
            sm.Get(@"Visualiser\FloatVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.FloatVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\FloatVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Float").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\FloatVisualiser"));

            sm.Get(@"Visualiser\BooleanVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\BooleanVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\BooleanVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.BooleanVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\BooleanVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Boolean").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\BooleanVisualiser"));
            sm.Get(@"ZeroTypes\Boolean").AddEdge(sm.Get("UML*DefaultViewVisualiser"), sm.Get(@"Visualiser\BooleanVisualiser"));

            sm.Get(@"Visualiser\EnumVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\EnumVisualiser").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));                        
            sm.Get(@"Visualiser\EnumVisualiser").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.EnumVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\EnumVisualiser").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\EnumBase").AddEdge(sm.Get("UML*DefaultEditVisualiser"), sm.Get(@"Visualiser\EnumVisualiser"));

            sm.Get(@"Visualiser\Diagram").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Diagram").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\Diagram").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\Diagram").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.Diagram, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Diagram\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));            
            sm.Get(@"Visualiser\Diagram\Item").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*DiagramItemBase"));
            sm.Get(@"Visualiser\Diagram\SizeX").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*Float"));
            sm.Get(@"Visualiser\Diagram\SizeX").AddVertex(sm.Get(@"*DefaultValue"), (double)1000.0);
            sm.Get(@"Visualiser\Diagram\SizeX").AddVertex(sm.Get(@"*MinValue"), (double)0.0);
            sm.Get(@"Visualiser\Diagram\SizeX").AddVertex(sm.Get(@"*MaxValue"), (double)4000.0);
            sm.Get(@"Visualiser\Diagram\SizeY").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*Float"));
            sm.Get(@"Visualiser\Diagram\SizeY").AddVertex(sm.Get(@"*DefaultValue"), (double)1000.0);
            sm.Get(@"Visualiser\Diagram\SizeY").AddVertex(sm.Get(@"*MinValue"), (double)0.0);
            sm.Get(@"Visualiser\Diagram\SizeY").AddVertex(sm.Get(@"*MaxValue"), (double)4000.0);
            sm.Get(@"Visualiser\Diagram\CreationPool").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Vertex"));
            sm.Get(@"Visualiser\Diagram").AddEdge(sm.Get("UML*DefaultOpenVisualiser"), sm.Get(@"Visualiser\Diagram"));            
        }        

        void CreateSystemData(){
            IVertex sm = Root.Get(@"System\Meta");

            IVertex s = Root.Get(@"System");

            GeneralUtil.ParseAndExcute(s,sm,"{Data}");
        }

        void AddDiagramItemDefinition(String Value, String DirectVertexTestQuery, String MetaVertexTestQuery, IVertex DiagramItemClass,  IVertex InstanceCreation)
        {
            IVertex did = Root.Get(@"System\Meta*DiagramItemDefinition");

            IVertex v = Root.Get(@"System\Data\Visualiser\Diagram").AddVertex(did, Value);

            v.AddEdge(Root.Get(@"System\Meta*$Is"), Root.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemDefinition"));

            if(DirectVertexTestQuery!=null)
                v.AddVertex(did.Get("DirectVertexTestQuery"), DirectVertexTestQuery);

            if (MetaVertexTestQuery != null)
                v.AddVertex(did.Get("MetaVertexTestQuery"), MetaVertexTestQuery);

            v.AddEdge(did.Get("DiagramItemClass"), DiagramItemClass);  

            v.AddEdge(did.Get("InstanceCreation"), InstanceCreation);
        }

        void CreateSystemDataVisualiserDiagram()
        {
            IVertex sm = Root.Get(@"System\Meta");

            IVertex sd = Root.Get(@"System\Data");

            IVertex did = Root.Get(@"System\Meta*DiagramItemDefinition");
            IVertex dld = Root.Get(@"System\Meta*DiagramInternal\DiagramLineDefinition");

            GeneralUtil.ParseAndExcute(sd, sm, "{Visualiser{Diagram}}");

            IVertex Instance = sm.Get("*Instance");
            IVertex InstanceAndDirect = sm.Get("*InstanceAndDirect");
            IVertex Direct = sm.Get("*Direct");

            /////////////////////////////////////////////////////////////////////////
            // TEST
            /////////////////////////////////////////////////////////////////////////

            //AddDiagramItemDefinition("Test", @"", null, sm.Get(@"*DiagramRhombusItem"), Direct);
            AddDiagramItemDefinition("Test", @"", null, sm.Get(@"*DiagramImageItem"), Direct);

            IVertex tord = Root.Get(@"System\Data\Visualiser\Diagram\Test");

            IVertex toriv = tord.AddVertex(did.Get("DiagramItemVertex"), null);

           // toriv.AddVertex(Root.Get(@"System\Meta*SizeX"), 150.0);
            //toriv.AddVertex(Root.Get(@"System\Meta*SizeY"), 150.0);

            toriv.AddVertex(Root.Get(@"System\Meta*Filename"), "testimage.gif");

            // Test / Association

            IVertex tcrda = tord.AddVertex(dld, "Association");

            tcrda.AddVertex(dld.Get("EdgeTestQuery"), @"$Is:\");            

            tcrda.AddEdge(dld.Get("DiagramLineClass"), sm.Get(@"*DiagramInternal\DiagramLine"));

            IVertex tordadlv = tcrda.AddVertex(dld.Get("DiagramLineVertex"), null);

            tordadlv.AddEdge(sm.Get("*EndAnchor"), sm.Get(@"*DiagramInternal\LineEndEnum\Arrow"));
            tordadlv.AddEdge(sm.Get("*StartAnchor"), sm.Get(@"*DiagramInternal\LineEndEnum\Arrow"));            

            

            /////////////////////////////////////////////////////////////////////////
            // Object Rectangle
            /////////////////////////////////////////////////////////////////////////

            AddDiagramItemDefinition("Object", @"{$Is:{$Is:Class}}", @"{$Is:Class}", sm.Get(@"*DiagramRectangleItem"),InstanceAndDirect);

            IVertex ord = Root.Get(@"System\Data\Visualiser\Diagram\Object");

            IVertex oriv=ord.AddVertex(did.Get("DiagramItemVertex"), null);

            //oriv.AddVertex(Root.Get(@"System\Meta*RoundEdgeSize"), 20);

            oriv.AddVertex(Root.Get(@"System\Meta*ShowMeta"), "True");

            oriv.AddEdge(Root.Get(@"System\Meta*VisualiserClass"), Root.Get(@"System\Meta*ListVisualiser"));
            
            IVertex orivvv=oriv.AddVertex(Root.Get(@"System\Meta*VisualiserVertex"),null);
            
            orivvv.AddVertex(Root.Get(@"System\Meta*FilterQuery"), "{$Is:Attribute}:");

            orivvv.AddVertex(Root.Get(@"System\Meta*ShowHeader"), "False");

            // Object / Association instance
            
            IVertex orda = ord.AddVertex(dld, "Association instance");

            orda.AddEdge(sm.Get("*$Is"), dld);

            orda.AddVertex(dld.Get("EdgeTestQuery"), @"$Is:{$Is:Class}\Association:");

            orda.AddVertex(dld.Get("ToDiagramItemTestQuery"), @"Definition:Object");

            orda.AddEdge(dld.Get("DiagramLineClass"), sm.Get(@"*DiagramInternal\DiagramLine"));

            IVertex ordadlv=orda.AddVertex(dld.Get("DiagramLineVertex"), null);

            ordadlv.AddEdge(sm.Get("*EndAnchor"), sm.Get(@"*DiagramInternal\LineEndEnum\Arrow"));            
            ordadlv.AddVertex(sm.Get("*IsDashed"), "True");

            // Object / Aggregation instance

            IVertex ordag = ord.AddVertex(dld, "Aggregation instance");

            ordag.AddEdge(sm.Get("*$Is"), dld);

            ordag.AddVertex(dld.Get("EdgeTestQuery"), @"$Is:{$Is:Class}\Aggregation:");

            ordag.AddVertex(dld.Get("ToDiagramItemTestQuery"), @"Definition:Object");

            ordag.AddEdge(dld.Get("DiagramLineClass"), sm.Get(@"*DiagramInternal\DiagramLine"));

            IVertex ordagdlv = ordag.AddVertex(dld.Get("DiagramLineVertex"), null);
            
            ordagdlv.AddEdge(sm.Get("*StartAnchor"), sm.Get(@"*DiagramInternal\LineEndEnum\Diamond"));
            ordagdlv.AddVertex(sm.Get("*IsDashed"), "True");            

            /////////////////////////////////////////////////////////////////////////
            // Class
            /////////////////////////////////////////////////////////////////////////

            AddDiagramItemDefinition("Class", @"{$Is:Class}", "Class", sm.Get(@"*DiagramRectangleItem"), InstanceAndDirect);

            IVertex crd = Root.Get(@"System\Data\Visualiser\Diagram\Class");

            crd.AddVertex(sm.Get(@"*DoNotShowInherited"), "True");

            IVertex civ = crd.AddVertex(did.Get("DiagramItemVertex"), null);            

            civ.AddVertex(Root.Get(@"System\Meta*ShowMeta"), "True");

            civ.AddEdge(Root.Get(@"System\Meta*VisualiserClass"), Root.Get(@"System\Meta*ClassVisualiser"));

            IVertex civvv = civ.AddVertex(Root.Get(@"System\Meta*VisualiserVertex"), null);

            civvv.AddVertex(Root.Get(@"System\Meta*FilterQuery"), "Attribute:");

            civvv.AddVertex(Root.Get(@"System\Meta*ShowHeader"), "False");

            // Class / Association

            IVertex crda = crd.AddVertex(dld, "Association");

            crda.AddVertex(dld.Get("EdgeTestQuery"), @"$Is:Class\Association");

            crda.AddVertex(dld.Get("ToDiagramItemTestQuery"), @"Definition:Class");

            crda.AddEdge(dld.Get("DiagramLineClass"), sm.Get(@"*DiagramInternal\DiagramLine"));

            // Class / Aggregation

            IVertex crdag = crd.AddVertex(dld, "Aggregation");

            crdag.AddVertex(dld.Get("EdgeTestQuery"), @"$Is:Class\Aggregation");

            crdag.AddVertex(dld.Get("ToDiagramItemTestQuery"), @"Definition:Class");

            crdag.AddEdge(dld.Get("DiagramLineClass"), sm.Get(@"*DiagramInternal\DiagramLine"));

            IVertex crdaglv = crdag.AddVertex(dld.Get("DiagramLineVertex"), null);

            crdaglv.AddEdge(sm.Get("*StartAnchor"), sm.Get(@"*DiagramInternal\LineEndEnum\Diamond"));

            // Class / Inheritence

            IVertex crdah = crd.AddVertex(dld, "Inheritence");

            crdah.AddVertex(dld.Get("EdgeTestQuery"), @"$Is:Class\$Inherits");

            crdah.AddVertex(dld.Get("ToDiagramItemTestQuery"), @"Definition:Class");

            crdah.AddEdge(dld.Get("DiagramLineClass"), sm.Get(@"*DiagramInternal\DiagramLine"));

            IVertex crdahlv = crdah.AddVertex(dld.Get("DiagramLineVertex"), null);

            crdahlv.AddEdge(sm.Get("*EndAnchor"), sm.Get(@"*DiagramInternal\LineEndEnum\Triangle"));

            /////////////////////////////////////////////////////////////////////////
            // Vertex 
            /////////////////////////////////////////////////////////////////////////

            AddDiagramItemDefinition("Vertex", @"", null, sm.Get(@"*DiagramRectangleItem"), Direct);

            IVertex vd = Root.Get(@"System\Data\Visualiser\Diagram\Vertex");

            IVertex viv = vd.AddVertex(did.Get("DiagramItemVertex"), null);

            viv.AddVertex(Root.Get(@"System\Meta*ShowMeta"), "False");

            // Vertex / Edge

            IVertex vive = vd.AddVertex(dld, "Edge");

            vive.AddEdge(sm.Get("*$Is"), dld);

            vive.AddVertex(dld.Get("EdgeTestQuery"), @"$Is:\");

            //vive.AddVertex(dld.Get("ToDiagramItemTestQuery"), @"Definition:Vertex");

            vive.AddEdge(dld.Get("DiagramLineClass"), sm.Get(@"*DiagramInternal\DiagramMetaExtendedLine"));

            IVertex vivelv = vive.AddVertex(dld.Get("DiagramLineVertex"), null);

            vivelv.AddEdge(sm.Get("*EndAnchor"), sm.Get(@"*DiagramInternal\LineEndEnum\Arrow"));
        }

        void CreateStoresMeta()
        {
            FileSystemStore.FillSystemMeta();
        }

        void CreateSystemMetaCommands()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Commands{VisualiserClass,SynchronisedVisualiser}}");
        }

        void CreateUserMeta()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{User{CurrentUser,Class:NonAtomProcess{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1}},Class:Session{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1},Aggregation:Process{$MinCardinality:0,$MaxCardinality:-1}},Class:User{Attribute:CurrentSession{$MinCardinality:1,$MaxCardinality:1},Aggregation:Session{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Settings{$MinCardinality:1,$MaxCardinality:1},Association:Queries{$MinCardinality:1,$MaxCardinality:1}},Class:Settings{Attribute:CopyOnDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Attribute:AllowBlankAreaDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Attribute:AllowManyDiagramItemsForOneVertex{$MinCardinality:1,$MaxCardinality:1}},Enum:AllowBlankAreaDragAndDropEnum{EnumValue:No,EnumValue:OnlyEnd,EnumValue:StartAndEnd}}}");

            sm.Get(@"User\NonAtomProcess").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"User\Session").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"User\User").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"User\Settings").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));


            sm.Get(@"User\NonAtomProcess\StartTimeStamp").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\DateTime"));
            sm.Get(@"User\Session\StartTimeStamp").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\DateTime"));
            sm.Get(@"User\Session\Process").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"User\NonAtomProcess")); // to be updated
            sm.Get(@"User\User\Session").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"User\Session"));
            sm.Get(@"User\User\CurrentSession").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"User\Session"));
            sm.Get(@"User\User\Settings").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"User\Settings"));
            sm.Get(@"User\User\Queries").AddEdge(sm.Get(@"*$VertexTarget"), sm.Get(@"ZeroTypes\String"));
            sm.Get(@"User\AllowBlankAreaDragAndDropEnum").AddEdge(sm.Get(@"*$Inherits"), sm.Get(@"ZeroTypes\EnumBase"));

            sm.Get(@"User\Settings\CopyOnDragAndDrop").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"User\Settings\AllowBlankAreaDragAndDrop").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*AllowBlankAreaDragAndDropEnum"));
            sm.Get(@"User\Settings\AllowManyDiagramItemsForOneVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));            
        }

        void CreateUser(IVertex user)
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(user, sm, "{Settings:{CopyOnDragAndDrop:False,AllowManyDiagramItemsForOneVertex:True},Queries:{String:test,String:\"test{test2}\"}}");
            
            user.Get("Settings:").AddEdge(sm.Get("*AllowBlankAreaDragAndDrop"),sm.Get(@"User\AllowBlankAreaDragAndDropEnum\StartAndEnd"));            

            user.AddEdge(sm.Get(@"*$Is"),sm.Get(@"User\User"));
            user.Get("Settings:").AddEdge(sm.Get(@"*$Is"),sm.Get(@"User\Settings"));

            IVertex session = user.AddVertex(sm.Get(@"User\User\Session"), null);
            user.AddEdge(sm.Get(@"User\User\CurrentSession"), session);
        }

        void CreateUsers()
        {
            IVertex sm = Root.Get(@"System\Meta\User");

            GeneralUtil.ParseAndExcute(Root, sm, "{User{User:root,User:wlodek,User:tadek}}");

            foreach (IEdge u in Root.GetAll(@"User\"))
                CreateUser(u.To);

            Root.Get(@"User").AddEdge(Root.Get(@"System\Meta\User\CurrentUser"), Root.Get(@"User\root"));
        }

        void AddDrives()
        {
            string[] drives = System.IO.Directory.GetLogicalDrives();

            IVertex DriveMeta=Root.Get(@"System\Meta\Store\FileSystem\Drive");

            foreach (string str in drives)
            {
                FileSystemStore fss = new FileSystemStore(str, this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions });

                //fss.IncludeFileContent = true;

                Root.AddEdge(DriveMeta,fss.Root);
            }
        }
        
        public MinusZero(){
            //Initialize();
        }       

        private System.IO.StreamWriter logFile;

        public bool DoLog=false;

        public int LogLevel=1;

        private void InitializeLog()
        {
            if (DoLog)
            {
                logFile = new System.IO.StreamWriter("log.txt");
                logFile.AutoFlush = true;

                Log(0,"InitializeLog", "START");
            }
        }

        public void Log(int Level,string Where, string What)
        {
            if(DoLog&&Level<=LogLevel)
                logFile.WriteLine(DateTime.Now.ToLongTimeString()+":"+DateTime.Now.Millisecond+" "+Where+": "+What);
        }

        private void DisposeLog()
        {
            Log(0,"DisposeLog", "STOP");
            logFile.Close();
        }

        private void AddAttributeIsAttribute()
        {
            IVertex attributes = root.GetAll(@"System*Attribute:");
            IVertex ismeta = root.Get(@"System\Meta*$Is");
            IVertex ameta=root.Get(@"System\Meta\UML\Class\Attribute");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);    
        }

        public void Initialize(){
            InitializeLog();            

            Bootstrap();

            CreateSystem();            

            Init();

            CreateSystemMeta();

            CreatePresentation();

            CreateSystemMetaUml();

            CreateSystemMetaZeroTypes();

            CreateSystemMetaVisualiserDiagram();

            CreateSystemMetaVisualiser();            

            CreateSystemData();

            CreateSystemDataVisualiserDiagram();

            CreateStoresMeta();            

            CreateSystemMetaCommands();

            CreateUserMeta();

            CreateUsers();

            AddAttributeIsAttribute();

            AddDrives();            

            UIWpf.UIWpf.InitializeUIWpf();

            IsInitialized = true;
        }

        public void Dispose()
        {
           // DisposeLog();
        }

        public void Refresh()
        {
            List<string[]> StoresPersistency=new List<string[]>();

            foreach (IStore s in Stores)
            {
                string[] e=new string[2];

                e[0] = s.TypeName;
                e[1] = s.Identifier;

                StoresPersistency.Add(e);

                s.Close();
            }

            Bootstrap();

            foreach (string[] e in StoresPersistency)
            {
                GetStore(e[0], e[1]);
            }
            
        }

        public void BeginTransaction()
        {
            foreach (ITransactionRoot r in Stores)
                r.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            foreach (IStore s in Stores)
                s.Detach();

            foreach (IStore s in Stores)
                s.RollbackTransaction();

            foreach (IStore s in Stores)
                s.Attach();
        }

        public void CommitTransaction()
        {
            foreach (IStore s in Stores)
                s.Detach();

            foreach (IStore s in Stores)
                s.CommitTransaction();

            foreach (IStore s in Stores)
                s.Attach();
        }


        public IStore GetStore(string StoreTypeName, string StoreIdentifier)
        {
            IStore store = Stores.Where(s => s.TypeName == StoreTypeName & s.Identifier == StoreIdentifier).FirstOrDefault();

            if(store!=null)
                return store;

            store=(IStore)Activator.CreateInstance(Type.GetType(StoreTypeName), new object[] { StoreIdentifier, this, GetStoreDefaultAccessLevelList });

            //Stores.Add(store);
            // store's constructor does this

            return store;
        }
    }
}
