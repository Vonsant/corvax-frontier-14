using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server.Database;
using System.Threading;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class SetAllBalanceCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _dbManager = default!;

    public string Command => "setallbalance";
    public string Description => Loc.GetString("set-all-balance-command-description");
    public string Help => Loc.GetString("set-all-balance-command-help-text", ("command",Command));

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var loc = IoCManager.Resolve<ILocalizationManager>();

        if (args.Length == 1 && int.TryParse(args[0], out var set_balance))
        {
            _dbManager.SetAllBalance(set_balance);
            return;
        }
        else if (args.Length == 1)
        {
            shell.WriteLine(Loc.GetString("shell-argument-number-invalid", ("index", "1")));
            return;
        }
        else
        {
            _dbManager.SetAllBalance(25000);
            return;
        }
    }
}

[AdminCommand(AdminFlags.Host)]
public sealed class SetBalanceCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _dbManager = default!;

    public string Command => "setbalance";
    public string Description => Loc.GetString("set-balance-command-description");
    public string Help => Loc.GetString("set-balance-command-help-text", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var loc = IoCManager.Resolve<ILocalizationManager>();
        var set_balance = 0;
        int.TryParse(args[1], out set_balance);
        if (args.Length == 1 || args.Length == 2)
        {
            var _userId = await _dbManager.GetPlayerRecordByUserName(args[0], new CancellationToken());
            if (_userId is not null) {
                var userId = _userId.UserId;
                var _profile = await _dbManager.GetPlayerPreferencesAsync(userId, new CancellationToken());
                if (_profile is not null) {
                    foreach (var item in _profile.Characters)
                    {
                        if (item.Value is HumanoidCharacterProfile profile)
                        {
                            var newProfile = new HumanoidCharacterProfile(
                                    profile.Name,
                                    profile.FlavorText,
                                    profile.Species,
                                    profile.Age,
                                    profile.Sex,
                                    profile.Gender,
                                    set_balance,
                                    profile.Appearance,
                                    profile.SpawnPriority,
                                    profile.JobPriorities,
                                    profile.PreferenceUnavailable,
                                    profile.AntagPreferences,
                                    profile.TraitPreferences,
                            new Dictionary<string, RoleLoadout>(profile.Loadouts));
                            await _dbManager.SaveCharacterSlotAsync(userId, newProfile, item.Key);
                        }
                    }
                }
            }
            return;
        }
        else
        {
            shell.WriteLine(Loc.GetString("shell-need-between-arguments", ("lower", "1"), ("upper", "2")));
            return;
        }
    }
}
