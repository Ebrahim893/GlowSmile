using GlowSmile.Data;
using GlowSmile.Models;
using GlowSmile.Services; // تأكد من اسم الـ Namespace الخاص بـ EmailService
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace GlowSmile.Pages.Admin
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailService _emailService;

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment environment, EmailService emailService)
        {
            _context = context;
            _environment = environment;
            _emailService = emailService;
        }

        public IList<Appointment> Appointments { get; set; }
        public IList<Doctor> DoctorsList { get; set; }
        public IList<Service> ServicesList { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalDoctors { get; set; }

        public async Task OnGetAsync()
        {
            Appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            DoctorsList = await _context.Doctors.ToListAsync();
            ServicesList = await _context.Services.ToListAsync();

            TotalAppointments = Appointments.Count;
            TotalDoctors = DoctorsList.Count;
        }

        // --- دالة معالجة الصور باستخدام ImageSharp ---
        private async Task<string> ProcessUploadedFile(IFormFile file, string defaultName)
        {
            if (file == null || file.Length == 0) return defaultName;

            string folderPath = Path.Combine(_environment.WebRootPath, "imgs");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + ".jpg";
            string filePath = Path.Combine(folderPath, fileName);

            // هنا حددنا المسار بالكامل لحل الـ Ambiguous Reference
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(800, 0),
                    Mode = ResizeMode.Max
                }));

                await image.SaveAsJpegAsync(filePath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                {
                    Quality = 75
                });
            }

            return fileName;
        }

        public async Task<IActionResult> OnPostAddDoctorAsync(string FullName, string Bio, string Shift, int ServiceID, int MaxPatients, IFormFile ImageFile)
        {
            string imgName = await ProcessUploadedFile(ImageFile, "default-doctor.png");

            var newDoctor = new Doctor
            {
                FullName = FullName,
                Bio = Bio,
                Shift = Shift,
                ServiceID = ServiceID,
                MaxPatientsPerShift = MaxPatients,
                ImagePath = imgName
            };
            _context.Doctors.Add(newDoctor);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Index");
        }

        public async Task<IActionResult> OnPostAddServiceAsync(string Name, string Description, decimal Price, IFormFile ImageFile)
        {
            string imgName = await ProcessUploadedFile(ImageFile, "default-service.png");

            var newService = new Service
            {
                Name = Name,
                Description = Description,
                Price = Price,
                ImagePath = imgName
            };
            _context.Services.Add(newService);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Index");
        }

        public async Task<IActionResult> OnPostDeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null) { _context.Doctors.Remove(doctor); await _context.SaveChangesAsync(); }
            return RedirectToPage("/Admin/Index");
        }

        public async Task<IActionResult> OnPostDeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null) { _context.Services.Remove(service); await _context.SaveChangesAsync(); }
            return RedirectToPage("/Admin/Index");
        }

        public async Task<IActionResult> OnPostResetDatabaseAsync()
        {
            _context.Appointments.RemoveRange(_context.Appointments);
            _context.Doctors.RemoveRange(_context.Doctors);
            _context.Services.RemoveRange(_context.Services);
            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Appointments', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Doctors', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Services', RESEED, 0)");

            return RedirectToPage("/Admin/Index");
        }

        // --- تأكيد الحجز وإرسال إيميل للعميل ---
        public async Task<IActionResult> OnPostConfirmAsync(int id, string assignedTime)
        {
            var app = await _context.Appointments.Include(a => a.Doctor).FirstOrDefaultAsync(a => a.ID == id);
            if (app != null)
            {
                app.IsConfirmed = true;
                app.AssignedTime = assignedTime;
                await _context.SaveChangesAsync();

                // إرسال إيميل التأكيد
                string subject = "تم تأكيد موعدك في GlowSmile";
                string body = $@"<h2>مرحباً {app.PatientName}</h2>
                                <p>يسعدنا إبلاغك بأنه تم تأكيد حجزك بنجاح.</p>
                                <p><b>الموعد المحدد:</b> {assignedTime}</p>
                                <p><b>الطبيب:</b> د. {app.Doctor?.FullName}</p>
                                <p>نتمنى لك دوام الصحة.</p>";

                if (!string.IsNullOrEmpty(app.PatientEmail))
                    await _emailService.SendEmailAsync(app.PatientEmail, subject, body);
            }
            return RedirectToPage("/Admin/Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var app = await _context.Appointments.FindAsync(id);
            if (app != null) { _context.Appointments.Remove(app); await _context.SaveChangesAsync(); }
            return RedirectToPage("/Admin/Index");
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Admin/Login");
        }

        public async Task<JsonResult> OnGetAvailableSlotsAsync(int doctorId, string date)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return new JsonResult(new List<string>());

            var allPossibleSlots = new List<string>();
            if (doctor.Shift == "Morning") allPossibleSlots = new List<string> { "08:00 AM", "09:00 AM", "10:00 AM", "11:00 AM", "12:00 PM" };
            else if (doctor.Shift == "Afternoon") allPossibleSlots = new List<string> { "01:00 PM", "02:00 PM", "03:00 PM", "04:00 PM" };
            else allPossibleSlots = new List<string> { "05:00 PM", "06:00 PM", "07:00 PM", "08:00 PM", "09:00 PM" };

            var confirmed = await _context.Appointments
                .Where(a => a.DoctorID == doctorId && a.AppointmentDate.Date == DateTime.Parse(date).Date && a.IsConfirmed)
                .Select(a => a.AssignedTime).ToListAsync();

            return new JsonResult(allPossibleSlots.Where(s => !confirmed.Contains(s)).ToList());
        }
    }
}