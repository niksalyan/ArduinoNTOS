
using NTOSCompiler;
using NTOSCompiler.Compiler;
using System.Diagnostics;

namespace NTOSDev
{
    public partial class Main : Form
    {
        VMFunctions vmFunctions = new VMFunctions();
        public Main()
        {
            InitializeComponent();


            vmFunctions.AddFunction(0, "debug", VariableType.Void, (args) =>
            {
                debugOutput.Text += args[0] + Environment.NewLine;
                return null;
            });

            vmFunctions.AddFunction(1, "add", VariableType.Int, (args) =>
            {
                return (int)args[0] + (int)args[1];
            });
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                debugOutput.Text = "";
                var source = textBox1.Text;
                var lexer = new Lexer(source);
                var tokens = lexer.Tokenize();

                foreach (var token in tokens)
                {
                    Debug.WriteLine($"{token.Kind}: {token.Text}");
                }

                var parser = new Parser(tokens, vmFunctions);
                var program = parser.Compile();

                listBox1.Items.Clear();
                string bc = "";

                foreach (var instruction in program.Instructions)
                {
                    var item = instruction.OpCode.ToString() + " " + (instruction.Operand?.ToString() ?? string.Empty);
                    listBox1.Items.Add(item);

                    bc += item + Environment.NewLine;
                }

                var vm = new VirtualMachine(4096, vmFunctions);
                vm.Execute(program.ToByteCode());

                Debug.WriteLine("Bytecode:\n" + bc);
            }
            catch (Exception ex)
            {
                listBox1.Items.Clear();
                debugOutput.Text = "Error: " + ex.Message;
                Debug.WriteLine("Error:\n" + ex.Message);
            }
        }
    }
}
