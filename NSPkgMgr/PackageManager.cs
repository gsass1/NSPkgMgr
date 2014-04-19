using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;

namespace NSPkgMgr
{
    class PackageManager
    {
        // Are we using the interactive mode or just executing a command?
        public static Mode mode;

        public static String configFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\NSPkgMgr";

        public static String configPath = configFolder + "\\pkgmgr.conf";
        public static String pkgCachePath = configFolder + "\\packages.cache";
        public static String pkgInstalledPath = configFolder + "\\packages.installed";

        public static Config config = new Config();

        public static List<Package> packages = new List<Package>();

        static void PrintUsage()
        {

        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("NukeSoftware Package Manager DEV");
                Command.Initialize();
                StartupCheck();
                ReadPackagesIntoBuffer();
                if (args.Length == 0)
                {
                    mode = NSPkgMgr.Mode.Interactive;
                    StartInteractiveMode();
                }
                else
                {
                    mode = NSPkgMgr.Mode.Command;
                    if (args.Length == 0)
                    {
                        PrintUsage();
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach(string s in args)
                        {
                            builder.Append(s + " ");
                        }
                        ExecuteCommand(builder.ToString());
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(string.Format("Unhandled Exception caught: {0}\nStack Trace: {1}", e.Message, e.StackTrace));
                return;
            }
        }

        static void ExecuteCommand(string cmdLine)
        {
            string[] cmdLineSplit = cmdLine.Split(null);
            string command = cmdLineSplit[0];
            string[] args = new string[0];
            int argc = cmdLineSplit.Length - 1;
            if (argc != 0)
            {
                args = new string[argc];
                Array.Copy(cmdLineSplit, 1, args, 0, argc);
            }

            try
            {
                Command.Execute(command, args);
            }
            catch (CommandException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void StartInteractiveMode()
        {
            while (true)
            {
                Console.Write(">");
                string cmdLine = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmdLine))
                {
                    Console.WriteLine("Invalid argument.");
                }
                else
                {
                    ExecuteCommand(cmdLine);
                }
            }
        }
        

        static void StartupCheck()
        {
            if(File.Exists(configPath) == false)
            {
                Console.WriteLine("Detected that NSPkgMgr is not set up. Setting up!");
                Setup();
            }
        }

        static void Setup()
        {
            Directory.CreateDirectory(configFolder);
            TextWriter fileStream = new StreamWriter(configPath);
            new XmlSerializer(typeof(Config)).Serialize(fileStream, config);
            fileStream.Close();
            Directory.CreateDirectory(config.IncludePath);
            Directory.CreateDirectory(config.LibraryPath);
            UpdatePackageList();
            XmlDocument doc = new XmlDocument();
            XmlNode pkgNode = doc.AppendChild(doc.CreateElement("InstalledPackages"));
            doc.Save(pkgInstalledPath);
        }

        public static Package GetPackageFromList(string pkgName)
        {
            foreach (Package package in packages)
            {
                if (package.Name == pkgName)
                {
                    return package;
                }
            }
            return null;
        }

        static string MakePkgCachePath(string packagename)
        {
            return configFolder + "\\" + packagename + ".temp";
        }

        static string MakePkgArchiveCachePath(string packagename)
        {
            return MakePkgCachePath(packagename) + ".d";
        }

        public static void RemovePackage(Package package)
        {
            Console.WriteLine("Removing package '{0}'", package.Name);
        }

        public static void InstallPackage(Package package)
        {
            WebClient client = new WebClient();
            string cachePath = MakePkgCachePath(package.Name);
            Console.WriteLine("Downloading from '{0}' to '{1}'", package.URL, cachePath);
            try
            {
                client.DownloadFile(package.URL, cachePath);
            }
            catch (WebException e)
            {
                Console.WriteLine("Could not download package: {0}", e.Message);
                return;
            }
            Console.WriteLine("Extracting...");
            string archiveExtractPath = MakePkgArchiveCachePath(package.Name);
            ZipFile.ExtractToDirectory(cachePath, archiveExtractPath);
            
            if(!string.IsNullOrEmpty(package.IncludeDir))
            {
                string includeDirName;
                if(!string.IsNullOrEmpty(package.IncludeOutputDir))
                {
                    includeDirName = package.IncludeOutputDir;
                }
                else
                {
                    includeDirName = package.Name;
                }
                string includeDirPath = config.IncludePath + "\\" + includeDirName;
                DirectoryCopy(archiveExtractPath + "\\" + package.IncludeDir, includeDirPath, true);
            }

            if (!string.IsNullOrEmpty(package.LibraryDir))
            {
                DirectoryCopy(archiveExtractPath + "\\" + package.LibraryDir, config.LibraryPath, true);
            }

            WritePackageIsInstalled(package.Name);

            Console.WriteLine("Cleaning up temp files");
            File.Delete(cachePath);
            Directory.Delete(archiveExtractPath, true);
        }

        public static bool CheckPackageExists(string pkgName)
        {
            CheckPackageList();
            foreach(Package package in packages)
            {
                if(package.Name == pkgName)
                {
                    return true;
                }
            }
            return false;
        }

        public static void CheckPackageList()
        {
            if(File.Exists(pkgCachePath) == false)
            {
                UpdatePackageList();
            }
        }

        public static void GetPackage(string name)
        {
            WebClient client = new WebClient();
        }

        public static void UpdatePackageList()
        {
            Console.WriteLine("Downloading package list...");
            WebClient client = new WebClient();

            try
            {
                client.DownloadFile(config.MirrorUrl, pkgCachePath);
            }
            catch (WebException e)
            {
                Console.WriteLine("Could not download package list: {0}", e.Message);
                return;
            }

            ReadPackagesIntoBuffer();
        }

        static void ReadPackagesIntoBuffer()
        {
            packages.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load(pkgCachePath);

            XmlNode packagesNode = doc.ChildNodes[1];
            foreach (XmlNode pkgNode in packagesNode.ChildNodes)
            {
                Package package = (Package)new XmlSerializer(typeof(Package)).Deserialize(new XmlNodeReader(pkgNode));
                packages.Add(package);
            }
        }

        public static void WritePackageIsInstalled(string pkgName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pkgInstalledPath);

            XmlNode packagesNode = doc.FirstChild;
            XmlNode packageNode = packagesNode.AppendChild(doc.CreateElement("Package"));
            XmlAttribute attribName = packageNode.Attributes.Append(doc.CreateAttribute("Name"));
            attribName.Value = pkgName;
            doc.Save(pkgInstalledPath);
        }

        public static bool IsPackageInstalled(string pkgName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pkgInstalledPath);

            XmlNode packagesNode = doc.FirstChild;

            foreach(XmlNode node in packagesNode.ChildNodes)
            {
                if(node.Attributes["Name"].InnerText == pkgName)
                {
                    return true;
                }
            }
            return false;
        }

        static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            Console.WriteLine("Installing {0} into {1}", sourceDirName, destDirName);
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
