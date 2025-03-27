using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class CenterPoleModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            using (var circle = new Circle(Point3d.Origin, Vector3d.ZAxis, parameters.CenterPoleDia / 2))
            using (var regionCollection = Region.CreateFromCurves(new DBObjectCollection { circle }))
            {
                var region = (Region)regionCollection[0];
                var centerPole = new Solid3d();
                centerPole.Extrude(region, parameters.OverallHeight, 0);
                centerPole.ColorIndex = 251; // Gray per flow chart
                // No displacement; bottom at Z=0, top at OverallHeight

                return new Entity[] { centerPole };
            }
        }
    }
}