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
using Microsoft.VisualBasic;

namespace PuppetMaster
{

    public partial class Form1 : Form
    {

        List<NodeData> nodesList = new List<NodeData>();

        PuppetMaster pm = null;

        delegate void FormInvoke();
        public Form1()
        {
            InitializeComponent();

            int port = Int32.Parse(Interaction.InputBox("Set Puppet Master port:", "PuppetMaster", "20001"));


            pm = new PuppetMaster(port);
            pm.NewWorkerEvent += onNewWorkerEvent;


            pm.JoinEvent += (sender, url) =>
            {
                listView2.Invoke((FormInvoke)delegate
                {
                    ListViewItem item = new ListViewItem(url);
                    item.SubItems.Add("No");
                    item.SubItems.Add("No");
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");


                    listView2.Items.Add(item);


                });
            };
            pm.DisconectedEvent += (sender, url) =>
            {
                listView2.Invoke((FormInvoke)delegate
                {
                    ListViewItem remove = null;
                    foreach (ListViewItem l in listView2.Items)
                    {
                        Console.WriteLine(l.Text + "=?" + url);
                        if (l.Text.Equals(url))
                        {
                            remove = l;
                            break;
                        }
                    }

                    if (remove != null)
                        listView2.Items.Remove(remove);

                });
            };



            pm.WorkStartEvent += (sender, peer, split, clientURL) =>
            {
                listView2.Invoke((FormInvoke)delegate
                {
                    ListViewItem updateIV = null;
                    foreach (ListViewItem l in listView2.Items)
                    {
                        Console.WriteLine(l.Text + "=?" + peer);
                        if (l.Text.Equals(peer))
                        {
                            updateIV = l;
                            break;
                        }
                    }

                    if (updateIV != null)
                    {

                        updateIV.SubItems[1].Text = "No";
                        updateIV.SubItems[2].Text = "YES";
                        updateIV.SubItems[3].Text = "" + split;
                        updateIV.SubItems[4].Text = clientURL;
                    }
                });
            };


            pm.WorkEndEvent += (sender, peer) =>
            {

                listView2.Invoke((FormInvoke)delegate
                {
                    ListViewItem updateIV = null;
                    foreach (ListViewItem l in listView2.Items)
                    {
                        Console.WriteLine(l.Text + "=?" + peer);
                        if (l.Text.Equals(peer))
                        {
                            updateIV = l;
                            break;
                        }
                    }

                    if (updateIV != null)
                    {

                        updateIV.SubItems[1].Text = "No";
                        updateIV.SubItems[2].Text = "No";
                        updateIV.SubItems[3].Text = "-";
                        updateIV.SubItems[4].Text = "-";
                    }
                });
            };


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

                listView1.Clear();
                step = 0;
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
            else
            {
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
            textBox_console.Text = "";

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

        private void label3_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "WAIT <SECS>";
        }

        private void label4_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "STATUS";
        }

        private void label5_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "SLOWW <ID> <SECS>";
        }

        private void label6_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "FREEZEW <ID>";
        }

        private void label7_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "UNFREEZEW <ID>";
        }

        private void label8_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>";
        }

        private void label9_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "FREEZEC <ID>";
        }

        private void label11_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "UNFREEZEC <ID>";
        }

        private void label10_Click(object sender, EventArgs e)
        {
            textBox_console.Text = "SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP>:";
        }
    }
}
