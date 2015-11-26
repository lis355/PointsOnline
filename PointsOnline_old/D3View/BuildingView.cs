using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SlimDX;

namespace D3View
{
    public partial class D3View : UserControl
    {
        // Девайс на рисование
        public GraphicsDevice GraphicsDevice { private set; get; }

        // События
        public event EventHandler MapScaleChanged;
        public event EventHandler ViewportChanged;

        // Координаты мыши
        public Point MousePos { private set; get; }
        public Point MouseViewPos { private set; get; }
        public Point LastMousePos { private set; get; }
        public Point LastMouseViewPos { private set; get; }

        // Таймер апдейта
        public Timer UpdateTimer = null;

        /// <summary>
        /// Вьюпорт
        /// </summary>
        Vector2 viewPort = Vector2.Zero;
        public virtual Vector2 ViewPort
        {
            get
            {
                return viewPort;
            }
            set
            {
                if ( viewPort == value )
                    return;

                viewPort = value;
                ViewportChanged.SaveInvoke( this );
            }
        }

        /// <summary>
        /// Масштабирование
        /// </summary>
        Matrix scaleMatrix;
        Matrix scaleInvertMatrix;

        public float MinimumMapScale { set; get; }
        public float MaximumMapScale { set; get; }

        float mapScale = 1.0f;
        public virtual float MapScale
        {
            get
            {
                return mapScale;
            }
            set
            {
                mapScale = value.Clamp( MinimumMapScale, MaximumMapScale );
                MapScaleChanged.SaveInvoke( this );
            }
        }

        public D3View()
        {
            GraphicsDevice = new GraphicsDevice( this );

            MinimumMapScale = 0.1f;
            MaximumMapScale = 4.5f;

            UpdateTimer = new Timer();
            UpdateTimer.Interval = 15; // ~60 fps
            UpdateTimer.Tick += ( o, e ) => UpdateView();
            UpdateTimer.Start();
        }

        void UpdateView()
        {
            Update();
            Invalidate();
            base.Update();
        }

        void SetTransformMatrix()
        {
            GraphicsDevice.Projection = Matrix.OrthoOffCenterRH( 0, Width, Height, 0, 100, -100 );

            GraphicsDevice.WorldTransform = Matrix.Translation( ViewPort.X, ViewPort.Y, 0 );

            // скейлим от центра экрана
            scaleMatrix = Matrix.Translation( -Width / 2, -Height / 2, 0 );
            scaleMatrix *= Matrix.Scaling( mapScale, mapScale, 1.0f );
            scaleMatrix *= Matrix.Translation( Width / 2, Height / 2, 0 );

            scaleInvertMatrix = scaleMatrix;
            scaleInvertMatrix.Invert();

            scaleInvertMatrix *= Matrix.Translation( - ViewPort.X, - ViewPort.Y, 0 );

            GraphicsDevice.WorldTransform *= scaleMatrix;
        }
        
        void DrawView()
        {
            GraphicsDevice.BeginPaint();

            GraphicsDevice.Clear( BackColor );

            SetTransformMatrix();

            try
            {
                Draw(); 
            }
            catch ( Exception )
            {
                // ...
            }

            GraphicsDevice.EndPaint();
        }

        protected new virtual void Update()
        {
        }

        protected virtual void Draw()
        {
        }

        #region Mouse

        /// <summary>
        /// Конвертирует координаты мыши в координаты пространства, учитывая скейл и смещение
        /// </summary>
        public Point FromScreenToView( Point localPoint )
        {
            Vector2 nl = new Vector2( localPoint.X, localPoint.Y );
            nl = Vector2.TransformCoordinate( nl, scaleInvertMatrix );
            return new Point( ( int )nl.X, ( int )nl.Y );
        }

        protected virtual void ViewMouseDown( MouseEventArgs e )
        {
        }

        protected virtual void ViewMouseMove( MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Middle )
            {
                DragViewPort( e );
            }
        }

        protected virtual void ViewMouseUp( MouseEventArgs e )
        {
        }

        void DragViewPort( MouseEventArgs e )
        {
            // зажатым колесом драгаем карту

            Vector2 screenDelta = new Vector2(
                MousePos.X - LastMousePos.X,
                MousePos.Y - LastMousePos.Y );

            ViewPort += screenDelta / MapScale;
                
                //new PointF(
                //viewPort.X + screenDelta.X / MapScale,
                //viewPort.Y + screenDelta.Y / MapScale );
        }

        #endregion

        protected override void OnPaintBackground( PaintEventArgs e )
        {
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            DrawView();
            base.OnPaint( e );
        }

        protected override void OnMouseDown( MouseEventArgs e )
        {
            base.OnMouseDown( e );
            Point eLocation = FromScreenToView( e.Location );
            MousePos = e.Location;
            MouseViewPos = eLocation;
            ViewMouseDown( new MouseEventArgs( e.Button, e.Clicks, eLocation.X, eLocation.Y, e.Delta ) );
            LastMousePos = e.Location;
            LastMouseViewPos = eLocation;
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            base.OnMouseMove( e );
            Point eLocation = FromScreenToView( e.Location );
            MousePos = e.Location;
            MouseViewPos = eLocation;
            ViewMouseMove( new MouseEventArgs( e.Button, e.Clicks, eLocation.X, eLocation.Y, e.Delta ) );
            LastMousePos = e.Location;
            LastMouseViewPos = eLocation;
        }

        protected override void OnMouseUp( MouseEventArgs e )
        {
            base.OnMouseUp( e );
            Point eLocation = FromScreenToView( e.Location );
            MousePos = e.Location;
            MouseViewPos = eLocation;
            ViewMouseUp( new MouseEventArgs( e.Button, e.Clicks, eLocation.X, eLocation.Y, e.Delta ) );
            LastMousePos = e.Location;
            LastMouseViewPos = eLocation;
        }

        protected override void OnMouseWheel( MouseEventArgs e )
        {
            base.OnMouseWheel( e );

            if ( e.Delta != 0 )
            {
                float step = 0.2f;
                MapScale = ( float )( ( e.Delta >= 0 ) ? MapScale * ( 1.0 + step ) :
                    MapScale * ( 1.0f - step ) );
            }
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            base.OnKeyDown( e );
        }

        protected override void OnResize( EventArgs e )
        {
            base.OnResize( e );

            UpdateView();
        }
    }
}
