#if !HAVOK_PHYSICS_EXISTS

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using SM.Physics.Systems;

namespace SM.Physics.Authoring
{
    // A system which draws any collision events produced by the physics step system
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(StepPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
    public partial class DisplayCollisionEventsSystem : SystemBase
    {
        BuildPhysicsWorld m_BuildPhysicsWorldSystem;
        StepPhysicsWorld m_StepPhysicsWorldSystem;
        DebugStream m_DebugStreamSystem;

        protected override void OnCreate()
        {
            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
            m_DebugStreamSystem = World.GetOrCreateSystem<DebugStream>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            this.RegisterPhysicsRuntimeSystemReadOnly();
        }

        protected override void OnUpdate()
        {
            if (!(HasSingleton<PhysicsDebugDisplayData>() && GetSingleton<PhysicsDebugDisplayData>().DrawCollisionEvents != 0))
            {
                return;
            }

            unsafe
            {
                // Allocate a block of memory to store our debug output, so it can be shared across the display/finish jobs
                var sharedOutput = (DebugStream.Context*)UnsafeUtility.Malloc(sizeof(DebugStream.Context), 16, Allocator.TempJob);
                *sharedOutput = m_DebugStreamSystem.GetContext(1);
                sharedOutput->Begin(0);

                // This will call the extension method defined in SM.Physics
                Dependency = new DisplayCollisionEventsJob
                {
                    World = m_BuildPhysicsWorldSystem.PhysicsWorld,
                    OutputStreamContext = sharedOutput
                }.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);

#pragma warning disable 618

                Dependency = new FinishDisplayCollisionEventsJob
                {
                    OutputStreamContext = sharedOutput
                }.Schedule(Dependency);
#pragma warning restore 618
            }
        }

        // Job which iterates over collision events and writes display info to a DebugStream.
        [BurstCompile]
        private unsafe struct DisplayCollisionEventsJob : ICollisionEventsJobBase
        {
            [ReadOnly] public PhysicsWorld World;
            [NativeDisableUnsafePtrRestriction]
            public DebugStream.Context* OutputStreamContext;

            public unsafe void Execute(CollisionEvent collisionEvent)
            {
                CollisionEvent.Details details = collisionEvent.CalculateDetails(ref World);

                // Color code the impulse depending on the collision feature
                // vertex - blue
                // edge - cyan
                // face - magenta
                Unity.DebugDisplay.ColorIndex color;
                switch (details.EstimatedContactPointPositions.Length)
                {
                    case 1:
                        color = Unity.DebugDisplay.ColorIndex.Blue;
                        break;
                    case 2:
                        color = Unity.DebugDisplay.ColorIndex.Cyan;
                        break;
                    default:
                        color = Unity.DebugDisplay.ColorIndex.Magenta;
                        break;
                }

                var averageContactPosition = details.AverageContactPointPosition;
                OutputStreamContext->Point(averageContactPosition, 0.01f, color);
                OutputStreamContext->Arrow(averageContactPosition, collisionEvent.Normal * details.EstimatedImpulse, color);
            }
        }

        [BurstCompile]
        private unsafe struct FinishDisplayCollisionEventsJob : IJob
        {
            [NativeDisableUnsafePtrRestriction]
            internal DebugStream.Context* OutputStreamContext;

            public void Execute()
            {
                OutputStreamContext->End();
                UnsafeUtility.Free(OutputStreamContext, Allocator.TempJob);
            }
        }
    }
}

#endif
