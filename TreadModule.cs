using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace SpiralStairPlugin
{
    public class TreadModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            Application.ShowAlertDialog($"TreadModule Params:\n" +
                                        $"NumTreads: {parameters.NumTreads}\n" +
                                        $"RiserHeight: {parameters.RiserHeight}\n" +
                                        $"TreadAngle: {parameters.TreadAngle}\n" +
                                        $"MidlandingIndex: {parameters.MidlandingIndex}");

            var treads = new List<Entity>();
            double innerRadius = parameters.CenterPoleDia / 2; // e.g., 5 / 2 = 2.5 inches
            double outerRadius = parameters.OutsideDia / 2; // e.g., 60 / 2 = 30 inches
            double cumulativeAngle = 0;
            int treadCount = 0;

            // Below Mid-Landing (treads 1 to MidlandingIndex - 1)
            for (int i = 1; i < parameters.MidlandingIndex; i++)
            {
                double zPos = i * parameters.RiserHeight; // Start at Z=9.5
                double startAngle = cumulativeAngle;
                double treadAngle = parameters.TreadAngle;

                var tread = TreadGeometry.CreateTread(innerRadius, outerRadius, startAngle, treadAngle, zPos);
                if (tread == null)
                {
                    Application.ShowAlertDialog($"Tread {i} is null");
                    continue;
                }
                treads.Add(tread);
                treadCount++;
                cumulativeAngle += treadAngle;
            }

            // Above Mid-Landing (treads MidlandingIndex + 1 to NumTreads - 2, leaving last for top landing)
            for (int i = parameters.MidlandingIndex + 1; i < parameters.NumTreads - 1; i++)
            {
                double zPos = i * parameters.RiserHeight; // e.g., Z=85.5 to Z=142.5
                double startAngle = cumulativeAngle;
                double treadAngle = parameters.TreadAngle;

                var tread = TreadGeometry.CreateTread(innerRadius, outerRadius, startAngle, treadAngle, zPos);
                if (tread == null)
                {
                    Application.ShowAlertDialog($"Tread {i} is null");
                    continue;
                }
                treads.Add(tread);
                treadCount++;
                cumulativeAngle += treadAngle;
            }

            Application.ShowAlertDialog($"Total Treads Created: {treadCount}");
            return treads.ToArray();
        }
    }
}