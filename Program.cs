using System;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using DevExpress.LookAndFeel;

namespace HelpDesk_Desktop
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Windows Forms görsel stillerini aktif eder
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // DevExpress temalarını (Skins) yükler
            BonusSkins.Register();
            SkinManager.EnableFormSkins();
            UserLookAndFeel.Default.SetSkinStyle("The Bezier"); // Modern bir tema seçtik

            // Uygulamayı senin yazdığın LoginForm ile başlatır
            Application.Run(new LoginForm());
        }
    }
}