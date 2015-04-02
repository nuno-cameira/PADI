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
            string s = textBox_script.Text;
            try
            {
                pm.loadScript(s);
                // TODO should this be here?
                button_run.Enabled = true;
                button_step.Enabled = true;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File doesn't exist");
            }
        }

        private void button_step_Click(object sender, EventArgs e)
        {
            if (pm.readLine() == null)
            {
                button_step.Enabled = false;
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






    }
}
