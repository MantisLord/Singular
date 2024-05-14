using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class MainMenu : Control
{
	Game game;
	CheckButton fullscreenCheckButton;
	OptionButton resolutionOptionsButton;
    readonly Vector2I[] resolutionsArray =
	{
		new(640, 360),
        new(1024, 576),
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

    enum ConfigSections
	{
		Video
	}
	enum ConfigKeys
	{
		FullscreenEnabled,
		ScreenResolution,
	}

    public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
		menuContainer = GetNode<PanelContainer>("MainContainer/MenuContainer");
        optionsContainer = GetNode<PanelContainer>("MainContainer/OptionsContainer");
		fullscreenCheckButton = optionsContainer.GetNode<CheckButton>("VBoxContainer/HBoxContainer2/FullscreenCheckButton");
		resolutionOptionsButton = optionsContainer.GetNode<OptionButton>("VBoxContainer/HBoxContainer/ResolutionOptionButton");
        userResolutionsArray = (Vector2I[])resolutionsArray.Clone();
        Window win = GetWindow();
		LoadSettingsFromConfig(win);
        UpdateUI(win);
        win.SizeChanged += () => PreselectResolution(win);
    }

	private void LoadSettingsFromConfig(Window win)
	{
		var fullscreenEnabled = Config.GetConfig(ConfigSections.Video.ToString(), ConfigKeys.FullscreenEnabled.ToString(), false);
        SetFullscreenEnabled((bool)fullscreenEnabled, win);
        var screenResolution = Config.GetConfig(ConfigSections.Video.ToString(), ConfigKeys.ScreenResolution.ToString(), new Vector2I(640, 360));
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

	private void PlayButtonPressed()
	{
		game.ChangeScene("world");
	}

    private void OptionsButtonPressed()
	{
		optionsContainer.Visible = true;
		menuContainer.Visible = false;
    }

    private void BackButtonPressed()
	{
        optionsContainer.Visible = false;
        menuContainer.Visible = true;
    }

    private static void ExitButtonPressed()
	{
		System.Environment.Exit(1);
	}

	public override void _Process(double delta)
	{
	}
}
