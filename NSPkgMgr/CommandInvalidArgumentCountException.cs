using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSPkgMgr
{
    class CommandInvalidArgumentCountException : CommandException
    {
        public CommandInvalidArgumentCountException(Command command) : base("Invalid argument count", command)
        {
        }
    }
}
