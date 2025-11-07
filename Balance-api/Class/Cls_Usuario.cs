namespace Balance_api.Class
{
    public class Cls_Usuario
    {
        public string User = string.Empty;
        public string Nombre = string.Empty;
        public byte[] Pwd;
        public string Rol = string.Empty;
        public string FechaLogin = string.Empty;
        public bool Desconectar = true;
        public string? CON_CodMail;
        public bool? UpdatePass;
    }
}
