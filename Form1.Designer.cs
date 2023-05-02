using System.Drawing;
using System.Windows.Forms;

namespace NanoAnalyzer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            imageBox = new PictureBox();
            graphY = new PictureBox();
            selectButton = new Button();
            progressBar = new ProgressBar();
            progressLabel = new Label();
            graphX = new PictureBox();
            tickUpdate = new Timer(components);
            genButton = new Button();
            ((System.ComponentModel.ISupportInitialize)imageBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)graphY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)graphX).BeginInit();
            SuspendLayout();
            // 
            // imageBox
            // 
            imageBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            imageBox.BorderStyle = BorderStyle.FixedSingle;
            imageBox.Location = new Point(12, 12);
            imageBox.Name = "imageBox";
            imageBox.Size = new Size(540, 370);
            imageBox.SizeMode = PictureBoxSizeMode.Zoom;
            imageBox.TabIndex = 0;
            imageBox.TabStop = false;
            imageBox.MouseDown += imageBox_MouseDown;
            imageBox.MouseMove += imageBox_MouseMove;
            // 
            // graphY
            // 
            graphY.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            graphY.BorderStyle = BorderStyle.FixedSingle;
            graphY.Location = new Point(12, 388);
            graphY.Name = "graphY";
            graphY.Size = new Size(540, 50);
            graphY.SizeMode = PictureBoxSizeMode.CenterImage;
            graphY.TabIndex = 1;
            graphY.TabStop = false;
            // 
            // selectButton
            // 
            selectButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            selectButton.Location = new Point(614, 12);
            selectButton.Name = "selectButton";
            selectButton.Size = new Size(174, 23);
            selectButton.TabIndex = 3;
            selectButton.Text = "Select file...";
            selectButton.UseVisualStyleBackColor = true;
            selectButton.Click += selectButton_Click;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            progressBar.Location = new Point(614, 410);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(174, 10);
            progressBar.TabIndex = 4;
            // 
            // progressLabel
            // 
            progressLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            progressLabel.Location = new Point(614, 423);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(174, 15);
            progressLabel.TabIndex = 5;
            progressLabel.Text = "0%";
            progressLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // graphX
            // 
            graphX.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            graphX.BorderStyle = BorderStyle.FixedSingle;
            graphX.Location = new Point(558, 12);
            graphX.Name = "graphX";
            graphX.Size = new Size(50, 370);
            graphX.SizeMode = PictureBoxSizeMode.CenterImage;
            graphX.TabIndex = 6;
            graphX.TabStop = false;
            // 
            // tickUpdate
            // 
            tickUpdate.Enabled = true;
            tickUpdate.Interval = 10;
            tickUpdate.Tick += tickUpdate_Tick;
            // 
            // genButton
            // 
            genButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            genButton.Location = new Point(614, 41);
            genButton.Name = "genButton";
            genButton.Size = new Size(174, 23);
            genButton.TabIndex = 7;
            genButton.Text = "Show Gen";
            genButton.UseVisualStyleBackColor = true;
            genButton.Click += genButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(genButton);
            Controls.Add(graphX);
            Controls.Add(progressLabel);
            Controls.Add(progressBar);
            Controls.Add(selectButton);
            Controls.Add(graphY);
            Controls.Add(imageBox);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)imageBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)graphY).EndInit();
            ((System.ComponentModel.ISupportInitialize)graphX).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox imageBox;
        private PictureBox graphY;
        private Button selectButton;
        private ProgressBar progressBar;
        private Label progressLabel;
        private PictureBox graphX;
        private Timer tickUpdate;
        private Button genButton;
    }
}