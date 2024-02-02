using Core.Entities;
using Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema.Containers
{
    public struct UsuarioContainer
    {
        private Core.Entities.Usuario _usuario;

        public UsuarioContainer(Core.Entities.Usuario u)
        {
            this._usuario = u;
        }

        public Core.Entities.Usuario Usuario
        {
            get
            {
                return _usuario;
            }
        }

        public double SaldoRentabilidade
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Rentabilidade);
                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public double SaldoBonus
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Bonus);
                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public double SaldoTransferencias
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Transferencias);
                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public double SaldoInvestimento
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Investimento);
                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public double Saldo
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Investimento || l.ContaID == (int)Conta.Contas.Rentabilidade || l.ContaID == (int)Conta.Contas.Transferencias);
                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public double SaldoConta1
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Rentabilidade && l.TipoID != (int)Lancamento.Tipos.Compra);
                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public double SaldoConta2
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Bonus);
                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public double TotalAcumulado
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Rentabilidade && l.Valor >= 0);

                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public double Pontos
        {
            get
            {
                var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Bonus);
                if (lancamentosSaldo != null)
                {
                    return (double)lancamentosSaldo.Sum(l => l.Valor);
                }
                return 0;
            }
        }

        public decimal PontoPosicao
        {
            get
            {
                return 0;
            }
        }

        public double? TetoValorMaximoPacoteAtual
        {
            get
            {
                return this._usuario.Pedido.SelectMany(x => x.PedidoPagamento).Where
                    (x => x.PedidoPagamentoStatus.Any
                    (p => p.Status == PedidoPagamentoStatus.TodosStatus.Pago)).Sum
                    (s => s.Valor) * ConfiguracaoHelper.GetInt("FATOR_MULTIPLICADOR_TETO") / 9720;
            }
        }

        public bool PendenteMigracao
        {
            get
            {
                if (ConfiguracaoHelper.GetBoolean("EXIGI_MIGRACAO"))
                {
                    return _usuario.DataMigracao == null;
                }

                return false;
            }
        }
    }
}