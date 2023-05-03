﻿namespace NanoAnalyzer.UI
{
    partial class MainForm
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
            imageDisplay = new System.Windows.Forms.PictureBox();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            drawOriginalImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            drawBordersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            drawElementsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            withOriginalImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            withBordersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)imageDisplay).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // imageDisplay
            // 
            imageDisplay.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            imageDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            imageDisplay.Location = new System.Drawing.Point(12, 27);
            imageDisplay.Name = "imageDisplay";
            imageDisplay.Size = new System.Drawing.Size(776, 411);
            imageDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            imageDisplay.TabIndex = 0;
            imageDisplay.TabStop = false;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, toolToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(800, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // toolToolStripMenuItem
            // 
            toolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { drawOriginalImageToolStripMenuItem, drawBordersToolStripMenuItem, drawElementsToolStripMenuItem });
            toolToolStripMenuItem.Name = "toolToolStripMenuItem";
            toolToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            toolToolStripMenuItem.Text = "Tool";
            // 
            // drawOriginalImageToolStripMenuItem
            // 
            drawOriginalImageToolStripMenuItem.Name = "drawOriginalImageToolStripMenuItem";
            drawOriginalImageToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            drawOriginalImageToolStripMenuItem.Text = "Draw original image";
            drawOriginalImageToolStripMenuItem.Click += drawOriginalImageToolStripMenuItem_Click;
            // 
            // drawBordersToolStripMenuItem
            // 
            drawBordersToolStripMenuItem.Name = "drawBordersToolStripMenuItem";
            drawBordersToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            drawBordersToolStripMenuItem.Text = "Draw borders";
            drawBordersToolStripMenuItem.Click += drawBordersToolStripMenuItem_Click;
            // 
            // drawElementsToolStripMenuItem
            // 
            drawElementsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { withOriginalImageToolStripMenuItem, withBordersToolStripMenuItem });
            drawElementsToolStripMenuItem.Name = "drawElementsToolStripMenuItem";
            drawElementsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            drawElementsToolStripMenuItem.Text = "Draw elements";
            // 
            // withOriginalImageToolStripMenuItem
            // 
            withOriginalImageToolStripMenuItem.Name = "withOriginalImageToolStripMenuItem";
            withOriginalImageToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            withOriginalImageToolStripMenuItem.Text = "With original image";
            withOriginalImageToolStripMenuItem.Click += withOriginalImageToolStripMenuItem_Click;
            // 
            // withBordersToolStripMenuItem
            // 
            withBordersToolStripMenuItem.Name = "withBordersToolStripMenuItem";
            withBordersToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            withBordersToolStripMenuItem.Text = "With borders";
            withBordersToolStripMenuItem.Click += withBordersToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(imageDisplay);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)imageDisplay).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox imageDisplay;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawOriginalImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawBordersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawElementsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withOriginalImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem withBordersToolStripMenuItem;
    }
}