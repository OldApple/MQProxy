using ApCommLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ApServerProxyGenerator
{
    public class GeneratorFactory
    {
        #region 变量、属性
        private Assembly _selectedAssembly;

        private string assemblyPath = string.Empty;
        /// <summary>
        /// 程序集路径
        /// </summary>
        public string AssemblyPath
        {
            get
            {
                return assemblyPath;
            }
            set
            {
                assemblyPath = value;
            }
        }

        private List<string> lisClasses = new List<string>();
        /// <summary>
        /// 待生成客户端代理类集合
        /// </summary>
        public List<string> LisClasses
        {
            get
            {
                return lisClasses;
            }
        }
        private int selectClass = 0;
        /// <summary>
        /// 索引
        /// 选中要生成客户端代理的服务端类
        /// </summary>
        public int SelectClass
        {
            get
            {
                return selectClass;
            }
            set
            {
                selectClass = value;
            }
        }

        private string nameSpace = string.Empty;
        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpace
        {
            get
            {
                return nameSpace;
            }
            set
            {
                nameSpace = value;
            }
        }

        private string targetFolder = string.Empty;
        /// <summary>
        /// 目标文件夹
        /// </summary>
        public string TargetFolder
        {
            get
            {
                return targetFolder;
            }
            set
            {
                targetFolder = value;
            }
        }

        private bool isAsync;
        /// <summary>
        /// 客户端是否异步获取回复消息
        /// </summary>
        public bool IsAsync
        {
            get
            {
                return isAsync;
            }
            set
            {
                isAsync = value;
            }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 获取需要生成客户端代理的类
        /// </summary>
        public void FillClasses()
        {
            lisClasses.Clear();
            lisClasses.Add("All ApService classes");
            var assembly = Assembly.LoadFrom(assemblyPath);
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(ApServiceAttribute), true);
                if (attributes.Length <= 0)
                {
                    continue;
                }
                lisClasses.Add(type.FullName);
            }
            _selectedAssembly = assembly;
        }

        public bool GenerateCode()
        {
            if (lisClasses.Count < 2)
            {
                MessageBox.Show("There is no class to generate.");
                return false;
            }

            if (string.IsNullOrEmpty(targetFolder))
            {
                MessageBox.Show("Please enter a target folder to generate code files.");
                return false;
            }

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (selectClass == 0)
            {
                for (var i = 1; i < lisClasses.Count; i++)
                {
                    GenerateProxyClass(_selectedAssembly, nameSpace, lisClasses[i].ToString(), targetFolder,isAsync);
                }
            }
            else
            {
                GenerateProxyClass(_selectedAssembly, nameSpace, lisClasses[selectClass].ToString(), targetFolder,isAsync);
            }
            return true;
        }

        private static void GenerateProxyClass(Assembly assembly, string namespaceName, string className, string targetFolder,bool isAsync)
        {
            var type = assembly.GetType(className);
            var generateMethod = type.GetMethod("GenerateProxyClass");
            var obj = Activator.CreateInstance(type);
            var classCode = (string)generateMethod.Invoke(obj, new object[] { assembly.FullName.Split(',')[0], namespaceName, isAsync });
            var proxyClassName = type.Name + "Proxy";
            using (var writer = new StreamWriter(Path.Combine(targetFolder, proxyClassName + ".cs"), false, Encoding.UTF8))
            {
                writer.WriteLine(classCode);
            }
        }

        #endregion
    }
}
