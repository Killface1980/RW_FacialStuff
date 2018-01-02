// ReSharper disable StyleCop.SA1307
// ReSharper disable InconsistentNaming
// ReSharper disable StyleCop.SA1401
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace FacialStuff.Defs
{
    public class HandDef : ThingDef
    {

        #region Public Fields

        [NotNull]
        public List<CompLoaderTargets> CompLoaderTargets = new List<CompLoaderTargets>();

        #endregion Public Fields

    }
}
