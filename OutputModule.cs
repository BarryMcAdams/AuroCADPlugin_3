using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public class OutputModule : IOutputModule
    {
        public void Finalize(Document doc, ValidatedStairInput input, StairParameters parameters, EntityCollection entities)
        {
            doc.Editor.Regen();
            doc.Editor.Command("ZOOM", "E"); // Workaround for ZoomExtents

            string message = $"Staircase created successfully!\n" +
                             $"Center Pole Dia: {input.CenterPoleDia:F2} in\n" +
                             $"Overall Height: {input.OverallHeight:F2} in\n" +
                             $"Outside Dia: {input.OutsideDia:F2} in\n" +
                             $"Rotation: {input.RotationDeg:F2}°\n" +
                             $"Treads: {parameters.NumTreads}\n" +
                             $"Riser Height: {parameters.RiserHeight:F2} in\n" +
                             $"Tread Angle: {parameters.TreadAngle:F2}°\n" +
                             $"Walkline Radius: {parameters.WalklineRadius:F2} in\n" +
                             $"Clear Width: {parameters.ClearWidth:F2} in\n" +
                             $"Midlanding: {(parameters.MidlandingIndex >= 0 ? $"Tread {parameters.MidlandingIndex + 1}" : "None")}";
            Application.ShowAlertDialog(message);
        }
    }
}