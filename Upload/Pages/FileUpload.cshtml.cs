using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Upload.Data;
using Upload.Models;
using Upload.Utilities;

namespace Upload.Pages
{
    public class FileUploadModel : PageModel
    {
        private readonly UploadContext _context;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".txt" };

        public FileUploadModel(UploadContext context,
            IConfiguration config)
        {
            _context = context;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
        }

        [BindProperty]
        public FileUpload FileUpload { get; set; }

        public string Result { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            // Perform an initial check to catch FileUpload class
            // attribute violations.
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            foreach (var formFile in FileUpload.FormFiles)
            {
                var formFileContent =
                    await FileHelpers.ProcessFormFile<FileUpload>(
                        formFile, ModelState, _permittedExtensions,
                        _fileSizeLimit);

                // Perform a second check to catch ProcessFormFile method
                // violations. If any validation check fails, return to the
                // page.
                if (!ModelState.IsValid)
                {
                    Result = "Please correct the form.";

                    return Page();
                }

                // **WARNING!**
                // In the following example, the file is saved without
                // scanning the file's contents. In most production
                // scenarios, an anti-virus/anti-malware scanner API
                // is used on the file before making the file available
                // for download or for use by other systems. 
                // For more information, see the topic that accompanies 
                // this sample.

                var file = new AppFile()
                {
                    Content = formFileContent,
                    UntrustedName = formFile.FileName,
                    Note = FileUpload.Note,
                    Size = formFile.Length,
                    UploadDT = DateTime.UtcNow
                };

                _context.File.Add(file);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./FileList");
        }
    }

    public class FileUpload
    {
        [Required]
        [Display(Name = "File")]
        public List<IFormFile> FormFiles { get; set; }

        [Display(Name = "Note")]
        [StringLength(50, MinimumLength = 0)]
        public string Note { get; set; }
    }
}