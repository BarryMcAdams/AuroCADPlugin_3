using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class CalculationModule : ICalculationModule
    {
        public StairParameters Calculate(ValidatedStairInput input)
        {
            var parameters = new StairParameters
            {
                CenterPoleDia = input.CenterPoleDia,
                OverallHeight = input.OverallHeight,
                OutsideDia = input.OutsideDia,
                RotationDeg = input.RotationDeg,
                IsClockwise = input.IsClockwise,
                NumTreads = (int)System.Math.Ceiling(input.OverallHeight / 9.5),
                MidlandingIndex = input.OverallHeight > 151 ? 8 : -1 // Default index 8 if height > 151"
            };

            parameters.RiserHeight = parameters.OverallHeight / parameters.NumTreads;
            parameters.TreadAngle = parameters.MidlandingIndex >= 0 ? (parameters.RotationDeg - 90) / (parameters.NumTreads - 2) : parameters.RotationDeg / (parameters.NumTreads - 1);
            parameters.WalklineRadius = (parameters.CenterPoleDia / 2) + 12;
            parameters.ClearWidth = (parameters.OutsideDia / 2) - (parameters.CenterPoleDia / 2) - 1.5;

            double walklineWidth = parameters.WalklineRadius * (parameters.TreadAngle * System.Math.PI / 180);
            parameters.IsCompliant = walklineWidth >= 6.75 && parameters.ClearWidth >= 26;
            parameters.ComplianceMessage = "";
            if (walklineWidth < 6.75)
                parameters.ComplianceMessage += $"R311.7.10.1: Walkline width {walklineWidth:F2} in < 6.75 in\n";
            if (parameters.ClearWidth < 26)
                parameters.ComplianceMessage += $"Clear width {parameters.ClearWidth:F2} in < 26 in";

            return parameters;
        }

        public ComplianceRetryOption HandleComplianceFailure(StairParameters parameters)
        {
            string message = $"Code Violation(s):\n{parameters.ComplianceMessage}\n\nAbort: Cancel script\nRetry: Re-enter inputs\nIgnore: Build anyway";
            DialogResult result = MessageBox.Show(message, "Compliance Warning", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning);
            switch (result)
            {
                case DialogResult.Abort: return ComplianceRetryOption.Abort;
                case DialogResult.Retry: return ComplianceRetryOption.Retry;
                case DialogResult.Ignore: return ComplianceRetryOption.Ignore;
                default: return ComplianceRetryOption.Abort; // Fallback
            }
        }
    }
}