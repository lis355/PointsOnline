using System;
using System.Windows;
using System.Windows.Controls;

namespace PointsOnline
{
    public partial class Game 
    {
        public Game()
        {
            InitializeComponent();
        }

        Canvas _canvasWithPoints;
        Canvas _canvasWithRegions;
        
        private void Game_OnLoaded(object sender, RoutedEventArgs e)
        {
            _canvasWithPoints = (Canvas)this.FindByUid("canvasWithPoints");
            _canvasWithRegions = (Canvas)this.FindByUid("canvasWithRegions");

            LoadPoints();
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
                    _canvasWithPoints.Children.Add(p);
                }
            }
        }
    }
}
