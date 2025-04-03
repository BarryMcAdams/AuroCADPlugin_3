using Autodesk.AutoCAD.DatabaseServices;
using SpiralStairPlugin;

namespace AuroCADPlugin_3 // Ensure this namespace matches your project
{
    /// <summary>
    /// Interface for a module that contributes to the stair generation process.
    /// </summary>
    public interface IStairModule
    {
        /// <summary>
        /// Gets the name of the module (for logging or identification).
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Executes the module's logic to create or modify geometry.
        /// </summary>
        /// <param name="tr">The active AutoCAD Transaction.</param>
        /// <param name="db">The active AutoCAD Database.</param>
        /// <param name="parameters">The parameters defining the stair.</param>
        void Execute(Transaction tr, Database db, StairParameters parameters);
    }
}