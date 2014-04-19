using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSPkgMgr
{
    class CommandGet : Command
    {
        public CommandGet()
        {
            Name = "get";
        }

        public override void Execute()
        {
            if(Args.Length == 0)
            {
                throw new CommandInvalidArgumentCountException(this);
            }

            string packagename = Args[0];
            if(!PackageManager.CheckPackageExists(packagename))
            {
                Console.WriteLine("Could not find package '{0}'", packagename);
                return;
            }

            if(PackageManager.IsPackageInstalled(packagename))
            {
                Console.WriteLine("'{0}' is already installed", packagename);
                return;
            }

            Package package = PackageManager.GetPackageFromList(packagename);

            List<Package> pkgsToInstall = new List<Package>();

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

            pkgsToInstall.Add(package);

            PromptInstallPackages(pkgsToInstall);

            base.Execute();
        }

        void PromptInstallPackages(List<Package> packages)
        {
            int sizeTotal = 0;
            StringBuilder builder = new StringBuilder();
            foreach(Package package in packages)
            {
                sizeTotal += package.Size;
                builder.Append(package.Name + " ");
            }

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

            foreach (Package package in packages)
            {
                PackageManager.InstallPackage(package);
            }
        }
    }
}
