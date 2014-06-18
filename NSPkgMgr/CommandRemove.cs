/*
===============================================================================
    NSPkgMgr
    Copyright (C) 2014  Gian Sass

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
===============================================================================
 */

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
        bool force = false;

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

            if (Args.Length > 1 && Args[1] == "-y")
            {
                force = true;
            }

            string pkgName = Args[0];

            if(!PackageManager.IsPackageInstalled(pkgName))
            {
                Console.WriteLine("Package '{0}' is not installed", pkgName);
                return;
            }

            if (!force)
            {
                Console.WriteLine("Remove package {0}? [y|n]", pkgName);
                string answer = Console.ReadLine();
                bool remove = false;

                switch (answer)
                {
                    case "y":
                    case "yes":
                        remove = true;
                        break;
                }

                if (!remove)
                {
                    Console.WriteLine("Aborting");
                    return;
                }
            }

            PackageManager.RemovePackage(pkgName);

            base.Execute();
        }
    }
}
