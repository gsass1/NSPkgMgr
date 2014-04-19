using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSPkgMgr
{
    class Command
    {
        public String Name { get; set; }
        public String[] Args { get; set; }

        static List<Command> Commands = new List<Command>();

        public virtual void Execute() { }

        public static void Initialize()
        {
            Commands.Add(new CommandGet());
            Commands.Add(new CommandSync());
        }

        public static void AddCommand(Command command)
        {
            Commands.Add(command);
        }

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
                else
                {
                    continue;
                }
                throw new CommandException("Could not find command for " + name, null);
            }
        }
    }
}
