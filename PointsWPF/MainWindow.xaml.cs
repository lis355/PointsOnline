using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PointsOnline
{
    public partial class MainWindow
    {
        Canvas _canvas;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            _canvas = sender as Canvas;
        }

        private void Window_Rendered(object sender, EventArgs e)
        {
            // TMP

            LoadPoints();
            SaveDataManager.Instance.Load();
        }

        private void LoadPoints()
        {
            int diameter = (int)(double)Application.Current.FindResource("ShapeDiameter");
            int offset = (int)(diameter / 2.0);

            for (int i = 0; i < 11; ++i)
            {
                for (int j = 0; j < 11; ++j)
                {
                    var p = new PlanePoint();
                    Canvas.SetLeft(p, i * (diameter + offset));
                    Canvas.SetTop(p, j * (diameter + offset));
                    _canvas.Children.Add(p);
                }
            }
        }
    }
}
