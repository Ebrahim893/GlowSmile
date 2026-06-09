using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GlowSmile.Data;
using GlowSmile.Models;
using GlowSmile.Services;

namespace GlowSmile.Pages
{
    public class BookingModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public BookingModel(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [BindProperty]
        public Appointment NewAppointment { get; set; }

        public SelectList ServiceList { get; set; }

        public List<SelectListItem> NextSevenDays { get; set; }
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            var services = await _context.Services.ToListAsync();
            ServiceList = new SelectList(services, "ID", "Name");

            NextSevenDays = new List<SelectListItem>();
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Now.AddDays(i);
                string dateText = date.ToString("dddd (dd/MM/yyyy)");
                string dateValue = date.ToString("yyyy-MM-dd");

                NextSevenDays.Add(new SelectListItem { Text = dateText, Value = dateValue });
            }
        }

        public async Task<JsonResult> OnGetDoctorsByServiceAsync(int serviceId)
        {
            var doctors = await _context.Doctors
                .Where(d => d.ServiceID == serviceId)
                .Select(d => new { id = d.ID, name = d.FullName, shift = d.Shift })
                .ToListAsync();
            return new JsonResult(doctors);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("NewAppointment.AssignedTime");

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var existingCount = await _context.Appointments
                .CountAsync(a => a.DoctorID == NewAppointment.DoctorID &&
                                 a.AppointmentDate.Date == NewAppointment.AppointmentDate.Date &&
                                 a.ShiftTime == NewAppointment.ShiftTime);

            if (existingCount >= 5)
            {
                StatusMessage = "Error: This shift is fully booked for the selected doctor.";
                await OnGetAsync();
                return Page();
            }

            _context.Appointments.Add(NewAppointment);
            await _context.SaveChangesAsync();

            string subject = "استلام طلب حجزك - GlowSmile";
            string body = $@"<h2>مرحباً {NewAppointment.PatientName}</h2>
                            <p>نشكرك على اختيار GlowSmile. لقد استلمنا طلب حجزك وسنقوم بمراجعته وتأكيده معك في أقرب وقت.</p>
                            <p><b>رقم الطلب:</b> {NewAppointment.ID}</p>";

            if (!string.IsNullOrEmpty(NewAppointment.PatientEmail))
                await _emailService.SendEmailAsync(NewAppointment.PatientEmail, subject, body);

            return RedirectToPage("Confirmation", new { id = NewAppointment.ID });
        }
    }
}
