using Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Core.Repositories.Financeiro;
using System.Data.Entity;

namespace Core.Services.Financeiro
{
   public class ImpostosService
   {
      private IRRepository irRepository;

      public ImpostosService(DbContext context)
      {
         irRepository = new IRRepository(context);
      }

      /// <summary>
      /// Calcula o valor do INSS sobre um dado valor
      /// </summary>
      /// <param name="valor"></param>
      /// <returns></returns>
      public decimal CalculoINSS(decimal valor)
      {
         decimal decRetorno = 0;

         //CalculoINSS é 11% EmailService todo o pais, esta fixo aqui
         decRetorno = (valor * 0.11m);
         return decRetorno;
      }

      public double CalculoIR(double valor, double valorINSS)
      {

         double decDeduzir = 0;
         double decAliquota = 0;
         double decRetorno = 0;

         //Para o calculo do IR deve-se obter o valor base de calculo retirando do valor bruto o INSS já descontato
         decRetorno = valor - valorINSS;

         var ListaIR = irRepository.GetByValor(decRetorno).ToList();

         foreach (var itemIR in ListaIR)
         {
            decAliquota = (double)itemIR.Aliquota;
            decDeduzir  = (double)itemIR.Deduzir;
         }
         //Calculo do IR é obter a aliquota dependedo do valor de base de calculo e subtrair o valor Deduzir da tabela
         decRetorno = decRetorno * (decAliquota / 100.0) - decDeduzir;

         //CalculoINSS é 11% EmailService todo o pais, esta fixo aqui
         //decRetorno = (valor * 0.11);
         return (double)decRetorno;

      }



   }
}
