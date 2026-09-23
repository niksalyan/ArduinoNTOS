
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


            vmFunctions.AddFunction(0, "debug", (args) =>
            {
                debugOutput.Text += args[0] + Environment.NewLine;
                return null;
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


                var parser = new Parser(tokens, vmFunctions);
                var program = parser.Compile();

                listBox1.Items.Clear();
                string bc = "";

                foreach (var instruction in program.Instructions)
                {
                    var item = instruction.OpCode.ToString() + " " + (instruction.Operand?.ToString() ?? string.Empty) + "  Addr:" + instruction.Address;
                    listBox1.Items.Add(item);

                    bc += item + Environment.NewLine;
                }

                var vm = new VirtualMachine(4096, vmFunctions);
                vm.Execute(program.Bytecode);

                Debug.WriteLine("Bytecode:\n" + bc);
                Debug.WriteLine(string.Join(" ", program.Bytecode.Select(b => b.ToString("X2"))));

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
