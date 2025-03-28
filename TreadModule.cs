using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace SpiralStairPlugin
{
    public class TreadModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            var treads = new List<Entity>();
            double innerRadius = parameters.CenterPoleDia / 2; // 2.5 inches
            double outerRadius = parameters.OutsideDia / 2; // 30 inches
            double cumulativeAngle = 0;

            for (int i = 0; i < parameters.NumTreads - 1; i++) // 0 to 14 (15 treads)
            {
                if (i == parameters.MidlandingIndex) // Skip mid-landing tread
                {
                    cumulativeAngle += 90; // Mid-landing spans 90°
                    continue;
                }

                double treadHeight = parameters.RiserHeight * (i + 1) - 0.25; // Base height, e.g., 9.25
                if (treadHeight + 0.25 > parameters.OverallHeight) treadHeight = parameters.OverallHeight - 0.25;

                double startAngle = cumulativeAngle;
                double treadAngle = parameters.TreadAngle;

                var tread = TreadGeometry.CreateTread(innerRadius, outerRadius, startAngle, treadAngle, treadHeight);
                if (tread != null)
                {
                    treads.Add(tread);
                }
                cumulativeAngle += treadAngle;
            }

            return treads.ToArray();
        }
    }
}