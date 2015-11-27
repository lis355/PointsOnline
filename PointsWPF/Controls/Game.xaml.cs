using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
        Vector _coffset;
        double _pointRadius;
        double _pointOffset;

        private void Game_OnLoaded(object sender, RoutedEventArgs e)
        {
            _canvasWithPoints = (Canvas)this.FindByUid("canvasWithPoints");
            _canvasWithRegions = (Canvas)this.FindByUid("canvasWithRegions");

            _pointRadius = (double)Application.Current.FindResource("ShapeDiameter") / 2.0;
            _pointOffset = (double)Application.Current.FindResource("ShapeOffset");

            _coffset.X = _canvasWithPoints.ActualWidth / 2;
            _coffset.Y = _canvasWithPoints.ActualHeight / 2;

            ScrollToCenter();

            var pr = new [] 
            {
                new IntPoint(0, 0),
                new IntPoint(1, 0),
                new IntPoint(1, 1)
            };

            LoadRedPoints(pr);
            LoadRedRegion(pr);

            LoadRedPoints(new[] { new IntPoint(0, 2) });

            var pb = new[]
            {
                new IntPoint(1, 2),
                new IntPoint(1, 3),
                new IntPoint(0, 3),
                new IntPoint(-1, 3),
                new IntPoint(-1, 2),
                new IntPoint(-1, 1),
                new IntPoint(-1, 0),
                new IntPoint(0, 1)
            };

            LoadBluePoints(pb);
            LoadBlueRegion(pb);
        }

        private void ScrollToCenter()
        {
            var offset = new Vector(
                (_canvasWithPoints.ActualWidth - scroll.ActualWidth) / 2.0,
                (_canvasWithPoints.ActualHeight - scroll.ActualHeight) / 2.0);

            scroll.SnapContentOffsetTo(offset);
        }

        private void LoadRedPoints(IEnumerable<IntPoint> points)
        {
            LoadPoints(points, (SolidColorBrush)FindResource("RedTeamPointBrush"));
        }

        private void LoadBluePoints(IEnumerable<IntPoint> points)
        {
            LoadPoints(points, (SolidColorBrush)FindResource("BlueTeamPointBrush"));
        }

        private void LoadPoints(IEnumerable<IntPoint> points, Brush fillBrush)
        {
            Path p = new Path();
            p.Fill = fillBrush;

            _canvasWithPoints.Children.Add(p);

            GeometryGroup g = new GeometryGroup();
            p.Data = g;

            foreach (var pt in points)
            {
                EllipseGeometry e = new EllipseGeometry(GetPointCanvasCoordinatesFromIndex(pt.X, pt.Y),
                    _pointRadius, _pointRadius);
                g.Children.Add(e);
            }
        }

        private void LoadRedRegion(IEnumerable<IntPoint> points)
        {
            LoadRegion(points,
                (SolidColorBrush)FindResource("RedTeamRegionBrush"),
                (SolidColorBrush)FindResource("RedTeamPointBrush"));
        }

        private void LoadBlueRegion(IEnumerable<IntPoint> points)
        {
            LoadRegion(points,
                (SolidColorBrush)FindResource("BlueTeamRegionBrush"),
                (SolidColorBrush)FindResource("BlueTeamPointBrush"));
        }

        private void LoadRegion(IEnumerable<IntPoint> points, Brush fillBrush, Brush borderBrush)
        {
            Path p = new Path();
            p.Fill = fillBrush;
            p.Stroke = borderBrush;
            p.StrokeThickness = _pointOffset;
            p.StrokeLineJoin = PenLineJoin.Round;

            _canvasWithRegions.Children.Add(p);

            var pg = new PathGeometry();
            p.Data = pg;

            var pf = new PathFigure();
            pg.Figures.Add(pf);

            pf.IsClosed = true;

            bool isFirst = false;

            foreach (var pt in points)
            {
                if (!isFirst)
                {
                    pf.StartPoint = GetPointCanvasCoordinatesFromIndex(pt.X, pt.Y);
                    isFirst = true;
                }
                else
                {
                    pf.Segments.Add(new LineSegment(GetPointCanvasCoordinatesFromIndex(pt.X,pt.Y), true));
                }
            }
        }

        private Point GetPointCanvasCoordinatesFromIndex(int i, int j)
        {
            return new Point(
                _coffset.X + i * (2 * _pointRadius + _pointOffset),
                _coffset.Y + j * (2 * _pointRadius + _pointOffset));
        }
    }
}
