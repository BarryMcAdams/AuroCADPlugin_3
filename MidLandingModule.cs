using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class MidLandingModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            if (parameters.MidlandingIndex < 0)
            {
                return new Entity[0]; // No midlanding needed
            }

            double size = parameters.OutsideDia / 2;
            double baseHeight = parameters.RiserHeight * (parameters.MidlandingIndex + 1); // Base at tread height
            Point3d[] points = new Point3d[]
            {
                new Point3d(0, 0, baseHeight),
                new Point3d(size, 0, baseHeight),
                new Point3d(size, size, baseHeight),
                new Point3d(0, size, baseHeight)
            };

            using (var pline = new Polyline())
            {
                for (int i = 0; i < 4; i++)
                    pline.AddVertexAt(i, new Point2d(points[i].X, points[i].Y), 0, 0, 0);
                pline.Closed = true;
                pline.Elevation = baseHeight; // Explicitly set elevation to ensure Z-position

                using (var regionCollection = Region.CreateFromCurves(new DBObjectCollection { pline }))
                {
                    if (regionCollection.Count == 0) return new Entity[0]; // Safety check
                    var region = (Region)regionCollection[0];
                    var midLanding = new Solid3d();
                    midLanding.Extrude(region, 0.25, 0); // 0.25" height, top at baseHeight + 0.25
                    midLanding.ColorIndex = 1; // Red per flow chart

                    return new Entity[] { midLanding };
                }
            }
        }
    }
}