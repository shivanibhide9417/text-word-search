/* Written by Shivani Bhide for CS 6326, Assignment 4.
  NetID :ssb180002
  Title :Text Search.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ssb180002Asg4
{
    public partial class Form1 : Form
    {
        private int lineNo;
        Queue<string> data = new Queue<string>();  // The queue is the data structure that will be accessed by the main thread and the background worker for exchanging information.

        //Form constructor
        public Form1()
        {
            InitializeComponent();
            display("Select input file to search from.");
            SearchButton.Enabled = false;
            progressBar.Visible = false;
            // int h = Screen.PrimaryScreen.WorkingArea.Height ;
            // int w = Screen.PrimaryScreen.WorkingArea.Width;
            // this.ClientSize = new Size(w, h);
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
          
        }

        // Function to display appropriate message on the status bar.
        private void display(String msg)
        {
            toolStripStatusLabel1.Text = msg;
            StatusBar.Refresh();
        }

        //Action on click of browse button
        private void BrowseButton_Click(object sender, EventArgs e)
        {
           openFileDialog.Filter = "Text files|*.txt";
            openFileDialog.Title = "Select a Report File to Analyze";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileTextBox.Text = openFileDialog.FileName;
                display("Enter a text to search in file");
            }
        }

        //Action on click of search button. 
        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (String.Equals(SearchButton.Text, "Search"))
            {
                progressBar.Visible = true;
                SearchButton.Text = "Cancel";
                ListView.Items.Clear();
                display("Reading input file");
                BrowseButton.Enabled = false;
                Thread.Sleep(1000); // wait for emulation of a long document
                BackgroundWorker.RunWorkerAsync();
            }
            else
            {
                display("Reading action Cancelled.");
                SearchButton.Text = "Search";
                BackgroundWorker.CancelAsync();
            }
        }

        //Background worker is used for reading the file, calculating the progress and reporting it back to the UI.
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            String line = String.Empty;
            StreamReader file = new StreamReader(FileTextBox.Text);
            long fileSize = new FileInfo(FileTextBox.Text).Length; // get the size of file from file properties so that the read status can be coverted to percentage accurately
            Debug.Print("File size is: " + fileSize.ToString());
            long bytesRead = 0;
            int count = 0;
            while ((line = file.ReadLine()) != null)
            {
                bytesRead = bytesRead + line.Length;
                if (BackgroundWorker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                if (line.IndexOf(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lineNo = count;
                    data.Enqueue(line);
                    Debug.Print(bytesRead.ToString());
                    BackgroundWorker.ReportProgress(Convert.ToInt32((bytesRead * 100) / fileSize));
                    Thread.Sleep(100);
                }
                count = count + 1;
            }
        }

        //Function to display found data in the list view and update the progress change
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
             ListViewItem lvi = new ListViewItem(lineNo.ToString());
            if (data.Count != 0)
            {
                lvi.SubItems.Add(data.Dequeue());
                ListView.Items.Add(lvi);
               
            }
            Queue<string> numbers = new Queue<string>();
            display("Searching file : "+e.ProgressPercentage.ToString() + " % complete ");
            progressBar.Value = e.ProgressPercentage;
            
        }


        //Validate entry in search text field
        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(FileTextBox.Text)  
                && !String.IsNullOrWhiteSpace(SearchTextBox.Text)
                && System.IO.File.Exists(FileTextBox.Text))
            {
                SearchButton.Enabled = true;
            }
            else
            {
                SearchButton.Enabled = false;
            }
        }

        //File read is complete. Reset all inputs.
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                display("Reading cancelled");
                data.Clear();
            }
            else if (e.Error != null)
            {
                display(e.Error.Message);
            }
            else
            {
                if (ListView.Items.Count <= 0) // No matching string is found for the given input.
                {
                    display("No match found for the entered string.");
                    progressBar.Visible = false;
                }
                else
                {
                    display("Text search complete");
                    progressBar.Visible = false;
                }

            }

            BrowseButton.Enabled = true;
            SearchButton.Text = "Search";
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void StatusBar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void FileTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}