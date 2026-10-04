using NTOSDev.Components;
using NTOSDev.Controls;
using NTOSEmulator;
using System.Diagnostics;
using WeifenLuo.WinFormsUI.Docking;

namespace NTOSDev
{
    public partial class Main : Form
    {

        public ProjectExplorer projectExplorer = new ProjectExplorer()
        {
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

        public SpriteViewer spriteViewer = new SpriteViewer()
        {
            HideOnClose = true
        };

        public Instructions dInstructions = new Instructions()
        {
            HideOnClose = true
        };

        public Variables dVariables = new Variables()
        {
            HideOnClose = true
        };

        public Terminal dTerminal = new Terminal()
        {
            HideOnClose = true
        };

        public ComUploader comUploader = new ComUploader()
        {
            HideOnClose = true
        };

        private string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private string appLayoutFile;

        public Main()
        {
            NTOS.Main = this;
            appLayoutFile = Path.Combine(appDataPath, "NTOSDev", "layout.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(appLayoutFile));
            Debug.WriteLine($"Layout file path: {appLayoutFile}");
            InitializeComponent();
            dockPanel.Theme = NTOS.Theme;
            try
            {
                var deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
                dockPanel.LoadFromXml(appLayoutFile, deserializeDockContent);
            }
            catch { } // Ignore if layout file doesn't exist or is invalid

            AttachMenuHandlers(menuStrip.Items);
        }

        private void Main_Load(object sender, EventArgs e)
        {

            // new TerminalControl().Show(dockPanel, DockState.DockBottom);

            NTOS.Reload += (s) =>
            {
                CloseAllPanels(typeof(CodeEditor));
                projectExplorer.LoadFolder(s);
                DoAction("projectExplorer");
                DoAction("runEmulator");

                string appName = new DirectoryInfo(s).Name;
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

function draw() {{
    dialog(""{appName}"");
}}

draw();

function loop() {{
    // This function is called every frame
    var key = getKey();
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
                }
                catch (Exception ex)
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

        private IDockContent GetContentFromPersistString(string persistString)
        {
            // Tool windows: return existing instances so state is preserved
            if (persistString == typeof(ProjectExplorer).ToString()) return projectExplorer;
            if (persistString == typeof(DEmulator).ToString()) return dEmulator;
            if (persistString == typeof(DImage).ToString()) return dImage;
            if (persistString == typeof(SpriteViewer).ToString()) return spriteViewer;
            if (persistString == typeof(Instructions).ToString()) return dInstructions;
            if (persistString == typeof(Variables).ToString()) return dVariables;
            if (persistString == typeof(Terminal).ToString()) return dTerminal;
            if (persistString == typeof(ComUploader).ToString()) return comUploader;

            // Documents: try to extract a file path after a separator (comma or pipe)
            try
            {
                if (persistString.StartsWith(typeof(Controls.CodeEditor).ToString()) || persistString.Contains("CodeEditor"))
                {
                    string path = null;
                    int idx = persistString.IndexOf(',');
                    if (idx >= 0 && persistString.Length > idx + 1)
                        path = persistString.Substring(idx + 1).Trim();
                    else
                    {
                        idx = persistString.IndexOf('|');
                        if (idx >= 0 && persistString.Length > idx + 1)
                            path = persistString.Substring(idx + 1).Trim();
                    }

                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        return new Controls.CodeEditor(path);
                    return null;
                }

                if (persistString.StartsWith(typeof(Components.FileViewer).ToString()) || persistString.Contains("FileViewer"))
                {
                    string path = null;
                    int idx = persistString.IndexOf(',');
                    if (idx >= 0 && persistString.Length > idx + 1)
                        path = persistString.Substring(idx + 1).Trim();
                    else
                    {
                        idx = persistString.IndexOf('|');
                        if (idx >= 0 && persistString.Length > idx + 1)
                            path = persistString.Substring(idx + 1).Trim();
                    }

                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        return new Components.FileViewer(path);
                    return null;
                }
            }
            catch { }

            return null;
        }


        public void OpenFile(string filePath)
        {
            if (Path.GetExtension(filePath)?.ToLower() == ".js" || Path.GetExtension(filePath)?.ToLower() == ".ntx")
            {
                new CodeEditor(filePath).Show(dockPanel, DockState.Document);
            }
            else
            {
                new FileViewer(filePath).Show(dockPanel, DockState.Document);
            }

        }

        public void CloseAllPanels(Type type = null)
        {
            var contents = dockPanel.Contents.ToArray();
            foreach (DockContent p in contents)
            {
                if (type == null || p.GetType() == type)
                {
                    if (p.HideOnClose)
                    {
                        p.Hide();
                    }
                    else
                    {
                        p.Close();
                    }
                }
            }
        }

        public DockContentCollection GetAllPanels()
        {
            return dockPanel.Contents;
        }

        public void DoAction(string action)
        {
            switch (action)
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
                case "spriteViewer":
                    spriteViewer.Show(dockPanel, DockState.Document);
                    break;
                case "instructions":
                    dInstructions.Show(dockPanel, DockState.DockRight);
                    break;
                case "variables":
                    dVariables.Show(dockPanel, DockState.DockRight);
                    break;
                case "debugOutput":
                    dTerminal.Show(dockPanel, DockState.DockBottom);
                    break;
                case "about":
                    new AboutForm().ShowDialog();
                    break;
                case "upload":
                    comUploader.Show(dockPanel, DockState.DockBottom);
                    break;
                case "exit":
                    Close();
                    break;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                dockPanel.SaveAsXml(appLayoutFile);
            }
            catch { }
#if !DEBUG
            if (TBMessageBox.Confirm(this, "Project is not saved!", "Do you want to save this project?") == DialogResult.Yes)
            {
                // DoAction("save");
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
