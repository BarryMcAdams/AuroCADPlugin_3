using Autodesk.AutoCAD.ApplicationServices;
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

            DialogResult result = MessageBox.Show("Export stair info to CSV on Desktop?", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                    writer.WriteLine($"Clear Width,{parameters.ClearWidth:F2}");
                    writer.WriteLine($"Midlanding,{(parameters.MidlandingIndex >= 0 ? $"Tread {parameters.MidlandingIndex + 1}" : "None")}");
                }
                MessageBox.Show($"Exported to {csvPath}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}