using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintageEssentials
{
    public class VintageEssentialsClientSystem : ModSystem
    {
        private ICoreClientAPI clientApi;
        private ChestRadiusInventoryDialog chestRadiusDialog;
        private PlayerInventorySortDialog playerSortDialog;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            this.clientApi = api;

            // Create dialogs
            chestRadiusDialog = new ChestRadiusInventoryDialog(api);
            playerSortDialog = new PlayerInventorySortDialog(api);

            // Register keybind for chest radius inventory
            clientApi.Input.RegisterHotKey("chestradius", "Open Chest Radius Inventory", GlKeys.R, HotkeyType.GUIOrOtherControls);
            clientApi.Input.SetHotKeyHandler("chestradius", OnChestRadiusHotkey);

            // Register keybind for player inventory sort
            clientApi.Input.RegisterHotKey("playerinvsort", "Sort Player Inventory", GlKeys.S, HotkeyType.InventoryHotkeys, shiftPressed: true);
            clientApi.Input.SetHotKeyHandler("playerinvsort", OnPlayerSortHotkey);

            clientApi.Logger.Notification("VintageEssentials client-side loaded. Press 'R' to open chest radius inventory.");
        }

        private bool OnChestRadiusHotkey(KeyCombination keyCombination)
        {
            if (chestRadiusDialog.IsOpened())
            {
                chestRadiusDialog.TryClose();
            }
            else
            {
                chestRadiusDialog.TryOpen();
            }
            return true;
        }

        private bool OnPlayerSortHotkey(KeyCombination keyCombination)
        {
            playerSortDialog.SortPlayerInventory();
            return true;
        }

        public override void Dispose()
        {
            chestRadiusDialog?.Dispose();
            playerSortDialog?.Dispose();
            base.Dispose();
        }
    }
}
