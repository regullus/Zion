using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Sistema.ModelBinders
{
    [Serializable]
    public class NoRedeBinderOld
    {
        public int id;
        public string name;
        public DataNoRedeBinder data;
        public List<NoRedeBinder> children;
    }

    [Serializable]
    public class NoRedeBinder
    {
        public int id;
        public int? parent;
        public string description;
        public string title;
        public string label;
        public string itemTitleColor;
        public string groupTitleColor;
        public string phone;
        public string image;
    }

    [Serializable]
    public class DataNoRedeBinder
    {
        public string classificacao;
        public string sexo;
        public string nome;
        public int statusAtivacao;
        public string email;
        public string telefone;
        public string observacoes;
        public string ladoPatrocinador;
    }

}
