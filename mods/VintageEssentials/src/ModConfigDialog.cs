using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace VintageEssentials
{
    public class ModConfigDialog : GuiDialogGeneric
    {
        private new ICoreClientAPI capi;
        private ModConfig config;
        private Action onKeybindsChanged;
        private Dictionary<string, bool> capturingKeys = new Dictionary<string, bool>();
        private Dictionary<string, KeybindConfig> tempKeybinds = new Dictionary<string, KeybindConfig>();
        private Dictionary<string, bool> tempModToggles = new Dictionary<string, bool>();
        private int currentTab = 0;

        private static readonly string[] TabNames = new string[]
        {
            "General",
            "Storage & Inventory",
            "Cooking & Food",
            "Hunting & Animals",
            "Building & Decoration",
            "Crafting & Smithing",
            "Combat & Skills",
            "World & Exploration",
            "Quality of Life",
            "Libraries"
        };

        public ModConfigDialog(ICoreClientAPI capi, ModConfig config, Action onKeybindsChanged) : base(Lang.Get("vintageessentials:config-title"), capi)
        {
            this.capi = capi;
            this.config = config;
            this.onKeybindsChanged = onKeybindsChanged;
        }

        private void ComposeDialog()
        {
            double tabWidth = 160;
            double contentWidth = 480;
            double totalWidth = tabWidth + contentWidth + 20;
            double dialogHeight = 520;

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);

            var composer = capi.Gui.CreateCompo("modconfig", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("vintageessentials:config-title"), OnTitleBarClose)
                .BeginChildElements(bgBounds);

            var allBounds = new List<ElementBounds>();

            // Compose tab buttons on the left
            double tabY = 30;
            for (int i = 0; i < TabNames.Length; i++)
            {
                int tabIndex = i;
                string prefix = (i == currentTab) ? "\u25B8 " : "  ";
                string tabLabel = prefix + TabNames[i];
                ElementBounds tabBounds = ElementBounds.Fixed(0, tabY, tabWidth, 28);
                allBounds.Add(tabBounds);

                EnumButtonStyle style = (i == currentTab) ? EnumButtonStyle.Small : EnumButtonStyle.Normal;
                composer.AddSmallButton(tabLabel, () => OnTabClicked(tabIndex), tabBounds, style, "tab_" + i);
                tabY += 32;
            }

            // Compose content area on the right
            double contentX = tabWidth + 15;
            double contentY = 30;

            if (currentTab == 0)
            {
                ComposeGeneralTab(composer, allBounds, contentX, contentY, contentWidth);
            }
            else
            {
                ModCategory category = (ModCategory)(currentTab - 1);
                ComposeCategoryTab(composer, allBounds, contentX, contentY, contentWidth, category);
            }

            // Save and Cancel buttons at the bottom
            double bottomY = Math.Max(tabY, dialogHeight - 45);
            ElementBounds saveButtonBounds = ElementBounds.Fixed(contentX, bottomY, contentWidth / 2 - 5, 30);
            ElementBounds cancelButtonBounds = ElementBounds.Fixed(contentX + contentWidth / 2 + 5, bottomY, contentWidth / 2 - 5, 30);
            allBounds.Add(saveButtonBounds);
            allBounds.Add(cancelButtonBounds);

            composer
                .AddSmallButton(Lang.Get("vintageessentials:config-save"), OnSaveClicked, saveButtonBounds)
                .AddSmallButton(Lang.Get("vintageessentials:config-cancel"), OnCancelClicked, cancelButtonBounds)
                .EndChildElements();

            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(allBounds.ToArray());

            SingleComposer = composer.Compose();

            // Set initial values for General tab controls
            if (currentTab == 0)
            {
                SingleComposer.GetNumberInput("maxslotsInput")?.SetValue(config.MaxLockedSlots);
                var slider = SingleComposer.GetSlider("stackSizeSlider");
                slider?.SetValues(config.StackSizeMultiplier, 1, 200, 1);
            }
            else
            {
                // Set initial toggle values for category tab
                ModCategory category = (ModCategory)(currentTab - 1);
                var mods = ConglomerateModManager.GetModsByCategory(category);
                foreach (var mod in mods)
                {
                    string key = "modtoggle_" + SanitizeKey(mod.FileName);
                    var toggle = SingleComposer.GetSwitch(key);
                    if (toggle != null)
                    {
                        bool enabled = tempModToggles.ContainsKey(mod.FileName)
                            ? tempModToggles[mod.FileName]
                            : config.IsModEnabled(mod.FileName);
                        toggle.SetValue(enabled);
                    }
                }
            }
        }

        private void ComposeGeneralTab(GuiComposer composer, List<ElementBounds> allBounds, double x, double y, double width)
        {
            double currentY = y;

            // Section title
            ElementBounds sectionTitle = ElementBounds.Fixed(x, currentY, width, 25);
            allBounds.Add(sectionTitle);
            composer.AddStaticText(Lang.Get("vintageessentials:config-tab-general-title"), CairoFont.WhiteDetailText(), sectionTitle);
            currentY += 30;

            // Max locked slots
            ElementBounds maxSlotsLabel = ElementBounds.Fixed(x, currentY, width / 2, 25);
            ElementBounds maxSlotsInput = ElementBounds.Fixed(x + width / 2 + 10, currentY, width / 2 - 10, 25);
            allBounds.Add(maxSlotsLabel);
            allBounds.Add(maxSlotsInput);
            composer.AddStaticText(Lang.Get("vintageessentials:config-maxslots"), CairoFont.WhiteSmallText(), maxSlotsLabel);
            composer.AddNumberInput(maxSlotsInput, OnMaxSlotsChanged, CairoFont.WhiteSmallText(), "maxslotsInput");
            currentY += 35;

            // Stack size multiplier
            ElementBounds stackLabel = ElementBounds.Fixed(x, currentY, width, 25);
            allBounds.Add(stackLabel);
            composer.AddStaticText(Lang.Get("vintageessentials:config-stackmult"), CairoFont.WhiteSmallText(), stackLabel);
            currentY += 25;

            ElementBounds stackSlider = ElementBounds.Fixed(x, currentY, width - 70, 25);
            ElementBounds stackValue = ElementBounds.Fixed(x + width - 65, currentY, 65, 25);
            allBounds.Add(stackSlider);
            allBounds.Add(stackValue);
            composer.AddSlider(OnStackSizeChanged, stackSlider, "stackSizeSlider");
            composer.AddStaticText($"{config.StackSizeMultiplier}x", CairoFont.WhiteSmallText(), stackValue, "stackSizeValue");
            currentY += 35;

            ElementBounds stackInfo = ElementBounds.Fixed(x, currentY, width, 35);
            allBounds.Add(stackInfo);
            composer.AddStaticText(Lang.Get("vintageessentials:config-stackinfo"), CairoFont.WhiteSmallText(), stackInfo);
            currentY += 45;

            // Keybinds section
            ElementBounds keybindsTitle = ElementBounds.Fixed(x, currentY, width, 25);
            allBounds.Add(keybindsTitle);
            composer.AddStaticText(Lang.Get("vintageessentials:config-keybinds"), CairoFont.WhiteDetailText(), keybindsTitle);
            currentY += 30;

            var keybindNames = new Dictionary<string, string>
            {
                { "chestradius", "Chest Radius Inventory" },
                { "playerinvsort", "Sort Player Inventory" },
                { "toggleslotlock", "Toggle Slot Locking" },
                { "veconfig", "Open Settings" }
            };

            foreach (var kvp in keybindNames)
            {
                string code = kvp.Key;
                string name = kvp.Value;

                if (!tempKeybinds.ContainsKey(code))
                {
                    tempKeybinds[code] = config.CustomKeybinds.ContainsKey(code)
                        ? new KeybindConfig
                        {
                            Key = config.CustomKeybinds[code].Key,
                            Ctrl = config.CustomKeybinds[code].Ctrl,
                            Shift = config.CustomKeybinds[code].Shift,
                            Alt = config.CustomKeybinds[code].Alt
                        }
                        : new KeybindConfig();
                }

                string currentKey = GetKeybindDisplay(tempKeybinds[code]);
                bool isCapturing = capturingKeys.ContainsKey(code) && capturingKeys[code];
                string buttonText = isCapturing ? Lang.Get("vintageessentials:config-presskey-btn") : Lang.Get("vintageessentials:config-change");

                ElementBounds labelBounds = ElementBounds.Fixed(x, currentY, width * 0.38, 25);
                ElementBounds currentBounds = ElementBounds.Fixed(x + width * 0.38 + 5, currentY, width * 0.28, 25);
                ElementBounds buttonBounds = ElementBounds.Fixed(x + width * 0.68, currentY, width * 0.32, 25);
                allBounds.Add(labelBounds);
                allBounds.Add(currentBounds);
                allBounds.Add(buttonBounds);

                composer
                    .AddStaticText(name + ":", CairoFont.WhiteSmallText(), labelBounds)
                    .AddStaticText(currentKey, CairoFont.WhiteSmallText(), currentBounds, $"keybind_{code}_text")
                    .AddSmallButton(buttonText, () => OnChangeKeybind(code), buttonBounds, EnumButtonStyle.Normal, $"keybind_{code}_btn");

                currentY += 30;
            }
        }

        private void ComposeCategoryTab(GuiComposer composer, List<ElementBounds> allBounds, double x, double y, double width, ModCategory category)
        {
            double currentY = y;
            string categoryName = ConglomerateModManager.GetCategoryName(category);

            // Category title
            ElementBounds titleBounds = ElementBounds.Fixed(x, currentY, width, 25);
            allBounds.Add(titleBounds);
            composer.AddStaticText(categoryName, CairoFont.WhiteDetailText(), titleBounds);
            currentY += 8;

            // Category description
            ElementBounds descBounds = ElementBounds.Fixed(x, currentY + 22, width, 20);
            allBounds.Add(descBounds);
            composer.AddStaticText(Lang.Get("vintageessentials:config-category-desc"), CairoFont.WhiteSmallText(), descBounds);
            currentY += 50;

            // Mod entries
            var mods = ConglomerateModManager.GetModsByCategory(category);

            // Enable All / Disable All buttons
            ElementBounds enableAllBounds = ElementBounds.Fixed(x, currentY, width / 2 - 5, 25);
            ElementBounds disableAllBounds = ElementBounds.Fixed(x + width / 2 + 5, currentY, width / 2 - 5, 25);
            allBounds.Add(enableAllBounds);
            allBounds.Add(disableAllBounds);
            composer.AddSmallButton(Lang.Get("vintageessentials:config-enableall"), () => OnToggleAll(category, true), enableAllBounds);
            composer.AddSmallButton(Lang.Get("vintageessentials:config-disableall"), () => OnToggleAll(category, false), disableAllBounds);
            currentY += 35;

            foreach (var mod in mods)
            {
                string sanitizedKey = SanitizeKey(mod.FileName);

                // Toggle switch
                ElementBounds switchBounds = ElementBounds.Fixed(x, currentY + 3, 40, 20);
                allBounds.Add(switchBounds);
                composer.AddSwitch(
                    (val) => OnModToggled(mod.FileName, val),
                    switchBounds,
                    "modtoggle_" + sanitizedKey
                );

                // Mod name
                ElementBounds nameBounds = ElementBounds.Fixed(x + 50, currentY, width - 50, 20);
                allBounds.Add(nameBounds);
                composer.AddStaticText(mod.DisplayName, CairoFont.WhiteSmallText(), nameBounds);
                currentY += 20;

                // Mod description
                ElementBounds modDescBounds = ElementBounds.Fixed(x + 50, currentY, width - 50, 18);
                allBounds.Add(modDescBounds);

                var smallFont = CairoFont.WhiteSmallText();
                smallFont.Color[3] = 0.7;
                smallFont.UnscaledFontsize -= 2;
                composer.AddStaticText(mod.Description, smallFont, modDescBounds);
                currentY += 25;
            }
        }

        private string SanitizeKey(string fileName)
        {
            // Replace characters that may cause issues in element keys
            return fileName.Replace(" ", "_").Replace(".", "_").Replace("-", "_");
        }

        private bool OnTabClicked(int tabIndex)
        {
            currentTab = tabIndex;
            ComposeDialog();
            return true;
        }

        private void OnModToggled(string fileName, bool enabled)
        {
            tempModToggles[fileName] = enabled;
        }

        private bool OnToggleAll(ModCategory category, bool enabled)
        {
            var mods = ConglomerateModManager.GetModsByCategory(category);
            foreach (var mod in mods)
            {
                tempModToggles[mod.FileName] = enabled;
                string key = "modtoggle_" + SanitizeKey(mod.FileName);
                var toggle = SingleComposer.GetSwitch(key);
                toggle?.SetValue(enabled);
            }
            return true;
        }

        private bool OnStackSizeChanged(int newValue)
        {
            config.StackSizeMultiplier = newValue;

            var textElem = SingleComposer.GetStaticText("stackSizeValue");
            if (textElem != null)
            {
                textElem.SetValue($"{newValue}x");
            }

            return true;
        }

        private string GetKeybindDisplay(KeybindConfig kb)
        {
            if (string.IsNullOrEmpty(kb.Key))
            {
                return "Not Set";
            }

            string result = "";
            if (kb.Ctrl) result += "Ctrl+";
            if (kb.Shift) result += "Shift+";
            if (kb.Alt) result += "Alt+";
            result += kb.Key;
            return result;
        }

        private bool OnChangeKeybind(string hotkeyCode)
        {
            bool alreadyCapturing = capturingKeys.ContainsValue(true);
            if (alreadyCapturing)
            {
                return true;
            }

            capturingKeys[hotkeyCode] = true;
            ComposeDialog();
            capi.ShowChatMessage(Lang.Get("vintageessentials:config-presskey", hotkeyCode));
            return true;
        }

        public override void OnKeyDown(KeyEvent e)
        {
            string capturingCode = null;
            foreach (var kvp in capturingKeys)
            {
                if (kvp.Value)
                {
                    capturingCode = kvp.Key;
                    break;
                }
            }

            if (capturingCode != null)
            {
                if (e.KeyCode == (int)GlKeys.ControlLeft || e.KeyCode == (int)GlKeys.ControlRight ||
                    e.KeyCode == (int)GlKeys.ShiftLeft || e.KeyCode == (int)GlKeys.ShiftRight ||
                    e.KeyCode == (int)GlKeys.AltLeft || e.KeyCode == (int)GlKeys.AltRight)
                {
                    return;
                }

                if (e.KeyCode == (int)GlKeys.Escape)
                {
                    capturingKeys[capturingCode] = false;
                    ComposeDialog();
                    capi.ShowChatMessage(Lang.Get("vintageessentials:config-keybindcancelled"));
                    return;
                }

                string keyName = Enum.GetName(typeof(GlKeys), e.KeyCode);
                if (!string.IsNullOrEmpty(keyName))
                {
                    tempKeybinds[capturingCode] = new KeybindConfig
                    {
                        Key = keyName,
                        Ctrl = capi.Input.KeyboardKeyState[(int)GlKeys.ControlLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.ControlRight],
                        Shift = capi.Input.KeyboardKeyState[(int)GlKeys.ShiftLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.ShiftRight],
                        Alt = capi.Input.KeyboardKeyState[(int)GlKeys.AltLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.AltRight]
                    };

                    capturingKeys[capturingCode] = false;
                    ComposeDialog();

                    string keyDisplay = GetKeybindDisplay(tempKeybinds[capturingCode]);
                    capi.ShowChatMessage(Lang.Get("vintageessentials:config-keybindset", keyDisplay));
                }
                return;
            }

            base.OnKeyDown(e);
        }

        private void OnMaxSlotsChanged(string value)
        {
            if (int.TryParse(value, out int slots))
            {
                config.MaxLockedSlots = Math.Max(1, Math.Min(20, slots));
            }
        }

        private bool OnSaveClicked()
        {
            // Apply temp keybinds to config
            foreach (var kvp in tempKeybinds)
            {
                config.CustomKeybinds[kvp.Key] = kvp.Value;
            }

            // Apply temp mod toggles to config
            foreach (var kvp in tempModToggles)
            {
                config.SetModEnabled(kvp.Key, kvp.Value);
            }

            config.Save(capi);
            capi.ShowChatMessage(Lang.Get("vintageessentials:config-saved"));

            onKeybindsChanged?.Invoke();

            TryClose();
            return true;
        }

        private bool OnCancelClicked()
        {
            tempKeybinds.Clear();
            capturingKeys.Clear();
            tempModToggles.Clear();
            config = ModConfig.Load(capi);
            TryClose();
            return true;
        }

        private void OnTitleBarClose()
        {
            OnCancelClicked();
        }

        public override bool TryOpen()
        {
            tempKeybinds.Clear();
            capturingKeys.Clear();
            tempModToggles.Clear();
            currentTab = 0;
            ComposeDialog();
            return base.TryOpen();
        }

        public override string ToggleKeyCombinationCode => "modconfig";
    }
}
