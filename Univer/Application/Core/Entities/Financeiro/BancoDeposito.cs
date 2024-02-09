using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class BancoDeposito
    {
        public string Codigo;
        public string Nome;

        public static List<BancoDeposito> Bancos()
        {
            var list = new List<BancoDeposito>();
            list.Add(new BancoDeposito() { Codigo = "001", Nome = "Banco do Brasil" });
            list.Add(new BancoDeposito() { Codigo = "033", Nome = "Santander" });
            list.Add(new BancoDeposito() { Codigo = "237", Nome = "Bradesco" });
            list.Add(new BancoDeposito() { Codigo = "341", Nome = "Itaú" });
            list.Add(new BancoDeposito() { Codigo = "399", Nome = "HSBC" });

            return list;
        }
    }
}
