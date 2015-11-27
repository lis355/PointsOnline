using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        Grid _canvasWithGrid;
        Vector _coffset;
        double _pointRadius;
        double _pointOffset;

        private void Game_OnLoaded(object sender, RoutedEventArgs e)
        {
            _canvasWithPoints = (Canvas)this.FindByUid("canvasWithPoints");
            _canvasWithRegions = (Canvas)this.FindByUid("canvasWithRegions");
            _canvasWithGrid = (Grid)this.FindByUid("canvasWithGrid");

            _pointRadius = (double)Application.Current.FindResource("ShapeDiameter") / 2.0;
            _pointOffset = (double)Application.Current.FindResource("ShapeOffset");

            _coffset.X = _canvasWithPoints.ActualWidth / 2;
            _coffset.Y = _canvasWithPoints.ActualHeight / 2;

            //PointsSaveDataManager.Instance.Data.RedPlayer.ActivePoints.AddRange(
            //new[]
            //{
            //    new IntPoint(0, 0),
            //    new IntPoint(1, 0),
            //    new IntPoint(1, 1)
            //});
            //
            //PointsSaveDataManager.Instance.Data.RedPlayer.CapturedPoints.AddRange(
            //new[]
            //{
            //    new IntPoint(0, 2)
            //});
            //
            //PointsSaveDataManager.Instance.Data.RedPlayer.Regions.Add( new GameSaveData.Region(
            //new[]
            //{
            //    new IntPoint(0, 0),
            //    new IntPoint(1, 0),
            //    new IntPoint(1, 1)
            //}));
            //
            //PointsSaveDataManager.Instance.Data.BluePlayer.ActivePoints.AddRange(
            //new[]
            //{
            //    new IntPoint(1, 2),
            //    new IntPoint(1, 3),
            //    new IntPoint(0, 3),
            //    new IntPoint(-1, 3),
            //    new IntPoint(-1, 2),
            //    new IntPoint(-1, 1),
            //    new IntPoint(-1, 0),
            //    new IntPoint(0, 1)
            //});
            //
            //PointsSaveDataManager.Instance.Data.BluePlayer.Regions.Add(new GameSaveData.Region(
            //new[]
            //{
            //    new IntPoint(1, 2),
            //    new IntPoint(1, 3),
            //    new IntPoint(0, 3),
            //    new IntPoint(-1, 3),
            //    new IntPoint(-1, 2),
            //    new IntPoint(-1, 1),
            //    new IntPoint(-1, 0),
            //    new IntPoint(0, 1)
            //}));
            
            var data = PointsSaveDataManager.Instance.Data;

            LoadRedPoints(data.RedPlayer.ActivePoints);
            LoadRedPoints(data.RedPlayer.CapturedPoints);
            LoadRedRegions(data.RedPlayer.Regions);
            LoadBluePoints(data.BluePlayer.ActivePoints);
            LoadBluePoints(data.BluePlayer.CapturedPoints);
            LoadBlueRegions(data.BluePlayer.Regions);

            ScrollToCenter();
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

        private void LoadRedRegions(IEnumerable<GameSaveData.Region> regions)
        {
            var rb = (SolidColorBrush)FindResource("RedTeamRegionBrush");
            var pb = (SolidColorBrush)FindResource("RedTeamPointBrush");

            foreach (var r in regions)
            {
                LoadRegion(r.Border, rb, pb);
            }
        }

        private void LoadBlueRegions(IEnumerable<GameSaveData.Region> regions)
        {
            var rb = (SolidColorBrush)FindResource("BlueTeamRegionBrush");
            var pb = (SolidColorBrush)FindResource("BlueTeamPointBrush");

            foreach (var r in regions)
            {
                LoadRegion(r.Border, rb, pb);
            }
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

        private void ScrollBox_MouseMove(object sender, MouseEventArgs e)
        {
            ShowGrid();
        }

        private void ScrollBox_Dragged(object sender, RoutedEventArgs e)
        {
            ShowGrid();
        }

        private void ShowGrid()
        {
            var sb = (Storyboard)FindResource("GridShowStoryboard");
            Storyboard.SetTarget(sb, _canvasWithGrid);
            sb.Begin();
        }
    }
}
