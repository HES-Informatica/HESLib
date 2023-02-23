﻿using System;
using System.Drawing;
using System.Text;
using org.pdfclown.documents.contents.composition;

namespace HESDanfe
{
    internal static class Extensions
    {
        #region Private Fields

        private const float PointFactor = 72F / 25.4F;

        #endregion Private Fields

        #region Internal Methods

        internal static Estilo CriarEstilo(float tFonteCampoCabecalho = 6, float tFonteCampoConteudo = 10) => new Estilo(DANFE.FonteRegular, DANFE.FonteNegrito, DANFE.FonteItalico, tFonteCampoCabecalho, tFonteCampoConteudo);

        #endregion Internal Methods

        #region Public Methods

        public static StringBuilder AppendChaveValor(this StringBuilder sb, string chave, string valor)
        {
            if (sb.Length > 0) sb.Append(' ');
            return sb.Append(chave).Append(": ").Append(valor);
        }

        public static RectangleF CutBottom(this RectangleF r, float height) => new RectangleF(r.X, r.Y, r.Width, r.Height - height);

        public static RectangleF CutLeft(this RectangleF r, float width) => new RectangleF(r.X + width, r.Y, r.Width - width, r.Height);

        public static RectangleF CutTop(this RectangleF r, float height) => new RectangleF(r.X, r.Y + height, r.Width, r.Height - height);

        public static RectangleF InflatedRetangle(this RectangleF rect, float top, float button, float horizontal) => new RectangleF(rect.X + horizontal, rect.Y + top, rect.Width - 2 * horizontal, rect.Height - top - button);

        public static RectangleF InflatedRetangle(this RectangleF rect, float value) => rect.InflatedRetangle(value, value, value);

        /// <summary>
        /// Converts Point to Millimeters
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float ToMm(this float point) => point / PointFactor;

        /// <summary>
        /// Converts Point to Millimeters
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static SizeF ToMm(this SizeF s) => new SizeF(s.Width.ToMm(), s.Height.ToMm());

        /// <summary>
        /// Converts Point to Millimeters
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double ToMm(this double point) => point / PointFactor;

        public static XAlignmentEnum ToPdfClownAlignment(this AlinhamentoHorizontal ah)
        {
            switch (ah)
            {
                case AlinhamentoHorizontal.Centro:
                    return XAlignmentEnum.Center;

                case AlinhamentoHorizontal.Direita:
                    return XAlignmentEnum.Right;

                case AlinhamentoHorizontal.Esquerda:
                default:
                    return XAlignmentEnum.Left;
            }

            throw new InvalidOperationException();
        }

        public static YAlignmentEnum ToPdfClownAlignment(this AlinhamentoVertical av)
        {
            switch (av)
            {
                case AlinhamentoVertical.Topo:
                    return YAlignmentEnum.Top;

                case AlinhamentoVertical.Centro:
                    return YAlignmentEnum.Middle;

                case AlinhamentoVertical.Base:
                    return YAlignmentEnum.Bottom;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Converts Millimeters to Point
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        public static float ToPoint(this float mm) => PointFactor * mm;

        /// <summary>
        /// Converts Millimeters to Point
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        public static double ToPoint(this double mm) => PointFactor * mm;

        /// <summary>
        /// Converts Point to Millimeters
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static SizeF ToPointMeasure(this SizeF s) => new SizeF(s.Width.ToPoint(), s.Height.ToPoint());

        public static RectangleF ToPointMeasure(this RectangleF r) => new RectangleF(r.X.ToPoint(), r.Y.ToPoint(), r.Width.ToPoint(), r.Height.ToPoint());

        public static PointF ToPointMeasure(this PointF r) => new PointF(r.X.ToPoint(), r.Y.ToPoint());

        #endregion Public Methods
    }
}