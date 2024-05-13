using Robust.Shared.Player;

namespace Content.Shared._NF.Bank.Events;
public sealed class BalanceChangedEvent : EntityEventArgs
{
    public readonly ulong Amount;
    public readonly ICommonSession Player;

    public BalanceChangedEvent(ulong amount, ICommonSession player)
    {
        Amount = amount;
        Player = player;
    }
}
