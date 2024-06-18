using Content.Client.Chat.Managers;
using Content.Client.Gameplay;
using Content.Shared.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input.Binding;

namespace Content.Client.Corvax.CustomButtonsController
{
    public sealed class CustomButtonsUIController : UIController, IOnStateChanged<GameplayState>
    {
        [Dependency] private readonly IChatManager _chatManager = default!;

        public void OnStateEntered(GameplayState state)
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.Lay,
                    InputCmdHandler.FromDelegate(_ => ToggleLayMode()))
                .Register<CustomButtonsUIController>();
        }

        public void OnStateExited(GameplayState state)
        {
            CommandBinds.Unregister<CustomButtonsUIController>();
        }

        private void ToggleLayMode()
        {
            _chatManager.SendMessage("123", Shared.Chat.ChatSelectChannel.Local);
        }
    }
}
