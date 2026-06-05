using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DXApplication6;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HelpDesk_Desktop
{
    public class UserTicketForm : XtraForm
    {
        private readonly int _userId;
        private readonly DatabaseManager _database = DatabaseManager.Instance;
        private TextEdit txtTitle = null!;
        private MemoEdit txtDescription = null!;
        private SimpleButton btnSubmit = null!;
        private SimpleButton btnRefresh = null!;
        private LabelControl lblStatus = null!;
        private GridControl gridTickets = null!;
        private GridView gridViewTickets = null!;

        public UserTicketForm(int userId)
        {
            _userId = userId;
            SetupUi();
            LoadUserTickets();
        }

        private void SetupUi()
        {
            Text = "Help Desk - Arıza Bildir";
            Size = new Size(860, 640);
            MinimumSize = new Size(720, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Appearance.BackColor = Color.FromArgb(245, 247, 250);

            LabelControl lblTitle = new LabelControl
            {
                Text = "Arıza Başlığı",
                Location = new Point(32, 32),
                Parent = this,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            txtTitle = new TextEdit
            {
                Location = new Point(32, 58),
                Size = new Size(360, 32),
                Parent = this
            };

            LabelControl lblDescription = new LabelControl
            {
                Text = "Arıza Detayı",
                Location = new Point(32, 112),
                Parent = this,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            txtDescription = new MemoEdit
            {
                Location = new Point(32, 138),
                Size = new Size(360, 120),
                Parent = this
            };

            btnSubmit = new SimpleButton
            {
                Text = "ARIZA BİLDİR",
                Location = new Point(32, 280),
                Size = new Size(360, 42),
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
                Location = new Point(32, 332),
                Size = new Size(360, 24),
                Parent = this,
                Text = string.Empty
            };

            LabelControl lblHistory = new LabelControl
            {
                Text = "Geçmiş Taleplerim",
                Location = new Point(430, 32),
                Parent = this,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            btnRefresh = new SimpleButton
            {
                Text = "YENİLE",
                Location = new Point(720, 28),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Parent = this
            };
            btnRefresh.Click += (_, _) => LoadUserTickets();

            gridTickets = new GridControl
            {
                Location = new Point(430, 64),
                Size = new Size(380, 500),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Parent = this
            };

            gridViewTickets = new GridView(gridTickets);
            gridTickets.MainView = gridViewTickets;
            gridTickets.ViewCollection.Add(gridViewTickets);
            gridViewTickets.OptionsBehavior.Editable = false;
            gridViewTickets.OptionsSelection.EnableAppearanceFocusedCell = false;
            gridViewTickets.OptionsView.ShowGroupPanel = false;
        }

        private void LoadUserTickets()
        {
            try
            {
                DataTable tickets = _database.GetTicketsByUser(_userId);
                gridTickets.DataSource = tickets;
                ConfigureTicketColumns();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Talepler yüklenemedi.";
                XtraMessageBox.Show($"Database connection failed:\n{ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureTicketColumns()
        {
            gridViewTickets.BestFitColumns();

            if (gridViewTickets.Columns["TalepID"] != null)
            {
                gridViewTickets.Columns["TalepID"].Caption = "ID";
                gridViewTickets.Columns["TalepID"].Width = 60;
            }

            if (gridViewTickets.Columns["Baslik"] != null)
            {
                gridViewTickets.Columns["Baslik"].Caption = "Başlık";
            }

            if (gridViewTickets.Columns["MesajText"] != null)
            {
                gridViewTickets.Columns["MesajText"].Caption = "Detay";
            }

            if (gridViewTickets.Columns["Durum"] != null)
            {
                gridViewTickets.Columns["Durum"].Caption = "Durum";
            }

            if (gridViewTickets.Columns["OlusturmaTarihi"] != null)
            {
                gridViewTickets.Columns["OlusturmaTarihi"].Caption = "Oluşturma Tarihi";
                gridViewTickets.Columns["OlusturmaTarihi"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gridViewTickets.Columns["OlusturmaTarihi"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
            }
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                XtraMessageBox.Show("Lütfen başlık ve arıza detayını doldurun.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSubmit.Enabled = false;
            lblStatus.Text = "Talep kaydediliyor...";

            try
            {
                bool created = _database.CreateTicket(_userId, title, description);

                if (created)
                {
                    txtTitle.Text = string.Empty;
                    txtDescription.Text = string.Empty;
                    lblStatus.Text = "Arıza bildiriminiz kaydedildi.";
                    LoadUserTickets();
                    XtraMessageBox.Show("Arıza bildiriminiz başarıyla oluşturuldu.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "Talep kaydedilemedi.";
                    XtraMessageBox.Show("Talep kaydedilemedi. Veritabanı bilgilerini kontrol edin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Bağlantı hatası.";
                XtraMessageBox.Show($"Database connection failed:\n{ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSubmit.Enabled = true;
            }
        }
    }
}
