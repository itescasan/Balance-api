using DevExpress.CodeParser;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{

    public class ReembolsosD
    {
        [Key]
        public int id { get; set; }
        public string Cuenta { get; set; }
        public string Referencia { get; set; }
        public string idCC { get; set; }       
        public decimal Valor { get; set; }

    }
}
