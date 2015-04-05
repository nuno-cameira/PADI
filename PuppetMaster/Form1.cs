using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{

    public partial class Form1 : Form
    {

        List<NodeData> nodesList = new List<NodeData>();

        PuppetMaster pm = new PuppetMaster();

        public Form1()
        {
            InitializeComponent();

            pm.NewWorkerEvent += onNewWorkerEvent;



            


        }


        private void onNewWorkerEvent(string nodeUrl)
        {
            //MessageBox.Show("ADDED " + nodeUrl);
            nodesList.Add(new NodeData(nodeUrl));
        }



        #region "User Interface"

        private void textBox_script_TextChanged(object sender, EventArgs e)
        {
            button_loadScript_canExecute();
        }

        private void button_loadScript_canExecute()
        {
            bool canExecute = !String.IsNullOrEmpty(textBox_script.Text);

            button_loadScript.Enabled = canExecute;
        }

        #endregion


        private void button_loadScript_Click(object sender, EventArgs e)
        {
            string scriptName = textBox_script.Text;
            try
            {
                pm.loadScript(scriptName);


                using (StreamReader reader = File.OpenText(scriptName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        listView1.Items.Add(new ListViewItem(line));
                    }
                }

                // TODO should this be here?
                button_run.Enabled = true;
                button_step.Enabled = true;

                label_loadedScript.Text = "Script Loaded: \"" + scriptName + "\"";
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File doesn't exist");
            }
        }

        int step = 0;
        private void button_step_Click(object sender, EventArgs e)
        {
            if (pm.readLine() == null)
            {
                button_step.Enabled = false;
            }
            else {
                listView1.Items[step].Font = new Font(listView1.Font, FontStyle.Strikeout);
                step++;
            }
        }

        private void button_run_Click(object sender, EventArgs e)
        {
            pm.parser();
            button_run.Enabled = false;
        }

        private void button_submit_Click(object sender, EventArgs e)
        {
            pm.executeCommand(textBox_console.Text);
        }

        private void button_openScript_Click(object sender, EventArgs e)
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            //openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            //openFileDialog1.FilterIndex = 1;

            openFileDialog1.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            DialogResult userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == DialogResult.OK)
            {
                textBox_script.Text = openFileDialog1.FileName;
            }
        }













    }
}
