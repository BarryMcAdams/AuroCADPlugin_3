using Autodesk.AutoCAD.ApplicationServices;
using System;
using WinForms = System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class InputModule : IInputModule
    {
        public StairInput GetInput(Document doc)
        {
            using (var form = new SpiralStairForm())
            {
                var result = Application.ShowModalDialog(null, form, true);
                if (result == WinForms.DialogResult.OK)
                {
                    try
                    {
                        var input = new StairInput
                        {
                            CenterPoleDia = double.Parse(form.CenterPoleDiaText),
                            OverallHeight = double.Parse(form.OverallHeightText),
                            OutsideDia = double.Parse(form.OutsideDiaText),
                            RotationDeg = double.Parse(form.RotationDegText),
                            IsClockwise = form.IsClockwise,
                            Submitted = true
                        };
                        return input;
                    }
                    catch (FormatException)
                    {
                        Application.ShowAlertDialog("Please enter valid numeric values.");
                        return new StairInput { Submitted = false };
                    }
                }
                Application.ShowAlertDialog("Script aborted by user.");
                return new StairInput { Submitted = false };
            }
        }

        public void ShowRetryPrompt(string errorMessage)
        {
            Application.ShowAlertDialog(errorMessage);
        }

        public StairInput GetAdjustedInput(Document doc, StairParameters parameters)
        {
            using (var form = new SpiralStairForm())
            {
                var result = Application.ShowModalDialog(null, form, true);
                if (result == WinForms.DialogResult.OK)
                {
                    try
                    {
                        var input = new StairInput
                        {
                            CenterPoleDia = double.Parse(form.CenterPoleDiaText),
                            OverallHeight = double.Parse(form.OverallHeightText),
                            OutsideDia = double.Parse(form.OutsideDiaText),
                            RotationDeg = double.Parse(form.RotationDegText),
                            IsClockwise = form.IsClockwise,
                            Submitted = true
                        };
                        return input;
                    }
                    catch (FormatException)
                    {
                        Application.ShowAlertDialog("Please enter valid numeric values.");
                        return new StairInput { Submitted = false };
                    }
                }
                Application.ShowAlertDialog("Script aborted by user.");
                return new StairInput { Submitted = false };
            }
        }

        private class SpiralStairForm : WinForms.Form
        {
            private WinForms.TextBox txtCenterPoleDia, txtOverallHeight, txtOutsideDia, txtRotationDeg;
            private WinForms.RadioButton optClockwise, optCounterClockwise;
            private WinForms.Button btnOk, btnCancel;

            public string CenterPoleDiaText => txtCenterPoleDia.Text;
            public string OverallHeightText => txtOverallHeight.Text;
            public string OutsideDiaText => txtOutsideDia.Text;
            public string RotationDegText => txtRotationDeg.Text;
            public bool IsClockwise => optClockwise.Checked;

            public SpiralStairForm()
            {
                InitializeComponents();
            }

            private void InitializeComponents()
            {
                this.Text = "Spiral Stair Parameters";
                this.Width = 300;
                this.Height = 250;
                this.FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;

                int yPos = 10;
                AddLabelAndTextBox("Center Pole Dia (in):", ref txtCenterPoleDia, yPos);
                yPos += 30;
                AddLabelAndTextBox("Overall Height (in):", ref txtOverallHeight, yPos);
                yPos += 30;
                AddLabelAndTextBox("Outside Dia (in):", ref txtOutsideDia, yPos);
                yPos += 30;
                AddLabelAndTextBox("Rotation Deg:", ref txtRotationDeg, yPos);

                yPos += 30;
                var lblRotation = new WinForms.Label { Text = "Rotation Direction:", Left = 10, Top = yPos, Width = 100 };
                optClockwise = new WinForms.RadioButton { Text = "Clockwise", Left = 120, Top = yPos, Width = 80, Checked = true };
                optCounterClockwise = new WinForms.RadioButton { Text = "Counter-Clockwise", Left = 200, Top = yPos, Width = 100 };
                this.Controls.AddRange(new WinForms.Control[] { lblRotation, optClockwise, optCounterClockwise });

                yPos += 40;
                btnOk = new WinForms.Button { Text = "OK", Left = 50, Top = yPos, Width = 80, DialogResult = WinForms.DialogResult.OK };
                btnCancel = new WinForms.Button { Text = "Cancel", Left = 150, Top = yPos, Width = 80, DialogResult = WinForms.DialogResult.Cancel };
                this.Controls.AddRange(new WinForms.Control[] { btnOk, btnCancel });

                this.AcceptButton = btnOk;
                this.CancelButton = btnCancel;
            }

            private void AddLabelAndTextBox(string labelText, ref WinForms.TextBox textBox, int yPos)
            {
                var label = new WinForms.Label { Text = labelText, Left = 10, Top = yPos, Width = 100 };
                textBox = new WinForms.TextBox { Left = 120, Top = yPos, Width = 150 };
                this.Controls.AddRange(new WinForms.Control[] { label, textBox });
            }
        }
    }
}