using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema.Models
{
    public class ImagemUploadSuporteMensagem
    {
        public string FileName { get; set; }
        public Guid Guid { get; set; }

        public ImagemUploadSuporteMensagem()
        {

        }

        public ImagemUploadSuporteMensagem(string fileName, Guid guid)
        {
            FileName = fileName;
            Guid = guid;
        }

    }
}