namespace Xpera.Screens
{
    partial class SalesScreen
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
            customerStats = new Xpera.Controls.MiniStats();
            quickPanel = new FlowLayoutPanel();
            quickAddLabel = new Label();
            receiptLabel = new Label();
            itemColumn = new DataGridViewTextBoxColumn();
            lessColumn = new DataGridViewTextBoxColumn();
            qtyColumn = new DataGridViewTextBoxColumn();
            moreColumn = new DataGridViewTextBoxColumn();
            priceColumn = new DataGridViewTextBoxColumn();
            subtotalColumn = new DataGridViewTextBoxColumn();
            removeColumn = new DataGridViewTextBoxColumn();
            receiptColumn = new DataGridViewTextBoxColumn();
            dateColumn = new DataGridViewTextBoxColumn();
            customerColumn = new DataGridViewTextBoxColumn();
            itemsColumn = new DataGridViewTextBoxColumn();
            typeColumn = new DataGridViewTextBoxColumn();
            amountColumn = new DataGridViewTextBoxColumn();
            topBar = new Panel();
            tabsPanel = new Xpera.Controls.Card();
            changeButton = new Xpera.Controls.RoundButton();
            addItemButton = new Xpera.Controls.RoundButton();
            cancelButton = new Xpera.Controls.RoundButton();
            historyTab = new Xpera.Controls.RoundButton();
            checkoutTab = new Xpera.Controls.RoundButton();
            bodyPanel = new Panel();
            layout = new TableLayoutPanel();
            leftPanel = new Panel();
            itemsCard = new Xpera.Controls.Card();
            cartGrid = new DataGridView();
            itemsHeader = new Panel();
            itemCountLabel = new Label();
            itemsTitleLabel = new Label();
            spacer = new Panel();
            customerCard = new Xpera.Controls.Card();
            customerInset = new Xpera.Controls.Card();
            customerBox = new ComboBox();
            customerMetaLabel = new Label();
            customerNameLabel = new Label();
            avatar = new Xpera.Controls.Avatar();
            customerTitleLabel = new Label();
            rightPanel = new Panel();
            paymentCard = new Xpera.Controls.Card();
            payBody = new FlowLayoutPanel();
            segPanel = new Xpera.Controls.Card();
            subtotalRow = new Panel();
            totalRow = new Panel();
            saveButton = new Xpera.Controls.RoundButton();
            creditNoteLabel = new Label();
            totalValueLabel = new Label();
            totalCaptionLabel = new Label();
            subtotalValueLabel = new Label();
            subtotalCaptionLabel = new Label();
            dueDatePicker = new Xpera.Controls.DateField();
            dueLabel = new Label();
            creditButton = new Xpera.Controls.RoundButton();
            cashButton = new Xpera.Controls.RoundButton();
            paymentTitleLabel = new Label();
            historyCard = new Xpera.Controls.Card();
            historyGrid = new DataGridView();
            historyPager = new Xpera.Controls.Pager();
            historyToolbar = new Panel();
            historyCountLabel = new Label();
            historySearchBox = new Xpera.Controls.SearchBox();
            historySubLabel = new Label();
            historyTitleLabel = new Label();
            topBar.SuspendLayout();
            bodyPanel.SuspendLayout();
            layout.SuspendLayout();
            leftPanel.SuspendLayout();
            itemsCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cartGrid).BeginInit();
            itemsHeader.SuspendLayout();
            customerCard.SuspendLayout();
            customerInset.SuspendLayout();
            rightPanel.SuspendLayout();
            paymentCard.SuspendLayout();
            payBody.SuspendLayout();
            historyCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)historyGrid).BeginInit();
            historyToolbar.SuspendLayout();
            SuspendLayout();
            //
            // topBar
            //
            topBar.Controls.Add(cancelButton);
            topBar.Controls.Add(tabsPanel);
            topBar.Dock = DockStyle.Top;
            topBar.Location = new Point(0, 0);
            topBar.Name = "topBar";
            topBar.Size = new Size(984, 50);
            topBar.TabIndex = 0;
            //
            // tabsPanel
            //
            tabsPanel.Controls.Add(historyTab);
            tabsPanel.Controls.Add(checkoutTab);
            tabsPanel.Location = new Point(0, 0);
            tabsPanel.Margin = new Padding(0, 0, 8, 0);
            tabsPanel.Name = "tabsPanel";
            tabsPanel.Padding = new Padding(4);
            tabsPanel.Radius = 12;
            tabsPanel.Size = new Size(252, 44);
            tabsPanel.TabIndex = 0;
            //
            // checkoutTab
            //
            checkoutTab.Dock = DockStyle.Left;
            checkoutTab.Name = "checkoutTab";
            checkoutTab.Size = new Size(108, 36);
            checkoutTab.TabIndex = 0;
            checkoutTab.Text = "Checkout";
            checkoutTab.Click += checkoutTab_Click;
            //
            // historyTab
            //
            historyTab.Dock = DockStyle.Left;
            historyTab.Name = "historyTab";
            historyTab.Size = new Size(130, 36);
            historyTab.TabIndex = 1;
            historyTab.Text = "Sales history";
            historyTab.Click += historyTab_Click;
            //
            // cancelButton
            //
            cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cancelButton.Location = new Point(884, 2);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(100, 44);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.Click += cancelButton_Click;
            //
            // bodyPanel
            //
            bodyPanel.Controls.Add(layout);
            bodyPanel.Controls.Add(historyCard);
            bodyPanel.Dock = DockStyle.Fill;
            bodyPanel.Location = new Point(0, 50);
            bodyPanel.Name = "bodyPanel";
            bodyPanel.Size = new Size(984, 578);
            bodyPanel.TabIndex = 1;
            //
            // layout
            //
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));
            layout.Controls.Add(leftPanel, 0, 0);
            layout.Controls.Add(rightPanel, 1, 0);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Name = "layout";
            layout.RowCount = 1;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Size = new Size(984, 578);
            layout.TabIndex = 0;
            //
            // leftPanel
            //
            leftPanel.Controls.Add(itemsCard);
            leftPanel.Controls.Add(spacer);
            leftPanel.Controls.Add(customerCard);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Margin = new Padding(0);
            leftPanel.Name = "leftPanel";
            leftPanel.Padding = new Padding(0, 0, 8, 0);
            leftPanel.TabIndex = 0;
            //
            // customerCard
            //
            customerCard.Controls.Add(customerInset);
            customerCard.Controls.Add(customerTitleLabel);
            customerCard.Dock = DockStyle.Top;
            customerCard.Name = "customerCard";
            customerCard.Size = new Size(612, 190);
            customerCard.TabIndex = 0;
            //
            // customerTitleLabel
            //
            customerTitleLabel.Dock = DockStyle.Top;
            customerTitleLabel.Font = new Font("Segoe UI Semibold", 11F);
            customerTitleLabel.Name = "customerTitleLabel";
            customerTitleLabel.Size = new Size(576, 30);
            customerTitleLabel.TabIndex = 0;
            customerTitleLabel.Text = "Customer";
            //
            // customerInset
            //
            customerInset.Controls.Add(customerStats);
            customerInset.Controls.Add(changeButton);
            customerInset.Controls.Add(customerBox);
            customerInset.Controls.Add(customerMetaLabel);
            customerInset.Controls.Add(customerNameLabel);
            customerInset.Controls.Add(avatar);
            customerInset.Dock = DockStyle.Fill;
            customerInset.Inset = true;
            customerInset.Name = "customerInset";
            customerInset.Padding = new Padding(12);
            customerInset.Radius = 10;
            customerInset.Size = new Size(576, 128);
            customerInset.TabIndex = 1;
            //
            // avatar
            //
            avatar.Location = new Point(14, 16);
            avatar.Name = "avatar";
            avatar.Size = new Size(40, 40);
            avatar.TabIndex = 0;
            //
            // customerNameLabel
            //
            customerNameLabel.Font = new Font("Segoe UI Semibold", 10.5F);
            customerNameLabel.Location = new Point(64, 14);
            customerNameLabel.Name = "customerNameLabel";
            customerNameLabel.Size = new Size(300, 22);
            customerNameLabel.TabIndex = 1;
            customerNameLabel.Text = "Customer name";
            //
            // customerMetaLabel
            //
            customerMetaLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            customerMetaLabel.Font = new Font("Segoe UI", 9F);
            customerMetaLabel.Location = new Point(64, 37);
            customerMetaLabel.Name = "customerMetaLabel";
            customerMetaLabel.Size = new Size(330, 20);
            customerMetaLabel.TabIndex = 2;
            customerMetaLabel.Tag = "muted";
            customerMetaLabel.Text = "Regular · balance ₱0 · usually buys —";
            //
            // changeButton
            //
            changeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            changeButton.Location = new Point(460, 20);
            changeButton.Name = "changeButton";
            changeButton.Size = new Size(100, 34);
            changeButton.TabIndex = 4;
            changeButton.Tag = "link";
            changeButton.Text = "Change ⌄";
            changeButton.Click += changeButton_Click;
            //
            // customerBox
            //
            customerBox.Visible = false;
            customerBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            customerBox.DropDownStyle = ComboBoxStyle.DropDownList;
            customerBox.Location = new Point(404, 22);
            customerBox.Name = "customerBox";
            customerBox.Size = new Size(160, 25);
            customerBox.TabIndex = 3;
            customerBox.SelectedIndexChanged += customerBox_SelectedIndexChanged;
            //
            // spacer
            //
            spacer.Dock = DockStyle.Top;
            spacer.Name = "spacer";
            spacer.Size = new Size(612, 14);
            spacer.TabIndex = 1;
            //
            // itemsCard
            //
            itemsCard.Controls.Add(cartGrid);
            itemsCard.Controls.Add(quickPanel);
            itemsCard.Controls.Add(addItemButton);
            itemsCard.Controls.Add(itemsHeader);
            itemsCard.Dock = DockStyle.Fill;
            itemsCard.Name = "itemsCard";
            itemsCard.Size = new Size(612, 428);
            itemsCard.TabIndex = 2;
            //
            // itemsHeader
            //
            itemsHeader.Controls.Add(itemCountLabel);
            itemsHeader.Controls.Add(itemsTitleLabel);
            itemsHeader.Dock = DockStyle.Top;
            itemsHeader.Name = "itemsHeader";
            itemsHeader.Size = new Size(576, 34);
            itemsHeader.TabIndex = 0;
            //
            // itemsTitleLabel
            //
            itemsTitleLabel.Dock = DockStyle.Left;
            itemsTitleLabel.Font = new Font("Segoe UI Semibold", 11F);
            itemsTitleLabel.Name = "itemsTitleLabel";
            itemsTitleLabel.Size = new Size(200, 34);
            itemsTitleLabel.TabIndex = 0;
            itemsTitleLabel.Text = "Items";
            //
            // itemCountLabel
            //
            itemCountLabel.Dock = DockStyle.Right;
            itemCountLabel.Font = new Font("Segoe UI", 9F);
            itemCountLabel.Name = "itemCountLabel";
            itemCountLabel.Size = new Size(160, 34);
            itemCountLabel.TabIndex = 1;
            itemCountLabel.Tag = "muted";
            itemCountLabel.Text = "0 items";
            itemCountLabel.TextAlign = ContentAlignment.TopRight;
            //
            // addItemButton
            //
            addItemButton.Dock = DockStyle.Bottom;
            addItemButton.Name = "addItemButton";
            addItemButton.Padding = new Padding(14, 0, 0, 0);
            addItemButton.Size = new Size(576, 42);
            addItemButton.TabIndex = 3;
            addItemButton.Tag = "dashed";
            addItemButton.Text = "+  Add item by name";
            addItemButton.TextAlign = ContentAlignment.MiddleLeft;
            addItemButton.Click += addItemButton_Click;
            //
            //
            //
            // cartGrid
            //
            cartGrid.Columns.AddRange(new DataGridViewColumn[] { itemColumn, lessColumn, qtyColumn, moreColumn, priceColumn, subtotalColumn, removeColumn });
            cartGrid.Dock = DockStyle.Fill;
            cartGrid.Name = "cartGrid";
            cartGrid.Size = new Size(576, 326);
            cartGrid.TabIndex = 1;
            cartGrid.CellClick += cartGrid_CellContentClick;
            //
            // rightPanel
            //
            rightPanel.Controls.Add(paymentCard);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Margin = new Padding(0);
            rightPanel.Name = "rightPanel";
            rightPanel.Padding = new Padding(8, 0, 0, 0);
            rightPanel.TabIndex = 1;
            //
            // paymentCard
            //
            paymentCard.Controls.Add(payBody);
            paymentCard.Controls.Add(paymentTitleLabel);
            paymentCard.Dock = DockStyle.Top;
            paymentCard.Name = "paymentCard";
            paymentCard.Size = new Size(356, 470);
            paymentCard.TabIndex = 0;
            //
            // paymentTitleLabel
            //
            paymentTitleLabel.Dock = DockStyle.Top;
            paymentTitleLabel.Font = new Font("Segoe UI Semibold", 11F);
            paymentTitleLabel.Name = "paymentTitleLabel";
            paymentTitleLabel.Size = new Size(320, 34);
            paymentTitleLabel.TabIndex = 0;
            paymentTitleLabel.Text = "Payment";
            //
            // payBody
            //
            payBody.AutoSize = true;
            payBody.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            payBody.Controls.Add(receiptLabel);
            payBody.Controls.Add(segPanel);
            payBody.Controls.Add(dueLabel);
            payBody.Controls.Add(dueDatePicker);
            payBody.Controls.Add(subtotalRow);
            payBody.Controls.Add(totalRow);
            payBody.Controls.Add(creditNoteLabel);
            payBody.Controls.Add(saveButton);
            payBody.Dock = DockStyle.Top;
            payBody.FlowDirection = FlowDirection.TopDown;
            payBody.Name = "payBody";
            payBody.Size = new Size(320, 300);
            payBody.TabIndex = 1;
            payBody.WrapContents = false;
            payBody.SizeChanged += payBody_SizeChanged;
            //
            // segPanel
            //
            segPanel.Controls.Add(creditButton);
            segPanel.Controls.Add(cashButton);
            segPanel.Inset = true;
            segPanel.Margin = new Padding(0, 0, 0, 16);
            segPanel.Padding = new Padding(4);
            segPanel.Radius = 12;
            segPanel.Name = "segPanel";
            segPanel.Size = new Size(320, 44);
            segPanel.TabIndex = 0;
            //
            // cashButton
            //
            cashButton.Dock = DockStyle.Left;
            cashButton.Name = "cashButton";
            cashButton.Size = new Size(160, 36);
            cashButton.TabIndex = 0;
            cashButton.Text = "Cash";
            cashButton.Click += cashButton_Click;
            //
            // creditButton
            //
            creditButton.Dock = DockStyle.Fill;
            creditButton.Name = "creditButton";
            creditButton.Size = new Size(160, 36);
            creditButton.TabIndex = 1;
            creditButton.Text = "Credit";
            creditButton.Click += creditButton_Click;
            //
            // dueLabel
            //
            dueLabel.Font = new Font("Segoe UI Semibold", 9F);
            dueLabel.Margin = new Padding(0, 0, 0, 4);
            dueLabel.Name = "dueLabel";
            dueLabel.Size = new Size(320, 20);
            dueLabel.TabIndex = 2;
            dueLabel.Text = "Due date";
            //
            // dueDatePicker
            //
            dueDatePicker.Margin = new Padding(0, 0, 0, 14);
            dueDatePicker.Name = "dueDatePicker";
            dueDatePicker.Size = new Size(320, 40);
            dueDatePicker.TabIndex = 3;
            dueDatePicker.ValueChanged += dueDatePicker_ValueChanged;
            //
            // subtotalRow
            //
            subtotalRow.Controls.Add(subtotalValueLabel);
            subtotalRow.Controls.Add(subtotalCaptionLabel);
            subtotalRow.Margin = new Padding(0, 0, 0, 6);
            subtotalRow.Name = "subtotalRow";
            subtotalRow.Size = new Size(320, 24);
            subtotalRow.TabIndex = 4;
            //
            // subtotalCaptionLabel
            //
            subtotalCaptionLabel.Dock = DockStyle.Left;
            subtotalCaptionLabel.Name = "subtotalCaptionLabel";
            subtotalCaptionLabel.Size = new Size(140, 22);
            subtotalCaptionLabel.TabIndex = 4;
            subtotalCaptionLabel.Tag = "muted";
            subtotalCaptionLabel.Text = "Subtotal";
            subtotalCaptionLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // subtotalValueLabel
            //
            subtotalValueLabel.Dock = DockStyle.Fill;
            subtotalValueLabel.Name = "subtotalValueLabel";
            subtotalValueLabel.Size = new Size(160, 22);
            subtotalValueLabel.TabIndex = 5;
            subtotalValueLabel.Text = "₱0";
            subtotalValueLabel.TextAlign = ContentAlignment.MiddleRight;
            //
            // totalRow
            //
            totalRow.Controls.Add(totalValueLabel);
            totalRow.Controls.Add(totalCaptionLabel);
            totalRow.Margin = new Padding(0, 0, 0, 12);
            totalRow.Name = "totalRow";
            totalRow.Size = new Size(320, 42);
            totalRow.TabIndex = 5;
            //
            // totalCaptionLabel
            //
            totalCaptionLabel.Dock = DockStyle.Left;
            totalCaptionLabel.Font = new Font("Segoe UI Semibold", 10.5F);
            totalCaptionLabel.Name = "totalCaptionLabel";
            totalCaptionLabel.Size = new Size(120, 34);
            totalCaptionLabel.TabIndex = 6;
            totalCaptionLabel.Text = "Total";
            totalCaptionLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // totalValueLabel
            //
            totalValueLabel.Font = new Font("Segoe UI Semibold", 20F);
            totalValueLabel.Dock = DockStyle.Fill;
            totalValueLabel.Name = "totalValueLabel";
            totalValueLabel.Size = new Size(200, 40);
            totalValueLabel.TabIndex = 7;
            totalValueLabel.Text = "₱0";
            totalValueLabel.TextAlign = ContentAlignment.MiddleRight;
            //
            // creditNoteLabel
            //
            creditNoteLabel.Font = new Font("Segoe UI", 9F);
            creditNoteLabel.Name = "creditNoteLabel";
            creditNoteLabel.Margin = new Padding(0, 0, 0, 12);
            creditNoteLabel.Padding = new Padding(10, 8, 10, 8);
            creditNoteLabel.Size = new Size(320, 56);
            creditNoteLabel.TabIndex = 8;
            creditNoteLabel.Tag = "note";
            creditNoteLabel.Text = "This adds the total to the customer's balance.";
            //
            // saveButton
            //
            saveButton.Margin = new Padding(0);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(320, 44);
            saveButton.TabIndex = 9;
            saveButton.Text = "Save sale";
            saveButton.Click += saveButton_Click;
            //
            // historyCard
            //
            historyCard.Controls.Add(historyGrid);
            historyCard.Controls.Add(historyPager);
            historyCard.Controls.Add(historyToolbar);
            historyCard.Controls.Add(historySubLabel);
            historyCard.Controls.Add(historyTitleLabel);
            historyCard.Dock = DockStyle.Fill;
            historyCard.Name = "historyCard";
            historyCard.Size = new Size(984, 578);
            historyCard.TabIndex = 1;
            historyCard.Visible = false;
            //
            // historyTitleLabel
            //
            historyTitleLabel.Dock = DockStyle.Top;
            historyTitleLabel.Font = new Font("Segoe UI Semibold", 11F);
            historyTitleLabel.Name = "historyTitleLabel";
            historyTitleLabel.Size = new Size(948, 26);
            historyTitleLabel.TabIndex = 0;
            historyTitleLabel.Text = "All sales";
            //
            // historySubLabel
            //
            historySubLabel.Dock = DockStyle.Top;
            historySubLabel.Font = new Font("Segoe UI", 9F);
            historySubLabel.Name = "historySubLabel";
            historySubLabel.Size = new Size(948, 26);
            historySubLabel.TabIndex = 1;
            historySubLabel.Tag = "muted";
            historySubLabel.Text = "Newest first. Search by customer, item, or OR number.";
            //
            // historyToolbar
            //
            historyToolbar.Controls.Add(historyCountLabel);
            historyToolbar.Controls.Add(historySearchBox);
            historyToolbar.Dock = DockStyle.Top;
            historyToolbar.Name = "historyToolbar";
            historyToolbar.Size = new Size(948, 48);
            historyToolbar.TabIndex = 2;
            //
            // historySearchBox
            //
            historySearchBox.Location = new Point(0, 6);
            historySearchBox.Name = "historySearchBox";
            historySearchBox.PlaceholderText = "Search sales";
            historySearchBox.Size = new Size(300, 36);
            historySearchBox.TabIndex = 0;
            historySearchBox.TextChanged += historySearchBox_TextChanged;
            //
            // historyCountLabel
            //
            historyCountLabel.AutoSize = true;
            historyCountLabel.Font = new Font("Segoe UI", 9F);
            historyCountLabel.Location = new Point(314, 15);
            historyCountLabel.Name = "historyCountLabel";
            historyCountLabel.TabIndex = 1;
            historyCountLabel.Tag = "muted";
            historyCountLabel.Text = "0 sales";
            //
            // historyPager
            //
            historyPager.Dock = DockStyle.Bottom;
            historyPager.Name = "historyPager";
            historyPager.Size = new Size(948, 44);
            historyPager.TabIndex = 4;
            //
            // historyGrid
            //
            historyGrid.Columns.AddRange(new DataGridViewColumn[] { receiptColumn, dateColumn, customerColumn, itemsColumn, typeColumn, amountColumn });
            historyGrid.Dock = DockStyle.Fill;
            historyGrid.Name = "historyGrid";
            historyGrid.Size = new Size(948, 408);
            historyGrid.TabIndex = 3;
            //
            // SalesScreen
            //
            // 
            // itemColumn
            // 
            itemColumn.HeaderText = "Item";
            itemColumn.Name = "itemColumn";
            itemColumn.ReadOnly = true;
            itemColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            itemColumn.FillWeight = 260F;
            // 
            // lessColumn
            // 
            lessColumn.HeaderText = "";
            lessColumn.Name = "lessColumn";
            lessColumn.ReadOnly = true;
            lessColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            lessColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            lessColumn.Width = 36;
            // 
            // qtyColumn
            // 
            qtyColumn.HeaderText = "Qty";
            qtyColumn.Name = "qtyColumn";
            qtyColumn.ReadOnly = true;
            qtyColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            qtyColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            qtyColumn.Width = 40;
            // 
            // moreColumn
            // 
            moreColumn.HeaderText = "";
            moreColumn.Name = "moreColumn";
            moreColumn.ReadOnly = true;
            moreColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            moreColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            moreColumn.Width = 36;
            // 
            // priceColumn
            // 
            priceColumn.HeaderText = "Price";
            priceColumn.Name = "priceColumn";
            priceColumn.ReadOnly = true;
            priceColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // subtotalColumn
            // 
            subtotalColumn.HeaderText = "Subtotal";
            subtotalColumn.Name = "subtotalColumn";
            subtotalColumn.ReadOnly = true;
            subtotalColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // removeColumn
            // 
            removeColumn.HeaderText = "";
            removeColumn.Name = "removeColumn";
            removeColumn.ReadOnly = true;
            removeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            removeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            removeColumn.Width = 36;
            // 
            // receiptColumn
            // 
            receiptColumn.HeaderText = "OR no.";
            receiptColumn.Name = "receiptColumn";
            receiptColumn.ReadOnly = true;
            receiptColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // dateColumn
            // 
            dateColumn.HeaderText = "Date";
            dateColumn.Name = "dateColumn";
            dateColumn.ReadOnly = true;
            dateColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // customerColumn
            // 
            customerColumn.HeaderText = "Customer";
            customerColumn.Name = "customerColumn";
            customerColumn.ReadOnly = true;
            customerColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // itemsColumn
            // 
            itemsColumn.HeaderText = "Items";
            itemsColumn.Name = "itemsColumn";
            itemsColumn.ReadOnly = true;
            itemsColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // typeColumn
            // 
            typeColumn.HeaderText = "Type";
            typeColumn.Name = "typeColumn";
            typeColumn.ReadOnly = true;
            typeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // amountColumn
            // 
            amountColumn.HeaderText = "Amount";
            amountColumn.Name = "amountColumn";
            amountColumn.ReadOnly = true;
            amountColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // customerStats
            //
            customerStats.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            customerStats.Location = new Point(50, 74);
            customerStats.Name = "customerStats";
            customerStats.Size = new Size(510, 48);
            customerStats.TabIndex = 5;
            //
            // quickPanel
            //
            quickPanel.Controls.Add(quickAddLabel);
            quickPanel.Dock = DockStyle.Bottom;
            quickPanel.Name = "quickPanel";
            quickPanel.Padding = new Padding(0, 6, 0, 6);
            quickPanel.Size = new Size(576, 48);
            quickPanel.TabIndex = 4;
            quickPanel.WrapContents = false;
            //
            // quickAddLabel
            //
            quickAddLabel.Font = new Font("Segoe UI Semibold", 8.5F);
            quickAddLabel.Margin = new Padding(0, 0, 4, 0);
            quickAddLabel.Name = "quickAddLabel";
            quickAddLabel.Size = new Size(84, 34);
            quickAddLabel.TabIndex = 0;
            quickAddLabel.Tag = "muted";
            quickAddLabel.Text = "Quick add";
            quickAddLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // receiptLabel
            //
            receiptLabel.AutoEllipsis = true;
            receiptLabel.Font = new Font("Segoe UI", 8.5F);
            receiptLabel.Margin = new Padding(0, 0, 0, 6);
            receiptLabel.Name = "receiptLabel";
            receiptLabel.Size = new Size(320, 24);
            receiptLabel.TabIndex = 10;
            receiptLabel.Tag = "muted";
            receiptLabel.Text = "Receipt OR-000000  ·  cashier name";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(bodyPanel);
            Controls.Add(topBar);
            Name = "SalesScreen";
            Size = new Size(984, 628);
            topBar.ResumeLayout(false);
            bodyPanel.ResumeLayout(false);
            layout.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            itemsCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)cartGrid).EndInit();
            itemsHeader.ResumeLayout(false);
            customerCard.ResumeLayout(false);
            customerInset.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            paymentCard.ResumeLayout(false);
            payBody.ResumeLayout(false);
            payBody.PerformLayout();
            historyCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)historyGrid).EndInit();
            historyToolbar.ResumeLayout(false);
            historyToolbar.PerformLayout();
            ResumeLayout(false);
        }

        private Xpera.Controls.MiniStats customerStats;
        private FlowLayoutPanel quickPanel;
        private Label quickAddLabel;
        private Label receiptLabel;
        private DataGridViewTextBoxColumn itemColumn;
        private DataGridViewTextBoxColumn lessColumn;
        private DataGridViewTextBoxColumn qtyColumn;
        private DataGridViewTextBoxColumn moreColumn;
        private DataGridViewTextBoxColumn priceColumn;
        private DataGridViewTextBoxColumn subtotalColumn;
        private DataGridViewTextBoxColumn removeColumn;
        private DataGridViewTextBoxColumn receiptColumn;
        private DataGridViewTextBoxColumn dateColumn;
        private DataGridViewTextBoxColumn customerColumn;
        private DataGridViewTextBoxColumn itemsColumn;
        private DataGridViewTextBoxColumn typeColumn;
        private DataGridViewTextBoxColumn amountColumn;
        private Panel topBar;
        private Xpera.Controls.Card tabsPanel;
        private Xpera.Controls.RoundButton changeButton;
        private Xpera.Controls.RoundButton addItemButton;
        private Xpera.Controls.RoundButton cancelButton;
        private Xpera.Controls.RoundButton historyTab;
        private Xpera.Controls.RoundButton checkoutTab;
        private Panel bodyPanel;
        private TableLayoutPanel layout;
        private Panel leftPanel;
        private Xpera.Controls.Card itemsCard;
        private DataGridView cartGrid;
        private Panel itemsHeader;
        private Label itemCountLabel;
        private Label itemsTitleLabel;
        private Panel spacer;
        private Xpera.Controls.Card customerCard;
        private Xpera.Controls.Card customerInset;
        private ComboBox customerBox;
        private Label customerMetaLabel;
        private Label customerNameLabel;
        private Xpera.Controls.Avatar avatar;
        private Label customerTitleLabel;
        private Panel rightPanel;
        private Xpera.Controls.Card paymentCard;
        private FlowLayoutPanel payBody;
        private Xpera.Controls.Card segPanel;
        private Panel subtotalRow;
        private Panel totalRow;
        private Xpera.Controls.RoundButton saveButton;
        private Label creditNoteLabel;
        private Label totalValueLabel;
        private Label totalCaptionLabel;
        private Label subtotalValueLabel;
        private Label subtotalCaptionLabel;
        private Xpera.Controls.DateField dueDatePicker;
        private Label dueLabel;
        private Xpera.Controls.RoundButton creditButton;
        private Xpera.Controls.RoundButton cashButton;
        private Label paymentTitleLabel;
        private Xpera.Controls.Card historyCard;
        private DataGridView historyGrid;
        private Xpera.Controls.Pager historyPager;
        private Panel historyToolbar;
        private Label historyCountLabel;
        private Xpera.Controls.SearchBox historySearchBox;
        private Label historySubLabel;
        private Label historyTitleLabel;
    }
}
