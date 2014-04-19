using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSPkgMgr
{
    /// <summary>
    /// Class for command execution while being the base class for commands
    /// </summary>
    class Command
    {
        public String Name { get; set; }
        public String[] Args { get; set; }

        // List of all commands which can be executed
        public static List<Command> Commands = new List<Command>();

        public virtual void Execute() { }

        /// <summary>
        /// Add all commands here
        /// </summary>
        public static void Initialize()
        {
            Commands.Add(new CommandGet());
            Commands.Add(new CommandSync());
        }

        /// <summary>
        /// Search for the command and then execute it
        /// When nothing is found a CommandException is thrown
        /// </summary>
        public static void Execute(String name, String[] args)
        {
            foreach(Command command in Commands)
            {
                if(command.Name == name)
                {
                    command.Args = args;
                    command.Execute();
                    break;
                }
            }
            throw new CommandException("Could not find command for " + name, null);
        }
    }
}
