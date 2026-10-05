using NTOSEmulator;
using NTOSEmulator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace NTOSDev.Components
{
    public partial class ComUploader : DockContent
    {
        private static SerialPort serial = new SerialPort();
        private bool isUploading = false;

        private BindingList<DebugLine> uploadLog = new BindingList<DebugLine>();
        private readonly StringBuilder serialReceiveBuffer = new StringBuilder();

        public ComUploader()
        {
            InitializeComponent();
            serial.BaudRate = 4800;
            serial.DataReceived += Serial_DataReceived;
            dataGridView1.DataSource = uploadLog;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
        }


        private void ComUploader_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comPortsList.Items.Clear();
            foreach (var port in ports)
            {
                comPortsList.Items.Add(port);
            }
            UpdateConsole();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serial.IsOpen)
                {
                    serial.PortName = comPortsList.Text;
                    serial.BaudRate = 4800;
                    serial.Open();
                    AddLog(new DebugLine("Connected to: " + comPortsList.Text));
                }
                else
                {
                    serial.Close();
                    AddLog(new DebugLine("Disconnected from: " + comPortsList.Text));
                }

            }
            catch (Exception ex)
            {
                AddLog(new DebugLine("Error: " + ex.Message, true));
            }
            UpdateConsole();
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            uploadLog.Clear();
            Task.Run(DoUpload);
        }

        private async Task DoUpload()
        {
            if (isUploading) return;

            isUploading = true;
            UpdateConsole();

            try
            {
                Emulator.BuildAll(true);
                Emulator.CopyNtiFiles(true);
                string appName = new DirectoryInfo(Emulator.AppPath).Name;

                string[] files =
                    Directory.GetFiles(Emulator.AppPath + "build")
                        .Where(f =>
                            f.EndsWith(".ntx", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".nti", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                AddLog(new DebugLine("UPLOADING: " + appName));
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {

                        string name = Path.GetFileName(file);
                        byte[] bytecode = File.ReadAllBytes(file);
                        var sb = new StringBuilder();

                        for (int i = 0; i < bytecode.Length; i++)
                        {
                            sb.Append($"{bytecode[i]:X2}");
                        }

                        AddLog(new DebugLine("-> " + name + " (" + bytecode.Length + "b)"));

                        var sbHex = sb.ToString();

                        string cmd1 = "U " + appName + " " + name;
                        serial.WriteLine(cmd1);

                        await Task.Delay(1000);


                        string cmd2 = "<" + sbHex + ">";
                        serial.WriteLine(cmd2);

                        await Task.Delay(2000);




                    }
                }

                await Task.Delay(2000);
                AddLog(new DebugLine("DONE"));


            }
            catch (Exception ex)
            {
                AddLog(new DebugLine("Upload Error: " + ex.Message, true));
            }
            finally
            {
                isUploading = false;
                UpdateConsole();
            }
        }

        private void AddLog(DebugLine line)
        {
            BeginInvoke(() =>
            {
                uploadLog.Add(line);
            });
        }

        private void UpdateConsole()
        {
            BeginInvoke(() =>
            {
                uploadButton.Enabled = serial.IsOpen;
                comPortsList.Enabled = !serial.IsOpen;
                connectButton.Text = serial.IsOpen ? "Disconnect" : "Connect";
                uploadButton.Enabled = !isUploading && serial.IsOpen;
                uploadButton.Text = isUploading ? "Uploading" : "Upload";
            });

        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serial.ReadExisting();

                lock (serialReceiveBuffer)
                {
                    serialReceiveBuffer.Append(data);

                    while (true)
                    {
                        string buffer = serialReceiveBuffer.ToString();

                        int newlineIndex = buffer.IndexOf('\n');

                        if (newlineIndex < 0)
                            break;

                        string line = buffer[..newlineIndex];

                        // Remove the processed line including \n
                        serialReceiveBuffer.Remove(0, newlineIndex + 1);

                        // Handle possible \r\n
                        line = line.TrimEnd('\r');

                        AddLog(new DebugLine("[SERIAL] " + line));
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog(new DebugLine("Serial receive error: " + ex.Message, true));
            }
        }


        private void dataGridView1_CellFormatting(
                object sender,
                DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            if (dataGridView1.Rows[e.RowIndex].DataBoundItem is DebugLine debugLine &&
                debugLine.IsError)
            {

                e.CellStyle.ForeColor = Color.Red;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            }

        }

    }
}
