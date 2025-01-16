using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Balance_api.Models.Custom;

namespace Balance_api.Class
{

    public class Cls_Mensaje
    {
        public static string Tojson(object? o, int Length, string CodError, string Mensaje, int esError)
        {
            string json = string.Empty;


            if (o != null)
            {
                json = JsonConvert.SerializeObject(o);
                json = string.Concat("{ \"d\": ", json, ",  \"msj\": ", "{\"Codigo\":\"", CodError, "\",\"Mensaje\":\"", Mensaje, "\"}", ", \"count\":", Length, ", \"esError\":", 0, "}");
            }
            else
                json = string.Concat("{ \"d\":  [{ }],  \"msj\": ", "{\"Codigo\":\"", CodError, "\",\"Mensaje\":\"", Mensaje, "\"}", ", \"count\":", Length, ", \"esError\":", esError, "}");





            return json;
        }

        public static string TojsonT(object? o, int Length, string CodError, string Mensaje, int esError, AutorizacionResponse? Token)
        {
            string json = string.Empty;


            if (o != null)
            {
                json = JsonConvert.SerializeObject(o);
                json = string.Concat("{ \"d\": ", json, ",  \"msj\": ", "{\"Codigo\":\"", CodError, "\",\"Mensaje\":\"", Mensaje, "\"}", ", \"count\":", Length, ", \"esError\":", 0, ", \"token\":", JsonConvert.SerializeObject(Token), "}");
            }
            else
                json = string.Concat("{ \"d\":  [{ }],  \"msj\": ", "{\"Codigo\":\"", CodError, "\",\"Mensaje\":\"", Mensaje.Replace("\r\n", " "), "\"}", ", \"count\":", Length, ", \"esError\":", esError, ", \"token\":", JsonConvert.SerializeObject(Token), "}");





            return json;
        }



        public static string Tojson2(object? o, int Length, string CodError, string Mensaje, bool esError)
        {
            Cls_JSON json = new();
            Cls_Msj msj = new();

            msj.Mensaje = Mensaje;
            msj.Codigo = CodError;

            json.d = o;
            json.msj = msj;
            json.esError = esError ? 1 : 0;
            json.count = Length;


            return JsonConvert.SerializeObject(json);
        }

    }
}