using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NanoAnalyzer.UI
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm()
        {
            InitializeComponent();
            this.progressBar.Maximum = 10000;
        }

        private void SetFrame(Frame frame)
        {
            progressBar.Value = (int)Math.Round(frame.GetProgress() * progressBar.Maximum);
            progressLabel.Text = frame.GetText();
        }

        public static void StartProgress(string title, IEnumerable<Frame> progress, IWin32Window? owner = null)
        {
            ProgressBarForm form = new ProgressBarForm();
            form.Text = title;
            object _lock = new object();
            Frame? lastFrame = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task.Run(async () =>
            {
                while (token.IsCancellationRequested)
                {
                    Frame? frame;
                    lock (_lock)
                    {
                        frame = lastFrame;
                        lastFrame = null;
                    }
                    if (frame == null) continue;
                    form.Invoke(() => form.SetFrame(frame));
                    await Task.Delay(10);
                }
            });
            Task.Run(() =>
            {
                try
                {
                    foreach (Frame frame in progress)
                    {
                        lock (_lock)
                        {
                            lastFrame = frame;
                        }
                    }
                }
                finally
                {
                    tokenSource.Cancel();
                    form.Invoke(() =>
                    {
                        form.DialogResult = DialogResult.OK;
                        form.Close();
                    });
                }
            });
            form.ShowDialog(owner);
        }
        public static void StartProgress(string title, Action<Action<Frame>> progressAction, IWin32Window? owner = null)
        {
            ProgressBarForm form = new ProgressBarForm();
            form.Text = title;
            object _lock = new object();
            Frame? lastFrame = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    Frame? frame;
                    lock (_lock)
                    {
                        frame = lastFrame;
                        lastFrame = null;
                    }
                    if (frame == null) continue;
                    if (!form.IsHandleCreated || form.IsDisposed) continue;
                    form.Invoke(() => form.SetFrame(frame));
                    await Task.Delay(10);
                }
            });
            Task.Run(async () =>
            {
                bool dispose = false;
                try
                {
                    progressAction.Invoke(frame =>
                    {
                        lock (_lock)
                        {
                            lastFrame = frame;
                        }
                        if (form.IsDisposed) throw new ObjectDisposedException("Closed");
                    });
                }
                catch (ObjectDisposedException)
                {
                    dispose = true;
                    tokenSource.Cancel();
                }
                finally
                {
                    if (!dispose && !form.IsDisposed)
                    {
                        tokenSource.Cancel();
                        while (!form.IsHandleCreated)
                        {
                            await Task.Delay(10);
                        }
                        form.Invoke(() =>
                        {
                            form.DialogResult = DialogResult.OK;
                            form.Close();
                        });
                    }
                }
            });
            form.ShowDialog(owner);
        }
    }
}
