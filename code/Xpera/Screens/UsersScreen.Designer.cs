namespace Xpera.Screens
{
    partial class UsersScreen
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
            nameColumn = new DataGridViewTextBoxColumn();
            actionsColumn = new DataGridViewTextBoxColumn();
            usernameColumn = new DataGridViewTextBoxColumn();
            roleColumn = new DataGridViewTextBoxColumn();
            statusColumn = new DataGridViewTextBoxColumn();
            grid = new DataGridView();
            pager = new Xpera.Controls.Pager();
            toolbarPanel = new Panel();
            addButton = new Xpera.Controls.RoundButton();
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
            cardTitleLabel.Text = "Staff accounts";
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
            subLabel.Text = "The Admin creates accounts and sets each role.";
            //
            // toolbarPanel
            //
            toolbarPanel.Controls.Add(addButton);
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
            searchBox.PlaceholderText = "Search accounts";
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
            countLabel.Text = "0 accounts";
            //
            // addButton
            //
            addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addButton.Location = new Point(778, 4);
            addButton.Name = "addButton";
            addButton.Size = new Size(170, 40);
            addButton.TabIndex = 2;
            addButton.Text = "+ Add user";
            addButton.Click += addButton_Click;
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
            grid.Columns.AddRange(new DataGridViewColumn[] { nameColumn, usernameColumn, roleColumn, statusColumn, actionsColumn });
            grid.Dock = DockStyle.Fill;
            grid.Location = new Point(18, 120);
            grid.Name = "grid";
            grid.Size = new Size(948, 448);
            grid.TabIndex = 3;
            //
            // UsersScreen
            //
            // 
            // nameColumn
            // 
            nameColumn.HeaderText = "Name";
            nameColumn.Name = "nameColumn";
            nameColumn.ReadOnly = true;
            nameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // usernameColumn
            // 
            usernameColumn.HeaderText = "Username";
            usernameColumn.Name = "usernameColumn";
            usernameColumn.ReadOnly = true;
            usernameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // roleColumn
            // 
            roleColumn.HeaderText = "Role";
            roleColumn.Name = "roleColumn";
            roleColumn.ReadOnly = true;
            roleColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // statusColumn
            // 
            statusColumn.HeaderText = "Status";
            statusColumn.Name = "statusColumn";
            statusColumn.ReadOnly = true;
            statusColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // stats
            // 
            stats.Dock = DockStyle.Top;
            stats.Location = new Point(0, 0);
            stats.Name = "stats";
            stats.Size = new Size(984, 110);
            stats.TabIndex = 1;
            // 
            // actionsColumn
            // 
            actionsColumn.HeaderText = "";
            actionsColumn.Name = "actionsColumn";
            actionsColumn.ReadOnly = true;
            actionsColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            actionsColumn.ToolTipText = "Edit or delete";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(card);
            Controls.Add(stats);
            Name = "UsersScreen";
            Size = new Size(984, 628);
            card.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            toolbarPanel.ResumeLayout(false);
            toolbarPanel.PerformLayout();
            ResumeLayout(false);
        }

        private Xpera.Controls.StatStrip stats;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn usernameColumn;
        private DataGridViewTextBoxColumn roleColumn;
        private DataGridViewTextBoxColumn statusColumn;
        private Xpera.Controls.Card card;
        private DataGridView grid;
        private Xpera.Controls.Pager pager;
        private Panel toolbarPanel;
        private Xpera.Controls.RoundButton addButton;
        private Label countLabel;
        private Xpera.Controls.SearchBox searchBox;
        private Label subLabel;
        private DataGridViewTextBoxColumn actionsColumn;
        private Label cardTitleLabel;
    }
}
