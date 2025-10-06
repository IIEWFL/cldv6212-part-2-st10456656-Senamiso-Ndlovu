using ABC.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ABC.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly QueueStorageService _queueService;
        private readonly FileShareStorageService _fileShareService;

        public AuditLogController(QueueStorageService queueService,
                                   FileShareStorageService fileShareService)
        {
            _queueService = queueService;
            _fileShareService = fileShareService;
        }

        // GET: /AuditLog
        public async Task<IActionResult> Index()
        {
            var logs = await _queueService.GetLogEntriesAsync();
            return View(logs);
        }

        // POST: /AuditLogs/Export
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export()
        {
            var logs = await _queueService.GetLogEntriesAsync();

            // Optional: include timestamp in filename
            var fileName = $"AuditLog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

            await _fileShareService.UploadAuditLogAsync(logs, fileName);

            TempData["Message"] = $"Audit log exported successfully as {fileName}";
            return RedirectToAction(nameof(Index));
        }

        // NEW: list uploaded files
        public async Task<IActionResult> ViewLogs()
        {
            var files = await _fileShareService.ListFilesAsync();
            return View(files);
        }

        // NEW: download audit logs as CSV (without going through File Share)
        public async Task<IActionResult> DownloadCsv()
        {
            var logs = await _queueService.GetLogEntriesAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Action,Entity,Id,Name,Timestamp");

            foreach (var log in logs)
            {
                csv.AppendLine($"{log.Action},{log.Entity},{log.Id},{log.Name},{log.Timestamp}");
            }

            var fileName = $"AuditLogs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }
    }
}
