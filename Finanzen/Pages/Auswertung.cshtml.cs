using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Finanzen.Data;
using Finanzen.Models;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Finanzen.Pages
{
    public class AnalyseModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AnalyseModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public DateTime? StartDate { get; set; }

        [BindProperty]
        public DateTime? EndDate { get; set; }

        [BindProperty]
        public string CollapseStatesJson { get; set; }

        [BindProperty]
        public Dictionary<string, bool> CollapseStates { get; set; } = new();
        public Dictionary<string, decimal> ZeitraumAuswertungErgebnisse { get; set; } = new();
        public Dictionary<string, decimal> DurchschnittlicheKostenProKategorie { get; set; } = new();
        public Dictionary<string, decimal> GesamtausgabenProKategorie { get; set; } = new();
        public decimal EinnahmenGesamt { get; set; }
        public decimal EinnahmenDurchschnittProMonat { get; set; }
        public decimal KontostandUrsprung { get; set; }
        public decimal KontostandAktuell { get; set; }
        public bool IsEditingBalance { get; set; }
        public string Fehlermeldung { get; set; } = "";
        public List<Transaction> Transactions { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            await LoadBalance(userId);
            BerechneDurchschnittskosten(Transactions);
            BerechneGesamtausgaben(Transactions);
            BerechneEinnahmen();
            return Page();
        }
        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (!string.IsNullOrEmpty(CollapseStatesJson))
            {
                CollapseStates = JsonSerializer.Deserialize<Dictionary<string, bool>>(CollapseStatesJson);
            }

            await next();
        }
        public async Task<IActionResult> OnPostEditBalanceAsync()
        {
            IsEditingBalance = true;
            var userId = _userManager.GetUserId(User);
            await LoadBalance(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostSaveBalanceAsync(string NewBalance)
        {
            var userId = _userManager.GetUserId(User);
            if (decimal.TryParse(NewBalance.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                var balance = await _context.Balances.FirstOrDefaultAsync(b => b.ApplicationUserId == userId);
                if (balance != null)
                {
                    balance.Kontostand_Ursprung0 = balance.Kontostand_Ursprung;
                    balance.Kontostand_Ursprung = parsed;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                Fehlermeldung = "Ungültige Zahl. Bitte gültiges Format verwenden (z. B. 1234.56)";
                IsEditingBalance = true;
            }

            await LoadBalance(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostResetBalanceAsync()
        {
            var userId = _userManager.GetUserId(User);
            var balance = await _context.Balances.FirstOrDefaultAsync(b => b.ApplicationUserId == userId);
            if (balance != null)
            {
                balance.Kontostand_Ursprung = balance.Kontostand_Ursprung0;
                await _context.SaveChangesAsync();
            }
            await LoadBalance(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostRecalcBalanceAsync()
        {
            var userId = _userManager.GetUserId(User);
            await RecalculateBalanceAsync(userId);
            await LoadBalance(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostZeitraumAnalyse()
        {
            ZeitraumAuswertungErgebnisse = new Dictionary<string, decimal>();

            if (StartDate == null || EndDate == null)
            {
                ModelState.AddModelError(string.Empty, "Bitte gültigen Zeitraum wählen.");
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            var filtered = await _context.Transactions
               .Where(t => t.ApplicationUserId == userId)
               .ToListAsync();

            foreach (var t in filtered)
            {
                t.Buchungstag = DateTime.SpecifyKind(t.Buchungstag, DateTimeKind.Utc);
                if (t.Wertstellung != null)
                    t.Wertstellung = DateTime.SpecifyKind(t.Wertstellung.Value, DateTimeKind.Utc);
            }

            filtered = filtered
                .Where(t => t.Buchungstag >= StartDate.Value && t.Buchungstag <= EndDate.Value)
                .ToList();

            ZeitraumAuswertungErgebnisse = filtered
                .GroupBy(t => t.Kategorie)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Betrag));

            //await LoadBalance(userId);
            await OnGetAsync();
            return Page();
        }

        private async Task LoadBalance(string userId)
        {
            var balance = await _context.Balances.FirstOrDefaultAsync(b => b.ApplicationUserId == userId);
            KontostandUrsprung = balance?.Kontostand_Ursprung ?? 0;
            KontostandAktuell = balance?.Kontostand_Aktuell ?? 0;

            Transactions = await _context.Transactions
                .Where(t => t.ApplicationUserId == userId)
                .OrderByDescending(t => t.Buchungstag)
                .ToListAsync();
        }

        private async Task RecalculateBalanceAsync(string userId)
        {
            var balance = await _context.Balances.FirstOrDefaultAsync(b => b.ApplicationUserId == userId);
            var sum = await _context.Transactions
                .Where(t => t.ApplicationUserId == userId)
                .SumAsync(t => t.Betrag);

            if (balance != null)
            {
                balance.Kontostand_Aktuell = balance.Kontostand_Ursprung + sum;
                await _context.SaveChangesAsync();
            }
        }

        private void BerechneDurchschnittskosten(List<Transaction> transactions)
        {
            var ausgaben = transactions.Where(t => t.Betrag < 0).ToList();

            if (!ausgaben.Any()) return;

            // 1. Zeitraum ermitteln
            var start = new DateTime(ausgaben.Min(t => t.Buchungstag).Year, ausgaben.Min(t => t.Buchungstag).Month, 1);
            var ende = new DateTime(ausgaben.Max(t => t.Buchungstag).Year, ausgaben.Max(t => t.Buchungstag).Month, 1);
            int gesamtMonate = ((ende.Year - start.Year) * 12) + ende.Month - start.Month + 1;

            // 2. Gruppieren nach Kategorie
            var gruppen = ausgaben
                .GroupBy(t => t.Kategorie);

            // 3. Durchschnitt berechnen
            DurchschnittlicheKostenProKategorie = gruppen
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(Math.Abs(g.Sum(t => t.Betrag)) / gesamtMonate, 2)
                );
        }

        private void BerechneGesamtausgaben(List<Transaction> transactions)
        {
            GesamtausgabenProKategorie = transactions
                .Where(t => t.Betrag < 0)
                .GroupBy(t => t.Kategorie)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(Math.Abs(g.Sum(t => t.Betrag)), 2)
                );
        }

        private void BerechneEinnahmen()
        {
            if (Transactions == null || !Transactions.Any())
            {
                EinnahmenGesamt = 0;
                EinnahmenDurchschnittProMonat = 0;
                return;
            }

            var einnahmen = Transactions
                .Where(t => t.Kategorie == "Einnahmen")
                .ToList();

            EinnahmenGesamt = einnahmen.Sum(t => t.Betrag);

            var minDate = Transactions.Min(t => t.Buchungstag);
            var maxDate = Transactions.Max(t => t.Buchungstag);

            int gesamtMonate = ((maxDate.Year - minDate.Year) * 12 + maxDate.Month - minDate.Month) + 1;

            EinnahmenDurchschnittProMonat = gesamtMonate > 0
                ? EinnahmenGesamt / gesamtMonate
                : 0;
        }

    }

}
