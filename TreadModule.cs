using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic; // Required for DBObjectCollection

namespace AuroCADPlugin_3 // Ensure this namespace matches your project
{
    public class TreadModule : IStairModule
    {
        public string ModuleName => "Treads";

        public void Execute(Transaction tr, Database db, StairParameters parameters)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (parameters.NumberOfTreads <= 0)
            {
                ed.WriteMessage("\nSkipping tread creation: NumberOfTreads must be positive.");
                return;
            }
            if (parameters.OuterRadius <= parameters.InnerRadius || parameters.InnerRadius < 0)
            {
                ed.WriteMessage("\nSkipping tread creation: Invalid radii.");
                return;
            }
            if (parameters.TreadThickness <= 0)
            {
                ed.WriteMessage("\nSkipping tread creation: Invalid TreadThickness.");
                return;
            }

            // Get calculated values
            double risePerTread = parameters.CalculatedRiseHeightPerTread;
            double rotationPerTreadRad = parameters.TreadRotation * (Math.PI / 180.0);
            double currentRotationRad = 0;
            double currentHeight = 0; // Start height of the first tread's top surface

            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            for (int i = 0; i < parameters.NumberOfTreads; i++)
            {
                // Calculate angles for this tread
                double startAngleRad = currentRotationRad;
                double endAngleRad = currentRotationRad + rotationPerTreadRad;
                double treadTopZ = currentHeight + risePerTread; // Top surface elevation

                // Define vertices for the top surface of the tread
                Point3d p1 = new Point3d( // Inner Start
                   parameters.InnerRadius * Math.Cos(startAngleRad),
                   parameters.InnerRadius * Math.Sin(startAngleRad),
                   treadTopZ);
                Point3d p2 = new Point3d( // Outer Start
                    parameters.OuterRadius * Math.Cos(startAngleRad),
                    parameters.OuterRadius * Math.Sin(startAngleRad),
                    treadTopZ);
                Point3d p3 = new Point3d( // Outer End
                    parameters.OuterRadius * Math.Cos(endAngleRad),
                    parameters.OuterRadius * Math.Sin(endAngleRad),
                    treadTopZ);
                Point3d p4 = new Point3d( // Inner End
                    parameters.InnerRadius * Math.Cos(endAngleRad),
                    parameters.InnerRadius * Math.Sin(endAngleRad),
                    treadTopZ);

                // Create Polyline for the tread profile (similar to landing)
                using (Polyline pline = new Polyline())
                {
                    double sweepAngle = rotationPerTreadRad;
                    double bulge = Math.Tan(sweepAngle / 4.0);

                    pline.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0); // p1 Inner Start
                    pline.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0); // p2 Outer Start (Line p1->p2)
                    pline.AddVertexAt(2, new Point2d(p3.X, p3.Y), bulge, 0, 0); // p3 Outer End (Arc p2->p3)
                    pline.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0); // p4 Inner End (Line p3->p4)
                    pline.SetBulgeAt(3, bulge); // Bulge for closing arc p4->p1
                    pline.Closed = true;
                    pline.Elevation = treadTopZ; // Set Z elevation

                    // Create Region
                    DBObjectCollection curves = new DBObjectCollection { pline };
                    DBObjectCollection regions = new DBObjectCollection();
                    Region treadRegion = null;
                    Solid3d treadSolid = null; // Declare here for broader scope in try/catch/finally

                    try
                    {
                        regions = Region.CreateFromCurves(curves);
                        if (regions.Count > 0 && regions[0] is Region)
                        {
                            treadRegion = regions[0] as Region;
                        }
                        else
                        {
                            ed.WriteMessage($"\nError: Failed to create region for tread {i + 1}.");
                            continue; // Skip to next tread
                        }

                        // Extrude Region to Solid
                        treadSolid = new Solid3d();
                        // Extrude downwards by thickness
                        treadSolid.Extrude(treadRegion, -parameters.TreadThickness, 0);

                        // Add solid to database
                        btr.AppendEntity(treadSolid);
                        tr.AddNewlyCreatedDBObject(treadSolid, true);
                        // Solid ownership passed to transaction/database
                    }
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage($"\nError processing tread {i + 1}: {ex.Message}");
                        // Dispose solid if created but not added
                        if (treadSolid != null && !treadSolid.IsWriteEnabled && !treadSolid.IsReadEnabled && treadSolid.ObjectId == ObjectId.Null)
                        {
                            if (!treadSolid.IsDisposed) treadSolid.Dispose();
                        }
                    }
                    finally
                    {
                        // Dispose the region
                        if (treadRegion != null && !treadRegion.IsDisposed)
                        {
                            treadRegion.Dispose();
                        }
                        // Dispose any other region objects created
                        foreach (DBObject regionObj in regions)
                        {
                            if (regionObj != null && !regionObj.IsDisposed && regionObj != treadRegion)
                                regionObj.Dispose();
                        }
                        // Polyline is disposed by the 'using' statement
                    }
                } // End using Polyline

                // Update rotation and height for the next tread
                currentRotationRad = endAngleRad; // Or += rotationPerTreadRad for precision
                currentHeight = treadTopZ;

            } // End for loop treads

            ed.WriteMessage($"\n{ModuleName}: {parameters.NumberOfTreads} treads generated.");
        }
    }
}