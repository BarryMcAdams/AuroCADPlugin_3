<<<<<<< HEAD
﻿using AuroCADPlugin_3;
using Autodesk.AutoCAD.ApplicationServices;
=======
﻿using Autodesk.AutoCAD.ApplicationServices;
>>>>>>> 4d29bcbe81dff028fcabfbae03d247807fc17421
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class InputModule : IInputModule
    {
        public StairInput GetInput(Document doc)
        {
<<<<<<< HEAD
            var form = new StairInputForm();
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
=======
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
>>>>>>> 4d29bcbe81dff028fcabfbae03d247807fc17421
        }

        public void ShowRetryPrompt(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public StairInput GetAdjustedInput(Document doc, StairParameters parameters)
        {
<<<<<<< HEAD
            var form = new StairInputForm();
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
=======
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
>>>>>>> 4d29bcbe81dff028fcabfbae03d247807fc17421
        }
    }
}