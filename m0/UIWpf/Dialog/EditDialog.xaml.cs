﻿using m0.Foundation;
using m0.ZeroTypes;
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
using System.Windows.Shapes;

namespace m0.UIWpf.Dialog
{
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class EditDialog : Window
    {
        IEdge baseEdge;
        Point _mousePosition;

        void OnLoad(object sender, RoutedEventArgs e)
        {
            UIWpf.SetWindowPosition(this, _mousePosition);
        }

        public EditDialog(IEdge _baseEdge, Point position)
        {
            baseEdge = _baseEdge;

            InitializeComponent();            

            this.Title = "new " + baseEdge.Meta.Value + " dialog";

            Edge.ReplaceEdgeEdges(FormVisuliser.Vertex.Get("BaseEdge:"), baseEdge);

            if (position!=null)
            {
                _mousePosition = position;
                this.Loaded += new RoutedEventHandler(OnLoad);
            }
            else
                Owner = m0Main.Instance;

            ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            baseEdge.To.Value = Name.Text;

            Close();
        }
    }
}
