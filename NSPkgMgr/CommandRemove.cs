using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSPkgMgr
{
    // Remove a package
    class CommandRemove : Command
    {
        public CommandRemove()
        {
            Name = "rm";
        }

        public override void Execute()
        {
            if(Args.Length == 0)
            {
                throw new CommandInvalidArgumentCountException(this);
            }

            string pkgName = Args[0];

            if(!PackageManager.IsPackageInstalled(pkgName))
            {
                Console.WriteLine("Package '{0}' is not installed", pkgName);
                return;
            }

            Console.WriteLine("Remove package {0}? [y|n]", pkgName);
            string answer = Console.ReadLine();
            bool remove = false;

            switch(answer)
            {
                case "y":
                case "yes":
                    remove = true;
                    break;
            }

            if(!remove)
            {
                Console.WriteLine("Aborting");
            }

            PackageManager.RemovePackage(pkgName);

            base.Execute();
        }
    }
}
