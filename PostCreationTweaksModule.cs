﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class PostCreationTweaksModule : IPostCreationTweaksModule
    {
        public EntityCollection ApplyTweaks(Document doc, EntityCollection entities, ValidatedStairInput input, StairParameters parameters)
        {
            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForWrite);

                // Stair info as MText
                string stairInfo = $"Staircase Info:\n" +
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

                var mText = new MText
                {
                    Contents = stairInfo,
                    Location = new Point3d(100, 50, 0),
                    TextHeight = 2.5,
                    Attachment = AttachmentPoint.TopLeft
                };
                btr.AppendEntity(mText);
                tr.AddNewlyCreatedDBObject(mText, true);

                // Mass properties (using Volume, not Mass)
                double totalVolume = 0;
                foreach (var entity in entities.Entities)
                {
                    if (entity is Solid3d solid)
                    {
                        var props = solid.MassProperties;
                        totalVolume += props.Volume; // Volume in cubic inches
                    }
                }
                double density = 0.283; // Steel density, lb/in³—adjust as needed
                double totalWeight = totalVolume * density;

                var massText = new MText
                {
                    Contents = $"Total Weight: {totalWeight:F2} lb (steel, density = {density} lb/in³)",
                    Location = new Point3d(100, 30, 0),
                    TextHeight = 2.5,
                    Attachment = AttachmentPoint.TopLeft
                };
                btr.AppendEntity(massText);
                tr.AddNewlyCreatedDBObject(massText, true);

                tr.Commit();
            }
            return entities;
        }
    }
}