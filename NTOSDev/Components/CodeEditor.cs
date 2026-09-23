using ScintillaNet.Abstractions.Classes;
using ScintillaNet.Abstractions.Enumerations;
using ScintillaNet.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace NTOSDev.Controls
{
    public partial class CodeEditor : DockContent
    {

        private Scintilla scintillaEditor;
        private bool fileSaved = false;
        public string filePath;

        public static string currentFile = string.Empty;

        public CodeEditor(string filePath)
        {
            InitializeComponent();
            InitializeScintilla();
            scintillaEditor.TextChanged += ScintillaEditor_TextChanged;
            if (File.Exists(filePath))
            {
                this.filePath = filePath;
                scintillaEditor.Text = File.ReadAllText(filePath);
                fileSaved = true;
                UpdateFileName();
            }

            scintillaEditor.GotFocus += ScintillaEditor_GotFocus;

        }

        private void ScintillaEditor_GotFocus(object? sender, EventArgs e)
        {
            currentFile = filePath;
        }

        private void ScintillaEditor_TextChanged(object? sender, EventArgs e)
        {
            fileSaved = false;
            UpdateFileName();
        }

        private void UpdateFileName()
        {
            Text = Path.GetFileName(filePath) + (fileSaved ? "" : "*");
        }

        private void InitializeScintilla()
        {
            scintillaEditor = new Scintilla();
            scintillaEditor.Dock = DockStyle.Fill;

            // Basic editor settings
            scintillaEditor.WrapMode = WrapMode.None;
            scintillaEditor.IndentWidth = 4;
            scintillaEditor.TabWidth = 4;
            scintillaEditor.UseTabs = true;

            // Line numbers
            scintillaEditor.Margins[0].Type = MarginType.Number;
            scintillaEditor.Margins[0].Width = 35;

            // Set font
            scintillaEditor.Styles[StyleConstants.Default].Font = "Consolas";
            scintillaEditor.Styles[StyleConstants.Default].Size = 11;
            scintillaEditor.StyleClearAll();

            // Lexer
            scintillaEditor.LexerName = "cpp"; // built-in JS lexer

            // --- COLORS --- (numeric style indexes from JS lexer)
            scintillaEditor.Styles[0].ForeColor = Color.Black;          // Default
            scintillaEditor.Styles[1].ForeColor = Color.Green;          // Comment
            scintillaEditor.Styles[2].ForeColor = Color.Green;          // Line comment
            scintillaEditor.Styles[3].ForeColor = Color.Brown;          // Double quoted string
            scintillaEditor.Styles[4].ForeColor = Color.Brown;          // Single quoted string
            scintillaEditor.Styles[5].ForeColor = Color.Blue;           // Keyword
            scintillaEditor.Styles[6].ForeColor = Color.Purple;         // Number
            scintillaEditor.Styles[7].ForeColor = Color.DarkOrange;     // Boolean/null
            scintillaEditor.Styles[8].ForeColor = Color.Teal;           // Identifier (e.g., Bot/Web/custom commands)
            scintillaEditor.Styles[9].ForeColor = Color.DarkMagenta;    // Operator / function call

            // Keywords
            scintillaEditor.SetKeywords(0, "Bot Web function return var let const if else for while break continue new");
            scintillaEditor.SetKeywords(1, "true false null undefined send wait Bot Web");

            this.Controls.Add(scintillaEditor);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                SaveFile();
                return true; // handled
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                File.WriteAllText(filePath, scintillaEditor.Text);
                fileSaved = true;
                UpdateFileName();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save file: " + ex.Message);
            }
        }

    }
}
