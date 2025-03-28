using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public class PostCreationTweaksModule : IPostCreationTweaksModule
    {
        public EntityCollection ApplyTweaks(Document doc, EntityCollection entities)
        {
            if (entities.Entities.Count < 2) return entities; // Need at least 2 entities to union

            var result = new EntityCollection();
            var baseEntity = entities.Entities[0] as Solid3d; // Cast to Solid3d
            if (baseEntity == null) return entities; // Return unchanged if cast fails

            for (int i = 1; i < entities.Entities.Count; i++)
            {
                var nextEntity = entities.Entities[i] as Solid3d;
                if (nextEntity == null) continue; // Skip if not a Solid3d

                var clone = (Solid3d)baseEntity.Clone();
                clone.BooleanOperation(BooleanOperationType.BoolUnite, nextEntity);
                baseEntity = clone;
            }
            result.Add(baseEntity);

            return result;
        }
    }
}