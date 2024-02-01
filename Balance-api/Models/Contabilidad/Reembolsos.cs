using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{

    public class Reembolsos
    {
        [Key]        
        public string Titulo { get; set; }        
        
    }
}
