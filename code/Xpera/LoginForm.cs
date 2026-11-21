using Xpera.Theme;

namespace Xpera
{
    // Two halves, from the Figma design: the brand on the left, the sign-in form on the right.
    // Only a username and password: the Master database knows which store each username belongs to
    // (table CompanyUsers), so xpera finds the right store database by itself.
    public partial class LoginForm : Form
    {
        // Bone is the brand's background-only neutral.
        private static readonly Color Bone = ColorTranslator.FromHtml("#F4F1EA");

        public LoginForm()
        {
            InitializeComponent();
            Ui.UseBestFonts(this);

            brandPanel.BackColor = Ui.Colors.Secondary;
            wordmarkLabel.ForeColor = Tints.Carbon.White;
            taglineLabel.ForeColor = Tints.Carbon.White;
            storeLabel.ForeColor = Tints.Carbon.MediumGray;

            formPanel.BackColor = Bone;
            titleLabel.ForeColor = Ui.Colors.TextPrimary;
            subtitleLabel.ForeColor = Ui.Colors.TextSecondary;
            usernameLabel.ForeColor = Ui.Colors.TextPrimary;
            passwordLabel.ForeColor = Ui.Colors.TextPrimary;
            forgotLabel.ForeColor = Tints.Carbon.Gray;
            Ui.StylePrimary(signInButton);
            foreach (Control field in new Control[] { usernameBox, passwordBox }) field.Font = Ui.UiFont(10.5F, false);

            // Usernames are lowercase everywhere (Rules.CheckUsername), so typing "Owner" still works.
            usernameBox.CharacterCasing = CharacterCasing.Lower;
            usernameBox.MaxLength = Rules.UsernameLength;
            Shown += (sender, e) => usernameBox.FocusInput();
        }

        private void signInButton_Click(object sender, EventArgs e)
        {
            string username = usernameBox.Text.Trim();
            if (username == "" || passwordBox.Text == "")
            {
                Notice.Show(this, "Missing details", "Please fill in your username and password.", NoticeKind.Warning);
                return;
            }

            try
            {
                // 1. The Master finds which store this username belongs to (or the Master itself, for the Super Admin).
                //    An unknown username gets the same message as a wrong password, so nobody can fish for usernames.
                StoreInfo store = MasterDb.FindStoreForUser(username);
                if (store == null)
                {
                    Notice.Show(this, "Can't sign in", "Wrong username or password.", NoticeKind.Warning);
                    passwordBox.Text = "";
                    return;
                }

                if (store.IsMaster)
                {
                    bool ok = MasterDb.SuperAdminSignIn(usernameBox.Text, passwordBox.Text);
                    if (ok) Notice.Show(this, "Signed in as Super Admin", "The Super Admin screens come in the Final lab.");
                    else Notice.Show(this, "Can't sign in", "Wrong username or password.", NoticeKind.Warning);
                    return;
                }

                if (store.Status != "Active")
                {
                    Notice.Show(this, "Store suspended", store.Name + " is suspended. Ask xpera to reactivate the subscription.", NoticeKind.Warning);
                    return;
                }

                // 2. Everything after this point uses this store's own database, where the password is checked.
                Session.Store = store;
                SignInResult result = StoreDb.SignIn(usernameBox.Text, passwordBox.Text);
                if (result == SignInResult.Locked)
                {
                    Notice.Show(this, "Account locked", "This account is locked after 3 wrong passwords. Ask the store owner to unlock it.", NoticeKind.Warning);
                    return;
                }
                if (result == SignInResult.WrongPassword)
                {
                    Notice.Show(this, "Can't sign in", "Wrong username or password. After 3 wrong tries, the account locks.", NoticeKind.Warning);
                    passwordBox.Text = "";
                    return;
                }
                StoreDb.LogEvent("Signed in");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 18456)
            {
                Notice.Show(this, "Database password is wrong", "Fix it in " + Db.SettingsFile + ", then restart xpera.", NoticeKind.Warning);
                return;
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                Notice.Show(this, "Can't reach the database", "Run code\\Database\\setup.ps1 once, then try again.", NoticeKind.Warning);
                return;
            }

            MainForm main = new MainForm();
            main.ShowStore(Session.Store.Name + " · " + Session.Store.Branch, "Tenant " + Session.Store.Plan + " · " + Session.Store.PlanName);
            // Sign out → back to this window; closing xpera's window → the app ends.
            main.FormClosed += (s, args) =>
            {
                if (!main.SignedOut) { Close(); return; }
                passwordBox.Text = "";
                Show();
                usernameBox.FocusInput();
            };
            main.Show();
            Hide();
        }
    }
}
