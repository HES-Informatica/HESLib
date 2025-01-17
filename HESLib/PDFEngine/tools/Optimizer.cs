/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

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

using System.Collections.Generic;
using HES.Files;
using HES.Objects;

namespace HES.Tools
{
    /**
      <summary>Tool to enhance PDF files.</summary>
    */
    public sealed class Optimizer
    {
        #region types
        private class AliveObjectCollector
          : Visitor
        {
            private ISet<int> aliveObjectNumbers;

            public AliveObjectCollector(
              ISet<int> aliveObjectNumbers
              )
            { this.aliveObjectNumbers = aliveObjectNumbers; }

            public override PdfObject Visit(
              PdfReference obj,
              object data
              )
            {
                int objectNumber = obj.Reference.ObjectNumber;
                if (aliveObjectNumbers.Contains(objectNumber))
                    return obj;

                aliveObjectNumbers.Add(objectNumber);
                return base.Visit(obj, data);
            }
        }
        #endregion

        #region static
        #region interface
        #region public
        /**
          <summary>Removes indirect objects which have no reference in the document structure.</summary>
          <param name="file">File to optimize.</param>
        */
        public static void RemoveOrphanedObjects(
          PdfFile file
          )
        {
            // 1. Collecting alive indirect objects...
            ISet<int> aliveObjectNumbers = new HashSet<int>();
            {
                // Alive indirect objects collector.
                IVisitor visitor = new AliveObjectCollector(aliveObjectNumbers);
                // Walk through the document structure to collect alive indirect objects!
                file.Trailer.Accept(visitor, null);
            }

            // 2. Removing orphaned indirect objects...
            IndirectObjects indirectObjects = file.IndirectObjects;
            for (int objectNumber = 0, objectCount = indirectObjects.Count; objectNumber < objectCount; objectNumber++)
            {
                if (!aliveObjectNumbers.Contains(objectNumber))
                { indirectObjects.RemoveAt(objectNumber); }
            }
        }
        #endregion
        #endregion
        #endregion
    }
}
