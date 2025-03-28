using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class TopLandingModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            double width = 50; // Fixed width per flow chart
            double length = parameters.OutsideDia / 2; // 30 inches for 60" diameter
            double baseHeight = parameters.OverallHeight - 0.25; // Top at 152, base at 151.75
            Point3d[] points = new Point3d[]
            {
                new Point3d(0, 0, baseHeight),
                new Point3d(width, 0, baseHeight),
                new Point3d(width, length, baseHeight),
                new Point3d(0, length, baseHeight)
            };

            using (var pline = new Polyline())
            {
                for (int i = 0; i < 4; i++)
                    pline.AddVertexAt(i, new Point2d(points[i].X, points[i].Y), 0, 0, 0);
                pline.Closed = true;
                pline.Elevation = baseHeight; // Ensure Z-position

                using (var regionCollection = Region.CreateFromCurves(new DBObjectCollection { pline }))
                {
                    if (regionCollection.Count == 0) return new Entity[0];
                    var region = (Region)regionCollection[0];
                    var landing = new Solid3d();
                    landing.Extrude(region, 0.25, 0); // 0.25" height
                    landing.ColorIndex = 3; // Green

                    return new Entity[] { landing };
                }
            }
        }
    }
}