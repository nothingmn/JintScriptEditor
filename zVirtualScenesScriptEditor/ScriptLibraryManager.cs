using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zVirtualScenesScriptEditor
{
    public class ScriptLibraryManager
    {
        public System.IO.DirectoryInfo RootFolder { get; set; }
        public string Root { get; set; }
        public System.Windows.Forms.TreeView TreeView { get; set; }
        public ContextMenuStrip ContextMenu { get; set; }

        public ScriptLibraryManager(System.Windows.Forms.TreeView Tree,  ContextMenuStrip ContextMenu, string root = null)
        {
            if (string.IsNullOrEmpty(root))
                this.Root = System.IO.Path.Combine((new System.IO.FileInfo(typeof(ScriptEditor).Assembly.Location)).Directory.FullName, "ScriptLibrary");
            else
                this.Root = root;

            TreeView = Tree;
            RootFolder = new System.IO.DirectoryInfo(this.Root);
            this.ContextMenu = ContextMenu;

            RootNode.Text = "/";
            RootNode.Tag = this.Root;
            this.SelectedNode = RootNode;
            this.TreeView.Nodes.Add(RootNode);
            
            this.TreeView.NodeMouseDoubleClick += TreeView_NodeMouseDoubleClick;
            this.TreeView.NodeMouseClick += TreeView_NodeMouseClick;

            this.ContextMenu.Click += ContextMenu_Click;            
            RootNode.Expand();
            BindNode(this.Root);
            RootNode.ExpandAll();
        }

        void ContextMenu_Click(object sender, EventArgs e)
        {
            var mouse = (e as MouseEventArgs);
            var item = this.ContextMenu.GetItemAt(mouse.X, mouse.Y);
            bool isFolder = (item.Text.Contains("Folder"));
            string key = (isFolder ? "Folder" : "File");
            InputBoxResult result = InputBox.Show("New " + key + " Name:", "New...");
            if (result.ReturnCode == DialogResult.OK)
            {
                string name = result.Text;
                if (!string.IsNullOrEmpty(name))
                {
                    if (isFolder)
                    {
                        NewFolder(name, this.SelectedNode);
                    }
                    else
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(name);
                        if (string.IsNullOrEmpty(fi.Extension)) name = name + ".js";
                        NewFile(name, this.SelectedNode);
                    }
                }
            }

        }
        void NewFile(string name, TreeNode ParentNode)
        {
            System.IO.DirectoryInfo path = (ParentNode.Tag as System.IO.DirectoryInfo);
            if (path != null)
            {
                string newFile = System.IO.Path.Combine(path.FullName, name);
                System.IO.FileInfo file = new System.IO.FileInfo(newFile);
                System.IO.File.WriteAllText(newFile, "");
                System.Windows.Forms.TreeNode node = new System.Windows.Forms.TreeNode(file.Name);
                node.Tag = file;
                this.SelectedNode.Nodes.Add(node);
                LoadFile(file);

            }
        }
        void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.SelectedNode = e.Node;
            if (e.Button == MouseButtons.Right)
            {
                var path = (e.Node.Tag as System.IO.DirectoryInfo);
                if(path!=null)
                    this.ContextMenu.Show((sender as Control), e.X, e.Y);
            }
        }
        public void NewFolder(string Name, TreeNode ParentNode)
        {
            System.IO.DirectoryInfo path = (ParentNode.Tag as System.IO.DirectoryInfo);
            if (path != null)
            {
                string newPath = System.IO.Path.Combine(path.FullName, Name);
                System.IO.DirectoryInfo folder = System.IO.Directory.CreateDirectory(newPath);
                System.Windows.Forms.TreeNode node = new System.Windows.Forms.TreeNode(folder.Name);
                node.Tag = folder;
                ParentNode.Nodes.Add(node);
            }
        }
        void TreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.SelectedNode = e.Node;
            System.IO.FileInfo file = (e.Node.Tag as System.IO.FileInfo);
            LoadFile(file);
        }
        void LoadFile(System.IO.FileInfo file)
        {
            if (file != null)
            {
                string contents = System.IO.File.ReadAllText(file.FullName);
                if (OnFileChosen != null) OnFileChosen(file, contents);
            }
        }


        public delegate void FileChosen(System.IO.FileInfo Filename, string Contents);
        public event FileChosen OnFileChosen;
        private System.Windows.Forms.TreeNode RootNode = new System.Windows.Forms.TreeNode();
        private System.Windows.Forms.TreeNode SelectedNode;
        private void BindNode(string path)
        {
            System.IO.DirectoryInfo rootPath = new System.IO.DirectoryInfo(path);

            foreach (var file in rootPath.GetFiles("*.js"))
            {
                System.Windows.Forms.TreeNode node = new System.Windows.Forms.TreeNode(file.Name);
                node.Tag = file;
                this.SelectedNode.Nodes.Add(node);
            }
            TreeNode n = this.SelectedNode;
            foreach (var folder in rootPath.GetDirectories())
            {
                System.Windows.Forms.TreeNode node = new System.Windows.Forms.TreeNode(folder.Name);
                node.Tag = folder;                
                this.SelectedNode.Nodes.Add(node);
                this.SelectedNode = node;
                BindNode(folder.FullName);
            }
            this.SelectedNode = n;
        }


    }
}
