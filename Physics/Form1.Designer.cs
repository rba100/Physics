namespace Physics
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
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.cbScenarioList = new System.Windows.Forms.ComboBox();
            this.btLoad = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox.Location = new System.Drawing.Point(0, 49);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(779, 641);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.Resize += new System.EventHandler(this.pictureBox_Resize);
            // 
            // cbScenarioList
            // 
            this.cbScenarioList.FormattingEnabled = true;
            this.cbScenarioList.Location = new System.Drawing.Point(12, 12);
            this.cbScenarioList.Name = "cbScenarioList";
            this.cbScenarioList.Size = new System.Drawing.Size(264, 24);
            this.cbScenarioList.TabIndex = 1;
            // 
            // btLoad
            // 
            this.btLoad.Location = new System.Drawing.Point(282, 12);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(75, 25);
            this.btLoad.TabIndex = 2;
            this.btLoad.Text = "Run";
            this.btLoad.UseVisualStyleBackColor = true;
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(779, 690);
            this.Controls.Add(this.btLoad);
            this.Controls.Add(this.cbScenarioList);
            this.Controls.Add(this.pictureBox);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Simulator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ComboBox cbScenarioList;
        private System.Windows.Forms.Button btLoad;
    }
}

