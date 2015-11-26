using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using D3View;
using SlimDX;
using System.Diagnostics;

namespace PointsOnlineProject
{
    class InternalD3View : D3View.D3View
    {
        // inert scroll
        bool isAutoScroll = false;
        bool isMouseDown = false;
        Point lastMouseScreenPos;
        float dt = 1.0f;
        Vector2 da;

        // inert zoom

        public InternalD3View():
            base()
        {
            dt = Math.Max( dt, UpdateTimer.Interval );
        }

        protected override void Update()
        {
            base.Update();

            ProcessAutoScroll();
        }

        protected override void ViewMouseDown( MouseEventArgs e )
        {
            base.ViewMouseDown( e );

            EndAutoScroll();

            isMouseDown = e.Button == MouseButtons.Middle;
            lastMouseScreenPos = PointToClient( MousePosition );
            da = Vector2.Zero;
        }

        protected override void ViewMouseUp( MouseEventArgs e )
        {
            base.ViewMouseUp( e );

            isMouseDown = false;

            StartAutoScroll();
        }

        void StartAutoScroll()
        {
            isAutoScroll = true;
        }

        void ProcessAutoScroll()
        {
            if ( isMouseDown )
            {
                WriteMove();
            }
            else if ( isAutoScroll )
            {
                ViewPort += da;

                da *= 0.95f;

                if ( da.LengthSquared() < 1.0f )
                {
                    EndAutoScroll();
                }
            }
        }

        void EndAutoScroll()
        {
            isAutoScroll = false;
            ViewPort = new Vector2( ( float )Math.Round( ViewPort.X ),
                ( float )Math.Round( ViewPort.Y ) );
        }

        void WriteMove()
        {
            Point newMouseScreenPos = PointToClient( MousePosition );

            if ( newMouseScreenPos == lastMouseScreenPos )
                return;

            Vector2 ds = new Vector2( newMouseScreenPos.X - lastMouseScreenPos.X,
                newMouseScreenPos.Y - lastMouseScreenPos.Y );

            Debug.WriteLine( ds / dt );
            da += ds / dt;

            lastMouseScreenPos = newMouseScreenPos;
        }
    }
}