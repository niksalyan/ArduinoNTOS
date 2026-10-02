using NTOSDev.Components;
using ScintillaNet.Abstractions.Classes;
using ScintillaNet.Abstractions.Enumerations;
using ScintillaNet.Abstractions.Extensions;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                //// Select indicator #8 for compiler errors
                //scintillaEditor.IndicatorCurrent = 8;

                //// Make it a squiggly underline
                //scintillaEditor.Indicators[8].Style = IndicatorStyle.Squiggle;

                //// Red
                //scintillaEditor.Indicators[8].ForeColor = Color.Red;

                //// Apply to the error position
                //scintillaEditor.IndicatorFillRange(1500, 10);

                //scintillaEditor.CharPositionFromPoint(15, 2); // Move cursor to the start of the document
                //scintillaEditor.IndicatorFillRange(200, 1);


            }

            scintillaEditor.GotFocus += ScintillaEditor_GotFocus;

        }

        private void ScintillaEditor_GotFocus(object? sender, EventArgs e)
        {
            ReloadEmulator();
        }

        private void ReloadEmulator(bool force = false)
        {
            if (currentFile == filePath && !force) return;
            currentFile = filePath;
            DEmulator.RefreshEmulator();
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
            scintillaEditor.BorderStyle = BorderStyle.None;
            scintillaEditor.Dock = DockStyle.Fill;

            scintillaEditor.WrapMode = WrapMode.None;
            scintillaEditor.IndentWidth = 4;
            scintillaEditor.TabWidth = 4;
            scintillaEditor.UseTabs = true;

            scintillaEditor.Margins[0].Type = MarginType.Number;
            scintillaEditor.Margins[0].Width = 40;

            scintillaEditor.Styles[StyleConstants.Default].Font = "Consolas";
            scintillaEditor.Styles[StyleConstants.Default].Size = 11;
            scintillaEditor.Styles[StyleConstants.Default].ForeColor =
                Color.FromArgb(220, 220, 220);
            scintillaEditor.Styles[StyleConstants.Default].BackColor =
                Color.FromArgb(30, 30, 30);

            // Your Scintilla build has the C++ lexer
            scintillaEditor.LexerName = "cpp";

            scintillaEditor.StyleClearAll();

            // Default
            scintillaEditor.Styles[0].ForeColor =
                Color.FromArgb(220, 220, 220);
            scintillaEditor.Styles[0].BackColor =
                Color.FromArgb(30, 30, 30);

            // Comments
            scintillaEditor.Styles[1].ForeColor =
                Color.FromArgb(106, 153, 85);

            scintillaEditor.Styles[2].ForeColor =
                Color.FromArgb(106, 153, 85);

            // Numbers
            scintillaEditor.Styles[4].ForeColor =
                Color.FromArgb(181, 206, 168);

            // Keywords
            scintillaEditor.Styles[5].ForeColor =
                Color.FromArgb(86, 156, 214);

            // Strings
            scintillaEditor.Styles[6].ForeColor =
                Color.FromArgb(206, 145, 120);

            // Character
            scintillaEditor.Styles[7].ForeColor =
                Color.FromArgb(206, 145, 120);

            // Operators
            scintillaEditor.Styles[10].ForeColor =
                Color.FromArgb(220, 220, 170);

            // C++ lexer keywords, but populated with our NTOS/JS vocabulary
            scintillaEditor.SetKeywords(
                0,
                @$"
using break case const continue debugg default delete do else export extends false finally for
from function get if import in instanceof let new null of return set static super switch this
throw true try typeof var void while with yield
drawBox fillBox cursor print printCentered printRight delay confirm alert confirmNumber editText dialog loadStr saveStr
cls fillCircle drawCircle drawPixel drawLine load drawSprite collision
"
            );


            // ---------------------------------------------------------
            // Cursor
            // ---------------------------------------------------------

            scintillaEditor.CaretForeColor =
                Color.FromArgb(255, 255, 255);

            scintillaEditor.CaretWidth = 2;

            // Do not highlight the entire current line.
            scintillaEditor.CaretLineVisible = false;


            // ---------------------------------------------------------
            // Line number bar
            // ---------------------------------------------------------

            scintillaEditor.Styles[StyleConstants.LineNumber].ForeColor =
                Color.FromArgb(64, 64, 64);

            scintillaEditor.Styles[StyleConstants.LineNumber].BackColor =
                Color.FromArgb(30, 30, 30);

            scintillaEditor.Styles[StyleConstants.LineNumber].Size = 9;




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
                ReloadEmulator(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save file: " + ex.Message);
            }
        }

    }
}
