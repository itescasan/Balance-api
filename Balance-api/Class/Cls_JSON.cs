namespace Balance_api.Class
{
    public class Cls_JSON
    {
        public object? d { get; set; }
        public Cls_Msj msj { get; set; }
        public int count { get; set; }
        public int esError { get; set; }


        

    }

    public class Cls_Msj
    {
        public string Codigo { get; set; }
        public string Mensaje { get; set; }

    }



}
