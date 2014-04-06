﻿using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RTSEngine.Controllers {
    public class EnvironmentInputController : InputController {
        Thread thread;
        private bool running;
        private bool paused;

        private Random random;
        private ImpactGrid grid;
        private int counter;

        private Vector2[] treePositions;
        private IndexedBuildingType tree;
        private IndexedBuildingType ore;
        private Point[,] numSpawns;
        private IndexedUnitType[] spawns;
        private int[] spawnCaps;

        // Impact Levels For Level One, Level Two, Level Three
        private const int LEVEL_ONE = 10;
        private const int LEVEL_TWO = 20;
        private const int LEVEL_THREE = 30;

        // Impact Level At Which Region Will No Longer Recover
        private const int RECOVER_IMPACT = 25;

        // Time Intervals At Which Recovery And Spawning Happens (In Seconds)
        private const int RECOVER_TIME = 30;
        private const int SPAWN_TIME = 60;

        // Maximum Number Of Units Per Region
        private const int SPAWN_CAP1 = 10;
        private const int SPAWN_CAP2 = 20;
        private const int SPAWN_CAP3 = 30;

        private const int MAX_OFFSET = 20;
        private const int MIN_RECOVER = 5;
        private const int MAX_RECOVER = 10;
        
        public EnvironmentInputController(GameState g, int ti)
            : base(g, ti) {
            grid = g.IGrid;
            spawnCaps = new int[3];
            spawnCaps[0] = SPAWN_CAP1;
            spawnCaps[1] = SPAWN_CAP2;
            spawnCaps[2] = SPAWN_CAP3;
            random = new Random();
            thread = new Thread(WorkThread);
            thread.IsBackground = true;
            running = true;
            paused = true;
            thread.Start();
        }

        public void Init(IndexedUnitType[] s, Point[,] n, IndexedBuildingType t, IndexedBuildingType o, Vector2[] treePos, Vector2[] orePos) {
            spawns = s;
            numSpawns = n;
            tree = t;
            ore = o;
            treePositions = treePos;
            for(int i = 0; i < treePositions.Length; i++) {
                Point treeC = HashHelper.Hash(treePositions[i], GameState.IGrid.numCells, GameState.IGrid.size);
                AddEvent(new SpawnBuildingEvent(TeamIndex, tree.Index, treeC));
            }
            for(int j = 0; j < orePos.Length; j++) {
                Point oreC = HashHelper.Hash(orePos[j], GameState.IGrid.numCells, GameState.IGrid.size);
                AddEvent(new SpawnBuildingEvent(TeamIndex, ore.Index, oreC));
            }
        }

        private void WorkThread() {
            while(running) {
                if(paused) {
                    Thread.Sleep(1000);
                    continue;
                }
                UpdateUnitsInRegion();
                if(counter % SPAWN_TIME == 0) {
                    SpawnUnits();
                    SetInitTarget();
                }
                if(counter % RECOVER_TIME == 0) {
                    Recover();
                }
                counter++;
                counter = counter % (SPAWN_TIME + RECOVER_TIME);
                Thread.Sleep(1000);
            }
        }

        public void Start() {
            counter = 0;
            paused = false;
        }
        public override void Dispose() {
            running = false;
        }

        private void UpdateUnitsInRegion() {
            foreach (var r in GameState.Regions) {
                r.units = new List<IEntity>();
            }
            foreach (var u in Team.units) {
                Point unitI = HashHelper.Hash(u.GridPosition, grid.numCells, grid.size);
                grid.Region[unitI.X, unitI.Y].units.Add(u);
            }
        }

        private void Recover() {
            foreach(var r in GameState.Regions) {
                if(r.RegionImpact < RECOVER_IMPACT && r.RegionImpact > 0) {
                    // Randomly Choose The Location Of A Starting Tree
                    int tp = random.Next(treePositions.Length);
                    Vector2 treePos = treePositions[tp];
                    Point treeC = HashHelper.Hash(treePos,GameState.CGrid.numCells,GameState.CGrid.size);
                    Point treeI = HashHelper.Hash(treePos,grid.numCells,grid.size);
                    // Spawn Trees Around The Starting Tree
                    for(int x = -1; x <= 1; x+=2) {
                        for (int y = -1; y <= 1; y+=2) {
                            Point newTreeC = new Point(treeC.X + x, treeC.Y + y);
                            Vector2 newTreePos = new Vector2(newTreeC.X * GameState.CGrid.size.X, newTreeC.Y * GameState.CGrid.size.Y);
                            Point newTreeI = HashHelper.Hash(newTreePos, grid.numCells, grid.size);
                            foreach (var t in GameState.IGrid.ImpactGenerators[newTreeI.X,newTreeI.Y]) {
                                Point tc = HashHelper.Hash(t.GridPosition, GameState.CGrid.numCells, GameState.CGrid.size);
                                if (!Point.Equals(tc, newTreeC) && tc.X > -1 && tc.Y > -1 && tc.X < GameState.CGrid.numCells.X && tc.Y < GameState.CGrid.numCells.Y) {
                                    AddEvent(new SpawnBuildingEvent(TeamIndex, tree.Index, newTreeC));
                                    r.AddToRegionImpact(-tree.Data.Impact);
                                }
                            }
                        }
                    }
                    // Regenerate Ore Health
                    foreach(var c in r.Cells) {
                        foreach(var o in GameState.IGrid.ImpactGenerators[c.X, c.Y]) {
                            if(o.Data.FriendlyName.Equals(ore.Data.FriendlyName)) {
                                int recover = random.Next(MIN_RECOVER, MAX_RECOVER);
                                o.Health += recover;
                                r.AddToRegionImpact(-(ore.Data.Impact / recover));
                            }
                        }
                    }

                }
            }
        }

        private void SpawnUnits() {
            foreach(var r in GameState.Regions) {
                // Decide Level
                int level;
                if (r.RegionImpact > LEVEL_THREE)
                    level = 3;
                else if (r.RegionImpact > LEVEL_TWO)
                    level = 2;
                else if (r.RegionImpact > LEVEL_ONE)
                    level = 1;
                else
                    level = 0;
                if(level > 0) {
                    // Find The Cell With The Largest Impact
                    Point p = r.Cells.First();
                    foreach(var c in r.Cells) {
                        if(grid.CellImpact[c.X, c.Y] > grid.CellImpact[p.X, p.Y]) {
                            p = c;
                        }
                    }
                    // Randomly Choose An Impact Generator In That Cell
                    ImpactGenerator g = null;
                    while(g == null || !g.Data.FriendlyName.Equals(ore.Data.FriendlyName) || !g.Data.FriendlyName.Equals(tree.Data.FriendlyName)) {
                        int i = random.Next(grid.ImpactGenerators[p.X, p.Y].Count);
                        g = grid.ImpactGenerators[p.X, p.Y][i];
                    }
                    // Spawn Environmental Units
                    Vector2 spawnPos = g.GridPosition;
                    Vector2 offset;
                    int numSpawn;
                    for (int i = 0; i < numSpawns.GetLength(1) && r.units.Count < spawnCaps[level - 1]; i++) {
                            numSpawn = random.Next(numSpawns[level - 1, i].X, numSpawns[level - 1, i].Y);
                            offset.X = random.Next(MAX_OFFSET);
                            offset.Y = random.Next(MAX_OFFSET);
                            for (int j = 0; j < numSpawn; j++) {
                                AddEvent(new SpawnUnitEvent(TeamIndex, spawns[i].Index, spawnPos + offset));
                            }
                    }
                }

            }
        }

        private void SetInitTarget() {
            foreach (var r in GameState.Regions) {
                // Select Units Not In A Squad
                List<IEntity> squad = new List<IEntity>();
                foreach (RTSUnit u in r.units){
                    if(u.Squad.Units.Count == 1)
                        squad.Add(u);
                }
                AddEvent(new SelectEvent(TeamIndex, squad));
                // Set The Target For Those Units
                IEntity target = null;
                Vector2 sumPos = Vector2.Zero;
                foreach (var u2 in squad)
                    sumPos += u2.GridPosition;
                Vector2 averagePos = new Vector2(sumPos.X / squad.Count, sumPos.Y / squad.Count);
                foreach (var t2 in GameState.activeTeams) 
                    if (t2.Index != TeamIndex) 
                        foreach (var u3 in t2.Team.units) 
                            if (target == null || Vector2.Distance(u3.GridPosition, averagePos) < Vector2.Distance(u3.GridPosition, target.GridPosition)) 
                                target = u3;    
                AddEvent(new SetTargetEvent(TeamIndex, target));
            }
        }

    }
}