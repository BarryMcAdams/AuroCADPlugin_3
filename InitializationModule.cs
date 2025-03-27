using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public class InitializationModule : IInitializationModule
    {
        private readonly double[] availableDiameters = { 3, 3.5, 4, 4.5, 5, 5.56, 6, 6.625, 8, 8.625, 10.75, 12.75 };
        private readonly string[] diameterLabels = { "3 (tube)", "3.5 (tube)", "4 (tube)", "4.5 (tube)", "5 (tube)",
            "5.56 (tube)", "6 (tube)", "6.625 (tube)", "8 (tube)", "8.625 (tube)", "10.75 (tube)", "12.75 (12in. pipe)" };

        public AutoCADContext Initialize()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                Application.ShowAlertDialog("No active document found. Please open a drawing.");
                return null;
            }

            doc.Database.Insunits = UnitsValue.Inches;
            doc.Database.Lunits = 2;
            Application.ShowAlertDialog("Drawing units set to decimal inches for this script.");

            return new AutoCADContext { Document = doc };
        }

        public CenterPoleOptions GetCenterPoleOptions()
        {
            return new CenterPoleOptions
            {
                Diameters = availableDiameters,
                Labels = diameterLabels
            };
        }
    }
}