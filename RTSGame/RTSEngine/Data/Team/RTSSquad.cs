﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSSquad {
        // This Squad's Team
        public RTSTeam Team {
            get;
            private set;
        }

        // Units In The Squad
        private List<RTSUnit> units;
        public List<RTSUnit> Units {
            get { return units; }
        }

        // Death Condition
        public bool IsDead {
            get { return units.Count < 1; }
        }
        public event Action<RTSSquad> OnDeath;

        // The Average Position Of The Squad
        private Vector2 gridPos;
        public Vector2 GridPosition {
            get { return gridPos; }
        }

        // Events When Squad Is Altered
        public event Action<RTSSquad, RTSUnit> OnUnitAddition;
        public event Action<RTSSquad, RTSUnit> OnUnitRemoval;

        // The Action Controller For This Squad
        private ACSquadActionController aController;
        public ACSquadActionController ActionController {
            get { return aController; }
            set {
                aController = value;
                if(aController != null)
                    aController.SetSquad(this);
            }
        }

        // The Targetting Controller For This Squad
        private ACSquadTargettingController tController;
        public ACSquadTargettingController TargettingController {
            get { return tController; }
            set {
                tController = value;
                if(tController != null)
                    tController.SetSquad(this);
            }
        }

        public RTSSquad(RTSTeam team) {
            Team = team;
            units = new List<RTSUnit>();
        }

        // Adds A Combatant To This Squad
        public void Add(RTSUnit u) {
            // Units Cannot Be Added Twice
            if(u.Squad == this) return;

            // Squad Invariant Performed Here
            if(u.Squad != null) u.Squad.Remove(u);

            u.Squad = this;
            units.Add(u);
            u.OnDestruction += OnUnitDestruction;
            if(OnUnitAddition != null)
                OnUnitAddition(this, u);
        }
        public void Remove(RTSUnit u) {
            // Make Sure Unit Is In The Squad
            if(u.Squad == this) {
                // Remove All References
                units.Remove(u);
                u.OnDestruction -= OnUnitDestruction;

                // Send Update Event
                if(OnUnitRemoval != null)
                    OnUnitRemoval(this, u);

                // Check Death Condition
                if(IsDead && OnDeath != null)
                    OnDeath(this);
            }
        }

        // Removes All Combatants From This Squad That Match A Predicate
        public void RemoveAll(Predicate<RTSUnit> f) {
            List<RTSUnit> nUnits = new List<RTSUnit>(units.Count);
            for(int i = 0; i < units.Count; i++) {
                if(f(units[i])) {
                    if(OnUnitRemoval != null)
                        OnUnitRemoval(this, units[i]);
                }
                else nUnits.Add(units[i]);
            }

            // Set The New List Of Units
            units = nUnits;

            // Check Death Condition
            if(IsDead && OnDeath != null) 
                OnDeath(this);
        }

        // Should Be Done At The Beginning Of Each Frame (Only Once)
        public void RecalculateGridPosition() {
            if(units.Count > 0) {
                gridPos = Vector2.Zero;
                foreach(var u in units)
                    gridPos += u.GridPosition;
                gridPos /= units.Count;
            }
        }

        // When A Unit Dies, It Must Be Removed From Here
        private void OnUnitDestruction(IEntity u) {
            Remove(u as RTSUnit);
        }
    }
}