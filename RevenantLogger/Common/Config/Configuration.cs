using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Helpers
{
    public class Configuration : IRevenantConfiguration
    {

        private readonly ILogger<Configuration>? _logger;
        private static DateTime _creationTime;
        private string _appDataDir;
        private string _configHome = string.Empty;
        private string _defaultConfigFilename = "revenantlogger.config.json";
        private string _defaultConfigFile;
        private string _logPath = string.Empty;
        private ConfigDefinition.ConfigRoot _defaultConfig;
        private ConfigDefinition.ConfigRoot _runningConfig;
        private bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private string? _customConfig;
        private string _runningConfigFile;
        private bool _isCustomConfigFile = false;
        private string _os;
        public EventHandler<PSWriteEventArgs>? PSWriteMessage;

        public DateTime CreationTime { get => _creationTime; }

#pragma warning disable CA1416
        public string AppDataDir
        {
            get
            {
                if (null == _appDataDir)
                {
                    if (_isLinux)
                    {
                        _appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                        if (Directory.Exists(_appDataDir) == false)
                        {
#if NET8_0_OR_GREATER
                            Directory.CreateDirectory(_appDataDir,
                                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                                UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                                );
#else
                            Directory.CreateDirectory(_appDataDir);
#endif
                        }
                        return _appDataDir;
                    }

                    if (null == Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                    {
                        if (_isWindows)
                        {
                            _appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming");
                            if (Directory.Exists(_appDataDir) == false)
                            {
                                Directory.CreateDirectory(_appDataDir);
                            }
                        }
                        else
                        {
                            throw new PlatformNotSupportedException($"The current platform: \"{RuntimeInformation.OSDescription}\" is not supported.");
                        }
                    }
                    else
                    {
                        _appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    }
                }

                return _appDataDir;
            }
        }
        public ConfigDefinition.ConfigRoot DefaultConfig
        {
            get
            {
                _defaultConfig ??= GetDefaultConfig();
                return _defaultConfig;
            }
            private set { _defaultConfig = value; }
        }

        public ConfigDefinition.ConfigRoot RunningConfig
        {
            get
            {
                _runningConfig ??= GetDefaultConfig();
                return _runningConfig;
            }
            set { _defaultConfig = value; }
        }

        public ConfigDefinition.LoggingRoot LoggingConfig
        {
            get => _runningConfig.Logging;
        }

        public ConfigDefinition.LoggingColorRoot Colors
        {
            get => _runningConfig.Logging.Colors;
        }

        public string ConfigHome
        {
            get => _configHome;
            private set => _configHome = value;
        }

        public string DefaultConfigFilename
        {
            get => _defaultConfigFilename;
            private set => _defaultConfigFilename = value;
        }

        public string DefaultConfigFile
        {
            get => _defaultConfigFile;
            private set => _defaultConfigFile = value;
        }

        public string RunningConfigFile
        {
            get => _runningConfigFile;
            private set => _runningConfigFile = value;
        }

        public bool FileLoggingEnabled
        {
            get => _runningConfig.Logging.Enabled;
            set => _runningConfig.Logging.Enabled = value;
        }
        public string LogPath
        {
            get => _logPath;
            set => _logPath = value;
        }

        public bool IsWindows
        {
            get => _isWindows;
            private set => _isWindows = value;
        }

        public bool IsLinux
        {
            get => _isLinux;
            private set => _isLinux = value;
        }

        public bool IsCustomConfig
        {
            get => _isCustomConfigFile;
            private set => _isCustomConfigFile = value;
        }

        public string OS
        {
            get => _os;
            private set => _os = value;
        }

        protected internal ILogger<Configuration>? Logger
        {
            get => _logger;
        }

        public Configuration(ILogger<Configuration> logger, EventHandler<PSWriteEventArgs>? writeHandler = null)
        {
            _logger = logger;
            PSWriteMessage = writeHandler;
            Init();
        }

        public Configuration()
        {
            Init();
        }

        public Configuration(string customConfig)
        {
            _customConfig = customConfig;
            _isCustomConfigFile = true;
            Init();
        }

        public Configuration(EventHandler<PSWriteEventArgs>? writeHandler = null)
        {
            PSWriteMessage = writeHandler;
            Init();
        }

        public Configuration(ILogger<Configuration> logger, string customConfig, EventHandler<PSWriteEventArgs>? writeHandler = null)
        {
            PSWriteMessage = writeHandler;
            _logger = logger;
            _customConfig = customConfig;
            _isCustomConfigFile = true;
            Init();
        }

        public Configuration(string customConfig, EventHandler<PSWriteEventArgs>? writeHandler = null)
        {
            PSWriteMessage = writeHandler;
            _customConfig = customConfig;
            _isCustomConfigFile = true;
            Init();
        }

        private void Init()
        {
            PSWrappers.WriteVerbose(this, PSWriteMessage, "Initializing configuration");
            _defaultConfig = GetDefaultConfig();
            _runningConfig ??= GetDefaultConfig();

            _os = _isWindows ? "Windows" : _isLinux ? "Linux" : "Unknown";

            // per-directory logging settings support if dotfile of config filename exists
            if (File.Exists($".{_defaultConfigFilename}") && !_isCustomConfigFile)
            {
                PSWrappers.WriteVerbose(this, PSWriteMessage, "Using config file found in the current directory");
                _configHome = Directory.GetCurrentDirectory();
                _runningConfigFile = Path.Combine(_configHome, _defaultConfigFilename);
            }
            else
            {
                _configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? AppDataDir;
                _configHome = Path.Combine(
                [
                    _configHome,
                        "rosettatools",
                        "pwsh",
                        "text",
                        "revenantlogger"
                ]);
            }
            _defaultConfigFile = Path.Combine(_configHome, _defaultConfigFilename);
            _runningConfigFile = _defaultConfigFile;
#pragma warning disable CA1416 // Validate platform compatibility

            // Yeah, this looks dumb, but it's a whole hell of a lot easier to read
            // in this context than just slapping a negation operator at the front
            if (Directory.Exists(_configHome) == false)
            {
                PSWrappers.WriteVerbose(this, PSWriteMessage, $"Default config directory {_configHome} does not exist, creating it");
                if (_isLinux)
                {
#if NET8_0_OR_GREATER
                    Directory.CreateDirectory(_configHome,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                        );
#else
                    Directory.CreateDirectory(_configHome);
#endif
                }
                else
                {
                    Directory.CreateDirectory(_configHome);
                }
            }
#pragma warning restore CA1416 // Validate platform compatibility
            LoadConfig();

            if (String.IsNullOrWhiteSpace(_runningConfig.Logging.DateFormat))
            {
                _runningConfig.Logging.DateFormat = "yyyy-MM-dd";
            }
            else
            {
                try
                {
                    DateTime.Now.ToString(_runningConfig.Logging.DateFormat);
                }
                catch (Exception)
                {
                    _runningConfig.Logging.DateFormat = "yyyy-MM-dd";
                    PSWrappers.WriteWarning(
                        this,
                        PSWriteMessage,
                        $"The date format \"{_runningConfig.Logging.DateFormat}\" provided in the config file is an invalid date format, using default value instead"
                        );
                }
            }

            if (String.IsNullOrWhiteSpace(_runningConfig.Logging.TimeFormat))
            {
                _runningConfig.Logging.TimeFormat = "HH:mm:ss.fffK";
            }
            else
            {
                try
                {
                    DateTime.Now.ToString(_runningConfig.Logging.TimeFormat);
                }
                catch (Exception)
                {
                    _runningConfig.Logging.TimeFormat = "HH:mm:ss.fffK";
                    PSWrappers.WriteWarning(
                        this,
                        PSWriteMessage,
                        $"The time format \"{_runningConfig.Logging.TimeFormat}\" provided in the config file is an invalid time format, using default value instead"
                        );
                }
            }

            if (null == _runningConfig.Logging.DateTimeSeperator)
            {
                _runningConfig.Logging.DateTimeSeperator = "";
            }

            _logPath = Path.Combine(_configHome, _runningConfig.Logging.LogDirectory);

            if (_runningConfig.Logging.UTC)
            {
                _creationTime = DateTime.UtcNow;
            }
            else
            {
                _creationTime = DateTime.Now;
            }
        }

        public void LoadConfig()
        {
            if (File.Exists(_defaultConfigFile) == false)
            {
                PSWrappers.WriteVerbose(this, PSWriteMessage, "Default configuration file does not exist, creating it");
                SaveConfig();
            }

            string configToLoad = String.Empty;

            if (IsCustomConfig)
            {
                PSWrappers.WriteVerbose(this, PSWriteMessage, $"Using custom configuration file at: {_customConfig}");
                if (String.IsNullOrWhiteSpace(_customConfig))
                {
                    PSWrappers.WriteError(
                        this,
                        PSWriteMessage,
                        "Config", "Parameter -Config is null", "ConfigIsNull", ErrorCategory.InvalidArgument, this
                        );
                    throw new ArgumentNullException(nameof(_customConfig), "Custom configuration file path is null or empty");
                }

                if (File.Exists(_customConfig) == false)
                {
                    PSWrappers.WriteError(
                        this,
                        PSWriteMessage,
                        "Config", $"Config file {_customConfig} is inaccessible or does not exist.", "ConfigUnreadable", ErrorCategory.ObjectNotFound, this
                        );
                    throw new FileNotFoundException("Custom configuration file does not exist", _customConfig);
                }

                configToLoad = _customConfig;
            }
            else
            {
                configToLoad = _defaultConfigFile;
            }

            PSWrappers.WriteVerbose(this, PSWriteMessage, $"Loading configuration file from disk at path: {configToLoad}");
            try
            {
                string json = File.ReadAllText(configToLoad);
                _runningConfig = JsonConvert.DeserializeObject<ConfigDefinition.ConfigRoot>(json) ?? GetDefaultConfig();
                _runningConfigFile = configToLoad ?? _defaultConfigFile;
            }
            catch (Exception ex)
            {
                PSWrappers.WriteError(
                    this,
                    PSWriteMessage,
                    "Config", $"Failed to load configuration file from disk at path {configToLoad}: {ex.Message}", "ConfigUnreadable", ErrorCategory.ReadError, this
                    );
                throw;
            }
        }

        public void SaveConfig()
        {
            SaveConfig(_runningConfig, _defaultConfigFile);
        }

        public void SaveConfig(ConfigDefinition.ConfigRoot _incomingConfig)
        {
            SaveConfig(_incomingConfig, _defaultConfigFile);
        }

        public void SaveConfig(string _savePath)
        {
            SaveConfig(_runningConfig, _savePath);
        }

        public void SaveConfig(ConfigDefinition.ConfigRoot _incomingConfig, string _savePath)
        {
            PSWrappers.WriteVerbose(this, PSWriteMessage, $"Writing configuration file to disk at path: {_savePath}");
            string json = JsonConvert.SerializeObject(_incomingConfig, Formatting.Indented);
            try
            {
                File.WriteAllText(_savePath, json);
            }
            catch (Exception ex)
            {
                PSWrappers.WriteError(
                    this,
                    PSWriteMessage,
                    "Config", $"Failed to write configuration file to disk: {ex.Message}", "ConfigWriteError", ErrorCategory.WriteError, this
                    );
                throw;
            }
        }

        public ConfigDefinition.ConfigRoot GetDefaultConfig()
        {

            return new ConfigDefinition.ConfigRoot
            {
                Logging = new ConfigDefinition.LoggingRoot
                {
                    Colors = new ConfigDefinition.LoggingColorRoot()
                }
            };

            //return new ConfigDefinition.ConfigRoot {
            //    ShowLogo = false,
            //    CheckForUpdates = true,
            //    Logging = new ConfigDefinition.LoggingRoot {
            //        Enabled = true,
            //        LogDirectory = "logs",
            //        LogFilename = "revenantlogger.log",
            //        DateFormat = "yyyy-MM-dd",
            //        TimeFormat = "HH:mm:ss.ffffK",
            //        DateTimeSeperator = "T",
            //        UTC = false,
            //        MinimumLogLevel = "Information",
            //        Colors = new ConfigDefinition.LoggingColorRoot {
            //            Timestamp = "dim cyan",
            //            TimestampSeperator = "dim grey"
            //        }
            //    }
            //};
        }
    }
}
