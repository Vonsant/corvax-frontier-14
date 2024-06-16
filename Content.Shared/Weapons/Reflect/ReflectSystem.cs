using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Administration.Logs;
using Content.Shared.Audio;
using Content.Shared.Database;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Weapons.Reflect;

/// <summary>
/// This handles reflecting projectiles and hitscan shots.
/// </summary>
public sealed class ReflectSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReflectComponent, ProjectileReflectAttemptEvent>(OnObjectReflectProjectileAttempt);
        SubscribeLocalEvent<ReflectComponent, HitScanReflectAttemptEvent>(OnObjectReflectHitscanAttempt);
        SubscribeLocalEvent<ReflectComponent, GotEquippedEvent>(OnReflectEquipped);
        SubscribeLocalEvent<ReflectComponent, GotUnequippedEvent>(OnReflectUnequipped);
        SubscribeLocalEvent<ReflectComponent, GotEquippedHandEvent>(OnReflectHandEquipped);
        SubscribeLocalEvent<ReflectComponent, GotUnequippedHandEvent>(OnReflectHandUnequipped);
        SubscribeLocalEvent<ReflectComponent, ItemToggledEvent>(OnToggleReflect);

        SubscribeLocalEvent<ReflectUserComponent, ProjectileReflectAttemptEvent>(OnUserProjectileReflectAttempt);
        SubscribeLocalEvent<ReflectUserComponent, HitScanReflectAttemptEvent>(OnUserHitscanReflectAttempt);
    }

    private void OnUserHitscanReflectAttempt(EntityUid user, ReflectUserComponent component, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected)
            return;

        if (!UserCanReflect(user, out var bestReflectorUid))
            return;

        if (!TryReflectHitscan(user, bestReflectorUid.Value, args.Shooter, args.SourceItem, args.Direction, out var dir))
            return;

        args.Direction = dir.Value;
        args.Reflected = true;
    }

    private void OnUserProjectileReflectAttempt(EntityUid user, ReflectUserComponent component, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<ReflectiveComponent>(args.ProjUid, out var reflectiveComponent))
            return;

        if (!UserCanReflect(user, out var bestReflectorUid, (args.ProjUid, reflectiveComponent)))
            return;

        if (!TryReflectProjectile(user, bestReflectorUid.Value, (args.ProjUid, args.Component)))
            return;

        args.Cancelled = true;
    }

    private void OnObjectReflectHitscanAttempt(EntityUid uid, ReflectComponent component, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected || (component.Reflects & args.Reflective) == 0)
            return;

        if (TryReflectHitscan(uid, uid, args.Shooter, args.SourceItem, args.Direction, out var dir))
        {
            args.Direction = dir.Value;
            args.Reflected = true;
        }
    }

    private void OnObjectReflectProjectileAttempt(EntityUid uid, ReflectComponent component, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (TryReflectProjectile(uid, uid, (args.ProjUid, args.Component)))
        {
            args.Cancelled = true;
        }
    }

    private bool UserCanReflect(EntityUid user, [NotNullWhen(true)] out EntityUid? bestReflectorUid, (EntityUid, ReflectiveComponent)? projectile = null)
    {
        bestReflectorUid = null;

        foreach (var entityUid in _inventorySystem.GetHandOrInventoryEntities(user, SlotFlags.WITHOUT_POCKET))
        {
            if (!TryComp<ReflectComponent>(entityUid, out var comp))
                continue;

            if (!comp.Enabled)
                continue;

            if (bestReflectorUid != null && TryComp<ReflectComponent>(bestReflectorUid.Value, out var bestComp) && bestComp.ReflectProb >= comp.ReflectProb)
                continue;

            if (projectile != null && (comp.Reflects & projectile.Value.Item2.Reflective) == 0)
                continue;

            bestReflectorUid = entityUid;
        }

        return bestReflectorUid != null;
    }

    private bool TryReflectProjectile(EntityUid user, EntityUid reflectorUid, (EntityUid, ProjectileComponent) projectile)
    {
        if (!TryComp<ReflectComponent>(reflectorUid, out var reflector) ||
            !reflector.Enabled ||
            !TryComp<ReflectiveComponent>(projectile.Item1, out var reflective) ||
            (reflector.Reflects & reflective.Reflective) == 0 ||
            !_random.Prob(reflector.ReflectProb) ||
            !TryComp<PhysicsComponent>(projectile.Item1, out var physics) ||
            (TryComp<StaminaComponent>(user, out var staminaComponent) && staminaComponent.Critical) ||
            _physics.IsDown(reflectorUid))
        {
            return false;
        }

        if (!_random.Prob(GetReflectChance(reflectorUid)))
            return false;

        var rotation = _random.NextAngle(-reflector.Spread / 2, reflector.Spread / 2).Opposite();
        var existingVelocity = _physics.GetMapLinearVelocity(projectile.Item1, component: physics);
        var relativeVelocity = existingVelocity - _physics.GetMapLinearVelocity(user);
        var newVelocity = rotation.RotateVec(relativeVelocity);

        var difference = newVelocity - existingVelocity;

        _physics.SetLinearVelocity(projectile.Item1, physics.LinearVelocity + difference, body: physics);

        var locRot = Transform(projectile.Item1).LocalRotation;
        var newRot = rotation.RotateVec(locRot.ToVec());
        _transform.SetLocalRotation(projectile.Item1, newRot.ToAngle());

        if (_netManager.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("reflect-shot"), user);
            _audio.PlayPvs(reflector.SoundOnReflect, user, AudioHelpers.WithVariation(0.05f, _random));
        }

        _adminLogger.Add(LogType.BulletHit, LogImpact.Medium, $"{ToPrettyString(user)} reflected {ToPrettyString(projectile.Item1)} from {ToPrettyString(projectile.Item2.Weapon)} shot by {projectile.Item2.Shooter}");

        projectile.Item2.Shooter = user;
        projectile.Item2.Weapon = user;
        Dirty(projectile.Item1);

        return true;
    }

    private bool TryReflectHitscan(EntityUid user, EntityUid reflectorUid, EntityUid? shooter, EntityUid shotSource, Vector2 direction, [NotNullWhen(true)] out Vector2? newDirection)
    {
        if (!TryComp<ReflectComponent>(reflectorUid, out var reflector) ||
            !reflector.Enabled ||
            (TryComp<StaminaComponent>(user, out var staminaComponent) && staminaComponent.Critical) ||
            _physics.IsDown(reflectorUid))
        {
            newDirection = null;
            return false;
        }

        if (!_random.Prob(GetReflectChance(reflectorUid)))
        {
            newDirection = null;
            return false;
        }

        if (_netManager.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("reflect-shot"), user);
            _audio.PlayPvs(reflector.SoundOnReflect, user, AudioHelpers.WithVariation(0.05f, _random));
        }

        var spread = _random.NextAngle(-reflector.Spread / 2, reflector.Spread / 2);
        newDirection = -spread.RotateVec(direction);

        if (shooter != null)
            _adminLogger.Add(LogType.HitScanHit, LogImpact.Medium, $"{ToPrettyString(user)} reflected hitscan from {ToPrettyString(shotSource)} shot by {ToPrettyString(shooter.Value)}");
        else
            _adminLogger.Add(LogType.HitScanHit, LogImpact.Medium, $"{ToPrettyString(user)} reflected hitscan from {ToPrettyString(shotSource)}");

        return true;
    }

    private float GetReflectChance(EntityUid reflectorUid)
    {
        if (!TryComp<ReflectComponent>(reflectorUid, out var reflector))
            return 0f;

        if (reflector.Innate)
            return reflector.ReflectProb;

        if (_physics.IsWeightless(reflectorUid))
            return reflector.MinReflectProb;

        if (!TryComp<PhysicsComponent>(reflectorUid, out var reflectorPhysics))
            return reflector.ReflectProb;

        return MathHelper.Lerp(
            reflector.MinReflectProb,
            reflector.ReflectProb,
            1 - Math.Clamp((reflectorPhysics.LinearVelocity.Length() - reflector.VelocityBeforeNotMaxProb) / (reflector.VelocityBeforeMinProb - reflector.VelocityBeforeNotMaxProb), 0, 1)
        );
    }

    private void OnReflectEquipped(EntityUid uid, ReflectComponent reflector, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        EnsureComp<ReflectUserComponent>(args.Equipee);

        if (reflector.Enabled)
            EnableAlert(args.Equipee);
    }

    private void OnReflectUnequipped(EntityUid uid, ReflectComponent reflector, GotUnequippedEvent args)
    {
        RefreshReflectUser(args.Equipee);
    }

    private void OnReflectHandEquipped(EntityUid uid, ReflectComponent reflector, GotEquippedHandEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        EnsureComp<ReflectUserComponent>(args.User);

        if (reflector.Enabled)
            EnableAlert(args.User);
    }

    private void OnReflectHandUnequipped(EntityUid uid, ReflectComponent reflector, GotUnequippedHandEvent args)
    {
        RefreshReflectUser(args.User);
    }

    private void OnToggleReflect(EntityUid uid, ReflectComponent reflector, ItemToggledEvent args)
    {
        reflector.Enabled = args.Activated;
        Dirty(uid);

        if (args.User == null)
            return;

        if (reflector.Enabled)
            EnableAlert(args.User.Value);
        else
            DisableAlert(args.User.Value);
    }

    private void RefreshReflectUser(EntityUid user)
    {
        foreach (var ent in _inventorySystem.GetHandOrInventoryEntities(user, SlotFlags.WITHOUT_POCKET))
        {
            if (!HasComp<ReflectComponent>(ent))
                continue;

            EnsureComp<ReflectUserComponent>(user);
            return;
        }

        RemCompDeferred<ReflectUserComponent>(user);
    }

    private void EnableAlert(EntityUid user)
    {
        // Implementation of enabling alert for the user
    }

    private void DisableAlert(EntityUid user)
    {
        // Implementation of disabling alert for the user
    }
}
