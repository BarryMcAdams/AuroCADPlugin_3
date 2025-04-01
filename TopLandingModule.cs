using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class TopLandingModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            // Calculate dimensions and height
            double outerRadius = parameters.OutsideDia / 2.0; // e.g., 36"
            double treadHeight = parameters.RiserHeight * 15 - 0.25; // Top of tread #15, e.g., 134.75
            if (treadHeight + 0.25 > parameters.OverallHeight) treadHeight = parameters.OverallHeight - 0.25;
            double baseHeight = treadHeight;

            // Normalize RotationDeg to [0, 360) and convert to radians with clockwise adjustment
            double normalizedRotationDeg = parameters.RotationDeg % 360.0;
            if (normalizedRotationDeg < 0) normalizedRotationDeg += 360.0;
            double exitAngleRad = normalizedRotationDeg * Math.PI / 180.0 * (parameters.IsClockwise ? -1 : 1);

            // Precompute sine and cosine for the exit angle (tread #15’s left edge)
            double cosExit = Math.Cos(exitAngleRad);
            double sinExit = Math.Sin(exitAngleRad);

            // Define landing corners
            // ptA_3d: Center base, ptB_3d: Left edge (radial, aligned with tread #15)
            Point3d ptA_3d = new Point3d(0, 0, baseHeight);
            Point3d ptB_3d = new Point3d(outerRadius * cosExit, outerRadius * sinExit, baseHeight);

            // Tangent direction: 90° clockwise from radial for clockwise stairs (e.g., -90° → -X)
            double tangentDirX = parameters.IsClockwise ? sinExit : -sinExit;
            double tangentDirY = parameters.IsClockwise ? -cosExit : cosExit;

            // Right edge: Offset 50 units tangentially from left edge
            Point3d ptC_3d = new Point3d(
                ptB_3d.X + 50.0 * tangentDirX,
                ptB_3d.Y + 50.0 * tangentDirY,
                baseHeight
            );
            Point3d ptD_3d = new Point3d(
                ptA_3d.X + 50.0 * tangentDirX,
                ptA_3d.Y + 50.0 * tangentDirY,
                baseHeight
            );

            // Create the four lines
            Entity[] createdEntities = new Entity[0];
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForWrite);

                    // Left edge (ptA_3d to ptB_3d)
                    Line leftEdge = new Line(ptA_3d, ptB_3d);
                    leftEdge.ColorIndex = 3; // Green
                    btr.AppendEntity(leftEdge);
                    tr.AddNewlyCreatedDBObject(leftEdge, true);

                    // Top edge (ptB_3d to ptC_3d)
                    Line topEdge = new Line(ptB_3d, ptC_3d);
                    topEdge.ColorIndex = 3;
                    btr.AppendEntity(topEdge);
                    tr.AddNewlyCreatedDBObject(topEdge, true);

                    // Right edge (ptC_3d to ptD_3d)
                    Line rightEdge = new Line(ptC_3d, ptD_3d);
                    rightEdge.ColorIndex = 3;
                    btr.AppendEntity(rightEdge);
                    tr.AddNewlyCreatedDBObject(rightEdge, true);

                    // Bottom edge (ptD_3d to ptA_3d)
                    Line bottomEdge = new Line(ptD_3d, ptA_3d);
                    bottomEdge.ColorIndex = 3;
                    btr.AppendEntity(bottomEdge);
                    tr.AddNewlyCreatedDBObject(bottomEdge, true);

                    createdEntities = new Entity[0]; // Avoid double-adding
                    tr.Commit();
                }
                catch (Exception ex)
                {
                    doc.Editor.WriteMessage($"\nError creating top landing lines: {ex.Message}");
                    tr.Abort();
                    createdEntities = new Entity[0];
                }
            }

            return createdEntities;
        }
    }
}