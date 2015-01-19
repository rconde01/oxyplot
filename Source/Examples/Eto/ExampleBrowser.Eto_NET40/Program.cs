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
         if (Eto.Platform.Detect.IsWpf)
         {
            //Fix up fact that Eto drawable implementation is tiled by default
            //This causes horrible performance because you render the entire plot
            //for each tile
            Eto.Style.Add<Eto.Wpf.Forms.Controls.DrawableHandler>(null, h => h.AllowTiling = false);
         }

         new Application(Eto.Platform.Detect).Run(new MainForm());
      }
   }
}
