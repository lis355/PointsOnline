using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;

namespace D3View
{
    public enum CompositingMode
    {
        Replace,
        AlphaBlending,
        Additive,
        Modulate
    }

    public enum EOverlayMode
    {
        None,
        AlphaBlend,
        Add,
        Multiply
    }

    public class GraphicsDevice
    {
        static Direct3D d3d = null;
        static Device device = null;

        public static Device Direct3DDevice
        {
            get { return device; }
        }

        static GraphicsDevice()
        {
            d3d = new Direct3D();

            PresentParameters presentParameters = new PresentParameters();

            presentParameters.BackBufferCount = 1;
            presentParameters.BackBufferFormat = Format.Unknown;
            presentParameters.BackBufferHeight = d3d.Adapters[0].CurrentDisplayMode.Height; // back-buffer на весь экран.
            presentParameters.BackBufferWidth = d3d.Adapters[0].CurrentDisplayMode.Width;
            presentParameters.DeviceWindowHandle = IntPtr.Zero;
            presentParameters.EnableAutoDepthStencil = false;
            presentParameters.PresentationInterval = PresentInterval.Immediate;
            presentParameters.SwapEffect = SwapEffect.Discard;
            presentParameters.Windowed = true;

            device = new Device( d3d, 0, DeviceType.Hardware, IntPtr.Zero, 
                CreateFlags.HardwareVertexProcessing, presentParameters );
        }

        private Control control = null;
        private Matrix worldTransform = Matrix.Identity;
        private Matrix projection = Matrix.Identity;

        public Matrix Projection
        {
            get { return projection; }
            set 
            { 
                projection = value;
                device.SetTransform( TransformState.Projection, projection );
            }
        }

        public Matrix WorldTransform
        {
            get { return worldTransform; }
            set 
            {
                worldTransform = value;
                device.SetTransform( TransformState.World, worldTransform );
            }
        }

        CompositingMode currentMode = CompositingMode.Replace;
        public CompositingMode CompositingMode
        {
            get { return currentMode; }
            set
            {
                SetCompositingMode( value, false );
            }
        }
        
        private EOverlayMode currentOverlayMode = EOverlayMode.None;
        public EOverlayMode OverlayMode
        {
            get { return currentOverlayMode; }
            set 
            {
                SetOverlayMode( value, currentOverlayColor, false );
            }
        }

        private Color currentOverlayColor = Color.Black;
        public Color OverlayColor
        {
            get { return currentOverlayColor; }
            set 
            {
                SetOverlayMode( currentOverlayMode, value, false );
            }
        }
        
        public GraphicsDevice( Control control )
        {
            this.control = control;
        }

        public void Clear( Color color )
        {
            if ( device != null )
                device.Clear( ClearFlags.Target, color.ToArgb(), 0, 0 );
        }

        public void BeginPaint()
        {
            if ( device == null || control == null )
                return;

            device.BeginScene();

            device.SetRenderState( RenderState.ZEnable, false );
            device.SetRenderState( RenderState.ZFunc, Compare.Always );
            device.SetRenderState( RenderState.Lighting, false );
            device.SetRenderState( RenderState.CullMode, Cull.None );

            SetCompositingMode( CompositingMode.AlphaBlending, true );

            device.SetTransform( TransformState.View, Matrix.Identity );
            device.SetTransform( TransformState.Projection, Projection );
            device.SetTransform( TransformState.World, WorldTransform );

            device.Viewport = new Viewport( 
                0,
                0,
                (int)control.ClientRectangle.Width,
                (int)control.ClientRectangle.Height );

            SetupTexture( null, true );
        }

        public void EndPaint()
        {
            device.EndScene();
            device.Present(
                new Rectangle( 0, 0, control.Width, control.Height ),
                control.ClientRectangle,
                control.Handle );
        }

        private void SetCompositingMode( CompositingMode mode, bool force )
        {
            if ( !force && currentMode == mode )
                return;

            switch ( mode )
            {
                case CompositingMode.AlphaBlending:
                    device.SetRenderState( RenderState.AlphaBlendEnable, true );
                    device.SetRenderState( RenderState.BlendOperation, BlendOperation.Add );
                    device.SetRenderState( RenderState.SourceBlend, Blend.SourceAlpha );
                    device.SetRenderState( RenderState.DestinationBlend, Blend.InverseSourceAlpha );
                    break;

                case CompositingMode.Additive:
                    device.SetRenderState( RenderState.AlphaBlendEnable, true );
                    device.SetRenderState( RenderState.BlendOperation, BlendOperation.Add );
                    device.SetRenderState( RenderState.SourceBlend, Blend.One );
                    device.SetRenderState( RenderState.DestinationBlend, Blend.One );
                    break;

                case CompositingMode.Modulate:
                    device.SetRenderState( RenderState.AlphaBlendEnable, true );
                    device.SetRenderState( RenderState.BlendOperation, BlendOperation.Add );
                    device.SetRenderState( RenderState.SourceBlend, Blend.Zero );
                    device.SetRenderState( RenderState.DestinationBlend, Blend.SourceColor );
                    break;

                default:
                    device.SetRenderState( RenderState.AlphaBlendEnable, false );
                    break;
            }

            currentMode = mode;
        }

        private void SetOverlayMode( EOverlayMode mode, Color overlayColor, bool force )
        {
            if ( !force && mode == currentOverlayMode && overlayColor == this.currentOverlayColor )
                return;

            SetupTexture( currentTexture, true );

            currentOverlayMode = mode;
            currentOverlayColor = overlayColor;
        }

        private void SetupTexture( SmartTexture smartTexture )
        {
            Texture texture = null;
            if ( smartTexture != null )
                texture = smartTexture.Texture;

            SetupTexture( texture, false );
        }

        Texture currentTexture = null;
        private void SetupTexture( Texture texture, bool force )
        {
            if ( !force && (currentTexture == texture) )
                return;

            if ( texture == null )
            {
                device.SetTexture( 0, null );
                device.SetTextureStageState( 0, TextureStage.ColorOperation, TextureOperation.SelectArg2 );
                device.SetTextureStageState( 0, TextureStage.ColorArg2, TextureArgument.Diffuse );
                device.SetTextureStageState( 0, TextureStage.AlphaOperation, TextureOperation.SelectArg2 );
                device.SetTextureStageState( 0, TextureStage.AlphaArg2, TextureArgument.Diffuse );

                device.SetTextureStageState( 1, TextureStage.ColorOperation, TextureOperation.Disable );
                device.SetTextureStageState( 1, TextureStage.AlphaOperation, TextureOperation.Disable );
            }
            else
            {
                device.SetTexture( 0, texture );
                
                switch ( OverlayMode )
                { 
                    case EOverlayMode.Add:
                        device.SetTextureStageState( 0, TextureStage.ColorOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 0, TextureStage.ColorArg1, TextureArgument.Texture );
                        device.SetTextureStageState( 0, TextureStage.ColorArg2, TextureArgument.Diffuse );
                        device.SetTextureStageState( 0, TextureStage.AlphaOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 0, TextureStage.AlphaArg1, TextureArgument.Texture );
                        device.SetTextureStageState( 0, TextureStage.AlphaArg2, TextureArgument.Diffuse );

                        device.SetTextureStageState( 1, TextureStage.ColorOperation, TextureOperation.Add );
                        device.SetTextureStageState( 1, TextureStage.ColorArg1, TextureArgument.Specular );
                        device.SetTextureStageState( 1, TextureStage.ColorArg2, TextureArgument.Current );
                        device.SetTextureStageState( 1, TextureStage.AlphaOperation, TextureOperation.SelectArg1 );
                        device.SetTextureStageState( 1, TextureStage.AlphaArg1, TextureArgument.Current );

                        device.SetTextureStageState( 2, TextureStage.ColorOperation, TextureOperation.Disable );
                        device.SetTextureStageState( 2, TextureStage.AlphaOperation, TextureOperation.Disable );
                        break;

                    case EOverlayMode.AlphaBlend:

                        device.SetTextureStageState( 0, TextureStage.ColorOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 0, TextureStage.ColorArg1, TextureArgument.Texture );
                        device.SetTextureStageState( 0, TextureStage.ColorArg2, TextureArgument.Diffuse );
                        device.SetTextureStageState( 0, TextureStage.AlphaOperation, TextureOperation.SelectArg1 );
                        device.SetTextureStageState( 0, TextureStage.AlphaArg1, TextureArgument.Specular );

                        if ( texture != null )
							device.SetTexture( 1, texture );
                        device.SetTextureStageState( 1, TextureStage.TexCoordIndex, 0 );

                        device.SetTextureStageState( 1, TextureStage.ColorOperation, TextureOperation.BlendCurrentAlpha );
                        device.SetTextureStageState( 1, TextureStage.ColorArg1, TextureArgument.Specular );
                        device.SetTextureStageState( 1, TextureStage.ColorArg2, TextureArgument.Current );
                        device.SetTextureStageState( 1, TextureStage.AlphaOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 1, TextureStage.AlphaArg1, TextureArgument.Texture );
                        device.SetTextureStageState( 1, TextureStage.AlphaArg2, TextureArgument.Diffuse );

                        device.SetTextureStageState( 2, TextureStage.ColorOperation, TextureOperation.Disable );
                        device.SetTextureStageState( 2, TextureStage.AlphaOperation, TextureOperation.Disable );
                        break;

                    case EOverlayMode.Multiply:
                        device.SetTextureStageState( 0, TextureStage.ColorOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 0, TextureStage.ColorArg1, TextureArgument.Texture );
                        device.SetTextureStageState( 0, TextureStage.ColorArg2, TextureArgument.Diffuse );
                        device.SetTextureStageState( 0, TextureStage.AlphaOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 0, TextureStage.AlphaArg1, TextureArgument.Texture );
                        device.SetTextureStageState( 0, TextureStage.AlphaArg2, TextureArgument.Diffuse );

                        device.SetTextureStageState( 1, TextureStage.ColorOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 1, TextureStage.ColorArg1, TextureArgument.Specular );
                        device.SetTextureStageState( 1, TextureStage.ColorArg2, TextureArgument.Current );
                        device.SetTextureStageState( 1, TextureStage.AlphaOperation, TextureOperation.SelectArg1 );
                        device.SetTextureStageState( 1, TextureStage.AlphaArg1, TextureArgument.Current );

                        device.SetTextureStageState( 2, TextureStage.ColorOperation, TextureOperation.Disable );
                        device.SetTextureStageState( 2, TextureStage.AlphaOperation, TextureOperation.Disable );
                        break;

                    default:
                        device.SetTextureStageState( 0, TextureStage.ColorOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 0, TextureStage.ColorArg1, TextureArgument.Texture );
                        device.SetTextureStageState( 0, TextureStage.ColorArg2, TextureArgument.Diffuse );
                        device.SetTextureStageState( 0, TextureStage.AlphaOperation, TextureOperation.Modulate );
                        device.SetTextureStageState( 0, TextureStage.AlphaArg1, TextureArgument.Texture );
                        device.SetTextureStageState( 0, TextureStage.AlphaArg2, TextureArgument.Diffuse );

                        device.SetTextureStageState( 1, TextureStage.ColorOperation, TextureOperation.Disable );
                        device.SetTextureStageState( 1, TextureStage.AlphaOperation, TextureOperation.Disable );
                        break;
                }
            }

            currentTexture = texture;
        }

        struct TexturedVertex
        {
            public Vector3 Position;
            public int DiffuseColor;
            public int SpecularColor;
            public Vector2 TextureCoords;

            public TexturedVertex( Vector3 position, int diffuse, Vector2 textureCoords )
            {
                this.Position = position;
                this.DiffuseColor = diffuse;
                this.SpecularColor = 0;
                this.TextureCoords = textureCoords;
            }

            public TexturedVertex( Vector3 position, int diffuse, int specular, Vector2 textureCoords )
            {
                this.Position = position;
                this.DiffuseColor = diffuse;
                this.SpecularColor = specular;
                this.TextureCoords = textureCoords;
            }
        }

        public void DrawImage( SmartTexture texture, float x, float y )
        {
            DrawImageColored( texture, new Color4( Color.White ), x, y );
        }

        public void DrawImage( SmartTexture texture, RectangleF screenRect, RectangleF textureRect )
        {
            DrawImageColored( texture, new Color4( Color.White ), screenRect, textureRect );
        }

        public void DrawImageColored( SmartTexture texture, Color4 color, float x, float y )
        {
            float twidth = (texture == null) ? 0 : texture.Width;
            float theight = (texture == null) ? 0 : texture.Height;

            Vector2[] verts = new Vector2[] 
            {
                new Vector2( x, y ),
                new Vector2( x, y + theight ),
                new Vector2( x + twidth, y + theight ),
                new Vector2( x + twidth, y )
            };

            DrawImageColored( texture, color, verts );
        }

        public void DrawImageColored( SmartTexture texture, Color4 color, RectangleF screenRect )
        {
            float twidth = (texture == null) ? 0 : texture.Width;
            float theight = (texture == null) ? 0 : texture.Height;

            RectangleF textureRect = new RectangleF( 0, 0, twidth, theight );

            DrawImageColored( texture, color, screenRect, textureRect );
        }

        public void DrawImageColored( SmartTexture texture, Color4 color, RectangleF screenRect, RectangleF textureRect )
        {
            Vector2[] verts = new Vector2[] 
            {
                new Vector2( screenRect.Left, screenRect.Top ),
                new Vector2( screenRect.Left, screenRect.Bottom ),
                new Vector2( screenRect.Right, screenRect.Bottom ),
                new Vector2( screenRect.Right, screenRect.Top )
            };

            Vector2[] tverts = new Vector2[] 
            {
                new Vector2( textureRect.Left, textureRect.Top ),
                new Vector2( textureRect.Left, textureRect.Bottom ),
                new Vector2( textureRect.Right, textureRect.Bottom ),
                new Vector2( textureRect.Right, textureRect.Top )
            };

            DrawImageColored( texture, color, verts, tverts );
        }

        public void DrawImageColored( SmartTexture texture, Color4 color, Vector2[] verts )
        {
            float twidth = (texture == null) ? 0 : texture.Width;
            float theight = (texture == null) ? 0 : texture.Height;

            Vector2[] tverts = new Vector2[] 
            {
                new Vector2( 0, 0 ),
                new Vector2( 0, theight ),
                new Vector2( twidth, theight ),
                new Vector2( twidth, 0 )
            };

            DrawImageColored( texture, color, verts, tverts );
        }

        public void DrawImageColored( SmartTexture texture, Color4 color, Vector2[] verts, Vector2[] tverts )
        {
            device.VertexFormat = VertexFormat.Position
                | VertexFormat.Diffuse
                | VertexFormat.Specular
                | VertexFormat.Texture1;

            SetupTexture( texture );

            Vector2 toUV = new Vector2(
                (texture == null) ? 1 : 1.0f / (float)texture.Width,
                (texture == null) ? 1 : 1.0f / (float)texture.Height );

            int diffuseColor = color.ToArgb();
            int specular = currentOverlayColor.ToArgb();
            TexturedVertex[] verts2 = new TexturedVertex[]
            {
                new TexturedVertex( new Vector3( verts[0], 0 ), diffuseColor, specular, Vector2.Modulate( tverts[0], toUV ) ),
                new TexturedVertex( new Vector3( verts[1], 0 ), diffuseColor, specular, Vector2.Modulate( tverts[1], toUV ) ),
                new TexturedVertex( new Vector3( verts[2], 0 ), diffuseColor, specular, Vector2.Modulate( tverts[2], toUV ) ),
                new TexturedVertex( new Vector3( verts[3], 0 ), diffuseColor, specular, Vector2.Modulate( tverts[3], toUV ) ),
            };

            device.DrawUserPrimitives<TexturedVertex>( PrimitiveType.TriangleFan, 2, verts2 );
        }

        public void DrawImageColoredTransformed( SmartTexture texture, Color color, RectangleF dstRect, RectangleF texRect, PointF scaleCenter, float scale, PointF rotateCenter, float rotateAngle )
        {
            Vector2[] verts = new Vector2[4];
            Vector2[] tverts = new Vector2[4];

            verts[0].X = dstRect.X;
            verts[0].Y = dstRect.Y;
            verts[1].X = dstRect.X;
            verts[1].Y = dstRect.Y + dstRect.Height;
            verts[2].X = dstRect.X + dstRect.Width;
            verts[2].Y = dstRect.Y + dstRect.Height;
            verts[3].X = dstRect.X + dstRect.Width;
            verts[3].Y = dstRect.Y;

            Matrix mtx = Matrix.Transformation2D(
                new Vector2( scaleCenter.X, scaleCenter.Y ), 0.0f, new Vector2( scale, scale ),
                new Vector2( rotateCenter.X, rotateCenter.Y ), (float)(rotateAngle * Math.PI / 180.0f),
                new Vector2( 0, 0 ) );

            for ( int i = 0; i < 4; ++i )
            {
                Vector2 v = Vector2.TransformCoordinate( new Vector2( verts[i].X, verts[i].Y ), mtx );
                verts[i].X = v.X;
                verts[i].Y = v.Y;
            }

            tverts[0].X = texRect.X;
            tverts[0].Y = texRect.Y;
            tverts[1].X = texRect.X;
            tverts[1].Y = texRect.Y + texRect.Height;
            tverts[2].X = texRect.X + texRect.Width;
            tverts[2].Y = texRect.Y + texRect.Height;
            tverts[3].X = texRect.X + texRect.Width;
            tverts[3].Y = texRect.Y;

            DrawImageColored( texture, color, verts, tverts );
        }

        struct SimpleVertex
        {
            public Vector3 Position;
            public int DiffuseColor;

            public SimpleVertex( Vector3 position, int diffuse )
            {
                this.Position = position;
                this.DiffuseColor = diffuse;
            }
        }

        public void DrawLineColored( Color color, Vector2 p0, Vector2 p1 )
        {
            DrawLineColored( new Color4( color ), p0, p1 );
        }

        public void DrawLineColored( Color4 color, Vector2 p0, Vector2 p1 )
        {
            device.VertexFormat = VertexFormat.Position | VertexFormat.Diffuse;

            SimpleVertex[] verts = new SimpleVertex[]
            {
                new SimpleVertex( new Vector3( p0, 0 ), color.ToArgb() ),
                new SimpleVertex( new Vector3( p1, 0 ), color.ToArgb() )
            };

            SetupTexture( null );

            device.DrawUserPrimitives<SimpleVertex>( PrimitiveType.LineList, 1, verts );
        }


        public void DrawRectangle( Color color, RectangleF rc )
        {
            DrawLineColored( color, new Vector2( rc.Left, rc.Top ), new Vector2( rc.Right, rc.Top ) );
            DrawLineColored( color, new Vector2( rc.Left, rc.Bottom ), new Vector2( rc.Right, rc.Bottom ) );
            DrawLineColored( color, new Vector2( rc.Left, rc.Top ), new Vector2( rc.Left, rc.Bottom ) );
            DrawLineColored( color, new Vector2( rc.Right, rc.Top ), new Vector2( rc.Right, rc.Bottom ) );
        }

		public void DrawCross( Color color, float x, float y, float width, float height )
		{
			float halfWidth = width / 2.0f;
			float halfHeight = height / 2.0f;

			Vector2 p0 = new Vector2( x - halfWidth, y );
			Vector2 p1 = new Vector2( x + halfWidth, y );

			Vector2 p2= new Vector2( x, y - halfHeight );
			Vector2 p3 = new Vector2( x, y + halfHeight );

			DrawLineColored( color, p0, p1 );
			DrawLineColored( color, p2, p3 );
		}

        public Texture LoadTexture( string fileName )
        {
            return Texture.FromFile( device, fileName );
        }

        public void FillQuad( Color color, PointF p0, PointF p1, PointF p2, PointF p3 )
        {
            device.VertexFormat = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Specular | VertexFormat.Texture1;

            SetupTexture( null );

            int diffuseColor = color.ToArgb();
            int specular = currentOverlayColor.ToArgb();
            TexturedVertex[] verts2 = new TexturedVertex[]
            {
                new TexturedVertex( new Vector3( p0.X, p0.Y, 0 ), diffuseColor, specular, Vector2.Zero ),
                new TexturedVertex( new Vector3( p1.X, p1.Y, 0 ), diffuseColor, specular, Vector2.Zero ),
                new TexturedVertex( new Vector3( p2.X, p2.Y, 0 ), diffuseColor, specular, Vector2.Zero ),
                new TexturedVertex( new Vector3( p3.X, p3.Y, 0 ), diffuseColor, specular, Vector2.Zero ),
            };

            device.DrawUserPrimitives<TexturedVertex>( PrimitiveType.TriangleFan, 2, verts2 );
        }

		public void FillTriangle( Color color, PointF p0, PointF p1, PointF p2 )
        {
            device.VertexFormat = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Specular | VertexFormat.Texture1;

            SetupTexture( null );

            int diffuseColor = color.ToArgb();
            int specular = currentOverlayColor.ToArgb();
            TexturedVertex[] verts2 = new TexturedVertex[]
            {
                new TexturedVertex( new Vector3( p0.X, p0.Y, 0 ), diffuseColor, specular, Vector2.Zero ),
                new TexturedVertex( new Vector3( p1.X, p1.Y, 0 ), diffuseColor, specular, Vector2.Zero ),
                new TexturedVertex( new Vector3( p2.X, p2.Y, 0 ), diffuseColor, specular, Vector2.Zero ),
            };

            device.DrawUserPrimitives<TexturedVertex>( PrimitiveType.TriangleFan, 1, verts2 );
        }
    }
}
