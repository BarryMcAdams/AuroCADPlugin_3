using System;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public partial class StairInputForm : Form
    {
        public double CenterPoleDia => double.TryParse(txtCenterPoleDia.Text, out double val) ? val : 5;
        public double OverallHeight => double.TryParse(txtOverallHeight.Text, out double val) ? val : 152;
        public double OutsideDia => double.TryParse(txtOutsideDia.Text, out double val) ? val : 60;
        public double RotationDeg => double.TryParse(txtRotationDeg.Text, out double val) ? val : 450;
        public bool IsClockwise => chkClockwise.Checked;
        public bool Submitted { get; private set; }

        public StairInputForm()
        {
            InitializeComponent();
            txtCenterPoleDia.Text = "5";
            txtOverallHeight.Text = "152";
            txtOutsideDia.Text = "60";
            txtRotationDeg.Text = "450";
            chkClockwise.Checked = true;
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