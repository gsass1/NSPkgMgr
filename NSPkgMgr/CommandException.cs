using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSPkgMgr
{
    class CommandException : Exception
    {
        public Command Command { get; private set; }
        public CommandException(String message, Command command) : base(message)
        {
            Command = command;
        }
    }
}
