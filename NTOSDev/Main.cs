using NTOSDev.Components;
using NTOSDev.Controls;
using NTOSDev.Libs;
using NTOSImage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace NTOSDev
{
    public partial class Main : Form
    {
        
        public ProjectExplorer projectExplorer = new ProjectExplorer() { 
            HideOnClose = true
        };

        public DEmulator dEmulator = new DEmulator()
        {
            HideOnClose = true
        };

        public DImage dImage = new DImage()
        {
            HideOnClose = true
        };

        public Main()
        {
            NTOS.Main = this;
            InitializeComponent();
            dockPanel.Theme = NTOS.Theme;
            AttachMenuHandlers(menuStrip.Items);
        }

        private void Main_Load(object sender, EventArgs e)
        {

            // new TerminalControl().Show(dockPanel, DockState.DockBottom);

            NTOS.Reload += (s) =>
            {
                CloseAllPanels();
                projectExplorer.LoadFolder(s);
                DoAction("projectExplorer");
                DoAction("runEmulator");


                try
                {
                    if (!string.IsNullOrWhiteSpace(NTOS.LastProject) && Directory.Exists(NTOS.LastProject))
                    {
                        string lastProj = NTOS.LastProject + "\\main.js";
                        if (!File.Exists(lastProj))
                        {
                            File.WriteAllText(lastProj, @$"
// NTOS Main executable file
function init() {{
    // This function is called when the project is initialized
}}

dunction draw() {{
    dialog(""Hello, World!"");
}}

draw();

function loop() {{
    // This function is called every frame
    char key = getKey();
    switch(key) {{
        case '*':
            // Do something when the '*' key is pressed
            break;
        case '#':
            // Do something when the '#' key is pressed
            break;
    }}
    delay(1);
}}
");
                        }

                        if (File.Exists(lastProj))
                        {
                            OpenFile(lastProj);
                        }
                        projectExplorer.LoadFolder(NTOS.LastProject);
                    }
                } catch(Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                
                

            };

            string command = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();
            string lastProject = NTOS.LastProject;
            if (!string.IsNullOrWhiteSpace(command) && Directory.Exists(command))
            {
                NTOS.OpenProject(command);
            }
            else if (!string.IsNullOrWhiteSpace(lastProject) && Directory.Exists(lastProject))
            {
                NTOS.OpenProject(lastProject);
            }
            else
            {
                // SM.InitializeEmptyProject();
            }

            DoAction("runEmulator");



#if RELEASE
            WindowState = FormWindowState.Maximized;
            // SplashScreen();
#endif
        }

        private void AttachMenuHandlers(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.Text = menuItem.Text;
                    menuItem.Click += MenuItem_Click;

                    if (menuItem.DropDownItems.Count > 0)
                    {
                        AttachMenuHandlers(menuItem.DropDownItems);
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
                DoAction(item.Name.Replace("ToolStripMenuItem", ""));
        }


        public void OpenFile(string filePath)
        {
            if (Path.GetExtension(filePath)?.ToLower() == ".js")
            {
                new CodeEditor(filePath).Show(dockPanel, DockState.Document);
            } else
            {
                new FileViewer(filePath).Show(dockPanel, DockState.Document);
            }
            
        }

        public void CloseAllPanels()
        {
            var contents = dockPanel.Contents.ToArray();
            foreach (DockContent p in contents)
            {
                if (p.HideOnClose)
                {
                    p.Hide();
                } else
                {
                    p.Close();
                }
            }
        }

        public DockContentCollection GetAllPanels()
        {
            return dockPanel.Contents;
        }

        public void DoAction(string action)
        {
            switch(action)
            {
                case "openProject":
                    NTOS.OpenProject();
                    break;
                case "projectExplorer":
                    projectExplorer.Show(dockPanel, DockState.DockLeft);
                    break;
                case "runEmulator":
                    dEmulator.Show(dockPanel, DockState.DockRight);
                    dEmulator.InitEmulator();
                    break;
                case "build":
                    CodeEditor.currentFile = null;
                    dEmulator.Show(dockPanel, DockState.DockRight);
                    dEmulator.InitEmulator();
                    dEmulator.BuildAll();
                    //serialManager.Show(dockPanel, DockState.DockBottom);
                    break;
                case "imageConverter":
                    dImage.Show(dockPanel, DockState.Document);
                    break;
                case "about":
                    new AboutForm().ShowDialog();
                    break;
                case "exit":
                    Close();
                    break;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
#if !DEBUG
            if (TBMessageBox.Confirm(this, "Project is not saved!", "Do you want to save this project?") == DialogResult.Yes)
            {
                DoAction("save");
            }
#else
            DoAction("save");
#endif
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
#if DEBUG
            // TranslationService.Save();
#endif
        }
    }
}
