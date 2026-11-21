namespace Xpera
{
    partial class LoginForm
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
            brandPanel = new Panel();
            storeLabel = new Label();
            taglineLabel = new Label();
            wordmarkLabel = new Label();
            logoMark = new LogoMark();
            formPanel = new Panel();
            forgotLabel = new Label();
            signInButton = new Xpera.Controls.RoundButton();
            passwordBox = new Xpera.Controls.InputBox();
            passwordLabel = new Label();
            usernameBox = new Xpera.Controls.InputBox();
            usernameLabel = new Label();
            subtitleLabel = new Label();
            titleLabel = new Label();
            brandPanel.SuspendLayout();
            formPanel.SuspendLayout();
            SuspendLayout();
            //
            // brandPanel
            //
            brandPanel.Controls.Add(storeLabel);
            brandPanel.Controls.Add(taglineLabel);
            brandPanel.Controls.Add(wordmarkLabel);
            brandPanel.Controls.Add(logoMark);
            brandPanel.Dock = DockStyle.Left;
            brandPanel.Location = new Point(0, 0);
            brandPanel.Name = "brandPanel";
            brandPanel.Size = new Size(450, 540);
            brandPanel.TabIndex = 0;
            //
            // logoMark
            //
            logoMark.Location = new Point(46, 50);
            logoMark.Name = "logoMark";
            logoMark.Size = new Size(30, 30);
            logoMark.TabIndex = 0;
            //
            // wordmarkLabel
            //
            wordmarkLabel.Font = new Font("Segoe UI Semibold", 16F);
            wordmarkLabel.Location = new Point(82, 46);
            wordmarkLabel.Name = "wordmarkLabel";
            wordmarkLabel.Size = new Size(120, 38);
            wordmarkLabel.TabIndex = 1;
            wordmarkLabel.Text = "xpera";
            wordmarkLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // taglineLabel
            //
            taglineLabel.Font = new Font("Segoe UI Light", 24F);
            taglineLabel.Location = new Point(40, 380);
            taglineLabel.Name = "taglineLabel";
            taglineLabel.Size = new Size(380, 96);
            taglineLabel.TabIndex = 2;
            taglineLabel.Text = "Level up\r\nevery customer.";
            //
            // storeLabel
            //
            storeLabel.Font = new Font("Segoe UI", 9F);
            storeLabel.Location = new Point(46, 476);
            storeLabel.Name = "storeLabel";
            storeLabel.Size = new Size(360, 22);
            storeLabel.TabIndex = 3;
            storeLabel.Text = "Your username opens your own store.";
            //
            // formPanel
            //
            formPanel.Controls.Add(forgotLabel);
            formPanel.Controls.Add(signInButton);
            formPanel.Controls.Add(passwordBox);
            formPanel.Controls.Add(passwordLabel);
            formPanel.Controls.Add(usernameBox);
            formPanel.Controls.Add(usernameLabel);
            formPanel.Controls.Add(subtitleLabel);
            formPanel.Controls.Add(titleLabel);
            formPanel.Dock = DockStyle.Fill;
            formPanel.Location = new Point(450, 0);
            formPanel.Name = "formPanel";
            formPanel.Size = new Size(450, 540);
            formPanel.TabIndex = 1;
            //
            // titleLabel
            //
            titleLabel.Font = new Font("Segoe UI Semibold", 20F);
            titleLabel.Location = new Point(80, 140);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(300, 46);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Welcome back";
            //
            // subtitleLabel
            //
            subtitleLabel.Location = new Point(84, 188);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new Size(300, 24);
            subtitleLabel.TabIndex = 1;
            subtitleLabel.Text = "Sign in to see today's customers.";
            // 
            // usernameLabel
            // 
            usernameLabel.Font = new Font("Segoe UI Semibold", 9F);
            usernameLabel.Location = new Point(84, 226);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new Size(280, 20);
            usernameLabel.TabIndex = 2;
            usernameLabel.Text = "Username";
            //
            // usernameBox
            //
            usernameBox.Font = new Font("Segoe UI", 11F);
            usernameBox.Location = new Point(84, 248);
            usernameBox.Name = "usernameBox";
            usernameBox.Size = new Size(282, 40);
            usernameBox.TabIndex = 3;
            //
            // passwordLabel
            //
            passwordLabel.Font = new Font("Segoe UI Semibold", 9F);
            passwordLabel.Location = new Point(84, 298);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(280, 20);
            passwordLabel.TabIndex = 4;
            passwordLabel.Text = "Password";
            //
            // passwordBox
            //
            passwordBox.Font = new Font("Segoe UI", 11F);
            passwordBox.Location = new Point(84, 320);
            passwordBox.Name = "passwordBox";
            passwordBox.Size = new Size(282, 40);
            passwordBox.TabIndex = 5;
            passwordBox.UseSystemPasswordChar = true;
            //
            // signInButton
            //
            signInButton.Location = new Point(84, 378);
            signInButton.Name = "signInButton";
            signInButton.Size = new Size(282, 44);
            signInButton.TabIndex = 6;
            signInButton.Text = "Sign in";
            signInButton.Click += signInButton_Click;
            //
            // forgotLabel
            //
            forgotLabel.Font = new Font("Segoe UI", 8.5F);
            forgotLabel.Location = new Point(84, 430);
            forgotLabel.Name = "forgotLabel";
            forgotLabel.Size = new Size(300, 20);
            forgotLabel.TabIndex = 7;
            forgotLabel.Text = "Forgot your password? Ask the store owner to reset it.";
            //
            // LoginForm
            //
            AcceptButton = signInButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 540);
            Controls.Add(formPanel);
            Controls.Add(brandPanel);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "xpera";
            brandPanel.ResumeLayout(false);
            formPanel.ResumeLayout(false);
            formPanel.PerformLayout();
            ResumeLayout(false);
        }

        private Panel brandPanel;
        private Label storeLabel;
        private Label taglineLabel;
        private Label wordmarkLabel;
        private LogoMark logoMark;
        private Panel formPanel;
        private Label forgotLabel;
        private Xpera.Controls.RoundButton signInButton;
        private Xpera.Controls.InputBox passwordBox;
        private Label passwordLabel;
        private Xpera.Controls.InputBox usernameBox;
        private Label usernameLabel;
        private Label subtitleLabel;
        private Label titleLabel;
    }
}
