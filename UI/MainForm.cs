using NanoAnalyzer.Extremum;
using NanoAnalyzer.Extremum.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NanoAnalyzer.UI;

public partial class MainForm : Form
{
    private (Image<Rgba32> src, ExtremumImage extremum)? image = null;
    private ElementData[]? cache_elements = null;

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
        if (!Directory.Exists("cache")) Directory.CreateDirectory("cache");
        ProgressBarForm.StartProgress($"Load {fileName} file...", progress =>
        {
            progress.Invoke(Frame.Of(0, 5, "Read file"));
            Image<Rgba32> image = Image.Load<Rgba32>(fileName);
            progress.Invoke(Frame.Of(1, 5, "Grayscale"));
            image.Mutate(v => v.Grayscale());

            progress.Invoke(Frame.Of(2, 5, "Check cache"));
            string md5 = "cache/" + image.HashMD5() + ".ext";
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
            this.cache_elements = null;
        }, this);

        drawOriginalImageToolStripMenuItem_Click(sender, e);
    }


    private void drawOriginalImageToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (image is not (Image<Rgba32> src, ExtremumImage _))
        {
            MessageBox.Show(this, "Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        imageDisplay.Image = src.ToSystem();
    }

    private void drawBordersToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (image is not (Image<Rgba32> _, ExtremumImage extremum))
        {
            MessageBox.Show(this, "Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MessageBox.Show(this, "Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        using Image<Rgba32> img = src.Clone();
        ProgressBarForm.StartProgress($"Create elements...", progress =>
        {
            cache_elements = extremum.ExtractElementsWithDraw(img, 1, 5000, progress).ToArray();
        }, this);
        imageDisplay.Image = img.ToSystem();
    }

    private void withBordersToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (image is not (Image<Rgba32> _, ExtremumImage extremum))
        {
            MessageBox.Show(this, "Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        using Image<Rgba32> img = new Image<Rgba32>(extremum.width, extremum.height);
        extremum.DrawBorder(img);
        ProgressBarForm.StartProgress($"Create elements...", progress =>
        {
            cache_elements = extremum.ExtractElementsWithDraw(img, 1, 5000, progress).ToArray();
        }, this);
        imageDisplay.Image = img.ToSystem();
    }

    private void exportData(string extension, Func<IEnumerable<ElementData>, byte[]> serialize)
    {
        if (image is not (Image<Rgba32> _, ExtremumImage extremum))
        {
            MessageBox.Show(this, "Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        extension = extension.TrimStart('.');
        using SaveFileDialog dialog = new SaveFileDialog()
        {
            Title = $"Export .{extension} file",
            Filter = $"{extension.ToUpper()} file (.{extension})|.{extension}",
            FileName = $"export.{extension}"
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        string fileName = dialog.FileName;
        ProgressBarForm.StartProgress(
            $"Export to {fileName}...",
            progress => File.WriteAllBytes(fileName, serialize.Invoke((cache_elements ?? extremum.ExtractElements(5000, progress)))),
            this);
    }
    private bool exportExecute(Func<bool> filterExecute, Action<ExtremumImage, IEnumerable<ElementData>> serialize)
    {
        if (image is not (Image<Rgba32> _, ExtremumImage extremum))
        {
            MessageBox.Show(this, "Image not opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        if (!filterExecute.Invoke()) return false;
        ProgressBarForm.StartProgress(
            $"Calculate...",
            progress => serialize.Invoke(extremum, (cache_elements ?? extremum.ExtractElements(5000, progress))),
            this);
        return true;
    }

    private void exportTocsvToolStripMenuItem_Click(object sender, EventArgs e)
    {
        exportData("csv", v => v
            .Select(v => new
            {
                X = v.x,
                Y = v.y,
                Radius = v.radius,
                Weight = v.weight
            })
            .ToCsv());
    }

    private void exportTojsonToolStripMenuItem_Click(object sender, EventArgs e)
    {
        exportData("json", v => v
            .Select(v => new
            {
                x = v.x,
                y = v.y,
                radius = v.radius,
                weight = v.weight
            })
            .ToJson());
    }

    private void showObjectsCountPerUnitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string? postfix = null;
        double scale = 0;
        double countPerUnit = 0;
        if (!exportExecute(() =>
        {
            if (InputBox.Show("Input image width scale", "1300.0 n", out string output, this) != DialogResult.OK) return false;
            string[] args = output.Split(' ', 2);
            if (!double.TryParse(args[0], CultureInfo.InvariantCulture.NumberFormat, out scale))
            {
                MessageBox.Show(this, $"'{args[0]}' is not number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            postfix = args.Length > 1 ? args[1] : null;
            return true;
        }, (extremum, v) =>
        {
            double pixelSize = extremum.width / scale;

            double widthSize = extremum.width * pixelSize;
            double heightSize = extremum.height * pixelSize;

            double size = extremum.width * extremum.height;

            double sum = v.Sum(v => v.radius);

            countPerUnit = sum / size;
            countPerUnit = (sum * sum) / (size * size);
        })) return;
        MessageBox.Show(this, $"{countPerUnit} units in {postfix ?? "any"}^2");
    }

    private void exportTojsonToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        const int PART_COUNT = 20;
        exportData("json", v =>
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            List<ElementData> values = v.ToList();
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            foreach (ElementData value in values)
            {
                double radius = value.radius;
                if (max < radius) max = radius;
                if (min > radius) min = radius;
            }
            double delta = max - min;
            int maxCount = 0;
            for (int i = 0; i < PART_COUNT; i++)
                counts[i] = 0;
            foreach (ElementData value in values)
            { 
                int index = (int)Math.Round(((value.radius - min) / delta) * PART_COUNT);
                counts.TryGetValue(index, out int count);
                count += value.weight;
                if (count > maxCount) maxCount = maxCount = count;
                counts[index] = count;
            }
            return counts
                .Select(kv => new
                {
                    index = kv.Key,
                    value = ((kv.Key / (double)PART_COUNT) * delta) + min,
                    count = kv.Value / (double)maxCount
                })
                .OrderBy(v => v.index)
                .ToJson();
        });
    }

    private void exportTocsvToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        const int PART_COUNT = 20;
        exportData("csv", v =>
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            List<ElementData> values = v.ToList();
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            foreach (ElementData value in values)
            {
                double radius = value.radius;
                if (max < radius) max = radius;
                if (min > radius) min = radius;
            }
            double delta = max - min;
            int maxCount = 0;
            for (int i = 0; i < PART_COUNT; i++)
                counts[i] = 0;
            foreach (ElementData value in values)
            {
                int index = (int)Math.Round(((value.radius - min) / delta) * PART_COUNT);
                counts.TryGetValue(index, out int count);
                count += value.weight;
                if (count > maxCount) maxCount = maxCount = count;
                counts[index] = count;
            }
            return counts
                .Select(kv => new
                {
                    Index = kv.Key,
                    Value = ((kv.Key / (double)PART_COUNT) * delta) + min,
                    Count = kv.Value / (double)maxCount
                })
                .OrderBy(v => v.Index)
                .ToCsv();
        });
    }
}
