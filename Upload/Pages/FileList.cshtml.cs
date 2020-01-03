using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Upload.Data;
using Upload.Models;

namespace Upload.Pages
{
    public class FileListModel : PageModel
    {
        private readonly UploadContext _context;

        public FileListModel(UploadContext context)
        {
            _context = context;
        }

        public IList<AppFile> DatabaseFiles { get; private set; }

        public async Task OnGetAsync()
        {
            DatabaseFiles = await _context.File.Where(f => f.Size < 2000).AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnGetDownloadDbAsync(int? id)
        {
            if (id == null)
            {
                return Page();
            }

            var requestFile = await _context.File.SingleOrDefaultAsync(m => m.Id == id);

            if (requestFile == null)
            {
                return Page();
            }

            // Don't display the untrusted file name in the UI. HTML-encode the value.
            return File(requestFile.Content, MediaTypeNames.Application.Octet, WebUtility.HtmlEncode(requestFile.UntrustedName));
        }
    }
}