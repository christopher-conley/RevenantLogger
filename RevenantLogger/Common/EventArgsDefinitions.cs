using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RosettaTools.Pwsh.Text.RevenantLogger.Interfaces;
using RosettaTools.Pwsh.Text.RevenantLogger.Helpers;
using RosettaTools.Pwsh.Text.RevenantLogger.Common;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common
{

    public class PSWriteEventArgs : EventArgs
    {
        public string? WriteMethod { get; set; }
        public string? WriteMessage { get; set; }
        public string? ErrorParamName { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorID { get; set; }
        public ErrorCategory? ErrorCategory { get; set; }
        public object? ErrorTargetObject { get; set; }
        public ErrorRecord? ErrorRecord { get; set; }

        public PSWriteEventArgs()
        {
            
        }
        public PSWriteEventArgs(string? writeMethod = "WriteVerbose", string? writeMessage = "")
        {
            WriteMethod = writeMethod;
            WriteMessage = writeMessage;
        }
    }
}
