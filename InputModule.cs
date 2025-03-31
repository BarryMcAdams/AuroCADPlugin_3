using Autodesk.AutoCAD.ApplicationServices;
using System.Windows.Forms;
using System.Linq;

namespace SpiralStairPlugin
{
    public class InputModule : IInputModule
    {
        private readonly double[] availablePipeSizes = { 3.0, 3.5, 4.0, 4.5, 5.0, 5.563, 6.0, 6.625, 8.0, 8.625, 10.75, 12.75 };

        public StairInput GetInput(Document doc)
        {
            using (var form = new StairInputForm())
            {
                form.ShowDialog();
                StairInput input = ValidateInput(form);
                if (!input.Submitted) return input; // Cancelled
                return input;
            }
        }

        public void ShowRetryPrompt(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public StairInput GetAdjustedInput(Document doc, StairParameters parameters)
        {
            using (var form = new StairInputForm(parameters.SuggestedPoleDia, parameters.SuggestedHeight, parameters.SuggestedOutsideDia, parameters.SuggestedRotation))
            {
                form.ShowDialog();
                StairInput input = ValidateInput(form);
                if (!input.Submitted) return input; // Cancelled
                return input;
            }
        }

        private StairInput ValidateInput(StairInputForm form)
        {
            StairInput input = new StairInput
            {
                CenterPoleDia = form.CenterPoleDia,
                OverallHeight = form.OverallHeight,
                OutsideDia = form.OutsideDia,
                RotationDeg = form.RotationDeg,
                IsClockwise = form.IsClockwise,
                Submitted = form.Submitted
            };

            if (input.Submitted && !availablePipeSizes.Contains(input.CenterPoleDia))
            {
                string pipeList = string.Join(", ", availablePipeSizes.Select(size => $"{size}\""));
                string message = $"Center pole diameter {input.CenterPoleDia:F2}\" is not a standard size.\n" +
                                 $"Available sizes: {pipeList}\n\n" +
                                 "Use Anyway: Proceed with custom size\nRetry: Re-enter a standard size";
                DialogResult result = MessageBox.Show(message, "Non-Standard Size", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel)
                {
                    input.Submitted = false; // Retry
                }
            }

            return input;
        }
    }
}