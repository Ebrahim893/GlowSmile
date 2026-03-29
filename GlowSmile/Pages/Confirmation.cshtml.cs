using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GlowSmile.Data;
using GlowSmile.Models;

namespace GlowSmile.Pages
{
    public class ConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public ConfirmationModel(ApplicationDbContext context) => _context = context;

        public Appointment Appointment { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // بنجيب بيانات الحجز مع اسم الدكتور عشان نعرضهم للمستخدم
            Appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (Appointment == null) return RedirectToPage("Index");

            return Page();
        }
    }
}