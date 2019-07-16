using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BMWTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Application.ApplicationExit += Application_ApplicationExit;
            txtSource.Text = Properties.Settings.Default.Source;
            txtDestination.Text = Properties.Settings.Default.Destination;
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            saveProperties();
        }
        private void saveProperties()
        {
            if (!string.IsNullOrEmpty(txtSource.Text))
                Properties.Settings.Default.Source = txtSource.Text;
            if (!string.IsNullOrEmpty(txtDestination.Text))
                Properties.Settings.Default.Destination = txtDestination.Text;
            Properties.Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            doDialogBrowse(sourceFolderDialog, txtSource);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            doDialogBrowse(destinationFolderDialog, txtDestination);
        }
        private void doDialogBrowse(FolderBrowserDialog dialog, TextBox toPopulate){
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                toPopulate.Text = dialog.SelectedPath;
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            DoCopy copy = new DoCopy(txtSource.Text, txtDestination.Text, checkBox1.Checked, checkBox2.Checked);
            copy.PropertyChanged += copy_PropertyChanged;
            copy.doCopy();
        }

        void copy_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            progressBar1.Value = ((DoCopy)sender).fileCopyPercentage;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            saveProperties();
            Application.Exit();
        }
    }
}
