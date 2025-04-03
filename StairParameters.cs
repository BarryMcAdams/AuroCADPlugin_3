using System; // Required for Math functions

namespace SpiralStairPlugin // Updated to match SpiralStairCommand.cs
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
        public int NumberOfTreads { get; set; } = 15; // Maps to NumTreads

        // Rotation
        public double TreadRotation { get; set; } // Rotation per tread in degrees (Calculated), maps to TreadAngle, made writable
        public double TotalRotation { get; set; } = 300.0; // Total rotation in degrees (Primary input), maps to RotationDeg
        public bool IsClockwise { get; set; } = true; // Indicates rotation direction (true for clockwise)

        // Landing Parameters
        public double FloorHeight { get; private set; } // Z-level of the floor/landing top surface (Calculated from TotalRise), maps to OverallHeight
        public double LandingWidth { get; set; } = 60.0; // Angular width of the landing in degrees

        // Calculated Values (Read-only properties)
        public double CalculatedRiseHeightPerTread => NumberOfTreads > 0 ? TotalRise / NumberOfTreads : 0; // Maps to RiserHeight

        // Additional Properties from SpiralStairCommand.cs
        public double WalklineRadius { get; set; }
        public double ClearWidth { get; set; }
        public int MidlandingIndex { get; set; }
        public double CenterPoleDia { get; set; }
        public bool IsCompliant { get; set; }
        public string ComplianceMessage { get; set; }
        public double? SuggestedPoleDia { get; set; }
        public double? SuggestedOutsideDia { get; set; }
        public double? SuggestedRotation { get; set; }
        public double? SuggestedHeight { get; set; }

        // Properties to map to existing ones
        public double NumTreads { get => NumberOfTreads; set => NumberOfTreads = (int)value; }
        public double RiserHeight { get => CalculatedRiseHeightPerTread; }
        public double TreadAngle { get => TreadRotation; set => TreadRotation = value; }
        public double OverallHeight { get => FloorHeight; set => TotalRise = value; }
        public double OutsideDia { get => OuterRadius * 2.0; set => OuterRadius = value / 2.0; }
        public double RotationDeg { get => TotalRotation; set => TotalRotation = value; }

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