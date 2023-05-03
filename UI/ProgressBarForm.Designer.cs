namespace NanoAnalyzer.UI
{
    partial class ProgressBarForm
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
            progressBar = new System.Windows.Forms.ProgressBar();
            progressLabel = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // progressBar
            // 
            progressBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            progressBar.Location = new System.Drawing.Point(12, 12);
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(265, 10);
            progressBar.TabIndex = 1;
            // 
            // progressLabel
            // 
            progressLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            progressLabel.Location = new System.Drawing.Point(12, 25);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new System.Drawing.Size(265, 15);
            progressLabel.TabIndex = 2;
            progressLabel.Text = "0 / 10000 | 0%";
            progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ProgressBarForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(289, 49);
            Controls.Add(progressLabel);
            Controls.Add(progressBar);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Name = "ProgressBarForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "ProgressBarForm";
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label progressLabel;
    }
}