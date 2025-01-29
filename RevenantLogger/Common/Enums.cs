﻿namespace RosettaTools.Pwsh.Text.RevenantLogger
{

    /// <summary>
    /// These directly map to the log levels in the Microsoft.Extensions.Logging namespace.
    /// They're just shortened for the file logger.
    /// </summary>
    public enum ShortLogLevel
    {
        trace = 0,
        debug = 1,
        info = 2,
        warn = 3,
        error = 4,
        crit = 5,
        none = 6,
    }

}
