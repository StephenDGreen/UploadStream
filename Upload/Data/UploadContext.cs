using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Upload.Models;

namespace Upload.Data
{
    public class UploadContext : DbContext
    {
        public UploadContext(DbContextOptions<UploadContext> options)
            : base(options)
        {
        }

        public DbSet<AppFile> File { get; set; }
    }
}
