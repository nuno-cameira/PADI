using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class Form1 : Form
    {

        PuppetMaster pm = new PuppetMaster();

        public Form1()
        {
            InitializeComponent();
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
            pm.loadScript(s);
            pm.parser();
        }
    }
}
