using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Finanzen.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public DateTime Buchungstag { get; set; }

        public DateTime? Wertstellung { get; set; }

        public string Umsatzart { get; set; }

        public string Buchungstext { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Betrag { get; set; }

        public string Währung { get; set; }

        public string IBANKontoinhaber { get; set; }

        public string Kategorie { get; set; }

        // Verknüpfung zum Benutzer (ASP.NET Identity)
        public string ApplicationUserId { get; set; }
    }
}
