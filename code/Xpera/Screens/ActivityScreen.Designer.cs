namespace Xpera.Screens
{
    partial class ActivityScreen
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
            card = new Xpera.Controls.Card();
            stats = new Xpera.Controls.StatStrip();
            whenColumn = new DataGridViewTextBoxColumn();
            whoColumn = new DataGridViewTextBoxColumn();
            actionColumn = new DataGridViewTextBoxColumn();
            cloudColumn = new DataGridViewTextBoxColumn();
            grid = new DataGridView();
            pager = new Xpera.Controls.Pager();
            toolbarPanel = new Panel();
            backupButton = new Xpera.Controls.RoundButton();
            countLabel = new Label();
            searchBox = new Xpera.Controls.SearchBox();
            subLabel = new Label();
            cardTitleLabel = new Label();
            card.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            toolbarPanel.SuspendLayout();
            SuspendLayout();
            //
            // card
            //
            card.Controls.Add(grid);
            card.Controls.Add(pager);
            card.Controls.Add(toolbarPanel);
            card.Controls.Add(subLabel);
            card.Controls.Add(cardTitleLabel);
            card.Dock = DockStyle.Fill;
            card.Location = new Point(0, 0);
            card.Name = "card";
            card.Size = new Size(984, 628);
            card.TabIndex = 0;
            //
            // cardTitleLabel
            //
            cardTitleLabel.Dock = DockStyle.Top;
            cardTitleLabel.Font = new Font("Segoe UI Semibold", 11F);
            cardTitleLabel.Location = new Point(18, 16);
            cardTitleLabel.Name = "cardTitleLabel";
            cardTitleLabel.Size = new Size(948, 26);
            cardTitleLabel.TabIndex = 0;
            cardTitleLabel.Text = "Activity";
            //
            // subLabel
            //
            subLabel.Dock = DockStyle.Top;
            subLabel.Font = new Font("Segoe UI", 9F);
            subLabel.Location = new Point(18, 42);
            subLabel.Name = "subLabel";
            subLabel.Size = new Size(948, 26);
            subLabel.TabIndex = 1;
            subLabel.Tag = "muted";
            subLabel.Text = "Each line says if the record it is about is already in the cloud backup.";
            //
            // toolbarPanel
            //
            toolbarPanel.Controls.Add(backupButton);
            toolbarPanel.Controls.Add(countLabel);
            toolbarPanel.Controls.Add(searchBox);
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Location = new Point(18, 68);
            toolbarPanel.Name = "toolbarPanel";
            toolbarPanel.Size = new Size(948, 52);
            toolbarPanel.TabIndex = 2;
            //
            // searchBox
            //
            searchBox.Location = new Point(0, 6);
            searchBox.Name = "searchBox";
            searchBox.PlaceholderText = "Search activity";
            searchBox.Size = new Size(300, 36);
            searchBox.TabIndex = 0;
            searchBox.TextChanged += searchBox_TextChanged;
            //
            // countLabel
            //
            countLabel.AutoSize = true;
            countLabel.Font = new Font("Segoe UI", 9F);
            countLabel.Location = new Point(314, 15);
            countLabel.Name = "countLabel";
            countLabel.TabIndex = 1;
            countLabel.Tag = "muted";
            countLabel.Text = "0 lines";
            //
            // backupButton
            //
            backupButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            backupButton.Location = new Point(778, 4);
            backupButton.Name = "backupButton";
            backupButton.Size = new Size(170, 40);
            backupButton.TabIndex = 2;
            backupButton.Text = "Back up now";
            backupButton.Click += backupButton_Click;
            //
            // pager
            //
            pager.Dock = DockStyle.Bottom;
            pager.Location = new Point(18, 568);
            pager.Name = "pager";
            pager.Size = new Size(948, 44);
            pager.TabIndex = 4;
            //
            // grid
            //
            grid.Columns.AddRange(new DataGridViewColumn[] { whenColumn, whoColumn, actionColumn, cloudColumn });
            grid.Dock = DockStyle.Fill;
            grid.Location = new Point(18, 120);
            grid.Name = "grid";
            grid.Size = new Size(948, 448);
            grid.TabIndex = 3;
            //
            // ActivityScreen
            //
            // 
            // whenColumn
            // 
            whenColumn.HeaderText = "When";
            whenColumn.Name = "whenColumn";
            whenColumn.ReadOnly = true;
            whenColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // whoColumn
            // 
            whoColumn.HeaderText = "Who";
            whoColumn.Name = "whoColumn";
            whoColumn.ReadOnly = true;
            whoColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // actionColumn
            // 
            actionColumn.HeaderText = "What happened";
            actionColumn.FillWeight = 260F;
            actionColumn.Name = "actionColumn";
            actionColumn.ReadOnly = true;
            actionColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // cloudColumn
            // 
            cloudColumn.HeaderText = "Cloud";
            cloudColumn.Name = "cloudColumn";
            cloudColumn.ReadOnly = true;
            cloudColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // stats
            // 
            stats.Dock = DockStyle.Top;
            stats.Location = new Point(0, 0);
            stats.Name = "stats";
            stats.Size = new Size(984, 110);
            stats.TabIndex = 1;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(card);
            Controls.Add(stats);
            Name = "ActivityScreen";
            Size = new Size(984, 628);
            card.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            toolbarPanel.ResumeLayout(false);
            toolbarPanel.PerformLayout();
            ResumeLayout(false);
        }

        private Xpera.Controls.StatStrip stats;
        private DataGridViewTextBoxColumn whenColumn;
        private DataGridViewTextBoxColumn whoColumn;
        private DataGridViewTextBoxColumn actionColumn;
        private DataGridViewTextBoxColumn cloudColumn;
        private Xpera.Controls.Card card;
        private DataGridView grid;
        private Xpera.Controls.Pager pager;
        private Panel toolbarPanel;
        private Xpera.Controls.RoundButton backupButton;
        private Label countLabel;
        private Xpera.Controls.SearchBox searchBox;
        private Label subLabel;
        private Label cardTitleLabel;
    }
}
