using System.Runtime.Serialization;

namespace APIWMS.Data.Enums
{
    /// <summary>
    /// Typy dokumentów w systemie ERP
    /// </summary>
    public enum DocumentType
    {
        [EnumMember(Value = "1601 - Dokument magazynowy (WM)")]
        WM = 1601,

        [EnumMember(Value = "1089 - Przyjęcie magazynowe (PM)")]
        PM = 1089,

        [EnumMember(Value = "1602 - Przesunięcie magazynowe (MP)")]
        MP = 1602,

        [EnumMember(Value = "1093 - Awizo dostawy (AWD)")]
        AWD = 1093,

        [EnumMember(Value = "1605 - Zwrot magazynowy (ZWM)")]
        ZWM = 1605,

        [EnumMember(Value = "2041 - Faktura sprzedaży korygująca (FSK)")]
        FSK = 2041,

        [EnumMember(Value = "2033 - Faktura sprzedaży (FS)")]
        FS = 2033,

        [EnumMember(Value = "2001 - Wydanie zewnętrzne (WZ)")]
        WZ = 2001,

        [EnumMember(Value = "1616 - Rozchód wewnętrzny (RW)")]
        RW = 1616,

        [EnumMember(Value = "1617 - Przyjęcie wewnętrzne (PW)")]
        PW = 1617,

        [EnumMember(Value = "1603 - Między magazynowe wydanie (MMW)")]
        MMW = 1603,

        [EnumMember(Value = "2034 - Paragon (PA)")]
        PA = 2034,

        [EnumMember(Value = "2037 - Faktura sprzedaży eksportowej (FSE)")]
        FSE = 2037,

        [EnumMember(Value = "1604 - Między magazynowe przyjęcie (MMP)")]
        MMP = 1604
    }



    // Groups
    public static class DocumentTypeGroups
    {
        public static readonly HashSet<DocumentType> DokHandlowy = new()
        {
            DocumentType.FS,
            DocumentType.WZ,
            DocumentType.RW,
            DocumentType.PW,
            DocumentType.MMW,
            DocumentType.PA,
            DocumentType.FSE,
            DocumentType.MMP,
            DocumentType.FSK
        };

        public static readonly HashSet<DocumentType> DokMagazynowe = new()
        {
            DocumentType.WM,
            DocumentType.PM,
            DocumentType.MP,
            DocumentType.AWD,
            DocumentType.ZWM
        };
    }
}
