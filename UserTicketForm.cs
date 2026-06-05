using DevExpress.XtraEditors;
using DXApplication6;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace HelpDesk_Desktop
{
    public class UserTicketForm : XtraForm
    {
        private readonly int _userId;
        private TextEdit txtTitle = null!;
        private MemoEdit txtDescription = null!;
        private SimpleButton btnSubmit = null!;
        private LabelControl lblStatus = null!;

        public UserTicketForm(int userId)
        {
            _userId = userId;
            SetupUi();
        }

        private void SetupUi()
        {
            Text = "Help Desk - Ariza Bildir";
            Size = new Size(560, 430);
            MinimumSize = new Size(520, 390);
            StartPosition = FormStartPosition.CenterScreen;
            Appearance.BackColor = Color.FromArgb(245, 247, 250);

            LabelControl lblTitle = new LabelControl
            {
                Text = "Ariza Basligi",
                Location = new Point(32, 32),
                Parent = this,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            txtTitle = new TextEdit
            {
                Location = new Point(32, 58),
                Size = new Size(480, 32),
                Parent = this
            };

            LabelControl lblDescription = new LabelControl
            {
                Text = "Ariza Detayi",
                Location = new Point(32, 112),
                Parent = this,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            txtDescription = new MemoEdit
            {
                Location = new Point(32, 138),
                Size = new Size(480, 145),
                Parent = this
            };

            btnSubmit = new SimpleButton
            {
                Text = "ARIZA BILDIR",
                Location = new Point(32, 310),
                Size = new Size(480, 42),
                Parent = this,
                Appearance =
                {
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    Font = new Font("Tahoma", 10, FontStyle.Bold)
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat
            };
            btnSubmit.Click += BtnSubmit_Click;

            lblStatus = new LabelControl
            {
                Location = new Point(32, 365),
                Size = new Size(480, 24),
                Parent = this,
                Text = string.Empty
            };
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                XtraMessageBox.Show("Lutfen baslik ve ariza detayini doldurun.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSubmit.Enabled = false;
            lblStatus.Text = "Talep kaydediliyor...";

            try
            {
                DatabaseManager db = new DatabaseManager();
                bool created = db.CreateTicket(_userId, title, description);

                if (created)
                {
                    txtTitle.Text = string.Empty;
                    txtDescription.Text = string.Empty;
                    lblStatus.Text = "Ariza bildiriminiz kaydedildi.";
                    XtraMessageBox.Show("Ariza bildiriminiz basariyla olusturuldu.", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "Talep kaydedilemedi.";
                    XtraMessageBox.Show("Talep kaydedilemedi. Veritabani bilgilerini kontrol edin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Baglanti hatasi.";
                XtraMessageBox.Show($"Database connection failed:\n{ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSubmit.Enabled = true;
            }
        }
    }
}
