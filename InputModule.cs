using Autodesk.AutoCAD.ApplicationServices;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class InputModule : IInputModule
    {
        public StairInput GetInput(Document doc)
        {
            using (var form = new StairInputForm())
            {
                form.ShowDialog();
                return new StairInput
                {
                    CenterPoleDia = form.CenterPoleDia,
                    OverallHeight = form.OverallHeight,
                    OutsideDia = form.OutsideDia,
                    RotationDeg = form.RotationDeg,
                    IsClockwise = form.IsClockwise,
                    Submitted = form.Submitted
                };
            }
        }

        public void ShowRetryPrompt(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public StairInput GetAdjustedInput(Document doc, StairParameters parameters)
        {
            using (var form = new StairInputForm())
            {
                form.ShowDialog();
                return new StairInput
                {
                    CenterPoleDia = form.CenterPoleDia,
                    OverallHeight = form.OverallHeight,
                    OutsideDia = form.OutsideDia,
                    RotationDeg = form.RotationDeg,
                    IsClockwise = form.IsClockwise,
                    Submitted = form.Submitted
                };
            }
        }
    }
}