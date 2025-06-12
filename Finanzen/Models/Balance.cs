namespace Finanzen.Models
{
    public class Balance
    {
        public int Id { get; set; }

        public decimal Kontostand_Ursprung0 { get; set; }
        public decimal Kontostand_Ursprung { get; set; }
        public decimal Kontostand_Aktuell { get; set; }
        public string ApplicationUserId { get; set; }
    }
}
