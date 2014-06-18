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
            Commands.Add(new CommandRemove());
            Commands.Add(new CommandListPkgs());
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
                    return;
                }
            }
            throw new CommandException("Could not find command for " + name, null);
        }
    }
}
