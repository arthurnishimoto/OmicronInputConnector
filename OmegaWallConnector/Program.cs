/**
 * ---------------------------------------------
 * Program.cs
 * Description: Main Class
 * 
 * Class: 
 * System: Windows 7 Professional x64
 * Copyright 2010, Electronic Visualization Laboratory, University of Illinois at Chicago.
 * Author(s): Arthur Nishimoto
 * Version: 0.1
 * Version Notes:
 * 8/23/10      - Initial version
 * ---------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OmegaWallConnector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("TouchAPI Connector 1.0");
            Console.WriteLine("Copyright (C) 2010 Electronic Visualization Laboratory\nUniversity of Illinois at Chicago");
            Console.WriteLine("======================================================");
            Console.WriteLine("");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUI());
        }
    }
}
