﻿// Itinero - Routing for .NET
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

namespace Itinero.Data.Network.Edges
{
    /// <summary>
    /// Represents data attached to an edge in a routing network.
    /// </summary>
    public struct EdgeData
    {
        /// <summary>
        /// Gets or sets the profile id.
        /// </summary>
        public ushort Profile { get; set; }

        /// <summary>
        /// Gets or sets the distance.
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// Gets or sets the attributes id.
        /// </summary>
        public uint MetaId { get; set; }
    }
}