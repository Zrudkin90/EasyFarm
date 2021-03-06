﻿/*///////////////////////////////////////////////////////////////////
<EasyFarm, general farming utility for FFXI.>
Copyright (C) <2013>  <Zerolimits>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
*/
///////////////////////////////////////////////////////////////////

using EasyFarm.States;
using MemoryAPI;
using Prism.Mvvm;

namespace EasyFarm.Infrastructure
{
    public class ViewModelBase : BindableBase, IViewModel
    {
        public delegate void SessionSet(IMemoryAPI fface);

        /// <summary>
        ///     Global game engine controlling the player.
        /// </summary>
        public static GameEngine GameEngine;

        /// <summary>
        ///     Solo fface instance for current player.
        /// </summary>
        public static IMemoryAPI FFACE { get; set; }

        /// <summary>
        ///     View Model name for header in tab control item.
        /// </summary>
        public string ViewName { get; set; }

        public static void SetSession(IMemoryAPI fface)
        {
            if (fface == null) return;

            // Save fface Write
            FFACE = fface;

            // Create a new game engine to control our character. 
            GameEngine = new GameEngine(FFACE);

            OnSessionSet?.Invoke(fface);
        }

        public static event SessionSet OnSessionSet;
    }
}