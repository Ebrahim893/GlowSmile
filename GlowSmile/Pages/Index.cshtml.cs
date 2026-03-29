using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GlowSmile.Data;
using GlowSmile.Models;

namespace GlowSmile.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Service> Services { get; set; }
        // قائمة الدكاترة
        public IList<Doctor> Doctors { get; set; }

        public async Task OnGetAsync()
        {
            Services = await _context.Services.ToListAsync();

            // بنجيب الدكاترة وبنقول للسيستم "هات معاك بيانات الخدمة المرتبطة بكل دكتور"
            Doctors = await _context.Doctors
                .Include(d => d.Service)
                .ToListAsync();
        }
    }
}