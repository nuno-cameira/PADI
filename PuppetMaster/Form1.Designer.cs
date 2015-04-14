namespace PuppetMaster
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox_script = new System.Windows.Forms.TextBox();
            this.button_loadScript = new System.Windows.Forms.Button();
            this.button_submit = new System.Windows.Forms.Button();
            this.textBox_console = new System.Windows.Forms.TextBox();
            this.label_script = new System.Windows.Forms.Label();
            this.button_step = new System.Windows.Forms.Button();
            this.button_run = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label_loadedScript = new System.Windows.Forms.Label();
            this.button_openScript = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.listView2 = new System.Windows.Forms.ListView();
            this.ID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.isTracker = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.isWorking = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clientURL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_script
            // 
            this.textBox_script.Location = new System.Drawing.Point(12, 25);
            this.textBox_script.Multiline = true;
            this.textBox_script.Name = "textBox_script";
            this.textBox_script.Size = new System.Drawing.Size(573, 25);
            this.textBox_script.TabIndex = 0;
            this.textBox_script.Text = "Script.txt";
            this.textBox_script.TextChanged += new System.EventHandler(this.textBox_script_TextChanged);
            // 
            // button_loadScript
            // 
            this.button_loadScript.Location = new System.Drawing.Point(672, 25);
            this.button_loadScript.Name = "button_loadScript";
            this.button_loadScript.Size = new System.Drawing.Size(75, 25);
            this.button_loadScript.TabIndex = 1;
            this.button_loadScript.Text = "Load Script";
            this.button_loadScript.UseVisualStyleBackColor = true;
            this.button_loadScript.Click += new System.EventHandler(this.button_loadScript_Click);
            // 
            // button_submit
            // 
            this.button_submit.Location = new System.Drawing.Point(672, 373);
            this.button_submit.Name = "button_submit";
            this.button_submit.Size = new System.Drawing.Size(75, 25);
            this.button_submit.TabIndex = 3;
            this.button_submit.Text = "Submit";
            this.button_submit.UseVisualStyleBackColor = true;
            this.button_submit.Click += new System.EventHandler(this.button_submit_Click);
            // 
            // textBox_console
            // 
            this.textBox_console.Location = new System.Drawing.Point(12, 373);
            this.textBox_console.Multiline = true;
            this.textBox_console.Name = "textBox_console";
            this.textBox_console.Size = new System.Drawing.Size(651, 25);
            this.textBox_console.TabIndex = 4;
            // 
            // label_script
            // 
            this.label_script.AutoSize = true;
            this.label_script.Location = new System.Drawing.Point(12, 9);
            this.label_script.Name = "label_script";
            this.label_script.Size = new System.Drawing.Size(131, 13);
            this.label_script.TabIndex = 5;
            this.label_script.Text = "Name of the script to load:";
            // 
            // button_step
            // 
            this.button_step.Enabled = false;
            this.button_step.Location = new System.Drawing.Point(15, 69);
            this.button_step.Name = "button_step";
            this.button_step.Size = new System.Drawing.Size(75, 25);
            this.button_step.TabIndex = 6;
            this.button_step.Text = "Step";
            this.button_step.UseVisualStyleBackColor = true;
            this.button_step.Click += new System.EventHandler(this.button_step_Click);
            // 
            // button_run
            // 
            this.button_run.Enabled = false;
            this.button_run.Location = new System.Drawing.Point(94, 69);
            this.button_run.Name = "button_run";
            this.button_run.Size = new System.Drawing.Size(75, 25);
            this.button_run.TabIndex = 7;
            this.button_run.Text = "Run";
            this.button_run.UseVisualStyleBackColor = true;
            this.button_run.Click += new System.EventHandler(this.button_run_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(248, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Runing options (step by step or without interruption)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 357);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Console";
            // 
            // label_loadedScript
            // 
            this.label_loadedScript.AutoSize = true;
            this.label_loadedScript.Location = new System.Drawing.Point(12, 439);
            this.label_loadedScript.Name = "label_loadedScript";
            this.label_loadedScript.Size = new System.Drawing.Size(0, 13);
            this.label_loadedScript.TabIndex = 10;
            // 
            // button_openScript
            // 
            this.button_openScript.Location = new System.Drawing.Point(591, 25);
            this.button_openScript.Name = "button_openScript";
            this.button_openScript.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.button_openScript.Size = new System.Drawing.Size(75, 25);
            this.button_openScript.TabIndex = 11;
            this.button_openScript.Text = "Open script";
            this.button_openScript.UseVisualStyleBackColor = true;
            this.button_openScript.Click += new System.EventHandler(this.button_openScript_Click);
            // 
            // listView1
            // 
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(15, 100);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(335, 254);
            this.listView1.TabIndex = 12;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.List;
            // 
            // listView2
            // 
            this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ID,
            this.isTracker,
            this.isWorking,
            this.splitNumber,
            this.clientURL});
            this.listView2.Location = new System.Drawing.Point(356, 69);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(391, 285);
            this.listView2.TabIndex = 13;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.Details;
            // 
            // ID
            // 
            this.ID.Text = "ID";
            // 
            // isTracker
            // 
            this.isTracker.Text = "isTracker";
            // 
            // isWorking
            // 
            this.isWorking.Text = "isWorking";
            this.isWorking.Width = 76;
            // 
            // splitNumber
            // 
            this.splitNumber.Text = "splitNumber";
            this.splitNumber.Width = 98;
            // 
            // clientURL
            // 
            this.clientURL.Text = "clientURL";
            this.clientURL.Width = 88;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(528, 405);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "WAIT";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(128, 405);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "STATUS";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(184, 405);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "SLOWW";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(240, 405);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "FREEZEW";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(306, 405);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "UNFREEZEW";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 405);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "WORKER";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(388, 405);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "FREEZEC";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(74, 405);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "SUBMIT";
            this.label10.Click += new System.EventHandler(this.label10_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(450, 405);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 13);
            this.label11.TabIndex = 16;
            this.label11.Text = "UNFREEZEC";
            this.label11.Click += new System.EventHandler(this.label11_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(759, 461);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.button_openScript);
            this.Controls.Add(this.label_loadedScript);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_run);
            this.Controls.Add(this.button_step);
            this.Controls.Add(this.label_script);
            this.Controls.Add(this.textBox_console);
            this.Controls.Add(this.button_submit);
            this.Controls.Add(this.button_loadScript);
            this.Controls.Add(this.textBox_script);
            this.Name = "Form1";
            this.Text = "PuppetMaster";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_script;
        private System.Windows.Forms.Button button_loadScript;
        private System.Windows.Forms.Button button_submit;
        private System.Windows.Forms.TextBox textBox_console;
        private System.Windows.Forms.Label label_script;
        private System.Windows.Forms.Button button_step;
        private System.Windows.Forms.Button button_run;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_loadedScript;
        private System.Windows.Forms.Button button_openScript;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ColumnHeader ID;
        private System.Windows.Forms.ColumnHeader isTracker;
        private System.Windows.Forms.ColumnHeader isWorking;
        private System.Windows.Forms.ColumnHeader splitNumber;
        private System.Windows.Forms.ColumnHeader clientURL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
    }
}

