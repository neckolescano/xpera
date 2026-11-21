namespace Xpera
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            sidebarPanel = new Panel();
            menuPanel = new FlowLayoutPanel();
            profileCard = new Xpera.Controls.ProfileCard();
            themeToggle = new Xpera.Controls.ThemeToggle();
            backupRow = new Xpera.Controls.BackupRow();
            logoLabel = new Label();
            logoMark = new LogoMark();
            contentPanel = new Panel();
            screenPanel = new Panel();
            titleLabel = new Label();
            crumbLabel = new Label();
            subtitleLabel = new Label();
            actionsPanel = new FlowLayoutPanel();
            sidebarPanel.SuspendLayout();
            contentPanel.SuspendLayout();
            SuspendLayout();
            //
            // sidebarPanel
            //
            sidebarPanel.Controls.Add(profileCard);
            sidebarPanel.Controls.Add(themeToggle);
            sidebarPanel.Controls.Add(backupRow);
            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoLabel);
            sidebarPanel.Controls.Add(logoMark);
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Location = new Point(0, 0);
            sidebarPanel.Name = "sidebarPanel";
            sidebarPanel.Size = new Size(220, 720);
            sidebarPanel.TabIndex = 0;
            sidebarPanel.Tag = "keep";
            //
            // menuPanel
            //
            menuPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.Location = new Point(14, 76);
            menuPanel.Name = "menuPanel";
            menuPanel.Size = new Size(192, 506);
            menuPanel.TabIndex = 2;
            menuPanel.WrapContents = false;
            //
            //
            // 
            // 
            //
            // logoLabel
            //
            logoLabel.Font = new Font("Segoe UI Semibold", 16F);
            logoLabel.Location = new Point(56, 20);
            logoLabel.Name = "logoLabel";
            logoLabel.Size = new Size(120, 36);
            logoLabel.TabIndex = 1;
            logoLabel.Text = "xpera";
            logoLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // logoMark
            //
            logoMark.Location = new Point(18, 20);
            logoMark.Name = "logoMark";
            logoMark.Size = new Size(36, 36);
            logoMark.TabIndex = 0;
            //
            // contentPanel
            //
            contentPanel.Controls.Add(screenPanel);
            contentPanel.Controls.Add(subtitleLabel);
            contentPanel.Controls.Add(titleLabel);
            contentPanel.Controls.Add(crumbLabel);
            contentPanel.Controls.Add(actionsPanel);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(220, 0);
            contentPanel.Name = "contentPanel";
            contentPanel.Padding = new Padding(28, 20, 28, 24);
            contentPanel.Size = new Size(1040, 720);
            contentPanel.TabIndex = 1;
            //
            // screenPanel
            //
            screenPanel.Dock = DockStyle.Fill;
            screenPanel.Location = new Point(28, 68);
            screenPanel.Name = "screenPanel";
            screenPanel.Size = new Size(984, 628);
            screenPanel.TabIndex = 1;
            //
            // titleLabel
            //
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Font = new Font("Segoe UI Semibold", 20F);
            titleLabel.Location = new Point(28, 42);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(984, 40);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Title";
            //
            // crumbLabel
            //
            crumbLabel.Dock = DockStyle.Top;
            crumbLabel.Font = new Font("Segoe UI", 9F);
            crumbLabel.Location = new Point(28, 20);
            crumbLabel.Name = "crumbLabel";
            crumbLabel.Size = new Size(984, 22);
            crumbLabel.TabIndex = 2;
            crumbLabel.Tag = "muted";
            crumbLabel.Text = "Store › Group › Screen";
            //
            // subtitleLabel
            //
            subtitleLabel.Dock = DockStyle.Top;
            subtitleLabel.Font = new Font("Segoe UI", 9.5F);
            subtitleLabel.Location = new Point(28, 82);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new Size(984, 34);
            subtitleLabel.TabIndex = 3;
            subtitleLabel.Tag = "muted";
            subtitleLabel.Text = "What this screen is for";
            //
            // actionsPanel
            //
            actionsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            actionsPanel.AutoSize = true;
            actionsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            actionsPanel.Location = new Point(800, 40);
            actionsPanel.Name = "actionsPanel";
            actionsPanel.Size = new Size(200, 44);
            actionsPanel.TabIndex = 4;
            actionsPanel.WrapContents = false;
            //
            // MainForm
            //
            //
            // backupRow
            //
            backupRow.Location = new Point(14, 538);
            backupRow.Name = "backupRow";
            backupRow.Size = new Size(192, 52);
            backupRow.TabIndex = 7;
            //
            // themeToggle
            //
            themeToggle.Location = new Point(14, 594);
            themeToggle.Name = "themeToggle";
            themeToggle.Size = new Size(192, 40);
            themeToggle.TabIndex = 5;
            //
            // profileCard
            //
            profileCard.Location = new Point(14, 642);
            profileCard.Name = "profileCard";
            profileCard.Size = new Size(192, 108);
            profileCard.TabIndex = 6;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 720);
            Controls.Add(contentPanel);
            Controls.Add(sidebarPanel);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(1000, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "xpera";
            sidebarPanel.ResumeLayout(false);
            contentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Panel sidebarPanel;
        private Xpera.Controls.ProfileCard profileCard;
        private Xpera.Controls.ThemeToggle themeToggle;
        private Xpera.Controls.BackupRow backupRow;
        private FlowLayoutPanel menuPanel;
        private Label logoLabel;
        private LogoMark logoMark;
        private Panel contentPanel;
        private Panel screenPanel;
        private Label titleLabel;
        private Label crumbLabel;
        private Label subtitleLabel;
        private FlowLayoutPanel actionsPanel;
    }
}
