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
    /// Command for installing a package
    /// </summary>
    class CommandGet : Command
    {
        bool force = false;

        public CommandGet()
        {
            Name = "get";
        }

        public override void Execute()
        {
            // We need more than that
            if(Args.Length == 0)
            {
                throw new CommandInvalidArgumentCountException(this);
            }

            if(Args.Length > 1 && Args[1] == "-y")
            {
                force = true;
            }

            // Check if the package exists
            string packagename = Args[0];
            if(!PackageManager.CheckPackageExists(packagename))
            {
                Console.WriteLine("Could not find package '{0}'", packagename);
                return;
            }

            // Don't install if it already is installed
            if(PackageManager.IsPackageInstalled(packagename))
            {
                Console.WriteLine("'{0}' is already installed", packagename);
                return;
            }

            Package package = PackageManager.GetPackageFromList(packagename);

            // Packages which will be installed
            List<Package> pkgsToInstall = new List<Package>();

            // Add dependencies if there are any
            if(!string.IsNullOrEmpty(package.Dependencies))
            {
                string[] dependencies = package.Dependencies.Split(null);
                foreach(string s in dependencies)
                {
                    if (!PackageManager.IsPackageInstalled(s))
                    {
                        pkgsToInstall.Add(PackageManager.GetPackageFromList(s));
                    }
                }
            }

            // Add the package then as last
            pkgsToInstall.Add(package);

            // Install the packages
            PromptInstallPackages(pkgsToInstall);

            base.Execute();
        }

        /// <summary>
        /// Prompt the user for installation
        /// </summary>
        void PromptInstallPackages(List<Package> packages)
        {
            // Count the total size of the packages and add all the names to a string
            int sizeTotal = 0;
            StringBuilder builder = new StringBuilder();
            foreach(Package package in packages)
            {
                sizeTotal += package.Size;
                builder.Append(package.Name + " ");
            }

            if (!force)
            {
                Console.WriteLine("Download package(s) {0}, size: {1}KB? [y|n]", builder.ToString(), sizeTotal);

                string answer = Console.ReadLine();

                bool download = false;

                switch (answer)
                {
                    case "y":
                    case "yes":
                        download = true;
                        break;
                }

                if (!download)
                {
                    Console.WriteLine("Aborting");
                    return;
                }
            }

            // Install everything
            foreach (Package package in packages)
            {
                PackageManager.InstallPackage(package);
            }
        }
    }
}
