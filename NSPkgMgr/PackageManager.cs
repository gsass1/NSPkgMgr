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
            // TODO
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("NukeSoftware Package Manager DEV");
                Command.Initialize();
                Setup();

                // If no args start interactive mode
                if (args.Length == 0)
                {
                    mode = NSPkgMgr.Mode.Interactive;
                    StartInteractiveMode();
                }
                // else execute the command by the args
                else
                {
                    mode = NSPkgMgr.Mode.Command;

                    // Parse the array into a string, adding a whitespace for each
                    StringBuilder builder = new StringBuilder();
                    foreach (string s in args)
                    {
                        builder.Append(s + " ");
                    }

                    // Remove the last whitespace since we don't need it,
                    // but only if we have any arguments at all
                    if (builder.Length != 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                    }

                    ExecuteCommand(builder.ToString());

                }
            }
            catch(Exception e)
            {
                MessageBox.Show(string.Format("Unhandled Exception caught: {0}\nStack Trace: {1}", e.Message, e.StackTrace));
                return;
            }
        }

        /// <summary>
        /// Format the command line string and execute the command
        /// </summary>
        static void ExecuteCommand(string cmdLine)
        {
            // Split by whitespace
            string[] cmdLineSplit = cmdLine.Split(null);

            // Command name is first word
            string command = cmdLineSplit[0];

            // Create an args array with 0 elements so it isn't null
            // so when don't always have to check if it's null
            string[] args = new string[0];

            // Get all the other args
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

        /// <summary>
        /// Prompt for user input in a loop
        /// </summary>
        static void StartInteractiveMode()
        {
            while (true)
            {
                Console.Write(">");
                string cmdLine = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmdLine))
                {
                    Console.WriteLine("Malformed expression");
                }
                else
                {
                    ExecuteCommand(cmdLine);
                }
            }
        }

        /// <summary>
        /// Create all directories we need and write the basic files
        /// </summary>
        static void Setup()
        {
            // Create this first
            Directory.CreateDirectory(configFolder);

            // Write basic config file when no one is there
            if (!File.Exists(configPath))
            {
                new XmlSerializer(typeof(Config)).Serialize(new StreamWriter(configPath), config);
            }

            // Write empty installed packages file
            if (!File.Exists(pkgInstalledPath))
            {
                XmlDocument doc = new XmlDocument();
                XmlNode pkgNode = doc.AppendChild(doc.CreateElement("InstalledPackages"));
                doc.Save(pkgInstalledPath);
            }

            // Create the other directories
            Directory.CreateDirectory(config.IncludePath);
            Directory.CreateDirectory(config.LibraryPath);

            // Get newest packages.xml
            UpdatePackageList();
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

        public static void RemovePackage(string pkgName)
        {
            Package package = GetPackageFromList(pkgName);
            Console.WriteLine("Removing package '{0}'", package.Name);

            XmlDocument doc = new XmlDocument();
            doc.Load(pkgInstalledPath);

            XmlNode packagesNode = doc.FirstChild;
            foreach(XmlNode node in packagesNode.ChildNodes)
            {
                if(node.Attributes["Name"].InnerText == pkgName)
                {
                    // Split by ;
                    string[] files = node.ChildNodes[0].InnerText.Split(';');
                    foreach(string s in files)
                    {
                        // Sometimes this bugs out so ignore these
                        if(string.IsNullOrWhiteSpace(s))
                        {
                            continue;
                        }
                        Console.WriteLine("Removing {0}", s);
                        File.Delete(s);
                    }
                    packagesNode.RemoveChild(node);
                }
            }
            doc.Save(pkgInstalledPath);
        }

        public static void InstallPackage(Package package)
        {
            // Path of the zip file to be saved to
            string cachePath = MakePkgCachePath(package.Name);
            Console.WriteLine("Downloading from '{0}' to '{1}'", package.URL, cachePath);
            try
            {
                new WebClient().DownloadFile(package.URL, cachePath);
            }
            catch (WebException e)
            {
                Console.WriteLine("Could not download package: {0}", e.Message);
                return;
            }
            Console.WriteLine("Extracting...");
            // Path of the zip file to be extracted to
            string archiveExtractPath = MakePkgArchiveCachePath(package.Name);
            ZipFile.ExtractToDirectory(cachePath, archiveExtractPath);

            // String arrays containing the files which are copied
            string[] incFiles = null;
            string[] libFiles = null;

            // Copy the include dir if it is set
            if(!string.IsNullOrEmpty(package.IncludeDir))
            {
                // Either use the package name for the output directory
                // or the name set by IncludeOutputDir
                string includeDirName;
                if(!string.IsNullOrEmpty(package.IncludeOutputDir))
                {
                    includeDirName = package.IncludeOutputDir;
                }
                else
                {
                    includeDirName = package.Name;
                }
                incFiles = Directory.GetFiles(archiveExtractPath + "\\" + package.IncludeDir, "*.*", SearchOption.AllDirectories);
                string includeDirPath = config.IncludePath + "\\" + includeDirName;
                DirectoryCopy(archiveExtractPath + "\\" + package.IncludeDir, includeDirPath, true);
            }

            // Copy the library dir if it is set
            if (!string.IsNullOrEmpty(package.LibraryDir))
            {
                libFiles = Directory.GetFiles(archiveExtractPath + "\\" + package.LibraryDir, "*.*", SearchOption.AllDirectories);
                DirectoryCopy(archiveExtractPath + "\\" + package.LibraryDir, config.LibraryPath, true);
            }

            // Reformat the file paths
            // For example: Change C:\Users\User\AppData\Roaming\NSPkgMgr\pkg.temp.d\include\lala.h
            // to C:\Include\pkg\lala.h
            if(incFiles != null)
            {
                for(int i = 0; i < incFiles.Length; i++)
                {
                    int index = incFiles[i].IndexOf(configFolder);
                    string cleanPath = (index < 0) ? configFolder : incFiles[i].Remove(index, configFolder.Length);
                    // Now we have something like this \\pkg.temp.d\\SomeFolder\\include\\lala.h
                    // if we have IncludeOutputDir set then use that name
                    // or else we use the package name
                    // now format the string to that
                    string includeDir = "\\" + package.Name + ".temp.d" + "\\" + package.IncludeDir;
                    index = cleanPath.IndexOf(includeDir);
                    string reallyCleanPath = (index < 0) ? includeDir : cleanPath.Remove(index, includeDir.Length);
                    // Now it's just lala.h
                    // Append the config's include directory now and we're done
                    string includeOutputName;
                    if(!string.IsNullOrEmpty(package.IncludeOutputDir))
                    {
                        includeOutputName = package.IncludeOutputDir;
                    }
                    else
                    {
                        includeOutputName = package.Name;
                    }
                    includeOutputName = config.IncludePath + "\\" + includeOutputName + reallyCleanPath;
                    incFiles[i] = includeOutputName;
                }
            }
            if (libFiles != null)
            {
                for (int i = 0; i < libFiles.Length; i++)
                {
                    int index = libFiles[i].IndexOf(configFolder);
                    string cleanPath = (index < 0) ? configFolder : libFiles[i].Remove(index, configFolder.Length);

                    libFiles[i] = cleanPath;
                    string libDir = "\\" + package.Name + ".temp.d" + "\\" + package.LibraryDir;

                    index = cleanPath.IndexOf(libDir);
                    string reallyCleanPath = (index < 0) ? libDir : cleanPath.Remove(index, libDir.Length);

                    string libOutputName = config.LibraryPath + "\\" + reallyCleanPath;
                    libFiles[i] = libOutputName;
                }
            }

            // All file uris need to be copied in this array
            string[] files;

            if(incFiles != null && libFiles == null)
            {
                files = incFiles;
            } 
            else if(incFiles == null && libFiles != null)
            {
                files = libFiles;
            }
            else
            {
                // Merge the two file arrays
                files = new string[incFiles.Length + libFiles.Length];
                Array.Copy(incFiles, files, incFiles.Length);
                Array.Copy(libFiles, 0, files, incFiles.Length, libFiles.Length);
            }

            // Notify that the package is installed
            WritePackageIsInstalled(package.Name, files);

            // Delete temp files
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

        /// <summary>
        /// If no package cache list is present, download the newest one
        /// </summary>
        public static void CheckPackageList()
        {
            if(!File.Exists(pkgCachePath))
            {
                UpdatePackageList();
            }
        }

        /// <summary>
        /// Replace the package list with a new package list from the Mirror URL
        /// </summary>
        public static void UpdatePackageList()
        {
            Console.WriteLine("Downloading package list...");
            try
            {
                new WebClient().DownloadFile(config.MirrorUrl, pkgCachePath);
            }
            catch (WebException e)
            {
                Console.WriteLine("Could not download package list: {0}", e.Message);
                return;
            }
            ReadPackagesIntoBuffer();
        }

        /// <summary>
        /// Write all packages from the file into memory
        /// </summary>
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

        /// <summary>
        /// Write that the package is installed in packages.installed
        /// </summary>
        public static void WritePackageIsInstalled(string pkgName, string[] files)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pkgInstalledPath);

            XmlNode packagesNode = doc.FirstChild;
            XmlNode packageNode = packagesNode.AppendChild(doc.CreateElement("Package"));
            XmlAttribute attribName = packageNode.Attributes.Append(doc.CreateAttribute("Name"));
            attribName.Value = pkgName;

            XmlElement elem = (XmlElement)packageNode.AppendChild(doc.CreateElement("Files"));
    
            foreach(string s in files)
            {
                elem.InnerText += s + ";";
            }

            doc.Save(pkgInstalledPath);
        }

        public static bool IsPackageInstalled(string pkgName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pkgInstalledPath);
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                if(node.Attributes["Name"].Value == pkgName)
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
