﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSUnit {
        // The Friendly Name
        public string FriendlyName;

        // Health Of The Unit
        public int Health;

        // The Capital Cost Of The Unit
        public int CapitalCost;
        // The Population Cost Of The Unit
        public int PopulationCost;

        // Speed Of The Unit
        public float MovementSpeed;

        //BaseCombatData of The Unit
        public BaseCombatData BaseCombatData;

        //ICollidableShape of The Unit
        public ICollidable ICollidableShape;
    }
}