//function parseDate(dateStr) {
//    const parts = dateStr.split(".");
//    return new Date(parts[2], parts[1] - 1, parts[0]); // yyyy, mm-1, dd
//}

let sortDirections = {};

document.querySelectorAll("form").forEach(form => {
    form.addEventListener("submit", () => {
        const states = {};
        document.querySelectorAll(".collapse").forEach(c => {
            states[c.id] = c.classList.contains("show");
        });
        const hiddenInput = document.createElement("input");
        hiddenInput.type = "hidden";
        hiddenInput.name = "CollapseStatesJson";
        hiddenInput.value = JSON.stringify(states);
        form.appendChild(hiddenInput);
    });
});
function parseDate(dateStr) {
    const parts = dateStr.split(".");
    if (parts.length !== 3) return new Date(0); // ungültiges Datum → ganz nach oben
    const [day, month, year] = parts.map(Number);
    return new Date(year, month - 1, day);
}

function sortTable(columnIndex) {
    const table = document.getElementById('buchungenTable');
    const rows = Array.from(table.tBodies[0].rows);
    const ascending = !sortDirections[columnIndex];
    sortDirections[columnIndex] = ascending;

    // Reset Pfeile
    table.querySelectorAll("th span").forEach(s => s.textContent = "");
    const arrow = ascending ? "↑" : "↓";
    table.tHead.rows[0].cells[columnIndex].querySelector("span").textContent = arrow;

    // Datumsspalten erkennen (z. B. 0 = Buchungstag, 1 = Wertstellung)
    const isDateColumn = (columnIndex === 0 || columnIndex === 1);

    // Sortieren
    rows.sort((a, b) => {
        let aText = a.cells[columnIndex].textContent.trim();
        let bText = b.cells[columnIndex].textContent.trim();

        if (isDateColumn) {
            const aDate = parseDate(aText);
            const bDate = parseDate(bText);
            return ascending ? aDate - bDate : bDate - aDate;
        }

        const aNum = parseFloat(aText.replace(/[^\d,-]/g, '').replace(',', '.'));
        const bNum = parseFloat(bText.replace(/[^\d,-]/g, '').replace(',', '.'));

        if (!isNaN(aNum) && !isNaN(bNum)) return ascending ? aNum - bNum : bNum - aNum;
        return ascending ? aText.localeCompare(bText) : bText.localeCompare(aText);
    });

    rows.forEach(r => table.tBodies[0].appendChild(r));
}

// Filterfunktion
document.addEventListener('DOMContentLoaded', function () {
    const input = document.getElementById('filterInput');
    const table = document.getElementById('buchungenTable');
    if (!input || !table) return;

    input.addEventListener('input', function () {
        const filter = input.value.toLowerCase();
        for (const row of table.tBodies[0].rows) {
            const text = Array.from(row.cells).map(td => td.textContent.toLowerCase()).join(' ');
            row.style.display = text.includes(filter) ? '' : 'none';
        }
    });
});