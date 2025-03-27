using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class HandrailModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            double radius = parameters.OutsideDia / 2;
            double handrailDia = 1; // 1" diameter handrail
            double heightOffset = 36; // Handrail 36" above base

            using (var circle = new Circle(new Point3d(radius, 0, heightOffset), Vector3d.ZAxis, handrailDia / 2))
            using (var regionCollection = Region.CreateFromCurves(new DBObjectCollection { circle }))
            {
                var region = (Region)regionCollection[0];

                using (var helix = new Helix())
                {
                    helix.BaseRadius = radius;
                    helix.TopRadius = radius;
                    helix.Height = parameters.OverallHeight;
                    helix.Turns = parameters.RotationDeg / 360.0;
                    helix.StartPoint = new Point3d(radius, 0, heightOffset);
                    helix.Twist = !parameters.IsClockwise; // True = counterclockwise, False = clockwise

                    var handrail = new Solid3d();
                    handrail.CreateSweptSolid(region, helix, new SweepOptions());
                    handrail.ColorIndex = 2; // Yellow for visibility

                    return new Entity[] { handrail };
                }
            }
        }
    }
}