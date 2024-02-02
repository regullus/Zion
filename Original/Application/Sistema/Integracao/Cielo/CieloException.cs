using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema.Integracao.Models.Cielo
{
    public class CieloException : Exception
    {
        /// <summary>
        /// Código do erro
        /// </summary>
        public string Codigo { get; }

        /// <summary>
        /// Código do erro
        /// </summary>
        public string Descricao { get; }
        /// <summary>
        /// Construtor
        /// </summary>
        public CieloException()
        {
        }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="message">Mensagem de erro</param>
        /// <param name="erroCode">Código do erro</param>
        /// <param name="innerException">Exceção</param>
        public CieloException(string message, string erroCode, string descricao, Exception innerException) : base(message, innerException)
        {
            Codigo = erroCode;
            Descricao = descricao;
        }
    }
}