using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NTOSEmulator.Models
{
    public class DebugLine
    {
        public string Message { get; set; }

        [Browsable(false)]
        public bool IsError { get; set; } = false;
        public DebugLine(string message, bool isError = false)
        {
            Message = message;
            IsError = isError;
        }
    }
}
