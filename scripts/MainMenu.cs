using Godot;
using Godot.Collections;
using System;
using System.Linq;
using static AudioManager;

public partial class MainMenu : Control
{
    Game game;
    AudioManager audioMgr;
    CheckButton fullscreenCheckButton;
    OptionButton resolutionOptionsButton;
    readonly Vector2I[] resolutionsArray =
    {
		//new(640, 360),
		//new(1024, 576),
		new(1280, 720),
        new(1600, 900),
        new(1920, 1080),
        new(2048, 1152),
        new(2560, 1440),
        new(3200, 1800),
        new(3840, 2160),
    };
    Vector2I[] userResolutionsArray;
    PanelContainer optionsContainer;
    PanelContainer menuContainer;

    Dictionary<string, string> assignedInputEvents = new();
    Dictionary<string, Array<InputEvent>> defaultActionEvents = new();
    const string JOYSTICK_LEFT_NAME = "Left Gamepad Joystick";
    const string JOYSTICK_RIGHT_NAME = "Right Gamepad Joystick";
    public Dictionary<int, string> joyButtonNames = new()
    {
        { (int)JoyButton.A, "A Gamepad Button" },
        { (int)JoyButton.B, "B Gamepad Button" },
        { (int)JoyButton.X, "X Gamepad Button" },
        { (int)JoyButton.Y, "Y Gamepad Button" },
        { (int)JoyButton.LeftShoulder, "Left Shoulder Gamepad Button" },
        { (int)JoyButton.RightShoulder, "Right Shoulder Gamepad Button" },
        { (int)JoyButton.LeftStick, "Left Stick Gamepad Button" },
        { (int)JoyButton.RightStick, "Right Stick Gamepad Button" },
        { (int)JoyButton.Start, "Start Gamepad Button" },
        { (int)JoyButton.Guide, "Guide Gamepad Button" },
        { (int)JoyButton.Back, "Back Gamepad Button" }
    };
    public Dictionary<int, string> joyAxisNames = new()
    {
        { (int)JoyAxis.TriggerLeft, "A Gamepad Button" },
        { (int)JoyAxis.TriggerRight, "B Gamepad Button" },
    };

    [Export]
    Dictionary<string, string> actionNameMap = new() {
        { "forward", "Forward" },
        { "backward", "Backward" },
        { "left", "Left" },
        { "right", "Right" },
        { "escape", "Escape" }
    };
    [Export] Texture2D addButtonTexture;
    [Export] Texture2D removeButtonTexture;
    private Tree tree;
    private TreeItem editingItem;
    private string lastInputReadableName;
    private string editingActionName;
    private string assignmentPlaceholderText;

    Dictionary<TreeItem, string> treeItemAddMap = new();
    Dictionary<TreeItem, InputEvent> treeItemRemoveMap = new();
    Dictionary<TreeItem, string> treeItemActionMap = new();

    AcceptDialog oneInputMinimumDialog;
    AcceptDialog alreadyAssignedDialog;
    ConfirmationDialog keyAssignmentDialog;
    ConfirmationDialog keyDeletionDialog;

    public string GetText(InputEvent inputEvent)
    {
        if (inputEvent is InputEventJoypadButton joyButtonEvent)
        {
            if (joyButtonNames.ContainsKey((int)joyButtonEvent.ButtonIndex))
                return joyButtonNames[(int)joyButtonEvent.ButtonIndex];
        }
        else if (inputEvent is InputEventJoypadMotion joyMotionEvent)
        {
            var fullString = "";
            var directionString = "";
            bool isRightOrDown = joyMotionEvent.AxisValue > 0.0;
            if (joyAxisNames.ContainsKey((int)joyMotionEvent.Axis))
                return joyAxisNames[(int)joyMotionEvent.Axis];
            switch (joyMotionEvent.Axis)
            {
                case JoyAxis.LeftX:
                    fullString = JOYSTICK_LEFT_NAME;
                    directionString = isRightOrDown ? "Right" : "Left";
                    break;
                case JoyAxis.LeftY:
                    fullString = JOYSTICK_LEFT_NAME;
                    directionString = isRightOrDown ? "Down" : "Up";
                    break;
                case JoyAxis.RightX:
                    fullString = JOYSTICK_RIGHT_NAME;
                    directionString = isRightOrDown ? "Right" : "Left";
                    break;
                case JoyAxis.RightY:
                    fullString = JOYSTICK_RIGHT_NAME;
                    directionString = isRightOrDown ? "Down" : "Up";
                    break;
            }
            fullString += " " + directionString;
            return fullString;
        }
        else if (inputEvent is InputEventKey inputEventKey)
        {
            return OS.GetKeycodeString(inputEventKey.GetPhysicalKeycodeWithModifiers());
        }
        return inputEvent.AsText();
    }

    enum ConfigSections
    {
        Video,
        Input
    }
    enum ConfigKeys
    {
        FullscreenEnabled,
        ScreenResolution,
    }

    public override void _Ready()
    {
        InputMap.LoadFromProjectSettings();
        game = GetNode<Game>("/root/Game");
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        menuContainer = GetNode<PanelContainer>("MainContainer/MenuContainer");
        optionsContainer = GetNode<PanelContainer>("MainContainer/OptionsContainer");
        fullscreenCheckButton = optionsContainer.GetNode<CheckButton>("VBoxContainer/HBoxContainer2/FullscreenCheckButton");
        resolutionOptionsButton = optionsContainer.GetNode<OptionButton>("VBoxContainer/HBoxContainer/ResolutionOptionButton");
        userResolutionsArray = (Vector2I[])resolutionsArray.Clone();
        Window win = GetWindow();
        LoadSettingsFromConfig(win);
        UpdateUI(win);
        win.SizeChanged += () => PreselectResolution(win);

        keyAssignmentDialog = GetNode<ConfirmationDialog>("MainContainer/OptionsContainer/KeyAssignmentDialog");
        keyDeletionDialog = GetNode<ConfirmationDialog>("MainContainer/OptionsContainer/KeyDeletionDialog");
        oneInputMinimumDialog = GetNode<AcceptDialog>("MainContainer/OptionsContainer/OneInputMinimumDialog");
        alreadyAssignedDialog = GetNode<AcceptDialog>("MainContainer/OptionsContainer/AlreadyAssignedDialog");
        assignmentPlaceholderText = keyAssignmentDialog.DialogText;
        tree = GetNode<Tree>("MainContainer/OptionsContainer/VBoxContainer/Tree");
        BuildAssignedInputEvents();
        BuildUITree();
        HorizontallyAlignPopupLabels();
    }

    private void ButtonHover()
    {
        audioMgr.Play(Audio.ButtonHover, AudioChannel.SFX2);
    }

    private void SetDefaultInputs()
    {
        var actionList = GetFilteredActionNames();
        foreach (var actionName in actionList)
            defaultActionEvents[actionName] = InputMap.ActionGetEvents(actionName);
    }

    private void SetInputsFromConfig()
    {
        var actionList = GetFilteredActionNames();
        foreach (var actionName in actionList)
            SetInputFromConfig(actionName);
    }

    private void SetInputFromConfig(string actionName)
    {
        var actionEvents = InputMap.ActionGetEvents(actionName);
        var configEvents = Config.GetConfig(ConfigSections.Input.ToString(), actionName, actionEvents).AsGodotArray<InputEvent>();
        if (configEvents == actionEvents)
        {
            return;
        }
        if (configEvents.Count == 0)
        {
            Config.EraseSectionKey(ConfigSections.Input.ToString(), actionName);
            return;
        }
        InputMap.ActionEraseEvents(actionName);
        foreach (var configEvent in configEvents)
        {
            if (!actionEvents.Contains(configEvent))
            {
                InputMap.ActionAddEvent(actionName, configEvent);
            }
        }
    }

    private void LoadSettingsFromConfig(Window win)
    {
        SetDefaultInputs();
        SetInputsFromConfig();
        SetVideoFromConfig(win);
    }

    private void SetVideoFromConfig(Window win)
    {
        var fullscreenEnabled = Config.GetConfig(ConfigSections.Video.ToString(), ConfigKeys.FullscreenEnabled.ToString(), false);
        SetFullscreenEnabled((bool)fullscreenEnabled, win);
        var screenResolution = Config.GetConfig(ConfigSections.Video.ToString(), ConfigKeys.ScreenResolution.ToString(), new Vector2I(1920, 1080));
        SetScreenResolution((Vector2I)screenResolution, win);
    }

    private void UpdateUI(Window win)
    {
        fullscreenCheckButton.ButtonPressed = IsFullscreen(win);
        PreselectResolution(win);
        UpdateResolutionOptionsEnabled(win);
    }

    private void PreselectResolution(Window win)
    {
        Vector2I currentResolution = win.Size;
        if (!userResolutionsArray.Contains(currentResolution))
        {
            userResolutionsArray.Append(currentResolution);
            userResolutionsArray = userResolutionsArray.OrderBy(r => r.X).ToArray();
        }
        resolutionOptionsButton.Clear();
        foreach (var resolution in userResolutionsArray)
        {
            var resolutionString = $"{resolution.X} x {resolution.Y}";
            resolutionOptionsButton.AddItem(resolutionString);
            if (!resolutionsArray.Contains(resolution))
            {
                var lastIndex = resolutionOptionsButton.ItemCount - 1;
                resolutionOptionsButton.SetItemDisabled(lastIndex, true);
            }
        }
        var currentResolutionIndex = System.Array.IndexOf(userResolutionsArray, currentResolution);
        resolutionOptionsButton.Select(currentResolutionIndex);
    }

    private void UpdateResolutionOptionsEnabled(Window win)
    {
        if (OS.HasFeature("web"))
        {
            resolutionOptionsButton.Disabled = true;
            resolutionOptionsButton.TooltipText = "Disabled for web";
        }
        else if (IsFullscreen(win))
        {
            resolutionOptionsButton.Disabled = true;
            resolutionOptionsButton.TooltipText = "Disabled for fullscreen";
        }
        else
        {
            resolutionOptionsButton.Disabled = false;
            resolutionOptionsButton.TooltipText = "Select a screen size";
        }
    }

    private static bool IsFullscreen(Window win)
    {
        return (win.Mode == Window.ModeEnum.ExclusiveFullscreen) || (win.Mode == Window.ModeEnum.Fullscreen);
    }

    private static void SetFullscreenEnabled(bool value, Window win)
    {
        win.Mode = value ? Window.ModeEnum.ExclusiveFullscreen : Window.ModeEnum.Windowed;
        Config.SetConfig(ConfigSections.Video.ToString(), ConfigKeys.FullscreenEnabled.ToString(), value);
    }

    private static void SetScreenResolution(Vector2I value, Window win)
    {
        if (value.X == 0 || value.Y == 0)
            return;
        win.Size = value;
        Config.SetConfig(ConfigSections.Video.ToString(), ConfigKeys.ScreenResolution.ToString(), value);
        var rect = DisplayServer.ScreenGetUsableRect(win.CurrentScreen);
        win.Position = rect.Position + (rect.Size / 2 - win.Size / 2);
    }

    private void FullscreenButtonToggled(bool toggled)
    {
        var win = GetWindow();
        SetFullscreenEnabled(toggled, win);
        UpdateResolutionOptionsEnabled(win);
    }

    private void ResolutionOptionsItemSelected(int index)
    {
        if (index < 0 || index >= userResolutionsArray.Length)
            return;
        SetScreenResolution(userResolutionsArray[index], GetWindow());
    }

    private void HorizontallyAlignPopupLabels()
    {
        keyAssignmentDialog.GetLabel().HorizontalAlignment = HorizontalAlignment.Center;
        keyDeletionDialog.GetLabel().HorizontalAlignment = HorizontalAlignment.Center;
        oneInputMinimumDialog.GetLabel().HorizontalAlignment = HorizontalAlignment.Center;
        alreadyAssignedDialog.GetLabel().HorizontalAlignment = HorizontalAlignment.Center;
    }

    private void StartTree()
    {
        tree.Clear();
        tree.CreateItem();
    }

    private void BuildUITree()
    {
        StartTree();
        var actionNames = GetFilteredActionNames();
        foreach (var actionName in actionNames)
        {
            InputMap.LoadFromProjectSettings();
            var inputEvents = InputMap.ActionGetEvents(actionName);
            if (inputEvents.Count < 1)
            {
                GD.Print($"{actionName} is empty");
                continue;
            }
            var readableName = GetActionReadableName(actionName);
            AddActionAsTreeItem(readableName, actionName, inputEvents);
        }
    }

    private void AddInputEventAsTreeItem(string actionName, InputEvent inputEvent, TreeItem parentItem)
    {
        TreeItem inputTreeItem = tree.CreateItem(parentItem);
        inputTreeItem.SetText(0, GetText(inputEvent));
        if (removeButtonTexture != null)
            inputTreeItem.AddButton(0, removeButtonTexture, -1, false, "Remove");
        treeItemRemoveMap[inputTreeItem] = inputEvent;
        treeItemActionMap[inputTreeItem] = actionName;
    }

    private void AddActionAsTreeItem(string readableName, string actionName, Array<InputEvent> inputEvents)
    {
        TreeItem rootTreeItem = tree.GetRoot();
        TreeItem actionTreeItem = tree.CreateItem(rootTreeItem);
        actionTreeItem.SetText(0, readableName);
        treeItemAddMap[actionTreeItem] = actionName;
        if (addButtonTexture != null)
        {
            actionTreeItem.AddButton(0, addButtonTexture, -1, false, "Add");
        }
        foreach (var inputEvent in inputEvents)
        {
            AddInputEventAsTreeItem(actionName, inputEvent, actionTreeItem);
        }
    }

    private void BuildAssignedInputEvents()
    {
        assignedInputEvents.Clear();
        var actionNames = GetFilteredActionNames();
        foreach (var actionName in actionNames)
        {
            var inputEvents = InputMap.ActionGetEvents(actionName);
            foreach (var inputEvent in inputEvents)
            {
                AssignInputEvent(inputEvent, actionName);
            }
        }
    }

    private void AssignInputEvent(InputEvent inputEvent, string actionName)
    {
        assignedInputEvents[GetText(inputEvent)] = actionName;
    }

    private string GetActionReadableName(StringName actionName)
    {
        var readableName = actionName.ToString();
        if (actionNameMap.ContainsKey(readableName))
            readableName = actionNameMap[readableName];
        else
            actionNameMap[readableName] = readableName;
        return readableName;
    }

    private static Array<StringName> GetFilteredActionNames()
    {
        var returnList = new Array<StringName>();
        var actionList = InputMap.GetActions();
        foreach (var actionName in actionList)
            if (!actionName.ToString().StartsWith("ui_"))
                returnList.Add(actionName);
        return returnList;
    }

    private void PopupAddActionEvent(TreeItem item)
    {
        if (!treeItemAddMap.ContainsKey(item))
            return;
        editingItem = item;
        editingActionName = treeItemAddMap[item];
        keyAssignmentDialog.Title = $"Assign Key for {GetActionReadableName(editingActionName)}";
        keyAssignmentDialog.DialogText = assignmentPlaceholderText;
        keyAssignmentDialog.GetOkButton().Disabled = true;
        keyAssignmentDialog.PopupCentered();
    }

    private void PopupRemoveActionEvent(TreeItem item)
    {
        if (!treeItemRemoveMap.ContainsKey(item))
            return;
        editingItem = item;
        editingActionName = treeItemActionMap[item];
        string readableActionName = GetActionReadableName(editingActionName);
        keyDeletionDialog.Title = $"Remove Key for {readableActionName}";
        keyDeletionDialog.DialogText = $"Are you sure you want to remove {item.GetText(0)} from {readableActionName}?";
        keyDeletionDialog.PopupCentered();
    }

    private string GetActionForInputEvent(InputEvent @event)
    {
        if (assignedInputEvents.ContainsKey(GetText(@event)))
        {
            return assignedInputEvents[GetText(@event)];
        }
        return "";
    }

    private void AssignInputEventToAction(InputEvent @event, string actionName)
    {
        if (@event is InputEventMouseButton)
            return;

        AssignInputEvent(@event, actionName);
        InputMap.ActionAddEvent(actionName, @event);
        var actionEvents = InputMap.ActionGetEvents(actionName);
        Config.SetConfig(ConfigSections.Input.ToString(), actionName, actionEvents);
        AddInputEventAsTreeItem(actionName, @event, editingItem);
    }

    private void AddActionEvent()
    {
        var assignDialog = (KeyAssignmentDialog)keyAssignmentDialog;
        var lastInputEvent = assignDialog.lastInputEvent;
        lastInputReadableName = assignDialog.DialogText;
        if (lastInputEvent != null)
        {
            var assignedAction = GetActionForInputEvent(lastInputEvent);
            if (!string.IsNullOrEmpty(assignedAction))
            {
                var readableActionName = GetActionReadableName(assignedAction);
                alreadyAssignedDialog.DialogText = $"{lastInputReadableName} already assigned to {readableActionName}.";
                alreadyAssignedDialog.PopupCentered();
            }
            else
                AssignInputEventToAction(lastInputEvent, editingActionName);
        }
        editingActionName = "";
    }

    private void RemoveInputEvent(InputEvent inputEvent)
    {
        assignedInputEvents.Remove(GetText(inputEvent));
    }

    private void RemoveInputEventFromAction(InputEvent inputEvent, string actionName)
    {
        RemoveInputEvent(inputEvent);
        RemoveActionInputEvent(actionName, inputEvent);
    }

    private void RemoveActionInputEvent(string actionName, InputEvent inputEvent)
    {
        InputMap.ActionEraseEvent(actionName, inputEvent);
        var actionEvents = InputMap.ActionGetEvents(actionName);
        var configEvents = Config.GetConfig(ConfigSections.Input.ToString(), actionName, actionEvents).AsGodotArray();
        configEvents.Remove(inputEvent);
        Config.SetConfig(ConfigSections.Input.ToString(), actionName, configEvents);
    }

    private bool CanRemoveInputEvent(string actionName)
    {
        return InputMap.ActionGetEvents(actionName).Count > 1;
    }

    private void RemoveActionEvent(TreeItem item)
    {
        if (!treeItemRemoveMap.ContainsKey(item))
            return;
        var actionName = treeItemActionMap[item];
        var inputEvent = treeItemRemoveMap[item];
        if (!CanRemoveInputEvent(actionName))
        {
            var readableActionName = GetActionReadableName(actionName);
            oneInputMinimumDialog.DialogText = $"{readableActionName} must have at least one key or button assigned.";
            oneInputMinimumDialog.PopupCentered();
            return;
        }
        RemoveInputEventFromAction(inputEvent, actionName);
        var parentTreeItem = item.GetParent();
        parentTreeItem.RemoveChild(item);
    }

    private void CheckItemActions(TreeItem item)
    {
        if (treeItemAddMap.ContainsKey(item))
            PopupAddActionEvent(item);
        else if (treeItemRemoveMap.ContainsKey(item))
            PopupRemoveActionEvent(item);
    }

    private void ResetToDefaultInputs()
    {
        Config.EraseSection(ConfigSections.Input.ToString());
        foreach (var actionName in defaultActionEvents.Keys)
        {
            InputMap.ActionEraseEvents(actionName);
            var inputEvents = defaultActionEvents[actionName];
            foreach (var inputEvent in inputEvents)
                InputMap.ActionAddEvent(actionName, inputEvent);
        }
    }

    private void OnTreeItemActivated()
    {
        var item = tree.GetSelected();
        CheckItemActions(item);
    }

    private void OnResetButtonPressed()
    {
        ResetToDefaultInputs();
        BuildAssignedInputEvents();
        BuildUITree();
    }

    private void OnTreeButtonClicked(TreeItem item, int column, int id, int mouseButtonIndex)
    {
        CheckItemActions(item);
    }

    private void OnKeyAssignmentDialogCanceled()
    {
        editingActionName = "";
    }

    private void OnKeyAssignmentDialogConfirmed()
    {
        AddActionEvent();
    }

    private void OnKeyDeletionDialogConfirmed()
    {
        if (IsInstanceValid(editingItem))
            RemoveActionEvent(editingItem);
    }

    private void PlayButtonPressed()
    {
        game.ChangeScene("world");
        audioMgr.Play(Audio.ButtonPlay, AudioChannel.SFX3);
    }

    private void OptionsButtonPressed()
    {
        optionsContainer.Visible = true;
        menuContainer.Visible = false;
        audioMgr.Play(Audio.ButtonOptions, AudioChannel.SFX3);
    }

    private void BackButtonPressed()
    {
        optionsContainer.Visible = false;
        menuContainer.Visible = true;
    }

    private void ExitButtonPressed()
    {
        audioMgr.Play(Audio.ButtonQuit, AudioChannel.SFX3, true);
        System.Environment.Exit(1);
    }

    public override void _Process(double delta)
    {
    }
}
