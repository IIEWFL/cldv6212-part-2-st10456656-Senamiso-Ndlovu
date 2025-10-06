using ABC.Models;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Files.Shares;
using ClosedXML.Excel;

namespace ABC.Services.Storage
{
    public class FileShareStorageService
    {
        //defined fileshare client
        private readonly ShareClient _shareClient;

        //initialise the constructor
        public FileShareStorageService(string storageConnectionString, string shareName)
        {
            _shareClient = new ShareClient(storageConnectionString, shareName);
            _shareClient.CreateIfNotExists();
        }

        // Upload audit logs to a file in the File Share
        public async Task UploadAuditLogAsync(List<AuditLog> logs, string fileName)
        {
            // Create an Excel workbook in memory
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Audit Logs");

            // Header row
            worksheet.Cell(1, 1).Value = "Action";
            worksheet.Cell(1, 2).Value = "Entity";
            worksheet.Cell(1, 3).Value = "Product Name";
            worksheet.Cell(1, 4).Value = "Product ID";
            worksheet.Cell(1, 5).Value = "Queue Inserted At";
            worksheet.Cell(1, 6).Value = "Event Timestamp";

            // Fill data
            for (int i = 0; i < logs.Count; i++)
            {
                var row = i + 2;
                worksheet.Cell(row, 1).Value = logs[i].Action;
                worksheet.Cell(row, 2).Value = logs[i].Entity;
                worksheet.Cell(row, 3).Value = logs[i].Name;
                worksheet.Cell(row, 4).Value = logs[i].Id;
                worksheet.Cell(row, 5).Value = logs[i].InsertionTime?.ToLocalTime().ToString("g");
                worksheet.Cell(row, 6).Value = logs[i].Timestamp?.ToLocalTime().ToString("g");
            }

            // Save workbook to memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            //get the root directory
            var rootDirectory = _shareClient.GetRootDirectoryClient();

            // Get a reference to the file in the file share
            var fileClient = rootDirectory.GetFileClient(fileName);

            // Create or overwrite the file
            await fileClient.CreateAsync(stream.Length);
            await fileClient.UploadAsync(stream);
        }

        // List all files in the root directory
        public async Task<List<string>> ListFilesAsync()
        {
            var fileNames = new List<string>();
            var rootDirectory = _shareClient.GetRootDirectoryClient();

            await foreach (ShareFileItem item in rootDirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    fileNames.Add(item.Name);
                }
            }

            return fileNames;
        }

        // Download a file as a stream
        public async Task<Stream?> DownloadFileAsync(string fileName)
        {
            var rootDirectory = _shareClient.GetRootDirectoryClient();
            var fileClient = rootDirectory.GetFileClient(fileName);

            if (await fileClient.ExistsAsync())
            {
                var download = await fileClient.DownloadAsync();
                return download.Value.Content;
            }

            return null;
        }

    }
}
