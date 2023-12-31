﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("GruposCuentas", Schema = "CNT")]
    public class GruposCuentas
    {

        public GruposCuentas()
        {
            this.CatalogoCuenta = new HashSet<CatalogoCuenta>();
        }


        [Key]
        public int IdGrupo { get; set; }
        public required string Nombre { get; set; }


     
        public ICollection<CatalogoCuenta>? CatalogoCuenta { get; set; }

    }
}
