using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Sistema
{
    [Table("Usuarios", Schema = "SIS")]
    public class Usuarios
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Usuario { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public byte[] Pass { get; set; }
        public int? IdRol { get; set; }
        public bool Activo { get; set; }
        public int? IdVendedor { get; set; }
        public string? Celular { get; set; }
        public string? Correo { get; set; }
        public bool AccesoWeb { get; set; }
        public bool? ColaImpresionWeb { get; set; }
        public int? IdEmpleado { get; set; }
        public bool Desconectar { get; set; }
        public int? Intento { get; set; }
        public string? CON_Mail_Web { get; set; }
        public DateTime? CON_Mail_Web_Date { get; set; }
        public string? CON_CodMail { get; set; }
    }
}
