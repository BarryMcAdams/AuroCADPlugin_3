﻿using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace SpiralStairPlugin
{
    public class SpiralStairCommand
    {
        [CommandMethod("CreateSpiralStair")]
        public void Execute()
        {
            try
            {
                // Step 1: Initialization
                IInitializationModule initializer = new InitializationModule();
                var context = initializer.Initialize();
                if (context == null) return;

                // Step 2 & 3: Input and Validation with retry loop
                IInputModule inputModule = new InputModule();
                IValidationModule validationModule = new ValidationModule(initializer.GetCenterPoleOptions());
                ValidatedStairInput input = null;
                bool isValid = false;
                do
                {
                    var rawInput = inputModule.GetInput(context.Document);
                    if (!rawInput.Submitted) return;

                    input = validationModule.Validate(rawInput);
                    isValid = input.IsValid;
                    if (!isValid) inputModule.ShowRetryPrompt(input.ErrorMessage);
                } while (!isValid);

                // Step 4: Calculation with compliance check
                ICalculationModule calculationModule = new CalculationModule();
                StairParameters parameters = null;
                do
                {
                    parameters = calculationModule.Calculate(input);
                    if (!parameters.IsCompliant)
                    {
                        var retryOption = calculationModule.HandleComplianceFailure(parameters);
                        if (retryOption == ComplianceRetryOption.Abort) return;
                        if (retryOption == ComplianceRetryOption.Retry)
                        {
                            var adjustedInput = inputModule.GetAdjustedInput(context.Document, parameters);
                            if (!adjustedInput.Submitted) return;
                            input = validationModule.Validate(adjustedInput); // Re-validate adjusted input
                            continue;
                        }
                        break; // Ignore case
                    }
                    break; // Compliant case
                } while (true);

                // Step 5: Geometry Creation (commented out until implemented)
                // IGeometryCreator centerPoleCreator = new CenterPoleModule();
                // IGeometryCreator treadCreator = new TreadModule(new TreadGeometry());
                // IGeometryCreator midLandingCreator = new MidLandingModule(new MidLandingGeometry());
                // IGeometryCreator topLandingCreator = new TopLandingModule(new TopLandingGeometry());
                // IGeometryCreator handrailCreator = new HandrailModule(new HandrailGeometry());
                //
                // var entities = new EntityCollection();
                // entities.AddRange(centerPoleCreator.Create(context.Document, parameters));
                // entities.AddRange(treadCreator.Create(context.Document, parameters));
                // if (parameters.MidlandingIndex >= 0)
                //     entities.AddRange(midLandingCreator.Create(context.Document, parameters));
                // entities.AddRange(topLandingCreator.Create(context.Document, parameters));
                // entities.AddRange(handrailCreator.Create(context.Document, parameters));

                // Step 6: Post-Creation Tweaks (commented out until implemented)
                // IPostCreationTweaksModule tweaksModule = new PostCreationTweaksModule();
                // var finalEntities = tweaksModule.ApplyTweaks(context.Document, entities);

                // Step 7: Output (commented out until implemented)
                // IOutputModule outputModule = new OutputModule();
                // outputModule.Finalize(context.Document, input, parameters, finalEntities);

                // Placeholder: Show calculated parameters
                Application.ShowAlertDialog($"Calculated Parameters:\nTreads: {parameters.NumTreads}\nRiser Height: {parameters.RiserHeight:F2}\n" +
                                            $"Tread Angle: {parameters.TreadAngle:F2}\nWalkline Radius: {parameters.WalklineRadius:F2}\n" +
                                            $"Clear Width: {parameters.ClearWidth:F2}\nMidlanding Index: {parameters.MidlandingIndex}");
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog($"Error: {ex.Message}");
            }
        }
    }

    // Interfaces and DTOs unchanged from previous version
    public interface IInitializationModule
    {
        AutoCADContext Initialize();
        CenterPoleOptions GetCenterPoleOptions();
    }

    public interface IInputModule
    {
        StairInput GetInput(Document doc);
        void ShowRetryPrompt(string errorMessage);
        StairInput GetAdjustedInput(Document doc, StairParameters parameters);
    }

    public interface IValidationModule
    {
        ValidatedStairInput Validate(StairInput input);
    }

    public interface ICalculationModule
    {
        StairParameters Calculate(ValidatedStairInput input);
        ComplianceRetryOption HandleComplianceFailure(StairParameters parameters);
    }

    public interface IGeometryCreator
    {
        Entity[] Create(Document doc, StairParameters parameters);
    }

    public interface IPostCreationTweaksModule
    {
        EntityCollection ApplyTweaks(Document doc, EntityCollection entities);
    }

    public interface IOutputModule
    {
        void Finalize(Document doc, ValidatedStairInput input, StairParameters parameters, EntityCollection entities);
    }

    public class AutoCADContext
    {
        public Document Document { get; set; }
    }

    public class CenterPoleOptions
    {
        public double[] Diameters { get; set; }
        public string[] Labels { get; set; }
    }

    public class StairInput
    {
        public double CenterPoleDia { get; set; }
        public double OverallHeight { get; set; }
        public double OutsideDia { get; set; }
        public double RotationDeg { get; set; }
        public bool IsClockwise { get; set; }
        public bool Submitted { get; set; }
    }

    public class ValidatedStairInput : StairInput
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class StairParameters
    {
        public int NumTreads { get; set; }
        public double RiserHeight { get; set; }
        public double TreadAngle { get; set; }
        public double WalklineRadius { get; set; }
        public double ClearWidth { get; set; }
        public int MidlandingIndex { get; set; }
        public double CenterPoleDia { get; set; }
        public double OverallHeight { get; set; }
        public double OutsideDia { get; set; }
        public double RotationDeg { get; set; }
        public bool IsClockwise { get; set; }
        public bool IsCompliant { get; set; }
        public string ComplianceMessage { get; set; }
    }

    public enum ComplianceRetryOption
    {
        Retry,
        Ignore,
        Abort
    }

    public class EntityCollection
    {
        public List<Entity> Entities { get; } = new List<Entity>();
        public void Add(Entity entity) => Entities.Add(entity);
        public void AddRange(Entity[] entities) => Entities.AddRange(entities);
    }
}