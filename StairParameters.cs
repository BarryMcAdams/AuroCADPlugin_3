using System; // Required for Math functions

namespace AuroCADPlugin_3 // Ensure this namespace matches your project
{
    /// <summary>
    /// Holds all the parameters required to define and generate the spiral staircase.
    /// </summary>
    public class StairParameters
    {
        // Basic Dimensions
        public double InnerRadius { get; set; } = 10.0;
        public double OuterRadius { get; set; } = 50.0;
        public double TreadThickness { get; set; } = 2.0;

        // Height and Tread Count
        public double TotalRise { get; set; } = 120.0; // Defines the Z level of the top landing surface (FloorHeight)
        public int NumberOfTreads { get; set; } = 15;

        // Rotation
        public double TreadRotation { get; private set; } // Rotation per tread in degrees (Calculated)
        public double TotalRotation { get; set; } = 300.0; // Total rotation in degrees (Primary input)
        public bool IsClockwise { get; set; } = true; // Added to indicate rotation direction (true for clockwise)

        // Landing Parameters
        public double FloorHeight { get; private set; } // Z-level of the floor/landing top surface (Calculated from TotalRise)
        public double LandingWidth { get; set; } = 60.0; // Angular width of the landing in degrees

        // Calculated Values (Read-only properties)
        public double CalculatedRiseHeightPerTread => NumberOfTreads > 0 ? TotalRise / NumberOfTreads : 0;

        // Constructor
        public StairParameters()
        {
            // Initialize calculated values based on defaults
            UpdateCalculatedParameters();
        }

        /// <summary>
        /// Recalculates dependent values. Call this after changing primary parameters
        /// like TotalRotation, NumberOfTreads, or TotalRise.
        /// This version prioritizes TotalRotation to calculate TreadRotation
        /// and sets FloorHeight directly from TotalRise.
        /// </summary>
        public void UpdateCalculatedParameters()
        {
            if (NumberOfTreads > 0)
            {
                // Calculate TreadRotation based on TotalRotation
                this.TreadRotation = this.TotalRotation / this.NumberOfTreads;
            }
            else
            {
                this.TreadRotation = 0;
            }

            // Set FloorHeight based on TotalRise. Adjust if your definition differs.
            this.FloorHeight = this.TotalRise;
        }
    }
}