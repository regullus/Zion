using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Loja
{
    public class CarrinhoItemModel
    {

        public int Quantidade;
        public Entities.Produto Produto;
        public Entities.ProdutoOpcao Opcao;
        public Entities.ProdutoValor Valor;

    }
}
