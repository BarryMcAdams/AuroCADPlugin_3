using System;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public partial class StairInputForm : Form
    {
        public double CenterPoleDia => double.Parse(txtCenterPoleDia.Text);
        public double OverallHeight => double.Parse(txtOverallHeight.Text);
        public double OutsideDia => double.Parse(txtOutsideDia.Text);
        public double RotationDeg => double.Parse(txtRotationDeg.Text);
        public bool IsClockwise => chkClockwise.Checked;
        public bool Submitted { get; private set; }

        public StairInputForm()
        {
            InitializeComponent();
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