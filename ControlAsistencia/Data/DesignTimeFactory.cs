using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlAsistencia.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlAsistencia.Data
{
    public class DesignTimeFactory : IDesignTimeDbContextFactory<AsistenciaDbContext>
    {
        public AsistenciaDbContext CreateDbContext(string[] args)
            => new AsistenciaDbContext();
    }
}
