using System;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class StairInputForm : Form
    {
        private TextBox txtCenterPoleDia;
        private TextBox txtOverallHeight;
        private TextBox txtOutsideDia;
        private TextBox txtRotationDeg;
        private CheckBox chkClockwise;
        private Button btnSubmit;

        public double CenterPoleDia => double.TryParse(txtCenterPoleDia.Text, out double val) ? val : 5;
        public double OverallHeight => double.TryParse(txtOverallHeight.Text, out double val) ? val : 152;
        public double OutsideDia => double.TryParse(txtOutsideDia.Text, out double val) ? val : 60;
        public double RotationDeg => double.TryParse(txtRotationDeg.Text, out double val) ? val : 450;
        public bool IsClockwise => chkClockwise.Checked;
        public bool Submitted { get; private set; }

        public StairInputForm(double? suggestedPoleDia = null, double? suggestedHeight = null, double? suggestedOutsideDia = null, double? suggestedRotation = null)
        {
            this.Text = "Spiral Stair Input";
            this.Size = new System.Drawing.Size(300, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Labels
            Label lblCenterPoleDia = new Label { Text = "Center Pole Diameter:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            Label lblOverallHeight = new Label { Text = "Overall Height:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            Label lblOutsideDia = new Label { Text = "Outside Diameter:", Location = new System.Drawing.Point(20, 100), AutoSize = true };
            Label lblRotationDeg = new Label { Text = "Rotation Degree:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            Label lblClockwise = new Label { Text = "Clockwise:", Location = new System.Drawing.Point(20, 180), AutoSize = true };

            // TextBoxes with center justification
            txtCenterPoleDia = new TextBox { Location = new System.Drawing.Point(150, 20), Size = new System.Drawing.Size(100, 20), Text = suggestedPoleDia?.ToString("F2") ?? "5", TextAlign = HorizontalAlignment.Center };
            txtOverallHeight = new TextBox { Location = new System.Drawing.Point(150, 60), Size = new System.Drawing.Size(100, 20), Text = suggestedHeight?.ToString("F2") ?? "152", TextAlign = HorizontalAlignment.Center };
            txtOutsideDia = new TextBox { Location = new System.Drawing.Point(150, 100), Size = new System.Drawing.Size(100, 20), Text = suggestedOutsideDia?.ToString("F2") ?? "60", TextAlign = HorizontalAlignment.Center };
            txtRotationDeg = new TextBox { Location = new System.Drawing.Point(150, 140), Size = new System.Drawing.Size(100, 20), Text = suggestedRotation?.ToString("F2") ?? "450", TextAlign = HorizontalAlignment.Center };

            // CheckBox
            chkClockwise = new CheckBox { Location = new System.Drawing.Point(150, 180), Size = new System.Drawing.Size(100, 20), Text = "Clockwise", Checked = true };

            // Button
            btnSubmit = new Button { Text = "Submit", Location = new System.Drawing.Point(100, 220), Size = new System.Drawing.Size(100, 30) };
            btnSubmit.Click += new EventHandler(btnSubmit_Click);
            this.AcceptButton = btnSubmit; // Enter key triggers submit

            // Add controls to form
            this.Controls.AddRange(new Control[] { lblCenterPoleDia, lblOverallHeight, lblOutsideDia, lblRotationDeg, lblClockwise,
                                                  txtCenterPoleDia, txtOverallHeight, txtOutsideDia, txtRotationDeg,
                                                  chkClockwise, btnSubmit });
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                double.Parse(txtCenterPoleDia.Text);
                double.Parse(txtOverallHeight.Text);
                double.Parse(txtOutsideDia.Text);
                double.Parse(txtRotationDeg.Text);
                Submitted = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid input: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}