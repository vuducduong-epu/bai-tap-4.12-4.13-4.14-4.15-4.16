
namespace CryptoStockTicker
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new frmCryptoStockTicker());
        }
    }
}
