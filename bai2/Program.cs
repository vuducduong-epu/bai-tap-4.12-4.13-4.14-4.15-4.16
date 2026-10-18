using System;
using System.Windows.Forms;

namespace GymMemberManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new frmGymMemberManager());
        }
    }
}
