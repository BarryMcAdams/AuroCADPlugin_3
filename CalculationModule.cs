using Autodesk.AutoCAD.ApplicationServices;
using System;

namespace SpiralStairPlugin
{
    public class CalculationModule : ICalculationModule
    {
        public StairParameters Calculate(ValidatedStairInput input)
        {
            var parameters = new StairParameters
            {
                NumTreads = (int)Math.Ceiling(input.OverallHeight / 9.5), // Max riser height 9.5"
                WalklineRadius = input.CenterPoleDia / 2 + 12,
                ClearWidth = (input.OutsideDia / 2) - (input.CenterPoleDia / 2) - 1.5,
                MidlandingIndex = -1, // Default: no midlanding
                IsCompliant = true,
                ComplianceMessage = string.Empty,
                CenterPoleDia = input.CenterPoleDia,
                OverallHeight = input.OverallHeight,
                OutsideDia = input.OutsideDia,
                RotationDeg = input.RotationDeg,
                IsClockwise = input.IsClockwise
            };

            parameters.RiserHeight = input.OverallHeight / parameters.NumTreads;
            parameters.TreadAngle = input.RotationDeg / (parameters.NumTreads - 1);

            if (!CheckWalklineRadius(parameters))
            {
                parameters.IsCompliant = false;
                return parameters;
            }

            if (!CheckClearWidth(parameters))
            {
                parameters.IsCompliant = false;
                return parameters;
            }

            if (!CheckWalklineWidth(parameters))
            {
                parameters.IsCompliant = false;
                return parameters;
            }

            // Building Code: Midlanding required if height > 151"
            if (parameters.OverallHeight > 151)
            {
                parameters.MidlandingIndex = parameters.NumTreads / 2; // Midpoint default
                parameters.ComplianceMessage = $"Overall height exceeds 151 inches. Midlanding required at tread {parameters.MidlandingIndex + 1}.";
            }

            if (parameters.IsCompliant && parameters.MidlandingIndex >= 0)
            {
                AdjustTreadAngleForMidlanding(parameters, input.IsClockwise);
            }

            return parameters;
        }

        public ComplianceRetryOption HandleComplianceFailure(StairParameters parameters)
        {
            Application.ShowAlertDialog($"{parameters.ComplianceMessage}\nProceeding will ignore this violation; retry to adjust inputs.");
            return ComplianceRetryOption.Abort; // Simplified for now
        }

        private bool CheckWalklineRadius(StairParameters parameters)
        {
            if (parameters.WalklineRadius > 24.5)
            {
                double suggestedDia = (24.5 - 12) * 2;
                parameters.ComplianceMessage = $"Walkline Radius Violation: Current walkline radius is {parameters.WalklineRadius:F2} inches, exceeding 24.5 inches.\n" +
                                               $"Suggested center pole diameter: {suggestedDia:F2} inches.";
                return false;
            }
            return true;
        }

        private bool CheckClearWidth(StairParameters parameters)
        {
            if (parameters.ClearWidth < 26)
            {
                double suggestedOutsideDia = (26 + parameters.CenterPoleDia / 2 + 1.5) * 2;
                parameters.ComplianceMessage = $"Clear Width Violation: Current clear width is {parameters.ClearWidth:F2} inches, less than 26 inches.\n" +
                                               $"Suggested outside diameter: {suggestedOutsideDia:F2} inches.";
                return false;
            }
            return true;
        }

        private bool CheckWalklineWidth(StairParameters parameters)
        {
            double walklineWidth = parameters.WalklineRadius * (Math.Abs(parameters.TreadAngle) * Math.PI / 180);
            if (walklineWidth < 6.75)
            {
                double minRotationDeg = 90 + (6.75 / parameters.WalklineRadius) * (180 / Math.PI) * (parameters.NumTreads - 1);
                parameters.ComplianceMessage = $"Walkline Width Violation: Current walkline width is {walklineWidth:F2} inches, less than 6.75 inches.\n" +
                                               $"Suggested minimum rotation: {minRotationDeg:F2} degrees.";
                return false;
            }
            return true;
        }

        private void AdjustTreadAngleForMidlanding(StairParameters parameters, bool isClockwise)
        {
            int direction = isClockwise ? 1 : -1;
            if (parameters.MidlandingIndex >= 0)
            {
                parameters.TreadAngle = direction * Math.Abs((parameters.RotationDeg - 90) / (parameters.NumTreads - 2));
            }
            else
            {
                parameters.TreadAngle = direction * Math.Abs(parameters.RotationDeg / (parameters.NumTreads - 1));
            }
        }
    }
}