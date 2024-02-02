using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Sistema.ModelBinders
{
    public class UsuarioBinder
    {
        public string Foto
        {
            get
            {
                // Caminho Virtual
                string strdominio = Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO");
                string strCdn = Core.Helpers.ConfiguracaoHelper.GetString("URL_CDN");
                string strPath = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PERFIL");
                try
                {
                    if (_usuario != null)
                    {
                        //Caminho virtual
                        string caminhoVirtual = strdominio + strCdn.Replace("//", "/") + strPath.Replace("\\","/") + _usuario.ID.ToString("D6") + ".jpg";
                        // Caminho Fisico
                        string caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO") + "\\office\\cdn\\" + strPath + _usuario.ID.ToString("D6") + ".jpg";

                        if (File.Exists(caminhoFisico))
                        {
                            return caminhoVirtual;
                        }
                        else
                        {
                            caminhoVirtual = strdominio + "Content/img/" + (_usuario.Sexo == "M" ? Helpers.Local.Sistema + "/Homem" : Helpers.Local.Sistema + "/Mulher") + ".png"; 
                            caminhoFisico = HttpContext.Current.Request.PhysicalApplicationPath + "Content\\img\\" + (_usuario.Sexo == "M" ? Helpers.Local.Sistema + "\\Homem" : Helpers.Local.Sistema + "\\Mulher") + ".png";
                            if (File.Exists(caminhoFisico))
                            {
                                return caminhoVirtual;
                            }
                            else
                            {
                                caminhoVirtual = strdominio + "Content/img/Homem.png";
                                caminhoFisico = HttpContext.Current.Request.PhysicalApplicationPath + "Content\\img\\" + Helpers.Local.Sistema + "\\Homem.png";
                                if (File.Exists(caminhoFisico))
                                {
                                    return caminhoVirtual;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //Todo tratamento
                }
                return null;
            }
        }

        public string FotoUsuarioId
        {
            get
            {
                // Caminho Virtual
                string strdominio = Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO");
                string strCdn = Core.Helpers.ConfiguracaoHelper.GetString("URL_CDN");
                string strPath = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PERFIL"); // arquivoSecaoRepository.GetById(8).Caminho;
                string caminhoVirtual = strdominio + strCdn + strPath.Replace("/", "//") + _usuarioId.ToString("D6") + ".jpg";
                // Caminho Fisico
                string caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO") + strPath + _usuarioId.ToString("D6") + ".jpg";

                if (File.Exists(caminhoFisico))
                {
                    return caminhoVirtual;
                }
                return null;
            }
        }

        private Core.Entities.Usuario _usuario;
        public Core.Entities.Usuario Usuario
        {
            get
            {
                return _usuario;
            }
            set
            {
                _usuario = value;
            }
        }

        private int _usuarioId;
        public int usuarioId
        {
            get
            {
                return _usuarioId;
            }
            set
            {
                _usuarioId = value;
            }
        }
    }
}