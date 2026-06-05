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
    public class AdminPanelForm : XtraForm
    {
        private GridControl gridTickets = null!;
        private GridView gridViewTickets = null!;
        private ComboBoxEdit cmbStatus = null!;
        private SimpleButton btnUpdateStatus = null!;
        private SimpleButton btnRefresh = null!;
        private LabelControl lblInfo = null!;

        public AdminPanelForm()
        {
            SetupUi();
            LoadTickets();
        }

        private void SetupUi()
        {
            Text = "Help Desk - Admin Panel";
            Size = new Size(980, 620);
            MinimumSize = new Size(840, 520);
            StartPosition = FormStartPosition.CenterScreen;
            Appearance.BackColor = Color.FromArgb(245, 247, 250);

            LabelControl lblTitle = new LabelControl
            {
                Text = "Ariza Talepleri",
                Location = new Point(24, 22),
                Parent = this,
                Font = new Font("Tahoma", 14, FontStyle.Bold)
            };

            btnRefresh = new SimpleButton
            {
                Text = "YENILE",
                Location = new Point(830, 20),
                Size = new Size(110, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Parent = this
            };
            btnRefresh.Click += (_, _) => LoadTickets();

            gridTickets = new GridControl
            {
                Location = new Point(24, 72),
                Size = new Size(916, 390),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Parent = this
            };

            gridViewTickets = new GridView(gridTickets);
            gridTickets.MainView = gridViewTickets;
            gridTickets.ViewCollection.Add(gridViewTickets);
            gridViewTickets.OptionsBehavior.Editable = false;
            gridViewTickets.OptionsSelection.EnableAppearanceFocusedCell = false;
            gridViewTickets.OptionsView.ShowGroupPanel = false;
            gridViewTickets.FocusedRowChanged += (_, _) => SyncSelectedStatus();

            LabelControl lblStatus = new LabelControl
            {
                Text = "Secili Talep Durumu",
                Location = new Point(24, 490),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Parent = this,
                Font = new Font("Tahoma", 9, FontStyle.Bold)
            };

            cmbStatus = new ComboBoxEdit
            {
                Location = new Point(24, 516),
                Size = new Size(220, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Parent = this
            };
            cmbStatus.Properties.Items.AddRange(new[] { "Yeni", "Inceleniyor", "Cozuldu", "Kapatildi" });
            cmbStatus.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

            btnUpdateStatus = new SimpleButton
            {
                Text = "DURUMU GUNCELLE",
                Location = new Point(260, 514),
                Size = new Size(170, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Parent = this,
                Appearance =
                {
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    Font = new Font("Tahoma", 9, FontStyle.Bold)
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat
            };
            btnUpdateStatus.Click += BtnUpdateStatus_Click;

            lblInfo = new LabelControl
            {
                Text = string.Empty,
                Location = new Point(456, 522),
                Size = new Size(480, 24),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Parent = this
            };
        }

        private void LoadTickets()
        {
            try
            {
                DatabaseManager db = new DatabaseManager();
                DataTable tickets = db.GetAllTickets();
                gridTickets.DataSource = tickets;

                ConfigureColumns();
                SyncSelectedStatus();
                lblInfo.Text = $"{tickets.Rows.Count} talep listelendi.";
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Talepler yuklenemedi.";
                XtraMessageBox.Show($"Database connection failed:\n{ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureColumns()
        {
            gridViewTickets.BestFitColumns();

            if (gridViewTickets.Columns["TalepID"] != null)
            {
                gridViewTickets.Columns["TalepID"].Caption = "ID";
                gridViewTickets.Columns["TalepID"].Width = 60;
            }

            if (gridViewTickets.Columns["KullaniciAdi"] != null)
            {
                gridViewTickets.Columns["KullaniciAdi"].Caption = "Kullanici";
            }

            if (gridViewTickets.Columns["Baslik"] != null)
            {
                gridViewTickets.Columns["Baslik"].Caption = "Baslik";
            }

            if (gridViewTickets.Columns["Durum"] != null)
            {
                gridViewTickets.Columns["Durum"].Caption = "Durum";
            }

            if (gridViewTickets.Columns["OlusturmaTarihi"] != null)
            {
                gridViewTickets.Columns["OlusturmaTarihi"].Caption = "Olusturma Tarihi";
                gridViewTickets.Columns["OlusturmaTarihi"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gridViewTickets.Columns["OlusturmaTarihi"].DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
            }
        }

        private void SyncSelectedStatus()
        {
            object value = gridViewTickets.GetFocusedRowCellValue("Durum");
            cmbStatus.EditValue = value?.ToString();
        }

        private void BtnUpdateStatus_Click(object? sender, EventArgs e)
        {
            object idValue = gridViewTickets.GetFocusedRowCellValue("TalepID");
            if (idValue == null || idValue == DBNull.Value)
            {
                XtraMessageBox.Show("Lutfen bir talep secin.", "Secim Yok", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? newStatus = cmbStatus.EditValue?.ToString();
            if (string.IsNullOrWhiteSpace(newStatus))
            {
                XtraMessageBox.Show("Lutfen bir durum secin.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseManager db = new DatabaseManager();
                bool updated = db.UpdateTicketStatus(Convert.ToInt32(idValue), newStatus);

                if (updated)
                {
                    lblInfo.Text = "Talep durumu guncellendi.";
                    LoadTickets();
                }
                else
                {
                    lblInfo.Text = "Talep durumu guncellenemedi.";
                    XtraMessageBox.Show("Talep durumu guncellenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Guncelleme hatasi.";
                XtraMessageBox.Show($"Database connection failed:\n{ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
