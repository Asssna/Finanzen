using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Finanzen.Models;
using Finanzen.Data;
using Microsoft.EntityFrameworkCore;
namespace Finanzen.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public IFormFile CsvFile { get; set; }

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (CsvFile == null || CsvFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Bitte wähle eine gültige Datei.");
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            using var reader = new StreamReader(CsvFile.OpenReadStream());
            using var csv = new CsvHelper.CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            });
            csv.Context.RegisterClassMap<TransactionCsvMap>();
            var records = csv.GetRecords<Transaction>();

            foreach (var record in records)
            {
                try
                {
                    var datum = record.Buchungstag;
                    var wertstellung = record.Wertstellung;
                    var t = new Transaction
                    {
                        Buchungstag = datum,
                        Wertstellung = wertstellung,
                        Umsatzart = record.Umsatzart,
                        Buchungstext = record.Buchungstext,
                        Betrag = record.Betrag,
                        Waehrung = record.Waehrung,
                        IBANKontoinhaber = record.IBANKontoinhaber,
                        Kategorie = record.Kategorie,
                        ApplicationUserId = userId
                    };

                    _context.Transactions.Add(t);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Fehler beim Verarbeiten einer Zeile: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            await RecalculateBalanceAsync(userId);
            return RedirectToPage();
        }

        private async Task RecalculateBalanceAsync(string userId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.ApplicationUserId == userId)
                .ToListAsync();

            var sum = transactions.Sum(t => t.Betrag);

            var balance = await _context.Balances.FirstOrDefaultAsync(b => b.ApplicationUserId == userId);

            if (balance != null)
            {
                balance.Kontostand_Aktuell = balance.Kontostand_Ursprung + sum;
                await _context.SaveChangesAsync();
            }
        }
        public void OnGet()
        {

        }
    }
}
