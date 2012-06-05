using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace DataGenerator
{
    /// <summary>
    /// Data file generator tool
    /// </summary>
    public partial class MainWindow : Window
    {
        private const char INPUT_DELIMITER = ',';
        private const string INPUT_FILE_EXTENSION = "csv";

        // Create a background worker to do the generation on another thread.
        BackgroundWorker bw = new BackgroundWorker();

        private bool templateValid = true;
        private bool dataValid = true;

        public MainWindow()
        {
            InitializeComponent();

            // Setup the background worker to do the generation on another thread.
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.WorkerReportsProgress = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get command line parameters.
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 3)
            {
                // Both files specified.
                textBox1.Text = args[1];
                textBox2.Text = args[2];
                generate(args[1], (args[2]));
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // Get a the template file path with an OpenFileDialog.
            textBox1.Text = FileHelper.getFilePathWithOpenFileDialog();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            // Get a the data file path with an OpenFileDialog.
            textBox2.Text = FileHelper.getFilePathWithOpenFileDialog(textBox2.Text, INPUT_FILE_EXTENSION);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            // Generate if both files specified.
            if (!String.IsNullOrEmpty(textBox1.Text) && !String.IsNullOrEmpty(textBox2.Text))
                generate(textBox1.Text, textBox2.Text);
        }

        private void generate(string templateFilePath, string dataFilePath)
        {
            // Check files exist.
            if (FileHelper.readOK(templateFilePath) && FileHelper.readOK(dataFilePath))
            {

                // Disable buttons.
                button1.IsEnabled = false;
                button2.IsEnabled = false;
                button3.IsEnabled = false;

                // Run generator.
                bw.RunWorkerAsync(new string[] { templateFilePath, dataFilePath });
            }
            else
            {
                MessageBox.Show("File read error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] args = e.Argument as string[];
            string templateFilePath = args[0];
            string dataFilePath = args[1];

            // Load and validate template.
            StreamReader templateFileReader;
            string template;
            try
            {
                // open file for input
                templateFileReader = new StreamReader(templateFilePath);
                template = templateFileReader.ReadToEnd();
                templateFileReader.Close();
                templateValid = true;
            }
            catch
            {
                templateValid = false;
                // exit
                return;
            }

            // Load and validate data.
            StreamReader dataFileReader;
            try
            {
                // open file for input
                dataFileReader = new StreamReader(dataFilePath);
                dataValid = true;
            }
            catch
            {
                dataValid = false;
                // exit
                return;
            }
            // Find total lines.
            long totalCount = File.ReadLines(dataFilePath).Count();

            // Holds the line of the input file being worked on.
            string line;
            string[] lineFields;
            string[] headerFields = null;
            int lineCount = 0; // counter for all lines processed

            // Process header line.
            try
            {
                // Loop around all the lines of the file.
                if (!dataFileReader.EndOfStream)
                {
                    // Load a line.
                    line = dataFileReader.ReadLine();
                    // Process the line.
                    headerFields = line.Split(INPUT_DELIMITER);
                    // Update progress.
                    lineCount++;
                    // Change count to percentage (1-100).
                    float progress = (((float)lineCount / (float)totalCount) * 100);
                    // Report progress.
                    bw.ReportProgress((int)progress);
                }

            }
            catch
            {
                dataValid = false;
                return;
            }

            // Process other lines and generate output files.
            try
            {
                StreamWriter o = new StreamWriter(headerFields[0]);

                // Loop around all the lines of the file.
                while (!dataFileReader.EndOfStream)
                {
                    // Load a line.
                    lineFields = dataFileReader.ReadLine().Split(INPUT_DELIMITER);
                    if (lineFields.Length != headerFields.Length) continue;

                    // Try to open the output file for writing.
                    try
                    {
                        if (!String.IsNullOrEmpty(lineFields[0]) && FileHelper.newOK(lineFields[0]))
                        {
                            o.Flush();
                            o.Close();
                            o = new StreamWriter(lineFields[0]);
                        }

                        // Genereate the content.
                        string output = template;
                        for (int i = 1; i < headerFields.Length; i++)
                        {
                            output = output.Replace(headerFields[i], lineFields[i]);
                        }

                        // Write the content.
                        o.Write(output);

                        // Update progress.
                        lineCount++;
                        // Change count to percentage (1-100).
                        float progress = (((float)lineCount / (float)totalCount) * 100);
                        // Report progress.
                        bw.ReportProgress((int)progress);
                    }
                    catch
                    {
                        continue;
                    }
                }
                o.Flush();
                o.Close();
            }
            catch
            {
                return;
            }
            finally
            {
                // Close the file.
                dataFileReader.Close();
            }

        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Update progress bar.
            pbLoad.Value = e.ProgressPercentage;
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Reset UI.
            pbLoad.Value = 0;
            button1.IsEnabled = true;
            button2.IsEnabled = true;
            button3.IsEnabled = true;
            if (!templateValid)
            {
                MessageBox.Show("Template file is not valid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (!dataValid)
            {
                MessageBox.Show("Data file is not valid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
