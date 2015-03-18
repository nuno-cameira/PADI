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
            this.SuspendLayout();
            // 
            // textBox_script
            // 
            this.textBox_script.Location = new System.Drawing.Point(12, 10);
            this.textBox_script.Multiline = true;
            this.textBox_script.Name = "textBox_script";
            this.textBox_script.Size = new System.Drawing.Size(157, 23);
            this.textBox_script.TabIndex = 0;
            this.textBox_script.Text = "Script";
            this.textBox_script.TextChanged += new System.EventHandler(this.textBox_script_TextChanged);
            // 
            // button_loadScript
            // 
            this.button_loadScript.Location = new System.Drawing.Point(194, 10);
            this.button_loadScript.Name = "button_loadScript";
            this.button_loadScript.Size = new System.Drawing.Size(75, 23);
            this.button_loadScript.TabIndex = 1;
            this.button_loadScript.Text = "Load Script";
            this.button_loadScript.UseVisualStyleBackColor = true;
            this.button_loadScript.Click += new System.EventHandler(this.button_loadScript_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 382);
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
    }
}

