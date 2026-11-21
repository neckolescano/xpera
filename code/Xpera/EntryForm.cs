using Xpera.Controls;
using Xpera.Theme;

namespace Xpera
{
    // One reusable pop-up form, in xpera's modal frame. Each screen passes the fields it needs.
    // Problems show in red above the buttons instead of in a second pop-up, so nothing typed is lost.
    public class EntryForm : ModalForm
    {
        private class Field
        {
            public string Label;
            public Label Caption;
            public Control Input;
            public Label Note;
            public Func<int, string> NoteText;
            public Func<string, string> Rule;      // the field's own check (Rules.cs); returns the problem or null
        }

        private readonly List<Field> _fields = new List<Field>();
        private readonly Label _problem = new Label();
        private readonly string _saveText;

        // Answers typed by the user, in the same order as the fields.
        public string[] Values { get; private set; }

        // An extra check before saving, like "the amount is more than they owe". Returns the problem, or null if all is well.
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Func<string[], string> Check { get; set; }

        public EntryForm(string title, string description = "", string saveText = "Save")
            : base(title, description, 480)
        {
            _saveText = saveText;
        }

        // Edit: fills a text field with the record's current value before the form opens.
        public void SetValue(int field, string value)
        {
            _fields[field].Input.Text = value ?? "";
        }

        // Edit: picks an item in a dropdown field (0 = first). -1 or a missing item leaves it as it is.
        public void SetChoice(int field, int index)
        {
            if (index >= 0) ((ChoiceBox)_fields[field].Input).SelectedIndex = index;
        }

        // Which item was picked in a dropdown field (0 = first), counting fields from 0.
        public int ChoiceIndex(int field)
        {
            return ((ChoiceBox)_fields[field].Input).SelectedIndex;
        }

        // Free text, like a question or a comment. maxLength matches the table column.
        public void AddTextField(string label, string hint = "", int maxLength = Rules.QuestionLength)
        {
            AddField(label, MakeInput(hint, "", InputKind.Any, maxLength), text => Rules.CheckText(label, text, maxLength));
        }

        // A person's name: letters, spaces, and . - ' only.
        public void AddNameField(string label, string hint = "")
        {
            AddField(label, MakeInput(hint, "", InputKind.Name, Rules.NameLength), Rules.CheckPersonName);
        }

        // A mobile number: numbers only, and it stops at 11 digits (09565634228).
        public void AddPhoneField(string label, string hint = "Like 09565634228")
        {
            AddField(label, MakeInput(hint, "", InputKind.Digits, Rules.MobileLength), Rules.CheckMobile);
        }

        // A username: lowercase letters, numbers, . and _ (capitals turn lowercase as you type).
        public void AddUsernameField(string label, string hint = "")
        {
            AddField(label, MakeInput(hint, "", InputKind.Username, Rules.UsernameLength), Rules.CheckUsername);
        }

        public void AddPasswordField(string label, string hint = "At least 6 characters")
        {
            AddField(label, MakeInput(hint, "", InputKind.Any, Rules.PasswordMax), Rules.CheckPassword);
        }

        // A money field: a ₱ sign inside the box; only numbers and one dot can be typed.
        public void AddMoneyField(string label, string hint = "0")
        {
            AddField(label, MakeInput(hint, "₱", InputKind.Money, 11), text => Rules.CheckMoney(label, Money.ParsePeso(text)));
        }

        public ChoiceBox AddChoiceField(string label, string[] choices)
        {
            ChoiceBox box = new ChoiceBox();
            box.Searchable = choices.Length > 7;
            foreach (string choice in choices) box.AddChoice(choice);
            AddField(label, box);
            return box;
        }

        // A dropdown whose rows also show a gray line, a value on the right, and initials, like customers with what they owe.
        public ChoiceBox AddChoiceField(string label, IEnumerable<ChoiceBox.Choice> choices, string searchHint)
        {
            ChoiceBox box = new ChoiceBox { SearchHint = searchHint };
            foreach (ChoiceBox.Choice c in choices) box.AddChoice(c.Title, c.Detail, c.Side, c.Badge);
            box.Searchable = box.Count > 7;
            AddField(label, box);
            return box;
        }

        // A small gray line under a dropdown that changes with the choice, like "Due Oct 10 · overdue".
        public void SetNote(int field, Func<int, string> note)
        {
            _fields[field].NoteText = note;
        }

        private static InputBox MakeInput(string hint, string prefix, InputKind kind, int maxLength)
        {
            InputBox input = new InputBox { PlaceholderText = hint, Height = Sizes.Field, Kind = kind, MaxLength = maxLength };
            input.Font = Ui.UiFont(10F, false);
            input.Prefix = prefix;
            input.Padding = new Padding(input.Padding.Left, 11, 12, 6);
            return input;
        }

        private void AddField(string label, Control input, Func<string, string> rule = null)
        {
            input.AccessibleName = label;
            _fields.Add(new Field { Label = label, Input = input, Rule = rule });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int y = ContentTop;
            foreach (Field field in _fields)
            {
                field.Caption = new Label { Text = field.Label, Font = Ui.UiFont(9F, true), AutoSize = false };
                field.Caption.SetBounds(Side, y, ContentWidth, 20);
                Controls.Add(field.Caption);
                y += 23;

                field.Input.SetBounds(Side, y, ContentWidth, Sizes.Field);
                Controls.Add(field.Input);
                y += Sizes.Field;

                if (field.NoteText != null && field.Input is ChoiceBox box)
                {
                    field.Note = new Label { Font = Ui.UiFont(8.5F, false), Tag = "note", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 10, 0) };
                    field.Note.SetBounds(Side, y + 8, ContentWidth, 30);
                    Controls.Add(field.Note);
                    Field f = field;
                    box.SelectedIndexChanged += (sender, args) => f.Note.Text = f.NoteText(box.SelectedIndex);
                    field.Note.Text = field.NoteText(box.SelectedIndex);
                    y += 38;
                }
                y += 16;
            }

            _problem.Font = Ui.UiFont(9F, true);
            _problem.Tag = "keep";
            _problem.ForeColor = Ui.IsDarkMode ? Tints.SignalRed.SoftRose : Shades.SignalRed.Crimson;
            _problem.SetBounds(Side, y - 6, ContentWidth, 38);     // room for a 2-line problem, like the username rule
            Controls.Add(_problem);
            y += 36;

            RoundButton cancel = MakeButton("Cancel", false);
            cancel.DialogResult = DialogResult.Cancel;
            RoundButton save = MakeButton(_saveText, true);
            save.Click += SaveClicked;
            AcceptButton = save;
            CancelButton = cancel;
            PlaceButtons(y, cancel, save);
            Ui.ApplyTheme(this);
            _fields.FirstOrDefault(f => f.Input is InputBox)?.Input.Select();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_fields.FirstOrDefault(f => f.Input is InputBox)?.Input is InputBox first) first.FocusInput();
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            string[] values = _fields.Select(f => f.Input.Text.Trim()).ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Length > 0) continue;
                _problem.Text = "Fill in " + _fields[i].Label.ToLower() + ".";
                if (_fields[i].Input is InputBox box) box.FocusInput(); else _fields[i].Input.Focus();
                return;
            }
            // Then each field's own rule, top to bottom, so the first problem is the one shown.
            for (int i = 0; i < values.Length; i++)
            {
                string wrong = _fields[i].Rule?.Invoke(values[i]);
                if (wrong == null) continue;
                _problem.Text = wrong;
                if (_fields[i].Input is InputBox box) box.FocusInput();
                return;
            }
            string problem = Check?.Invoke(values);
            if (problem != null)
            {
                _problem.Text = problem;
                return;
            }
            Values = values;
            DialogResult = DialogResult.OK;
        }
    }
}
