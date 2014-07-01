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
    /// <summary>
    /// Remove a package
    /// </summary>
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

            List<Package> pkgsToRemove = new List<Package>();

            foreach (string pkgName in Args)
            {
                if(pkgName == "-y")
                {
                    force = true;
                    continue;
                }

                if (!PackageManager.IsPackageInstalled(pkgName))
                {
                    Console.WriteLine("Package '{0}' is not installed", pkgName);
                    return;
                }
                else
                {
                    Package package = PackageManager.GetPackageFromList(pkgName);
                    if(!pkgsToRemove.Contains(package))
                    {
                        pkgsToRemove.Add(package);
                    }
                }
            }

            if (pkgsToRemove.Count != 0)
            {
                PromptRemovePackages(pkgsToRemove);
            }

            base.Execute();
        }

        void PromptRemovePackages(List<Package> packages)
        {
            // Count the total size of the packages and add all the names to a string
            int sizeTotal = 0;
            StringBuilder builder = new StringBuilder();
            foreach (Package package in packages)
            {
                sizeTotal += package.Size;
                builder.Append(package.Name + " ");
            }

            if (!force)
            {
                Console.WriteLine("Remove package(s) {0}, size: {1}KB? [y|n]", builder.ToString(), sizeTotal);

                string answer = Console.ReadLine();

                bool remove = false;

                switch (answer)
                {
                    case "":
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

            // Remove everything
            foreach (Package package in packages)
            {
                PackageManager.RemovePackage(package);
            }
        }
    }
}
