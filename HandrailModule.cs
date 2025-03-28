using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class HandrailModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            // Debug: Log incoming parameters
            Application.ShowAlertDialog($"Handrail Input Params:\n" +
                                        $"CenterPoleDia: {parameters.CenterPoleDia}\n" +
                                        $"OverallHeight: {parameters.OverallHeight}\n" +
                                        $"OutsideDia: {parameters.OutsideDia}\n" +
                                        $"RotationDeg: {parameters.RotationDeg}\n" +
                                        $"IsClockwise: {parameters.IsClockwise}");

            double radius = parameters.OutsideDia / 2; // 60 / 2 = 30 inches
            double heightOffset = 36; // Handrail starts at Z=36 after move
            double height = parameters.OverallHeight; // Should be 152 inches
            double turns = parameters.RotationDeg / 360.0; // 450 / 360 = 1.25 turns

            // Step 1: Create helix at (0, 0, 0) with base/top diameter and height
            var helix = new Helix();
            helix.StartPoint = new Point3d(0, 0, 0); // Base point at origin
            helix.BaseRadius = radius; // 30 inches
            helix.TopRadius = radius; // 30 inches
            helix.Height = height; // 152 inches
            helix.ColorIndex = 2; // Yellow

            // Step 2: Adjust turns to 1.25 (default is 3.0)
            helix.Turns = turns; // 1.25 turns

            // Step 3: Move helix to Z=36
            helix.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, heightOffset)));

            // Debug: Log helix properties after setting
            Application.ShowAlertDialog($"Helix Created Params:\n" +
                                        $"BaseRadius: {helix.BaseRadius}\n" +
                                        $"TopRadius: {helix.TopRadius}\n" +
                                        $"Height: {helix.Height}\n" +
                                        $"Turns: {helix.Turns}\n" +
                                        $"StartPoint: {helix.StartPoint}\n" +
                                        $"Twist: {(helix.Twist ? "Counterclockwise" : "Clockwise")}");

            return new Entity[] { helix };
        }
    }
}