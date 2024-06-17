using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Standing
{
    [RegisterComponent, NetworkedComponent(), AutoGenerateComponentState]
    [Access(typeof(StandingStateSystem))]
    public sealed partial class StandingStateComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public SoundSpecifier DownSound { get; private set; } = new SoundCollectionSpecifier("BodyFall");

        [DataField, AutoNetworkedField]
        public bool Standing { get; set; } = true;

        /// <summary>
        ///     List of fixtures that had their collision mask changed when the entity was downed.
        ///     Required for re-adding the collision mask.
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<string> ChangedFixtures = new();

        [DataField]
        public EntProtoId ToggleAction = "ActionToggleLieDown";

        [DataField, AutoNetworkedField]
        public EntityUid? ToggleActionEntity;


        [DataField("IsDown"), AutoNetworkedField]
        public bool IsDown = false;
    }

    public sealed partial class LieDownActionEvent : InstantActionEvent {}
    public sealed partial class StandUpActionEvent : InstantActionEvent {}
}
