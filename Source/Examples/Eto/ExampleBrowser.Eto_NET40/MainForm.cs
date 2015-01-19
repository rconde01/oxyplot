using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using ExampleLibrary;

namespace ExampleBrowser.Eto_NET40
{
   class MainForm : Form
   {
      OxyPlot.Eto.Forms.PlotView plotView;
      TreeView treeView;

      ExampleInfo selectedExample;

      public IList<ExampleInfo> Examples { get; private set; }

      public ExampleInfo SelectedExample
      {
         get
         {
            return selectedExample;
         }

         set
         {
            selectedExample = value;
            plotView.Model = selectedExample != null ? selectedExample.PlotModel : null;
            plotView.Controller = selectedExample != null ? selectedExample.PlotController : null;
         }
      }

      public MainForm()
      {
         this.Width = 800;
         this.Height = 600;

         this.Examples = ExampleLibrary.Examples.GetList().OrderBy(e => e.Category).ToList();

         this.plotView = new OxyPlot.Eto.Forms.PlotView();

         this.treeView = new TreeView();

         var root = new TreeItem();

         TreeItem categoryNode = null;
         string categoryName = null;

         foreach(var ex in Examples)
         {
            if(categoryName == null || categoryName != ex.Category)
            {
               categoryName = ex.Category;
               categoryNode = new TreeItem();
               categoryNode.Text = ex.Category;

               root.Children.Add(categoryNode);
            }

            TreeItem exampleNode = new TreeItem();

            exampleNode.Text = ex.Title;

            categoryNode.Children.Add(exampleNode);
         }

         treeView.DataStore = root;

         treeView.SelectionChanged += (s, e) =>
            {
               if(treeView.SelectedItem != null)
               {
                  var sample = treeView.SelectedItem.Text;
                  var info = this.Examples.FirstOrDefault(ex => ex.Title == sample);

                  if(info != null)
                  {
                     this.SelectedExample = info;
                  }
               }
            };

         Content = new Splitter
         {
            Panel1 = treeView,
            Panel2 = plotView
         };
      }
   }
}
