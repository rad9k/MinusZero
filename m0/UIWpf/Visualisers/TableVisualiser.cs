﻿using System;
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
    public class TableVisualiser : ListVisualiser
    {
        protected override void AddFooter()
        {
            m0.UIWpf.Visualisers.Controls.NewButton button = new m0.UIWpf.Visualisers.Controls.NewButton();

            button.HorizontalAlignment = HorizontalAlignment.Left;

            this.Children.Add(button);
        }

        protected override void CreateView(){
            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get("AlternatingRows:"), "True"))
                this.ThisDataGrid.AlternatingRowBackground = (Brush)FindResource("0AlternatingBackgroundBrush");
            else
                this.ThisDataGrid.AlternatingRowBackground = (Brush)FindResource("0BackgroundBrush");

            ThisDataGrid.Columns.Clear();

            AddColumn("", ""); // Vertex level column

            AddDeleteTemplateButton();
            AddInfoTemplateButton();  

            if(ToShowEdgesMeta!=null)
            //foreach (IEdge e in ToShowEdgesMeta) // Old approach
            foreach(IEdge e in VertexOperations.GetChildEdges(ToShowEdgesMeta))
                if (e.To.Get("$Hide:") == null)
                AddColumn((string)e.To.Value, "To[" + (string)e.To.Value+"]");

           
        }

        protected virtual void AddDeleteTemplateButton()
        {
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn(); // DELETE

            valueColumn.CellStyle = (Style)FindResource("0ListValueColumn");

            valueColumn.CellTemplate = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Controls.DeleteButton));
            factory.SetBinding(Controls.DeleteButton.BaseEdgeProperty, new Binding(""));
            valueColumn.CellTemplate.VisualTree = factory;

            ThisDataGrid.Columns.Add(valueColumn);
        }

        protected virtual void AddInfoTemplateButton()
        {
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn(); //INFO

            valueColumn.CellStyle = (Style)FindResource("0ListValueColumn");

            valueColumn.CellTemplate = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Controls.InfoButton));
            factory.SetBinding(Controls.InfoButton.BaseEdgeProperty, new Binding(""));
            valueColumn.CellTemplate.VisualTree = factory;

            ThisDataGrid.Columns.Add(valueColumn);
        }

       protected virtual void AddColumn(string columnName, string bindingString)
        {
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn();

            valueColumn.CellStyle = (Style)FindResource("0ListValueColumn");

            //
            // CELL TEMPLATE
            //

            if (GeneralUtil.CompareStrings(Vertex.Get("IsAllVisualisersEdit:").Value, "True"))
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
                factory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new Binding(bindingString));
                valueColumn.CellTemplate.VisualTree = factory;
            }
            else
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserViewWrapper));
                factory.SetBinding(VisualiserViewWrapper.BaseEdgeProperty, new Binding(bindingString));
                valueColumn.CellTemplate.VisualTree = factory;
            }
            
            //
            // EDIT TEMPLATE
            //
            valueColumn.CellEditingTemplate = new DataTemplate();
            FrameworkElementFactory EditFactory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
            EditFactory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new Binding(bindingString));
            valueColumn.CellEditingTemplate.VisualTree = EditFactory;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get("ShowHeader:"), "True"))
                valueColumn.Header = columnName + " ";

            ThisDataGrid.Columns.Add(valueColumn);
        }

        protected override void SetVertexDefaultValues()
        {
            //Vertex.Get("IsMetaRightAlign:").Value = "False";
            Vertex.Get("IsAllVisualisersEdit:").Value = "False";
            Vertex.Get("ShowHeader:").Value = "True";
            Vertex.Get("AlternatingRows:").Value = "True";
            Vertex.Get("ZoomVisualiserContent:").Value = 100;

            GraphUtil.ReplaceEdge(Vertex, "GridStyle", MinusZero.Instance.Root.Get(@"System\Meta\Visualiser\GridStyleEnum\Round"));
        }

        protected override void PlatformClassInitialize(){
            MinusZero mz = MinusZero.Instance;

            //Vertex = mz.Root.Get(@"System\Session\Visualisers").AddVertex(null, "ListVisualiser" + this.GetHashCode());

            Vertex = mz.CreateTempVertex();
            Vertex.Value = "TableVisualiser" + this.GetHashCode();

            ClassVertex.AddIsClassAndAllAttributes(Vertex, mz.Root.Get(@"System\Meta\Visualiser\TableVisualiser"));

            ClassVertex.AddIsClassAndAllAttributes(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));

            //ClassVertex.AddIsClassAndAllAttributes(Vertex.Get("ToShowEdgesMeta:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));            

        }

        

        IVertex ToShowEdgesMeta;

        protected override void UpdateBaseEdge(){
            IVertex bas = Vertex.Get(@"BaseEdge:\To:");

            if (bas != null)
            {                
                ToShowEdgesMeta = null;

                if (Vertex.Get(@"ToShowEdgesMeta:\Meta:") != null)
                    ToShowEdgesMeta = Vertex.Get(@"ToShowEdgesMeta:\Meta:");

                if (ToShowEdgesMeta == null) // take first edge from BaseEdge\To, to have Meta as ToShowEdesMeta:\Meta:==null
                {
                    IEdge e=bas.FirstOrDefault();

                    if (e != null)
                        ToShowEdgesMeta = e.Meta;
                }

                if (ToShowEdgesMeta != null)
                {
                    ((EasyVertex)Vertex.Get(@"FilterQuery:")).CanFireChangeEvent = false;

                    Vertex.Get(@"FilterQuery:").Value = ToShowEdgesMeta.Value+":";

                    ((EasyVertex)Vertex.Get(@"FilterQuery:")).CanFireChangeEvent = true;
                }


                if (Vertex.Get(@"FilterQuery:") != null&&Vertex.Get(@"FilterQuery:").Value!=null) // do the filtering
                {
                    IVertex data=VertexOperations.DoFilter(bas, Vertex.Get(@"FilterQuery:"));

                    if (data != null)
                        ThisDataGrid.ItemsSource = data.ToList();
                    else
                        ThisDataGrid.ItemsSource = null;
                }
                else
                    ThisDataGrid.ItemsSource = bas.ToList(); // if there is no .ToList DataGrid can not edit

                ResetView();
            }           
        }


        protected override void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge"))
                 || (sender == Vertex.Get("BaseEdge:") && e.Type == VertexChangeType.ValueChanged)
                || ((sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && ((GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))))
                UpdateBaseEdge();

            if (sender == Vertex.Get(@"BaseEdge:\To:") && (e.Type == VertexChangeType.EdgeAdded || e.Type == VertexChangeType.EdgeRemoved))
                UpdateBaseEdge();

            if (sender == Vertex.Get(@"ToShowEdgesMeta:") && (e.Type == VertexChangeType.EdgeAdded || e.Type == VertexChangeType.EdgeRemoved))
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

            if (sender == Vertex.Get("AlternatingRows:") && e.Type == VertexChangeType.ValueChanged)
                ResetView();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "ShowHeader")))
                ResetView();

        }       

        
    }
}
