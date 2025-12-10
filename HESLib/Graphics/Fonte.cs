using System;
using Extensions;
using pcf = org.pdfclown.documents.contents.fonts;

namespace HES.Graphics
{
    /// <summary>
    /// Define uma fonte do PDF Clown com tamanho específico.
    /// Encapsula a fonte interna do PDF Clown e fornece métodos de medição.
    /// </summary>
    public class Fonte
    {
        private float _Tamanho;

        /// <summary>
        /// Fonte interna do PDF Clown.
        /// </summary>
        public pcf.Font FonteInterna { get; private set; }

        /// <summary>
        /// Cria uma nova instância de Fonte.
        /// </summary>
        /// <param name="font">Fonte interna do PDF Clown.</param>
        /// <param name="tamanho">Tamanho da fonte em pontos.</param>
        public Fonte(pcf.Font font, float tamanho)
        {
            FonteInterna = font ?? throw new ArgumentNullException(nameof(font));
            Tamanho = tamanho;
        }

        /// <summary>
        /// Tamanho da fonte.
        /// </summary>
        public float Tamanho
        {
            get => _Tamanho;
            set
            {
                if (value <= 0) throw new InvalidOperationException("O tamanho deve ser maior que zero.");
                _Tamanho = value;
            }
        }

        /// <summary>
        /// Mede a largura ocupada por uma string com esta fonte.
        /// </summary>
        /// <param name="str">String a ser medida.</param>
        /// <returns>Largura em milímetros.</returns>
        public float MedirLarguraTexto(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            return (float)FonteInterna.GetWidth(str, Tamanho).ToMm();
        }

        /// <summary>
        /// Mede a largura ocupada por um caractere com esta fonte.
        /// </summary>
        /// <param name="c">Caractere a ser medido.</param>
        /// <returns>Largura em milímetros.</returns>
        public float MedirLarguraChar(char c) => (float)FonteInterna.GetWidth(c, Tamanho).ToMm();

        /// <summary>
        /// Obtém a altura da linha para esta fonte.
        /// </summary>
        public float AlturaLinha => (float)FonteInterna.GetLineHeight(Tamanho).ToMm();

        /// <summary>
        /// Cria uma cópia desta fonte.
        /// </summary>
        public Fonte Clonar() => new Fonte(FonteInterna, Tamanho);
    }
}
