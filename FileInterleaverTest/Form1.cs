using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FileInterleaverTest
{
    public partial class Form1 : Form
    {
        public static Form1 Instance;
        
        public Form1()
        {
            InitializeComponent();
            Instance = this;
            background.ProgressChanged += Background_ProgressChanged;
            background.RunWorkerCompleted += ProcessFinished;
        }

        private void Background_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult res = filesOpen.ShowDialog();
            if(res == DialogResult.OK)
            {
                foreach(string path in filesOpen.FileNames)
                {
                    if (File.Exists(path)) {
                        listBox1.Items.Add(path);
                    }
                }
                UpdateSize();
            }
        }

        private void UpdateSize()
        {
            long size = 0;
            foreach(object data in listBox1.Items)
            {
                string path = (string)data;
                if (File.Exists(path))
                {
                    size += (new FileInfo(path)).Length;
                }
            }
            label2.Text = size + " B";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult res = fileSave.ShowDialog();
            if(res == DialogResult.OK)
            {
                groupBox1.Enabled = false;
                background.RunWorkerAsync();
            }
        }
        
        void ProcessFinished(object sender, EventArgs e)
        {
            MessageBox.Show("File Interleaving Finished", "OK");
            groupBox1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<int> indices = new List<int>();
            foreach(object data in listBox1.SelectedItems)
            {
                indices.Add(listBox1.Items.IndexOf(data));
            }
            indices.Reverse();
            foreach(int index in indices)
            {
                try
                {
                    listBox1.Items.RemoveAt(index);
                }
                catch (Exception) { }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void background_DoWork(object sender, DoWorkEventArgs e)
        {
            string savePath = fileSave.FileName;
            List<string> filePaths = new List<string>();
            foreach (object obj in listBox1.Items)
            {
                try { filePaths.Add((string)obj); }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, exc.GetType().Name);
                }
            }
            MemoryStream interleavedStream = Interleaver.Interleave(filePaths, (BackgroundWorker)sender);
            interleavedStream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[256];
            using (FileStream saveStream = File.OpenWrite(savePath))
            {
                while (interleavedStream.Position < interleavedStream.Length)
                {
                    int readSize = interleavedStream.Read(buffer, 0, 256);
                    saveStream.Write(buffer, 0, readSize);
                }
            }
        }
    }
}
