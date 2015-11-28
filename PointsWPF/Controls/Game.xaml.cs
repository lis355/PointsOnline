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
            CapturedPoints,
            RegionAndBorders,
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
            _pointRadius = (double)Application.Current.FindResource("ShapeDiameter") / 2.0;
            _pointOffset = (double)Application.Current.FindResource("ShapeOffset");

            InitializeLayers();
            SetupLayers();

            _coffset.X = GetLayer(ELayers.ActivePoints).ActualWidth / 2;
            _coffset.Y = GetLayer(ELayers.ActivePoints).ActualHeight / 2;

            LoadFieldFromSave();

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

        private void LoadFieldFromSave()
        {
            var data = PointsSaveDataManager.Instance.Data;

            LoadRedPoints(data.RedPlayer.ActivePoints, GetLayer(ELayers.ActivePoints));
            LoadRedPoints(data.RedPlayer.CapturedPoints, GetLayer(ELayers.CapturedPoints));
            LoadRedRegions(data.RedPlayer.Regions);
            LoadBluePoints(data.BluePlayer.ActivePoints, GetLayer(ELayers.ActivePoints));
            LoadBluePoints(data.BluePlayer.CapturedPoints, GetLayer(ELayers.CapturedPoints));
            LoadBlueRegions(data.BluePlayer.Regions);
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

        private void LoadRedRegions(IEnumerable<Region> regions)
        {
            var rb = (SolidColorBrush)FindResource("RedTeamRegionBrush");
            var pb = (SolidColorBrush)FindResource("RedTeamPointBrush");

            foreach (var r in regions)
            {
                LoadRegion(r.Border, rb, pb);
                LoadRedPoints(r.Points, GetLayer(ELayers.RegionAndBorders));
            }
        }

        private void LoadBlueRegions(IEnumerable<Region> regions)
        {
            var rb = (SolidColorBrush)FindResource("BlueTeamRegionBrush");
            var pb = (SolidColorBrush)FindResource("BlueTeamPointBrush");

            foreach (var r in regions)
            {
                LoadRegion(r.Border, rb, pb);
                LoadBluePoints(r.Points, GetLayer(ELayers.RegionAndBorders));
            }
        }

        private void LoadRegion(IEnumerable<IntPoint> points, Brush fillBrush, Brush borderBrush)
        {
            Path p = new Path();
            p.Fill = fillBrush;
            p.Stroke = borderBrush;
            p.StrokeThickness = _pointOffset;
            p.StrokeLineJoin = PenLineJoin.Round;

            GetLayer(ELayers.RegionAndBorders).Children.Add(p);

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
            
            p.MouseEnter += (s, e) =>
            {
                StartFadeAnimation(p, 0.2, 0.3);
            };

            p.MouseLeave += (s, e) =>
            {
                StartFadeAnimation(p, 0.2, 1);
            };
        }

        private Point GetPointCanvasCoordinatesFromIndex(IntPoint p)
        {
            return new Point(
                _coffset.X + p.X * (2 * _pointRadius + _pointOffset),
                _coffset.Y + p.Y * (2 * _pointRadius + _pointOffset));
        }

        private IntPoint GetPointIndexFromCanvasCoordinates(Point p)
        {
            var r = new IntPoint();
            double f = 2 * _pointRadius + _pointOffset;

            if (p.X > _coffset.X)
            {
                r.X = (int)(0.5 + (p.X - _coffset.X) / f);
            }
            else
            {
                r.X = (int)(-0.5 + (p.X - _coffset.X) / f);
            }

            if (p.Y > _coffset.Y)
            {
                r.Y = (int)(0.5 + (p.Y - _coffset.Y) / f);
            }
            else
            {
                r.Y = (int)(-0.5 + (p.Y - _coffset.Y) / f);
            }

            return r;
        }

        private void ScrollBox_MouseMove(object sender, MouseEventArgs e)
        {
            ShowGrid();

            var cpos = e.GetPosition(GetLayer(ELayers.ActivePoints));
            var indexes = GetPointIndexFromCanvasCoordinates(cpos);
            var centerCoord = GetPointCanvasCoordinatesFromIndex(indexes);

            UIElement shape = null;

            // сделаем проверку на попадание в радиус т.к. indexes - координаты квадрата
            if (_pointRadius >= Math.Sqrt((cpos.X - centerCoord.X) * (cpos.X - centerCoord.X)
                + (cpos.Y - centerCoord.Y) * (cpos.Y - centerCoord.Y)))
            {
                shape = GetLayer(ELayers.GrayPoints).InputHitTest(centerCoord) as UIElement;
                if (shape == null)
                {
                    // TEMP
                    shape = new PlanePoint();
                    Canvas.SetLeft(shape, centerCoord.X);
                    Canvas.SetTop(shape, centerCoord.Y);
                    GetLayer(ELayers.GrayPoints).Children.Add(shape);
                }

                StartFadeOutWaitFadeInAnimation(shape, 0.1, 1, 0.5);
            }
        }

        private void ScrollBox_Dragged(object sender, RoutedEventArgs e)
        {
            ShowGrid();
        }

        private void ShowGrid()
        {
            StartFadeOutWaitFadeInAnimation(GetLayer(ELayers.Grid), 0.5, 1, 0.5);
        }
        
        private void StartFadeOutWaitFadeInAnimation(UIElement element, double outTime, double waitTime, double inTime)
        {
            var storyboard = new Storyboard();

            var animation = new DoubleAnimation();
            storyboard.Children.Add(animation);
            animation.To = 1;
            animation.Duration = TimeSpan.FromSeconds(outTime);

            animation = new DoubleAnimation();
            storyboard.Children.Add(animation);
            animation.To = 1;
            animation.Duration = TimeSpan.FromSeconds(waitTime);
            animation.BeginTime = TimeSpan.FromSeconds(outTime);

            animation = new DoubleAnimation();
            storyboard.Children.Add(animation);
            animation.To = 0;
            animation.Duration = TimeSpan.FromSeconds(inTime);
            animation.BeginTime = TimeSpan.FromSeconds(outTime + waitTime);

            Storyboard.SetTarget(storyboard, element);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(OpacityProperty));

            //storyboard.FillBehavior = FillBehavior.HoldEnd;
            //storyboard.Completed += (s, e) =>
            //{
            //    storyboard.Remove();
            //};

            storyboard.Begin();
        }

        private void StartFadeAnimation(UIElement element, double time, double value)
        {
            var storyboard = new Storyboard();

            var animation = new DoubleAnimation();
            storyboard.Children.Add(animation);
            animation.To = value;
            animation.Duration = TimeSpan.FromSeconds(time);

            Storyboard.SetTarget(storyboard, element);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(OpacityProperty));
            storyboard.Begin();
        }
    }
}
