using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Linq;

namespace SpiralStairPlugin
{
    public class ValidationModule : IValidationModule
    {
        private readonly CenterPoleOptions centerPoleOptions;

        public ValidationModule(CenterPoleOptions centerPoleOptions)
        {
            this.centerPoleOptions = centerPoleOptions ?? throw new ArgumentNullException(nameof(centerPoleOptions));
        }

        public ValidatedStairInput Validate(StairInput input)
        {
            var validatedInput = new ValidatedStairInput
            {
                CenterPoleDia = input.CenterPoleDia,
                OverallHeight = input.OverallHeight,
                OutsideDia = input.OutsideDia,
                RotationDeg = input.RotationDeg,
                IsClockwise = input.IsClockwise,
                Submitted = input.Submitted,
                IsValid = true,
                ErrorMessage = string.Empty
            };

            // Validate Center Pole Diameter
            const double tolerance = 0.001;
            if (!centerPoleOptions.Diameters.Any(d => Math.Abs(d - validatedInput.CenterPoleDia) < tolerance))
            {
                double closestDia = centerPoleOptions.Diameters
                    .OrderBy(d => Math.Abs(d - validatedInput.CenterPoleDia))
                    .First();
                validatedInput.CenterPoleDia = closestDia;
                string optionsList = string.Join(", ", centerPoleOptions.Labels);
                Application.ShowAlertDialog($"Center pole diameter adjusted to {closestDia} inches (closest available).\nAvailable options: {optionsList}");
            }

            // Validate Overall Height
            if (validatedInput.OverallHeight < 20 || validatedInput.OverallHeight > 300)
            {
                validatedInput.IsValid = false;
                validatedInput.ErrorMessage = "Overall height must be between 20 and 300 inches.";
                return validatedInput;
            }

            // Validate Outside Diameter (updated max to 144")
            double minOutsideDia = validatedInput.CenterPoleDia + 10;
            if (validatedInput.OutsideDia < minOutsideDia || validatedInput.OutsideDia > 144)
            {
                validatedInput.IsValid = false;
                validatedInput.ErrorMessage = $"Outside diameter must be between {minOutsideDia} and 144 inches.";
                return validatedInput;
            }

            // Validate Rotation Degrees (updated max to 900°)
            if (validatedInput.RotationDeg < 90 || validatedInput.RotationDeg > 1080)
            {
                validatedInput.IsValid = false;
                validatedInput.ErrorMessage = "Total rotation must be between 90 and 1080 degrees.";
                return validatedInput;
            }

            return validatedInput;
        }
    }
}