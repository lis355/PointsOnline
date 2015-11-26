using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PointsOnlineProject.Core
{
    public static class MapObjectPool
    {
        static List<MapObject> _objects = new List<MapObject>();
        static List<MapObject> _removedObjects = new List<MapObject>();
        static List<MapObject> _addedObjects = new List<MapObject>();
        static Timer _updateTimer = null;

        static MapObjectPool()
        {
            _updateTimer = new Timer();
            _updateTimer.Interval = 1000 / 60;
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        static void UpdateTimer_Tick( object sender, EventArgs e )
        {
 	        Update();
        }

        static void Update()
        {
            // process delete
            if ( _removedObjects.Count > 0 )
            {
                foreach ( MapObject o in _removedObjects )
                {
                    _objects.Remove( o );
                }

                _removedObjects.Clear();
            }

            // process add
            if ( _addedObjects.Count > 0 )
            {
                foreach ( MapObject o in _addedObjects )
                {
                    o.Start();
                }

                _objects.AddRange( _addedObjects );
                _addedObjects.Clear();
            }

            // update all
            foreach ( MapObject o in _objects )
            {
                o.Update();
            }

            // draw all
            foreach ( MapObject o in _objects )
            {
                o.Draw();
            }
        }

        public static void Add( MapObject obj )
        {
            Debug.Assert( obj != null );
            if ( obj == null )
                return;

            _addedObjects.Add( obj );
        }

        public static void Remove( MapObject obj )
        {
            Debug.Assert( obj != null );
            if ( obj == null )
                return;

            _removedObjects.Add( obj );
        }
    }

    public class MapObject 
    {
        public static MapObject Create()
        {
            MapObject result = new MapObject();
            MapObjectPool.Add( result );
            result.OnCreated();

            return result;
        }

        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void Draw() { }

        protected virtual void OnCreated() { }
        protected virtual void OnDestroyed() { }

        public void Destroy()
        {
            OnDestroyed();
            MapObjectPool.Remove( this );
        }
    }
}
