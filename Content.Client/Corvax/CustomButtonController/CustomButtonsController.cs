using Content.Client.Gameplay;
using Content.Shared.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input.Binding;

namespace Content.Client.Corvax.CustomButtonsController
{
    public sealed class CustomButtonsUIController : UIController, IOnStateChanged<GameplayState>
    {
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

        }
    }
}
