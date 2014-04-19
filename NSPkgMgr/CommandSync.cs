using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSPkgMgr
{
    class CommandSync : Command
    {
        public CommandSync()
        {
            Name = "sync";
        }

        public override void Execute()
        {
            PackageManager.UpdatePackageList();
            base.Execute();
        }
    }
}
