using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Finanzen.Models;
using System.Globalization;
public class TransactionCsvMap : ClassMap<Transaction>
{
    public TransactionCsvMap()
    {
        Map(m => m.Buchungstag).Name("Buchungstag")
            .Convert(row =>
            {
                var val = row.Row.GetField("Buchungstag");
                return DateTime.SpecifyKind(DateTime.ParseExact(val, "dd.MM.yyyy", CultureInfo.InvariantCulture), DateTimeKind.Utc);
            });

        Map(m => m.Wertstellung).Name("Wertstellung")
            .Convert(row =>
            {
                var val = row.Row.GetField("Wertstellung");
                return DateTime.SpecifyKind(DateTime.ParseExact(val, "dd.MM.yyyy", CultureInfo.InvariantCulture), DateTimeKind.Utc);
            });


        Map(m => m.Umsatzart).Name("Umsatzart");
        Map(m => m.Buchungstext).Name("Buchungstext");
        Map(m => m.Betrag).Name("Betrag")
            .Convert(row =>
            {
                var raw = row.Row.GetField("Betrag");
                return decimal.Parse(raw, new CultureInfo("de-DE"));
            }); Map(m => m.Währung).Name("Währung");
        Map(m => m.IBANKontoinhaber).Name("IBAN Kontoinhaber");
        Map(m => m.Kategorie).Name("Kategorie");
    }
}
