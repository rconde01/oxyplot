using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto;
using Eto.Forms;

namespace ExampleBrowser.Eto_NET40
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         new Application(Eto.Platform.Detect).Run(new MainForm());
      }
   }
}
