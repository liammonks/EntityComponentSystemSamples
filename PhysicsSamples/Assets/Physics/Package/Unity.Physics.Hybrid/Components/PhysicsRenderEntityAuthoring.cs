using UnityEngine;
using Unity.Entities;
using SM.Physics.GraphicsIntegration;

namespace SM.Physics.Authoring
{
    [AddComponentMenu("DOTS/Physics/Physics Render Entity")]
    [HelpURL(HelpURLs.PhysicsRenderEntityAuthoring)]
    [DisallowMultipleComponent]
    public sealed class PhysicsRenderEntityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("Specifies an Entity in a different branch of the hierarchy that holds the graphical representation of this PhysicsShape.")]
        public GameObject RenderEntity;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var renderEntity = new PhysicsRenderEntity { Entity = conversionSystem.GetPrimaryEntity(RenderEntity) };
            dstManager.AddComponentData(entity, renderEntity);
        }
    }
}
