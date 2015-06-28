using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ApServerProxyGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ProxyGeneratorForm());
            }
            else
            {
                GeneratorFactory gf = new GeneratorFactory();
                //程序集路径
                gf.AssemblyPath = args[0];
                //命名空间
                gf.NameSpace = args[1];
                //目标文件夹
                gf.TargetFolder = args[2];
                gf.FillClasses();
                gf.GenerateCode();
            }
        }
    }
}