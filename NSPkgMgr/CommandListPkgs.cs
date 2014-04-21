using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSPkgMgr
{
    class CommandListPkgs : Command
    {
        public CommandListPkgs()
        {
            Name = "lspkg";
        }

        public override void Execute()
        {
            if(Args.Length != 0)
            {
                throw new CommandInvalidArgumentCountException(this);
            }

            foreach(Package pkg in PackageManager.packages)
            {
                Console.WriteLine(pkg.Name);
            }
            base.Execute();
        }
    }
}
