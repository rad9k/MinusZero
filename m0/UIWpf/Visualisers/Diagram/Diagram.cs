using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.UML;
using m0.ZeroTypes;
using System.Windows;
using m0.Graph;
using m0.Util;
using System.Windows.Media;
using System.Windows.Shapes;
using m0.UIWpf.Controls;
using System.Windows.Input;
using System.Diagnostics;
using m0.UIWpf.Foundation;
using m0.UIWpf.Commands;
using m0.UIWpf.Visualisers;

namespace m0.UIWpf.Visualisers.Diagram
{
    public enum ClickTargetEnum
    {
        MouseUpOrLeave, Selection, Item, AnchorLeftTop, AnchorMiddleTop, AnchorRightTop_CreateDiagramLine, AnchorLeftMiddle, AnchorRightMiddle, AnchorLeftBottom, AnchorMiddleBottom, AnchorRightBottom
    }

    public class MetaToPair
    {
        public IVertex Meta;
        public IVertex To;
        public int DiagramLinesNumber;
        public int EdgesNumber;
    }

    public class Diagram : Border, IPlatformClass, IDisposable, IHasLocalizableEdges, IHasSelectableEdges
    {
        public Canvas TheCanvas;

        Rectangle SelectionArea;

        public bool IsSelecting = false;

        public bool IsDrawingLine = false;

        public Line CreatedDiagramLine;

        public FrameworkElement ClickedAnchor;

        int SelectionAreaLeft, SelectionAreaTop;

        public List<DiagramItemBase> Items;

        public ClickTargetEnum ClickTarget;

        public DiagramItemBase ClickedItem;

        public DiagramItemBase HighlightedItem;

        public double ClickPositionX_ItemCordinates;
        public double ClickPositionY_ItemCordinates;

        public double ClickPositionX_AnchorCordinates;
        public double ClickPositionY_AnchorCordinates;
        
        public bool IsPaiting = false;

        bool IsFirstPainted = false;

   

        protected List<DiagramItemBase> GetItemsByBaseEdge(IVertex to)
        {
            List<DiagramItemBase> r = new List<DiagramItemBase>();

            foreach (DiagramItemBase i in Items)
                if (i.Vertex.Get(@"BaseEdge:\To:") == to.Get("To:"))
                    r.Add(i);

            return r;
        }        

        public void AddEdgesFromDefintion(IVertex baseVertex, IVertex definitionEdges)
        {
            foreach (IEdge e in definitionEdges)                
                GraphUtil.CreateOrReplaceEdge(baseVertex, e.Meta, e.To);
        }

        protected void AddItem(IVertex ItemVertex){
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(ItemVertex);

            if (pc is DiagramItemBase)
            {
                if(pc.Vertex.Get(@"Definition:\DiagramItemVertex:")!=null)
                    AddEdgesFromDefintion(pc.Vertex,pc.Vertex.Get(@"Definition:\DiagramItemVertex:"));

                DiagramItemBase item = (DiagramItemBase)pc;

                item.Diagram = this;                

                item.VertexSetedUp();

           

                Items.Add(item);

                Panel.SetZIndex(item, 1);
                Canvas.SetLeft(item, GraphUtil.GetDoubleValue(item.Vertex.Get("PositionX:")));
                Canvas.SetTop(item, GraphUtil.GetDoubleValue(item.Vertex.Get("PositionY:")));        

                TheCanvas.Children.Add(item);

                item.UpdateLayout();
            }
        }

        public void AddLineObjects()
        {
            List<MetaToPair> metatopairs = new List<MetaToPair>();

           foreach(DiagramItemBase item in Items){
               foreach (IEdge l in item.Vertex.GetAll("DiagramLine:")) // calculate DiagramLines number and Edges number for each Meta/To edge pair
               {
                   MetaToPair found = null;

                   foreach (MetaToPair pair in metatopairs)
                       if (pair.Meta == l.To.Get(@"BaseEdge:\Meta:") && pair.To == l.To.Get(@"BaseEdge:\To:"))
                           found = pair;

                   if (found == null)
                   {
                       MetaToPair newpair = new MetaToPair();
                       newpair.Meta = l.To.Get(@"BaseEdge:\Meta:");
                       newpair.To = l.To.Get(@"BaseEdge:\To:");
                       newpair.DiagramLinesNumber = 1;
                       newpair.EdgesNumber = 0;

                       foreach(IEdge e in item.Vertex.GetAll(@"BaseEdge:\To:\"))
                           if (newpair.Meta == e.Meta && newpair.To == e.To)
                               newpair.EdgesNumber++;

                       metatopairs.Add(newpair);
                       
                   }else
                       found.DiagramLinesNumber++;
               }

               foreach(MetaToPair pair in metatopairs){ // delete DiagramLines for edges that been deleted
                   if(pair.DiagramLinesNumber>pair.EdgesNumber)
                       foreach(IEdge e in item.Vertex.GetAll("DiagramLine:")){
                           if(pair.Meta == e.To.Get(@"BaseEdge:\Meta:") && pair.To == e.To.Get(@"BaseEdge:\To:") && pair.DiagramLinesNumber>pair.EdgesNumber){
                               item.Vertex.DeleteEdge(e);
                               pair.DiagramLinesNumber--;
                           }
                       }
               }

               foreach (IEdge l in item.Vertex.GetAll("DiagramLine:")) // add doagram line objects
                   item.AddDiagramLineObject(GetToDiagramItemFromLineVertex(l.To), l.To);
               }
                            
        }

        public DiagramItemBase GetToDiagramItemFromLineVertex(IVertex lineVertex)
        {
            IVertex toFind = null;

            if (lineVertex.Get(@"BaseEdge:\Meta:\$VertexTarget:") != null)
                toFind = lineVertex.Get(@"BaseEdge:\To:\$EdgeTarget:");
            else
                toFind = lineVertex.Get(@"BaseEdge:\To:");

            if (toFind != null)
                foreach (DiagramItemBase i in Items)
                {
                    bool canReturn = true;

                    if (lineVertex.Get(@"Definition:\ToDiagramItemTestQuery:") != null && i.Vertex.Get((string)lineVertex.Get(@"Definition:\ToDiagramItemTestQuery:").Value) == null)
                        canReturn = false;

                    if (i.Vertex.Get(@"BaseEdge:\To:") == toFind && canReturn)
                        return i;
                }

            return null;
        }

        public List<DiagramItemBase> GetToDiagramItemFromEdge(IEdge edge)
        {
            IVertex toFind = null;

            List<DiagramItemBase> list=new List<DiagramItemBase>();

            if (edge.Meta.Get(@"$VertexTarget:") != null)
                toFind = edge.To.Get(@"$EdgeTarget:");
            else
                toFind = edge.To;

            if (toFind != null)
                foreach (DiagramItemBase i in Items)
                {                  
                    if (i.Vertex.Get(@"BaseEdge:\To:") == toFind)
                        list.Add(i);
                }

            return list;
        }

        public void HideSelectionArea()
        {
            Canvas.SetLeft(SelectionArea, 0);
            Canvas.SetTop(SelectionArea, 0);
            SelectionArea.Width = 0;
            SelectionArea.Height = 0;

            IsSelecting = false;
        }

        public void SetSelectionArea(double left, double top, double right, double bottom) {
            double _left, _right, _top, _bottom;

            SelectionArea_RemapCordinates(left, top, right, bottom, out _left, out _right, out _top, out _bottom);

            Canvas.SetLeft(SelectionArea, _left);
            Canvas.SetTop(SelectionArea, _top);
            SelectionArea.Width = _right - _left;
            SelectionArea.Height = _bottom - _top;

            IsSelecting = true;
        }

        private static void SelectionArea_RemapCordinates(double left, double top, double right, double bottom, out double _left, out double _right, out double _top, out double _bottom)
        {
            if (left > right)
            {
                _left = right;
                _right = left;
            }
            else
            {
                _left = left;
                _right = right;
            }

            if (top > bottom)
            {
                _top = bottom;
                _bottom = top;
            }
            else
            {
                _top = top;
                _bottom = bottom;
            }
        }

        private void TurnOnSelectedEdgesFireChange()
        {
            if (Vertex.Get("SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get("SelectedEdges:")).CanFireChangeEvent = true;
        }

        private void TurnOffSelectedEdgesFireChange()
        {
            if (Vertex.Get("SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get("SelectedEdges:")).CanFireChangeEvent = false;
        }

        void SelectItemsBySelectionArea(double left, double top, double right, double bottom)
        {
            double _left, _right, _top, _bottom;

            SelectionArea_RemapCordinates(left, top, right, bottom, out _left, out _right, out _top, out _bottom);

            UnselectAllEdges();

            TurnOffSelectedEdgesFireChange();

            foreach(DiagramItemBase i in Items){
                int ileft, itop, iright, ibottom;

                ileft = (int)Canvas.GetLeft(i);
                itop = (int)Canvas.GetTop(i);
                iright = ileft + (int)i.ActualWidth;
                ibottom = itop + (int)i.ActualHeight;

                if (_left <= ileft && _right >= iright && _top <= itop && _bottom >= ibottom)
                    i.AddToSelectedEdges();
            }

            TurnOnSelectedEdgesFireChange();

            SelectedVertexesUpdated();
        }

        public void PaintDiagram()
        {
            if (ActualHeight != 0 || IsFirstPainted)
            {
                MinusZero.Instance.Log(1, "Diagram", "");

                // turn off Vertex.Change listener

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                //                                

                IsPaiting = true;                

                TheCanvas.Children.Clear();

                foreach (DiagramItemBase e in Items)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();

                Items.Clear();

                Width = GraphUtil.GetDoubleValue(Vertex.Get("SizeX:"));
                Height = GraphUtil.GetDoubleValue(Vertex.Get("SizeY:"));

                Background = new SolidColorBrush(Color.FromRgb(255, 200, 200));

                foreach (IEdge ie in Vertex.GetAll("Item:"))
                    AddItem(ie.To);

                UpdateLayout();


                AddLineObjects();

                SelectionArea = new Rectangle();
                TheCanvas.Children.Add(SelectionArea);

                SelectionArea.Stroke = (Brush)FindResource("0HighlightBrush");
                SelectionArea.StrokeDashArray=new DoubleCollection(new double[]{3,3});

                HideSelectionArea();

               

                SelectWrappersForSelectedVertexes();

                IsFirstPainted = true;

                IsPaiting = false;

                // turn on Vertex.Change listener

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                //

                CheckAndUpdateDiagramLines();
            }
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            PaintDiagram();

            if (IsFirstPainted)
                this.Loaded -= OnLoad;
        }        

        public Diagram()
        {
            MinusZero mz = MinusZero.Instance;

            Items = new List<DiagramItemBase>();

            TheCanvas = new Canvas();

            TheCanvas.Background = (Brush)FindResource("0BackgroundBrush");

            this.Child = TheCanvas;

            this.BorderThickness = new Thickness(1);

            this.BorderBrush = (Brush)FindResource("0LightGrayBrush");


            this.AllowDrop = true;

            if (mz != null && mz.IsInitialized)
            {                       
                this.ContextMenu = new m0ContextMenu(this);
            }

            this.Loaded += new RoutedEventHandler(OnLoad);
            this.MouseMove += MouseMoveHandler;
            this.MouseLeave+=MouseLeaveHandler;
            this.MouseLeftButtonDown += MouseButtonDownHandler;
            this.MouseLeftButtonUp+=MouseButtonUpHandler;
            this.Drop += dndDrop;
        }

        protected void MouseButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            SelectionAreaLeft = (int)e.GetPosition(TheCanvas).X;
            SelectionAreaTop = (int)e.GetPosition(TheCanvas).Y;

            ClickTarget = ClickTargetEnum.Selection;

            UnselectAllEdges();
        }

        protected void CreateAndUpdateDiagramLine(double ToX, double ToY)
        {
            if (CreatedDiagramLine == null)
            {
                CreatedDiagramLine = new Line();

                IsDrawingLine = true;

                Panel.SetZIndex(CreatedDiagramLine, 100000);

                CreatedDiagramLine.Stroke = (Brush)FindResource("0HighlightBrush");

                CreatedDiagramLine.StrokeThickness = 2;

                TheCanvas.Children.Add(CreatedDiagramLine);

                CreatedDiagramLine.X1 = Canvas.GetLeft(ClickedItem)+ClickedItem.ActualWidth;
                CreatedDiagramLine.Y1 = Canvas.GetTop(ClickedItem);
            }

            CreatedDiagramLine.X2= ToX;
            CreatedDiagramLine.Y2=ToY;

            Point p = new Point(ToX, ToY);            

            foreach (DiagramItemBase i in Items)
            {
                if (VisualTreeHelper.HitTest(i, TranslatePoint(p, i)) != null)
                {
                    if (HighlightedItem == null)
                    {
                        i.Highlight();

                        HighlightedItem = i;
                    }
                }
                else
                {
                    if (HighlightedItem == i)
                    {
                        HighlightedItem = null;

                        i.Unhighlight();
                    }
                }
            }
        }
       
        protected void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ClickTarget == ClickTargetEnum.AnchorLeftTop)
                {
                    ClickedItem.MoveAndResizeItem(
                        (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                        (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates),
                        ClickedItem.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem)),                       
                       ClickedItem.ActualHeight - ( (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates) - Canvas.GetTop(ClickedItem)) );                        
                }

                if (ClickTarget == ClickTargetEnum.AnchorMiddleTop)
                {
                    ClickedItem.MoveAndResizeItem(
                        Canvas.GetLeft(ClickedItem),
                        (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates),
                        ClickedItem.ActualWidth,
                       ClickedItem.ActualHeight - ((e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates) - Canvas.GetTop(ClickedItem)));
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightTop_CreateDiagramLine)
                {                    
                    CreateAndUpdateDiagramLine(e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);
                }

                if (ClickTarget == ClickTargetEnum.AnchorLeftMiddle)
                {
                    ClickedItem.MoveAndResizeItem(
                        (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                        Canvas.GetTop(ClickedItem),
                        ClickedItem.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem)),
                       ClickedItem.ActualHeight);
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightMiddle)
                {
                    ClickedItem.MoveAndResizeItem(
                        Canvas.GetLeft(ClickedItem),
                        Canvas.GetTop(ClickedItem),
                        e.GetPosition(TheCanvas).X - Canvas.GetLeft(ClickedItem) - ClickPositionX_AnchorCordinates,
                       ClickedItem.ActualHeight);
                }

                if (ClickTarget == ClickTargetEnum.AnchorLeftBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                      Canvas.GetTop(ClickedItem),
                      ClickedItem.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem)),
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.AnchorMiddleBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      Canvas.GetLeft(ClickedItem),
                      Canvas.GetTop(ClickedItem),
                      ClickedItem.ActualWidth,
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      Canvas.GetLeft(ClickedItem),
                      Canvas.GetTop(ClickedItem),
                      e.GetPosition(TheCanvas).X - Canvas.GetLeft(ClickedItem) - ClickPositionX_AnchorCordinates,
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.Selection) // selection
                {
                    SetSelectionArea(SelectionAreaLeft, SelectionAreaTop, e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);
                    //SelectItemsBySelectionArea(SelectionAreaLeft, SelectionAreaTop, e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);
                    // too slow
                }

                if (ClickTarget == ClickTargetEnum.Item) // item move
                {
                    if ((Vertex.GetAll(@"SelectedEdges:\").Count() > 0 && ClickedItem.IsSelected == false) ||
                        Vertex.GetAll(@"SelectedEdges:\").Count() > 1)
                    {
                        if (ClickedItem.IsSelected == false)
                            ClickedItem.AddToSelectedEdges();

                        foreach (IEdge ed in Vertex.GetAll(@"SelectedEdges:\"))
                            foreach (DiagramItemBase item in GetItemsByBaseEdge(ed.To))
                                item.MoveItem(GraphUtil.GetDoubleValue(item.Vertex.Get("PositionX:")) + (e.GetPosition(ClickedItem).X - ClickPositionX_ItemCordinates),
                                    GraphUtil.GetDoubleValue(item.Vertex.Get("PositionY:")) + (e.GetPosition(ClickedItem).Y - ClickPositionY_ItemCordinates));
                    }
                    else
                    {                        
                        if (ClickedItem.IsSelected == false)
                        {
                            UnselectAllEdges();

                            ClickedItem.AddToSelectedEdges();
                        }

                        ClickedItem.MoveItem((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates), (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates));
                    }

                }
            }
        }

        protected void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            MouseUpOrLeave(false,e);
        }

        protected void MouseButtonUpHandler(object sender, MouseButtonEventArgs e){
            MouseUpOrLeave(true,e);
        }

        protected void MouseUpOrLeave(bool IsUp, MouseEventArgs e)
        {
            if (ClickTarget == ClickTargetEnum.Selection)
            {
                SelectItemsBySelectionArea(SelectionAreaLeft, SelectionAreaTop, e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);

                HideSelectionArea();
            }

            if (ClickTarget == ClickTargetEnum.AnchorRightTop_CreateDiagramLine)
            {
                if (HighlightedItem != null)
                {
                    HighlightedItem.Unhighlight();

                    if (IsUp)
                        ClickedItem.DoCreateDiagramLine(HighlightedItem);
                }

                HighlightedItem = null;
                TheCanvas.Children.Remove(CreatedDiagramLine);
                CreatedDiagramLine = null;

                IsDrawingLine = false;
            }

            ClickTarget = ClickTargetEnum.MouseUpOrLeave;
        }

        protected void ChangeZoomVisualiserContent()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get("ZoomVisualiserContent:"))) / 100;

            if (scale != 1.0)
            {
                if (ActualHeight != 0)
                {
                    this.LayoutTransform = new ScaleTransform(scale, scale, ActualWidth / 2, ActualHeight / 2);
                }
            }
            else
                this.LayoutTransform = null;
        }               
        
        protected void UnselectAllSelected()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            foreach (IEdge v in sv)
                foreach(DiagramItemBase i in Items)
                    if(i.Vertex.Get(@"BaseEdge:\To:")==v.To.Get("To:"))
                        i.Unselect();                                        

            UnselectAllEdges();
        }

        protected void UnselectAll()
        {
            foreach (DiagramItemBase i in Items)
                i.Unselect();
        }

        public void UnselectAllEdges()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            GraphUtil.RemoveAllEdges(sv);
        }

        protected void SelectedVertexesUpdated()
        {
            if (IsFirstPainted)
            {
                UnselectAll();

                SelectWrappersForSelectedVertexes();
            }
        }

        protected void SelectWrappersForSelectedVertexes()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            foreach (IEdge e in sv)
                foreach (DiagramItemBase i in Items)
                    if (i.Vertex.Get(@"BaseEdge:\To:") == e.To.Get("To:"))
                        i.Select();                                        
        }

        public void VertexChange(object sender, VertexChangeEventArgs e)
        {            
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedEdges")))
            { SelectedVertexesUpdated(); return; }

            if ((sender == Vertex.Get("SelectedEdges:")) && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
            { SelectedVertexesUpdated(); return; }

            if (sender is IVertex && GraphUtil.FindEdgeByToVertex(Vertex.GetAll(@"SelectedEdges:\"), (IVertex)sender) != null)
            { SelectedVertexesUpdated(); return; }

            if (sender == Vertex.Get("ZoomVisualiserContent:") && e.Type == VertexChangeType.ValueChanged)
            { ChangeZoomVisualiserContent(); return; }

            if ((sender == Vertex.Get("SizeX:") || sender == Vertex.Get("SizeY:")) && e.Type == VertexChangeType.ValueChanged)
            { PaintDiagram(); return; }   
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                MinusZero mz = MinusZero.Instance;

                if (_Vertex != null)
                {
                    GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(@"System\Session\Visualisers"), Vertex);

                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));
                }

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                //mz.Root.Get(@"System\Session\Visualisers").AddEdge(null, Vertex);

                PaintDiagram();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                MinusZero mz = MinusZero.Instance;

                IsDisposed = true;

                foreach (DiagramItemBase e in Items)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();
                
                //GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(@"System\Session\Visualisers"), Vertex);

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                //if (Vertex is IDisposable) HELLO NO - Vertex stayes!!! this is not normal Visualiser where its Vertex disapears @ Dispose
                 //   ((IDisposable)Vertex).Dispose();
                
            }
        }


        // IHasLocalizableEdges

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByLocation(Point p)
        {
            vertexByLocationToReturn = null;

            foreach(DiagramItemBase i in Items)
            {
                if (VisualTreeHelper.HitTest(i, TranslatePoint(p, i)) != null)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    //Edge.AddEdgeEdgesOnlyTo(v, i.Vertex.Get(@"BaseEdge:\To:"));
                    Edge.AddEdgeEdges(v,i.Vertex.Get(@"BaseEdge:\From:"),i.Vertex.Get(@"BaseEdge:\Meta:"), i.Vertex.Get(@"BaseEdge:\To:"));
                    vertexByLocationToReturn = v;
                }
            }          

            return vertexByLocationToReturn;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

       /////////////////////////////

        private void AddDiagramItemDialog(double x, double y, IVertex vv, bool isSet, DragEventArgs e)
        {
            IVertex r = m0.MinusZero.Instance.Root;

            NewDiagramItem ndi = new NewDiagramItem(vv, isSet, e);


            if (ndi.DiagramItemDefinition != null)
            {
                if (ndi.InstanceOfMeta)
                {
                    IVertex v = VertexOperations.AddInstance(Vertex.Get("CreationPool:"), ndi.BaseEdge.Get("To:"));

                    v.Value = ndi.InstanceValue;

                    AddDiagramItem(x,
                                   y,
                                   ndi.DiagramItemDefinition,
                                   ndi.BaseEdge.Get("To:"), v);
                }
                else
                {
                    bool ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo = false;
                    bool ThereIsDiagramItemOfThisBaseEdgeTo = false;

                    IVertex DiagramItemOfThisDiagramItemDefinition = Vertex.GetAll(@"Item:{Definition:" + ndi.DiagramItemDefinition.Value + "}");

                    foreach (IEdge ee in DiagramItemOfThisDiagramItemDefinition)
                        if (ee.To.Get(@"BaseEdge:\To:") == ndi.BaseEdge.Get("To:"))
                            ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo = true;

                    foreach (DiagramItemBase b in Items)
                        if (b.Vertex.Get(@"BaseEdge:\To:") == ndi.BaseEdge.Get("To:"))
                            ThereIsDiagramItemOfThisBaseEdgeTo = true;


                    if (ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo == false)
                    {
                        if (ThereIsDiagramItemOfThisBaseEdgeTo == false ||
                            GeneralUtil.CompareStrings(r.Get(@"User\CurrentUser:\Settings:\AllowManyDiagramItemsForOneVertex:").Value, "True"))
                        {
                            AddDiagramItem(x,
                                        y,
                                        ndi.DiagramItemDefinition,
                                        ndi.BaseEdge);
                        }
                        else
                            m0.MinusZero.Instance.DefaultShow.ShowInfo("There is allready diagram item, that visualises dropped vertex.\n\nNow, it is not possible to add second representation of same vertex.\n\nOne can change this limitation by changing \"User\\CurrentUser:\\Settings:\\AllowManyDiagramItemsForOneVertex:\" setting.");
                    }
                    else
                        m0.MinusZero.Instance.DefaultShow.ShowInfo("There is allready \"" + ndi.DiagramItemDefinition.Value + "\" diagram item, that visualises dropped vertex.\n\nIt is not possible to add second representation of same vertex, with the same diagram item type.");
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex r=m0.MinusZero.Instance.Root;

                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                double x = e.GetPosition(TheCanvas).X, y = e.GetPosition(TheCanvas).Y;

                bool isSet = false;

                if (dndVertex.Count() > 1)
                    isSet = true;

                if(isSet)
                    Process.UI.NonAtomProcess.StartNonAtomProcess();

                foreach (IEdge eee in dndVertex)
                {
                    AddDiagramItemDialog(x,y, eee.To,isSet,e);
                    x += 25;
                    y += 25;
                }

                CheckAndUpdateDiagramLines();

                if (isSet)
                    Process.UI.NonAtomProcess.StopNonAtomProcess();

                if (e.Data.GetData("DragSource") is IHasSelectableEdges)
                    ((IHasSelectableEdges)e.Data.GetData("DragSource")).UnselectAllEdges();

                GraphUtil.RemoveAllEdges(dndVertex);
            }
        }

        private IVertex AddDiagramItem_Base(double x,double y, IVertex DiagramItemDefinition){
            IVertex r = m0.MinusZero.Instance.Root;

            IVertex v = VertexOperations.AddInstance(Vertex, DiagramItemDefinition.Get("DiagramItemClass:"), r.Get(@"System\Meta\Visualiser\Diagram\Item"));

            GraphUtil.SetVertexValue(v, r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\PositionX"), x);
            GraphUtil.SetVertexValue(v, r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\PositionY"), y);

            GraphUtil.ReplaceEdge(v, r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), DiagramItemDefinition);                        

            return v;
        }

        public void AddDiagramItem(double x,double y, IVertex DiagramItemDefinition, IVertex BaseEdge){
            IVertex r = m0.MinusZero.Instance.Root;

            IVertex v = AddDiagramItem_Base(x, y, DiagramItemDefinition);
            
            GraphUtil.ReplaceEdge(v, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), BaseEdge);

            AddItem(v);            
        }

        public void AddDiagramItem(double x, double y, IVertex DiagramItemDefinition, IVertex metaVertex,IVertex newVertex)
        {
            IVertex v = AddDiagramItem_Base(x, y, DiagramItemDefinition);

            IVertex be = v.Get("BaseEdge:");

            Edge.AddEdgeEdgesOnlyMetaTo(be, metaVertex, newVertex);

            AddItem(v);

            CheckAndUpdateDiagramLines();
        }

        public void CheckAndUpdateDiagramLines()
        {
            foreach(DiagramItemBase item in Items)
                CheckAndUpdateDiagramLinesForItem(item);
        }

        public void CheckAndUpdateDiagramLinesForItem(DiagramItemBase item)
        {
            foreach (IEdge e in item.Vertex.Get(@"BaseEdge:\To:"))
            {
                List<DiagramItemBase> toDiagramItems = null;

                toDiagramItems = GetItemsByBaseEdgeTo_ForLines(e);

                foreach (DiagramItemBase toDiagramItem in toDiagramItems)
                {
                    bool needAdding = true;

                    foreach (IEdge ee in item.Vertex.GetAll("DiagramLine:"))
                        if (ee.To.Get(@"BaseEdge:\Meta:") == e.Meta && ee.To.Get(@"BaseEdge:\To:") == e.To)
                            needAdding = false;

                    if (needAdding)
                    {
                        IVertex lineDef = GetLineDefinition(e,item.Vertex, toDiagramItem);

                        if (lineDef != null)
                        {
                            bool canAdd = true;

                            if (item.Vertex.Get(@"Definition:\DoNotShowInherited:True") != null)
                                if (VertexOperations.IsInheritedEdge(item.Vertex.Get(@"BaseEdge:\To:"), e.Meta))
                                    canAdd = false;

                            if (canAdd)
                                item.AddDiagramLineVertex(e, lineDef, toDiagramItem);
                        }
                    }
                }
            }
        }

        protected List<DiagramItemBase> GetItemsByBaseEdgeTo_ForLines(IEdge toEdge)
        {
            List<DiagramItemBase> r = new List<DiagramItemBase>();

            foreach (DiagramItemBase i in Items)
            {
                if (i.Vertex.Get(@"BaseEdge:\To:") == toEdge.To)
                    r.Add(i);

                if (toEdge.Meta.Get("$VertexTarget:") != null && toEdge.To.Get("$EdgeTarget:") == i.Vertex.Get(@"BaseEdge:\To:"))
                    r.Add(i);
            }

            return r;
        }

        public IVertex GetLineDefinition(IEdge e,IVertex Vertex, DiagramItemBase toItem){
            if (GeneralUtil.CompareStrings(Vertex.Get("Definition:"), "Vertex")) // Vertex / Edge
                return Vertex.Get(@"Definition:\DiagramLineDefinition:Edge");

            foreach (IEdge def in Vertex.GetAll(@"Definition:\DiagramLineDefinition:"))
            {
                bool canReturn=true;

                if(def.To.Get("EdgeTestQuery:")!=null){
                    canReturn=false;

                    foreach (IEdge toTest in Vertex.GetAll(@"BaseEdge:\To:\" + def.To.Get("EdgeTestQuery:")))
                        if (toTest.To == e.Meta)
                            canReturn = true;
                }

                if (canReturn && def.To.Get("ToDiagramItemTestQuery:") != null && toItem.Vertex.Get((string)def.To.Get("ToDiagramItemTestQuery:").Value) != null)
                    return def.To;
            }
            

            return null;           
        }

    }
}
