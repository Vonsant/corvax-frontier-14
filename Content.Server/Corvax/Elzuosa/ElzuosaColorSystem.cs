using Content.Server.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.Corvax.Elzuosa
{
    public sealed class ElzuosaColorSystem : EntitySystem
    {
        [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearance = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ElzuosaColorComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<ElzuosaColorComponent, InteractUsingEvent>(OnInteractUsing);
        }

        private void OnMapInit(EntityUid uid, ElzuosaColorComponent comp, MapInitEvent args)
        {
            if (!HasComp<HumanoidAppearanceComponent>(uid))
                return;

            _humanoidAppearance.SetSkinColor(uid, comp.SkinColor, true, true);
        }

        private void OnInteractUsing(EntityUid uid, ElzuosaColorComponent comp, InteractUsingEvent args)
        {
            if (args.Handled)
                return;
            
            if (!TryComp(args.Used, out ToolComponent? tool) || !tool.Qualities.ContainsAny("Pulsing"))
                return;

            args.Handled = true;
            comp.Hacked = !comp.Hacked;

            if (comp.Hacked)
            {
                var rgb = EnsureComp<RgbLightControllerComponent>(uid);
                _rgbSystem.SetCycleRate(uid, comp.CycleRate, rgb);
            }
            else
                RemComp<RgbLightControllerComponent>(uid);
        }
    }
}
