namespace ExtTS_GUI
{
    partial class ExtTS_GUI
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
            this.browse_dialog = new System.Windows.Forms.FolderBrowserDialog();
            this.browsewd_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.workDir_field = new System.Windows.Forms.TextBox();
            this.ejsRoot_field = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.browseejs_button = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ejsVer_field = new System.Windows.Forms.ComboBox();
            this.ejsTk_field = new System.Windows.Forms.ComboBox();
            this.exit_button = new System.Windows.Forms.Button();
            this.progresLog_field = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.generate_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // browse_dialog
            // 
            this.browse_dialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.browse_dialog.ShowNewFolderButton = false;
            // 
            // browsewd_button
            // 
            this.browsewd_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browsewd_button.Location = new System.Drawing.Point(408, 5);
            this.browsewd_button.Name = "browsewd_button";
            this.browsewd_button.Size = new System.Drawing.Size(75, 23);
            this.browsewd_button.TabIndex = 0;
            this.browsewd_button.Text = "Browse...";
            this.browsewd_button.UseVisualStyleBackColor = true;
            this.browsewd_button.Click += new System.EventHandler(this.browsewd_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Working directory:";
            // 
            // workDir_field
            // 
            this.workDir_field.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.workDir_field.Location = new System.Drawing.Point(107, 7);
            this.workDir_field.Name = "workDir_field";
            this.workDir_field.Size = new System.Drawing.Size(297, 20);
            this.workDir_field.TabIndex = 2;
            this.workDir_field.TextChanged += new System.EventHandler(this.workDir_field_TextChanged);
            // 
            // ejsRoot_field
            // 
            this.ejsRoot_field.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ejsRoot_field.Location = new System.Drawing.Point(107, 34);
            this.ejsRoot_field.Name = "ejsRoot_field";
            this.ejsRoot_field.Size = new System.Drawing.Size(297, 20);
            this.ejsRoot_field.TabIndex = 5;
            this.ejsRoot_field.TextChanged += new System.EventHandler(this.ejsRoot_field_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "ExtJS sources root:";
            // 
            // browseejs_button
            // 
            this.browseejs_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseejs_button.Location = new System.Drawing.Point(408, 32);
            this.browseejs_button.Name = "browseejs_button";
            this.browseejs_button.Size = new System.Drawing.Size(75, 23);
            this.browseejs_button.TabIndex = 3;
            this.browseejs_button.Text = "Browse...";
            this.browseejs_button.UseVisualStyleBackColor = true;
            this.browseejs_button.Click += new System.EventHandler(this.browseejs_button_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "ExtJS version:";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(269, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "ExtJS toolkit:";
            // 
            // ejsVer_field
            // 
            this.ejsVer_field.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ejsVer_field.FormattingEnabled = true;
            this.ejsVer_field.Location = new System.Drawing.Point(107, 62);
            this.ejsVer_field.Name = "ejsVer_field";
            this.ejsVer_field.Size = new System.Drawing.Size(156, 21);
            this.ejsVer_field.TabIndex = 11;
            this.ejsVer_field.TextChanged += new System.EventHandler(this.ejsVer_field_TextChanged);
            // 
            // ejsTk_field
            // 
            this.ejsTk_field.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ejsTk_field.FormattingEnabled = true;
            this.ejsTk_field.Location = new System.Drawing.Point(336, 62);
            this.ejsTk_field.Name = "ejsTk_field";
            this.ejsTk_field.Size = new System.Drawing.Size(68, 21);
            this.ejsTk_field.TabIndex = 12;
            // 
            // exit_button
            // 
            this.exit_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exit_button.Location = new System.Drawing.Point(344, 374);
            this.exit_button.Name = "exit_button";
            this.exit_button.Size = new System.Drawing.Size(133, 33);
            this.exit_button.TabIndex = 13;
            this.exit_button.Text = "Exit ExtTS Generator";
            this.exit_button.UseVisualStyleBackColor = true;
            this.exit_button.Click += new System.EventHandler(this.exit_button_Click);
            // 
            // progresLog_field
            // 
            this.progresLog_field.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progresLog_field.Location = new System.Drawing.Point(7, 108);
            this.progresLog_field.MaxLength = 2097151;
            this.progresLog_field.Multiline = true;
            this.progresLog_field.Name = "progresLog_field";
            this.progresLog_field.ReadOnly = true;
            this.progresLog_field.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.progresLog_field.Size = new System.Drawing.Size(475, 258);
            this.progresLog_field.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(7, 92);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Console output:";
            // 
            // generate_button
            // 
            this.generate_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.generate_button.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.generate_button.Enabled = false;
            this.generate_button.ForeColor = System.Drawing.SystemColors.ControlText;
            this.generate_button.Location = new System.Drawing.Point(12, 374);
            this.generate_button.Name = "generate_button";
            this.generate_button.Size = new System.Drawing.Size(133, 33);
            this.generate_button.TabIndex = 16;
            this.generate_button.Text = "Generate";
            this.generate_button.UseVisualStyleBackColor = false;
            this.generate_button.Click += new System.EventHandler(this.generate_button_Click);
            // 
            // ExtTS_GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ClientSize = new System.Drawing.Size(489, 417);
            this.Controls.Add(this.generate_button);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.progresLog_field);
            this.Controls.Add(this.exit_button);
            this.Controls.Add(this.ejsTk_field);
            this.Controls.Add(this.ejsVer_field);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ejsRoot_field);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.browseejs_button);
            this.Controls.Add(this.workDir_field);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.browsewd_button);
            this.Name = "ExtTS_GUI";
            this.Text = "Ext TS Generator";
            this.Load += new System.EventHandler(this.ExtTS_GUI_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog browse_dialog;
        private System.Windows.Forms.Button browsewd_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox workDir_field;
        private System.Windows.Forms.TextBox ejsRoot_field;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button browseejs_button;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ejsVer_field;
        private System.Windows.Forms.ComboBox ejsTk_field;
        private System.Windows.Forms.Button exit_button;
        private System.Windows.Forms.TextBox progresLog_field;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button generate_button;
    }
}

