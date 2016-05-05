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

using m0.UIWpf;
using m0.Graph;

using Xceed.Wpf.AvalonDock.Layout;
using m0.Foundation;

using m0.UIWpf.Visualisers;
using m0.ZeroTypes;
using m0.Util;
using Xceed.Wpf.AvalonDock.Controls;
using m0.UIWpf.Dialog;

namespace m0
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class m0Main : Window, IShow
    {
        public static m0Main Instance;        

        public m0Main()
        {
            Instance = this;

            InitializeComponent();

            MinusZero.Instance.Initialize();


            CreateTestData();


            TreeVisualiser stv = new TreeVisualiser();

            GraphUtil.ReplaceEdge(stv.Vertex.Get("BaseEdge:"), "To", MinusZero.Instance.Root);                                    
            
            this.root.Content=stv;
                   
           // test();
            
            //Window1 w = new Window1();
            //w.Show();
        }

        void test3()
        {            
            IVertex r=MinusZero.Instance.Root;

            IVertex res = r.GetAll("\"C:\"\\");

            IPlatformClass stv = (IPlatformClass)PlatformClass.CreatePlatformObject(r.Get(@"System\Meta\Visualiser\GraphVisualiser"));

            //GraphUtil.ReplaceEdge(stv.Vertex, "BaseVertex", r.Get("\"C:\""));
            //GraphUtil.ReplaceEdge(stv.Vertex, "BaseVertex", r.Get("\"C:\"\\Windows"));
            //GraphUtil.ReplaceEdge(stv.Vertex, "BaseVertex", r.Get(@"TEST\Person"));
            GraphUtil.ReplaceEdge(stv.Vertex, "BaseVertex", r);


            

            GraphUtil.ReplaceEdge(stv.Vertex, "SelectedVertexes", res);

            MinusZero.Instance.DefaultShow.ShowContent(stv);

            /////////////////////


            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(r.Get(@"System\Meta\Visualiser\TreeVisualiser"));

            GraphUtil.ReplaceEdge(sv.Vertex, "BaseVertex", r);

            GraphUtil.ReplaceEdge(sv.Vertex, "SelectedVertexes", res);

            MinusZero.Instance.DefaultShow.ShowContent(sv);
            
        }

        void test2()
        {
            IVertex r=MinusZero.Instance.Root;

            IPlatformClass stv = (IPlatformClass)PlatformClass.CreatePlatformObject(r.Get(@"System\Meta\Visualiser\TreeVisualiser"));

            GraphUtil.ReplaceEdge(stv.Vertex, "BaseVertex", r);

            MinusZero.Instance.DefaultShow.ShowContent(stv);            
        }

        void test(){
            /*ListVisualiser slv = new ListVisualiser();
            
            GraphUtil.ReplaceEdge(slv.Vertex, "BaseVertex", MinusZero.Instance.Root.Get(@"TEST\Person1"));

            MinusZero.Instance.DefaultShow.ShowContent(slv);
            */
            

        }

        private string randomChars()
        {
            Random r = new Random();
            int x=r.Next(5);

            string xxx = "";

            for(int xx=0;xx<x;xx++)
                xxx+=xx;

            return xxx;
        }

        private void CreateTestData()
        {
            IVertex r=MinusZero.Instance.Root;

            GeneralUtil.ParseAndExcute(r, r.Get(@"System\Meta"), @"{TEST3{Class:Customer{},Class:Person{Attribute:Name,Attribute:Surname,Attribute:DateOfBirth},Class:Company{Attribute:Name,Attribute:RegistrationNumber,},Class:Adress{Attribute:Line 1,Attribute:Line 2,Attribute:Line 3,Attribute:City,Attribute:County,Attribute:Postal code,Attribute:Country},Class:Basket{Attribute:Creation date,Attribute:Status},Class:Item{Attribute:Name,Attribute:Description,Attribute:Price}}}");

            r.Get(@"TEST3\Customer").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Person").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Company").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Adress").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Basket").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));
            r.Get(@"TEST3\Item").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));

            GeneralUtil.ParseAndExcute(r, r.Get(@"System\Meta"), "{TEST2,TEST{Class:Person{Association:Spouse{$MaxCardinality:1,$MaxTargetCardinality:1},Aggregation:Child{$MaxCardinality:3},Attribute:Name,Attribute:Surname,Attribute:Age{MinValue:0,MaxValue:40},Attribute:NoseLength{MinValue:0,MaxValue:40},Attribute:Money{MinValue:0,MaxValue:1000},Attribute:IsGood,Attribute:IsPretty,Attribute:IsPretty2,Attribute:IsPretty3},Enum:Pretty{EnumValue:Yes,EnumValue:No,EnumValue:Maybe}}}");

            r.Get(@"TEST\Pretty").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"System\Meta\ZeroTypes\EnumBase"));
            r.Get(@"TEST\Person").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\UML\Class"));

            IVertex smzt=r.Get(@"System\Meta\ZeroTypes");

            IVertex EdgeTarget = r.Get(@"System\Meta*$EdgeTarget");

            IVertex Person = r.Get(@"TEST\Person");

            Person.Get("Name").AddEdge(EdgeTarget, smzt.Get("String"));

            Person.Get("Spouse").AddEdge(r.Get(@"System\Meta*$EdgeTarget"), Person);
            Person.Get("Child").AddEdge(r.Get(@"System\Meta*$EdgeTarget"), Person);

            Person.Get("Surname").AddEdge(EdgeTarget, smzt.Get("String"));
            Person.Get("Age").AddEdge(EdgeTarget, smzt.Get("Integer"));
            Person.Get("NoseLength").AddEdge(EdgeTarget, smzt.Get("Float"));
            Person.Get("Money").AddEdge(EdgeTarget, smzt.Get("Decimal"));
            Person.Get("IsGood").AddEdge(EdgeTarget, smzt.Get("Boolean"));
            Person.Get("IsPretty").AddEdge(EdgeTarget, r.Get(@"TEST\Pretty"));
            Person.Get("IsPretty2").AddEdge(EdgeTarget, r.Get(@"TEST\Pretty"));
            Person.Get("IsPretty3").AddEdge(EdgeTarget, r.Get(@"TEST\Pretty"));

            GeneralUtil.ParseAndExcute(r.Get("TEST"), r.Get(@"TEST"), "{Person:Person1{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person2{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");

            GeneralUtil.ParseAndExcute(r.Get("TEST"), r.Get(@"TEST"), "{Person:Person3{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person4{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");
            
            r.Get(@"TEST\Person1").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\Person2").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\Person3").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\Person4").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));

            GraphUtil.ReplaceEdge(r.Get(@"TEST\Person1"), "IsPretty", r.Get(@"TEST\Pretty\No"));
            GraphUtil.ReplaceEdge(r.Get(@"TEST\Person2"), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
            GraphUtil.ReplaceEdge(r.Get(@"TEST\Person3"), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
            GraphUtil.ReplaceEdge(r.Get(@"TEST\Person4"), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
                

            for (int x = 0; x < 100; x++)
            {
                GeneralUtil.ParseAndExcute(r.Get("TEST2"), r.Get(@"TEST"), "{Person:Person1"+x+"{Name:Radek,Surname:Tereszczuk,Age:34,NoseLength:\"2,3\",Money:999,IsGood:False,IsPretty:},Person:Person2"+x+"{Name:Maurycy,Surname:Tereszczuk,Age:1,NoseLength:1.1,Money:9999,IsGood:True,IsPretty:}}");
                
              
                GeneralUtil.ParseAndExcute(r.Get("TEST2"), r.Get(@"TEST"), "{Person:Person3"+x+"{Name:Magda,Surname:Tereszczuk,Age:18,NoseLength:\"2,1\",Money:999,IsGood:True,IsPretty:},Person:Person4"+x+"{Name:Jan,Surname:Kuciak,Age:10,NoseLength:0.6,Money:99999,IsGood:True,IsPretty:}}");

                GraphUtil.ReplaceEdge(r.Get(@"TEST2\Person1"+x), "IsPretty", r.Get(@"TEST\Pretty\No"));
                GraphUtil.ReplaceEdge(r.Get(@"TEST2\Person2"+x), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
                GraphUtil.ReplaceEdge(r.Get(@"TEST2\Person3"+x), "IsPretty", r.Get(@"TEST\Pretty\Yes"));
                GraphUtil.ReplaceEdge(r.Get(@"TEST2\Person4"+x), "IsPretty", r.Get(@"TEST\Pretty\Yes"));

                r.Get(@"TEST2\Person1"+x+@"\Radek").AddEdge(r.Get(@"System\Meta*$Is"), smzt.Get("String"));


                r.Get(@"TEST2\Person1"+x).AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
                r.Get(@"TEST2\Person2"+x).AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
                r.Get(@"TEST2\Person3"+x).AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
                r.Get(@"TEST2\Person4"+x).AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\Person"));
            }

            for (int i = 1; i <= 10; i++)
            {
                IVertex x=r.Get("TEST2").AddVertex(null, i);

                for (int ii = 1; ii <= 10; ii++)
                {
                    IVertex xx = x.AddVertex(null, i + " " + ii);

                    for (int iii = 1; iii <= 10; iii++)
                    {
                        IVertex xxx=xx.AddVertex(null, i + " " + ii + " " + iii);

                        for (int iiii = 1; iiii <= 10; iiii++)
                            xxx.AddVertex(null, i + " " + ii + " " + iii+" "+iiii);
                    }
                }
            }

            //r.Get(@"TEST2\1").AddEdge(null, r.Get(@"TEST2\2\2 2\2 2 1"));
            
            GeneralUtil.ParseAndExcute(r.Get("TEST"), r.Get(@"System\Meta"), "{Diagram:TestDiagram{ZoomVisualiserContent:100,SelectedEdges:,CreationPool:}}");

            r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta\Visualiser\Diagram\SizeX"), 600.0);

            r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta\Visualiser\Diagram\SizeY"), 600.0);

            r.Get(@"TEST\TestDiagram").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta*Diagram"));
            
            IVertex i1=r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta*Item"),null);
            
            GeneralUtil.ParseAndExcute(i1,r.Get(@"System\Meta"),"{PositionX:0,PositionY:0,SizeX:100,SizeY:100}");

            IVertex i2 = r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i2, r.Get(@"System\Meta"), "{PositionX:200,PositionY:200,SizeX:100,SizeY:100}");

            i1.AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i1.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(@"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i1, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), r.Get(@"TEST\Person1"));

            i2.AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i2.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(@"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i2, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), r.Get(@"TEST\Person2"));

            



            i1 = r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i1, r.Get(@"System\Meta"), "{PositionX:350,PositionY:0}");

            i2 = r.Get(@"TEST\TestDiagram").AddVertex(r.Get(@"System\Meta*Item"), null);

            GeneralUtil.ParseAndExcute(i2, r.Get(@"System\Meta"), "{PositionX:0,PositionY:350}");

            i1.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(@"System\Data\Visualiser\Diagram\Object"));

            i1.AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            Edge.AddEdgeByToVertex(i1, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), r.Get(@"TEST\Person3"));

            i2.AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem"));

            i2.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), r.Get(@"System\Data\Visualiser\Diagram\Object"));

            Edge.AddEdgeByToVertex(i2, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), r.Get(@"TEST\Person4"));

            /////////////////////

           /* GeneralUtil.ParseAndExcute(r.Get("TEST"), r.Get(@"System\Meta"), "{Class:X1,Class:X2,Class:X3,Class:X4,Class:PersonA,Class:PersonB,Class:PersonB2{Attribute:New}}");

            r.Get(@"TEST\PersonB2\New").AddEdge(r.Get(@"System\Meta*$EdgeTarget"), r.Get(@"System\Meta*String"));

            VertexOperations.AddInstance(r.Get("TEST"), r.Get(@"TEST\PersonB2"), r.Get(@"TEST\Person")).Value="XXX";

            r.Get(@"TEST\X2").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\X3").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\X2"));
            r.Get(@"TEST\X4").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\X3"));

            r.Get(@"TEST\PersonA").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\PersonB").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\Person"));
            r.Get(@"TEST\PersonB2").AddEdge(r.Get(@"System\Meta*$Inherits"), r.Get(@"TEST\PersonB"));

            r.Get(@"TEST\XXX").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\PersonA"));
            r.Get(@"TEST\XXX").AddEdge(r.Get(@"System\Meta*$Is"), r.Get(@"TEST\X4"));*/

            //////////////////////

            IVertex tt = r.Get("TEST").AddVertex(r.Get("System*Class"), "TestClass");

            for(int x=0;x<3;x++)
                for (int y = 0; y < 10+x; y++)
                {
                    IVertex tta = tt.AddVertex(r.Get("System*Attribute"), "a" + x + " " + y + ";" + randomChars());
                    tta.AddVertex(r.Get("System*$Group"), x.ToString());
                    tta.AddVertex(r.Get("System*$Section"), y.ToString());

                    IVertex ttb = tt.AddVertex(r.Get("System*Attribute"), "b" + x + " " + y + ";" + randomChars());
                    ttb.AddVertex(r.Get("System*$Group"), x.ToString());
                    //ttb.AddVertex(r.Get("System*$Section"), y.ToString());

                    IVertex ttc = tt.AddVertex(r.Get("System*Attribute"), "c" + x + " " + y + ";" + randomChars());
                    ttc.AddVertex(r.Get("System*$Group"), x.ToString());
                    ttc.AddVertex(r.Get("System*$Section"), y.ToString());
                    ttc.AddVertex(r.Get("System*$MaxCardinality"), 6);
                }

            VertexOperations.AddInstance(r.Get("TEST"), tt);


            //////////////////////

            IVertex associations = r.GetAll(@"TEST\Person\Association:");
            IVertex ismeta = r.Get(@"System\Meta*$Is");
            IVertex asmeta = r.Get(@"System\Meta\UML\Class\Association");

            //foreach (IEdge v in associations)
             //   v.To.AddEdge(ismeta, asmeta);
            
            IVertex attributes = r.GetAll(@"TEST\Person\Attribute:");
            //IVertex ismeta=r.Get(@"System\Meta*$Is");
            IVertex ameta=r.Get(@"System\Meta\UML\Class\Attribute");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);

            attributes = r.GetAll(@"TEST3\\Attribute:");

            foreach (IEdge v in attributes)
                v.To.AddEdge(ismeta, ameta);
        }

        public void ShowContent(object obj)
        {
            _ShowContent(obj);
        }        

        protected LayoutAnchorable _ShowContent(object obj)
        {            
            LayoutAnchorable a = new LayoutAnchorable();                        

            if (obj is IPlatformClass)
            {
                IPlatformClass pc=(IPlatformClass)obj;

                if (pc.Vertex.Get(@"BaseEdge:\To:")!=null&&pc.Vertex.Get(@"BaseEdge:\To:").Value != null&&(!GeneralUtil.CompareStrings(pc.Vertex.Get(@"BaseEdge:\To:").Value,"")))
                    a.Title = pc.Vertex.Get(@"BaseEdge:\To:").Value.ToString();
                else
                    a.Title = (string)pc.Vertex.Value;

                PlatformClassSimpleWrapper pcsw = new PlatformClassSimpleWrapper();

                pcsw.SetContent(pc);

                a.Content = pcsw;

                a.IsVisibleChanged += pcsw.HideEventHandler;

                //a.Closing +=pcsw.ClosedEventHandler;
                a.Closed += pcsw.ClosedEventHandler;

                // not work - to focus
                //System.Windows.Input.Keyboard.Focus((IInputElement)pc);

                this.Pane.Children.Add(a);
                
                pcsw.IsIntialising = true;

                a.Hide(); // this works
                a.Show(); // for getting focus

                pcsw.IsIntialising = false;
            }else{
                a.Title = obj.ToString();
                a.Content = obj;

                this.Pane.Children.Add(a);

                a.Hide(); // this works
                a.Show(); // for getting focus
            }
            
            //a.AddToLayout(this.dockingManager, AnchorableShowStrategy.Most); 
            // maybe for later use

            return a;
        }

        public int DialogWindowDefaultWidth=400;
        public int DialogWindowDefaultHeight = 400;

        public void ShowContentFloating(object obj){            
            LayoutAnchorable a=_ShowContent(obj);
            

            a.FloatingTop = this.Top + (this.Height / 2) - (Math.Min(this.Height,DialogWindowDefaultHeight) / 2);
            a.FloatingLeft = this.Left + (this.Width / 2) - (Math.Min(this.Width,DialogWindowDefaultWidth) / 2);

            a.FloatingWidth = DialogWindowDefaultWidth;
            a.FloatingHeight = DialogWindowDefaultHeight;            

            a.Float();
            
        }

        public void CloseWindowByContent(object obj)
        {
            LayoutContent layoutContent = dockingManager.Layout.ActiveContent;

            layoutContent.Close();
        }

        public void ShowInfo(string info)
        {
            m0.UIWpf.Dialog.Info i = new UIWpf.Dialog.Info();

            i.Owner = this;

            i.Text = info;

            i.ShowDialog();
        }

        public IVertex SelectDialog(IVertex info, IVertex options, Point position)
        {
            SelectDialog d = new SelectDialog(info, options,position);

            return d.SelectedOption;
        }

        public void EditDialog(IEdge baseEdge, Point position)
        {
            EditDialog d = new EditDialog(baseEdge, position);
        }
    }
}
