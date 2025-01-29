﻿using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;
using System.Reflection;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "RevenantLogger", DefaultParameterSetName = "default")]
    //[OutputType(typeof(ILogger))]
    //[OutputType(typeof(CmdNewRevenantLogger))]
    [OutputType(typeof(void))]
    [OutputType(type: typeof(PSObject), ParameterSetName = ["RawLogger"])]
    public class CmdNewRevenantLogger : RevenantLoggerPSCmdlet
    {
        private UserCustomLogger _oldreturnObject;
        private PSObject _returnObject;

        [Parameter(Mandatory = false)]
        [Alias("Configuration", "ConfigFile")]
#if NET8_0_OR_GREATER
        [ValidateNotNullOrWhiteSpace()]
#else
        [ValidateNotNullOrEmpty()]
#endif
        [ValidateString(minLength: 2)]
        //[ValidateTypes(typeof(string), typeof(PSCustomObject))]
        [ValidateTypes(typeof(string), typeof(FileInfo))]
        public PSObject Config
        {
            get;
            set;
        }

        [Parameter(Mandatory = false)]
        [Alias("Create-RevenantLogger")]
#if NET8_0_OR_GREATER
        [ValidateNotNullOrWhiteSpace()]
#else
        [ValidateNotNullOrEmpty()]
#endif
        [ValidateString(minLength: 1, allowNull: false, allowWhitespace: false)]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "default")]
        [Parameter(Mandatory = true, ParameterSetName = "RawLogger")]
        public SwitchParameter ReturnRaw
        {
            get;
            set;
        }


        public new ILogger? CmdletLogger { get => _cmdletLogger; }

        public Dictionary<string, ILogger?>? BuiltLoggers
        {
            get;
            private set;
        }
        public CmdNewRevenantLogger()
        {

        }

        public CmdNewRevenantLogger(ILogger<CmdNewRevenantLogger> logger, IRevenantConfiguration config)
        {
            _config = config;
            _cmdletLogger = logger;
            BeginProcessing();
        }

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            base.init();
            string methodName = MethodBase.GetCurrentMethod().Name;

            InitDIContainer<CmdEditRevenantLoggerConfig>();

            //BaseBootstrap = GetExistingPSVariable<Bootstrap>(SessionState: this.SessionState, psVariable: "__RevenantLoggerExistingBootstrap");
            //BaseBootstrap ??= new Bootstrap(SessionState: this.SessionState);

            CmdletLogger?.BeginScope(methodName);
            CmdletLogger?.RLogDebug("{success}: {methodName}(): Inside New-RevenantLogger BeginProcessing", caller: methodName, args: SuccessMessage.Value);
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            CmdletLogger?.BeginScope("ProcessRecord");
            CmdletLogger?.RLogDebug("Inside New-RevenantLogger ProcessRecord");
            Guid loggerGUID = Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = $"Revenant-{loggerGUID}";
            }
            ILogger userLogger = SharedLoggerFactory.CreateLogger(Name);
            UserLogger loggerObject = new UserLogger(Name, userLogger, loggerGUID);
            AddToLoggersList(Name, userLogger);
            AddToCustomLoggers(Name, loggerObject, loggerGUID);

            if (ReturnRaw)
            {
                _returnObject = new PSObject(loggerObject);
            }

        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            CmdletLogger?.BeginScope("EndProcessing");
            CmdletLogger?.RLogDebug("Inside New-RevenantLogger EndProcessing");

            //_oldreturnObject = new UserCustomLogger(BaseBootstrap.Logger);

            BuiltLoggers = ILoggersList;

            if (ReturnRaw)
            {
                //var userLoggerObject = new ExpandoObject() as IDictionary<string, object?>;
                //userLoggerObject.Add("Name", Name);
                //userLoggerObject.Add("CreationTime", null);
                //if (RevenantConfig.LoggingConfig.UTC)
                //{
                //    userLoggerObject["CreationTime"] = DateTime.UtcNow;
                //}
                //else
                //{
                //    userLoggerObject["CreationTime"] = DateTime.Now;
                //}

                //userLoggerObject.Add("Logger", new UserCustomLogger(ILoggersList[Name]));
                //_returnObject = new PSObject(userLoggerObject);
                WriteObject(_returnObject);
            }
        }
    }
}
