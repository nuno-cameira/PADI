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
            this.SuspendLayout();
            // 
            // textBox_script
            // 
            this.textBox_script.Location = new System.Drawing.Point(12, 25);
            this.textBox_script.Multiline = true;
            this.textBox_script.Name = "textBox_script";
            this.textBox_script.Size = new System.Drawing.Size(187, 25);
            this.textBox_script.TabIndex = 0;
            this.textBox_script.Text = "Script";
            this.textBox_script.TextChanged += new System.EventHandler(this.textBox_script_TextChanged);
            // 
            // button_loadScript
            // 
            this.button_loadScript.Location = new System.Drawing.Point(205, 25);
            this.button_loadScript.Name = "button_loadScript";
            this.button_loadScript.Size = new System.Drawing.Size(75, 25);
            this.button_loadScript.TabIndex = 1;
            this.button_loadScript.Text = "Load Script";
            this.button_loadScript.UseVisualStyleBackColor = true;
            this.button_loadScript.Click += new System.EventHandler(this.button_loadScript_Click);
            // 
            // button_submit
            // 
            this.button_submit.Location = new System.Drawing.Point(205, 164);
            this.button_submit.Name = "button_submit";
            this.button_submit.Size = new System.Drawing.Size(75, 25);
            this.button_submit.TabIndex = 3;
            this.button_submit.Text = "Submit";
            this.button_submit.UseVisualStyleBackColor = true;
            this.button_submit.Click += new System.EventHandler(this.button_submit_Click);
            // 
            // textBox_console
            // 
            this.textBox_console.Location = new System.Drawing.Point(12, 164);
            this.textBox_console.Multiline = true;
            this.textBox_console.Name = "textBox_console";
            this.textBox_console.Size = new System.Drawing.Size(187, 25);
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
            this.button_step.Location = new System.Drawing.Point(15, 94);
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
            this.button_run.Location = new System.Drawing.Point(94, 94);
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
            this.label1.Location = new System.Drawing.Point(12, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(248, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Runing options (step by step or without interruption)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 148);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Console";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 382);
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
    }
}

