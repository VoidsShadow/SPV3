/**
 * Copyright (C) 2018-2019 Emilian Roman
 * 
 * This file is part of HCE.HCE.BalsamV.
 * 
 * HCE.HCE.BalsamV is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * HCE.HCE.BalsamV is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with HCE.HCE.BalsamV.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace HCE.BalsamV.Settings
{
    // TODO: Handle HW Acceleration & Environmental Sound.

    /// <summary>
    ///     Representation of the profile audio settings.
    /// </summary>
    public class Audio
    {
        /// <summary>
        ///     Audio volume.
        /// </summary>
        public Volume Volume { get; set; } = new Volume();

        /// <summary>
        ///     Audio quality.
        /// </summary>
        public Quality Quality { get; set; } = Quality.High;

        /// <summary>
        ///     Audio variety.
        /// </summary>
        public Quality Variety { get; set; } = Quality.High;
    }
}