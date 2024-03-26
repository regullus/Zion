#region Bibliotecas

using System;
//Acesso ao Banco de Dados
using System.Data;
using System.Data.SqlClient;
//Acesso ao Registry
using Microsoft.Win32;
//Upload
using System.Reflection;
//Email
using System.Web;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Configuration;
//Criptografia
using System.Security.Cryptography;
using System.Text;
//WebService
using System.Net;
//EventLog
using System.Diagnostics;
using System.Xml;
//Cripto
using System.IO;
//Regular Expressions
using System.Text.RegularExpressions;
using System.DirectoryServices;
//Acesso ao web.config
using System.Configuration;
using System.Security.Claims;
using System.Threading;
using System.Web.Configuration;

#endregion

namespace cpUtilities
{

    #region Enun

    /// <summary>
    /// Usado para definir o status do registro na base de dados.
    /// </summary>
    public enum StatusRegistro
    {
        // Ativo
        Ativo = 1,
        // Desativado
        Desativado = 2,
        // Excluido
        Excluido = 3
    }

    /// <summary>
    /// Usado para definir o status do registro na base de dados.
    /// </summary>
    public enum StatusLead
    {
        TomarAcao = 1,
        EntrarEmContato = 2,
        ContatoEfetuado = 3,
        AguardandoRetorno = 4,
        RetomarContato = 5,
        NaoHouveInteresse = 6,
        DadosIncorretos = 7,
        VendaEfetuada = 8,
        Remover = 9,
        JaAdquiriu = 10
    }

    /// <summary>
    /// Usado para definir o status do registro na base de dados.
    /// </summary>
    public enum QualificacaoLead
    {
        Acesso = 1,
        SolicitadoContato = 2
    }

    /// <summary>
    /// Usado para definir o status do registro na base de dados.
    /// </summary>
    public enum tipoSituacaoAnuncio
    {
        // Ativo
        Ativado = 1,
        // Desativado
        Desativado = 2
    }

    public enum tipoChamada
    {
        Facebook = 1,
        Google = 2,
        Publicidade = 3,
        Simulador = 4,
        Denakop = 5
    }

    /// <summary>
    /// Usado para definir a ação que será executada.<br></br>
    /// </summary>
    public enum TipoCriptografia
    {
        /// <summary>
        /// Criptografa o dado.
        /// </summary>
        Criptografa = 0,
        /// <summary>
        /// Descriptografa o dado.
        /// </summary>
        Descriptografa = 1
    }

    public enum PerfilUsuario
    {
        /// <summary>
        /// Administrador
        /// </summary>
        Master = 1,

        /// <summary>
        /// Vendedor
        /// </summary>
        Vendedor = 2,

        /// <summary>
        /// Gerente.
        /// </summary>
        Gerente = 3,

        /// <summary>
        /// Administrador.
        /// </summary>
        Administrador = 4,

        /// <summary>
        /// Administrativo.
        /// </summary>
        Administrativo = 5,

        /// <summary>
        /// Regional.
        /// </summary>
        Regional = 6

    }

    public enum TipoPrioridade
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Baixa.
        /// </summary>
        Baixa = 1,

        /// <summary>
        /// Alta.
        /// </summary>
        Alta = 2

    }

    public enum TipoFormato
    {
        /// <summary>
        /// Text
        /// </summary>
        Text = 0,

        /// <summary>
        /// Html.
        /// </summary>
        Html = 1
    }

    public enum TipoAtivo
    {
        /// <summary>
        /// Sim
        /// </summary>
        Sim = 1,
        /// <summary>
        /// Não
        /// </summary>
        Nao = 2,
        /// <summary>
        /// Não esta Bloqueado
        /// </summary>
        Bloqueado = 3,
        /// <summary>
        /// Não esta em dia com o pagamento
        /// </summary>
        Devedor = 4,
        /// <summary>
        /// Importado de outro sistema
        /// </summary>
        Importado = 5
    }

    public enum TipoGrupo
    {
        /// <summary>
        /// Developer
        /// </summary>
        Developer = 1,
        /// <summary>
        /// Administrador
        /// </summary>
        Administrador = 2,
        /// <summary>
        /// Administrativo
        /// </summary>
        Administrativo = 3,
        /// <summary>
        /// Regional
        /// </summary>
        Regional = 4,
        /// <summary>
        /// Gerente
        /// </summary>
        Gerente = 5,
        /// <summary>
        /// Vendedor
        /// </summary>
        Vendedor = 6,
        /// <summary>
        /// Gerente Administrador
        /// </summary>
        GerenteAdministrador = 8,
        /// <summary>
        /// Usuário
        /// </summary>
        Usuario = 9
    }

    public enum PublicoUsuario
    {
        id = 1
    }

    public enum TipoPublico
    {
        Geral = 1,
        Vendedor = 2,
        Gerente = 3
    }

    public enum TipoImagem
    {
        Produto = 1,
        Usuario = 2,
        Email = 3,
        Filial = 4
    }

    public enum Empresa
    {
        MediaTron = 1,
        Realiza = 2,
        D9 = 3,
        Prospera = 4
    }

    public enum TipoPlano
    {
        Nenhuma = 1,
        Basico = 2,
        Intermediario = 3,
        Avancado = 4,
        Enterprise = 5,
        Basico3Meses = 2,
        Intermediario3Meses = 5,
        Avancado3Meses = 8
    }

    public enum TipoMensagem
    {
        Todos = 1,
        Usuario = 2,
        Vendedor = 3,
        Gerente = 4,
        Regional = 5,
        GerenteMaster = 6,
        Administrativo = 7,
        Administrador = 8,
        Master = 9,
        Supervisor = 10
    }

    public enum TipoSituacaoTrona
    {
        Ativado = 1,
        Desativado = 2
    }

    public enum TipoSituacaoAnuncio
    {
        Ativado = 1,
        Desativado = 2
    }

    public enum TipoArquivoSecao
    {
        Banner = 1,
        Documento = 2
    }

    public enum TipoArquivo
    {
        Imagem = 1,
        Documento = 2,
        Excel = 3,
        PDF = 4,
        Audio = 5,
        Video = 6,
        PowerPoint = 7,
        ZIP = 8,
        File = 9
    }

    // ===

    public enum TipoS
    {
        Ativado = 1,
        Desativado = 2
    }


    #endregion

    #region Gerais

    /// <summary>
    /// Funcoes gerais<br></br>
    /// </summary>
    public class Gerais : cpUtilities.TratamentoErro
    {

        #region Declaracoes

        const string cstrPastaEventLog = "scuti";
        private string cstrSistema = "";

        #endregion

        #region Propriedades

        /// <summary>
        /// Retorna o nome do sistema
        /// </summary>
        public static string Sistema
        {
            get
            {
                return ConfigurationManager.AppSettings["Sistema"];
            }
        }

        /// <summary>
        /// Retorna a versao do sistema
        /// </summary>
        public static string Versao
        {
            get
            {
                return ConfigurationManager.AppSettings["Versao"];
            }
        }

        /// <summary>
        /// Retorna o Ambiente onde o sistema se encontra: Desenvolvimento, Homologação ou 
        /// Produção
        /// </summary>
        public static string Ambiente
        {
            get
            {
                return ConfigurationManager.AppSettings["Ambiente"];
            }
        }

        /// <summary>
        /// Retorna a primeira letra do Ambiente onde o sistema se encontra: 
        /// D - Desenvolvimento, H - Homologação ou P - Produção
        /// </summary>
        public static string AmbienteLetra
        {
            get
            {
                string strReturno = ConfigurationManager.AppSettings["Ambiente"];
                strReturno = strReturno.Substring(0, 1).ToUpper();
                return strReturno;
            }
        }

        /// <summary>
        /// Retorna o ip do Servidor de SMTP
        /// </summary>
        public static string smtpIP
        {
            get
            {
                return ConfigurationManager.AppSettings["smtpIP"];
            }
        }

        /// <summary>
        /// Retorna a porta do Servidor de SMTP
        /// </summary>
        public static string smtpPorta
        {
            get
            {
                return ConfigurationManager.AppSettings["smtpPort"];
            }
        }

        /// <summary>
        /// Retorna o usuario do Servidor de SMTP
        /// </summary>
        public static string smtpUser
        {
            get
            {
                return ConfigurationManager.AppSettings["smtpUser"];
            }
        }

        /// <summary>
        /// Retorna a senha do Servidor de SMTP
        /// </summary>
        public static string smtpPassword
        {
            get
            {
                string strPassword = ConfigurationManager.AppSettings["smtpUser"];
                //strPassword = Morpho("o1Pdpdpep2#22!", "Ava2014", TipoCriptografia.Criptografa);
                strPassword = Morpho(strPassword, "Ava2014", 0);
                return strPassword;
            }
        }

        #endregion

        #region Metodos

        /// <summary> 
        /// Validacao de endereço IP 
        /// </summary> 
        /// <param name="strIP">IP</param> 
        /// <returns>True se validado</returns> 
        /// <remarks>Validacao de endereço IP</remarks> 
        /// <history> 
        /// Marcelo 
        /// </history> 
        public static bool IP(string strIP)
        {
            try
            {
                Regex objRegEx = default(Regex);
                objRegEx = new Regex("^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$");
                bool returnBoolean = false;
                returnBoolean = objRegEx.IsMatch(strIP);
                return returnBoolean;
            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.IP]" + ex.Message, ex);
            }
        }

        /// <summary> 
        /// Verifica se há entradas maliciosas de SQL Injection 
        /// </summary> 
        /// <param name="strEntr">String a ser Verificada a entrada</param> 
        /// <returns>True se há tentativa ocorreu</returns> 
        /// <remarks> 
        /// </remarks> 
        /// <history> 
        /// Marcelo 
        /// </history> 
        public static string VerificarSQLInjection(string strEntr)
        {
            string[] strMal = new string[22];
            string strAchou = "";
            short shtI = 0;

            try
            {
                strMal[0] = "--";
                //Comentario 
                strMal[1] = "SELECT";
                // retrieve rows from a table or view 
                strMal[2] = "DELETE";
                // delete rows of a table 
                strMal[3] = "INSERT";
                // create new rows in a table 
                strMal[4] = "UPDATE";
                // update rows of a table 
                strMal[5] = "DROP";
                // remove a user-defined aggregate function 
                strMal[6] = "ALTER";
                // add users to a group or remove users from a group 
                strMal[7] = "ANALYZE";
                // collect statistics about a database 
                strMal[8] = "BEGIN";
                // start a transaction block 
                strMal[9] = "COMMIT";
                // commit the current transaction 
                strMal[10] = "CREATE";
                // define a new aggregate function 
                strMal[11] = "DEALLOCATE";
                // remove a prepared query 
                strMal[12] = "DECLARE";
                // define a cursor 
                strMal[13] = "EXECUTE";
                // execute a prepared query 
                strMal[14] = "EXPLAIN";
                // show the execution plan of a statement 
                strMal[15] = "GRANT";
                // define access privileges 
                strMal[16] = "ROLLBACK";
                // abort the current transaction 
                strMal[17] = "TRANSACTION";
                // start a transaction block 
                strMal[18] = "TRUNCATE";
                // empty a table 
                strMal[19] = "<";
                // empty a table 
                strMal[20] = ">";
                // empty a table 

                for (shtI = 0; shtI <= 20; shtI++)
                {
                    if (strEntr.IndexOf(strMal[shtI]) >= 0)
                    {
                        strAchou = strMal[shtI];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.VerificarSQLInjection]" + ex.Message, ex);
            }
            return strAchou;
        }

        /// <summary>
        /// Formata a data atual no formato YYYYMMDD.
        /// </summary>
        /// <returns>Data Atual.</returns>
        public static string DataAtual(DateTime data)
        {
            try
            {
                string strDataTrans;
                string strDataParc;
                DateTime dteData = data;
                int intAno = dteData.Year;
                int intMes = dteData.Month;
                int intDia = dteData.Day;
                strDataTrans = Convert.ToString(intAno);
                strDataParc = Convert.ToString(intMes);
                if (strDataParc.Length < 2)
                {
                    strDataParc = "0" + strDataParc;
                };
                strDataTrans += strDataParc;
                strDataParc = Convert.ToString(intDia);
                if (strDataParc.Length < 2)
                {
                    strDataParc = "0" + strDataParc;
                };
                strDataTrans += strDataParc;
                return strDataTrans;

            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.DataAtual]" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Formata a data atual no formato YYYYMM00.
        /// </summary>
        /// <returns>Data Atual.</returns>
        public static int AnoMesAtual(DateTime data)
        {
            try
            {
                string strDataTrans;
                string strDataParc;
                DateTime dteData = data;
                //Subtrai um mes
                //dteData = dteData.AddMonths(-1);
                int intAno = dteData.Year;
                int intMes = dteData.Month;
                strDataTrans = Convert.ToString(intAno);
                strDataParc = Convert.ToString(intMes);
                if (strDataParc.Length < 2)
                {
                    strDataParc = "0" + strDataParc;
                };
                strDataTrans += strDataParc;
                strDataTrans += "00";

                return Convert.ToInt32(strDataTrans);

            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.DataAtual]" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Formata a data atual no formato YYYYMM00.
        /// </summary>
        /// <param name="intMes">Subtrai quantidade de meses</param>
        /// <returns>Data atual menos os meses passados como paramentro</returns>
        public static int AnoMesAtual(int intMesSubtrai, DateTime data)
        {
            try
            {
                string strDataTrans;
                string strDataParc;
                DateTime dteData = data;
                if (intMesSubtrai > 0)
                {
                    dteData = dteData.AddMonths(-1 * intMesSubtrai);
                }
                int intAno = dteData.Year;
                int intMes = dteData.Month;
                strDataTrans = Convert.ToString(intAno);
                strDataParc = Convert.ToString(intMes);
                if (strDataParc.Length < 2)
                {
                    strDataParc = "0" + strDataParc;
                };
                strDataTrans += strDataParc;
                strDataTrans += "00";

                return Convert.ToInt32(strDataTrans);

            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.DataAtual]" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Formata a data do mes passado no formato YYYYMM00.
        /// </summary>
        /// <returns>Data Atual.</returns>
        public static int AnoMesAnterior(DateTime data)
        {
            try
            {
                string strDataTrans;
                string strDataParc;
                DateTime dteData = data;
                //Subtrai um mes
                dteData = dteData.AddMonths(-1);
                int intAno = dteData.Year;
                int intMes = dteData.Month;

                strDataTrans = Convert.ToString(intAno);
                strDataParc = Convert.ToString(intMes);
                if (strDataParc.Length < 2)
                {
                    strDataParc = "0" + strDataParc;
                };
                strDataTrans += strDataParc;
                strDataTrans += "00";

                return Convert.ToInt32(strDataTrans);

            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.DataAtual]" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Formata a data do proximo mes no formato YYYYMM00.
        /// </summary>
        /// <returns>Data Atual.</returns>
        public static int AnoMesFuturo(DateTime data)
        {
            try
            {
                string strDataTrans;
                string strDataParc;
                DateTime dteData = data;
                //Subtrai um mes
                dteData = dteData.AddMonths(+1);
                int intAno = dteData.Year;
                int intMes = dteData.Month;

                strDataTrans = Convert.ToString(intAno);
                strDataParc = Convert.ToString(intMes);
                if (strDataParc.Length < 2)
                {
                    strDataParc = "0" + strDataParc;
                };
                strDataTrans += strDataParc;
                strDataTrans += "00";

                return Convert.ToInt32(strDataTrans);

            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.DataAtual]" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Criptogra ou descriptografa a string.
        /// </summary>
        /// <param name="strCript">Palavra que deseja criptografar ou descriptografar.</param>
        /// <param name="strPassword">Chave usada para a criptografia ou descriptografia.</param>
        /// <param name="tipo">Criptografa ou Descriptografa</param>
        /// <example>
        /// <i>cpUtilities.Gerais gerais = new cpUtilities.Gerais();<br></br>
        ///    lblErro.Text = gerais.Cripto("Senha", "Chave", TipoCriptografia.Criptografa);
        /// </i>
        /// </example>
        /// <returns>Palavra criptografada ou descriptografa.</returns>
        /// <remarks>Marcelo</remarks>
        public static string Cripto(string strCript, string strPassword, TipoCriptografia tipo)
        {
            //DES3 Encriptação
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            //Gera um MD5
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            byte[] pwdhash, buff;
            string strRet;

            //hashmd5 = new MD5CryptoServiceProvider();

            //des = new TripleDESCryptoServiceProvider();
            try
            {
                pwdhash = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strPassword));

                //A chave secreta
                des.Key = pwdhash;

                //Há vários tipos de cifras disponível em DES3 
                des.Mode = CipherMode.ECB; //ECB, CBC, CFB
                strRet = "";
                switch (tipo)
                {
                    case TipoCriptografia.Criptografa:
                        //O strCript deve estar em array de byte para trabalhar com o des3 
                        buff = ASCIIEncoding.ASCII.GetBytes(strCript);
                        //Retorna dados sem caracteres estranhos
                        strRet = Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(buff, 0, buff.Length));
                        break;
                    case TipoCriptografia.Descriptografa:
                        buff = Convert.FromBase64String(strCript);
                        //decrypt DES 3 encrypted byte buffer and return ASCII string
                        strRet = ASCIIEncoding.ASCII.GetString(des.CreateDecryptor().TransformFinalBlock(buff, 0, buff.Length));
                        break;
                }
                //Apagando
                return strRet;
            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.Cripto]" + ex.Message, ex);
            }
            finally
            {
                hashmd5 = null;
                des = null;
            }
        }

        /// <summary>
        /// Criptografia Simples
        /// </summary>
        /// <param name="strEnt">Palavra que deseja criptografar ou descriptografar.</param>
        /// <param name="strChave">Chave usada para a criptografia ou descriptografia.</param>
        /// <param name="tipo">Criptografa, Descriptografa.</param>
        /// <returns>Valor criptografado ou descriptografar.</returns>
        public static string Morpho(string strEnt, string strChave, TipoCriptografia tipo)
        {
            int lngPE;
            int lngCh;
            string strTemp;
            string strRet = "";
            short intStep;
            if (String.IsNullOrEmpty(strEnt))
            {
                return "";
            }

            try
            {
                if (tipo == TipoCriptografia.Criptografa)
                {
                    intStep = 1;
                }
                else
                {
                    intStep = 3;
                }

                lngCh = 0;
                for (lngPE = 0; lngPE <= strEnt.Length - 1; lngPE += intStep)
                {
                    if (lngCh > strChave.Length - 1)
                    {
                        lngCh = 0;
                    }
                    if (tipo == TipoCriptografia.Criptografa)
                    {
                        strTemp = Convert.ToString(Convert.ToInt32(Convert.ToChar(strEnt.Substring(lngPE, 1))) ^ Convert.ToInt32(Convert.ToChar(strChave.Substring(lngCh, 1))));
                    }
                    else
                    {
                        strTemp = Convert.ToString(Convert.ToChar(Convert.ToInt32(strEnt.Substring(lngPE, 3)) ^ Convert.ToInt32(Convert.ToChar(strChave.Substring(lngCh, 1)))));
                    }

                    if (tipo == TipoCriptografia.Criptografa)
                    {
                        switch (strTemp.Length)
                        {
                            case 1:
                                strTemp = "00" + strTemp;
                                break;
                            case 2:
                                strTemp = "0" + strTemp;
                                break;
                        }
                    }
                    strRet = strRet + strTemp;
                    lngCh = lngCh + 1;
                }

            }
            catch (Exception)
            {
                strRet = "";
                //throw new Exception("[Gerais.Morpho]" + ex.Message, ex);
            }

            return strRet;
        }

        /// <summary>
        /// Criptografia 
        /// </summary>
        /// <param name="strEnt">Palavra que deseja criptografar ou descriptografar.</param>
        /// <param name="tipo">Criptografa, Descriptografa.</param>
        /// <returns>Valor criptografado ou descriptografar.</returns>
        public static string Morpho(string strEnt, TipoCriptografia tipo)
        {
            string strChave = "5GvBenOO";
            int lngPE;
            int lngCh;
            string strTemp;
            string strRet = "";
            short intStep;
            if (String.IsNullOrEmpty(strEnt))
            {
                return "";
            }
            try
            {
                if (tipo == TipoCriptografia.Criptografa)
                {
                    intStep = 1;
                }
                else
                {
                    intStep = 3;
                }

                lngCh = 0;
                for (lngPE = 0; lngPE <= strEnt.Length - 1; lngPE += intStep)
                {
                    if (lngCh > strChave.Length - 1)
                    {
                        lngCh = 0;
                    }
                    if (tipo == TipoCriptografia.Criptografa)
                    {
                        strTemp = Convert.ToString(Convert.ToInt32(Convert.ToChar(strEnt.Substring(lngPE, 1))) ^ Convert.ToInt32(Convert.ToChar(strChave.Substring(lngCh, 1))));
                    }
                    else
                    {
                        strTemp = Convert.ToString(Convert.ToChar(Convert.ToInt32(strEnt.Substring(lngPE, 3)) ^ Convert.ToInt32(Convert.ToChar(strChave.Substring(lngCh, 1)))));
                    }

                    if (tipo == TipoCriptografia.Criptografa)
                    {
                        switch (strTemp.Length)
                        {
                            case 1:
                                strTemp = "00" + strTemp;
                                break;
                            case 2:
                                strTemp = "0" + strTemp;
                                break;
                        }
                    }
                    strRet = strRet + strTemp;
                    lngCh = lngCh + 1;
                }

                return strRet;
            }
            catch (Exception Ex)
            {
                return "";
            }
        }

        /// <summary>
        /// Gera uma chave.
        /// </summary>
        /// <returns>Gera uma chave tipo de instalação de Software DFGHWERTSDFGCDFV</returns>
        public static string GerarChave()
        {
            try
            {
                int intBytes = 8;
                RNGCryptoServiceProvider objRNG = new RNGCryptoServiceProvider();
                byte[] bytBuffer = new byte[intBytes];

                objRNG.GetBytes(bytBuffer);

                StringBuilder hexString = new StringBuilder(64);

                for (int counter = 0; counter < bytBuffer.Length; counter++)
                {
                    hexString.Append(String.Format("{0:X2}", bytBuffer[counter]));
                }
                string strRetorno = hexString.ToString();
                return strRetorno;
            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.GerarChave]" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Gera senha forte.
        /// </summary>
        /// <returns>Senha forte.</returns>
        /// <example><code>
        /// <value>C#</value>
        /// cpUtilities.<font color="#2B91AF">Gerais</font> funcoes = <font color="blue">new</font> cpUtilities.<font color="#2B91AF">Gerais</font>();
        /// <font color="blue">try</font>
        /// {
        ///     lblInformacoes.Text = <font color="#a31515">"Gerar senha forte: "</font> + funcoes.GerarSenha();
        /// }
        /// <font color="blue">catch</font> (<font color="#2B91AF">Exception </font>ex)
        /// {
        ///     lblErro.Text = ex.Message;
        /// }
        /// <font color="blue">finally</font>
        /// {
        ///	  funcoes = <font color="blue">null</font>;
        /// }
        /// </code>
        /// <code>
        /// <value>VB</value>
        /// </code>
        ///</example>
        public static string GerarSenha
        {
            get
            {
                string strSenha = "";
                Random rndValor = new Random();
                byte[] bytEspeciais = { 33, 35, 36 };
                try
                {
                    for (int i = 0; i <= 7; i++)
                    {
                        if (i == 0)
                        {
                            strSenha = Convert.ToChar(rndValor.Next(65, 90)).ToString();
                        }

                        if (i == 1)
                        {
                            //strSenha += Convert.ToChar(bytEspeciais[rndValor.Next(0, 9)]).ToString();
                            strSenha += "#";
                        }

                        if ((i >= 2) || (i <= 6) && (i != 0) && (i != 1))
                        {
                            strSenha += Convert.ToChar(rndValor.Next(97, 122)).ToString();
                        }

                        if (i == 7)
                        {
                            strSenha += Convert.ToChar(rndValor.Next(48, 57)).ToString();
                        }
                    }
                    return strSenha;
                }
                catch (Exception ex)
                {
                    throw new Exception("[Gerais.GerarSenha]" + ex.Message, ex);
                }
                finally
                {
                    rndValor = null;
                }
            }
        }

        /// <summary>
        /// Varifica se o email é válido
        /// </summary>
        /// <param name="strIn">String a ser tratada</param>
        /// <returns>Retorna true se o email for válido</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// Marcelo
        /// </history>
        public static bool ValidarEmail(string strIn)
        {
            return Regex.IsMatch(strIn, "\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
        }

        public static string RemoverAcentos(string strEntrada)
        {

            /** Troca os caracteres acentuados por não acentuados **/
            string[] acentos = new string[] { "ç", "Ç", "á", "é", "í", "ó", "ú", "ý", "Á", "É", "Í", "Ó", "Ú", "Ý", "à", "è", "ì", "ò", "ù", "À", "È", "Ì", "Ò", "Ù", "ã", "õ", "ñ", "ä", "ë", "ï", "ö", "ü", "ÿ", "Ä", "Ë", "Ï", "Ö", "Ü", "Ã", "Õ", "Ñ", "â", "ê", "î", "ô", "û", "Â", "Ê", "Î", "Ô", "Û" };
            string[] semAcento = new string[] { "c", "C", "a", "e", "i", "o", "u", "y", "A", "E", "I", "O", "U", "Y", "a", "e", "i", "o", "u", "A", "E", "I", "O", "U", "a", "o", "n", "a", "e", "i", "o", "u", "y", "A", "E", "I", "O", "U", "A", "O", "N", "a", "e", "i", "o", "u", "A", "E", "I", "O", "U" };

            for (int i = 0; i < acentos.Length; i++)
            {
                strEntrada = strEntrada.Replace(acentos[i], semAcento[i]);
            }

            /** Troca os caracteres especiais da string por "" **/
            string[] caracteresEspeciais = { "\\.", ",", "-", ":", "\\(", "\\)", "ª", "\\|", "\\\\", "°", " " };

            for (int i = 0; i < caracteresEspeciais.Length; i++)
            {
                strEntrada = strEntrada.Replace(caracteresEspeciais[i], "");
            }

            /** Troca os espaços no início por "" **/
            strEntrada = strEntrada.Replace("^\\s+", "");
            /** Troca os espaços no início por "" **/
            strEntrada = strEntrada.Replace("\\s+$", "");
            /** Troca os espaços duplicados, tabulações e etc por  "" **/
            strEntrada = strEntrada.Replace("\\s+", "");

            //Caso sobre algo difertente
            string strPattern = @"(?i)[^0-9_a-záéíóúàèìòùâêîôûãõç\s]";
            string strReplacement = "";
            Regex rexNovo = new Regex(strPattern);
            string strResult = rexNovo.Replace(strEntrada, strReplacement);

            //deve ser caixa baixa
            strResult = strResult.ToLower();

            return strResult;
        }

        /// <summary>
        /// Converte Data 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Data no formato dd/mm/yyyy ou yyyymmdd</returns>
        /// <remarks></remarks>
        public static string ConverterData(string strData)
        {
            string strRetorno = "";
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    if (strData.Trim().Length > 0)
                    {
                        if (strData.IndexOf("/") > 0)
                        {
                            strRetorno = strData.Substring(6, 4) + strData.Substring(3, 2) + strData.Substring(0, 2);
                        }
                        else
                        {
                            strRetorno = strData.Substring(6, 2) + "/" + strData.Substring(4, 2) + "/" + strData.Substring(0, 4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("[Gerais.ConverteData]" + ex.Message, ex);
                strRetorno = "";
            }

            return strRetorno;
        }

        /// <summary>
        /// Converte Data 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Data no formato dd/mm/yyyy ou yyyy-mm-dd</returns>
        /// <remarks></remarks>
        public static string ConverterDataBanco(string strData)
        {
            string strRetorno = "";
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    if (strData.Trim().Length > 0)
                    {
                        if (strData.IndexOf("/") > 0)
                        {
                            //17/04/2022 => 2022-04-17
                            strRetorno = strData.Substring(6, 4) + "-" + strData.Substring(3, 2) + "-" + strData.Substring(0, 2);
                        }
                        else if (strData.IndexOf("-") > 0)
                        {
                            //2022-04-17 => 17/04/2022
                            strRetorno = strData.Substring(8, 2) + "/" + strData.Substring(5, 2) + "/" + strData.Substring(0, 4);
                        }
                        else
                        {
                            //20220417 => 17/04/2022
                            strRetorno = strData.Substring(6, 2) + "/" + strData.Substring(4, 2) + "/" + strData.Substring(0, 4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("[Gerais.ConverteData]" + ex.Message, ex);
                strRetorno = "";
            }

            return strRetorno;
        }

        /// <summary>
        /// Converte Data 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Data no formato dd/mm/yyyy ou yyyy-mm-dd</returns>
        /// <remarks></remarks>
        public static string ConverterDataBanco(string strData, string idioma)
        {
            string strRetorno = "";
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    if (strData.Trim().Length > 0)
                    {
                        if (strData.IndexOf("/") > 0)
                        {
                            if (idioma == "en-US")
                            {
                                //04/17/2022 => 2022-04-17
                                strRetorno = strData.Substring(6, 4) + "-" + strData.Substring(0, 2) + "-" + strData.Substring(3, 2);
                            }
                            else
                            {
                                //17/04/2022 => 2022-04-17
                                strRetorno = strData.Substring(6, 4) + "-" + strData.Substring(3, 2) + "-" + strData.Substring(0, 2);
                            }
                        }
                        else if (strData.IndexOf("-") > 0)
                        {
                            if (idioma == "en-US")
                            {
                                //2022-04-17 => 04/17/2022
                                strRetorno = strData.Substring(5, 2) + "/" + strData.Substring(8, 2) + "/" + strData.Substring(0, 4);
                            }
                            else
                            {
                                //2022-04-17 => 17/04/2022
                                strRetorno = strData.Substring(8, 2) + "/" + strData.Substring(5, 2) + "/" + strData.Substring(0, 4);
                            }
                        }
                        else
                        {
                            if (idioma == "en-US")
                            {
                                //20220417 => 04/17/2022
                                strRetorno = strData.Substring(4, 2) + "/" + strData.Substring(6, 2) + "/" + strData.Substring(0, 4);
                            }
                            else
                            {
                                //20220417 => 17/04/2022
                                strRetorno = strData.Substring(6, 2) + "/" + strData.Substring(4, 2) + "/" + strData.Substring(0, 4);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("[Gerais.ConverteData]" + ex.Message, ex);
                strRetorno = "";
            }

            return strRetorno;
        }


        /// <summary>
        /// Converte Data 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Data no formato dd/mm/yyyy ou yyyymmdd</returns>
        /// <remarks></remarks>
        public static string ConverterData(int intData)
        {
            string strData = "";
            string strRetorno = "";
            try
            {
                strData = Convert.ToString(intData);
                if (!string.IsNullOrEmpty(strData))
                {
                    if (strData.IndexOf("/") > 0)
                    {
                        strRetorno = strData.Substring(6, 4) + strData.Substring(3, 2) + strData.Substring(0, 2);
                    }
                    else
                    {
                        strRetorno = strData.Substring(6, 2) + "/" + strData.Substring(4, 2) + "/" + strData.Substring(0, 4);
                    }
                }
                return strRetorno;
            }
            catch (Exception ex)
            {
                //throw new Exception("[Gerais.ConverteData]" + ex.Message, ex);
                return "19000101";
            }
        }

        /// <summary>
        /// Converte Data 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Data no formato dd/mm/yyyy ou yyyymmdd</returns>
        /// <remarks></remarks>
        public static string ConverterData(DateTime? dttDataEntrada)
        {
            //Caso data seja null
            DateTime semData = new DateTime(1900, 1, 1, 0, 0, 0);

            DateTime dttData = dttDataEntrada ?? semData;

            string strData = dttData.ToString("yyyyMMdd");
            string strRetorno = "";
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    if (strData.Trim().Length > 0)
                    {
                        if (strData.IndexOf("/") > 0)
                        {
                            strRetorno = strData.Substring(6, 4) + strData.Substring(3, 2) + strData.Substring(0, 2);
                        }
                        else
                        {
                            strRetorno = strData.Substring(6, 2) + "/" + strData.Substring(4, 2) + "/" + strData.Substring(0, 4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("[Gerais.ConverteData]" + ex.Message, ex);
                strRetorno = "";
            }

            return strRetorno;
        }

        /// <summary>
        /// Gera um evento na pasta SBS
        /// </summary>
        /// <param name="strMSG">Mensagem</param>
        public static void EventView(string strMsg)
        {
            EventLog objEventLog = new EventLog();
            try
            {
                //Gera um evento em uma dada pasta do eventlog
                if (!EventLog.SourceExists(cstrPastaEventLog))
                    EventLog.CreateEventSource(cstrPastaEventLog, "Application");

                //Seta pasta
                objEventLog.Source = cstrPastaEventLog;
                //Escreve na log
                objEventLog.WriteEntry(strMsg, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.EventView.1]" + ex.Message, ex);
            }
            finally
            {
                objEventLog = null;
            }
        }

        /// <summary>
        /// Gera um evento na pasta SBS
        /// </summary>
        /// <param name="strMsg">Mensagem</param>
        /// <param name="shtTipo">Tipo de log: 1 - Information, 2 - Warning, 3 - Error</param>
        public static void EventView(string strMsg, short shtTipo)
        {
            EventLog objEventLog = new EventLog();
            try
            {
                //Gera um evento em uma dada pasta do eventlog
                if (!EventLog.SourceExists(cstrPastaEventLog))
                    EventLog.CreateEventSource(cstrPastaEventLog, "Application");

                //Seta pasta
                objEventLog.Source = cstrPastaEventLog;
                //Escreve na log
                objEventLog.WriteEntry(strMsg, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.EventView.2]" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Gera um evento em uma dada pasta do eventlog
        /// </summary>
        /// <param name="strMsg">Mensagem</param>
        /// <param name="shtTipo">Tipo de log: 1 - Information, 2 - Warning, 3 - Error</param>
        /// <param name="strSistema">Sistema</param>
        /// <remarks>Marcelo</remarks>
        /// <example><code>
        /// <value>C#</value>
        /// cpUtilities.<font color="#2B91AF">Gerais</font> funcoes = <font color="blue">new</font> cpUtilities.<font color="#2B91AF">Gerais</font>();
        /// <font color="blue">try</font>
        /// {
        ///     funcoes.EventView(<font color="#a31515">"Mensagem"</font>, 1, <font color="#a31515">"SBS"</font>);
        /// }
        /// <font color="blue">catch</font> (<font color="#2B91AF">Exception </font>ex)
        /// {
        ///     lblErro.Text = ex.Message;
        /// }
        /// <font color="blue">finally</font>
        /// {
        ///	  funcoes = <font color="blue">null</font>;
        /// }
        /// </code>
        /// <code>
        /// <value>VB</value>
        /// </code>
        /// </example>
        public static void EventView(string strMsg, short shtTipo, string strSistema)
        {
            EventLog objEventLog = new EventLog();
            try
            {
                if (strSistema.Trim() == "") strSistema = cstrPastaEventLog;

                //Gera um evento em uma dada pasta do eventlog
                if (!EventLog.SourceExists(strSistema))
                    EventLog.CreateEventSource(strSistema, "Application");

                //Seta pasta
                objEventLog.Source = strSistema;
                //Escreve na log
                switch (shtTipo)
                {
                    case 1:
                        objEventLog.WriteEntry(strMsg, EventLogEntryType.Information);
                        break;
                    case 2:
                        objEventLog.WriteEntry(strMsg, EventLogEntryType.Warning);
                        break;
                    default:
                        objEventLog.WriteEntry(strMsg, EventLogEntryType.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("[Gerais.EventView.3]" + ex.Message, ex);
            }
        }

        public static string EnviarEmail(string strDestinatario, string strTitulo, string strMensagem, DateTime data)
        {
            bool blnValid = true;
            string strRet = "";
            try
            {
                Email objEmail = new Email();
                //evio com prioridade alta
                objEmail.Prioridade = TipoPrioridade.Alta;
                //formato do email em html
                objEmail.Formato = TipoFormato.Html;
                //email com Layout
                objEmail.Layout = true;
                objEmail.Remetente = "service@XXXXXX.com.br"; //Trocar por email valido
                objEmail.Destinatario = strDestinatario;
                objEmail.Titulo = strTitulo;
                objEmail.Origem = "Consórcio Realiza";

                objEmail.Responsaveis = "<font size = 2>" + " Jessyca Vieira <br/>" + " <a style='COLOR: #900; TEXT-DECORATION: none' href='mailto:XXXXXX1@XXXXXX.com.br'>jessyca.vieira@consorciorealiza.com.br</a><br/><br/>" +
                                        "<font size = 2>" + " Sirlene Preto <br/>" + " <a style='COLOR: #900; TEXT-DECORATION: none' href='mailto:XXXXXX2@XXXXXX.com.br'>sirlene.preto@consorciorealiza.com.br</a><br/><br/>";
                objEmail.Mensagem = strMensagem;

                blnValid = objEmail.Enviar(data);

                if (blnValid)
                {
                    strRet = "ok";
                }
                else
                {
                    strRet = "Não foi possível enviar o email. Tente novamente.";
                }
            }

            catch (Exception ex)
            {
                strRet = "Erro no envio de email: " + ex.Message;
            }
            return strRet;
        }

        public static string EnviarEmail(string strDestinatario, string strCopia, string strTitulo, string strMensagem, DateTime data)
        {
            bool blnValid = true;
            string strRet = "";
            try
            {
                Email objEmail = new Email();
                //evio com prioridade alta
                objEmail.Prioridade = TipoPrioridade.Alta;
                //formato do email em html
                objEmail.Formato = TipoFormato.Html;
                //email com Layout
                objEmail.Layout = true;
                objEmail.Remetente = "service@XXXXXX.com.br";
                objEmail.Destinatario = strDestinatario;
                objEmail.DestinatarioCopias = strCopia;
                objEmail.Titulo = strTitulo;
                objEmail.Origem = "Nome da Empresa"; //Todo trocar por nome valido

                objEmail.Responsaveis = "<font size = 2>" + " Marketing <br/>" + " <a style='COLOR: #900; TEXT-DECORATION: none' href='mailto:XXXXXX3@XXXXXX.com.br'>XXXXXX3@XXXXXX.com.br</a><br/><br/>";
                objEmail.Mensagem = strMensagem;

                blnValid = objEmail.Enviar(data);

                if (blnValid)
                {
                    strRet = "ok";
                }
                else
                {
                    strRet = "Não foi possível enviar o email. Tente novamente.";
                }
            }

            catch (Exception ex)
            {
                strRet = "Erro no envio de email: " + ex.Message;
            }
            return strRet;
        }

        /// <summary>
        /// Retorna data atual no formato dd-MM-yyyy
        /// </summary>
        /// <returns></returns>
        public static string DataAtual2(DateTime data)
        {
            return data.ToString("dd-MM-yyyy");
        }

        /// <summary>
        /// Retorna data atual no formato yyyyMMdd
        /// </summary>
        /// <returns></returns>
        public static int DataAtualInt(DateTime data)
        {
            return Convert.ToInt32(data.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// Retorna data atual no formato datetime
        /// </summary>
        /// <returns></returns>
        public static DateTime DataAtualDT(DateTime data)
        {
            return data;
        }

        #endregion

    } //class Gerais

    #endregion

    #region Correio
    public class Email
    {

        #region Declaracoes

        int cintPosicao;
        int cintPosicaoInicial;
        bool cblnPadrao = true;
        int cintFormato = 1;

        #endregion

        #region Propriedades

        ///<summary>
        ///Obtem ou retorna o destinatario para envio de email
        ///</summary>
        public string Destinatario { get; set; }

        ///<summary>
        ///Obtem ou retorna o titulo para envio de email
        ///</summary>
        public string Titulo { get; set; }

        ///<summary>
        ///Obtem ou retorna a Mensagem para envio de email
        ///</summary>
        public string Mensagem { get; set; }

        ///<summary>
        ///Obtem ou retorna se o email esta no padrao 
        ///</summary>
        public bool Layout
        {
            get
            {
                return cblnPadrao;
            }
            set
            {
                cblnPadrao = value;
            }
        }

        ///<summary>
        ///Obtem ou retorna a URL da imagem do sistema - 415x88 px
        ///</summary>
        public string URLImagemSistema { get; set; }

        ///<summary>
        ///Obtem ou retorna a URL do Logo - 415x88 px
        ///</summary>
        public string URLImagemLogo { get; set; }

        ///<summary>
        ///Obtem ou retorna o Remetente para envio de email
        ///</summary>
        public string Remetente { get; set; }

        ///<summary>
        ///Obtem ou retorna o Responsaveis pelo sistema - deve estar em formato html
        ///</summary>
        public string Responsaveis { get; set; }

        ///<summary>
        ///Obtem ou retorna a Origem, tipo SIS, GQ, CC Informa, etc responsável pelo envio do email
        ///</summary>
        public string Origem { get; set; }

        ///<summary>
        ///Obtem ou retorna os Anexos para envio de email
        ///</summary>
        public string Anexos { get; set; }

        ///<summary>
        ///Obtem ou retorna o Destinatario de Copias para envio de email
        ///</summary>
        public string DestinatarioCopias { get; set; }

        ///<summary>
        ///Obtem ou retorna os Destinatario de Copias Ocultas para envio de email
        ///</summary>
        public string DestinatarioCopiasOcultas { get; set; }

        ///<summary>
        ///Obtem ou retorna o Formato de envio de email: Text = 0, Html = 1
        ///</summary>
        public TipoFormato Formato { get; set; }

        ///<summary>
        ///Obtem ou retorna a prioridade de envio de email: Normal = 0, Low = 1, High = 2
        ///</summary>
        public TipoPrioridade Prioridade { get; set; }

        #endregion

        #region Metodos

        /// <summary>
        /// Envia o eMail
        /// </summary>
        public bool Enviar(DateTime data)
        {
            bool blnValida = true;
            Attachment objAtt;

            try
            {
                if (Remetente == "")
                {
                    throw new ArgumentNullException("Não foi informado o Remetente.}");
                }
                if (Destinatario == "")
                {
                    throw new ArgumentNullException("Não foi informado o Destinatário.}");
                }

                string strHost = WebConfigurationManager.AppSettings["SMTP"];
                string strPort = WebConfigurationManager.AppSettings["SMTPPorta"];
                string strUser = WebConfigurationManager.AppSettings["SMTPUsuario"];
                string strPassword = WebConfigurationManager.AppSettings["SMTPSenha"];
                string strSSL = WebConfigurationManager.AppSettings["SMTPSSL"];

                if (blnValida == true)
                {
                    //Normal = 0, Low = 1, High =2
                    MailMessage objCorreio = new MailMessage();
                    objCorreio.From = new MailAddress(Remetente);
                    objCorreio.To.Add(Destinatario);

                    objCorreio.Subject = Titulo;
                    objCorreio.Priority = (MailPriority)Convert.ToInt32(Prioridade);
                    objCorreio.Headers.Add("Disposition-Notification-To", Remetente);

                    cintFormato = Convert.ToInt32(Formato);
                    if (cintFormato == 0)
                    {
                        objCorreio.IsBodyHtml = false;
                    }
                    else
                    {
                        objCorreio.IsBodyHtml = true;
                    }

                    if (cblnPadrao)
                    {
                        string strAmbiente = Dados.Ambiente();
                        if (strAmbiente != "Produção")
                        {
                            if (cblnPadrao)
                            {
                                Mensagem = "<font size=3 color='red'><b>Favor desconsiderar este email.<br /> Enviado do ambiente de " + strAmbiente + "</b></font><br/><br/>" + Mensagem;
                            }
                            else
                            {
                                Mensagem = "Favor desconsiderar este email." + Convert.ToChar(10) + "Enviado do ambiente de " + strAmbiente + Convert.ToChar(10) + Convert.ToChar(10) + Mensagem;
                            }
                        }
                        // Verificar as imagens (Pedro)
                        Anexos = System.Web.HttpContext.Current.Server.MapPath("~/Arquivos/email/logo.gif") + "," + System.Web.HttpContext.Current.Server.MapPath("~/Arquivos/email/emailBanner.gif") + ",";
                        URLImagemLogo = "cid:logo.gif";
                        URLImagemSistema = "cid:emailBanner.gif";
                        Mensagem = PadraoEmail(data);
                    }

                    objCorreio.Body = Mensagem;

                    if (!String.IsNullOrEmpty(DestinatarioCopias))
                    {
                        objCorreio.CC.Add(DestinatarioCopias);
                    }
                    if (!String.IsNullOrEmpty(DestinatarioCopiasOcultas))
                    {
                        objCorreio.Bcc.Add(DestinatarioCopiasOcultas);
                    }

                    if (!String.IsNullOrEmpty(Anexos))
                    {
                        cintPosicao = Anexos.IndexOf(",", 1);
                        if (cintPosicao == -1)
                        {
                            objAtt = new Attachment(Anexos.ToString());
                            objCorreio.Attachments.Add(objAtt);
                        }
                        else
                        {
                            cintPosicaoInicial = 0;
                            cintPosicao = Anexos.IndexOf(",", 1);
                            do
                            {
                                objAtt = new Attachment(Anexos.Substring(cintPosicaoInicial, cintPosicao - cintPosicaoInicial).ToString());
                                objCorreio.Attachments.Add(objAtt);
                                cintPosicaoInicial = cintPosicao + 1;
                                cintPosicao = Anexos.IndexOf(",", cintPosicaoInicial);

                            }
                            while (cintPosicao > 0);
                        }
                    }

                    Encryption objEncryption = new Encryption();
                    var section = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                    SmtpClient SmtpMail = new SmtpClient();
                    SmtpMail.Host = strHost;
                    SmtpMail.Port = Convert.ToInt32(strPort);
                    if (strSSL == "True")
                    {
                        SmtpMail.EnableSsl = true;
                    }

                    strPassword = objEncryption.Morpho("o1Pdpdpep2#22!", "Ava2014", TipoCriptografia.Criptografa);
                    NetworkCredential cred = new NetworkCredential(strUser, objEncryption.Morpho(strPassword, "Ava2014", TipoCriptografia.Descriptografa));

                    // inclui as credenciais
                    SmtpMail.Credentials = cred;
                    SmtpMail.Send(objCorreio);

                    objAtt = null;
                    objCorreio.Dispose();
                    objCorreio = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public bool Enviar(string strHost, string strPort, string strUser, string strPassword, bool blnSSL, DateTime data)
        {
            bool blnValida = true;
            Attachment objAtt;

            try
            {
                if (Remetente == "")
                {
                    throw new ArgumentNullException("Não foi informado o Remetente.}");
                }
                if (Destinatario == "")
                {
                    throw new ArgumentNullException("Não foi informado o Destinatário.}");
                }

                if (blnValida == true)
                {
                    //Normal = 0, Low = 1, High =2
                    MailMessage objCorreio = new MailMessage();
                    objCorreio.From = new MailAddress(Remetente);
                    objCorreio.To.Add(Destinatario);

                    objCorreio.Subject = Titulo;
                    objCorreio.Priority = (MailPriority)Convert.ToInt32(Prioridade);
                    objCorreio.Headers.Add("Disposition-Notification-To", Remetente);

                    cintFormato = Convert.ToInt32(Formato);
                    if (cintFormato == 0)
                        objCorreio.IsBodyHtml = false;
                    else
                        objCorreio.IsBodyHtml = true;

                    string strAmbiente = Dados.Ambiente();

                    if (strAmbiente != "Produção")
                    {
                        if (cblnPadrao)
                        {
                            Mensagem = "<font size=3 color='red'><b>Favor desconsiderar este email.<br /> Enviado do ambiente de " +
                               strAmbiente + "</b></font><br/><br/>" + Mensagem;
                        }
                        else
                        {
                            Mensagem = "Favor desconsiderar este email." + Convert.ToChar(10) + "Enviado do ambiente de " +
                               strAmbiente + Convert.ToChar(10) + Convert.ToChar(10) + Mensagem;
                        }
                    }

                    // Verificar as imagens (Pedro)
                    Anexos = System.Web.HttpContext.Current.Server.MapPath("~/Arquivos/email/logo.gif") + "," + System.Web.HttpContext.Current.Server.MapPath("~/Arquivos/email/emailBanner.gif") + ",";
                    URLImagemLogo = "cid:logo.gif";
                    URLImagemSistema = "cid:emailBanner.gif";

                    if (cblnPadrao)
                        Mensagem = PadraoEmail(data);

                    objCorreio.Body = Mensagem;

                    if (!String.IsNullOrEmpty(DestinatarioCopias))
                        objCorreio.CC.Add(DestinatarioCopias);

                    if (!String.IsNullOrEmpty(DestinatarioCopiasOcultas))
                        objCorreio.Bcc.Add(DestinatarioCopiasOcultas);

                    if (Anexos != "")
                    {
                        cintPosicao = Anexos.IndexOf(",", 1);
                        if (cintPosicao == -1)
                        {
                            objAtt = new Attachment(Anexos.ToString());
                            objCorreio.Attachments.Add(objAtt);
                        }
                        else
                        {
                            cintPosicaoInicial = 0;
                            cintPosicao = Anexos.IndexOf(",", 1);
                            do
                            {
                                objAtt = new Attachment(Anexos.Substring(cintPosicaoInicial, cintPosicao - cintPosicaoInicial).ToString());
                                objCorreio.Attachments.Add(objAtt);
                                cintPosicaoInicial = cintPosicao + 1;
                                cintPosicao = Anexos.IndexOf(",", cintPosicaoInicial);

                            }
                            while (cintPosicao > 0);
                        }
                    }
                    Encryption objEncryption = new Encryption();
                    var section = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                    SmtpClient SmtpMail = new SmtpClient();
                    SmtpMail.Host = strHost;
                    SmtpMail.Port = Convert.ToInt32(strPort);
                    if (blnSSL)
                    {
                        SmtpMail.EnableSsl = true;
                    }
                    NetworkCredential cred = new NetworkCredential(strUser, strPassword);

                    // inclui as credenciais
                    SmtpMail.Credentials = cred;
                    SmtpMail.Send(objCorreio);

                    objAtt = null;
                    objCorreio.Dispose();
                    objCorreio = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Formata Layout do email para o padrão
        /// </summary>
        /// <returns></returns>
        private string PadraoEmail(DateTime data)
        {
            StringBuilder strMeg = new StringBuilder();

            DateTime dtAtual = new DateTime();
            dtAtual = data;

            //Monta HTML do e-mail a ser enviado 
            strMeg.Append(@"<!DOCTYPE HTML PUBLIC " + Convert.ToChar(34) + "-//W3C//DTD HTML 4.0 Transitional//EN" + Convert.ToChar(34) + ">" + Convert.ToChar(10) + Convert.ToChar(10));
            strMeg.Append(@"<html>" + Convert.ToChar(10));
            //Header
            strMeg.Append(@"   <head>" + Convert.ToChar(10));
            strMeg.Append(@"      <title>" + Titulo + "</title>" + Convert.ToChar(10));
            strMeg.Append(@"      <meta http-equiv='Content-Type' content='text/html; charset=iso-8859-1'>" + Convert.ToChar(10));
            strMeg.Append(@"   </head>" + Convert.ToChar(10));
            //Inicio do email
            strMeg.Append(@"   <body style='MARGIN: 0px; COLOR: #000; FONT-FAMILY: Arial, Helvetica, sans-serif; BACKGROUND-COLOR: #fff'>" + Convert.ToChar(10));
            strMeg.Append(@"      <div style='DISPLAY: none; WIDTH: 700px; LINE-HEIGHT: 1px; HEIGHT: 1px'></div>" + Convert.ToChar(10));
            //Cabeçalho - Logo 
            strMeg.Append(@"      <table style='WIDTH: 772px; BACKGROUND-COLOR: #ddd' cellSpacing='0' cellPadding='0'>" + Convert.ToChar(10));
            strMeg.Append(@"         <tbody>" + Convert.ToChar(10));
            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td style='WIDTH: 44px; BORDER-BOTTOM: #fff 1px solid'>&nbsp;</td>" + Convert.ToChar(10));

            strMeg.Append(@"               <td style='VERTICAL-ALIGN: top; WIDTH: 306px; BORDER-BOTTOM: #fff 1px solid'><IMG height='60' alt='Logo: ' src='" + URLImagemLogo + "' width='144'></td>" + Convert.ToChar(10));

            strMeg.Append(@"               <td style='WIDTH: 2px; BORDER-BOTTOM: #fff 1px solid; BACKGROUND-COLOR: #fff'></td>" + Convert.ToChar(10));
            // ******        Imagem do sistema
            strMeg.Append(@"               <td style='WIDTH: 420px; BORDER-BOTTOM: #fff 1px solid'><IMG height='90' alt='Keyvisual' src='" + URLImagemSistema + "' width='420'></td>" + Convert.ToChar(10));
            strMeg.Append(@"              </td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));
            strMeg.Append(@"         </tbody>" + Convert.ToChar(10));
            strMeg.Append(@"      </table>" + Convert.ToChar(10));
            //Espacamento
            strMeg.Append(@"      <table style='WIDTH: 772px; BACKGROUND-COLOR: #a0b6c0' cellSpacing='0' cellPadding='0'>" + Convert.ToChar(10));
            strMeg.Append(@"         <tbody>" + Convert.ToChar(10));
            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td width='44'>&nbsp;</td>" + Convert.ToChar(10));
            //Origem
            strMeg.Append(@"               <td style='BORDER-RIGHT: #cad6da 2px solid; PADDING-RIGHT: 0px; PADDING-LEFT: 0px; FONT-SIZE: 11px; PADDING-BOTTOM: 5px; WIDTH: 728px; LINE-HEIGHT: 16px; PADDING-TOP: 3px; FONT-FAMILY: Arial, Helvetica, sans-serif'><b>" + Origem + "</b></td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));
            strMeg.Append(@"         </tbody>" + Convert.ToChar(10));
            strMeg.Append(@"      </table>" + Convert.ToChar(10));
            //Conteudo do email
            strMeg.Append(@"      <table style='WIDTH: 772px; BACKGROUND-COLOR: #fff' cellSpacing='0' cellPadding='0'>" + Convert.ToChar(10));
            //Corpo
            strMeg.Append(@"         <tbody>" + Convert.ToChar(10));
            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td width='44'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td width='450'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td width='26'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td width='232'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td width='20'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));
            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));
            //                          Titulo 3ª Linha
            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td style='PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 16px; PADDING-TOP: 2px; BORDER-BOTTOM: #cad6da 2px solid' >" + Convert.ToChar(10));
            strMeg.Append(@"                  <H1 style='FONT-WEIGHT: normal; FONT-SIZE: 23px; MARGIN: 0px; LINE-HEIGHT: 28px; FONT-FAMILY: Arial, Helvetica, sans-serif'>" + Convert.ToChar(10));
            // ******        TITULO do eMail
            strMeg.Append(@"                     " + Titulo + Convert.ToChar(10));
            strMeg.Append(@"                  </H1>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td style='BORDER-BOTTOM: #cad6da 2px solid'><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            // Data
            strMeg.Append(@"               <td style='FONT-WEIGHT: normal; FONT-SIZE: 11px; LINE-HEIGHT: 16px; BORDER-BOTTOM: #cad6da 2px solid; FONT-FAMILY: Arial, Helvetica, sans-serif; TEXT-ALIGN: right' >" + "<apan style='FONT-SIZE: 8.5pt; FONT-FAMILY: Arial'>" + Convert.ToDateTime(data).ToString("dd/MM/yyyy") + "</td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));
            //Espacamento 4ª Linha
            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td colspan='5' height='16'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));
            //Corpo 5ª Linha
            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/></td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td valign = top>" + Convert.ToChar(10));
            // ******        1ª Mensagem do eMail
            strMeg.Append(@"                  <span style='FONT-SIZE: 10pt; FONT-FAMILY: Arial'>" + Mensagem + "<br/></span>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/></td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td>" + Convert.ToChar(10));
            strMeg.Append(@"                  <table cellSpacing='0' cellPadding='0'>" + Convert.ToChar(10));
            strMeg.Append(@"                     <tbody>" + Convert.ToChar(10));
            strMeg.Append(@"                        <tr>" + Convert.ToChar(10));
            strMeg.Append(@"                           <td width='232'></td>" + Convert.ToChar(10));
            strMeg.Append(@"                        </tr>" + Convert.ToChar(10));
            strMeg.Append(@"                        <tr>" + Convert.ToChar(10));
            // ******        Responsaveis pelo sistema
            strMeg.Append(@"                           <td style='PADDING-RIGHT: 0px; BORDER-TOP: #d0d3da 1px solid; PADDING-LEFT: 0px; FONT-SIZE: 11px; PADDING-BOTTOM: 6px; LINE-HEIGHT: 16px; PADDING-TOP: 6px; BORDER-BOTTOM: #d0d3da 1px solid; FONT-FAMILY: Arial, Helvetica, sans-serif' >");
            strMeg.Append(@"                              " + Responsaveis);
            strMeg.Append(@"                           </td>" + Convert.ToChar(10));
            strMeg.Append(@"                        </tr>" + Convert.ToChar(10));
            strMeg.Append(@"                     </tbody>" + Convert.ToChar(10));
            strMeg.Append(@"                  </table>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/></td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));
            //                          6ª Linha
            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td colspan='5' height='15'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));

            strMeg.Append(@"            <tr>" + Convert.ToChar(10));
            strMeg.Append(@"               <td colspan='2'>" + Convert.ToChar(10));
            strMeg.Append(@"                  <table style='WIDTH: 496px; HEIGHT: 542px' cellSpacing='0' cellPadding='0'>" + Convert.ToChar(10));
            strMeg.Append(@"                     <tbody>" + Convert.ToChar(10));
            strMeg.Append(@"                        <tr>" + Convert.ToChar(10));
            strMeg.Append(@"                           <td width='44'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"                           <td width='450'>&nbsp;</td>" + Convert.ToChar(10));
            strMeg.Append(@"                        </tr>" + Convert.ToChar(10));
            strMeg.Append(@"                     </tbody>" + Convert.ToChar(10));
            strMeg.Append(@"                  </table>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td></td>" + Convert.ToChar(10));
            strMeg.Append(@"               <td><br/>" + Convert.ToChar(10));
            strMeg.Append(@"               </td>" + Convert.ToChar(10));
            strMeg.Append(@"            </tr>" + Convert.ToChar(10));
            //                       Fim do Corpo
            strMeg.Append(@"         </tbody>" + Convert.ToChar(10));
            strMeg.Append(@"      </table>" + Convert.ToChar(10));
            //Finalizando
            strMeg.Append(@"   </BODY>" + Convert.ToChar(10));
            strMeg.Append(@"</HTML>" + Convert.ToChar(10));

            return strMeg.ToString();

        }

        #endregion

    }
    #endregion

    #region Valores
    public class Dados
    {
        /// <summary>
        /// Obtem a Letra inicial do ambiente dev, homol ou produção
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[Marcelo]
        /// </history>
        public static string Ambiente()
        {
            try
            {
                string strAmbiente = AppSettingsValue("Ambiente");

                if (!string.IsNullOrEmpty(strAmbiente))
                {
                    return (strAmbiente);
                }
                else
                {
                    return (string.Empty);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Obtem um valor do arquivo de configuração
        /// a partir da chave passada como parâmetro.
        /// </summary>
        /// <param name="keyValue">nome da chave a ser buscada</param>
        /// <returns>valor da chave ou null</returns>
        /// <history>
        /// 	[Pedro]
        /// </history>
        public static string AppSettingsValue(string keyValue)
        {
            try
            {
                string strValue = ConfigurationManager.AppSettings[keyValue];

                return strValue;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Converte Data 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Data no formato dd/mm/yyyy ou yyyymmdd</returns>
        /// <remarks></remarks>
        public static string DataValue(string strData)
        {
            string strRetorno = "";
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    if (strData.IndexOf("/") > 0)
                    {
                        strRetorno = strData.Substring(6, 4) + strData.Substring(3, 2) + strData.Substring(0, 2);
                    }
                    else
                    {
                        strRetorno = strData.Substring(6, 2) + "/" + strData.Substring(4, 2) + "/" + strData.Substring(0, 4);
                    }
                }
                return strRetorno;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Retorna a hora e minuto 
        /// em um formato especifico. 
        /// </summary>
        /// <param name="Hora"></param>
        /// <returns>Hora no formato HH:MM ou HHMM</returns>
        /// <remarks></remarks>
        public static string HoraMinutoValue(string strHora)
        {
            string strRetorno = "";
            try
            {
                if (!string.IsNullOrEmpty(strHora))
                {
                    if (strHora.Length == 2)
                    {
                        strRetorno = strHora + ":00";
                    }
                    else
                    {
                        if (strHora.IndexOf(":") > 0)
                        {
                            strRetorno = strHora.Substring(0, 2) + strHora.Substring(3, 2);
                        }
                        else
                        {
                            strRetorno = strHora.Substring(0, 2) + ":" + strHora.Substring(2, 2);
                        }
                    }
                }
                return strRetorno;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Retorna a hora a partir de uma string 
        /// no formato hh:mm ou hhmm. 
        /// </summary>
        /// <param name="Hora"></param>
        /// <returns>Hora no formato HH</returns>
        /// <remarks></remarks>
        public static string HoraValue(string strHora)
        {
            try
            {
                if (!string.IsNullOrEmpty(strHora))
                {
                    return (strHora.Substring(0, 2));
                }
                else
                    return (null);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Retorna a data e hora formatada a partir
        /// de uma string no formato yyyymmdd hhmm
        /// </summary>
        /// <param name="dataHora"></param>
        /// <returns></returns>
        public static string DataHoraValue(string dataHora)
        {
            try
            {
                string[] valores = dataHora.Split(' ');

                if (valores.Length == 2)
                    return (DataValue(valores[0]) + " " + HoraMinutoValue(valores[1]));
                else
                    return (string.Empty);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Obtem a Letra inicial do ambiente dev, homol ou produção
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[Marcelo]
        /// </history>
        public static string Chave()
        {
            try
            {
                string strAmbiente = AppSettingsValue("Chave");

                if (!string.IsNullOrEmpty(strAmbiente))
                {
                    return (strAmbiente);
                }
                else
                {
                    return (string.Empty);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
    #endregion

    #region Encryption
    public class Encryption
    {

        #region "MD5Hasher"

        public static class Md5Encrypt
        {
            public static string Md5EncryptPassword(string data)
            {
                var encoding = new ASCIIEncoding();
                var bytes = encoding.GetBytes(data);
                var hashed = MD5.Create().ComputeHash(bytes);
                return Encoding.UTF8.GetString(hashed);
            }
        }

        public static string GetSaltValue()
        {
            string saltValue = ConfigurationManager.AppSettings["SaltValue"];
            return saltValue;
        }

        public byte[] EncryptData(string dataString)
        {

            // NKT: custom method using functionality from this article
            // http://www.4guysfromrolla.com/articles/103002-1.2.aspx
            // salting has value
            //http://www.4guysfromrolla.com/articles/112002-1.aspx
            // this isn't as secure as a unique salt per user, but if you use a unique salt per site, at least they won't know that salt value if they steal the database and not the web.config file
            // store the saltvalue in the web.config file. make unique per website.
            string saltedString = dataString + GetSaltValue();

            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes = null;
            UTF8Encoding encoder = new UTF8Encoding();
            hashedDataBytes = md5Hasher.ComputeHash(encoder.GetBytes(saltedString));

            return hashedDataBytes;
        }

        #endregion

        #region "RSAEncryption"

        public string RSAEncryptDecrypt(string dataString)
        {

            try
            {
                //Create a UnicodeEncoder to convert between byte array and string.
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] dataToEncrypt = ByteConverter.GetBytes(dataString);
                byte[] encryptedData = null;
                byte[] decryptedData = null;

                //Create a new instance of RSACryptoServiceProvider to generate
                //public and private key data.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Pass the data to ENCRYPT, the public key information 
                    //(using RSACryptoServiceProvider.ExportParameters(false),
                    //and a boolean flag specifying no OAEP padding.
                    encryptedData = RSAEncryptByte(dataToEncrypt, RSA.ExportParameters(false), false);
                    decryptedData = RSADecryptByte(encryptedData, RSA.ExportParameters(true), false);

                    return ByteConverter.GetString(encryptedData) + " / " + ByteConverter.GetString(decryptedData);

                }
            }
            catch (CryptographicException e)
            {
                string errorMessage = e.ToString();
                return string.Empty;
            }
        }

        public byte[] RSAEncryptByte(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData = null;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                string errorMessage = e.ToString();
                return null;
            }
        }

        public byte[] RSADecryptByte(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData = null;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);

                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                string errorMessage = e.ToString();
                return null;
            }
        }

        #endregion

        #region Morpho

        /// <summary>
        /// Criptografia Simples
        /// </summary>
        /// <param name="strEnt">Palavra que deseja criptografar ou descriptografar.</param>
        /// <param name="strChave">Chave usada para a criptografia ou descriptografia.</param>
        /// <param name="tipo">Criptografa, Descriptografa.</param>
        /// <returns>Valor criptografado ou descriptografar.</returns>
        public string Morpho(string strEnt, string strChave, TipoCriptografia tipo)
        {
            int lngPE;
            int lngCh;
            string strTemp;
            string strRet = "";
            short intStep;

            try
            {
                if (tipo == TipoCriptografia.Criptografa)
                {
                    intStep = 1;
                }
                else
                {
                    intStep = 3;
                }

                lngCh = 0;
                for (lngPE = 0; lngPE <= strEnt.Length - 1; lngPE += intStep)
                {
                    if (lngCh > strChave.Length - 1)
                    {
                        lngCh = 0;
                    }
                    if (tipo == TipoCriptografia.Criptografa)
                    {
                        strTemp = Convert.ToString(Convert.ToInt32(Convert.ToChar(strEnt.Substring(lngPE, 1))) ^ Convert.ToInt32(Convert.ToChar(strChave.Substring(lngCh, 1))));
                    }
                    else
                    {
                        strTemp = Convert.ToString(Convert.ToChar(Convert.ToInt32(strEnt.Substring(lngPE, 3)) ^ Convert.ToInt32(Convert.ToChar(strChave.Substring(lngCh, 1)))));
                    }

                    if (tipo == TipoCriptografia.Criptografa)
                    {
                        switch (strTemp.Length)
                        {
                            case 1:
                                strTemp = "00" + strTemp;
                                break;
                            case 2:
                                strTemp = "0" + strTemp;
                                break;
                        }
                    }
                    strRet = strRet + strTemp;
                    lngCh = lngCh + 1;
                }

                return strRet;
            }
            catch (Exception Ex)
            {
                throw (Ex);
                //return null;
            }
        }

        #endregion

        #region Token

        public static string CriarHash(string valor)
        {
            string key = "enj7#A8d!Hu";

            var keyByte = Encoding.ASCII.GetBytes(key);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                var bytes = Encoding.ASCII.GetBytes(valor);
                var hash = hmacsha256.ComputeHash(bytes);

                string sbinary = "";
                for (int i = 0; i < hash.Length; i++)
                    sbinary += hash[i].ToString("X2");
                return sbinary.ToUpper();
            }
        }

        public string GetHashSha256(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;

            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        public static string CreateToken(string message)
        {
            string secret = "enj7#A8d!Hu";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }


        #endregion
    }

    public class Criptografia
    {

        private const string _KEY = "4Fun@DM1N";

        public static string Criptografar(string text)
        {
            try
            {
                byte[] Result;
                UTF8Encoding UTF8 = new UTF8Encoding();
                MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
                byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(_KEY));
                TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
                TDESAlgorithm.Key = TDESKey;
                TDESAlgorithm.Mode = CipherMode.ECB;
                TDESAlgorithm.Padding = PaddingMode.ISO10126;
                byte[] DataToEncrypt = UTF8.GetBytes(text);
                try
                {
                    ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                    Result = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
                }
                finally
                {
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }
                return Convert.ToBase64String(Result);
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteFile("ERROR Criptografar (" + text + ") : " + ex.Message, "CriptografiaHelper");
                return text;
            }
        }

        public static string CriptografarMD5(string input)
        {
            MD5 md5Hash = MD5.Create();

            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string Descriptografar(string text)
        {
            try
            {
                byte[] Result;
                UTF8Encoding UTF8 = new UTF8Encoding();
                MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
                byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(_KEY));
                TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
                TDESAlgorithm.Key = TDESKey;
                TDESAlgorithm.Mode = CipherMode.ECB;
                TDESAlgorithm.Padding = PaddingMode.ISO10126;
                byte[] DataToDecrypt = Convert.FromBase64String(text);
                try
                {
                    ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                    Result = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
                }
                catch (Exception ex)
                {
                    LoggerHelper.WriteFile("ERROR DEScriptografar1 (" + text + ") : " + ex.Message, "CriptografiaHelper");
                    return text;
                }
                finally
                {
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }
                return UTF8.GetString(Result);
            }
            catch (Exception e)
            {
                LoggerHelper.WriteFile("ERROR DEScriptografar2 (" + text + ") : " + e.Message, "CriptografiaHelper");
                return text;
            }
        }

        public static string DescriptografarRSA(string sMensagem, string sPrivateKey)
        {
            // Carregar chave
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            //Carrega Chave Privada
            RSA.FromXmlString(sPrivateKey);

            //Descriptografa a mensagem
            byte[] Byte_DecryptedMensagem = RSA.Decrypt(Convert.FromBase64String(sMensagem), false);
            string String_DecryptedMensagem = System.Text.Encoding.Unicode.GetString(Byte_DecryptedMensagem);

            return String_DecryptedMensagem;
        }

    }

    #endregion

    #region Validações
    public class Validacoes
    {
        //Método que valida o CPF
        public static bool ValidaCPF(string vrCPF)
        {
            try
            {
                string valor = vrCPF.Replace(".", "");
                valor = valor.Replace("-", "");

                if (valor.Length != 11)
                    return false;

                bool igual = true;
                for (int i = 1; i < 11 && igual; i++)
                    if (valor[i] != valor[0])
                        igual = false;

                if (igual || valor == "12345678909")
                    return false;

                int[] numeros = new int[11];
                for (int i = 0; i < 11; i++)
                    numeros[i] = int.Parse(
                    valor[i].ToString());

                int soma = 0;
                for (int i = 0; i < 9; i++)
                    soma += (10 - i) * numeros[i];

                int resultado = soma % 11;
                if (resultado == 1 || resultado == 0)
                {
                    if (numeros[9] != 0)
                        return false;
                }
                else if (numeros[9] != 11 - resultado)
                    return false;

                soma = 0;
                for (int i = 0; i < 10; i++)
                    soma += (11 - i) * numeros[i];

                resultado = soma % 11;

                if (resultado == 1 || resultado == 0)
                {
                    if (numeros[10] != 0)
                        return false;

                }
                else
                    if (numeros[10] != 11 - resultado)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Método que valida o CNPJ 
        public static bool ValidaCNPJ(string vrCNPJ)
        {

            string CNPJ = vrCNPJ.Replace(".", "");
            CNPJ = CNPJ.Replace("/", "");
            CNPJ = CNPJ.Replace("-", "");

            int[] digitos, soma, resultado;
            int nrDig;
            string ftmt;
            bool[] CNPJOk;

            ftmt = "6543298765432";
            digitos = new int[14];
            soma = new int[2];
            soma[0] = 0;
            soma[1] = 0;
            resultado = new int[2];
            resultado[0] = 0;
            resultado[1] = 0;
            CNPJOk = new bool[2];
            CNPJOk[0] = false;
            CNPJOk[1] = false;

            try
            {
                for (nrDig = 0; nrDig < 14; nrDig++)
                {
                    digitos[nrDig] = int.Parse(
                     CNPJ.Substring(nrDig, 1));
                    if (nrDig <= 11)
                        soma[0] += (digitos[nrDig] *
                        int.Parse(ftmt.Substring(
                          nrDig + 1, 1)));
                    if (nrDig <= 12)
                        soma[1] += (digitos[nrDig] *
                        int.Parse(ftmt.Substring(
                          nrDig, 1)));
                }

                for (nrDig = 0; nrDig < 2; nrDig++)
                {
                    resultado[nrDig] = (soma[nrDig] % 11);
                    if ((resultado[nrDig] == 0) || (resultado[nrDig] == 1))
                        CNPJOk[nrDig] = (
                        digitos[12 + nrDig] == 0);

                    else
                        CNPJOk[nrDig] = (
                        digitos[12 + nrDig] == (
                        11 - resultado[nrDig]));

                }

                return (CNPJOk[0] && CNPJOk[1]);

            }
            catch
            {
                return false;
            }

        }

        //Método que valida o Cep
        public static bool ValidaCep(string cep)
        {
            try
            {
                if (cep.Length == 8)
                {
                    cep = cep.Substring(0, 5) + "-" + cep.Substring(5, 3);
                    //txt.Text = cep;
                }
                return System.Text.RegularExpressions.Regex.IsMatch(cep, ("[0-9]{5}-[0-9]{3}"));

            }
            catch (Exception)
            {
                return false;
            }
        }

        //Método que valida o Email
        public static bool ValidaEmail(string email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(email, ("(?<user>[^@]+)@(?<host>.+)"));
        }
    }

    #endregion

    #region log

    public class LoggerHelper
    {
        private static string _logFolder = "d:/logs/";

        public static void WriteFile(string content, string fileName)
        {
            if (ConfigurationManager.AppSettings["pathLog"] != null)
            {
                _logFolder = ConfigurationManager.AppSettings["pathLog"];
            }
            try
            {
                content = DateTime.Now.ToString("hh:mm:ss") + " " + content;
                fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + fileName + ".txt";
                string path = _logFolder;
                string fullPath = path + fileName;
                File.AppendAllLines(fullPath, new string[1] { content });
            }
            catch (Exception ex)
            {
                string erro = ex.Message;
                erro = "";
            }

        }

        public static string GetFileContent(string fileName)
        {
            try
            {
                if (ConfigurationManager.AppSettings["pathLog"] != null)
                {
                    _logFolder = ConfigurationManager.AppSettings["pathLog"];
                }
                string path = System.Web.HttpContext.Current.Server.MapPath(_logFolder);
                string fullPath = path + fileName + ".txt";

                string ret = "";
                if (File.Exists(fullPath))
                {
                    ret = File.ReadAllText(fullPath);
                }
                return ret;
            }
            catch
            {
                return "";
            }
        }


    }

    #endregion
}