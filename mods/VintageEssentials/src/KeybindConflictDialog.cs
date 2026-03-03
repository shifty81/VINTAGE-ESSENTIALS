using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace VintageEssentials
{
    public class ConflictInfo
    {
        public string HotkeyCode { get; set; }
        public string ConflictingWith { get; set; }
        public string Description { get; set; }
    }
    
    public class KeybindConflictDialog : GuiDialogGeneric
    {
        private new ICoreClientAPI capi;
        private List<ConflictInfo> conflicts;
        private int currentConflictIndex = 0;
        private ModConfig config;
        private Action onKeybindsChanged;
        private bool isCapturingKey = false;
        private bool captureCtrl = false;
        private bool captureShift = false;
        private bool captureAlt = false;

        public KeybindConflictDialog(ICoreClientAPI capi, ModConfig config, Action onKeybindsChanged) : base(Lang.Get("vintageessentials:conflict-header"), capi)
        {
            this.capi = capi;
            this.config = config;
            this.onKeybindsChanged = onKeybindsChanged;
        }

        public void ShowConflicts(List<ConflictInfo> conflicts)
        {
            this.conflicts = conflicts;
            this.currentConflictIndex = 0;
            
            if (conflicts.Count > 0)
            {
                ComposeDialog();
                TryOpen();
            }
        }

        private void ComposeDialog()
        {
            if (conflicts == null || currentConflictIndex >= conflicts.Count)
            {
                TryClose();
                return;
            }

            double elemWidth = 450;
            
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            
            ElementBounds textBounds = ElementBounds.Fixed(0, 30, elemWidth, 120);
            ElementBounds currentKeyBounds = ElementBounds.Fixed(0, 160, elemWidth / 2 - 5, 30);
            ElementBounds changeKeyButtonBounds = ElementBounds.Fixed(elemWidth / 2 + 5, 160, elemWidth / 2 - 5, 30);
            ElementBounds acceptButtonBounds = ElementBounds.Fixed(0, 200, (elemWidth - 10) / 3, 30);
            ElementBounds skipButtonBounds = ElementBounds.Fixed((elemWidth - 10) / 3 + 5, 200, (elemWidth - 10) / 3, 30);
            ElementBounds skipAllButtonBounds = ElementBounds.Fixed(2 * (elemWidth - 10) / 3 + 10, 200, (elemWidth - 10) / 3, 30);
            
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(textBounds, currentKeyBounds, changeKeyButtonBounds, 
                                  acceptButtonBounds, skipButtonBounds, skipAllButtonBounds);

            var conflictInfo = conflicts[currentConflictIndex];
            string conflictMessage = GetConflictMessage(conflictInfo);
            string currentKey = config.GetKeybindDisplayString(conflictInfo.HotkeyCode);
            string buttonText = isCapturingKey ? Lang.Get("vintageessentials:config-presskey-btn") : Lang.Get("vintageessentials:conflict-changekey");
            
            SingleComposer = capi.Gui.CreateCompo("keybindconflict", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("vintageessentials:conflict-title", currentConflictIndex + 1, conflicts.Count), OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText(Lang.Get("vintageessentials:conflict-header"), CairoFont.WhiteSmallText(), 
                                   ElementBounds.Fixed(0, 5, elemWidth, 20))
                    .AddStaticText(conflictMessage, CairoFont.WhiteDetailText(), textBounds)
                    .AddStaticText($"Current: {currentKey}", CairoFont.WhiteSmallText(), currentKeyBounds)
                    .AddSmallButton(buttonText, OnChangeKeyClicked, changeKeyButtonBounds, EnumButtonStyle.Normal, "changeKeyBtn")
                    .AddSmallButton(Lang.Get("vintageessentials:conflict-accept"), OnAcceptClicked, acceptButtonBounds)
                    .AddSmallButton(Lang.Get("vintageessentials:conflict-skip"), OnSkipClicked, skipButtonBounds)
                    .AddSmallButton(Lang.Get("vintageessentials:conflict-skipall"), OnSkipAllClicked, skipAllButtonBounds)
                .EndChildElements()
                .Compose();
                
            if (isCapturingKey)
            {
                // Disable other buttons while capturing
                SingleComposer.GetButton("changeKeyBtn").Enabled = true;
            }
        }

        private string GetConflictMessage(ConflictInfo conflictInfo)
        {
            string baseMessage = GetFeatureDescription(conflictInfo.HotkeyCode);
            return Lang.Get("vintageessentials:conflict-message", baseMessage, conflictInfo.ConflictingWith);
        }
        
        private string GetFeatureDescription(string hotkeyCode)
        {
            switch (hotkeyCode)
            {
                case "chestradius":
                    return Lang.Get("vintageessentials:feature-chestradius");
                case "playerinvsort":
                    return Lang.Get("vintageessentials:feature-sort");
                case "toggleslotlock":
                    return Lang.Get("vintageessentials:feature-lock");
                case "veconfig":
                    return Lang.Get("vintageessentials:feature-config");
                default:
                    return hotkeyCode;
            }
        }

        private bool OnChangeKeyClicked()
        {
            if (!isCapturingKey)
            {
                isCapturingKey = true;
                captureCtrl = false;
                captureShift = false;
                captureAlt = false;
                ComposeDialog();
                capi.ShowChatMessage(Lang.Get("vintageessentials:conflict-presskey"));
            }
            return true;
        }
        
        public override void OnKeyDown(KeyEvent e)
        {
            if (isCapturingKey)
            {
                // Capture modifiers
                if (e.KeyCode == (int)GlKeys.ControlLeft || e.KeyCode == (int)GlKeys.ControlRight)
                {
                    captureCtrl = true;
                    return;
                }
                if (e.KeyCode == (int)GlKeys.ShiftLeft || e.KeyCode == (int)GlKeys.ShiftRight)
                {
                    captureShift = true;
                    return;
                }
                if (e.KeyCode == (int)GlKeys.AltLeft || e.KeyCode == (int)GlKeys.AltRight)
                {
                    captureAlt = true;
                    return;
                }
                
                // Capture main key
                if (e.KeyCode != (int)GlKeys.Escape && e.KeyCode != (int)GlKeys.Enter)
                {
                    string keyName = Enum.GetName(typeof(GlKeys), e.KeyCode);
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        var conflictInfo = conflicts[currentConflictIndex];
                        
                        // Update config with new keybind
                        config.CustomKeybinds[conflictInfo.HotkeyCode] = new KeybindConfig
                        {
                            Key = keyName,
                            Ctrl = captureCtrl,
                            Shift = captureShift,
                            Alt = captureAlt
                        };
                        
                        // Save config
                        config.Save(capi);
                        
                        // Notify that keybinds changed
                        onKeybindsChanged?.Invoke();
                        
                        isCapturingKey = false;
                        string keyDisplay = config.GetKeybindDisplayString(conflictInfo.HotkeyCode);
                        capi.ShowChatMessage(Lang.Get("vintageessentials:conflict-changed", keyDisplay));
                        
                        ComposeDialog();
                    }
                }
                else if (e.KeyCode == (int)GlKeys.Escape)
                {
                    // Cancel key capture
                    isCapturingKey = false;
                    ComposeDialog();
                }
                
                return;
            }
            
            base.OnKeyDown(e);
        }

        private bool OnAcceptClicked()
        {
            // Accept the current keybind and move to next
            currentConflictIndex++;
            if (currentConflictIndex < conflicts.Count)
            {
                ComposeDialog();
            }
            else
            {
                TryClose();
            }
            
            return true;
        }

        private bool OnSkipClicked()
        {
            // Skip this conflict and move to next
            currentConflictIndex++;
            if (currentConflictIndex < conflicts.Count)
            {
                ComposeDialog();
            }
            else
            {
                TryClose();
            }
            
            return true;
        }
        
        private bool OnSkipAllClicked()
        {
            // Skip all remaining conflicts
            TryClose();
            return true;
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }
    }
}
