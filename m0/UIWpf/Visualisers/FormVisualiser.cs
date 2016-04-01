using m0.Foundation;
using m0.Graph;
using m0.UML;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0.UIWpf.Visualisers
{
    public class ControlInfo
    {
        public FrameworkElement GapControl;
        public FrameworkElement MetaControl;
        public FrameworkElement DataControl;
        public int Column;        
    }

    public class SectionInfo
    {
        public Panel Panel;
        public int Column;
    }

    public class TabInfo
    {
        public int TotalNumberOfControls;
        public int CurrentNumberOfControls;
        public IDictionary<string, SectionInfo> Sections;
        public IDictionary<IVertex, ControlInfo> ControlInfos;
        public TabItem TabItem;

        public TabInfo()
        {
            Sections=new Dictionary<string,SectionInfo>();
            ControlInfos = new Dictionary<IVertex, ControlInfo>();
            TotalNumberOfControls=0;
            CurrentNumberOfControls = 0;
        }
    }

    public class FormVisualiser: ContentControl, IPlatformClass
    {
        bool isLoaded;

        bool SectionsAsTabs;
        bool MetaOnLeft;

        bool HasTabs { get; set; }
        int ColumnNumber { get; set; }
        IDictionary<string, TabInfo> TabList { get; set; }

        TabControl TabControl;

        double marginOnRight = 1;
        double marginBetweenColumns = 5;
        double sectionControlBorderWidth = 17;
        double metaVsDataSeparator = 4;
        double controlLineVsControlLineSeparator = 4;


        private bool isFormTyped()
        {
            return Vertex.Get(@"BaseEdge:\Meta:") == null || Vertex.Get(@"BaseEdge:\Meta:").Count() == 0;
        }

        private string getGroup(IVertex meta)
        {
            if (SectionsAsTabs){
                string _section = (string)GraphUtil.GetValue(meta.Get("$Section:")); 
                string _group = (string)GraphUtil.GetValue(meta.Get("$Group:"));

                if(_group==null && _section==null)
                    return "";

                if (_group == null)
                    return "| " + _section;

                if (_section == null)
                    return _group;

                return _group + " | " + _section;
            }
            else
            {
                string _group=(string)GraphUtil.GetValue(meta.Get("$Group:"));

                if (_group == null)
                    return "";
                else
                    return _group;
            }
        }

        private string getSection(IVertex meta)
        {
            if (SectionsAsTabs)
                return null;
            else
                return (string)GraphUtil.GetValue(meta.Get("$Section:")); 
        }

        private void PreFillFormAnalyseEdge(IVertex meta, bool isSet)
        {
            string group = getGroup(meta);
            string section = getSection(meta);      

            TabInfo t;

            if (group == null || group=="")
                HasTabs = false;
            else
                HasTabs = true;

            if (TabList.ContainsKey(group))
                t = TabList[group];
            else
            {
                t = new TabInfo();
                TabList.Add(group, t);
            }

            //if(isSet==false)
                t.TotalNumberOfControls++;

        }

        private void PreFillForm()
        {
            TabList = new Dictionary<string, TabInfo>();

            IVertex basTo = Vertex.Get(@"BaseEdge:\To:");

            if (isFormTyped()) // if Form is not typed
            {
                IList<IVertex> visited = new List<IVertex>();

                foreach (IEdge e in basTo)
                {
                    if (!visited.Contains(e.Meta) && e.Meta.Get("$Hide:") == null)
                        if (basTo.GetAll(e.Meta + ":").Count() > 1)
                        {
                            PreFillFormAnalyseEdge(e.Meta, true);
                            visited.Add(e.Meta);
                        }
                        else
                            PreFillFormAnalyseEdge(e.Meta, false);
                }
            }
            else // Form is typed
            {
                foreach (IEdge e in VertexOperations.GetChildEdges(Vertex.Get(@"BaseEdge:\Meta:")))
                    if (e.To.Get("$Hide:") == null)
                        if (GraphUtil.GetIntegerValue(e.To.Get("$MaxCardinality:")) > 1 || GraphUtil.GetIntegerValue(e.To.Get("$MaxCardinality:")) == -1)
                            PreFillFormAnalyseEdge(e.To, true);
                        else
                            PreFillFormAnalyseEdge(e.To, false);
            }
        }

        

        public void UpdateBaseEdge()
        {
            if (!isLoaded)
                return;

            IVertex basTo = Vertex.Get(@"BaseEdge:\To:");            

            if (basTo != null)
            {
                if ((string)Vertex.Get(@"SectionsAsTabs:").Value == "True")
                    SectionsAsTabs = true;
                else
                    SectionsAsTabs = false;

                if ((string)Vertex.Get(@"MetaOnLeft:").Value == "True")
                    MetaOnLeft = true;
                else
                    MetaOnLeft = false;

                ColumnNumber=GraphUtil.GetIntegerValue(Vertex.Get(@"ColumnNumber:"));

                PreFillForm();

                InitializeControlContent();

                
                if (isFormTyped()) // if Form is not typed
                {
                    IList<IVertex> visited = new List<IVertex>();

                    foreach (IEdge e in basTo)
                    {
                        if (!visited.Contains(e.Meta)&&e.Meta.Get("$Hide:") == null)
                            if (basTo.GetAll(e.Meta + ":").Count() > 1)
                            {
                                AddEdge(e.Meta, true);
                                visited.Add(e.Meta);
                            }
                            else
                                AddEdge(e.Meta, false);
                    }
                }
                else // Form is typed
                {                    
                    foreach (IEdge e in VertexOperations.GetChildEdges(Vertex.Get(@"BaseEdge:\Meta:")))
                        if (e.To.Get("$Hide:") == null)
                            if (GraphUtil.GetIntegerValue(e.To.Get("$MaxCardinality:")) > 1 || GraphUtil.GetIntegerValue(e.To.Get("$MaxCardinality:")) == -1)
                                AddEdge(e.To, true);
                            else
                                AddEdge(e.To, false);
                }

               
            }
        }

        protected void CorrectWidth(TabInfo i)
        {            
            double oneColumnWidth = ((this.ActualWidth - marginOnRight) / ColumnNumber) - marginBetweenColumns;
                      
                    double[] maxMetaWidthInColumn = new double[ColumnNumber];

                    foreach (ControlInfo ci in i.ControlInfos.Values)
                        if (ci.MetaControl.ActualWidth > maxMetaWidthInColumn[ci.Column])
                            maxMetaWidthInColumn[ci.Column] = ci.MetaControl.ActualWidth;

            
            if(i.Sections.Count()==0)
            foreach (KeyValuePair<IVertex, ControlInfo> ci in i.ControlInfos) // if there are no sections
                    {                       
                        ci.Value.MetaControl.Width = maxMetaWidthInColumn[ci.Value.Column];
                        ci.Value.GapControl.Width = 0;
                        
                        ci.Value.DataControl.Width = oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - 5;                                  
                    }
            else
                foreach (KeyValuePair<IVertex, ControlInfo> ci in i.ControlInfos) // if there are sections
                {
                    ci.Value.MetaControl.Width = maxMetaWidthInColumn[ci.Value.Column];

                    if (getSection(ci.Key) == null)
                    {
                        ci.Value.GapControl.Width = (sectionControlBorderWidth / 2) - 2;
                        ci.Value.DataControl.Width = oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - 9 - sectionControlBorderWidth / 2;
                    }
                    else
                    {
                        ci.Value.GapControl.Width = 0;
                        ci.Value.DataControl.Width = oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - sectionControlBorderWidth;
                    }

                }

            
        }

        protected object CreateColumnedContent()
        {
            Grid g = new Grid();

            bool notFirstColumn = false;

            int columnCount = 0;

            for (int i = 0; i < ColumnNumber; i++)
            {
                if (notFirstColumn)
                {
                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(marginBetweenColumns);
                    g.ColumnDefinitions.Add(cd);

                    columnCount++;
                }
                
                g.ColumnDefinitions.Add(new ColumnDefinition());
                StackPanel s = new StackPanel();
                Grid.SetColumn(s, columnCount);
                g.Children.Add(s);

                columnCount++;

                notFirstColumn = true;
            }

            ColumnDefinition cdd = new ColumnDefinition();
            cdd.Width = new GridLength(marginOnRight);
            g.ColumnDefinitions.Add(cdd);

            columnCount++;

            return g;
        }

        private void InitializeControlContent()
        {               
            if (HasTabs)
            {
                TabControl = new TabControl();

                Content = TabControl;

                foreach (KeyValuePair<string,TabInfo> t in TabList)
                {
                    TabItem i = new TabItem();
                    i.Header = t.Key ;
                    TabControl.Items.Add(i);
                    t.Value.TabItem = i;
                    i.Tag = t.Value;

                    if(MetaOnLeft)
                        i.SizeChanged += tabItem_SizeChanged;
                           

                    i.Content = CreateColumnedContent();
                }
            }
            else
                Content = CreateColumnedContent();
        }

        private void tabItem_SizeChanged(object sender, EventArgs e)
        {
            TabItem i = (TabItem)sender;
            TabInfo t = (TabInfo)i.Tag;

            CorrectWidth(t);
        }

        protected Panel GetUIPlace(string group,string section, ControlInfo ci)
        {
            TabInfo t = TabList[group];

            
            int targetColumn = (int)((double)t.CurrentNumberOfControls * (double)ColumnNumber / (double)t.TotalNumberOfControls);

            if (targetColumn >= ColumnNumber)
                targetColumn = ColumnNumber-1;

            t.CurrentNumberOfControls++;

            ci.Column = targetColumn;

            if (section != null)
            {
                if (t.Sections.ContainsKey(section))
                {
                    ci.Column = t.Sections[section].Column;

                    return ((Panel)t.Sections[section].Panel);
                }

                Panel toAdd;

                if (HasTabs)
                    toAdd=(Panel)((Grid)t.TabItem.Content).Children[targetColumn];
                else
                    toAdd=(Panel)((Grid)this.Content).Children[0];

                GroupBox g = new GroupBox();

                //Expander g = new Expander();

                g.BorderBrush = (Brush)FindResource("0ForegroundBrush");

                toAdd.Children.Add(g);

                Border b = new Border(); // separator

                b.BorderThickness = new System.Windows.Thickness(0, controlLineVsControlLineSeparator, 0, 0);

                toAdd.Children.Add(b);


                g.Header = section;

                StackPanel gp = new StackPanel();

                g.Content = gp;

                SectionInfo si = new SectionInfo();
                si.Panel = gp;
                si.Column = targetColumn;

                t.Sections.Add(section, si);

                return gp;
            }


            if(HasTabs)
                return (Panel)((Grid)t.TabItem.Content).Children[targetColumn];
            else
                return (Panel)((Grid)this.Content).Children[targetColumn];
        }

        protected void AddEdge(IVertex meta, bool isSet)
        {
            string group = getGroup(meta);
            string section = getSection(meta);  

            IVertex r = MinusZero.Instance.Root;

            TextBlock metaControl = new TextBlock();
            metaControl.Text = (string)meta.Value;
            metaControl.Foreground = (Brush)FindResource("0GrayBrush");
            metaControl.FontStyle = FontStyles.Italic;

            Control dataControl=null;
                       
            if(isSet)
            {
                TableVisualiser tv = new TableVisualiser();
                GraphUtil.ReplaceEdge(tv.Vertex.Get("BaseEdge:"),"To", Vertex.Get(@"BaseEdge:\To:"));

                GraphUtil.CreateOrReplaceEdge(tv.Vertex.Get("ToShowEdgesMeta:"), r.Get(@"System\Meta\ZeroTypes\Edge\Meta"), meta);
                //GraphUtil.CreateOrReplaceEdge(tv.Vertex.Get("ToShowEdgesMeta:"), r.Get(@"System\Meta\ZeroTypes\Edge\To"), e.To); // do not need

                dataControl = tv;
            }
            else
            {
                VisualiserEditWrapper w = new VisualiserEditWrapper();
                IEdge e=Vertex.GetAll(@"BaseEdge:\To:\"+(string)meta.Value+":").FirstOrDefault();

                if (e == null) // no edge in data vertex
                {
                    w.BaseEdge = new EasyEdge(Vertex.Get(@"BaseEdge:\To:"), meta, null);
                }
                else
                    w.BaseEdge = e;

                dataControl = w;
            }

            ControlInfo ci = new ControlInfo();

            ci.MetaControl = metaControl;
            ci.DataControl = dataControl;
            
            TabList[group].ControlInfos.Add(meta, ci);

            Panel place = GetUIPlace(group,section,ci);



            if (MetaOnLeft)
            {                
                metaControl.TextAlignment = TextAlignment.Right;

                StackPanel s=new StackPanel();
                s.Orientation=Orientation.Horizontal;

                ci.GapControl = new StackPanel();
                
                s.Children.Add(ci.GapControl);

                s.Children.Add(metaControl);

                Border b2 = new Border();

                b2.BorderThickness = new System.Windows.Thickness(metaVsDataSeparator, 0, 0, 0);

                s.Children.Add(b2);

                s.Children.Add(dataControl);

                place.Children.Add(s);
            }
            else
            {
                place.Children.Add(metaControl);

                place.Children.Add(dataControl);
            }


            Border b = new Border();

            b.BorderThickness = new System.Windows.Thickness(0, controlLineVsControlLineSeparator, 0, 0);

            place.Children.Add(b);
        }

        public FormVisualiser(){
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.CreateTempVertex();

                Vertex.Value = "FormVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributes(Vertex, mz.Root.Get(@"System\Meta\Visualiser\FormVisualiser"));

                ClassVertex.AddIsClassAndAllAttributes(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));

                SetVertexDefaultValues();

                this.Loaded += new RoutedEventHandler(OnLoad);

                // DO NOT WANT CONTEXTMENU HERE
                // this.ContextMenu = new m0ContextMenu(this);
            }

        }

        protected virtual void SetVertexDefaultValues()
        {
            Vertex.Get("ZoomVisualiserContent:").Value = 100;
            Vertex.Get("ColumnNumber:").Value = 1;
            Vertex.Get("SectionsAsTabs:").Value = "False";
            Vertex.Get("MetaOnLeft:").Value = "False";  
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            isLoaded = true;

            Vertex.Get("ColumnNumber:").Value = (int)this.ActualWidth/300;
        }   


        protected void ChangeZoomVisualiserContent()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get("ZoomVisualiserContent:"))) / 100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }   

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();

            if ((sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))
                UpdateBaseEdge();

            if (sender == Vertex.Get(@"BaseEdge:\To:") && (/*e.Type == VertexChangeType.EdgeAdded ||*/ e.Type == VertexChangeType.EdgeRemoved))
                UpdateBaseEdge();

            if (sender == Vertex.Get(@"ColumnNumber:") && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

            if (sender == Vertex.Get(@"SectionsAsTabs:") && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

            if (sender == Vertex.Get(@"MetaOnLeft:") && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

            if (sender == Vertex.Get("ZoomVisualiserContent:") && e.Type == VertexChangeType.ValueChanged)
                ChangeZoomVisualiserContent();
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                if (_Vertex != null)
                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                UpdateBaseEdge();
            }
        }
    }
}
