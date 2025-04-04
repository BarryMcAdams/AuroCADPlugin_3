﻿using System.Windows.Forms;
using System;
using System.Linq;

namespace SpiralStairPlugin
{
    public class CalculationModule : ICalculationModule
    {
        private readonly string codeReference = "R311.7.10.1 - Spiral Stairways: Spiral stairways are permitted, provided that the clear width at and below the handrail is not less than 26\" and the walkline radius is not greater than 24.5\". Each tread shall have a depth of not less than 6.75\" at the walkline. All treads shall be identical, and the rise shall be not more than 9.5\". Headroom shall be not less than 78\".";
        private readonly (double size, string type)[] availablePipeSizes = {
            (3.0, "tube"), (3.5, "3\" pipe"), (4.0, "tube"), (4.5, "4\" pipe"), (5.0, "tube"),
            (5.563, "5\" pipe"), (6.0, "tube"), (6.625, "6\" pipe"), (8.0, "tube"),
            (8.625, "8\" pipe"), (10.75, "10\" pipe"), (12.75, "12\" pipe")
        };

        public StairParameters Calculate(ValidatedStairInput input)
        {
            var parameters = new StairParameters
            {
                CenterPoleDia = input.CenterPoleDia,
                TotalRise = input.OverallHeight, // Maps to OverallHeight
                OuterRadius = input.OutsideDia / 2.0, // Maps to OutsideDia
                TotalRotation = input.RotationDeg, // Maps to RotationDeg
                IsClockwise = input.IsClockwise,
                NumberOfTreads = (int)Math.Ceiling(input.OverallHeight / 9.5), // Maps to NumTreads
                MidlandingIndex = input.OverallHeight > 151 ? 8 : -1
            };

            // RiserHeight is read-only (maps to CalculatedRiseHeightPerTread), so calculate it directly
            double riserHeight = parameters.TotalRise / parameters.NumberOfTreads;
            parameters.TreadRotation = parameters.MidlandingIndex >= 0 ? (parameters.TotalRotation - 90) / (parameters.NumberOfTreads - 2) : parameters.TotalRotation / (parameters.NumberOfTreads - 1);
            parameters.WalklineRadius = (parameters.CenterPoleDia / 2) + 12;
            parameters.ClearWidth = (parameters.OuterRadius) - 1.5 - (parameters.CenterPoleDia / 2); // OuterRadius - 1.5" - CenterPoleRadius

            double walklineWidth = parameters.WalklineRadius * (parameters.TreadRotation * Math.PI / 180);
            parameters.IsCompliant = walklineWidth >= 6.75 && parameters.ClearWidth >= 26 && parameters.TotalRise <= 78 * parameters.NumberOfTreads;
            parameters.ComplianceMessage = "";

            double? suggestedPoleDia = null;
            double? suggestedOutsideDia = null;
            double? suggestedRotation = null;
            double? suggestedHeight = null;

            if (walklineWidth < 6.75)
            {
                double requiredRadius = 6.75 / (parameters.TreadRotation * Math.PI / 180);
                double requiredPoleDia = (requiredRadius - 12) * 2;
                var currentPipeIndex = Array.FindIndex(availablePipeSizes, p => Math.Abs(p.size - input.CenterPoleDia) < 0.01);
                var nextPipe = availablePipeSizes.Skip(currentPipeIndex + 1).FirstOrDefault(p => p.size > requiredPoleDia);
                double nextPipeDia = nextPipe.size > 0 ? nextPipe.size : availablePipeSizes.Last().size;
                string nextPipeType = nextPipe.size > 0 ? nextPipe.type : availablePipeSizes.Last().type;
                double requiredRotation = parameters.MidlandingIndex >= 0 ? (6.75 / parameters.WalklineRadius * 180 / Math.PI) * (parameters.NumberOfTreads - 2) + 90 : (6.75 / parameters.WalklineRadius * 180 / Math.PI) * (parameters.NumberOfTreads - 1);
                parameters.ComplianceMessage += $"R311.7.10.1: Walkline width {walklineWidth:F2} in < 6.75 in\n\n" +
                                                $"RECOMMENDATION: Increase center pole diameter to {nextPipeDia:F2} in ({nextPipeType})\n" +
                                                $"RECOMMENDATION: Or increase total rotation to {requiredRotation:F2}°\n";
                suggestedPoleDia = nextPipeDia;
                suggestedRotation = requiredRotation;
            }
            if (parameters.ClearWidth < 26)
            {
                double requiredOutsideDia = (26 + 1.5 + (parameters.CenterPoleDia / 2)) * 2;
                parameters.ComplianceMessage += $"R311.7.10.1: Clear width {parameters.ClearWidth:F2} in < 26 in\n\n" +
                                                $"RECOMMENDATION: Increase outside diameter to {requiredOutsideDia:F2} in\n";
                suggestedOutsideDia = requiredOutsideDia;
            }
            if (parameters.TotalRise > 78 * parameters.NumberOfTreads)
            {
                double requiredHeight = 78 * parameters.NumberOfTreads;
                parameters.ComplianceMessage += $"R311.7.10.1: Headroom {(parameters.TotalRise / parameters.NumberOfTreads):F2} in < 78 in\n\n" +
                                                $"RECOMMENDATION: Reduce overall height to {requiredHeight:F2} in or adjust treads\n";
                suggestedHeight = requiredHeight;
            }

            parameters.SuggestedPoleDia = suggestedPoleDia;
            parameters.SuggestedOutsideDia = suggestedOutsideDia;
            parameters.SuggestedRotation = suggestedRotation;
            parameters.SuggestedHeight = suggestedHeight;

            // Update calculated parameters after setting all values
            parameters.UpdateCalculatedParameters();

            return parameters;
        }

        public ComplianceRetryOption HandleComplianceFailure(StairParameters parameters)
        {
            string message = $"Code Violation(s):\n{parameters.ComplianceMessage}\n\n{codeReference}\n\nAbort: Cancel script\nRetry: Re-enter inputs\nIgnore: Build anyway";
            DialogResult result = MessageBox.Show(message, "Compliance Warning", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning);
            switch (result)
            {
                case DialogResult.Abort: return ComplianceRetryOption.Abort;
                case DialogResult.Retry: return ComplianceRetryOption.Retry;
                case DialogResult.Ignore: return ComplianceRetryOption.Ignore;
                default: return ComplianceRetryOption.Abort;
            }
        }
    }
}