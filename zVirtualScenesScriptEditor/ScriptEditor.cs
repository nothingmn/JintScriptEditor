using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zVirtualScenesScriptEditor
{
    public partial class ScriptEditor : Form
    {
        public ScriptEditor()
        {
            InitializeComponent();

            this.dataGridView1.Columns.Add("Name", "Name");
            this.dataGridView1.Columns.Add("Value", "Value");
            this.dataGridView1.Columns.Add("Class", "Class");
            this.dataGridView1.Columns.Add("IsClr", "IsClr");
            this.dataGridView1.Columns.Add("Type", "Type");

        }
        ScriptLibraryManager scripts;
        private void Form1_Load(object sender, EventArgs e)
        {
            this.saveButton.Enabled = false;
            this.playButton.Enabled = false;

            this.tabControl2.TabPages.Clear();
            scripts = new ScriptLibraryManager(this.folderTreeView, this.newFolderFileMenu, @"C:\Users\Administrator\Desktop\source\zVirtualScenesScriptEditor\zVirtualScenesScriptEditor\ScriptLibrary\");
            scripts.OnFileChosen += scripts_OnFileChosen;
        }

        void scripts_OnFileChosen(System.IO.FileInfo File, string Contents)
        {
            TabPage page = new TabPage(File.Name);
            page.Tag = File;
            ScintillaNET.Scintilla editor = new ScintillaNET.Scintilla();
            editor.ConfigurationManager.Language = "js";
            editor.Margins[0].Width = 20;
            editor.Margins[1].Width = 20;
            editor.Margins[1].IsClickable = true;
            editor.MarginClick += editor_MarginClick;

            editor.Dock = DockStyle.Fill;
            editor.Text = Contents;
            editor.MatchBraces = true;
            
            editor.KeyUp += editor_KeyUp;
            page.Controls.Add(editor);
            this.tabControl2.TabPages.Add(page);

            this.saveButton.Enabled = true;
            this.playButton.Enabled = true;

            tabControl2.SelectedTab = page;
        }

        void editor_MarginClick(object sender, ScintillaNET.MarginClickEventArgs e)
        {
            var editor = (sender as ScintillaNET.Scintilla);
            if (editor != null)
            {
                ScintillaNET.Marker m = editor.Markers[0];
                m.Symbol = ScintillaNET.MarkerSymbol.Circle;
                var markers = e.Line.GetMarkers();
                if (markers != null && markers.Count > 0)
                {
                    e.Line.DeleteAllMarkers();
                }
                else
                {
                    e.Line.AddMarker(m);
                }
                
            }
            
        }

        void editor_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveCurrentFile();
            }
            if (e.KeyCode == Keys.F5)
            {
                Run();
            }
        }

        public void SaveCurrentFile()
        {

            System.IO.FileInfo selectedFile = SelectedFile;
            if (selectedFile != null)
            {
                string contents = SelectedScript;
                if (contents == null) return;
                System.IO.File.WriteAllText(selectedFile.FullName, contents);
            }
        }
        public void CloseCurrentFile()
        {
            DialogResult result = MessageBox.Show("Do you want to save your changes?", "Save...", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Cancel) return;
            if (result == DialogResult.Yes) SaveCurrentFile();

            TabPage page = this.tabControl2.SelectedTab;
            if (page != null)
            {
                page.Parent.Controls.Remove(page);
                page.Dispose();
            }

        }
        private void saveButton_Click(object sender, EventArgs e)
        {

            SaveCurrentFile();
        }

        private string SelectedScript
        {
            get
            {
                TabPage page = this.tabControl2.SelectedTab;
                if (page != null)
                {
                    System.IO.FileInfo file = (page.Tag as System.IO.FileInfo);
                    if (file != null)
                    {
                        var editor = (page.Controls[0] as ScintillaNET.Scintilla);
                        if (editor != null)
                        {
                            return editor.Text;
                        }
                    }
                }
                return null;
            }
        }
        private System.IO.FileInfo SelectedFile
        {
            get
            {
                TabPage page = this.tabControl2.SelectedTab;
                if (page != null)
                {
                    return (page.Tag as System.IO.FileInfo);
                }
                return null;
            }
        }
        private ScintillaNET.Scintilla SelectedEditor
        {
            get
            {
                TabPage page = this.tabControl2.SelectedTab;
                if (page != null)
                {
                    return (page.Controls[0] as ScintillaNET.Scintilla);
                }
                return null;
            }
        }

        private void Run()
        {

            ScintillaNET.Scintilla editor = SelectedEditor;

            List<int> breakpoints = new List<int>();
            foreach (ScintillaNET.Line line in editor.Lines)
            {
                if (line.GetMarkers().Count > 0) breakpoints.Add(line.Number);
            }

            this.dataGridView1.DataSource = null;
            this.outputTextbox.Text = "Execution started for:" + SelectedFile.FullName;
            ScriptEngine e = new ScriptEngine();
            e.Break += e_Break;
            string result = e.Execute(SelectedScript, breakpoints);
            this.outputTextbox.AppendText("\r\n");
            this.outputTextbox.AppendText(result);
        }

        void e_Break(object sender, Jint.Debugger.DebugInformation e)
        {
            this.dataGridView1.Rows.Clear();
            Dictionary<string, string> locals = new Dictionary<string, string>();
            foreach (var local in e.Locals)
            {

                string c = (local.Value).Class.ToString().ToLower();
                if (c != "function" && c != "global" && local.Key != "Function" && local.Key != "Math" && local.Key != "null" && local.Key != "NaN" && local.Key != "Infinity" && local.Key != "undefined" && local.Key != "this")
                {
                    var l = e.Locals[local.Key];
                    string value = "";
                    if (c == "array")
                    {
                        value = "[";
                        var ary = (l as Jint.Native.JsArray);
                        for (int i = 0; i <= ary.Length - 1; i++)
                        {
                            value += ary[i.ToString()].ToString();
                            if (i < ary.Length - 1) value += ",";
                        }
                        value += "]";
                    }
                    else if (c == "object")
                    {
                        value = "{";
                        var ary = (l as Jint.Native.JsObject);
                        foreach (var item in ary)
                        {
                            value += item.ToString() + ",";

                        }

                        if (value.EndsWith(",")) value = value.Substring(0, value.Length - 1);
                        value += "}";
                    }
                    else
                    {
                        value = (l.Value == null ? "null" : l.Value.ToString());
                    }
                    this.dataGridView1.Rows.Add(local.Key, value, l.Class, l.IsClr, l.Type);
                }

            }

            this.tabControl1.SelectTab(this.localsTabPage);

        }
        private void playButton_Click(object sender, EventArgs e)
        {
            Run();
        }


        private void closeLabel_Click(object sender, EventArgs e)
        {
            CloseCurrentFile();
        }


    }
}
