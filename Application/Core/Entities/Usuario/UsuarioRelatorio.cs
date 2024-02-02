using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class UsuarioRelatorio
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public string Patrocinador { get; set; }
        public int NivelAssociacao { get; set; }
        public int NivelClassificacao { get; set; }
        public int StatusID { get; set; }
        public bool GeraBonus { get; set; }
        public bool RecebeBonus { get; set; }
        public bool Bloqueado { get; set; }
        public decimal ValorPernaEsquerda { get; set; }
        public decimal ValorPernaDireita { get; set; }
        public decimal AcumuladoEsquerda { get; set; }
        public decimal AcumuladoDireita { get; set; }
        public decimal ValorSaldo { get; set; }
        public DateTime DataCriacao { get; set; }
        public string UsuarioAtivador { get; set; }
        public decimal ValorAtivacao { get; set; }
        public int TipoAtivacao { get; set; }

    }

    public class UsuarioEntradaDia
    {
        public DateTime Data { get; set; }
        public int QtdeNivel1 { get; set; }
        public int QtdeNivel2 { get; set; }
        public int QtdeNivel3 { get; set; }
        public int QtdeNivel4 { get; set; }
        public int QtdeNivel5 { get; set; }
    }

   public class UsuarioInativo
   {
      public int UsuarioID { get; set; }
      public string Login { get; set; }
      public DateTime DataAtivacao { get; set; }
      public DateTime DataValidade { get; set; }
      public DateTime? DataRenovacao { get; set; }
      public int PatrocinadorID { get; set; }
      public string Patrocinador { get; set; }
      public string Status { get; set; }
      public string Email { get; set; }
      public int NivelAssociacao { get; set; }
      public string Associacao { get; set; }
      public int Nivel { get; set; }
   }


    public class UsuarioMigrarRede
    {
        public int ID { get; set; }
        public int UsuarioID { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public string NomeAssociacao { get; set; }
        //public string Assinatura { get; set; }
        public DateTime DataAtivacao { get; set; }
        public int Linha { get; set; }
        public int Ordem { get; set; }
    }

}
