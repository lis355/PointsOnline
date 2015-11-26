using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using System.Drawing;
using System.IO;

namespace D3View
{
    public class SmartTexture
    {
        public int Width
        {
            get
            {
                if ( texture == null && !hasFailed )
                    LoadTexture();

                if ( hasFailed || loading )
                    return 0;
                else
                    return ( int )( imageInfo.Width * Scale );
            }
        }

        public int Height
        {
            get
            {
                if ( texture == null && !hasFailed )
                    LoadTexture();

                if ( hasFailed || loading )
                    return 0;
                else
                    return ( int )( imageInfo.Height * Scale );
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle( 0, 0, Width, Height );
            }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public bool Visible { get; set; }

        public int TextureWidth
        {
            get
            {
                if ( texture == null && !hasFailed )
                    LoadTexture();

                if ( hasFailed || loading )
                    return 0;
                else
                    return desc.Width;
            }
        }

        public int TextureHeight
        {
            get
            {
                if ( texture == null && !hasFailed )
                    LoadTexture();

                if ( hasFailed || loading )
                    return 0;
                else
                    return desc.Height;
            }
        }

        public Texture Texture
        {
            get
            {
                if ( texture == null && !hasFailed && !loading )
                    LoadTexture();

                return texture;
            }
        }

        public SmartTexture( string path )
        {
            this.path = path;
            Visible = true;
        }

        public SmartTexture( string path, float scale )
        {
            this.path = path;
            this.scale = scale;
            Visible = true;        
        }

        public SmartTexture( string path, bool loadAsync )
        {
            this.path = path;
            this.loadAsync = loadAsync;
            Visible = true;
        }

        public SmartTexture( string path, float scale, bool loadAsync )
        {
            this.path = path;
            this.scale = scale;
            this.loadAsync = loadAsync;
            Visible = true;
        }

        void LoadTextureAsync()
        {
            loading = true;

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += new DoWorkEventHandler( LoadWorkerDoWork );
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( LoadWorkerCompleted );

            worker.RunWorkerAsync();
        }

        void LoadWorkerDoWork( object sender, DoWorkEventArgs e )
        {
            LoadTextureNow();
        }

        void LoadWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
        }

        void LoadTextureNow()
        {
            loading = true;

            try
            {
                texture = Texture.FromFile( GraphicsDevice.Direct3DDevice,
                    path,
                    0, 0, 1,
                    Usage.None,
                    Format.Unknown,
                    Pool.Default,
                    Filter.Box,
                    Filter.None,
                    0, out imageInfo );
            }
            catch ( Exception )
            {
                hasFailed = true;
            }

            if ( texture != null )
            {
                desc = texture.GetLevelDescription( 0 );
                loadedTextures.Add( this );
            }

            loading = false;
        }

        void LoadTexture()
        {
            if ( loading )
                return;

            if ( loadAsync )
                LoadTextureAsync();
            else
                LoadTextureNow();
        }

        public void Unload()
        {
            if ( texture != null )
            {
                loadedTextures.Remove( this );
                texture.Dispose();
                texture = null;
            }
        }

        public static void UnloadAllTextures()
        {
            while ( loadedTextures.Count > 0 )
                loadedTextures[ 0 ].Unload();
        }

        string path;
        Texture texture = null;
        bool hasFailed = false;
        bool loadAsync = false;
        bool loading = false;
        ImageInformation imageInfo;
        SurfaceDescription desc;
        float scale = 1.0f;

        static List<SmartTexture> loadedTextures = new List<SmartTexture>();
    }
}
