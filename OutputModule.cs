using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.IO;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class OutputModule : IOutputModule
    {
        public void Finalize(Document doc, ValidatedStairInput input, StairParameters parameters, EntityCollection entities)
        {
            doc.Editor.Regen();
            doc.Editor.Command("ZOOM", "E");

            // Calculate total weight and walkline distance
            double totalVolume = 0;
            foreach (var entity in entities.Entities)
            {
                if (entity is Solid3d solid)
                {
                    var props = solid.MassProperties;
                    totalVolume += props.Volume;
                }
            }
            double density = 0.283; // Steel, lb/in³
            double totalWeight = totalVolume * density;
            double walklineRadius = (parameters.CenterPoleDia / 2) + 12;
            double walklineDistance = walklineRadius * (parameters.TreadAngle * Math.PI / 180);

            // Final stair details in prompt
            string stairInfo = $"Staircase Info:\n" +
                               $"Center Pole Dia: {input.CenterPoleDia:F2} in\n" +
                               $"Overall Height: {input.OverallHeight:F2} in\n" +
                               $"Outside Dia: {input.OutsideDia:F2} in\n" +
                               $"Rotation: {input.RotationDeg:F2}°\n" +
                               $"Treads: {parameters.NumTreads}\n" +
                               $"Riser Height: {parameters.RiserHeight:F2} in\n" +
                               $"Tread Angle: {parameters.TreadAngle:F2}°\n" +
                               $"Walkline Radius: {parameters.WalklineRadius:F2} in\n" +
                               $"Walkline Distance: {walklineDistance:F2} in\n" +
                               $"Clear Width: {parameters.ClearWidth:F2} in\n" +
                               $"Midlanding: {(parameters.MidlandingIndex >= 0 ? $"Tread {parameters.MidlandingIndex + 1}" : "None")}\n" +
                               $"Total Weight: {totalWeight:F2} lb\n\n" +
                               "Export to CSV on Desktop?";

            DialogResult result = MessageBox.Show(stairInfo, "Staircase Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string csvPath = Path.Combine(desktopPath, $"SpiralStair_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

                using (StreamWriter writer = new StreamWriter(csvPath))
                {
                    writer.WriteLine("Property,Value");
                    writer.WriteLine($"Center Pole Dia,{input.CenterPoleDia:F2}");
                    writer.WriteLine($"Overall Height,{input.OverallHeight:F2}");
                    writer.WriteLine($"Outside Dia,{input.OutsideDia:F2}");
                    writer.WriteLine($"Rotation,{input.RotationDeg:F2}");
                    writer.WriteLine($"Treads,{parameters.NumTreads}");
                    writer.WriteLine($"Riser Height,{parameters.RiserHeight:F2}");
                    writer.WriteLine($"Tread Angle,{parameters.TreadAngle:F2}");
                    writer.WriteLine($"Walkline Radius,{parameters.WalklineRadius:F2}");
                    writer.WriteLine($"Walkline Distance,{walklineDistance:F2}");
                    writer.WriteLine($"Clear Width,{parameters.ClearWidth:F2}");
                    writer.WriteLine($"Midlanding,{(parameters.MidlandingIndex >= 0 ? $"Tread {parameters.MidlandingIndex + 1}" : "None")}");
                    writer.WriteLine($"Total Weight,{totalWeight:F2}");
                }
                MessageBox.Show($"Exported to {csvPath}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}