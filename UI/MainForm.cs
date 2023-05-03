using NanoAnalyzer.Extremum;
using NanoAnalyzer.Extremum.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NanoAnalyzer.UI
{
    public partial class MainForm : Form
    {
        private (Image<Rgba32> src, ExtremumImage extremum)? image = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Application.Exit();
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Select image",
                Multiselect = false
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            string fileName = dialog.FileName;
            ProgressBarForm.StartProgress($"Load {fileName} file...", progress =>
            {
                progress.Invoke(Frame.Of(0, 5, "Read file"));
                Image<Rgba32> image = Image.Load<Rgba32>(fileName);
                progress.Invoke(Frame.Of(1, 5, "Grayscale"));
                image.Mutate(v => v.Grayscale());

                progress.Invoke(Frame.Of(2, 5, "Check cache"));
                string md5 = image.HashMD5() + ".ext";
                ExtremumImage extremumImage;
                if (File.Exists(md5))
                {
                    progress.Invoke(Frame.Of(3, 5, "Read cache of extremum"));
                    using Stream fileStream = File.OpenRead(md5);
                    using BinaryReader reader = new BinaryReader(fileStream);
                    progress.Invoke(Frame.Of(4, 5, "Read cache of extremum"));
                    extremumImage = ExtremumImage.Read(reader);
                }
                else
                {
                    Frame extremumFrame = Frame.Of(3, 5, "Create extremum");
                    progress.Invoke(extremumFrame);
                    extremumImage = Logic.GetExtremumImage(image, (i, total) => progress.Invoke(extremumFrame.Child(i, total)));
                    progress.Invoke(Frame.Of(4, 5, "Save cache of extremum"));
                    using Stream fileStream = File.Open(md5, FileMode.Create);
                    using BinaryWriter writer = new BinaryWriter(fileStream);
                    extremumImage.Write(writer);
                }
                progress.Invoke(Frame.Of(5, 5, "OK"));
                this.image = (image, extremumImage);
            });

            drawOriginalImageToolStripMenuItem_Click(sender, e);
        }


        private void drawOriginalImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image is not (Image<Rgba32> src, ExtremumImage _))
            {
                MessageBox.Show("Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            imageDisplay.Image = src.ToSystem();
        }

        private void drawBordersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image is not (Image<Rgba32> _, ExtremumImage extremum))
            {
                MessageBox.Show("Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using Image<Rgba32> img = new Image<Rgba32>(extremum.width, extremum.height);
            extremum.DrawBorder(img);
            imageDisplay.Image = img.ToSystem();
        }

        private void withOriginalImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image is not (Image<Rgba32> src, ExtremumImage extremum))
            {
                MessageBox.Show("Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            using Image<Rgba32> img = src.Clone();
            ProgressBarForm.StartProgress($"Create elements...", progress =>
            {
                _ = extremum.ExtractElementsWithDraw(img, Pens.Dot(Color.Aqua, 1), 5000, progress).All(v => true);
            });
            imageDisplay.Image = img.ToSystem();
        }

        private void withBordersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image is not (Image<Rgba32> _, ExtremumImage extremum))
            {
                MessageBox.Show("Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            using Image<Rgba32> img = new Image<Rgba32>(extremum.width, extremum.height);
            extremum.DrawBorder(img);
            ProgressBarForm.StartProgress($"Create elements...", progress =>
            {
                _ = extremum.ExtractElementsWithDraw(img, Pens.Dot(Color.Aqua, 1), 5000, progress).All(v => true);
            });
            imageDisplay.Image = img.ToSystem();
        }
    }
}
