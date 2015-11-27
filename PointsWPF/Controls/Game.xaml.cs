using System;
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

            _layers = new List<Canvas>();
        }

        Vector _coffset;
        double _pointRadius;
        double _pointOffset;

        enum ELayers
        {
            Grid = 0,
            GrayPoints,
            CapturedAndBorderPoints,
            Regions,
            ActivePoints
        }

        List<Canvas> _layers;

        private Canvas GetLayer(ELayers l)
        {
            return _layers[(int)l];
        }

        private void Game_Initialized(object sender, EventArgs e)
        {
        }

        private void Game_OnLoaded(object sender, RoutedEventArgs e)
        {
            //_canvasWithPoints = (Canvas)this.FindByUid("canvasWithPoints");
            //_canvasWithRegions = (Canvas)this.FindByUid("canvasWithRegions");
            //_canvasWithGrid = (Grid)this.FindByUid("canvasWithGrid");

            _pointRadius = (double)Application.Current.FindResource("ShapeDiameter") / 2.0;
            _pointOffset = (double)Application.Current.FindResource("ShapeOffset");

            InitializeLayers();
            SetupLayers();

            _coffset.X = GetLayer(ELayers.ActivePoints).ActualWidth / 2;
            _coffset.Y = GetLayer(ELayers.ActivePoints).ActualHeight / 2;

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

            LoadRedPoints(data.RedPlayer.ActivePoints, GetLayer(ELayers.ActivePoints));
            LoadRedPoints(data.RedPlayer.CapturedPoints, GetLayer(ELayers.CapturedAndBorderPoints));
            LoadRedRegions(data.RedPlayer.Regions);
            LoadBluePoints(data.BluePlayer.ActivePoints, GetLayer(ELayers.ActivePoints));
            LoadBluePoints(data.BluePlayer.CapturedPoints, GetLayer(ELayers.CapturedAndBorderPoints));
            LoadBlueRegions(data.BluePlayer.Regions);

            ScrollToCenter();
        }

        private void InitializeLayers()
        {
            var rootGrid = (Grid)this.FindByUid("layersContainerGrid");

            foreach (var l in Enum.GetValues(typeof(ELayers)))
            {
                Canvas c = new Canvas();
                rootGrid.Children.Add(c);
                _layers.Add(c);
            }

            UpdateLayout();
        }

        private void SetupLayers()
        {
            GetLayer(ELayers.Grid).Background = (Brush)FindResource("GridBackgroundDrawingBrush");
            GetLayer(ELayers.Grid).IsHitTestVisible = false;
        }

        private void ScrollToCenter()
        {
            var offset = new Vector(
                (GetLayer(ELayers.ActivePoints).ActualWidth - scroll.ActualWidth) / 2.0,
                (GetLayer(ELayers.ActivePoints).ActualHeight - scroll.ActualHeight) / 2.0);

            scroll.SnapContentOffsetTo(offset);
        }

        private void LoadRedPoints(IEnumerable<IntPoint> points, Canvas layer)
        {
            LoadPoints(points, layer, (SolidColorBrush)FindResource("RedTeamPointBrush"));
        }

        private void LoadBluePoints(IEnumerable<IntPoint> points, Canvas layer)
        {
            LoadPoints(points, layer, (SolidColorBrush)FindResource("BlueTeamPointBrush"));
        }

        private void LoadPoints(IEnumerable<IntPoint> points, Canvas layer, Brush fillBrush)
        {
            Path p = new Path();
            p.Fill = fillBrush;

            layer.Children.Add(p);

            GeometryGroup g = new GeometryGroup();
            p.Data = g;

            foreach (var pt in points)
            {
                EllipseGeometry e = new EllipseGeometry(GetPointCanvasCoordinatesFromIndex(pt),
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

            GetLayer(ELayers.Regions).Children.Add(p);

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
                    pf.StartPoint = GetPointCanvasCoordinatesFromIndex(pt);
                    isFirst = true;
                }
                else
                {
                    pf.Segments.Add(new LineSegment(GetPointCanvasCoordinatesFromIndex(pt), true));
                }
            }
        }

        private Point GetPointCanvasCoordinatesFromIndex(IntPoint p)
        {
            return new Point(
                _coffset.X + p.X * (2 * _pointRadius + _pointOffset),
                _coffset.Y + p.Y * (2 * _pointRadius + _pointOffset));
        }

        private IntPoint GetPointIndexFromCanvasCoordinates(Point p)
        {
            return new IntPoint(
                (int)((p.X - _coffset.X) / (2 * _pointRadius + _pointOffset)),
                (int)((p.Y - _coffset.Y) / (2 * _pointRadius + _pointOffset)));
        }

        private void ScrollBox_MouseMove(object sender, MouseEventArgs e)
        {
            ShowGrid();

            var indexes = GetPointIndexFromCanvasCoordinates(e.GetPosition(GetLayer(ELayers.ActivePoints)));
            var centerCoord = GetPointCanvasCoordinatesFromIndex(indexes);
            var shape = GetLayer(ELayers.GrayPoints).InputHitTest(centerCoord);
            if (shape == null)
            {
                var p = new PlanePoint();
                Canvas.SetLeft(p, centerCoord.X);
                Canvas.SetTop(p, centerCoord.Y);
                GetLayer(ELayers.GrayPoints).Children.Add(p);
            }
        }

        private void ScrollBox_Dragged(object sender, RoutedEventArgs e)
        {
            ShowGrid();
        }

        private void ShowGrid()
        {
            var sb = (Storyboard)FindResource("GridShowStoryboard");
            Storyboard.SetTarget(sb, GetLayer(ELayers.Grid));
            sb.Begin();
        }
    }
}
