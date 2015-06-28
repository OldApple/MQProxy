using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ApServerProxyGenerator
{
    public partial class ProxyGeneratorForm : Form
    {
        #region 变量、属性
        private GeneratorFactory gf;
        #endregion

        public ProxyGeneratorForm()
        {
            InitializeComponent();
            gf = new GeneratorFactory();
        }

        /// <summary>
        /// 选择程序集
        /// </summary>
        private void btnBrowseAssembly_Click(object sender, EventArgs e)
        {
            var dialogResult = AssemblyBrowseDialog.ShowDialog();
            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            try
            {
                txtAssemblyPath.Text = AssemblyBrowseDialog.FileName;
                gf.AssemblyPath = AssemblyBrowseDialog.FileName;
                gf.FillClasses();
                cmbClasses.DataSource = gf.LisClasses;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 目标文件夹
        /// </summary>
        private void btnTargetFolderBrowse_Click(object sender, EventArgs e)
        {
            var dialogResult = TargetFolderBrowseDialog.ShowDialog();
            if (dialogResult != DialogResult.OK)
            {
                return;
            }
            gf.TargetFolder = TargetFolderBrowseDialog.SelectedPath;
            txtTargetFolder.Text = TargetFolderBrowseDialog.SelectedPath;
        }

        /// <summary>
        /// 更改选中服务端类
        /// </summary>
        private void cmbClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            gf.SelectClass = cmbClasses.SelectedIndex;
        }

        /// <summary>
        /// 生成服务端代理
        /// </summary>
        private void btnGenerateCode_Click(object sender, EventArgs e)
        {
            btnGenerateCode.Enabled = false;
            try
            {
                Application.DoEvents();
                gf.NameSpace = txtNamespace.Text.TrimEnd();
                gf.IsAsync = cbIsAsync.Checked;
                if (gf.GenerateCode())
                {
                    MessageBox.Show("Proxy classes are generated.", "Success.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                btnGenerateCode.Enabled = true;
            }
        }

    }
}