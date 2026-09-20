
using NTOSCompiler;
using System.Diagnostics;

namespace NTOSDev
{
    public partial class Main : Form
    {
        FunctionRegistry functionRegistry = new FunctionRegistry();
        public Main()
        {
            InitializeComponent();

            
            functionRegistry.RegisterFunction("drawBox", (args) =>
            {
                Debug.WriteLine($"drawBox called with arguments: {args[0]}");
                return null;
            });
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var source = textBox1.Text;
                var lexer = new Lexer(source);
                var tokens = lexer.Tokenize();


                var parser = new Parser(tokens);
                var bytecode = parser.Compile();

                listBox1.Items.Clear();
                string bc = "";

                foreach (var instruction in bytecode.Instructions)
                {
                    var item = instruction.OpCode.ToString() + " " + (instruction.Operand?.ToString() ?? string.Empty);
                    listBox1.Items.Add(item);

                    bc += item + Environment.NewLine;
                }

                var vm = new VirtualMachine(functionRegistry);
                vm.Execute(bytecode);

                Debug.WriteLine("Bytecode:\n" + bc);
            }
            catch (Exception ex)
            {
                listBox1.Items.Clear();
                listBox1.Items.Add(ex.Message);
                Debug.WriteLine("Error:\n" + ex.Message);
            }
        }
    }
}
