﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public enum GameEventType {
        Select,
        SetWaypoint,
        SetTarget,
        SpawnUnit,
        SpawnBuilding
    }

    public class GameInputEvent {
        public GameEventType Action {
            get;
            private set;
        }

        public int Team {
            get;
            private set;
        }

        public GameInputEvent(GameEventType a, int t) {
            Action = a;
            Team = t;
        }
    }

    public class SelectEvent : GameInputEvent {
        public List<IEntity> Selected {
            get;
            private set;
        }

        public SelectEvent(int t, List<IEntity> s)
            : base(GameEventType.Select, t) {
            Selected = s;
        }
    }

    public class SetWayPointEvent : GameInputEvent {
        public Vector2 Waypoint {
            get;
            private set;
        }

        public SetWayPointEvent(int t, Vector2 w)
            : base(GameEventType.SetWaypoint, t) {
            Waypoint = w;
        }
    }

    public class SetTargetEvent : GameInputEvent {
        public IEntity Target {
            get;
            private set;
        }

        public SetTargetEvent(int t, IEntity target)
            : base(GameEventType.SetTarget, t) {
            Target = target;
        }
    }

    public class SpawnUnitEvent : GameInputEvent {
        public int Type {
            get;
            private set;
        }

        public Vector2 Position {
            get;
            private set;
        }

        public SpawnUnitEvent(int t, int type, Vector2 pos)
            : base(GameEventType.SpawnUnit, t) {
            Type = type;
            Position = pos;
        }
    }

    public class SpawnBuildingEvent : GameInputEvent {
        public int Type {
            get;
            private set;
        }

        public Vector2 Position {
            get;
            private set;
        }

        public SpawnBuildingEvent(int t, int type, Vector2 pos)
            : base(GameEventType.SpawnBuilding, t) {
            Type = type;
            Position = pos;
        }
    }
}