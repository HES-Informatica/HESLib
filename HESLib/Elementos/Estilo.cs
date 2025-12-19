using HES.Graphics;
using pcf = org.pdfclown.documents.contents.fonts;

namespace HES
{
    /// <summary>
    /// Define o estilo visual padrão para elementos do DANFE.
    /// Inclui fontes, espaçamentos e tamanhos padrão conforme Manual de Orientação do Contribuinte.
    /// </summary>
    public class Estilo
    {
        /// <summary>
        /// Espaçamento superior interno dos elementos (em mm).
        /// </summary>
        public float PaddingSuperior { get; set; }

        /// <summary>
        /// Espaçamento inferior interno dos elementos (em mm).
        /// </summary>
        public float PaddingInferior { get; set; }

        /// <summary>
        /// Espaçamento horizontal interno dos elementos (em mm).
        /// </summary>
        public float PaddingHorizontal { get; set; }

        /// <summary>
        /// Tamanho mínimo de fonte permitido.
        /// </summary>
        public float FonteTamanhoMinimo { get; set; }

        /// <summary>
        /// Fonte regular interna do PDF Clown.
        /// </summary>
        public pcf.Font FonteInternaRegular { get; set; }

        /// <summary>
        /// Fonte negrito interna do PDF Clown.
        /// </summary>
        public pcf.Font FonteInternaNegrito { get; set; }

        /// <summary>
        /// Fonte itálico interna do PDF Clown.
        /// </summary>
        public pcf.Font FonteInternaItalico { get; set; }

        /// <summary>
        /// Fonte para cabeçalhos de campos.
        /// </summary>
        public Fonte FonteCampoCabecalho { get; private set; }

        /// <summary>
        /// Fonte para conteúdo de campos.
        /// </summary>
        public Fonte FonteCampoConteudo { get; private set; }

        /// <summary>
        /// Fonte negrito para conteúdo de campos.
        /// </summary>
        public Fonte FonteCampoConteudoNegrito { get; private set; }

        /// <summary>
        /// Fonte para cabeçalhos de blocos.
        /// </summary>
        public Fonte FonteBlocoCabecalho { get; private set; }

        /// <summary>
        /// Fonte para numeração de folhas.
        /// </summary>
        public Fonte FonteNumeroFolhas { get; private set; }

        /// <summary>
        /// Cria um novo estilo com fontes e tamanhos especificados.
        /// </summary>
        /// <param name="fontRegular">Fonte regular (Times New Roman ou Courier New).</param>
        /// <param name="fontBold">Fonte negrito.</param>
        /// <param name="fontItalic">Fonte itálico.</param>
        /// <param name="tamanhoFonteCampoCabecalho">Tamanho da fonte para cabeçalhos de campos.</param>
        /// <param name="tamanhoFonteConteudo">Tamanho da fonte para conteúdo.</param>
        public Estilo(pcf.Font fontRegular, pcf.Font fontBold, pcf.Font fontItalic, float tamanhoFonteCampoCabecalho = 6, float tamanhoFonteConteudo = 10)
        {
            // Espaçamentos padrão conforme layout do DANFE
            PaddingHorizontal = 1.1F;
            PaddingSuperior = 0.65F;
            PaddingInferior = 0.3F;

            FonteInternaRegular = fontRegular;
            FonteInternaNegrito = fontBold;
            FonteInternaItalico = fontItalic;

            // Cria fontes com tamanhos específicos
            FonteCampoCabecalho = CriarFonteRegular(tamanhoFonteCampoCabecalho - 1);
            FonteCampoConteudo = CriarFonteRegular(tamanhoFonteConteudo - 1);
            FonteCampoConteudoNegrito = CriarFonteNegrito(tamanhoFonteConteudo - 1);
            FonteBlocoCabecalho = CriarFonteRegular(6);
            FonteNumeroFolhas = CriarFonteNegrito(9F);
            FonteTamanhoMinimo = 4.75F;
        }

        /// <summary>
        /// Cria uma fonte regular com o tamanho especificado.
        /// </summary>
        public Fonte CriarFonteRegular(float emSize) => new Fonte(FonteInternaRegular, emSize);

        /// <summary>
        /// Cria uma fonte negrito com o tamanho especificado.
        /// </summary>
        public Fonte CriarFonteNegrito(float emSize) => new Fonte(FonteInternaNegrito, emSize);

        /// <summary>
        /// Cria uma fonte itálico com o tamanho especificado.
        /// </summary>
        public Fonte CriarFonteItalico(float emSize) => new Fonte(FonteInternaItalico, emSize);
    }
}
