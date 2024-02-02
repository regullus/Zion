using cpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public class CriptografiaHelper
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
                    cpUtilities.LoggerHelper.WriteFile("ERROR DEScriptografar1 (" + text + ") : " + ex.Message, "CriptografiaHelper");
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
                cpUtilities.LoggerHelper.WriteFile("ERROR DEScriptografar2 (" + text + ") : " + e.Message, "CriptografiaHelper");
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

        #region Morpho

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

        /// <summary>
        /// Criptografia 
        /// </summary>
        /// <param name="strEnt">Palavra que deseja criptografar ou descriptografar.</param>
        /// <param name="tipo">Criptografa, Descriptografa.</param>
        /// <returns>Valor criptografado ou descriptografar.</returns>
        public static string Morpho(string strEnt, string strChave, TipoCriptografia tipo)
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

    }
}
