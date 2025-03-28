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
            double baseHeight = parameters.RiserHeight * (parameters.MidlandingIndex + 1) - 0.25; // 9 * 9.5 - 0.25 = 85.25
            double startAngle = parameters.TreadAngle * parameters.MidlandingIndex; // 8 * 25.71 = 205.68°
            double endAngle = startAngle - 90; // 115.68° (CW 90°)
            int numSegments = 16; // Smoothness of arc
            double startRad = startAngle * Math.PI / 180;
            double endRad = endAngle * Math.PI / 180;

            Application.ShowAlertDialog($"MidLanding Specs:\n" +
                                        $"OverallHeight: {parameters.OverallHeight}\n" +
                                        $"OutsideDia: {parameters.OutsideDia}\n" +
                                        $"TreadAngle: {parameters.TreadAngle}\n" +
                                        $"NumTreads: {parameters.NumTreads}\n" +
                                        $"MidlandingIndex: {parameters.MidlandingIndex}\n" +
                                        $"BaseHeight: {baseHeight}");

            using (var pline = new Polyline())
            {
                pline.AddVertexAt(0, new Point2d(radius * Math.Cos(startRad), radius * Math.Sin(startRad)), 0, 0, 0);
                for (int i = 1; i <= numSegments; i++)
                {
                    double angle = startRad + ((endRad - startRad) * i / numSegments) * (parameters.IsClockwise ? -1 : 1);
                    pline.AddVertexAt(i, new Point2d(radius * Math.Cos(angle), radius * Math.Sin(angle)), 0, 0, 0);
                }
                pline.AddVertexAt(numSegments + 1, new Point2d(0, 0), 0, 0, 0);
                pline.Closed = true;
                pline.Elevation = baseHeight;

                using (var regionCollection = Region.CreateFromCurves(new DBObjectCollection { pline }))
                {
                    if (regionCollection.Count == 0) return new Entity[0];
                    var region = (Region)regionCollection[0];
                    var midLanding = new Solid3d();
                    midLanding.Extrude(region, 0.25, 0); // Top at 85.5
                    midLanding.ColorIndex = 1; // Red

                    return new Entity[] { midLanding };
                }
            }
        }
    }
}