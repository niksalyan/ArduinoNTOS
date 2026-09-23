using NTOSCompiler;
using NTOSCompiler.Compiler;
using NTOSDev.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NTOSDev.Components
{
    public partial class DEmulator : DockContent
    {

        public DEmulator()
        {
            InitializeComponent();
            Text = "Emulator";
            HideOnClose = true;


            InitEmulator();
        }

        public void InitEmulator()
        {
            try
            {
                if (File.Exists(CodeEditor.currentFile))
                {
                    /*Debug.WriteLine("Loading: " + CodeEditor.currentFile);
                    string source = File.ReadAllText(CodeEditor.currentFile);

                    var lexer = new Lexer(source);
                    var tokens = lexer.Tokenize();


                    var parser = new Parser(tokens, emulator.vmFunctions);
                    var program = parser.Compile();

                    string bc = "";
                    foreach (var instruction in program.Instructions)
                    {
                        var item = instruction.OpCode.ToString() + " " + (instruction.Operand?.ToString() ?? string.Empty) + "  Addr:" + instruction.Address;
                        // listBox1.Items.Add(item);

                        bc += item + Environment.NewLine;
                    }

                    Debug.WriteLine("Bytecode:\n" + bc);
                    Debug.WriteLine(string.Join(" ", program.Bytecode.Select(b => b.ToString("X2"))));*/

                    string fileName = Path.GetFileNameWithoutExtension(CodeEditor.currentFile);
                    string source = File.ReadAllText(CodeEditor.currentFile);
                    emulator.Execute(fileName, source);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
    }
}
