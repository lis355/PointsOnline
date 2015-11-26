using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3View
{
    public static class Tools
    {
        public static void SaveInvoke( this EventHandler handler, object sender = null )
        {
            if ( handler != null )
            {
                handler.Invoke( sender, EventArgs.Empty );
            }
        }

        public static float Clamp( this float f, float l, float r )
        {
            return ( f < l ) ? l : ( ( f > r ) ? r : f );
        }
    }
}
