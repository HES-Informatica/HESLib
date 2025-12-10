using System;
using System.Drawing;
using HES.Graphics;

namespace HES
{
    /// <summary>
    /// Classe base para todos os objetos desenháveis no DANFE.
    /// Fornece propriedades e métodos básicos para posição e tamanho.
    /// </summary>
    internal abstract class DrawableBase
    {
        /// <summary>
        /// Coordenada X (horizontal) do objeto.
        /// </summary>
        public virtual float X { get; set; }

        /// <summary>
        /// Coordenada Y (vertical) do objeto.
        /// </summary>
        public virtual float Y { get; set; }

        /// <summary>
        /// Largura do objeto em milímetros.
        /// </summary>
        public virtual float Width { get; set; }

        /// <summary>
        /// Altura do objeto em milímetros.
        /// </summary>
        public virtual float Height { get; set; }

        /// <summary>
        /// Posição do objeto como um ponto.
        /// </summary>
        public PointF Position => new PointF(X, Y);

        /// <summary>
        /// Tamanho do objeto.
        /// </summary>
        public SizeF Size => new SizeF(Width, Height);

        public DrawableBase()
        {
        }

        /// <summary>
        /// Desenha o objeto usando o contexto gráfico fornecido.
        /// Valida as propriedades básicas antes do desenho.
        /// </summary>
        /// <param name="gfx">Contexto gráfico.</param>
        public virtual void Draw(Gfx gfx)
        {
            if (gfx == null) throw new ArgumentNullException(nameof(gfx));
            if (Width <= 0) throw new InvalidOperationException("Width is invalid.");
            if (Height <= 0) throw new InvalidOperationException("Height is invalid.");
            if (X < 0) throw new InvalidOperationException("X is invalid.");
            if (Y < 0) throw new InvalidOperationException("Y is invalid.");
        }

        /// <summary>
        /// Define a posição do objeto.
        /// </summary>
        /// <param name="x">Coordenada X.</param>
        /// <param name="y">Coordenada Y.</param>
        public virtual void SetPosition(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Define a posição do objeto a partir de um ponto.
        /// </summary>
        /// <param name="p">Ponto com as coordenadas.</param>
        public virtual void SetPosition(PointF p) => SetPosition(p.X, p.Y);

        /// <summary>
        /// Define o tamanho do objeto.
        /// </summary>
        /// <param name="w">Largura.</param>
        /// <param name="h">Altura.</param>
        public virtual void SetSize(float w, float h)
        {
            Width = w;
            if (Height != h)
                Height = h;
        }

        /// <summary>
        /// Define o tamanho do objeto a partir de um SizeF.
        /// </summary>
        /// <param name="s">Tamanho.</param>
        public virtual void SetSize(SizeF s) => SetSize(s.Width, s.Height);

        /// <summary>
        /// Retângulo delimitador do objeto.
        /// </summary>
        public RectangleF BoundingBox => new RectangleF(X, Y, Width, Height);
    }
}
