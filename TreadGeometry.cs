using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class TreadGeometry
    {
        public static Entity CreateTread(double innerRadius, double outerRadius, double startAngle, double treadAngle, double zPos)
        {
            // Convert angles to radians
            startAngle = startAngle * Math.PI / 180;
            treadAngle = treadAngle * Math.PI / 180;

            // Create 2D polyline for tread shape
            var pline = new Polyline();
            int numSegments = 10; // Smoothness of outer arc

            // Add inner start
            pline.AddVertexAt(0, new Point2d(innerRadius * Math.Cos(startAngle), innerRadius * Math.Sin(startAngle)), 0, 0, 0);

            // Add outer start
            pline.AddVertexAt(1, new Point2d(outerRadius * Math.Cos(startAngle), outerRadius * Math.Sin(startAngle)), 0, 0, 0);

            // Add arc points from startAngle to startAngle + treadAngle
            for (int i = 1; i <= numSegments; i++)
            {
                double angle = startAngle + (treadAngle * i / numSegments);
                pline.AddVertexAt(1 + i, new Point2d(outerRadius * Math.Cos(angle), outerRadius * Math.Sin(angle)), 0, 0, 0);
            }

            // Add inner end
            pline.AddVertexAt(2 + numSegments, new Point2d(innerRadius * Math.Cos(startAngle + treadAngle), innerRadius * Math.Sin(startAngle + treadAngle)), 0, 0, 0);
            pline.Closed = true;

            // Convert Polyline to Region
            using (var curves = new DBObjectCollection { pline })
            {
                var regions = Region.CreateFromCurves(curves);
                if (regions == null || regions.Count == 0)
                {
                    throw new Exception("Failed to create region from tread polyline");
                }
                var region = (Region)regions[0];

                // Extrude to 3D solid
                var solid = new Solid3d();
                solid.Extrude(region, 0.25, 0); // 0.25" tread thickness
                solid.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, zPos))); // Move to correct Z-position
                solid.ColorIndex = 4; // Cyan

                return solid;
            }
        }
    }
}