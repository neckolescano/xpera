using Xpera.Controls;
using Xpera.infrastructure.services;

namespace Xpera
{
    // "Backing up to the cloud…" while a backup runs. It has no buttons and can't be closed half-way,
    // because the copy happens in one transaction: it finishes, or nothing in the cloud changes.
    // It closes by itself when the backup is done; the result is in Result.
    public class BackupDialog : ModalForm
    {
        private readonly Func<Task<BackupResult>> _work;
        private bool _finished;
        public BackupResult Result { get; private set; }

        public BackupDialog(string title, string description, Func<Task<BackupResult>> work)
            : base(title, description, 440)
        {
            _work = work;
            Label note = new Label
            {
                Text = "Please keep xpera open. This usually takes a few seconds.",
                Font = Ui.UiFont(9.5F, false),
                ForeColor = Ui.Colors.TextSecondary,
                Tag = "keep"
            };
            note.SetBounds(Side, ContentTop - 8, ContentWidth, 22);
            Controls.Add(note);
            ClientSize = new Size(ClientSize.Width, note.Bottom + 28);
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // The copy runs off the screen's thread, so the window keeps drawing while it waits for the cloud.
            Result = await Task.Run(_work);
            _finished = true;
            DialogResult = DialogResult.OK;
        }

        // The × and Esc do nothing until the backup is done.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_finished && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; return; }
            base.OnFormClosing(e);
        }
    }
}
