namespace ArkWebMapSlaveInterface
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
            this.mainView = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // mainView
            // 
            this.mainView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainView.Location = new System.Drawing.Point(-3, 0);
            this.mainView.MinimumSize = new System.Drawing.Size(20, 20);
            this.mainView.Name = "mainView";
            this.mainView.ScrollBarsEnabled = false;
            this.mainView.Size = new System.Drawing.Size(472, 579);
            this.mainView.TabIndex = 0;
            this.mainView.Url = new System.Uri("", System.UriKind.Relative);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(54)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(466, 575);
            this.Controls.Add(this.mainView);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "Form1";
            this.Text = "Ark Web Map Control Panel";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser mainView;
    }
}

