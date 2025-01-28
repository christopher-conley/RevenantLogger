using Microsoft.Extensions.Logging;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common
{
    public abstract class StaticLoggerBase
    {
        protected static ILogger Logger
        {
            get; private set;
        }
        public static void InitializeLogger(ILoggerFactory factory)
        {
            Logger = factory.CreateLogger(typeof(StaticLoggerBase));
        }
    }
}
