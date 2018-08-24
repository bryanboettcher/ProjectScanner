using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ProjectScanner
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.Run(new MainDialog());
        }
    }
}
