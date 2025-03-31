using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class PostCreationTweaksModule : IPostCreationTweaksModule
    {
        public EntityCollection ApplyTweaks(Document doc, EntityCollection entities, ValidatedStairInput input, StairParameters parameters)
        {
            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForWrite);
                var layerTable = (LayerTable)tr.GetObject(doc.Database.LayerTableId, OpenMode.ForWrite);

                // Create or get "Text" layer
                string layerName = "Text";
                if (!layerTable.Has(layerName))
                {
                    var newLayer = new LayerTableRecord
                    {
                        Name = layerName,
                        Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 7) // Black
                    };
                    layerTable.Add(newLayer);
                    tr.AddNewlyCreatedDBObject(newLayer, true);
                }

                // Set "Text" as current layer
                doc.Database.Clayer = layerTable[layerName];

                // Calculate walkline distance
                double walklineRadius = (parameters.CenterPoleDia / 2) + 12;
                double walklineDistance = walklineRadius * (parameters.TreadAngle * Math.PI / 180);

                // Calculate total weight
                double totalVolume = 0;
                foreach (var entity in entities.Entities)
                {
                    if (entity is Solid3d solid)
                    {
                        var props = solid.MassProperties;
                        totalVolume += props.Volume;
                    }
                }
                double density = 0.0975; // Aluminum, lb/in³
                double totalWeight = totalVolume * density;

                // Stair info as MText
                string stairInfo = $"Spiral Stair Info:\n" +
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
                                   $"Mid-landing: {(parameters.MidlandingIndex >= 0 ? $"Tread {parameters.MidlandingIndex + 1}" : "None")}";
                if (!parameters.IsCompliant && !string.IsNullOrEmpty(parameters.ComplianceMessage))
                    stairInfo += $"\nCode Violations:\n{parameters.ComplianceMessage}";
                stairInfo += $"\nTotal Weight: {totalWeight:F2} lb (aluminum, density = {density} lb/in³)";

                var mText = new MText
                {
                    Contents = stairInfo,
                    Location = new Point3d(100, 50, 0),
                    TextHeight = 2.5,
                    Attachment = AttachmentPoint.TopLeft,
                    Layer = layerName // Explicitly assign to "Text" layer
                };
                btr.AppendEntity(mText);
                tr.AddNewlyCreatedDBObject(mText, true);

                tr.Commit();
            }
            return entities;
        }
    }
}