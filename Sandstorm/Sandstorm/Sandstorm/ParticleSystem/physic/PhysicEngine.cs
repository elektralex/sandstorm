﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Sandstorm.ParticleSystem.structs;
using Sandstorm.Terrain;

namespace Sandstorm.ParticleSystem.physic
{
    class PhysicEngine
    {
        private SharedList _sharedList = null;
        private FPSCounter _fpsCounter = new FPSCounter();

        private List<Vector3> _forces = new List<Vector3>();

        private HeightMap _heightMap;

        public PhysicEngine(SharedList pList, HeightMap heightMap)
        {
            this._sharedList = pList;
            this._heightMap = heightMap;

            this._forces.Add(new Vector3(0f, -0.1f, 0f));
        }

        public void Update(GameTime pGameTime) //Update physic
        {
            Parallel.ForEach(_sharedList.getParticles(), p =>
            {
                p.move();//move the Particle
                //check colision
                Vector3 nextPosition = p.getPosition();// +p.getForce();
                float pHeight = _heightMap.getHeight(nextPosition.X, nextPosition.Z);

                if (nextPosition.Y < pHeight + p.getRadius())
                {
                    Vector3 normal = _heightMap.getNormal(nextPosition.X, nextPosition.Z);
                    p.reflect(normal);
                    p.setPosition(new Vector3(p.getPosition().X, pHeight, p.getPosition().Z) + (p.getRadius() * 1.005f * normal));
                    p.applyFriction(0.2f);
                }

                foreach (Vector3 f in _forces)//apply external forces (Gravitation etc)
                {
                    p.applyExternalForce(f);
                }
                //p.collide(_sharedList.getParticles().ToArray());
            });
        }

        public void Draw() //Nothing to draw.. normally
        {
            _fpsCounter.Update();
        }

        public int getFPS()
        {
            return _fpsCounter.getFrames();
        }
    }
}
