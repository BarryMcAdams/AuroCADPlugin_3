using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class TopLandingModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            double radius = parameters.OutsideDia / 2; // e.g., 30"
            double baseHeight = parameters.OverallHeight - 0.25; // Top at OverallHeight - 0.25"
            int topTreadIndex = parameters.MidlandingIndex >= 0 ? parameters.NumTreads - 2 : parameters.NumTreads - 1; // Last tread before landing
            double endAngle = (topTreadIndex + 1) * parameters.TreadAngle; // End of top tread (left edge)
            double endRad = endAngle * Math.PI / 180 * (parameters.IsClockwise ? -1 : 1); // Clockwise adjusts direction

            using (var pline = new Polyline())
            {
                // Square landing: 30" wide (radius) x 30" deep, aligned with tread’s end
                pline.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0); // Center
                pline.AddVertexAt(1, new Point2d(radius, 0), 0, 0, 0); // Outer edge (start at tread’s left)
                pline.AddVertexAt(2, new Point2d(radius, radius), 0, 0, 0); // Extend inward (Y positive)
                pline.AddVertexAt(3, new Point2d(0, radius), 0, 0, 0); // Back to center line
                pline.Closed = true;
                pline.Elevation = baseHeight;

                using (var regionCollection = Region.CreateFromCurves(new DBObjectCollection { pline }))
                {
                    if (regionCollection.Count == 0) return new Entity[0];
                    var region = (Region)regionCollection[0];
                    var topLanding = new Solid3d();
                    topLanding.Extrude(region, 0.25, 0); // 0.25" thick
                    topLanding.ColorIndex = 3; // Green

                    // Rotate 90° clockwise from top tread’s end angle
                    double rotationAdjustment = parameters.IsClockwise ? Math.PI / 2 : -Math.PI / 2; // +90° clockwise, -90° counterclockwise
                    topLanding.TransformBy(Matrix3d.Rotation(endRad + rotationAdjustment, Vector3d.ZAxis, new Point3d(0, 0, baseHeight + 0.125)));

                    return new Entity[] { topLanding };
                }
            }
        }
    }
}