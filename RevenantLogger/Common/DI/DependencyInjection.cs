using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using static RosettaTools.Pwsh.Text.RevenantLogger.Common.StaticStrings;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common
{
    internal class DIContainer : RevenantLoggerBase
    {
        private DateTime _creationTime;
        private IServiceCollection? _services;
        private IServiceProvider? _diServiceProvider;
        private IHostBuilder? _genericHostBuilder;
        private ILoggingBuilder? _loggingBuilder;
        private IHost? _genericHost;
        private ILogger<DIContainer>? _logger;
        //private IRevenantConfiguration? _config;
        private PSVariable _existingDIContainerVariable = new(_PSVariableDIContainer, null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
        public EventHandler<PSWriteEventArgs> PSWriteMessage;
        private PSWriteEventArgs writeObject = new();

        public DateTime CreationTime { get => _creationTime; }
        protected internal IHostBuilder? GenericHostBuilder
        {
            get => _genericHostBuilder;
            private set => _genericHostBuilder = value;
        }

        protected internal ILoggingBuilder? LoggingBuilder
        {
            get => _loggingBuilder;
            private set => _loggingBuilder = value;
        }
        protected internal IHost? DGenericHost
        {
            get => _genericHost;
            private set => _genericHost = value;
        }
        protected internal IServiceCollection? Services
        {
            get => _services;
            private set => _services = value;
        }

        protected internal IServiceProvider DDIServiceProvider
        {
            get => _diServiceProvider;
            private set => _diServiceProvider = value;
        }
        protected internal ILoggerFactory? DSharedLoggerFactory
        {
            get => _sharedLoggerFactory;
            private set => _sharedLoggerFactory = value;
        }
        protected internal ILogger<DIContainer>? Logger
        {
            get => _logger;
            private set => _logger = value;
        }

        protected internal IRevenantConfiguration? Config
        {
            get
            {
                _config ??= new Configuration();
                return _config;
            }
            private set => _config = value;
        }
        protected internal PSVariable ExistingDIContainerVariable
        {
            get
            {
                _existingDIContainerVariable ??= new(_PSVariableDIContainer, null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
                return _existingDIContainerVariable;
            }
            private set => _existingDIContainerVariable = value;
        }


        //public DIContainer(SessionState sessionState)
        public DIContainer()
        {

        }

        // Necessary since we can't call the WriteVerbose/WriteInformation/Write* PS methods
        // directly unless we're in the PSCmdlet/running within the same thread. This allows
        // for the event handler running in the same thread to call the Write* methods.
        public DIContainer(EventHandler<PSWriteEventArgs> writeHandler)
        {
            PSWriteMessage += writeHandler;
        }

        public void BuildDIContainer()
        {
            if (!_buildFromCustomConfig && _genericHost != null)
            {
                writeObject.WriteMessage = "DIContainer already exists, and this is not a custom config. Returning.";
                PSWriteMessage(this, writeObject);
                return;
            }

            _genericHostBuilder = BuildAppHost();
            _genericHost = GenericHost = _genericHostBuilder.Build();
            _genericHost.RunAsync();
            _diServiceProvider = DIServiceProvider = _genericHost.Services;
            _config = RevenantConfig = DIServiceProvider.GetRequiredService<IRevenantConfiguration>();


            if (_config.LoggingConfig.UTC)
            {
                _creationTime = DateTime.UtcNow;
            }
            else
            {
                _creationTime = DateTime.Now;
            }
            _logger ??= SharedLoggerFactory?.CreateLogger<DIContainer>();
            AddToLoggersList<DIContainer>(_logger);

            writeObject.WriteMessage = "DIContainer created.";
            PSWriteMessage(this, writeObject);

            if (RevenantConfig.RunningConfig.ShowLogo)
            {
                Utilities.ShowLogo();
            }
        }

        private IHostBuilder BuildAppHost()
        {
            if (null == PSWriteMessage)
            {
                if (BuildFromCustomConfig)
                {
                    _config ??= new Configuration(CustomConfig);
                }
                else
                {
                    _config ??= new Configuration();
                }
            }
            else
            {
                if (BuildFromCustomConfig)
                {
                    _config ??= new Configuration(CustomConfig, PSWriteMessage);
                }
                else
                {
                    _config ??= new Configuration(PSWriteMessage);
                }
            }

            _sharedLoggerFactory = SharedLoggerFactory = Utilities.NewLoggerFactory(_config);

            string basePath = _config.ConfigHome;

            IHostBuilder hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(basePath);
                })
#if NET8_0_OR_GREATER
                .ConfigureHostOptions(options => {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(15);
                })
#endif
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IRevenantConfiguration, Configuration>();
                    services.AddSingleton<ILoggerFactory>(_sharedLoggerFactory);
                    services.AddSingleton<ILoggerProvider, FileLogProvider>();
                    services.AddSingleton<IFileLogProvider, FileLogProvider>();
                    services.AddSingleton<IRevenantFileLogger, RevenantFileLogger>();
                    services.AddSingleton<ILogger, RevenantFileLogger>();
                    //services.AddLogging();
                });

            return hostBuilder;
        }












        //private ILoggingBuilder NewLoggingBuilder()
        //{
        //    _config ??= new Configuration();
        //    return NewLoggingBuilder(_config);
        //}

        //private ILoggingBuilder NewLoggingBuilder(IRevenantConfiguration Config)
        //{
        //    LoggerFactory.Create(builder => {
        //        builder.ClearProviders();
        //        builder.AddSpectreConsole(config => {
        //            config.AddTemplateRenderers()
        //            .WriteInForeground()
        //            .ConfigureProfiles(profiles => {
        //                profiles.PreserveMarkupInFormatStrings = true;
        //                profiles.AddTypeStyle<SuccessMessage>("[green1]");
        //                profiles.AddTypeStyle<WarnMessage>("[yellow1]");
        //                profiles.AddTypeStyle<FailMessage>("[red1]");
        //            });
        //        });
        //        builder.AddProvider(new FileLogProvider(config: RevenantConfig));
        //        _loggingBuilder = builder;
        //    });

        //    return _loggingBuilder;
        //}

        //private IHostBuilder BuildAppHostOriginal()
        //{
        //    _services ??= new ServiceCollection();
        //    _config ??= new Configuration();
        //    _loggingBuilder ??= NewLoggingBuilder(_config);

        //    //ILoggerFactory internalFactory = _sharedLoggerFactory ?? sharedFactory ?? NewLoggerFactory(_config);

        //    string basePath = Directory.GetCurrentDirectory();

        //    IHostBuilder hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
        //        .ConfigureAppConfiguration(c => {
        //            c.SetBasePath(basePath);
        //        })
        //        .ConfigureHostOptions(options => {
        //            options.ShutdownTimeout = TimeSpan.FromSeconds(15);
        //        })
        //        .ConfigureLogging(builder => {
        //            builder.ClearProviders();
        //            builder = _loggingBuilder;
        //        })
        //        .ConfigureServices((context, services) => {
        //            services = _services;
        //        });

        //    //if (Config.FileLoggingEnabled && (null != Logger))
        //    //{
        //    //    hostBuilder.ConfigureServices((context, services) => {
        //    //        services.AddSingleton<ILogger>(Logger);
        //    //    });
        //    //}

        //    return hostBuilder;
        //}
        //private IServiceCollection NewServiceCollection()
        //{
        //    _services ??= new ServiceCollection();
        //    _services.AddSingleton<IRevenantConfiguration, Configuration>();

        //    _config ??= RevenantConfig ??= new Configuration();
        //    _loggingBuilder = NewLoggingBuilder(_config);
        //    _sharedLoggerFactory = Utilities.NewLoggerFactory(_config);

        //    _sharedLoggerFactory = Utilities.NewLoggerFactory(_config);
        //    _services.AddSingleton<ILoggerFactory>(_sharedLoggerFactory);
        //    _services.AddSingleton<IFileLogProvider, FileLogProvider>();
        //    _services.AddSingleton<ILoggerProvider, FileLogProvider>();
        //    _services.AddSingleton<IRevenantFileLogger, RevenantFileLogger>();
        //    _services.AddSingleton<ILogger, RevenantFileLogger>();

        //    _services.BuildServiceProvider();

        //    return _services;
        //}
    }
}
