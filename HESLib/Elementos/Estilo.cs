using HES.Graphics;
using pcf = HES.Documents.Contents.Fonts;

namespace HES
{
    /// <summary>
    /// Coleção de fontes e medidas a serem compartilhadas entre os elementos básicos.
    /// </summary>
    public class Estilo
    {
        public float PaddingSuperior { get; set; }
        public float PaddingInferior { get; set; }
        public float PaddingHorizontal { get; set; }
        public float FonteTamanhoMinimo { get; set; }

        public pcf.Font FonteInternaRegular { get; set; }
        public pcf.Font FonteInternaNegrito { get; set; }
        public pcf.Font FonteInternaItalico { get; set; }

        public Fonte FonteCampoCabecalho { get; private set; }
        public Fonte FonteCampoConteudo { get; private set; }
        public Fonte FonteCampoConteudoNegrito { get; private set; }
        public Fonte FonteBlocoCabecalho { get; private set; }
        public Fonte FonteNumeroFolhas { get; private set; }

        public Estilo(pcf.Font fontRegular, pcf.Font fontBold, pcf.Font fontItalic, float tamanhoFonteCampoCabecalho = 6, float tamanhoFonteConteudo = 10)
        {
            PaddingHorizontal = 2.5F;
            PaddingSuperior = 0.65F;
            PaddingInferior = 0.3F;

            FonteInternaRegular = fontRegular;
            FonteInternaNegrito = fontBold;
            FonteInternaItalico = fontItalic;

            FonteCampoCabecalho = CriarFonteRegular(tamanhoFonteCampoCabecalho - 1);
            FonteCampoConteudo = CriarFonteRegular(tamanhoFonteConteudo - 1);
            FonteCampoConteudoNegrito = CriarFonteNegrito(tamanhoFonteConteudo - 1);
            FonteBlocoCabecalho = CriarFonteRegular(6);
            FonteNumeroFolhas = CriarFonteNegrito(9F);
            FonteTamanhoMinimo = 4.75F;
        }

        public Fonte CriarFonteRegular(float emSize) => new Fonte(FonteInternaRegular, emSize);
        public Fonte CriarFonteNegrito(float emSize) => new Fonte(FonteInternaNegrito, emSize);
        public Fonte CriarFonteItalico(float emSize) => new Fonte(FonteInternaItalico, emSize);

    }
}
