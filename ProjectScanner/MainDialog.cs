using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ProjectScanner
{
    public partial class MainDialog : Form
    {
        public MainDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = true;
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.Cancel) return;

            var folderPath = folderBrowserDialog1.SelectedPath;

            var projectFiles = Directory.GetFiles(
                    folderPath,
                    "*.csproj",
                    SearchOption.AllDirectories
                );

            progressBar1.Maximum = projectFiles.Length;
            progressBar1.Value = 0;

            var sb = new StringBuilder();

            foreach (var projectFile in projectFiles)
            {
                progressBar1.Value++;
                
                var projectName = Path.GetFileNameWithoutExtension(projectFile);
                var projectPath = Path.GetDirectoryName(projectFile);

                var projectStructure = XDocument.Load(projectFile);
                var references = projectStructure.Descendants().Where(x => x.Name.LocalName == "Reference");

                foreach (var reference in references)
                {
                    var returnValue = GetAssemblyVersion(reference, projectPath);
                    var assemblyName = returnValue.Item1;
                    var assemblyVersion = returnValue.Item2;

                    if (string.IsNullOrWhiteSpace(assemblyName)) continue;

                    if (checkBox1.Checked)
                        sb.Append(projectPath).Append(",");

                    sb.AppendLine(string.Join(",",
                        projectName,
                        assemblyName,
                        assemblyVersion
                    ));
                }
            }

            textBox1.Text = sb.ToString();
            textBox1.Focus();
        }
        private static Tuple<string, string> GetAssemblyVersion(XElement reference, string projectPath)
        {
            try
            {
                if (reference.HasElements)
                {
                    var hintPath = reference.Elements().First(n => n.Name.LocalName == "HintPath").Value;
                    if (!hintPath.StartsWith(@"\\"))
                        hintPath = Path.Combine(projectPath, hintPath);

                    var assembly = Assembly.LoadFile(hintPath);
                    var assemblyName = assembly.GetName();

                    return Tuple.Create(
                        assemblyName.Name,
                        assemblyName.Version.ToString(3));
                }

                if (reference.HasAttributes)
                {
                    var includedName = reference.FirstAttribute.Value;

                    var segments = includedName.Split(',');
                    var project = segments[0];

                    if (project.StartsWith("System") || project.StartsWith("Microsoft")) return Tuple.Create("", "");

                    var version = segments[1].Substring(9);
                    return Tuple.Create(project, version);
                }
            }
            catch (Exception) { }

            return Tuple.Create("", "");
        }

        private void MainDialog_Load(object sender, EventArgs e)
        {

        }
    }
}
