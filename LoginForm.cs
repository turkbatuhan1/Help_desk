using DevExpress.XtraEditors;
using DXApplication6;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace HelpDesk_Desktop
{
    public partial class LoginForm : XtraForm
    {
        // DevExpress araçlarımızı tanımlıyoruz
        private TextEdit txtUsername = null!;
        private TextEdit txtPassword = null!;
        private SimpleButton btnLogin = null!;
        private LabelControl lblUser = null!;
        private LabelControl lblPass = null!;

        public LoginForm()
        {
            InitializeComponent();
            SetupCustomUI();
        }

        private void SetupCustomUI()
        {
            // Form Temel Ayarları
            this.Text = "Help Desk - Corporate Login";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Arkaplan rengini kurumsal bir gri yapalım
            this.Appearance.BackColor = Color.FromArgb(240, 240, 240);

            // Kullanıcı Adı Etiketi
            lblUser = new LabelControl
            {
                Text = "Username:",
                Location = new Point(50, 40),
                Parent = this,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            // Kullanıcı Adı Girişi (TextEdit)
            txtUsername = new TextEdit
            {
                Location = new Point(50, 65),
                Size = new Size(300, 35),
                Parent = this
            };
            txtUsername.Properties.ContextImageOptions.Image = DevExpress.Images.ImageResourceCache.Default.GetImage("office2013/users/user_16x16.png");

            // Şifre Etiketi
            lblPass = new LabelControl
            {
                Text = "Password:",
                Location = new Point(50, 115),
                Parent = this,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            // Şifre Girişi (TextEdit)
            txtPassword = new TextEdit
            {
                Location = new Point(50, 140),
                Size = new Size(300, 35),
                Parent = this
            };
            txtPassword.Properties.PasswordChar = '*';
            txtPassword.Properties.ContextImageOptions.Image = DevExpress.Images.ImageResourceCache.Default.GetImage("office2013/actions/key_16x16.png");

            // Giriş Butonu (SimpleButton)
            btnLogin = new SimpleButton
            {
                Text = "LOG IN",
                Location = new Point(50, 200),
                Size = new Size(300, 45),
                Parent = this,
                Appearance = {
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    Font = new Font("Tahoma", 10, FontStyle.Bold)
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat
            };

            // Buton Tıklama Olayı (Event)
            btnLogin.Click += BtnLogin_Click;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            // Boş bırakma kontrolü
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                XtraMessageBox.Show("Please fill in all fields!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DatabaseManager db = new DatabaseManager();
            LoginResult? loginResult;

            try
            {
                loginResult = db.Login(txtUsername.Text.Trim(), txtPassword.Text);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    $"Database connection failed:\n{ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (loginResult != null)
            {
                XtraMessageBox.Show($"Access Granted! Role: {loginResult.Role}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Başarılı girişte yapılacak yönlendirme buraya gelecek
                if (loginResult.Role == "Admin")
                {
                    AdminPanelForm adminPanel = new AdminPanelForm();
                    adminPanel.FormClosed += (_, _) => Show();
                    Hide();
                    adminPanel.Show();
                }
                else
                {
                    UserTicketForm ticketForm = new UserTicketForm(loginResult.UserId);
                    ticketForm.FormClosed += (_, _) => Show();
                    Hide();
                    ticketForm.Show();
                }
            }
            else
            {
                XtraMessageBox.Show("Invalid username or password!", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
