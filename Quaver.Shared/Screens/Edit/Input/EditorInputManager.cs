﻿using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.UI;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Input
{
    public class EditorInputManager
    {
        public EditorInputConfig InputConfig { get; }
        public EditScreen Screen { get; }
        private EditScreenView View { get; }

        private Dictionary<Keybind, HashSet<KeybindActions>> KeybindDictionary;
        private GenericKeyState PreviousKeyState;

        private const int HoldRepeatActionDelay = 250;
        private const int HoldRepeatActionInterval = 25;
        private Dictionary<KeybindActions, long> LastActionPress = new Dictionary<KeybindActions, long>();
        private Dictionary<KeybindActions, long> LastActionTime = new Dictionary<KeybindActions, long>();

        private static HashSet<KeybindActions> HoldRepeatActions = new HashSet<KeybindActions>()
        {
            KeybindActions.ZoomIn,
            KeybindActions.ZoomInLarge,
            KeybindActions.ZoomOut,
            KeybindActions.ZoomOutLarge,
            KeybindActions.SeekForwards,
            KeybindActions.SeekBackwards,
            KeybindActions.SeekForwards1ms,
            KeybindActions.SeekBackwards1ms,
            KeybindActions.IncreasePlaybackRate,
            KeybindActions.DecreasePlaybackRate,
            KeybindActions.ChangeToolUp,
            KeybindActions.ChangeToolDown,
        };

        private static HashSet<KeybindActions> HoldAndReleaseActions = new HashSet<KeybindActions>()
        {
        };

        private static HashSet<KeybindActions> EnabledActionsDuringGameplayPreview = new HashSet<KeybindActions>()
        {
        };

        public EditorInputManager(EditScreen screen)
        {
            InputConfig = EditorInputConfig.LoadFromConfig();
            KeybindDictionary = InputConfig.ReverseDictionary();
            PreviousKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            Screen = screen;
            View = (EditScreenView)screen.View;
        }

        public void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0) return;
            if (View.IsImGuiHovered) return;

            HandleKeyPresses();
            HandleKeyReleasesAfterHoldAction();
            HandlePluginKeyPresses();

            HandleMouseInputs();
        }

        private void HandleKeyPresses()
        {
            var keyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            var uniqueKeyPresses = keyState.UniqueKeyPresses(PreviousKeyState);
            foreach (var pressedKeybind in keyState.PressedKeybinds())
            {
                HashSet<KeybindActions> actions;

                if (!KeybindDictionary.TryGetValue(pressedKeybind, out actions)) continue;

                foreach (var action in actions)
                {
                    if (uniqueKeyPresses.Contains(pressedKeybind))
                    {
                        HandleAction(action);
                        LastActionPress[action] = GameBase.Game.TimeRunning;
                    }
                    else if (CanRepeat(action))
                        HandleAction(action);
                    else if (CanHold(action))
                        HandleAction(action, false);
                }
            }

            PreviousKeyState = keyState;
        }

        private void HandleKeyReleasesAfterHoldAction()
        {
            foreach (var action in HoldAndReleaseActions)
            {
                var binds = InputConfig.GetOrDefault(action);
                if (binds.IsNotBound()) continue;

                if (binds.IsUniqueRelease())
                    HandleAction(action, false, true);
            }
        }

        private long TimeSinceLastPress(KeybindActions action) => GameBase.Game.TimeRunning - LastActionPress.GetValueOrDefault(action, GameBase.Game.TimeRunning);
        private long TimeSinceLastAction(KeybindActions action) => GameBase.Game.TimeRunning - LastActionTime.GetValueOrDefault(action, GameBase.Game.TimeRunning);

        private bool CanRepeat(KeybindActions action)
        {
            if (!HoldRepeatActions.Contains(action))
                return false;

            return TimeSinceLastAction(action) > HoldRepeatActionInterval && TimeSinceLastPress(action) > HoldRepeatActionDelay;
        }

        private bool CanHold(KeybindActions action)
        {
            if (!HoldAndReleaseActions.Contains(action))
                return false;

            return TimeSinceLastAction(action) > HoldRepeatActionInterval;
        }

        private void HandleAction(KeybindActions action, bool isKeyPress = true, bool isRelease = false)
        {
            switch (action)
            {
                case KeybindActions.ExitEditor:
                    Screen.LeaveEditor();
                    break;
                case KeybindActions.PlayPause:
                    Screen.TogglePlayPause();
                    break;
                case KeybindActions.ZoomIn:
                    Screen.AdjustZoom(1);
                    break;
                case KeybindActions.ZoomInLarge:
                    Screen.AdjustZoom(5);
                    break;
                case KeybindActions.ZoomOut:
                    Screen.AdjustZoom(-1);
                    break;
                case KeybindActions.ZoomOutLarge:
                    Screen.AdjustZoom(-5);
                    break;
                case KeybindActions.SeekForwards:
                    Screen.SeekInDirection(Direction.Forward);
                    break;
                case KeybindActions.SeekBackwards:
                    Screen.SeekInDirection(Direction.Backward);
                    break;
                case KeybindActions.SeekForwards1ms:
                    if (!Screen.Track.IsPlaying)
                        Screen.SeekTo(Screen.Track.Time + 1);
                    break;
                case KeybindActions.SeekBackwards1ms:
                    if (!Screen.Track.IsPlaying)
                        Screen.SeekTo(Screen.Track.Time - 1);
                    break;
                case KeybindActions.SeekToStartOfSelection:
                    Screen.SeekToStart();
                    break;
                case KeybindActions.SeekToEndOfSelection:
                    Screen.SeekToEnd();
                    break;
                case KeybindActions.SeekToStart:
                    Screen.SeekToStartOfSelection();
                    break;
                case KeybindActions.SeekToEnd:
                    Screen.SeekToEndOfSelection();
                    break;
                case KeybindActions.IncreasePlaybackRate:
                    Screen.ChangeAudioPlaybackRate(Direction.Forward);
                    break;
                case KeybindActions.DecreasePlaybackRate:
                    Screen.ChangeAudioPlaybackRate(Direction.Backward);
                    break;
                case KeybindActions.ChangeToolUp:
                    Screen.ChangeTool(Direction.Backward);
                    break;
                case KeybindActions.ChangeToolDown:
                    Screen.ChangeTool(Direction.Forward);
                    break;
                case KeybindActions.ChangeToolToSelect:
                    Screen.ChangeToolTo(EditorCompositionTool.Select);
                    break;
                case KeybindActions.ChangeToolToNote:
                    Screen.ChangeToolTo(EditorCompositionTool.Note);
                    break;
                case KeybindActions.ChangeToolToLongNote:
                    Screen.ChangeToolTo(EditorCompositionTool.LongNote);
                    break;
                default:
                    return;
            }

            LastActionTime[action] = GameBase.Game.TimeRunning;
        }

        private void HandleMouseInputs()
        {
        }

        private void HandlePluginKeyPresses()
        {
            foreach (var (pluginName, keybinds) in InputConfig.PluginKeybinds)
            {
                if (keybinds.IsUniquePress())
                {
                    // Toggle plugin
                }
            }
        }
    }
}