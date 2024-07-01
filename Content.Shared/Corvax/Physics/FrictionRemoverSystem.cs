using Content.Shared.Corvax.VoidContainerComponent;
using Content.Shared.EntityList;
using Content.Shared.Tiles;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.SimpleStation14.Physics;

public sealed class FrictionRemoverSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhysicsComponent, PhysicsSleepEvent>(RemoveDampening);
    }


    private void RemoveDampening(EntityUid uid, PhysicsComponent component, PhysicsSleepEvent args)
    {
        if (EntityManager.TryGetComponent(uid, out VoidContainerComponent? voidContainerComp)||
            EntityManager.TryGetComponent(uid, out ProtectedGridComponent? protectedGridComp))
            return;

        _physics.SetAngularDamping(component, 0, false);
        _physics.SetLinearDamping(component, 0);
    }
}
