using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic; // Required for DBObjectCollection

namespace AuroCADPlugin_3 // Ensure this namespace matches your project
{
    public class TopLandingModule : IStairModule
    {
        public string ModuleName => "TopLanding";

        public void Execute(Transaction tr, Database db, StairParameters parameters)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (parameters.NumberOfTreads <= 0)
            {
                ed.WriteMessage("\nSkipping landing creation: NumberOfTreads must be greater than 0.");
                return;
            }
            if (parameters.OuterRadius <= parameters.InnerRadius || parameters.InnerRadius < 0)
            {
                ed.WriteMessage("\nSkipping landing creation: Invalid radii (Outer <= Inner or Inner < 0).");
                return;
            }
            if (parameters.TreadThickness <= 0)
            {
                ed.WriteMessage("\nSkipping landing creation: Invalid TreadThickness.");
                return;
            }
            if (parameters.LandingWidth <= 0)
            {
                ed.WriteMessage("\nSkipping landing creation: LandingWidth must be positive.");
                return;
            }

            // --- 1. Calculate Final Rotation and Landing Angles ---
            double finalRotationRad = parameters.TotalRotation * (Math.PI / 180.0); // Rotation of last tread's leading edge
            double landingAngleRad = parameters.LandingWidth * (Math.PI / 180.0);
            double startAngleRad = finalRotationRad; // Landing starts where last tread ends
            double endAngleRad = startAngleRad + landingAngleRad;

            // --- 2. Determine Landing Elevation and Thickness ---
            double landingZElevation = parameters.FloorHeight; // Top surface Z from parameters
            double landingThickness = parameters.TreadThickness; // Use TreadThickness for consistency

            // --- 3. Define Landing Geometry Vertices (Top Surface) ---
            Point3d p1 = new Point3d( // Inner Start
                parameters.InnerRadius * Math.Cos(startAngleRad),
                parameters.InnerRadius * Math.Sin(startAngleRad),
                landingZElevation);
            Point3d p2 = new Point3d( // Outer Start
                parameters.OuterRadius * Math.Cos(startAngleRad),
                parameters.OuterRadius * Math.Sin(startAngleRad),
                landingZElevation);
            Point3d p3 = new Point3d( // Outer End
                parameters.OuterRadius * Math.Cos(endAngleRad),
                parameters.OuterRadius * Math.Sin(endAngleRad),
                landingZElevation);
            Point3d p4 = new Point3d( // Inner End
                parameters.InnerRadius * Math.Cos(endAngleRad),
                parameters.InnerRadius * Math.Sin(endAngleRad),
                landingZElevation);

            // --- 4. Create 2D Profile (Polyline with bulge for arcs) ---
            using (Polyline pline = new Polyline())
            {
                double sweepAngle = landingAngleRad;
                double bulge = Math.Tan(sweepAngle / 4.0);

                // Vertices: p1 -> p2 -> p3 -> p4 -> p1
                pline.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);      // Vertex 0: p1 (Inner Start)
                pline.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);      // Vertex 1: p2 (Outer Start), Line p1->p2
                pline.AddVertexAt(2, new Point2d(p3.X, p3.Y), bulge, 0, 0);  // Vertex 2: p3 (Outer End), Arc p2->p3 (Set bulge on starting vertex p2)
                pline.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);      // Vertex 3: p4 (Inner End), Line p3->p4
                // Set bulge for the closing arc p4->p1 on the starting vertex of that segment (p4, which is index 3)
                pline.SetBulgeAt(3, bulge);
                pline.Closed = true;
                pline.Elevation = landingZElevation; // Set elevation for the 2D profile

                // --- 5. Create Region from Polyline ---
                DBObjectCollection curves = new DBObjectCollection { pline };
                DBObjectCollection regions = new DBObjectCollection();
                Region landingRegion = null;
                Solid3d landingSolid = null; // Declare higher for broader scope

                try
                {
                    regions = Region.CreateFromCurves(curves);
                    if (regions.Count > 0 && regions[0] is Region)
                    {
                        landingRegion = regions[0] as Region;
                    }
                    else
                    {
                        ed.WriteMessage("\nError: Failed to create region for top landing.");
                        return; // Exit if region fails
                    }

                    // --- 6. Extrude Region to Create 3D Solid ---
                    landingSolid = new Solid3d();
                    landingSolid.Extrude(landingRegion, -landingThickness, 0); // Extrude downwards

                    // --- 7. Add Solid to Database ---
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    btr.AppendEntity(landingSolid);
                    tr.AddNewlyCreatedDBObject(landingSolid, true);
                    // Solid ownership passed to transaction

                    ed.WriteMessage($"\n{ModuleName}: Landing created successfully.");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\nError during landing processing: {ex.Message}");
                    // Dispose solid if created but not added
                    if (landingSolid != null && !landingSolid.IsWriteEnabled && !landingSolid.IsReadEnabled && landingSolid.ObjectId == ObjectId.Null)
                    {
                        if (!landingSolid.IsDisposed) landingSolid.Dispose();
                    }
                }
                finally
                {
                    // Dispose the region
                    if (landingRegion != null && !landingRegion.IsDisposed)
                    {
                        landingRegion.Dispose();
                    }
                    // Dispose any other region objects
                    foreach (DBObject regionObj in regions)
                    {
                        if (regionObj != null && !regionObj.IsDisposed && regionObj != landingRegion)
                            regionObj.Dispose();
                    }
                    // Polyline disposed by 'using'
                }
            } // End using Polyline
        }
    }
}