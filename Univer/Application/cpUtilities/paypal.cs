using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cpUtilities
{
   public class paypal
   {
      public string nome { get; set; } // Exemplo: "Basico";
      public string preco { get; set; } // Exemplo:  "5,99";
      public string qtde { get; set; } // Exemplo:  "1";
      public string sku { get; set; } // Exemplo:  "1"; //Codigo do produto
      public string tax { get; set; } // Exemplo:  "1";
      public string shipping { get; set; } // Exemplo: "1";
      public string subTotal { get; set; } // Exemplo: "5";
      public string descricao { get; set; } // Exemplo: "Plano Basico.";
      public string invoiceNumber { get; set; } // Exemplo:  "idUsuario";
      public string total { get; set; } // Exemplo:  "idUsuario";

   }
}
