using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class TreadGeometry
    {
        public static Entity CreateTread(double innerRadius, double outerRadius, double startAngle, double treadAngle, double zPos)
        {
            startAngle = startAngle * Math.PI / 180;
            treadAngle = treadAngle * Math.PI / 180;
            var pline = new Polyline();
            int numSegments = 10;

            pline.AddVertexAt(0, new Point2d(innerRadius * Math.Cos(startAngle), innerRadius * Math.Sin(startAngle)), 0, 0, 0);
            pline.AddVertexAt(1, new Point2d(outerRadius * Math.Cos(startAngle), outerRadius * Math.Sin(startAngle)), 0, 0, 0);
            for (int i = 1; i <= numSegments; i++)
            {
                double angle = startAngle + (treadAngle * i / numSegments);
                pline.AddVertexAt(1 + i, new Point2d(outerRadius * Math.Cos(angle), outerRadius * Math.Sin(angle)), 0, 0, 0);
            }
            pline.AddVertexAt(2 + numSegments, new Point2d(innerRadius * Math.Cos(startAngle + treadAngle), innerRadius * Math.Sin(startAngle + treadAngle)), 0, 0, 0);
            pline.Closed = true;
            pline.Elevation = zPos;

            using (var curves = new DBObjectCollection { pline })
            {
                var regions = Region.CreateFromCurves(curves);
                if (regions == null || regions.Count == 0)
                {
                    Application.ShowAlertDialog("Failed to create region from tread polyline");
                    return null;
                }
                var region = (Region)regions[0];
                var solid = new Solid3d();
                solid.Extrude(region, 0.25, 0); // Extrudes up from zPos
                solid.ColorIndex = 4; // Cyan
                return solid;
            }
        }
    }
}