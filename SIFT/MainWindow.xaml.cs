using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SIFT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.Loaded += MainWindow_Initialized;
            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames().Length);
            Console.WriteLine(string.Join("\n", System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()));
            Console.WriteLine(IO.ReadResource("SIFT.shaders.example_vertex_shader.glsl"));
            //Console.WriteLine(IO.ReadResource("example_vertex_shader.glsl"));
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                GameWindow gameWindow = new GameWindow();
                gameWindow.Run(60, 60);
            });
            this.Hide();
        }
    }
}
