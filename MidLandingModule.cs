using System;
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
                return new Entity[0]; // No midlanding if height ≤ 151"
            }

            double radius = parameters.OutsideDia / 2; // 30 inches
            double baseHeight = parameters.RiserHeight * (parameters.MidlandingIndex + 1); // e.g., 76 inches
            int numSegments = 16; // Smoothness of arc
            double startAngle = 0; // 0°
            double endAngle = Math.PI / 2; // 90° in radians

            using (var pline = new Polyline())
            {
                // Start at (radius, 0, baseHeight)
                pline.AddVertexAt(0, new Point2d(radius, 0), 0, 0, 0);

                // Arc from 0° to 90°
                for (int i = 1; i <= numSegments; i++)
                {
                    double angle = startAngle + (endAngle - startAngle) * i / numSegments;
                    double x = radius * Math.Cos(angle);
                    double y = radius * Math.Sin(angle);
                    pline.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);
                }

                // Radial edges back to origin
                pline.AddVertexAt(numSegments + 1, new Point2d(0, 0), 0, 0, 0);
                pline.Closed = true;
                pline.Elevation = baseHeight; // Set Z-position

                using (var regionCollection = Region.CreateFromCurves(new DBObjectCollection { pline }))
                {
                    if (regionCollection.Count == 0) return new Entity[0];
                    var region = (Region)regionCollection[0];
                    var midLanding = new Solid3d();
                    midLanding.Extrude(region, 0.25, 0); // 0.25" height for now
                    midLanding.ColorIndex = 1; // Red

                    return new Entity[] { midLanding };
                }
            }
        }
    }
}