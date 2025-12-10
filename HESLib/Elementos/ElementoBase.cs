using System;
using HES.Graphics;

namespace HES
{
    /// <summary>
    /// Classe base para elementos do DANFE que possuem estilo e contorno.
    /// Estende DrawableBase adicionando suporte a estilos visuais.
    /// </summary>
    internal abstract class ElementoBase : DrawableBase
    {
        /// <summary>
        /// Estilo visual do elemento (fontes, cores, espaçamentos).
        /// </summary>
        public Estilo Estilo { get; protected set; }

        /// <summary>
        /// Indica se o elemento possui contorno visível.
        /// </summary>
        public virtual bool PossuiContono => true;

        /// <summary>
        /// Cria uma nova instância de ElementoBase.
        /// </summary>
        /// <param name="estilo">Estilo a ser aplicado ao elemento.</param>
        public ElementoBase(Estilo estilo)
        {
            Estilo = estilo ?? throw new ArgumentNullException(nameof(estilo));
        }

        /// <summary>
        /// Desenha o elemento e seu contorno (se aplicável).
        /// </summary>
        /// <param name="gfx">Contexto gráfico.</param>
        public override void Draw(Gfx gfx)
        {
            base.Draw(gfx);
            if (PossuiContono)
                gfx.StrokeRectangle(BoundingBox, 0.25f);
        }
    }
}
