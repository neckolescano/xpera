using Xpera.Controls;

namespace Xpera.Screens
{
    // Record Sale, from the Figma design: who and what on the left, payment on the right.
    // The customer comes first, so every sale goes under a name. Past sales are on their own tab.
    public partial class SalesScreen : UserControl, IThemeAware, IHeaderActions, IContentHeight
    {
        private const string AddItemHint = "+ Add item by name";
        private bool _credit;
        private bool _lastWasCredit;
        private decimal _balance;
        private List<(int Id, string Name)> _customers = new List<(int Id, string Name)>();
        private List<(int Id, string Name, decimal Price, bool Available)> _products = new List<(int Id, string Name, decimal Price, bool Available)>();
        private bool _showingHistory;
        private readonly PagedList _history;


        public SalesScreen()
        {
            InitializeComponent();
            paymentCard.Resize += (sender, e) => LayoutPayment(CartTotal());
            Ui.StyleGrid(cartGrid);
            Ui.StyleGrid(historyGrid);
            Ui.StylePrimary(saveButton);
            Ui.StyleSecondary(cancelButton);

            // The cart and history columns are placed in the Designer; the quantity is centered between its - and + buttons.
            qtyColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            qtyColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Ui.ShowAsTags(historyGrid, "typeColumn");
            _history = new PagedList(historyGrid, historyPager, historyCountLabel, "sales", StoreDb.Sales);

            cartGrid.CellPainting += PaintCartButton;
            cartGrid.Paint += PaintEmptyCart;
            BuildCustomerStats();
            BuildQuickAdd();
            topBar.Visible = false; // Its switch and Cancel button move up to the header.
            dueDatePicker.Value = DateTime.Today.AddDays(30);
            Load += (sender, e) => LoadChoices();
            VisibleChanged += (sender, e) => { if (Visible) LoadChoices(); };
        }

        // The Checkout / Sales history switch and Cancel sit in the header, on the right of the title.
        public Control[] HeaderActions()
        {
            return new Control[] { tabsPanel, cancelButton };
        }

        // Colors that ApplyTheme cannot set by itself: the two switches.
        public void ThemeChanged()
        {
            Ui.StyleChoice(checkoutTab, !_showingHistory);
            Ui.StyleChoice(historyTab, _showingHistory);
            Ui.StyleChoice(cashButton, !_credit);
            Ui.StyleChoice(creditButton, _credit);
            avatar.Invalidate();
        }

        private void LoadChoices()
        {
            // Customers and products come from this store's database.
            string current = customerBox.SelectedItem as string;
            _customers = StoreDb.CustomerChoices();
            _products = StoreDb.ProductChoices();
            customerBox.Items.Clear();
            customerBox.Items.AddRange(_customers.Select(c => c.Name).ToArray());
            if (customerBox.Items.Count > 0)
            {
                customerBox.SelectedItem = current != null && customerBox.Items.Contains(current) ? current : customerBox.Items[0];
            }

            _history.Refresh();
            ShowReceiptLine();
            ThemeChanged();
            UpdateTotals();

            // Lay out the payment card again after the screen has its final size.
            if (IsHandleCreated) BeginInvoke(new Action(() => LayoutPayment(CartTotal())));
        }

        // ---------- customer ----------

        private void customerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (customerBox.SelectedIndex < 0) return;
            var card = StoreDb.CustomerCard(_customers[customerBox.SelectedIndex].Id);
            _balance = card.Balance;

            avatar.Initials = Initials(card.Name);
            customerNameLabel.Text = card.Name;
            customerMetaLabel.Text = CustomerTag(card.Spent, card.LastVisit) + "  ·  usually buys " + (card.UsuallyBuys == "" ? "—" : card.UsuallyBuys);
            customerStats.SetItems(
                StatStrip.Make("Spent", Ui.Peso(card.Spent), ""),
                StatStrip.Make("Last visit", card.LastVisit == null ? "First time" : card.LastVisit.Value.ToString("MMM d"), ""),
                StatStrip.Make("Balance", Ui.Peso(card.Balance), "", card.Balance > 0));
            UpdateTotals();
        }

        // A quick read of who this is: new, drifting away (no visit in 60 days), a top spender, or a regular.
        private static string CustomerTag(decimal spent, DateTime? lastVisit)
        {
            if (lastVisit == null) return "New customer";
            if (lastVisit < DateTime.Today.AddDays(-60)) return "At risk";
            if (spent >= 50000) return "Top customer";
            return "Regular";
        }

        // ---------- items ----------

        // "+ Add item by name" opens a product list you can search. Ones that are not available can't be picked.
        private void addItemButton_Click(object sender, EventArgs e)
        {
            PickerPopup picker = new PickerPopup("Search products");
            Dictionary<int, string> categories = StoreDb.Products().ToDictionary(p => p.Index, p => Convert.ToString(p.Cells[1]));
            foreach (var product in _products)
            {
                string name = product.Name;
                string category = categories.TryGetValue(product.Id, out string c) ? c : "";
                PickerPopup.Item item = picker.AddItem(name, product.Available ? category : category + "  ·  a customer can log an inquiry",
                    product.Available ? Ui.Peso(product.Price) : "Not available", "", product.Available, false, () => AddProduct(name));
                item.Icon = "Products";
            }
            picker.ShowBy(addItemButton, addItemButton.Width);
        }

        // "Change" (or a click anywhere on the customer card) opens the customer list you can search.
        // Each row shows what they usually buy and what they still owe, so the cashier knows before picking.
        private void changeButton_Click(object sender, EventArgs e)
        {
            Dictionary<int, object[]> details = StoreDb.Customers().ToDictionary(c => c.Index, c => c.Cells);
            Dictionary<int, string> owed = StoreDb.Balances().ToDictionary(b => b.Index, b => Convert.ToString(b.Cells[1]));
            PickerPopup picker = new PickerPopup("Search by name or what they buy");
            int currentIndex = customerBox.SelectedIndex;
            for (int i = 0; i < _customers.Count; i++)
            {
                int index = i;
                var customer = _customers[i];
                string detail = details.TryGetValue(customer.Id, out object[] cells) ? Convert.ToString(cells[2]) + "  ·  " + Convert.ToString(cells[1]) : "";
                string side = owed.TryGetValue(customer.Id, out string balance) ? balance + " owed" : "";
                picker.AddItem(customer.Name, detail, side, Initials(customer.Name), true, i == currentIndex, () => customerBox.SelectedIndex = index);
            }
            picker.ShowBy(customerInset, customerInset.Width);
        }

        private static string Initials(string name)
        {
            return string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(w => char.IsLetterOrDigit(w[0])).Take(2).Select(w => char.ToUpper(w[0])));
        }

        private void AddProduct(string product)
        {
            foreach (DataGridViewRow row in cartGrid.Rows)
            {
                if (Convert.ToString(row.Cells["itemColumn"].Value) == product)
                {
                    ChangeQuantity(row, 1);
                    return;
                }
            }

            decimal price = _products.First(p => p.Name == product).Price;
            cartGrid.Rows.Add(product, null, 1, null, Ui.Peso(price), Ui.Peso(price), null);
            cartGrid.CurrentCell = null;
            UpdateTotals();
        }

        // Draws −, +, and × as small rounded boxes so they look like buttons.
        private void PaintCartButton(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string column = cartGrid.Columns[e.ColumnIndex].Name;
            if (column != "lessColumn" && column != "moreColumn" && column != "removeColumn")
            {
                // Plain cells: fill the whole cell first so no old color is left at its edges,
                // then draw the text. No cell border or focus frame.
                using (SolidBrush back = new SolidBrush(Ui.Colors.BackgroundPrimary)) e.Graphics.FillRectangle(back, e.CellBounds);
                e.PaintContent(e.CellBounds);
                e.Handled = true;
                return;
            }

            using (SolidBrush back = new SolidBrush(Ui.Colors.BackgroundPrimary)) e.Graphics.FillRectangle(back, e.CellBounds);
            Rectangle box = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - 26) / 2, e.CellBounds.Y + (e.CellBounds.Height - 26) / 2, 26, 26);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.Drawing2D.GraphicsPath path = Ui.Rounded(box, 7))
            using (Pen border = new Pen(Ui.Colors.BorderPrimary))
            using (SolidBrush fill = new SolidBrush(Ui.Colors.BackgroundPrimary))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(border, path);
            }
            string symbol = column == "lessColumn" ? "−" : column == "moreColumn" ? "+" : "×";
            TextRenderer.DrawText(e.Graphics, symbol, Ui.UiFont(10F, true), box, Ui.Colors.TextPrimary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            e.Handled = true;
        }

        private void cartGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = cartGrid.Rows[e.RowIndex];
            string column = cartGrid.Columns[e.ColumnIndex].Name;

            if (column == "moreColumn") ChangeQuantity(row, 1);
            else if (column == "lessColumn") ChangeQuantity(row, -1);
            else if (column == "removeColumn") { cartGrid.Rows.Remove(row); UpdateTotals(); }
        }

        private void ChangeQuantity(DataGridViewRow row, int change)
        {
            int quantity = Convert.ToInt32(row.Cells["qtyColumn"].Value) + change;
            if (quantity < 1)
            {
                cartGrid.Rows.Remove(row);
            }
            else
            {
                decimal price = Ui.ParsePeso(Convert.ToString(row.Cells["priceColumn"].Value));
                row.Cells["qtyColumn"].Value = quantity;
                row.Cells["subtotalColumn"].Value = Ui.Peso(price * quantity);
            }
            UpdateTotals();
        }

        private decimal CartTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in cartGrid.Rows)
            {
                total += Ui.ParsePeso(Convert.ToString(row.Cells["subtotalColumn"].Value));
            }
            return total;
        }

        // ---------- payment ----------

        private void cashButton_Click(object sender, EventArgs e)
        {
            _credit = false;
            ThemeChanged();
            UpdateTotals();
        }

        private void creditButton_Click(object sender, EventArgs e)
        {
            _credit = true;
            ThemeChanged();
            UpdateTotals();
        }

        private void dueDatePicker_ValueChanged(object sender, EventArgs e)
        {
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            int count = cartGrid.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToInt32(r.Cells["qtyColumn"].Value));
            itemCountLabel.Text = count + (count == 1 ? " item" : " items");

            decimal total = CartTotal();
            subtotalValueLabel.Text = Ui.Peso(total);
            saveButton.Text = total > 0 ? (_credit ? "Save credit sale  ·  " : "Save sale  ·  ") + Ui.Peso(total) : "Save sale";
            cartGrid.Invalidate();
            totalValueLabel.Text = Ui.Peso(total);

            // A credit sale warns how much it adds to what the customer already owes.
            string name = customerBox.SelectedItem as string ?? "";
            string first = name.Split(' ')[0];
            creditNoteLabel.Text = "This adds " + Ui.Peso(total) + " to " + first + "'s balance, due " + dueDatePicker.Value.ToString("MMMM d") +
                ". New balance: " + Ui.Peso(_balance + total) + ".";
            LayoutPayment(total);
        }

        private void payBody_SizeChanged(object sender, EventArgs e)
        {
            // The card is as tall as its title plus the stacked payment parts.
            paymentCard.Height = paymentTitleLabel.Height + payBody.Height + paymentCard.Padding.Vertical;
        }

        // The payment parts stack from top to bottom by themselves (FlowLayoutPanel).
        // This only sets their width and shows the due date and note for credit sales.
        private void LayoutPayment(decimal total)
        {
            int width = paymentCard.ClientSize.Width - paymentCard.Padding.Horizontal;
            if (width < 100) return; // Not laid out yet; this runs again once it has its real size.

            foreach (Control part in payBody.Controls) part.Width = width;
            cashButton.Width = width / 2;
            // The note is as tall as its text needs at this width, so no line gets cut off.
            creditNoteLabel.Height = creditNoteLabel.GetPreferredSize(new Size(width, 0)).Height;

            dueLabel.Visible = _credit;
            dueDatePicker.Visible = _credit;
            creditNoteLabel.Visible = _credit && total > 0;

            payBody.PerformLayout();
            payBody_SizeChanged(payBody, EventArgs.Empty);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (cartGrid.Rows.Count == 0 || customerBox.SelectedItem == null)
            {
                Notice.Show(this, "No items yet", "Add at least one item before saving the sale.", NoticeKind.Warning);
                return;
            }

            // Each cart row becomes a sale item with the product's id, quantity, and price.
            List<(int ProductId, int Quantity, decimal Price)> items = new List<(int ProductId, int Quantity, decimal Price)>();
            foreach (DataGridViewRow row in cartGrid.Rows)
            {
                var product = _products.First(p => p.Name == Convert.ToString(row.Cells["itemColumn"].Value));
                items.Add((product.Id, Convert.ToInt32(row.Cells["qtyColumn"].Value), product.Price));
            }

            string customer = customerBox.SelectedItem.ToString();
            int customerId = _customers[customerBox.SelectedIndex].Id;
            string receipt = StoreDb.SaveSale(customerId, items, _credit, _credit ? dueDatePicker.Value : (DateTime?)null);
            _lastWasCredit = _credit;

            // A credit sale shows up on the Payments screen as a balance.
            ClearSale();
            customerBox_SelectedIndexChanged(customerBox, EventArgs.Empty);
            _history.Refresh();
            ShowReceiptLine();
            Notice.Show(this, "Sale saved", receipt + " is saved under " + customer + (_lastWasCredit ? ". It now shows on the Payments screen." : "."), NoticeKind.Success);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            ClearSale();
        }

        private void ClearSale()
        {
            cartGrid.Rows.Clear();
            _credit = false;
            dueDatePicker.Value = DateTime.Today.AddDays(30);
            ThemeChanged();
            UpdateTotals();
        }

        // ---------- added look ----------

        // A size in pixels at 100% display scaling, made right for this screen (125% makes 10 into 13).
        private int Px(float value)
        {
            return (int)Math.Round(value * DeviceDpi / 96f);
        }

        // Spent, last visit, and balance sit on the right of the customer card, so the cashier
        // sees who this is before adding anything. A balance shows in red.
        private void BuildCustomerStats()
        {
            foreach (Control part in new Control[] { customerInset, avatar, customerNameLabel, customerMetaLabel, customerStats })
            {
                part.Click += changeButton_Click;
                part.Cursor = Cursors.Hand;
            }
            customerInset.Resize += (sender, e) => PlaceCustomerStats();
            PlaceCustomerStats();
        }

        // The numbers get their own row under the name, so they have room on any window size.
        private void PlaceCustomerStats()
        {
            int left = customerNameLabel.Left - 14;
            customerStats.SetBounds(left, Px(74), Math.Max(200, customerInset.ClientSize.Width - left - Px(12)), Px(48));
            customerNameLabel.Width = Math.Max(100, changeButton.Left - 8 - customerNameLabel.Left);
            customerMetaLabel.Width = customerNameLabel.Width;
            customerMetaLabel.Height = 20;
            customerStats.BringToFront();
        }

        // One-click chips for products that are available, so common items need no searching.
        private void BuildQuickAdd()
        {
            quickPanel.Resize += (sender, e) => FitQuickAdd();
        }

        private void ShowQuickAdd()
        {
            // The "Quick add" label is in the Designer; the chips depend on which products are available.
            foreach (Control chip in quickPanel.Controls.OfType<RoundButton>().ToList())
            {
                quickPanel.Controls.Remove(chip);
                chip.Dispose();
            }
            foreach (var product in _products.Where(p => p.Available).Take(5))
            {
                string name = product.Name;
                RoundButton chip = new RoundButton { Text = "+ " + name, Radius = Px(17), Height = Px(34), Margin = new Padding(0, 0, Px(8), 0) };
                chip.Font = Ui.UiFont(8.5F, false);
                chip.Width = TextRenderer.MeasureText(chip.Text, chip.Font).Width + Px(26);
                chip.Click += (sender, e) => AddProduct(name);
                quickPanel.Controls.Add(chip);
            }
            Ui.ApplyTheme(quickPanel);
            FitQuickAdd();
        }

        // Only whole chips show. One that would be cut off at the right edge waits for a wider window.
        private void FitQuickAdd()
        {
            int x = 0;
            foreach (Control chip in quickPanel.Controls)
            {
                x += chip.Width + chip.Margin.Horizontal;
                chip.Visible = x <= quickPanel.ClientSize.Width;
            }
        }

        // The cart says what to do when it is empty, instead of showing a blank box.
        private void PaintEmptyCart(object sender, PaintEventArgs e)
        {
            if (cartGrid.Rows.Count > 0) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int top = cartGrid.ColumnHeadersHeight;
            Rectangle area = new Rectangle(0, top, cartGrid.Width, cartGrid.Height - top);
            Rectangle circle = new Rectangle(area.X + (area.Width - 52) / 2, area.Y + area.Height / 2 - 56, 52, 52);
            using (Pen dash = new Pen(Ui.Colors.BorderPrimary, 1.5f) { DashPattern = new float[] { 3, 3 } }) g.DrawEllipse(dash, circle);
            MenuButton.DrawIcon(g, "Sales", new Rectangle(circle.X + 17, circle.Y + 17, 18, 18), Ui.Colors.TextSecondary);
            TextRenderer.DrawText(g, "No items yet", Ui.UiFont(10.5F, true), new Rectangle(area.X, circle.Bottom + 12, area.Width, 22), Ui.Colors.TextPrimary, TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(g, "Use Quick add, or search for a product below.", Ui.UiFont(9F, false), new Rectangle(area.X, circle.Bottom + 36, area.Width, 20), Ui.Colors.TextSecondary, TextFormatFlags.HorizontalCenter);
        }

        // The next receipt number and who is at the counter, at the top of the payment card.
        private void ShowReceiptLine()
        {
            receiptLabel.Text = "Receipt " + StoreDb.NextReceiptNo() + "  ·  " + (Session.UserName ?? "—");
            ShowQuickAdd();
        }

        // ---------- tabs ----------

        private void checkoutTab_Click(object sender, EventArgs e)
        {
            ShowHistory(false);
        }

        private void historyTab_Click(object sender, EventArgs e)
        {
            ShowHistory(true);
        }

        private void ShowHistory(bool show)
        {
            _showingHistory = show;
            historyCard.Visible = show;
            layout.Visible = !show;
            cancelButton.Visible = !show;
            if (show) _history.Refresh();
            ThemeChanged();
            PagedList.HeightChanged();
        }

        // Checkout needs about 600 px to show the cart and the payment card without squeezing;
        // the history tab is as tall as its card.
        public int ContentHeight
        {
            get { return _showingHistory ? historyCard.Height + historyCard.Margin.Vertical : Px(600); }
        }

        private void historySearchBox_TextChanged(object sender, EventArgs e)
        {
            _history.Search(historySearchBox.Text);
        }
    }
}
