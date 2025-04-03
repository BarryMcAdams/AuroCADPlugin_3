using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class TreadModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            // Placeholder for tread creation logic
            // This is a simplified version; adjust based on your actual tread generation logic
            Entity[] createdEntities = new Entity[parameters.NumberOfTreads];
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForWrite);

                    for (int i = 0; i < parameters.NumberOfTreads; i++)
                    {
                        double angle = i * parameters.TreadRotation * Math.PI / 180.0 * (parameters.IsClockwise ? -1 : 1);
                        double height = i * parameters.CalculatedRiseHeightPerTread;
                        double cosAngle = Math.Cos(angle);
                        double sinAngle = Math.Sin(angle);

                        // Define tread as a simple line for demonstration
                        Point3d innerPoint = new Point3d(parameters.InnerRadius * cosAngle, parameters.InnerRadius * sinAngle, height);
                        Point3d outerPoint = new Point3d(parameters.OuterRadius * cosAngle, parameters.OuterRadius * sinAngle, height);

                        Line treadLine = new Line(innerPoint, outerPoint) { ColorIndex = 1 }; // Red for visibility
                        btr.AppendEntity(treadLine);
                        tr.AddNewlyCreatedDBObject(treadLine, true);
                    }

                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    doc.Editor.WriteMessage($"\nError creating treads: {ex.Message}");
                    tr.Abort();
                }
            }

            return createdEntities; // Return empty array since entities are added directly to the database
        }
    }
}