using GJson;
using System.Collections.Generic;

namespace PointsOnline
{
    class Region
    {
        List<IntPoint> _points;
        List<IntPoint> _borderPoints;

        // Все точки, принадлежащие региону, включая границы
        [Name("Points")]
        public List<IntPoint> Points
        {
            get
            {
                return _points;
            }
            set
            {
                _points = value;
                ProcessPoints();
            }
        }

        // выпуклая оболочка, по которой строится регион
        public IReadOnlyCollection<IntPoint> Border { get { return _borderPoints.AsReadOnly(); } }

        public Region()
        {
            _points = new List<IntPoint>();
            _borderPoints = new List<IntPoint>();
        }

        public Region(IEnumerable<IntPoint> points) :
            this()
        {
            _points.AddRange(points);
            ProcessPoints();
        }

        public void AddPoint(IntPoint p)
        {
            _points.Add(p);
        }

        private void ProcessPoints()
        {
            // TODO
            _borderPoints.AddRange(_points);
        }
    }
}
