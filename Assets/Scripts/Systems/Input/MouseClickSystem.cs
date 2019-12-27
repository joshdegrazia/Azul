using Azul.Components.Input;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Azul.Systems.Input {

    [UpdateAfter(typeof(BuildPhysicsWorld)),
     UpdateBefore(typeof(MouseClickEndFrameSystem)),
     UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class MouseClickSystem : JobComponentSystem {
        private BuildPhysicsWorld BuildPhysicsWorld;
        private EntityQuery MouseClickListenerQuery;
        
        protected override void OnCreate() {
            this.BuildPhysicsWorld = base.World.GetOrCreateSystem<BuildPhysicsWorld>();

            this.MouseClickListenerQuery = base.GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] { typeof(ListenForMouseClick) }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (this.MouseClickListenerQuery.CalculateEntityCount() == 0 
                || !UnityEngine.Input.GetMouseButtonDown(0)) {
                return inputDeps;
            }

            UnityEngine.Ray unityRay = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            RaycastInput raycastInput = new RaycastInput {
                Start = unityRay.origin,
                End = unityRay.origin + unityRay.direction * 1000,
                Filter = CollisionFilter.Default
            };

            RaycastHit raycastHit;
            if (this.BuildPhysicsWorld.PhysicsWorld.CollisionWorld.CastRay(raycastInput, out raycastHit)) {
                UnityEngine.Debug.Log("Mouse hit");
            } else {
                UnityEngine.Debug.Log("Mouse did not hit");
            }

            return inputDeps;
        }
    }
}
