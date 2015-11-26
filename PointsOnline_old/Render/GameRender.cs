using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using D3View;
using SlimDX;

namespace PointsOnlineProject
{
    class GameRender : InternalD3View
    {
        SmartTexture text = new SmartTexture( Directory.GetCurrentDirectory() + "\\..\\..\\..\\scheme.png" );

        protected override void Update()
        {
            base.Update();
        }

        protected override void Draw()
        {
            base.Draw();

            GraphicsDevice.DrawImage( text, 0, 0 );

            GraphicsDevice.DrawLineColored( Color.Red, new Vector2(), new Vector2( 50 ) );
        }
    }
}
