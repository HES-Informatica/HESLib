/*
  Copyright 2006-2011 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

using System;
using System.Collections.Generic;
using HES.Objects;
using drawing = System.Drawing;

namespace HES.Documents.Contents.ColorSpaces
{
    /**
      <summary>CIE-based A single-transformation-stage color space, where A represents a calibrated
      achromatic single-component color value [PDF:1.6:4.5.4].</summary>
    */

    [PDF(VersionEnum.PDF11)]
    public sealed class CalGrayColorSpace
        : CalColorSpace
    {
        // TODO:IMPL new element constructor!

        #region Internal Constructors

        internal CalGrayColorSpace(
        PdfDirectObject baseObject
                                  ) : base(baseObject)
        { }

        #endregion Internal Constructors

        #region Public Properties

        public override int ComponentCount => 1;

        public override Color DefaultColor => CalGrayColor.Default;

        public override double[] Gamma
        {
            get
            {
                IPdfNumber gammaObject = (IPdfNumber)Dictionary[PdfName.Gamma];
                return (gammaObject == null
                    ? new double[] { 1 }
                    : new double[] { gammaObject.RawValue }
                       );
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override object Clone(
                               Document context
                                    ) => throw new NotImplementedException();

        public override Color GetColor(
            IList<PdfDirectObject> components,
            IContentContext context
                                      ) => new CalGrayColor(components);

        public override drawing::Brush GetPaint(
            Color color
                                               ) =>
            // FIXME: temporary hack
            new drawing::SolidBrush(drawing::Color.Black);

        #endregion Public Methods
    }
}