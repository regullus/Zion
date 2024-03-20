using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class TabuleiroModel
    {
        public int ID { get; set; }
        public int BoardID { get; set; }
        public int StatusID { get; set; }
        public int? Master { get; set; }
        public string NomeMaster { get; set; }
        public int? pinMaster { get; set; }
        public int? CoordinatorDir { get; set; }
        public string NomeCoordinatorDir { get; set; }
        public int? pinCoordinatorDir { get; set; }
        public int? IndicatorDirSup { get; set; }
        public string NomeIndicatorDirSup { get; set; }
        public int? pinIndicatorDirSup { get; set; }
        public int? IndicatorDirInf { get; set; }
        public string NomeIndicatorDirInf { get; set; }
        public int? pinIndicatorDirInf { get; set; }
        public int? DonatorDirSup1 { get; set; }
        public string NomeDonatorDirSup1 { get; set; }
        public int? pinDonatorDirSup1 { get; set; }
        public int? DonatorDirSup2 { get; set; }
        public string NomeDonatorDirSup2 { get; set; }
        public int? pinDonatorDirSup2 { get; set; }
        public int? DonatorDirInf1 { get; set; }
        public string NomeDonatorDirInf1 { get; set; }
        public int? pinDonatorDirInf1 { get; set; }
        public int? DonatorDirInf2 { get; set; }
        public string NomeDonatorDirInf2 { get; set; }
        public int? pinDonatorDirInf2 { get; set; }
        public int? CoordinatorEsq { get; set; }
        public string NomeCoordinatorEsq { get; set; }
        public int? pinCoordinatorEsq { get; set; }
        public int? IndicatorEsqSup { get; set; }
        public string NomeIndicatorEsqSup { get; set; }
        public int? pinIndicatorEsqSup { get; set; }
        public int? IndicatorEsqInf { get; set; }
        public string NomeIndicatorEsqInf { get; set; }
        public int? pinIndicatorEsqInf { get; set; }
        public int? DonatorEsqSup1 { get; set; }
        public string NomeDonatorEsqSup1 { get; set; }
        public int? pinDonatorEsqSup1 { get; set; }
        public int? DonatorEsqSup2 { get; set; }
        public string NomeDonatorEsqSup2 { get; set; }
        public int? pinDonatorEsqSup2 { get; set; }
        public int? DonatorEsqInf1 { get; set; }
        public string NomeDonatorEsqInf1 { get; set; }
        public int? pinDonatorEsqInf1 { get; set; }
        public int? DonatorEsqInf2 { get; set; }
        public string NomeDonatorEsqInf2 { get; set; }
        public int? pinDonatorEsqInf2 { get; set; }
        public int DataInicio { get; set; }
        public int? DataFim { get; set; }
    }
}
