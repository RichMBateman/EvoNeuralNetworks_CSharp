namespace ShepherdCrook.LearningSimpleFunctions
{
    partial class LaunchForm
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
            this.m_btnXOR = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_btnXOR
            // 
            this.m_btnXOR.Location = new System.Drawing.Point(12, 12);
            this.m_btnXOR.Name = "m_btnXOR";
            this.m_btnXOR.Size = new System.Drawing.Size(75, 23);
            this.m_btnXOR.TabIndex = 0;
            this.m_btnXOR.Text = "XOR";
            this.m_btnXOR.UseVisualStyleBackColor = true;
            this.m_btnXOR.Click += new System.EventHandler(this.m_btnXOR_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 570);
            this.Controls.Add(this.m_btnXOR);
            this.Name = "Form1";
            this.Text = "Launcher";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnXOR;
    }
}

