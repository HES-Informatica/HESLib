/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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

using HESDanfe.Documents;
using HESDanfe.Documents.Files;
using HESDanfe.Files;
using HESDanfe.Objects;

using System;

namespace HESDanfe.Documents
{
  /**
    <summary>Named embedded files [PDF:1.6:3.6.3].</summary>
  */
  [PDF(VersionEnum.PDF14)]
  public sealed class NamedEmbeddedFiles
    : NameTree<FileSpecification>
  {
    #region dynamic
    #region constructors
    public NamedEmbeddedFiles(
      Document context
      ) : base(context)
    {}

    internal NamedEmbeddedFiles(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region protected
    protected override FileSpecification WrapValue(
      PdfDirectObject baseObject
      )
    {return FileSpecification.Wrap(baseObject);}
    #endregion
    #endregion
    #endregion
  }
}