using Godot;

public partial class Config : Node
{
	private const string CONFIG_FILE_LOCATION = "user://config.cfg";
	private const string DEFAULT_CONFIG_FILE_LOCATION = "res://default_config.cfg";

	private static ConfigFile configFile;

	private static void _LoadConfigFile()
    {
		if (configFile != null)
			return;
        configFile = new ConfigFile();
		var loadError = configFile.Load(CONFIG_FILE_LOCATION);
		if (loadError != Error.Ok)
		{
			var loadDefaultError = configFile.Load(DEFAULT_CONFIG_FILE_LOCATION);
			if (loadDefaultError != Error.Ok)
			{
				GD.PrintErr($"Load default configuration file failed with error: {loadDefaultError}");
			}
			_SaveConfigFile();
        }
	}

	private static void _SaveConfigFile()
	{
		var saveError = configFile.Save(CONFIG_FILE_LOCATION);
		if (saveError != Error.Ok)
        {
            GD.PrintErr($"Save config file failed with error: {saveError}");
        }
	}

	public static void SetConfig(string section, string key, Variant value)
	{
		_LoadConfigFile();
		configFile.SetValue(section, key, value);
		_SaveConfigFile();
	}

	public static Variant GetConfig(string section, string key, Variant defaultVal)
	{
		_LoadConfigFile();
		return configFile.GetValue(section, key, defaultVal);
	}

	public override void _Ready()
	{
        _LoadConfigFile();
    }

	public override void _Process(double delta)
	{
	}
}
