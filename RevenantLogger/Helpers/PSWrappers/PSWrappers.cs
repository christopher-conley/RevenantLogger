using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Helpers
{

    internal static class PSWrappers
    {
        internal static void WriteError<TObject>(
            TObject senderObject,
            EventHandler<PSWriteEventArgs>? eventHandler,
            ErrorRecord errorRecord) where TObject : class
        {
            PSWriteEventArgs pSWriteEventArgs = new() {
                ErrorRecord = errorRecord
            };

            eventHandler?.Invoke(senderObject, pSWriteEventArgs);
        }

        internal static void WriteError<TObject>(
            TObject senderObject,
            EventHandler<PSWriteEventArgs>? eventHandler,
            string parameterName,
            string errorMessage,
            string errorID,
            ErrorCategory errorCategory = ErrorCategory.NotSpecified,
            object? targetObject = null) where TObject : class
        {
            ErrorRecord errorRecord = Utilities.NewPSError(parameterName, errorMessage, errorID, errorCategory, targetObject);

            PSWriteEventArgs psWriteEventArgs = new("WriteError", errorMessage) {
                WriteMethod = "WriteError",
                WriteMessage = errorMessage,
                ErrorParamName = parameterName,
                ErrorMessage = errorMessage,
                ErrorID = errorID,
                ErrorCategory = errorCategory,
                ErrorTargetObject = targetObject,
                ErrorRecord = errorRecord
            };

            eventHandler?.Invoke(senderObject, psWriteEventArgs);
        }
        internal static void WriteVerbose<TObject>(TObject senderObject, EventHandler<PSWriteEventArgs>? eventHandler, string message) where TObject : class
        {
            PSWriteEventArgs psWriteEventArgs = new("WriteVerbose", message);
            eventHandler?.Invoke(senderObject, psWriteEventArgs);
        }

        internal static void WriteWarning<TObject>(TObject senderObject, EventHandler<PSWriteEventArgs>? eventHandler, string message) where TObject : class
        {
            PSWriteEventArgs psWriteEventArgs = new("WriteWarning", message);
            eventHandler?.Invoke(senderObject, psWriteEventArgs);
        }
    }
}
