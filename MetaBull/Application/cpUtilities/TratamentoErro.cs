using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cpUtilities
{
   public class TratamentoErro
   {
      private short mshtErro = 0;                     //Quantidade de Erros ocorridos 
      private string[] mstrErro = new string[1000];   //Descrição do Erro 

      //Tratamento de Erros

      ///<summary>
      ///Devolve Quantidade de Erros
      ///</summary>
      ///
      public short ErroQtde
      {
         get
         {
            return mshtErro;
         }
      }

      ///<summary>
      ///Exibe erro
      ///</summary>
      public string Erro(short shtErro)
      {
         if ((shtErro <= mshtErro) && (shtErro != 0))
         {
            return "[Scuti.Autenticacao]" + mstrErro[shtErro];
         }
         else
         {
            return "[Scuti.Autenticacao]{Não há erros para o index informado.}";
         }
      }

      ///<summary>
      ///Exibe erro
      ///</summary>
      private string ErroI(short shtErro)
      {
         if ((shtErro <= mshtErro) && (shtErro != 0))
         {
            return mstrErro[shtErro];
         }
         else
         {
            return "[Scuti.Autenticacao]{Não há erros para o index informado.}";
         }
      }

      ///<summary>
      ///Retorna concatenado todos os erros ocorridos 
      ///</summary>
      public string Erros()
      {
         string strErro = "";
         short shtI;
         short shtErro = this.ErroQtde;

         for (shtI = 1; shtI <= shtErro; shtI++)
         {
            strErro += this.ErroI(shtI);
         }
         return "[Scuti.Autenticacao][" + shtI + "]" + strErro;
      }

      ///<summary>
      ///Tratamento de Erro
      ///</summary>
      public void ErroTrata(string strErro)
      {
         string strRet = strErro;
         strRet = strRet.Replace("'", "");
         strRet = strRet.Replace("\"", "");
         if (strRet != "")
         {
            mshtErro++;
            mstrErro[mshtErro] = strRet;
         }
      }
   }
   
}
