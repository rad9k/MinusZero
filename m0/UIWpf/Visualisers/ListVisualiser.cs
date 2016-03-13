using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using m0.Foundation;
using System.Windows.Data;
using m0.Graph;
using m0.UML;
using m0.ZeroTypes;
using m0.Util;
using System.Windows.Media;
using System.Windows;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using System.Windows.Input;
using m0.UIWpf.Commands;
using System.Windows.Controls.Primitives;

namespace m0.UIWpf.Visualisers
{
    public class ListVisualiser : DataGrid, IPlatformClass, IDisposable, IHasLocalizableEdges, IHasSelectableEdges
    {
       

        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVertexesUpdate = false;

        public void UnselectAllEdges(){
            IVertex sv = Vertex.Get("SelectedEdges:");

            GraphUtil.RemoveAllEdges(sv);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e){
            if (!TurnOffSelectedVertexesUpdate)
            {
                TurnOffSelectedItemsUpdate = true;
                
                IVertex sv = Vertex.Get("SelectedEdges:");

                UnselectAllEdges();

                IVertex baseVertex = Vertex.Get(@"BaseEdge:\To:");

                foreach (IEdge ee in SelectedItems)
                    Edge.AddEdge(sv, baseVertex, ee.Meta, ee.To); // becouse of possible FilterQuery
                   // Edge.AddEdge(sv, ee);

                TurnOffSelectedItemsUpdate = false;
            }

            base.OnSelectionChanged(e);
        }

        protected virtual void CreateView(){
            Columns.Clear();

            DataGridTextColumn metaColumn = new DataGridTextColumn();

            if( GraphUtil.GetValueAndCompareStrings(Vertex.Get("ShowHeader:"), "True"))
                metaColumn.Header = "meta";

            Binding mb = new Binding("Meta.Value");
            mb.Mode = BindingMode.OneWay;
            metaColumn.Binding = mb;
            
            if(GeneralUtil.CompareStrings(Vertex.Get("IsMetaRightAlign:").Value,"True"))
                metaColumn.CellStyle = (Style)FindResource("0ListMetaColumnRight");
            else
                metaColumn.CellStyle = (Style)FindResource("0ListMetaColumnLeft");

            //metaColumn.Foreground = (Brush)FindResource("0GrayBrush");            
            
            Columns.Add(metaColumn);

            //DataGridTextColumn valueColumn = new DataGridTextColumn();
            //valueColumn.Binding = new Binding("To.Value");
            
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn();

            valueColumn.CellStyle = (Style)FindResource("0ListValueColumn");

            //
            // CELL TEMPLATE
            //

            if (GeneralUtil.CompareStrings(Vertex.Get("IsAllVisualisersEdit:").Value, "True"))
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
                factory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new Binding(""));                
                valueColumn.CellTemplate.VisualTree = factory;
            }
            else
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserViewWrapper));
                factory.SetBinding(VisualiserViewWrapper.BaseEdgeProperty, new Binding(""));
                valueColumn.CellTemplate.VisualTree = factory;
            }

            //
            // EDIT TEMPLATE
            //
            valueColumn.CellEditingTemplate = new DataTemplate();
            FrameworkElementFactory EditFactory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
            EditFactory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new Binding(""));
            valueColumn.CellEditingTemplate.VisualTree = EditFactory;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get("ShowHeader:"), "True"))
                 valueColumn.Header = "value";
            
            Columns.Add(valueColumn); 
        }

        protected void ResetView()
        {
            CreateView();

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get("GridStyle:"), "Vertical"))
            {
                this.BorderThickness = new System.Windows.Thickness(0);
                this.GridLinesVisibility = DataGridGridLinesVisibility.Vertical;                
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get("GridStyle:"), "Horizontal"))
            {
                this.BorderThickness = new System.Windows.Thickness(0);
                this.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;                
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get("GridStyle:"), "All"))
            {
                this.BorderThickness = new System.Windows.Thickness(0);
                this.GridLinesVisibility = DataGridGridLinesVisibility.All;                
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get("GridStyle:"), "AllAndRound"))
            {
                this.BorderThickness = new System.Windows.Thickness(1);
                this.GridLinesVisibility = DataGridGridLinesVisibility.All;
                this.BorderBrush = (Brush)FindResource("0ForegroundBrush");
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get("GridStyle:"), "Round"))
            {
                this.BorderThickness = new System.Windows.Thickness(1);
                this.BorderBrush = (Brush)FindResource("0LightGrayBrush");
            }
            else
            {
                this.BorderThickness = new System.Windows.Thickness(0);
                this.GridLinesVisibility = DataGridGridLinesVisibility.None;                
            }
        }

        protected void ChangeZoomVisualiserContent()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get("ZoomVisualiserContent:")))/100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }        

        protected void SelectedVertexesUpdated(){
            if (TurnOffSelectedItemsUpdate)
                return;

            TurnOffSelectedVertexesUpdate = true;

            this.SelectedItems.Clear();

            IVertex b=Vertex.Get(@"BaseEdge:\To:");

            if(b!=null)
            foreach(IEdge e in Vertex.Get("SelectedEdges:")){
                IEdge ee = GraphUtil.FindEdgeByToVertex(b, e.To.Get("To:"));
                if (ee != null)
                    this.SelectedItems.Add(ee);
            }

            TurnOffSelectedVertexesUpdate = false;
        }

        protected virtual void SetVertexDefaultValues(){
            Vertex.Get("IsMetaRightAlign:").Value = "False";
            Vertex.Get("IsAllVisualisersEdit:").Value = "False";
            Vertex.Get("ZoomVisualiserContent:").Value = 100;

            GraphUtil.ReplaceEdge(Vertex, "GridStyle", MinusZero.Instance.Root.Get(@"System\Meta\Visualiser\GridStyleEnum\None"));
        }

        protected virtual void PlatformClassInitialize()
        {
            MinusZero mz = MinusZero.Instance;

            //Vertex = mz.Root.Get(@"System\Session\Visualisers").AddVertex(null, "ListVisualiser" + this.GetHashCode());

            Vertex = mz.CreateTempVertex();
            Vertex.Value = "ListVisualiser" + this.GetHashCode();

            ClassVertex.AddIsClassAndAllAttributes(Vertex, mz.Root.Get(@"System\Meta\Visualiser\ListVisualiser"));

            ClassVertex.AddIsClassAndAllAttributes(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));
        }

        public ListVisualiser()
        {
            this.AllowDrop = true;

            this.AutoGenerateColumns = false;

            this.RowBackground = (Brush)FindResource("0BackgroundBrush");
            this.Background = (Brush)FindResource("0BackgroundBrush");
            this.HorizontalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            this.VerticalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");     
                        
            this.HeadersVisibility = DataGridHeadersVisibility.Column;
            
            this.SelectedValuePath = "To";
            VirtualizingStackPanel.SetIsVirtualizing(this,false); 
            MinusZero mz=MinusZero.Instance;

            if (mz != null&&mz.IsInitialized)
            {
                PlatformClassInitialize();

                SetVertexDefaultValues();

                CreateView();

                this.ContextMenu = new m0ContextMenu(this);

                this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                this.MouseMove += dndPreviewMouseMove; // !!!!!!!!!!!!!!!!!! otherwise sliders do not work
                this.Drop += dndDrop;

                this.MouseEnter += dndMouseEnter;
            }
        }

        protected virtual void UpdateBaseEdge(){
            IVertex bas = Vertex.Get(@"BaseEdge:\To:");

            if (bas != null)
            {
                ResetView();

                if (Vertex.Get(@"FilterQuery:") != null&&Vertex.Get(@"FilterQuery:").Value!=null)
                {
                    IVertex data=VertexOperations.DoFilter(bas, Vertex.Get(@"FilterQuery:"));

                    if (data != null)
                        ItemsSource = data.ToList();
                    else
                        ItemsSource = null;
                }
                else 
                  ItemsSource = bas.ToList(); // if there is no .ToList DataGrid can not edit
            }           
        }


        protected virtual void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge"))
                 || (sender == Vertex.Get("BaseEdge:") && e.Type == VertexChangeType.ValueChanged)
                || ((sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && ((GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))))
                UpdateBaseEdge();

            if (sender == Vertex.Get(@"BaseEdge:\To:") && (e.Type == VertexChangeType.EdgeAdded || e.Type == VertexChangeType.EdgeRemoved))
                UpdateBaseEdge();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedEdges")))
                SelectedVertexesUpdated();

            if ((sender == Vertex.Get("SelectedEdges:")) && ((e.Type == VertexChangeType.EdgeAdded)||(e.Type == VertexChangeType.EdgeRemoved)))
                SelectedVertexesUpdated();

            if (sender is IVertex && GraphUtil.FindEdgeByToVertex(Vertex.GetAll(@"SelectedEdges:\"), (IVertex)sender) != null)
                SelectedVertexesUpdated();

            if (sender == Vertex.Get("IsMetaRightAlign:") && e.Type == VertexChangeType.ValueChanged) 
                ResetView();

            if (sender == Vertex.Get("IsAllVisualisersEdit:") && e.Type == VertexChangeType.ValueChanged)
                ResetView();

            if (sender == Vertex.Get("ZoomVisualiserContent:") && e.Type == VertexChangeType.ValueChanged)
                ChangeZoomVisualiserContent();

            if (sender == Vertex.Get("FilterQuery:") && e.Type == VertexChangeType.ValueChanged)
                UpdateBaseEdge();
      
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "FilterQuery")))
                UpdateBaseEdge();

            if (sender == Vertex.Get("ShowHeader:") && e.Type == VertexChangeType.ValueChanged)
                ResetView();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "ShowHeader")))
                ResetView();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "GridStyle")))
                ResetView();
        }       

        private IVertex _Vertex;

        public IVertex Vertex { 
            get { return _Vertex;}
            set{
                if (_Vertex != null)
                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                UpdateBaseEdge();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                MinusZero mz = MinusZero.Instance;

                //GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(@"System\Session\Visualisers"), Vertex);

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        /*DataGridRow EditRow;

        protected override void OnBeginningEdit(DataGridBeginningEditEventArgs e)
        {
            EditRow = e.Row;    
            base.OnBeginningEdit(e);
        }

        protected virtual void OnRowEditEnding(DataGridRowEditEndingEventArgs e)
        {
            EditRow = null;
            base.OnRowEditEnding(e);
        }*/

        public IVertex GetEdgeByLocation(Point point)
        {
            var headersPresenter = UIWpf.FindVisualChild<DataGridColumnHeadersPresenter>(this);
            double headerActualHeight = headersPresenter.ActualHeight;

            if (point.Y <= headerActualHeight) // if header
                return null;
            
            foreach (var item in Items)
            {
                var row = ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

                if (row != null)
                {
                    DataGridRow roww = (DataGridRow)row;                    
                   
                    if (VisualTreeHelper.HitTest(roww, TranslatePoint(point, roww)) != null)
                        {
                            if (point.X >= Columns.First().ActualWidth && roww.IsEditing)
                                return null;

                            IVertex v = MinusZero.Instance.CreateTempVertex();
                            Edge.AddEdgeEdges(v, (IEdge)roww.Item);
                            return v;
                        }
                }
            }

            // DO WANT THIS FEATURE ?
            //
            if (GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(@"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                return Vertex.Get("BaseEdge:");
            else
                return null;
            
        }                        

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

        ///// DRAG AND DROP

        IVertex tempSelectedVertexes;

        protected void CopySelectedVertexesToTemp()
        {
            tempSelectedVertexes = MinusZero.Instance.CreateTempVertex();

            GraphUtil.CopyEdges(Vertex.Get("SelectedEdges:"), tempSelectedVertexes);
        }

        protected void RestoreSelectedVertexes()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            if (tempSelectedVertexes != null)
            {
                GraphUtil.RemoveAllEdges(sv);

                GraphUtil.CopyEdges(tempSelectedVertexes, sv);
            }
        }

       //
        
        Point dndStartPoint;
        bool hasButtonBeenDown;

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(this);
            hasButtonBeenDown = true;

            CopySelectedVertexesToTemp();

            MinusZero.Instance.IsGUIDragging = false;
        }

        bool isDraggin = false;

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            var headersPresenter = UIWpf.FindVisualChild<DataGridColumnHeadersPresenter>(this);
            double headerActualHeight = headersPresenter.ActualHeight;

            if (mousePos.Y <= headerActualHeight) // if header
            {
                e.Handled = false;
                return;
            }

            if (hasButtonBeenDown && isDraggin==false &&
                !UIWpf.IsMouseOverScrollbar(sender, dndStartPoint) &&
                (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                isDraggin = true;

                RestoreSelectedVertexes();

                IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                if (Vertex.Get(@"SelectedEdges:\") != null)
                    foreach (IEdge ee in Vertex.GetAll(@"SelectedEdges:\"))
                        dndVertex.AddEdge(null, ee.To);
                else
                {
                    IVertex v = GetEdgeByLocation(dndStartPoint);
                    if (v != null)
                        dndVertex.AddEdge(null, v);
                }

                if (dndVertex.Count() > 0)
                {
                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);

                    e.Handled = true;
                }

                isDraggin = false;
            }

           // e.Handled = true;
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            IVertex v = GetEdgeByLocation(e.GetPosition(this));

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(@"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))
                v = Vertex.Get("BaseEdge:");

            if (v != null)
                Dnd.DoDrop(null, v.Get("To:"), e);

            e.Handled = true;
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }

    }
}

