using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SpiralStairPlugin;

namespace AuroCADPlugin_3
{
    public class TopLandingModule : IGeometryCreator
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            double outerRadius = parameters.OuterRadius; // e.g., 36"
            double baseHeight = parameters.FloorHeight; // Z = 144 (top of stair)
            // Use TotalRotation as the total rotation (tread #15’s left edge)
            double exitAngleRad = parameters.TotalRotation * Math.PI / 180.0 * (parameters.IsClockwise ? -1 : 1);

            // Precompute sine and cosine
            double cosExit = Math.Cos(exitAngleRad);
            double sinExit = Math.Sin(exitAngleRad);

            // Define the landing corners
            // Left edge (ptA_3d to ptB_3d) aligns with tread #15’s left edge
            Point3d ptA_3d = new(0, 0, baseHeight); // Center of center pole
            Point3d ptB_3d = new(outerRadius * cosExit, outerRadius * sinExit, baseHeight); // Left edge at tread #15’s left edge

            // Tangent direction (90° counterclockwise from radial for clockwise stairs to extend left)
            double tangentDirX = parameters.IsClockwise ? -sinExit : sinExit;
            double tangentDirY = parameters.IsClockwise ? cosExit : -cosExit;

            // Offset by 50 units to form the right edge (ptD_3d to ptC_3d)
            Point3d ptD_3d = new(
                ptA_3d.X + 50.0 * tangentDirX,
                ptA_3d.Y + 50.0 * tangentDirY,
                baseHeight
            );
            Point3d ptC_3d = new(
                ptB_3d.X + 50.0 * tangentDirX,
                ptB_3d.Y + 50.0 * tangentDirY,
                baseHeight
            );

            // Create the four lines
            Entity[] createdEntities = new Entity[0];
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    doc.Editor.WriteMessage("\nOpening BlockTableRecord...");
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForWrite);
                    doc.Editor.WriteMessage("\nBlockTableRecord opened successfully.");

                    // Left edge (ptA_3d to ptB_3d)
                    doc.Editor.WriteMessage("\nCreating left edge line for top landing...");
                    Line leftEdge = new(ptA_3d, ptB_3d) { ColorIndex = 3 }; // Green for visibility
                    btr.AppendEntity(leftEdge);
                    tr.AddNewlyCreatedDBObject(leftEdge, true);
                    doc.Editor.WriteMessage("\nLeft edge line appended successfully.");

                    // Top edge (ptB_3d to ptC_3d)
                    doc.Editor.WriteMessage("\nCreating top edge line for top landing...");
                    Line topEdge = new(ptB_3d, ptC_3d) { ColorIndex = 3 };
                    btr.AppendEntity(topEdge);
                    tr.AddNewlyCreatedDBObject(topEdge, true);
                    doc.Editor.WriteMessage("\nTop edge line appended successfully.");

                    // Right edge (ptC_3d to ptD_3d)
                    doc.Editor.WriteMessage("\nCreating right edge line for top landing...");
                    Line rightEdge = new(ptC_3d, ptD_3d) { ColorIndex = 3 };
                    btr.AppendEntity(rightEdge);
                    tr.AddNewlyCreatedDBObject(rightEdge, true);
                    doc.Editor.WriteMessage("\nRight edge line appended successfully.");

                    // Bottom edge (ptD_3d to ptA_3d)
                    doc.Editor.WriteMessage("\nCreating bottom edge line for top landing...");
                    Line bottomEdge = new(ptD_3d, ptA_3d) { ColorIndex = 3 };
                    btr.AppendEntity(bottomEdge);
                    tr.AddNewlyCreatedDBObject(bottomEdge, true);
                    doc.Editor.WriteMessage("\nBottom edge line appended successfully.");

                    createdEntities = new Entity[0]; // Empty array to avoid double-adding

                    tr.Commit();
                    doc.Editor.WriteMessage("\nTransaction committed successfully.");
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