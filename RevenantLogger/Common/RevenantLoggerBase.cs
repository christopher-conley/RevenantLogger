using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Spectre.Console;
using System.Dynamic;

namespace RosettaTools.Pwsh.Text.RevenantLogger
{
    public abstract class RevenantLoggerBase : PSCmdlet
    {

        private static Dictionary<string, ILogger?>? _iloggersList;
        private static Dictionary<string, Guid> _userLoggersNameGUID;
        private static Dictionary<Guid, UserLogger> _userLoggersGUIDLogger;
        private static List<IDictionary<string, object>> _userCustomLoggers = [];
        private protected static ILoggerFactory _sharedLoggerFactory;
        private protected ILogger? _cmdletLogger;
        internal protected static string? _customConfig;
        internal protected static bool _buildFromCustomConfig = false;
        internal protected static IRevenantConfiguration _config;

        public static Dictionary<string, Guid> UserLoggersNG
        {
            get
            {
                _userLoggersNameGUID ??= new Dictionary<string, Guid>();
                return _userLoggersNameGUID;
            }
            set => _userLoggersNameGUID = value;
        }

        public static Dictionary<Guid, UserLogger> UserLoggersGU
        {
            get
            {
                _userLoggersGUIDLogger ??= new Dictionary<Guid, UserLogger>();
                return _userLoggersGUIDLogger;
            }
            set => _userLoggersGUIDLogger = value;
        }

        public static IDictionary<string, object>[] UserCustomLoggers
        {
            get
            {
                return _userCustomLoggers.ToArray();
            }
        }
        internal static DIContainer? CmdletDIContainer
        {
            get; set;
        }

        internal static IServiceProvider? DIServiceProvider
        {
            get; set;
        }
        internal static IHost? GenericHost
        {
            get; set;
        }

        internal static ILoggerFactory SharedLoggerFactory
        {
            get => _sharedLoggerFactory;
            set => _sharedLoggerFactory = value;
        }

        protected internal ILogger? CmdletLogger
        {
            get; set;
        }

        public static Dictionary<string, ILogger?>? ILoggersList
        {
            get
            {
                _iloggersList ??= [];
                return _iloggersList;
            }
            set => _iloggersList = value;
        }
        //internal static Bootstrap? BaseBootstrap
        //{
        //    get; set;
        //}

        internal static IRevenantConfiguration RevenantConfig
        {
            get => _config;
            set => _config = value;
        }

        internal static ConfigDefinition.LoggingColorRoot ColorConfig
        {
            get => RevenantConfig.Colors ?? new ConfigDefinition.LoggingColorRoot();
        }

        internal static string DateTimeSep
        {
            get => RevenantConfig.LoggingConfig.DateTimeSeperator ?? "";
        }

        internal static Exception LastException
        {
            get; set;
        }

        public static bool BuildFromCustomConfig { get => _buildFromCustomConfig; }
        public static string? CustomConfig { get => _customConfig; }

        public static Dictionary<LogLevel, string> LogLevelColors
        {
            get
            {
                return new Dictionary<LogLevel, string> {
                    { LogLevel.Trace, $"[{ColorConfig.LevelTrace}]" },
                    { LogLevel.Debug, $"[{ColorConfig.LevelDebug}]" },
                    { LogLevel.Information, $"[{ColorConfig.LevelInformation}]" },
                    { LogLevel.Warning, $"[{ColorConfig.LevelWarning}]" },
                    { LogLevel.Error, $"[{ColorConfig.LevelError}]" },
                    { LogLevel.Critical, $"[{ColorConfig.LevelCritical}]" }
                };
            }
        }

        private protected RevenantLoggerBase()
        {
            //string modulePath = Path.GetDirectoryName(typeof(RevenantLoggerBase).Assembly.Location);
            //var loadContext = new CustomAssemblyLoadContext(modulePath);
            init();
        }

        private protected void init()
        {
            ;
        }

        protected internal void InitDIContainer<TCmdlet>([CallerMemberName] string? caller = null) where TCmdlet : class
        {
            WriteVerbose("Inside InitDIContainer");
            if (null == CmdletDIContainer && null == GenericHost)
            {
                if (null == this.SessionState)
                {
                    //CmdletDIContainer = new DIContainer(sessionState: new SessionState());

                    //CmdletDIContainer = new DIContainer();
                    //CmdletDIContainer.PSWriteMessage += WritePSMessage;

                    CmdletDIContainer = new DIContainer(WritePSMessage);
                    CmdletDIContainer.BuildDIContainer();
                }
                else
                {
                    //CmdletDIContainer = new DIContainer(sessionState: this.SessionState);
                    //CmdletDIContainer = new DIContainer();

                    CmdletDIContainer = new DIContainer(WritePSMessage);
                    CmdletDIContainer.BuildDIContainer();
                }
            }

            _sharedLoggerFactory = SharedLoggerFactory ?? GetDIService<ILoggerFactory>(required: false);

            _cmdletLogger = GetExistingLogger<TCmdlet>();
            if (null == _cmdletLogger)
            {
                _cmdletLogger = _sharedLoggerFactory?.CreateLogger<TCmdlet>();
                AddToLoggersList<TCmdlet>(_cmdletLogger);
            }

            _config = RevenantConfig ?? new Configuration();

            CmdletLogger?.BeginScope(caller ?? "Unknown");
            CmdletLogger?.RLogDebug($"{caller ?? "Unknown:"} bootstrapping complete");
        }

        protected internal static TService? GetDIService<TService>(bool required = false) where TService : class
        {
            if (required)
            {
                return CmdletDIContainer?.DDIServiceProvider.GetRequiredService<TService>();
            }
            else
            {
                return CmdletDIContainer?.DDIServiceProvider.GetService<TService>();
            }
            //return CmdletDIContainer.DDIServiceProvider.GetService<TService>();
        }

        protected internal static bool IsValidMarkup(string testMessage = "")
        {
            Markup testMarkup;

            try
            {
                testMarkup = new Markup(testMessage.ToString());
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return false;
            }
        }

        protected internal static TObject? GetExistingPSVariable<TObject>(SessionState SessionState, string psVariable) where TObject : class
        {
            //var existingObject = SessionState.PSVariable.Get(psVariable);
            TObject? existingObject = (TObject?)SessionState.PSVariable.GetValue(psVariable, null);

            return (null == existingObject) ? null : existingObject;
            //if (null == existingObject) {
            //    return null;
            //}
            //return (TObject)existingObject;
        }

        protected internal static ILogger? GetExistingLogger(Type loggerType)
        {
            if (null == ILoggersList)
            {
                return null;
            }

            string loggerKey = loggerType.ToString();
            return GetExistingLogger(loggerKey);
        }

        protected internal static ILogger? GetExistingLogger<TLoggerType>() where TLoggerType : class
        {
            return GetExistingLogger(typeof(TLoggerType).Name.ToString());
        }
        protected internal static ILogger? GetExistingLogger(string loggerType)
        {
            if (null == ILoggersList)
            {
                return null;
            }

            if (ILoggersList.TryGetValue(loggerType, out ILogger? existingLogger) == false)
            {
                return null;
            }
            return existingLogger;
        }

        protected internal static void AddToCustomLoggers(string name, UserLogger userLogger, string? guid = null)
        {
            Guid parsedGuid;
            if (null == guid)
            {
                parsedGuid = Guid.NewGuid();
            }
            else
            {
                parsedGuid = Guid.Parse(guid);
            }

            AddToCustomLoggers(name, userLogger, parsedGuid);
        }
        protected internal static void AddToCustomLoggers(string name, UserLogger userLogger, Guid? guid = null)
        {
            Guid loggerGuid;

            if (null == guid)
            {
                loggerGuid = Guid.NewGuid();
            }
            else
            {
                loggerGuid = (Guid)guid;
            }

            UserLoggersNG.Add(name, loggerGuid);
            UserLoggersGU.Add(loggerGuid, userLogger);

            var listLogger = new ExpandoObject() as IDictionary<string, object>;
            listLogger.Add("Name", name);
            listLogger.Add("GUID", loggerGuid);
            listLogger.Add("Logger", userLogger);

            _userCustomLoggers.Add(listLogger);
        }

        protected internal static void AddToLoggersList(Type loggerType, ILogger? logger)
        {
            if (null == logger)
            {
                return;
            }
            AddToLoggersList(loggerType.ToString(), logger);
        }
        protected internal static void AddToLoggersList<TLoggerType>(ILogger? logger) where TLoggerType : class
        {
            if (null == logger)
            {
                return;
            }
            AddToLoggersList(typeof(TLoggerType).Name.ToString(), logger);
        }

        protected internal static void AddToLoggersList(string loggerType, ILogger? logger)
        {
            if (null == ILoggersList)
            {
                ILoggersList = [];
            }

            if (null == logger)
            {
                return;
            }
#if NET8_0_OR_GREATER
            if (ILoggersList.TryAdd(loggerType, logger) == false)
#else
            if (ILoggersList.ContainsKey(loggerType) == false)
#endif
            {
                ILoggersList[loggerType] = logger;
                return;
            }
        }

        protected internal static bool ExistsInPath(string? fileName = null)
        {
            return GetFullPath(fileName) != null;
        }

        protected internal static string? GetFullPath(string? fileName = null)
        {
            if (null == fileName)
            {
                return null;
            }

            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }

            string? values = Environment.GetEnvironmentVariable("PATH");

            foreach (string? path in values?.Split(Path.PathSeparator))
            {
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }

        protected internal void ValidateOrThrowConfigParam(PSObject? configParam)
        {
            if (null == configParam || null == configParam.BaseObject)
            {
                var terminatingError = Utilities.NewPSError("Config", "Parameter -Config is null", "ConfigIsNull", ErrorCategory.InvalidArgument, configParam);
                WriteError(terminatingError);
                throw terminatingError.Exception;
            }
            else
            {
                Type? configBaseType = configParam.BaseObject.GetType();

                switch (configBaseType)
                {
                    case Type _ when configBaseType == typeof(string):
                        _buildFromCustomConfig = true;
                        _customConfig = configParam.BaseObject.ToString();
                        break;

                    case Type _ when configBaseType == typeof(FileInfo):
                        _buildFromCustomConfig = true;
                        _customConfig = ((FileInfo)configParam.BaseObject).FullName.ToString();
                        break;

                    default:
                        var terminatingError = Utilities.NewPSError("Config", "The type of value provided to -Config is invalid, please provide a string or FileInfo object.", "ConfigIsInvalid", ErrorCategory.InvalidArgument, configParam);
                        WriteError(terminatingError);
                        throw terminatingError.Exception;
                }
            }

            return;
        }

        internal void WritePSMessage(object? sender, PSWriteEventArgs e)
        {
            switch (e.WriteMethod)
            {
                case "WriteVerbose":
                    WriteVerbose(e.WriteMessage);
                    break;
                case "WriteDebug":
                    WriteDebug(e.WriteMessage);
                    break;
                case "WriteWarning":
                    WriteWarning(e.WriteMessage);
                    break;
                case "WriteError":
                    WriteError(e.ErrorRecord);
                    break;
                //case "WriteInformation":
                //    WriteInformation(e.WriteMessage);
                //    break;
                //case "WriteProgress":
                //    WriteProgress(e.WriteMessage);
                //    break;
                case "WriteObject":
                    WriteObject(e.WriteMessage);
                    break;
                default:
                    WriteVerbose(e.WriteMessage);
                    break;
            }
        }

        //private protected void SetMarkupOperators() {
        //    List<string> operators = new();
        //    foreach (string code in ANSIMap.Keys) {
        //        operators.Add(code);
        //    }
        //    MarkupOperators = operators.ToArray();
        //}
    }
}
