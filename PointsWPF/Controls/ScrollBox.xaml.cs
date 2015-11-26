using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PointsOnline
{
    public partial class ScrollBox : IScrollInfo
    {
        public ScrollBox()
        {
            InitializeComponent();
        }

        private FrameworkElement content = null;
        private ScaleTransform contentScaleTransform = null;
        private TranslateTransform contentOffsetTransform = null;

        private bool enableContentOffsetUpdateFromScale = false;
        private bool disableScrollOffsetSync = false;
        private bool disableContentFocusSync = false;

        private double constrainedContentViewportWidth = 0.0;
        private double constrainedContentViewportHeight = 0.0;

        private bool canVerticallyScroll = false;
        private bool canHorizontallyScroll = false;
        private Size unScaledExtent = new Size(0, 0);
        private Size viewport = new Size(0, 0);

        private ScrollViewer scrollOwner = null;

        public static readonly DependencyProperty ContentScaleProperty =
                DependencyProperty.Register("ContentScale", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(1.0, ContentScale_PropertyChanged, ContentScale_Coerce));

        public static readonly DependencyProperty MinContentScaleProperty =
                DependencyProperty.Register("MinContentScale", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.01, MinOrMaxContentScale_PropertyChanged));

        public static readonly DependencyProperty MaxContentScaleProperty =
                DependencyProperty.Register("MaxContentScale", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(10.0, MinOrMaxContentScale_PropertyChanged));

        public static readonly DependencyProperty ContentOffsetXProperty =
                DependencyProperty.Register("ContentOffsetX", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.0, ContentOffsetX_PropertyChanged, ContentOffsetX_Coerce));

        public static readonly DependencyProperty ContentOffsetYProperty =
                DependencyProperty.Register("ContentOffsetY", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.0, ContentOffsetY_PropertyChanged, ContentOffsetY_Coerce));

        public static readonly DependencyProperty AnimationDurationProperty =
                DependencyProperty.Register("AnimationDuration", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.25));

        public static readonly DependencyProperty ContentZoomFocusXProperty =
                DependencyProperty.Register("ContentZoomFocusX", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ContentZoomFocusYProperty =
                DependencyProperty.Register("ContentZoomFocusY", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ViewportZoomFocusXProperty =
                DependencyProperty.Register("ViewportZoomFocusX", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ViewportZoomFocusYProperty =
                DependencyProperty.Register("ViewportZoomFocusY", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ContentViewportWidthProperty =
                DependencyProperty.Register("ContentViewportWidth", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ContentViewportHeightProperty =
                DependencyProperty.Register("ContentViewportHeight", typeof(double), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty IsMouseWheelScrollingEnabledProperty =
                DependencyProperty.Register("IsMouseWheelScrollingEnabled", typeof(bool), typeof(ScrollBox),
                                            new FrameworkPropertyMetadata(false));

        public double ContentOffsetX
        {
            get
            {
                return (double)GetValue(ContentOffsetXProperty);
            }
            set
            {
                SetValue(ContentOffsetXProperty, value);
            }
        }

        public event EventHandler ContentOffsetXChanged;

        public double ContentOffsetY
        {
            get
            {
                return (double)GetValue(ContentOffsetYProperty);
            }
            set
            {
                SetValue(ContentOffsetYProperty, value);
            }
        }

        public event EventHandler ContentOffsetYChanged;

        public Vector ContentOffset
        {
            get
            {
                return new Vector(ContentOffsetX, ContentOffsetY);
            }
            set
            {
                ContentOffsetX = value.X;
                ContentOffsetY = value.Y;
            }
        }

        public double ContentScale
        {
            get
            {
                return (double)GetValue(ContentScaleProperty);
            }
            set
            {
                SetValue(ContentScaleProperty, value);
            }
        }

        public event EventHandler ContentScaleChanged;

        public double MinContentScale
        {
            get
            {
                return (double)GetValue(MinContentScaleProperty);
            }
            set
            {
                SetValue(MinContentScaleProperty, value);
            }
        }

        public double MaxContentScale
        {
            get
            {
                return (double)GetValue(MaxContentScaleProperty);
            }
            set
            {
                SetValue(MaxContentScaleProperty, value);
            }
        }

        public double ContentZoomFocusX
        {
            get
            {
                return (double)GetValue(ContentZoomFocusXProperty);
            }
            set
            {
                SetValue(ContentZoomFocusXProperty, value);
            }
        }

        public double ContentZoomFocusY
        {
            get
            {
                return (double)GetValue(ContentZoomFocusYProperty);
            }
            set
            {
                SetValue(ContentZoomFocusYProperty, value);
            }
        }

        public double ViewportZoomFocusX
        {
            get
            {
                return (double)GetValue(ViewportZoomFocusXProperty);
            }
            set
            {
                SetValue(ViewportZoomFocusXProperty, value);
            }
        }

        public double ViewportZoomFocusY
        {
            get
            {
                return (double)GetValue(ViewportZoomFocusYProperty);
            }
            set
            {
                SetValue(ViewportZoomFocusYProperty, value);
            }
        }

        public double AnimationDuration
        {
            get
            {
                return (double)GetValue(AnimationDurationProperty);
            }
            set
            {
                SetValue(AnimationDurationProperty, value);
            }
        }

        public double ContentViewportWidth
        {
            get
            {
                return (double)GetValue(ContentViewportWidthProperty);
            }
            set
            {
                SetValue(ContentViewportWidthProperty, value);
            }
        }

        public double ContentViewportHeight
        {
            get
            {
                return (double)GetValue(ContentViewportHeightProperty);
            }
            set
            {
                SetValue(ContentViewportHeightProperty, value);
            }
        }

        public bool IsMouseWheelScrollingEnabled
        {
            get
            {
                return (bool)GetValue(IsMouseWheelScrollingEnabledProperty);
            }
            set
            {
                SetValue(IsMouseWheelScrollingEnabledProperty, value);
            }
        }

        public void AnimatedZoomTo(double newScale, Rect contentRect)
        {
            AnimatedZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)),
                delegate (object sender, EventArgs e)
                {
                    ContentOffsetX = contentRect.X;
                    ContentOffsetY = contentRect.Y;
                });
        }

        public void AnimatedZoomTo(Rect contentRect)
        {
            double scaleX = ContentViewportWidth / contentRect.Width;
            double scaleY = ContentViewportHeight / contentRect.Height;
            double newScale = ContentScale * Math.Min(scaleX, scaleY);

            AnimatedZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)), null);
        }

        public void ZoomTo(Rect contentRect)
        {
            double scaleX = ContentViewportWidth / contentRect.Width;
            double scaleY = ContentViewportHeight / contentRect.Height;
            double newScale = ContentScale * Math.Min(scaleX, scaleY);

            ZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)));
        }

        public void SnapContentOffsetTo(Point contentOffset)
        {
            CancelAnimation(this, ContentOffsetXProperty);
            CancelAnimation(this, ContentOffsetYProperty);

            ContentOffsetX = contentOffset.X;
            ContentOffsetY = contentOffset.Y;
        }

        public void SnapTo(Point contentPoint)
        {
            CancelAnimation(this, ContentOffsetXProperty);
            CancelAnimation(this, ContentOffsetYProperty);

            ContentOffsetX = contentPoint.X - (ContentViewportWidth / 2);
            ContentOffsetY = contentPoint.Y - (ContentViewportHeight / 2);
        }

        public void AnimatedSnapTo(Point contentPoint)
        {
            double newX = contentPoint.X - (ContentViewportWidth / 2);
            double newY = contentPoint.Y - (ContentViewportHeight / 2);

            StartAnimation(this, ContentOffsetXProperty, newX, AnimationDuration);
            StartAnimation(this, ContentOffsetYProperty, newY, AnimationDuration);
        }

        public void AnimatedZoomAboutPoint(double newContentScale, Point contentZoomFocus)
        {
            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

            CancelAnimation(this, ContentZoomFocusXProperty);
            CancelAnimation(this, ContentZoomFocusYProperty);
            CancelAnimation(this, ViewportZoomFocusXProperty);
            CancelAnimation(this, ViewportZoomFocusYProperty);

            ContentZoomFocusX = contentZoomFocus.X;
            ContentZoomFocusY = contentZoomFocus.Y;
            ViewportZoomFocusX = (ContentZoomFocusX - ContentOffsetX) * ContentScale;
            ViewportZoomFocusY = (ContentZoomFocusY - ContentOffsetY) * ContentScale;

            enableContentOffsetUpdateFromScale = true;

            disableScrollOffsetSync = true;
            disableContentFocusSync = true;

            StartAnimation(this, ContentScaleProperty, newContentScale, AnimationDuration,
                (sender, e) =>
                {
                    enableContentOffsetUpdateFromScale = false;

                    disableScrollOffsetSync = false;
                    disableContentFocusSync = false;

                    ResetViewportZoomFocus();
                });
        }

        public void ZoomAboutPoint(double newContentScale, Point contentZoomFocus)
        {
            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

            double screenSpaceZoomOffsetX = (contentZoomFocus.X - ContentOffsetX) * ContentScale;
            double screenSpaceZoomOffsetY = (contentZoomFocus.Y - ContentOffsetY) * ContentScale;
            double contentSpaceZoomOffsetX = screenSpaceZoomOffsetX / newContentScale;
            double contentSpaceZoomOffsetY = screenSpaceZoomOffsetY / newContentScale;
            double newContentOffsetX = contentZoomFocus.X - contentSpaceZoomOffsetX;
            double newContentOffsetY = contentZoomFocus.Y - contentSpaceZoomOffsetY;

            CancelAnimation(this, ContentScaleProperty);
            CancelAnimation(this, ContentOffsetXProperty);
            CancelAnimation(this, ContentOffsetYProperty);

            ContentScale = newContentScale;
            ContentOffsetX = newContentOffsetX;
            ContentOffsetY = newContentOffsetY;
        }

        public void AnimatedZoomTo(double contentScale)
        {
            Point zoomCenter = new Point(ContentOffsetX + (ContentViewportWidth / 2), ContentOffsetY + (ContentViewportHeight / 2));
            AnimatedZoomAboutPoint(contentScale, zoomCenter);
        }

        public void ZoomTo(double contentScale)
        {
            Point zoomCenter = new Point(ContentOffsetX + (ContentViewportWidth / 2), ContentOffsetY + (ContentViewportHeight / 2));
            ZoomAboutPoint(contentScale, zoomCenter);
        }

        public void AnimatedScaleToFit()
        {
            if (content == null)
            {
                throw new ApplicationException("PART_Content was not found in the ScrollBox visual template!");
            }

            AnimatedZoomTo(new Rect(0, 0, content.ActualWidth, content.ActualHeight));
        }

        public void ScaleToFit()
        {
            if (content == null)
            {
                throw new ApplicationException("PART_Content was not found in the ScrollBox visual template!");
            }

            ZoomTo(new Rect(0, 0, content.ActualWidth, content.ActualHeight));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            content = Template.FindName("ScrollData", this) as FrameworkElement;
            if (content != null)
            {
                contentScaleTransform = new ScaleTransform(ContentScale, ContentScale);

                contentOffsetTransform = new TranslateTransform();
                UpdateTranslationX();
                UpdateTranslationY();
                
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(contentOffsetTransform);
                transformGroup.Children.Add(contentScaleTransform);
                content.RenderTransform = transformGroup;
            }
        }

        private void AnimatedZoomPointToViewportCenter(double newContentScale, Point contentZoomFocus, EventHandler callback)
        {
            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

            CancelAnimation(this, ContentZoomFocusXProperty);
            CancelAnimation(this, ContentZoomFocusYProperty);
            CancelAnimation(this, ViewportZoomFocusXProperty);
            CancelAnimation(this, ViewportZoomFocusYProperty);

            ContentZoomFocusX = contentZoomFocus.X;
            ContentZoomFocusY = contentZoomFocus.Y;
            ViewportZoomFocusX = (ContentZoomFocusX - ContentOffsetX) * ContentScale;
            ViewportZoomFocusY = (ContentZoomFocusY - ContentOffsetY) * ContentScale;

            //
            // When zooming about a point make updates to ContentScale also update content offset.
            //
            enableContentOffsetUpdateFromScale = true;

            StartAnimation(this, ContentScaleProperty, newContentScale, AnimationDuration,
                delegate (object sender, EventArgs e)
                {
                    enableContentOffsetUpdateFromScale = false;

                    if (callback != null)
                    {
                        callback(this, EventArgs.Empty);
                    }
                });

            StartAnimation(this, ViewportZoomFocusXProperty, ViewportWidth / 2, AnimationDuration);
            StartAnimation(this, ViewportZoomFocusYProperty, ViewportHeight / 2, AnimationDuration);
        }

        private void ZoomPointToViewportCenter(double newContentScale, Point contentZoomFocus)
        {
            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

            CancelAnimation(this, ContentScaleProperty);
            CancelAnimation(this, ContentOffsetXProperty);
            CancelAnimation(this, ContentOffsetYProperty);

            ContentScale = newContentScale;
            ContentOffsetX = contentZoomFocus.X - (ContentViewportWidth / 2);
            ContentOffsetY = contentZoomFocus.Y - (ContentViewportHeight / 2);
        }

        private static void ContentScale_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ScrollBox c = (ScrollBox)o;

            if (c.contentScaleTransform != null)
            {
                //
                // Update the content scale transform whenever 'ContentScale' changes.
                //
                c.contentScaleTransform.ScaleX = c.ContentScale;
                c.contentScaleTransform.ScaleY = c.ContentScale;
            }

            //
            // Update the size of the viewport in content coordinates.
            //
            c.UpdateContentViewportSize();

            if (c.enableContentOffsetUpdateFromScale)
            {
                try
                {
                    // 
                    // Disable content focus syncronization.  We are about to update content offset whilst zooming
                    // to ensure that the viewport is focused on our desired content focus point.  Setting this
                    // to 'true' stops the automatic update of the content focus when content offset changes.
                    //
                    c.disableContentFocusSync = true;

                    //
                    // Whilst zooming in or out keep the content offset up-to-date so that the viewport is always
                    // focused on the content focus point (and also so that the content focus is locked to the 
                    // viewport focus point - this is how the google maps style zooming works).
                    //
                    double viewportOffsetX = c.ViewportZoomFocusX - (c.ViewportWidth / 2);
                    double viewportOffsetY = c.ViewportZoomFocusY - (c.ViewportHeight / 2);
                    double contentOffsetX = viewportOffsetX / c.ContentScale;
                    double contentOffsetY = viewportOffsetY / c.ContentScale;
                    c.ContentOffsetX = (c.ContentZoomFocusX - (c.ContentViewportWidth / 2)) - contentOffsetX;
                    c.ContentOffsetY = (c.ContentZoomFocusY - (c.ContentViewportHeight / 2)) - contentOffsetY;
                }
                finally
                {
                    c.disableContentFocusSync = false;
                }
            }

            if (c.ContentScaleChanged != null)
            {
                c.ContentScaleChanged(c, EventArgs.Empty);
            }

            if (c.scrollOwner != null)
            {
                c.scrollOwner.InvalidateScrollInfo();
            }
        }

        private static object ContentScale_Coerce(DependencyObject d, object baseValue)
        {
            ScrollBox c = (ScrollBox)d;
            double value = (double)baseValue;
            value = Math.Min(Math.Max(value, c.MinContentScale), c.MaxContentScale);
            return value;
        }

        private static void MinOrMaxContentScale_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ScrollBox c = (ScrollBox)o;
            c.ContentScale = Math.Min(Math.Max(c.ContentScale, c.MinContentScale), c.MaxContentScale);
        }

        private static void ContentOffsetX_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ScrollBox c = (ScrollBox)o;

            c.UpdateTranslationX();

            if (!c.disableContentFocusSync)
            {
                //
                // Normally want to automatically update content focus when content offset changes.
                // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
                //
                c.UpdateContentZoomFocusX();
            }

            if (c.ContentOffsetXChanged != null)
            {
                //
                // Raise an event to let users of the control know that the content offset has changed.
                //
                c.ContentOffsetXChanged(c, EventArgs.Empty);
            }

            if (!c.disableScrollOffsetSync && c.scrollOwner != null)
            {
                //
                // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
                //
                c.scrollOwner.InvalidateScrollInfo();
            }
        }

        private static object ContentOffsetX_Coerce(DependencyObject d, object baseValue)
        {
            ScrollBox c = (ScrollBox)d;
            double value = (double)baseValue;
            double minOffsetX = 0.0;
            double maxOffsetX = Math.Max(0.0, c.unScaledExtent.Width - c.constrainedContentViewportWidth);
            value = Math.Min(Math.Max(value, minOffsetX), maxOffsetX);
            return value;
        }

        private static void ContentOffsetY_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ScrollBox c = (ScrollBox)o;

            c.UpdateTranslationY();

            if (!c.disableContentFocusSync)
            {
                //
                // Normally want to automatically update content focus when content offset changes.
                // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
                //
                c.UpdateContentZoomFocusY();
            }

            if (c.ContentOffsetYChanged != null)
            {
                //
                // Raise an event to let users of the control know that the content offset has changed.
                //
                c.ContentOffsetYChanged(c, EventArgs.Empty);
            }

            if (!c.disableScrollOffsetSync && c.scrollOwner != null)
            {
                //
                // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
                //
                c.scrollOwner.InvalidateScrollInfo();
            }

        }

        private static object ContentOffsetY_Coerce(DependencyObject d, object baseValue)
        {
            ScrollBox c = (ScrollBox)d;
            double value = (double)baseValue;
            double minOffsetY = 0.0;
            double maxOffsetY = Math.Max(0.0, c.unScaledExtent.Height - c.constrainedContentViewportHeight);
            value = Math.Min(Math.Max(value, minOffsetY), maxOffsetY);
            return value;
        }

        private void ResetViewportZoomFocus()
        {
            ViewportZoomFocusX = ViewportWidth / 2;
            ViewportZoomFocusY = ViewportHeight / 2;
        }

        private void UpdateViewportSize(Size newSize)
        {
            if (viewport == newSize)
            {
                //
                // The viewport is already the specified size.
                //
                return;
            }

            viewport = newSize;

            //
            // Update the viewport size in content coordiates.
            //
            UpdateContentViewportSize();

            //
            // Initialise the content zoom focus point.
            //
            UpdateContentZoomFocusX();
            UpdateContentZoomFocusY();

            //
            // Reset the viewport zoom focus to the center of the viewport.
            //
            ResetViewportZoomFocus();

            //
            // Update content offset from itself when the size of the viewport changes.
            // This ensures that the content offset remains properly clamped to its valid range.
            //
            ContentOffsetX = ContentOffsetX;
            ContentOffsetY = ContentOffsetY;

            if (scrollOwner != null)
            {
                //
                // Tell that owning ScrollViewer that scrollbar data has changed.
                //
                scrollOwner.InvalidateScrollInfo();
            }
        }

        private void UpdateContentViewportSize()
        {
            ContentViewportWidth = ViewportWidth / ContentScale;
            ContentViewportHeight = ViewportHeight / ContentScale;

            constrainedContentViewportWidth = Math.Min(ContentViewportWidth, unScaledExtent.Width);
            constrainedContentViewportHeight = Math.Min(ContentViewportHeight, unScaledExtent.Height);

            UpdateTranslationX();
            UpdateTranslationY();
        }

        private void UpdateTranslationX()
        {
            if (contentOffsetTransform != null)
            {
                double scaledContentWidth = unScaledExtent.Width * ContentScale;
                if (scaledContentWidth < ViewportWidth)
                {
                    //
                    // When the content can fit entirely within the viewport, center it.
                    //
                    contentOffsetTransform.X = (ContentViewportWidth - unScaledExtent.Width) / 2;
                }
                else
                {
                    contentOffsetTransform.X = -ContentOffsetX;
                }
            }
        }

        private void UpdateTranslationY()
        {
            if (contentOffsetTransform != null)
            {
                double scaledContentHeight = unScaledExtent.Height * ContentScale;
                if (scaledContentHeight < ViewportHeight)
                {
                    //
                    // When the content can fit entirely within the viewport, center it.
                    //
                    contentOffsetTransform.Y = (ContentViewportHeight - unScaledExtent.Height) / 2;
                }
                else
                {
                    contentOffsetTransform.Y = -ContentOffsetY;
                }
            }
        }

        private void UpdateContentZoomFocusX()
        {
            ContentZoomFocusX = ContentOffsetX + (constrainedContentViewportWidth / 2);
        }

        private void UpdateContentZoomFocusY()
        {
            ContentZoomFocusY = ContentOffsetY + (constrainedContentViewportHeight / 2);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            Size childSize = base.MeasureOverride(infiniteSize);

            if (childSize != unScaledExtent)
            {
                //
                // Use the size of the child as the un-scaled extent content.
                //
                unScaledExtent = childSize;

                if (scrollOwner != null)
                {
                    scrollOwner.InvalidateScrollInfo();
                }
            }

            //
            // Update the size of the viewport onto the content based on the passed in 'constraint'.
            //
            UpdateViewportSize(constraint);

            double width = constraint.Width;
            double height = constraint.Height;

            if (double.IsInfinity(width))
            {
                //
                // Make sure we don't return infinity!
                //
                width = childSize.Width;
            }

            if (double.IsInfinity(height))
            {
                //
                // Make sure we don't return infinity!
                //
                height = childSize.Height;
            }

            UpdateTranslationX();
            UpdateTranslationY();

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size size = base.ArrangeOverride(DesiredSize);

            if (content == null)
                return size;

            if (content.DesiredSize != unScaledExtent)
            {
                //
                // Use the size of the child as the un-scaled extent content.
                //
                unScaledExtent = content.DesiredSize;

                if (scrollOwner != null)
                {
                    scrollOwner.InvalidateScrollInfo();
                }
            }

            //
            // Update the size of the viewport onto the content based on the passed in 'arrangeBounds'.
            //
            UpdateViewportSize(arrangeBounds);

            return size;
        }

        public bool CanVerticallyScroll
        {
            get
            {
                return canVerticallyScroll;
            }
            set
            {
                canVerticallyScroll = value;
            }
        }

        public bool CanHorizontallyScroll
        {
            get
            {
                return canHorizontallyScroll;
            }
            set
            {
                canHorizontallyScroll = value;
            }
        }

        public double ExtentWidth
        {
            get
            {
                return unScaledExtent.Width * ContentScale;
            }
        }

        public double ExtentHeight
        {
            get
            {
                return unScaledExtent.Height * ContentScale;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return viewport.Width;
            }
        }

        public double ViewportHeight
        {
            get
            {
                return viewport.Height;
            }
        }

        public ScrollViewer ScrollOwner
        {
            get
            {
                return scrollOwner;
            }
            set
            {
                scrollOwner = value;
            }
        }

        public double HorizontalOffset
        {
            get
            {
                return ContentOffsetX * ContentScale;
            }
        }

        public double VerticalOffset
        {
            get
            {
                return ContentOffsetY * ContentScale;
            }
        }

        public void SetHorizontalOffset(double offset)
        {
            if (disableScrollOffsetSync)
            {
                return;
            }

            try
            {
                disableScrollOffsetSync = true;

                ContentOffsetX = offset / ContentScale;
            }
            finally
            {
                disableScrollOffsetSync = false;
            }
        }

        public void SetVerticalOffset(double offset)
        {
            if (disableScrollOffsetSync)
            {
                return;
            }

            try
            {
                disableScrollOffsetSync = true;

                ContentOffsetY = offset / ContentScale;
            }
            finally
            {
                disableScrollOffsetSync = false;
            }
        }

        public void LineUp()
        {
            ContentOffsetY -= (ContentViewportHeight / 10);
        }

        public void LineDown()
        {
            ContentOffsetY += (ContentViewportHeight / 10);
        }

        public void LineLeft()
        {
            ContentOffsetX -= (ContentViewportWidth / 10);
        }

        public void LineRight()
        {
            ContentOffsetX += (ContentViewportWidth / 10);
        }

        public void PageUp()
        {
            ContentOffsetY -= ContentViewportHeight;
        }

        public void PageDown()
        {
            ContentOffsetY += ContentViewportHeight;
        }

        public void PageLeft()
        {
            ContentOffsetX -= ContentViewportWidth;
        }

        public void PageRight()
        {
            ContentOffsetX += ContentViewportWidth;
        }

        public void MouseWheelDown()
        {
            if (IsMouseWheelScrollingEnabled)
            {
                LineDown();
            }
        }

        public void MouseWheelLeft()
        {
            if (IsMouseWheelScrollingEnabled)
            {
                LineLeft();
            }
        }

        public void MouseWheelRight()
        {
            if (IsMouseWheelScrollingEnabled)
            {
                LineRight();
            }
        }

        public void MouseWheelUp()
        {
            if (IsMouseWheelScrollingEnabled)
            {
                LineUp();
            }
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (content.IsAncestorOf(visual))
            {
                Rect transformedRect = visual.TransformToAncestor(content).TransformBounds(rectangle);
                Rect viewportRect = new Rect(ContentOffsetX, ContentOffsetY, ContentViewportWidth, ContentViewportHeight);
                if (!transformedRect.Contains(viewportRect))
                {
                    double horizOffset = 0;
                    double vertOffset = 0;

                    if (transformedRect.Left < viewportRect.Left)
                    {
                        //
                        // Want to move viewport left.
                        //
                        horizOffset = transformedRect.Left - viewportRect.Left;
                    }
                    else if (transformedRect.Right > viewportRect.Right)
                    {
                        //
                        // Want to move viewport right.
                        //
                        horizOffset = transformedRect.Right - viewportRect.Right;
                    }

                    if (transformedRect.Top < viewportRect.Top)
                    {
                        //
                        // Want to move viewport up.
                        //
                        vertOffset = transformedRect.Top - viewportRect.Top;
                    }
                    else if (transformedRect.Bottom > viewportRect.Bottom)
                    {
                        //
                        // Want to move viewport down.
                        //
                        vertOffset = transformedRect.Bottom - viewportRect.Bottom;
                    }

                    SnapContentOffsetTo(new Point(ContentOffsetX + horizOffset, ContentOffsetY + vertOffset));
                }
            }
            return rectangle;
        }

        void StartAnimation(UIElement animatableElement, DependencyProperty dependencyProperty, double toValue, double animationDurationSeconds)
        {
            StartAnimation(animatableElement, dependencyProperty, toValue, animationDurationSeconds, null);
        }

        void StartAnimation(UIElement animatableElement, DependencyProperty dependencyProperty, double toValue, double animationDurationSeconds, EventHandler completedEvent)
        {
            double fromValue = (double)animatableElement.GetValue(dependencyProperty);

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = fromValue;
            animation.To = toValue;
            animation.Duration = TimeSpan.FromSeconds(animationDurationSeconds);
            QuadraticEase ease = new QuadraticEase();
            ease.EasingMode = EasingMode.EaseOut;
            animation.EasingFunction = ease;

            animation.Completed += (sender, e) =>
            {
                //
                // When the animation has completed bake final value of the animation
                // into the property.
                //
                animatableElement.SetValue(dependencyProperty, animatableElement.GetValue(dependencyProperty));
                CancelAnimation(animatableElement, dependencyProperty);

                if (completedEvent != null)
                {
                    completedEvent(sender, e);
                }
            };

            animation.Freeze();

            animatableElement.BeginAnimation(dependencyProperty, animation);
        }

        void CancelAnimation(UIElement animatableElement, DependencyProperty dependencyProperty)
        {
            animatableElement.BeginAnimation(dependencyProperty, null);
        }

        enum MouseHandlingMode
        {
            None,
            Dragging
        }

        private Point origZoomAndPanControlMouseDownPoint;
        private Point origContentMouseDownPoint;
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;

      //  int tt = 0;

        private void ScrollBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (content == null)
                return;
            //   Debug.Print("ScrollBox_MouseDown");
            //  Debug.Print((tt++).ToString());

            // ????
            Focus();
            Keyboard.Focus(this);

            var mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(content);
            origContentMouseDownPoint = e.GetPosition(content);
            mouseHandlingMode = MouseHandlingMode.None;

            if (mouseButtonDown == MouseButton.Middle)
            {
                mouseHandlingMode = MouseHandlingMode.Dragging;
            }

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                CaptureMouse();
                e.Handled = true;
            }
        }

        private void ScrollBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (content == null)
                return;
            //  Debug.Print("ScrollBox_MouseUp");
            //   Debug.Print((tt++).ToString());

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                //else if (mouseHandlingMode == MouseHandlingMode.Zooming)
                //{
                //    if (mouseButtonDown == MouseButton.Left)
                //    {
                //        // Shift + left-click zooms in on the content.
                //        ZoomIn(origContentMouseDownPoint);
                //    }
                //    else if (mouseButtonDown == MouseButton.Right)
                //    {
                //        // Shift + left-click zooms out from the content.
                //        ZoomOut(origContentMouseDownPoint);
                //    }
                //}
                //else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
                //{
                //    // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.
                //    ApplyDragZoomRect();
                //}

                //
                // Reenable clearing of selection when empty space is clicked.
                // This is disabled when drag panning is in progress.
                //
                //IsClearSelectionOnEmptySpaceClickEnabled = true;

               // Debug.Print("ReleaseMouseCapture");

                mouseHandlingMode = MouseHandlingMode.None;
                ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void ScrollBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (content == null)
                return;

            //Debug.Print("ScrollBox_MouseMove");

            if (mouseHandlingMode == MouseHandlingMode.Dragging)
            {
                e.Handled = true;

             // Debug.Print("Dragging");
             //   Debug.Print((tt++).ToString());


                Point curContentMousePoint = e.GetPosition(content);
                Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;

                //dragOffset.X = -1;

                ContentOffsetX -= dragOffset.X;
                ContentOffsetY -= dragOffset.Y;

               // Debug.Print(ContentOffsetX.ToString() + "   " +dragOffset.X.ToString()
               //     + "   " + contentOffsetTransform.X.ToString());
            }

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                e.Handled = true;
            }
        }

        private void ScrollBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (content == null)
                return;

            e.Handled = true;

            if (e.Delta > 0)
            {
                Point curContentMousePoint = e.GetPosition(content);
                ZoomIn(curContentMousePoint);
            }
            else if (e.Delta < 0)
            {
                Point curContentMousePoint = e.GetPosition(content);
                ZoomOut(curContentMousePoint);
            }
        }

        private void ZoomOut(Point contentZoomCenter)
        {
            double zoomVal = 0.5;
            AnimatedZoomAboutPoint(ContentScale - zoomVal, contentZoomCenter);
        }
        
        private void ZoomIn(Point contentZoomCenter)
        {
            double zoomVal = 0.5;
            AnimatedZoomAboutPoint(ContentScale + zoomVal, contentZoomCenter);
        }
    }
}
