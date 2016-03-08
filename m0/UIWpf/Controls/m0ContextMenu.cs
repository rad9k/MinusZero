using m0.Foundation;
using m0.UIWpf.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using m0.UIWpf.Commands;
using m0.Util;

namespace m0.UIWpf.Controls
{
    public class m0ContextMenu : ContextMenu
    {
        IPlatformClass PlatformClass;
        IVertex root;
        IVertex Edge;

        public m0ContextMenu(IPlatformClass pc)
        {
            root = MinusZero.Instance.Root;

            PlatformClass = pc;

            AddOpen();

            AddSeparator();

            AddOpenVisualiserItems();

            AddSeparator();

            AddNew();

            AddSeparator();

            AddNewDiagram();

            AddSeparator();

            AddZeroMenuItems();            

            AddSeparator();

            AddCutCopyPasteItems();

            this.Opened += m0ContextMenu_Opened;
        }

        void m0ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            Edge=null;

            if (PlatformClass is IHasLocalizableEdges && PlatformClass is IInputElement)
            {
                Point p = Mouse.GetPosition((IInputElement)PlatformClass);
                Edge = ((IHasLocalizableEdges)PlatformClass).GetEdgeByLocation(p);

                if (Edge == null)
                {
                    DisableMenuItems();
                    return;
                }

                EnableMenuItems();            

                FillNewVertexBySchemaMenu();
            }

            if (Edge == null)
            {
                DisableMenuItems();
                return;
            }
        }

        private void FillNewVertexBySchemaMenu()
        {
            IVertex r = Edge.GetAll(@"To:\$Is:");

            if (r.Count() == 0)
                r = Edge.GetAll(@"Meta:"); ;

            if (r.Count() == 0 || r.FirstOrDefault().To.Value==null || GeneralUtil.CompareStrings(r.FirstOrDefault().To.Value, "$Empty"))
            {
                NewVertexBySchema.IsEnabled = false;
                return;
            }
            
            NewVertexBySchema.Items.Clear();

            foreach(IEdge e in r)
                if (e.To.Value != null && !GeneralUtil.CompareStrings(e.To.Value, "$Empty"))
                {
                    MenuItem i = createMenuItem(e.To.Value.ToString());
                    NewVertexBySchema.Items.Add(i);

                    foreach (IEdge ee in e.To)
                        if (ee.To.Value != null && !GeneralUtil.CompareStrings(ee.To.Value, "$Empty"))
                    {
                        MenuItem ii = createMenuItem(ee.To.Value.ToString());
                        ii.Tag = ee.To;
                        i.Items.Add(ii);

                        ii.Click += OnNewVertexBySchema;
                    }
                }
            

            NewVertexBySchema.IsEnabled = true;
        }

        private void DisableMenuItems(){        
            ChangeActiveState_Reccurent(this, false);
        }

        private void EnableMenuItems()
        {
            ChangeActiveState_Reccurent(this, true);
        }

        private void ChangeActiveState_Reccurent(ItemsControl control, bool state){
            foreach (object o in control.Items)
                if (o is ItemsControl)
                {
                    ItemsControl c = (ItemsControl)o;

                    c.IsEnabled = state;

                    ChangeActiveState_Reccurent(c, state);
                }
            
        }

        private MenuItem createMenuItem(string header)
        {
            MenuItem m = new MenuItem();
            m.Header = header;
            return m;
        }

        private void AddSeparator(){
            this.Items.Add(new Separator());
        }

        private MenuItem NewVertexBySchema;

        private void AddOpen()
        {
            MenuItem NewVertex = createMenuItem("Open");
            NewVertex.Click += OnOpen;
            this.Items.Add(NewVertex);
        }

        private void AddNew()
        {
            MenuItem NewVertex = createMenuItem("New Vertex");
            NewVertex.Click += OnNewVertex;
            this.Items.Add(NewVertex);

            NewVertexBySchema = createMenuItem("New Vertex by Meta Schema");            
            this.Items.Add(NewVertexBySchema);
        }

        private void AddNewDiagram()
        {
            MenuItem NewVertex = createMenuItem("New Diagram");
            NewVertex.Click += OnNewDiagram;
            this.Items.Add(NewVertex);
        }

        private void AddCutCopyPasteItems()
        {
            MenuItem Cut = createMenuItem("Cut");
            Cut.Click += OnCut;
            this.Items.Add(Cut);

            MenuItem Copy = createMenuItem("Copy");
            Copy.Click += OnCopy;
            this.Items.Add(Copy);

            MenuItem Paste = createMenuItem("Paste");
            Paste.Click += OnPaste;
            this.Items.Add(Paste);

            MenuItem Delete = createMenuItem("Delete");
            Delete.Click += OnDelete;
            this.Items.Add(Delete);
        }

        private void AddZeroMenuItems()
        {
            MenuItem Query = createMenuItem("Query");
            Query.Click += OnQuery;
            this.Items.Add(Query);

            MenuItem ZeroCodeEditor = createMenuItem("ZeroCode Editor");
            ZeroCodeEditor.Click += OnZeroCodeEditor;
            this.Items.Add(ZeroCodeEditor);

            MenuItem GetGraphCreationCode = createMenuItem("Get graph creation code");
            GetGraphCreationCode.Click += OnGetGraphCreationCode;
            this.Items.Add(GetGraphCreationCode);
        }

        void AddOpenVisualiserItems()
        {
            MenuItem OpenVisualiser = createMenuItem("Open As");
            
            this.Items.Add(OpenVisualiser);

            // IVertex vislist = root.GetAll(@"System\Meta\Visualiser\"); BaseEdge ones currently not supported

            IVertex vislist = root.GetAll(@"System\Meta\Visualiser\Class:{$Inherits:HasBaseEdge}");

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());
                
                v.Tag = vis.To;

                v.Click += OnOpenVisualiser;
                
                OpenVisualiser.Items.Add(v);
            }

            MenuItem Special = createMenuItem("Open special");

            this.Items.Add(Special);

            /////////////////////// meta

            MenuItem OpenMetaVisualiser = createMenuItem("Open Visualiser for Meta");
            Special.Items.Add(OpenMetaVisualiser);

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenMetaVisualiser;

                OpenMetaVisualiser.Items.Add(v);
            }

            /////////////////////// floating

            MenuItem OpenVisualiserFloating = createMenuItem("Open Floating Visualiser");
            Special.Items.Add(OpenVisualiserFloating);

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenVisualiserFloating;

                OpenVisualiserFloating.Items.Add(v);
            }            

            /////////////////////// base synchronised

            MenuItem OpenVisualiserSelectedBase = createMenuItem("Open Master-Detail (SelectedEdges<>BaseEdge synchronised) Visualiser");
            Special.Items.Add(OpenVisualiserSelectedBase);

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenVisualiserSelectedBase;

                OpenVisualiserSelectedBase.Items.Add(v);
            }

            /////////////////////// selected synchronised

            vislist = root.GetAll(@"System\Meta\Visualiser\Class:{$Inherits:HasSelectedEdges}");

            MenuItem OpenVisualiserSelectedSelected = createMenuItem("Open SelectedEdges<>SelectedEdges synchronised Visualiser");
            Special.Items.Add(OpenVisualiserSelectedSelected);            

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenVisualiserSelectedSelected;

                OpenVisualiserSelectedSelected.Items.Add(v);
            }
        }

        /*m0ContextMenu getMenu(MenuItem i)
        {
            if (i.Parent is m0ContextMenu)
                return (m0ContextMenu)i.Parent;

            return (m0ContextMenu)getMenu((MenuItem)i.Parent);
        }*/
        

        void OnNewVertex(object sender, System.Windows.RoutedEventArgs e)
        {           
            BaseCommands.NewVertex(this.Edge, null);
        }
        
        void OnNewVertexBySchema(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is MenuItem)
                BaseCommands.NewVertexBySchema(this.Edge, (IVertex)((MenuItem)sender).Tag);            
        }

        void OnNewDiagram(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.NewDiagram(this.Edge, null);
        }


        void OnCut(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Cut(this.Edge, PlatformClass.Vertex);

            FromCopyPlatformClass = PlatformClass;
        }


        IPlatformClass FromCopyPlatformClass;

        void OnCopy(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Copy(this.Edge, PlatformClass.Vertex);

            FromCopyPlatformClass = PlatformClass;
        }

        void OnPaste(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Paste(this.Edge, PlatformClass.Vertex);

            if (FromCopyPlatformClass is IHasSelectableEdges)
                ((IHasSelectableEdges)FromCopyPlatformClass).UnselectAllEdges();
        }

        void OnDelete(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Delete(this.Edge, PlatformClass.Vertex);

            if (PlatformClass is IHasSelectableEdges)
                ((IHasSelectableEdges)PlatformClass).UnselectAllEdges();
        }

        void OnQuery(object sender, System.Windows.RoutedEventArgs e)
        {            
            BaseCommands.Query(this.Edge, null);
        }

        void OnZeroCodeEditor(object sender, System.Windows.RoutedEventArgs e)
        {           
            BaseCommands.ZeroCodeEditor(this.Edge, null);
        }

        void OnGetGraphCreationCode(object sender, System.Windows.RoutedEventArgs e)
        {            
            BaseCommands.GetGraphCreationCode(this.Edge, null);
        }

        void OnOpen(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Open(this.Edge, null);
        }

        void OnOpenVisualiser(object sender, System.Windows.RoutedEventArgs e)
        {            
            BaseCommands.OpenVisualiser(this.Edge, ((IVertex)((MenuItem)sender).Tag));
        }

        void OnOpenMetaVisualiser(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.OpenMetaVisualiser(this.Edge, ((IVertex)((MenuItem)sender).Tag));
        }

        void OnOpenVisualiserFloating(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.OpenVisualiserFloating(this.Edge, ((IVertex)((MenuItem)sender).Tag));
        }

        void OnOpenVisualiserSelectedBase(object sender, System.Windows.RoutedEventArgs e)
        {            
            IVertex input = MinusZero.Instance.CreateTempVertex();

            IVertex root = MinusZero.Instance.Root;

            input.AddEdge(root.Get(@"System\Meta\Commands*VisualiserClass"), ((IVertex)((MenuItem)sender).Tag));
            input.AddEdge(root.Get(@"System\Meta\Commands*SynchronisedVisualiser"), PlatformClass.Vertex);

            BaseCommands.OpenVisualiserSelectedBase(this.Edge, input);
        }

        void OnOpenVisualiserSelectedSelected(object sender, System.Windows.RoutedEventArgs e)
        {            
            IVertex input = MinusZero.Instance.CreateTempVertex();

            IVertex root=MinusZero.Instance.Root;

            input.AddEdge(root.Get(@"System\Meta\Commands*VisualiserClass"), ((IVertex)((MenuItem)sender).Tag));
            input.AddEdge(root.Get(@"System\Meta\Commands*SynchronisedVisualiser"), PlatformClass.Vertex);

            BaseCommands.OpenVisualiserSelectedSelected(this.Edge, input);
        }

    }
}
