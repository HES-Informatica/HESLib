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
using HESDanfe.Objects;
using drawing = System.Drawing;

namespace HESDanfe.Documents.Contents.ColorSpaces
{
    /**
      <summary>ICC-based color space [PDF:1.6:4.5.4].</summary>
    */

    // TODO:IMPL improve profile support (see ICC.1:2003-09 spec)!!!
    [PDF(VersionEnum.PDF13)]
    public sealed class ICCBasedColorSpace
        : ColorSpace
    {
        //TODO:IMPL new element constructor!

        #region Internal Constructors

        internal ICCBasedColorSpace(
        PdfDirectObject baseObject
                                   ) : base(baseObject)
        { }

        #endregion Internal Constructors

        #region Public Properties

        public override int ComponentCount
        {
            get
            {
                // FIXME: Auto-generated method stub
                return 0;
            }
        }

        public override Color DefaultColor
        {
            get
            { return DeviceGrayColor.Default; } // FIXME:temporary hack...
        }

        public PdfStream Profile
        {
            get
            { return (PdfStream)((PdfArray)BaseDataObject).Resolve(1); }
        }

        #endregion Public Properties

        #region Public Methods

        public override object Clone(
                               Document context
                                    )
        { throw new NotImplementedException(); }

        public override Color GetColor(
            IList<PdfDirectObject> components,
            IContentContext context
                                      )
        {
            return new DeviceRGBColor(components); // FIXME:temporary hack...
        }

        public override drawing::Brush GetPaint(
            Color color
                                               )
        {
            // FIXME: temporary hack
            return new drawing::SolidBrush(drawing::Color.Black);
        }

        #endregion Public Methods
    }
}