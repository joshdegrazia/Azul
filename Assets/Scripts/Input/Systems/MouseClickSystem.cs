using Input.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Input.Systems {
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(BuildPhysicsWorld)),
     UpdateBefore(typeof(MouseClickEndFrameSystem)),
     UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class MouseClickSystem : JobComponentSystem {
        private BuildPhysicsWorld BuildPhysicsWorld;
        private EntityQuery MouseClickListenerQuery;

        private Entity CurrentEntity;
        
        protected override void OnCreate() {
            this.BuildPhysicsWorld = base.World.GetOrCreateSystem<BuildPhysicsWorld>();

            this.MouseClickListenerQuery = base.GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] { typeof(ListenForMouseClick) }
            });

            this.CurrentEntity = Entity.Null;
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

            CollisionWorld collisionWorld = this.BuildPhysicsWorld.PhysicsWorld.CollisionWorld;

            RaycastHit raycastHit;
            if (collisionWorld.CastRay(raycastInput, out raycastHit)) {
                RigidBody rigidbody = collisionWorld.Bodies[raycastHit.RigidBodyIndex];
                
                EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);

                // this is feelin hella not worth
                // todo: replace with entity query declared above
                base.Entities.WithAll<ListenForMouseClick>()
                             .WithoutBurst()
                             .ForEach((ref Entity entity) => {
                                 if (entity.Equals(rigidbody.Entity)) {
                                     buffer.AddComponent(rigidbody.Entity, typeof(MouseClick));
                                 }
                                 
                             }).Run();

                buffer.Playback(base.EntityManager);
                buffer.Dispose();
            }

            return inputDeps;
        }
    }
}
