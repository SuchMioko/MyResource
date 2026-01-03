using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NNUfoInvasion;

public class UFOIA
{
	public bool IsDead = false;
	public Prop Ufoia = null;
	public Prop UfoiaBase = null;
	public Camera UfoiaFireT;
	public Camera UfoiaMoveT;
	public int SpeedMode = 0;
	public float SpeedUFO = 50f;
	public static int UFOCount;

	public Vector3 UFOIALastpos;
	public List<Prop> Lasers = new List<Prop>();
	public List<int> FireFXL = new List<int>();
	public List<Vector3> Dirs = new List<Vector3>();

	public Prop Cannon;
	public int firetime = 0;
	public bool CannonShoot = false;
	public int CannonFXTimer = 3000;
	public Vector3 CannonStart;

	public int beamfx;
	public bool abduction = false;

	public Ped[] Targets;
	private int GetTargetsT = 0;

	public Vector3 GoToPosition;
	public int GoToOK = -1;

	public float UFOIASpeed = 50f;
	public float UFOIAMaxSpeed = 200f;

	public Vector3 ChasePos;
	public Entity ChaseTarget;
	public int UFOIAMaxDistance = 150;

	public int ChaseOK = -1;
	public bool AttackOK;
	public Entity AttackTarget;
	public int UFOIAWaitFire = 1000;
	public float AttackDistance = 1000f;

	public bool LeaveOK = false;
	public Vector3 LeavePos;
	public int Health = 100;

	public int expdtimer;
	public int DesCannonTimer;
	public int AbductionTimer;

        public void CreateUFOIA()
        {
            Random rnd = new Random();
            Vector3 offsetInWorldCoords = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0f, 0f, 200f));
            Vector3 spawnPos = offsetInWorldCoords.Around(rnd.Next(10, 300));

            int num  = Game.GenerateHash("p_spinning_anus_s");
            int num2 = Game.GenerateHash("prop_ld_test_01");

            Function.Call(Hash.SET_MODEL_AS_NO_LONGER_NEEDED, num);

            while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, num))
            {
                Function.Call(Hash.REQUEST_MODEL, num);
                Script.Wait(0);
            }

            Vector3 pos = val;
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
            }
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

            Function.Call(
                Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
                "scr_alien_disintegrate",
                pos.X, pos.Y, pos.Z,
                0f, 0f, 0f,
                30f,
                false, false, false
            );

            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 255f)
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

            Function.Call(
                Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
                "scr_alien_disintegrate",
                pos.X, pos.Y, pos.Z,
                0f, 0f, 0f,
                30f,
                false, false, false
            );

            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 255f)
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

            Function.Call(
                Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
                "scr_alien_disintegrate",
                pos.X, pos.Y, pos.Z,
                0f, 0f, 0f,
                30f,
                false, false, false
            );

            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 255f)

            UfoiaBase = Function.Call<Prop>(
                Hash.CREATE_OBJECT_NO_OFFSET,
                num2,        // model hash
                val.X,       // 
                val.Y,       // Y
                val.Z,       // Z
                false,       // networked
                false,       // dynamic
                false        // place on ground
            );

            UfoiaBase.IsVisible = false;
            UfoiaBase.FreezePosition = true;
            Function.Call(
                Hash.SET_ENTITY_COLLISION,
                UfoiaBase,
                false,   // collision OFF
                false    // physics TIDAK dipertahankan
            );

            // Buat UFO utama
            
            UfoiaBase = Function.Call<Prop>(
                Hash.CREATE_OBJECT_NO_OFFSET,
                num,        // model hash
                val.X,       //
                val.Y,       // Y
                val.Z,       // Z
                false,       // networked
                false,       // dynamic
                false        // place on ground
            );

            Ufoia.AddBlip();

            // Kamera tembakan
            UfoiaFireT = World.CreateCamera(
                UfoiaBase.GetOffsetInWorldCoords(new Vector3(0f, 0f, 12f)),
                UfoiaBase.Rotation,
                80f
            );
            UfoiaFireT.AttachTo(UfoiaBase, new Vector3(0f, 0f, -15f));

            UfoiaMoveT = World.CreateCamera(
                UfoiaBase.GetOffsetInWorldCoords(new Vector3(0f, 0f, 12f)),
                UfoiaBase.Rotation,
                80f
            );
            UfoiaMoveT.AttachTo(UfoiaBase, new Vector3(0f, 0f, -15f));

            // Timer
            DesCannonTimer = Game.GameTime + 10000;
            AbductionTimer = Game.GameTime + 10000;

            // Set entity proof (tahan damage / ledakan)
            Function.Call(
                Hash.SET_ENTITY_MAX_SPEED,
                Ufoia,
                1000f
            );
        }

        public void DestroyUFOIA()
        {
            Vector3 pos = Ufoia.Position;

            // Pastikan particle asset sudah diload
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
            }
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

            Function.Call(
                Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
                "scr_alien_disintegrate",
                pos.X, pos.Y, pos.Z,
                0f, 0f, 0f,
                30f,
                false, false, false
            );

            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 255f)
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

            Function.Call(
                Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
                "scr_alien_disintegrate",
                pos.X, pos.Y, pos.Z,
                0f, 0f, 0f,
                30f,
                false, false, false
            );

            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 255f)
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

            Function.Call(
                Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
                "scr_alien_disintegrate",
                pos.X, pos.Y, pos.Z,
                0f, 0f, 0f,
                30f,
                false, false, false
            );

            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 255f)

            abduction = false;
            Health = 100;

            Ufoia.CurrentBlip.Remove();
            Ufoia.Delete();
            UfoiaBase.Delete();

            UfoiaFireT.Destroy();
            UfoiaMoveT.Destroy();
            UFOCount--;
        }

        public bool Exist()
        {
            try
            {
                if (((Entity)Ufoia).Exists() && ((Entity)UfoiaBase).Exists())
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Main()
        {
            if (!IsDead)
            {
                Ufoia.Rotation = new Vector3(
                    0f,
                    -2f,
                    Ufoia.Rotation.Z + 4f
                );

                UfoiaBase.Position = Ufoia.Position;
                UFOIALastpos = Ufoia.Position;
                UfoiaBase.Heading = UfoiaFireT.Rotation.Z;

                Function.Call(
                    Hash.SET_ENTITY_INVINCIBLE,
                    Ufoia,
                    false
                );

                // Input & behaviour
                UFOIAControls();
                Specials();
                IATasks();
            }

            // Update statistik (health, timer, state, dll)
            UFOIAstats();
        }

        public void IATasks()
        {
            Vector3 position;

            if (GoToOK == 0)
            {
                UfoiaMoveT.PointAt(GoToPosition);

                position = Ufoia.Position;
                if (position.DistanceTo(GoToPosition) <= 200f)
                {
                    Ufoia.Velocity = RotToDir(UfoiaMoveT.Rotation) * UFOIASpeed;
                }
                else
                {
                    Ufoia.Velocity = RotToDir(UfoiaMoveT.Rotation) * UFOIAMaxSpeed;
                }
            }

            position = Ufoia.Position;
            if (GoToOK == 0 && position.DistanceTo(GoToPosition) <= 20f)
                GoToOK = -1;

            if (ChaseOK == 0)
            {
                Random random = new Random();

                ChasePos = new Vector3(
                    ChasePos.X,
                    ChasePos.Y,
                    World.GetGroundHeight(ChasePos) + (float)random.Next(80, 100)
                );

                UfoiaMoveT.PointAt(ChasePos);
                Ufoia.Velocity = RotToDir(UfoiaMoveT.Rotation) * 15f;
            }

            position = Ufoia.Position;
            if (ChaseOK == 0 && position.DistanceTo(ChaseTarget.Position) > 500f)
            {
                Teleport(ChasePos);

                Random random2 = new Random();
                position = ChaseTarget.Position;
                
                ChasePos = position.Around(
                    (float)random2.Next(100, UFOIAMaxDistance)
                );
            }

            position = Ufoia.Position;
            if (ChaseOK == 0 && position.DistanceTo(ChasePos) <= 30f)
            {
                ChaseOK = 0;

                Random random = new Random();
                position = ChaseTarget.Position;
                ChasePos = position.Around(
                    (float)random.Next(10, UFOIAMaxDistance)
                );
            }

            if (AttackOK)
            {
                AttackTarget = Game.Player.Character;
                UfoiaFireT.PointAt(AttackTarget);

                RaycastResult hit = World.Raycast(
                    UfoiaFireT.Position,
                    AttackTarget.Position,
                    IntersectOptions.Everything
                );

                if (Game.GameTime > firetime && hit.HitEntity == AttackTarget)
                {
                    position = Ufoia.Position;
                    if (position.DistanceTo(AttackTarget.Position) <= AttackDistance)
                    {
                        firetime = Game.GameTime + UFOIAWaitFire;

                        Lasers.Add(World.CreateProp(new Model("w_lr_homing_rocket"),
                            UfoiaFireT.Position,
                            UfoiaFireT.Rotation,
                            false,
                            false
                        );

                        Lasers[Lasers.Count - 1]).IsVisible = false;
                        Dirs.Add(RotToDir(UfoiaFireT.Rotation));

                        if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
                        {
                            Function.Call(
                                Hash.REQUEST_NAMED_PTFX_ASSET,
                                "scr_rcbarry1");
                        }

                        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

                        int fx = Function.Call<int>(
                            Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY,
                            "scr_alien_impact",
                            laser,
                            0f, 0f, 0f,
                            0f, 0f, 0f,
                            4f,
                            false, false, false
                        );

                        Function.Call(
                            Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA,
                            fx,
                            255f
                        );

                        FireFXL.Add(fx);
                    }
                }
            }

            if (LeaveOK)
            {
                UfoiaMoveT.PointAt(LeavePos);

                position = Ufoia.Position;
                if (position.DistanceTo(GoToPosition) <= 200f)
                {
                    Ufoia.Velocity = RotToDir(UfoiaMoveT.Rotation) * UFOIASpeed;
                }
                else
                {
                    Ufoia.Velocity = RotToDir(UfoiaMoveT.Rotation) * UFOIAMaxSpeed;
                }
            }

            position = Ufoia.Position;
            if (LeaveOK && position.DistanceTo(LeavePos) <= 20f)
            {
                DestroyUFOIA();
                LeaveOK = false;
            }
        }
    }

    public void Teleport(Vector3 targetPos)
    {
        int num = 5        
        if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
        }
        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");
        Function.Call(
            Hash._START_PARTICLE_FX_ON_ENTITY,  // misal alias yang tepat
            "scr_alien_disintegrate",
            Ufoia,
            0f, 0f, 0f,     // offset X,Y,Z
            0f, 0f, 0f,     // rotation pitch, roll, yaw
            30f,            // scale / radius
            false, false, false // flags
        );

        Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 1f);
        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

        Function.Call(
            Hash._START_PARTICLE_FX_ON_ENTITY,  // misal alias yang tepat
            "scr_alien_disintegrate",
            Ufoia,
            0f, 0f, 0f,     // offset X,Y,Z
            0f, 0f, 0f,     // rotation pitch, roll, yaw
            30f,            // scale / radius
            false, false, false // flags
        );

        Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 1f);
        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

        Function.Call(
            Hash._START_PARTICLE_FX_ON_ENTITY,  // misal alias yang tepat
            "scr_alien_disintegrate",
            Ufoia,
            0f, 0f, 0f,     // offset X,Y,Z
            0f, 0f, 0f,     // rotation pitch, roll, yaw
            30f,            // scale / radius
            false, false, false // flags
        );

        Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 1f);

        while (num > 0)
        {
            num--;
            Script.Yield();
        }

        Function.Call(Hash.REMOVE_PARTICLE_FX_FROM_ENTITY, Ufoia, 0, false);

        Ufoia.Position = Position;
        int num2 = 5;
        if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
        }
        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

        Function.Call(
            Hash._START_PARTICLE_FX_ON_ENTITY,  // misal alias yang tepat
            "scr_alien_disintegrate",
            Ufoia,
            0f, 0f, 0f,     // offset X,Y,Z
            0f, 0f, 0f,     // rotation pitch, roll, yaw
            30f,            // scale / radius
            false, false, false // flags
        );

        Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 1f);
        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

        Function.Call(
            Hash._START_PARTICLE_FX_ON_ENTITY,  // misal alias yang tepat
            "scr_alien_disintegrate",
            Ufoia,
            0f, 0f, 0f,     // offset X,Y,Z
            0f, 0f, 0f,     // rotation pitch, roll, yaw
            30f,            // scale / radius
            false, false, false // flags
        );

        Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 1f);
        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

        Function.Call(
            Hash._START_PARTICLE_FX_ON_ENTITY,  // misal alias yang tepat
            "scr_alien_disintegrate",
            Ufoia,
            0f, 0f, 0f,     // offset X,Y,Z
            0f, 0f, 0f,     // rotation pitch, roll, yaw
            30f,            // scale / radius
            false, false, false // flags
        );

        Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 1f);
        while (num2 > 0)
        {
            num2--;
        }
        Function.Call(Hash.REMOVE_PARTICLE_FX_FROM_ENTITY, Ufoia, 0, false);
    }

    public void Specials()
    {
        Vector3 position;

        if (abduction && Ufoia.HeightAboveGround <= 250f)
        {
            Vector3 offsetInWorldCoords = Ufoia.GetOffsetInWorldCoords(new Vector3(0f, 0f, -5f));

            Vector3 groundPos = new Vector3(
                Ufoia.Position.X,
                Ufoia.Position.Y,
                World.GetGroundHeight(Ufoia.Position)
            );

            Vector3 direction = groundPos - beamOrigin;
            direction.Normalize();

            // Red beam light
            Function.Call(
                Hash.DRAW_LIGHT_WITH_RANGE,
                offsetInWorldCoords.X,
                offsetInWorldCoords.Y,
                offsetInWorldCoords.Z - 5f,
                255,   // Red
                0,     // Green
                0,     // Blue
                30f,   // Range (jarak cahaya)
                70f    // Intensity (kekuatan cahaya)
            );

            Function.Call(
                Hash.DRAW_SPOT_LIGHT,
                offsetInWorldCoords.X,
                offsetInWorldCoords.Y,
                offsetInWorldCoords.Z,
                direction.X,
                direction.Y,
                direction.Z,
                255, 0, 0,
                Ufoia.HeightAboveGround,
                500f,
                0f,
                50f,
                2f
            );

            Function.Call(Hash.SET_DRAW_ORIGIN, true);
            Ped[] nearbyPeds = World.GetNearbyPeds(groundPos, 30f);
            Function.Call(Hash.SET_DRAW_ORIGIN, true);

            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(groundPos, 30f);
            Ped[] array = nearbyPeds;

            foreach (Ped ped in array)
            {
                position = ped.Position;
                if (position.DistanceTo(groundPos) <= 30f)
                {
                    Function.Call(
                        Hash.SET_ENTITY_PROOFS,
                        ped,
                        true, true, true, true, true, true
                    );
            
                    Vector3 worldUp = Vector3.WorldUp;
                    position = Ufoia.Position;
                    ped.ApplyForce(Vector3.WorldUp * position.DistanceTo(groundPos));
                }
            }

            Vehicle[] array2 = nearbyVehicles;
            foreach (Vehicle veh in array2)
            {
                position = veh.Position;
                if (position.DistanceTo(groundPos) <= 30f)
                {
                     Vector3 worldUp2 = Vector3.WorldUp;
                     position = Ufoia.Position;
                     veh.ApplyForce(worldUp2 * position.DistanceTo(groundPos));
                }
            }
            nearbyPeds = null;
            nearbyVehicles = null;
        }
        if (!CannonShoot)
            return;
            

        Vector3 impact = new Vector3(
            CannonStart.X,
            CannonStart.Y,
            World.GetGroundHeight(CannonStart) - 1f
        );

        Cannon.Position = Vector3.Lerp(Cannon.Position, impact, 0.3f);
        position = Cannon.Position;

        if (position.DistanceTo(impact) > 3f)
            return;

        // Explosion FX
        if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_agencyheistb"))
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_agencyheistb");

        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_agencyheistb");

        Function.Call(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
            "scr_agency3b_heli_expl",
            impact.X,
            impact.Y,
            impact.Z,
            0f, 0f, 0f,
            6f,
            false,
            false,
            false
        );

        if (Function.Call<bool>(Hash.DOES_PARTICLE_FX_LOOPED_EXIST, beamfx))
        {
            Function.Call(Hash.STOP_PARTICLE_FX_LOOPED, beamfx, false);
        }

        Cannon.Delete();
        Function.Call(Hash.ADD_EXPLOSION,
            impact.X,
            impact.Y,
            impact.Z,
            ExplosionType.Rocket,
            30f,
            true,
            true,
            0f
        );

        Function.Call(Hash.SET_DRAW_ORIGIN, true);
        Ped[] nearbyPeds2 = World.GetNearbyPeds(impact, 50f);
        Function.Call(Hash.SET_DRAW_ORIGIN, true);
        Vehicle[] nearbyVehicles2 = World.GetNearbyVehicles(impact, 50f);
        
        Ped[] array3 = nearbyPeds2;
        foreach (Ped ped2 in array3)
        {

            Function.Call(
                Hash.SET_ENTITY_PROOFS,
                ped2,
                true, true, true, true, true, true
            );

            ped2.ApplyForce(Vector3.WorldUp * 20f + Vector3.Reflect(impact, ped.Position));

            if (ped2 != Game.Player.Character)
                ped2.Kill();
        }

        Vehicle[] array4 = nearbyVehicles2;
        foreach (Vehicle veh in array4)
        {
            veh.ApplyForce(Vector3.WorldUp * 20f + Vector3.Reflect(impact, veh.Position));
            veh.Explode();
        }

        CannonShoot = false;
    }

    public void ExplodeProjectiles()
    {
        if (Lasers.Count == 0)
            return;

        for (int i = 0; i < Lasers.Count; i++)
        {
            Vector3 position = Lasers[i].Position;
            World.AddExplosion(position2, ExplosionType.Rocket, 80f, 0f);

            if (Function.Call<bool>(Hash.DOES_PARTICLE_FX_LOOPED_EXIST, FireFXL[i]))
            {
                Function.Call(Hash.STOP_PARTICLE_FX_LOOPED, FireFXL[i], false);
            }
            else
            {
                Function.Call(Hash.REMOVE_PARTICLE_FX, FireFXL[i], false);
            }

            // Cleanup
            FireFXL.RemoveAt(i);
            Dirs.RemoveAt(i);
            Lasers[i].Delete();
            Lasers.RemoveAt(i);
        }
    }

    public void UFOIAshoot()
    {
        if (Lasers.Count == 0)
            return;

        // loop mundur biar aman saat RemoveAt
        for (int i = 0; i < Lasers.Count; i++)
        {
            Prop laser = Lasers[i];
            laser.Velocity = laser.Velocity + Dirs[i] * 2000f;

            RaycastResult hit = World.Raycast(
                laser[i].Position,
                Dirs[i] * 1000f,
                IntersectOptions.Everything
            );

            Vector3 position = Lasers[i].Position;
            if (position.DistanceTo(hit.HitCoords) < 3f)
            {
                Vector3 position2 = Lasers[i].Position;
                World.AddExplosion(position2, ExplosionType.Rocket, 80f, 0f);
                if (Function.Call<bool>(Hash.DOES_PARTICLE_FX_LOOPED_EXIST, FireFXL[i]))
                {
                    Function.Call(Hash.STOP_PARTICLE_FX_LOOPED, FireFXL[i], false);
                }
                else
                {
                    Function.Call(Hash.REMOVE_PARTICLE_FX, FireFXL[i], false);
                }

                FireFXL.RemoveAt(i);
                Dirs.RemoveAt(i);
                Lasers[i].Delete();
                Lasers.RemoveAt(i);
                continue;
            }

            position = Lasers[i].Position;
            if (position.DistanceTo(Ufoia.Position) > 400f)
            {
                Vector3 position3 = Lasers[i].Position;
                World.AddExplosion(position3, ExplosionType.Rocket, 80f, 0f);
                if (Function.Call<bool>(Hash.DOES_PARTICLE_FX_LOOPED_EXIST, FireFXL[i]))
                {
                    Function.Call(Hash.STOP_PARTICLE_FX_LOOPED, FireFXL[i], false);
                }
                else
                {
                    Function.Call(Hash.REMOVE_PARTICLE_FX, FireFXL[i], false);
                }

                
                FireFXL.RemoveAt(i);
                Dirs.RemoveAt(i);
                Lasers[i].Delete();
                Lasers.RemoveAt(i);
            }
        }
    }

	public double GenRand(double one, double two)
	{
		Random random = new Random();
		return one + random.NextDouble() * (two - one);
	}

	public void UFOIAControls()
    {
        UfoiaFireT.AttachTo(UfoiaBase, new Vector3(0f, 0f, -18f));
        ChaseTask(Game.Player.Character);

        // Mode normal: muter & nembak ke sekitar player
        if (!abduction && !CannonShoot)
        {
            float radius = Convert.ToSingle(GenRand(0.0, 80.0));

            Vector3 playerPos = Game.Player.Character.Position;
            Vector3 targetPos = playerPos.Around(radius);
            targetPos = new Vector3(
                targetPos.X,
                targetPos.Y,
                World.GetGroundHeight(targetPos)
            );

            ShootAtTask(targetPos, 400);
        }

        // Chance nembak destructive cannon
        Random random = new Random();
        if (random.Next(0, 100) == 62 && Game.GameTime > DesCannonTimer && !abduction)
        {
            CannonFXTimer = Game.GameTime + 3000;
            DestructiveCannon();
            DesCannonTimer = Game.GameTime + 10000;
        }

        // Chance abduction toggle
        Random random2 = new Random();
        if (random2.Next(0, 150) == 46 && Game.GameTime > AbductionTimer)
        {
            if (CanAbduct && !abduction)
            {
                abduction = true;
                UFOInvasion.CanAbduct = false;
                AbductionTimer = Game.GameTime + 10000;
            }
            else if (!CanAbduct && abduction)
            {
                abduction = false;
                UFOInvasion.CanAbduct = true;
                AbductionTimer = Game.GameTime + 10000;
            }
        }
    }

    public Ped SearchTarget(Vector3 position)
    {
        Ped closestPed = World.GetClosestPed(position, 50f);
        if (closestPed != null)
            return closestPed;
        return null;
    }

    public void ShootAtTask(Vector3 target, int waitFire = 1000)
    {
        UfoiaFireT.PointAt(target);

        if (Game.GameTime > firetime)
        {
            firetime = Game.GameTime + waitFire;

            Lasers.Add(World.CreateProp(
                new Model("w_lr_homing_rocket"),
                UfoiaFireT.Position,
                UfoiaFireT.Rotation,
                false,
                false
            );

            Lasers[Lasers.Count - 1].IsVisible = false;
            Function.Call(
                Hash.SET_ENTITY_MAX_SPEED,
                Lasers[Lasers.Count - 1],
                500f
            );
            Dirs.Add(RotToDir(UfoiaFireT.Rotation));
    
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
            }

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

            int fx = Function.Call<int>(
                Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY,
                "scr_alien_impact",
                Lasers[Lasers.Count - 1],
                0f, 0f, 0f,      // offset posisi
                0f, 0f, 0f,      // rotasi
                4f,              // scale
                false,
                false,
                false
            );

            Function.Call(
                Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA,
                fx,
                255f
            );

            FireFXL.Add(fx);
        }
    }

	public void AttackTask(Entity Target, int WaitFire = 1000, float Distance = 1000f)
	{
		if (!AttackOK)
		{
			AttackTarget = Target;
			UFOIAWaitFire = WaitFire;
			AttackDistance = Distance;
			AttackOK = true;
		}
	}

	public void StopAttackTask()
	{
		AttackOK = false;
	}

	public void ChaseTask(Entity Target, int MaxDistance = 200)
	{
		if (ChaseOK == -1)
		{
			Random random = new Random();
			UFOIAMaxDistance = MaxDistance;
			Vector3 position = Target.Position;
			ChasePos = position.Around((float)random.Next(10, MaxDistance));
			ChaseTarget = Target;
			ChaseOK = 0;
		}
	}

	public void StopChaseTask()
	{
		ChaseOK = -1;
	}

	public void GotoTask(Vector3 Position, float Speed = 50f, float MaxSpeed = 200f)
	{
		if (GoToOK == -1)
		{
			GoToPosition = Position;
			UFOIASpeed = Speed;
			UFOIAMaxSpeed = MaxSpeed;
			GoToOK = 0;
		}
	}

	public void LeaveTask()
	{
		if (!LeaveOK)
		{
			Vector3 position = Game.Player.Character.Position;
			Vector3 val = position.Around(500f);
			Vector3 leavePos = default(Vector3);
			leavePos = new Vector3(val.X, val.Y, val.Z + 200f);
			LeavePos = leavePos;
			UFOIASpeed = 50f;
			UFOIAMaxSpeed = 200f;
			LeaveOK = true;
		}
	}

	public void UFOIAstats()
    {
        if (Health > 0)
        {
            Vector3 position = Ufoia.Position;

            if (Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE,
                32,                 // Explosion type: Alien / Energy
                position.X,
                position.Y,
                position.Z,
                35f
            ) ||
            Function.Call<bool>(
                Hash.IS_EXPLOSION_IN_SPHERE,
                4,                  // Explosion type: Rocket
                position.X,
                position.Y,
                position.Z,
                35f
            ) ||
            Function.Call<bool>(
                Hash.IS_EXPLOSION_IN_SPHERE,
                8,                  // Explosion type: Tank shell
                position.X,
                position.Y,
                position.Z,
                35f
            ) ||
            Function.Call<bool>(
                Hash.IS_EXPLOSION_IN_SPHERE,
                1,                  // Explosion type: Grenade
                position.X,
                position.Y,
                position.Z,
                35f
            ) ||
            Function.Call<bool>(
                Hash.IS_EXPLOSION_IN_SPHERE,
                33,                 // Explosion type: EMP / Sci-fi
                position.X,
                position.Y,
                position.Z,
                35f
            ) ||
            Function.Call<bool>(
                Hash.IS_EXPLOSION_IN_SPHERE,
                18,                 // Explosion type: Plane explosion
                position.X,
                position.Y,
                position.Z,
                35f
            ))
        {
            Health--;
        }

        if (Health <= 0 && !IsDead)
        {
            IsDead = true;
            expdtimer = Game.GameTime + 200;

            Function.Call(
                Hash.SET_ENTITY_INVINCIBLE,
                Ufoia,
                true
            );

            Ufoia.IsInvincible = true;
            Ufoia.FreezePosition = false;
        }

        if (!IsDead)
            return;

        if (Game.GameTime > expdtimer)
        {
            float num = Convert.ToSingle(GenRand(-1.0, 1.0));
            float num2 = Convert.ToSingle(GenRand(-1.0, 1.0));
            float num3 = Convert.ToSingle(GenRand(-1.0, 1.0));
            float num4 = Convert.ToSingle(GenRand(-1.0, 1.0));

            Vector3 offsetInWorldCoords = Ufoia.GetOffsetInWorldCoords(new Vector3(num, num2, 0.5f));
            Vector3 offsetInWorldCoords2 = Ufoia.GetOffsetInWorldCoords(new Vector3(num3, num4, 0.5f));

            World.AddExplosion(offsetInWorldCoords, ExplosionType.GasCanister, 20f, 1f);
            World.AddExplosion(offsetInWorldCoords2, ExplosionType.GasCanister, 20f, 0.5f);

            Ufoia.ApplyForce(Vector3.WorldDown * 20f);
            expdtimer = Game.GameTime + 500;
        }

        Vector3 groundPos = default(Vector3);

        groundPos = new Vector3(
            Ufoia.Position.X,
            Ufoia.Position.Y,
            World.GetGroundHeight(Ufoia.Position)
        );

        Vector3 position2 = Ufoia.Position;
        if (position2.DistanceTo(groundPos) > 80f || !Ufoia.HasCollided)
            return;

        Vector3 position3 = Ufoia.Position;
        Vector3 offsetInWorldCoords3 = Ufoia.GetOffsetInWorldCoords(0f, 0f, -10f);
        
        Function.Call(
            Hash.ADD_EXPLOSION,
            position3.X,
            position3.Y,
            position3.Z,
            ExplosionType.Rocket,
            100f,
            true,
            true,
            2f
        );

        if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_agencyheistb"))
        {
            Function.Call(
                Hash.REQUEST_NAMED_PTFX_ASSET,
                "scr_agencyheistb"
            );
        }

        Function.Call(
            Hash.USE_PARTICLE_FX_ASSET,
            "scr_agencyheistb"
        );

        Function.Call(
            Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
            "scr_agency3b_heli_expl",   // nama particle
            offsetInWorldCoords3.X,
            offsetInWorldCoords3.Y,
            offsetInWorldCoords3.Z,
            0f, 0f, 0f,                // rotasi
            5f,                        // scale
            false,                     // axisX
            false,                     // axisY
            false                      // axisZ
        );

        Function.Call(
            Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA,
            255f
        );

        Ped[] nearbyPeds = World.GetNearbyPeds(((Entity)Ufoia).Position, 80f);
        Vehicle[] nearbyVehicles = World.GetNearbyVehicles(((Entity)Ufoia).Position, 80f);
        Ped[] array = nearbyPeds;

        foreach (Ped ped in array))
        {
            Function.Call(
                Hash.SET_ENTITY_PROOFS,
                ped,
                true, true, true, true, true, true
            );
            ped.ApplyForce(
                Vector3.WorldUp * 20f +
                Vector3.Reflect(ped.Position, Ufoia.Position) * 5f
            );

            if (ped != Game.Player.Character)
                ped.Kill();
        }

        Vehicle[] array2 = nearbyVehicles;
        foreach (Vehicle veh in array2)
        {
            veh.ApplyForce(
                Vector3.WorldUp * 20f +
                Vector3.Reflect(veh.Position, Ufoia.Position)
            );

            veh.Explode();
        }

        DestroyUFOIA();
    }

    public void DestructiveCannon()
    {
        CannonStart = Ufoia.GetOffsetInWorldCoords(new Vector3(0f, 0f, -6f));
        Ufoia.Velocity = Vector3.Zero;

        if (!World.HasNamedPtfxAssetLoaded("scr_rcbarry1"))
            World.RequestNamedPtfxAsset("scr_rcbarry1");

        World.UseParticleFxAsset("scr_rcbarry1");

        int chargeFx1 = World.StartParticleEffectNonLooped(
            "scr_alien_charging",
            CannonStart,
            Vector3.Zero,
            10f
        );

        int chargeFx2 = World.StartParticleEffectNonLooped(
            "scr_alien_charging",
            CannonStart,
            Vector3.Zero,
            10f
        );

        int chargeFx3 = World.StartParticleEffectNonLooped(
            "scr_alien_charging",
            CannonStart,
            Vector3.Zero,
            10f
        );

        World.SetParticleFxNonLoopedAlpha(chargeFx1, 255f);
        World.SetParticleFxNonLoopedAlpha(chargeFx2, 255f);
        World.SetParticleFxNonLoopedAlpha(chargeFx3, 255f);

        if (Game.GameTime < CannonFXTimer)
            Script.Yield();

        World.StopParticleEffect(chargeFx1, false);
        World.StopParticleEffect(chargeFx2, false);
        World.StopParticleEffect(chargeFx3, false);

        Cannon = World.CreateProp(
            new Model("w_lr_homing_rocket"),
            CannonStart,
            new Vector3(-90f, 0f, 0f),
            false,
            false
        );

        Cannon.IsVisible = false;

        if (!World.HasNamedPtfxAssetLoaded("scr_rcbarry1"))
            World.RequestNamedPtfxAsset("scr_rcbarry1");

        World.UseParticleFxAsset("scr_rcbarry1");

        beamfx = World.StartParticleEffectLoopedOnEntity(
            "scr_alien_impact",
            Cannon,
            Vector3.Zero,
            Vector3.Zero,
            8f
        );

        World.SetParticleFxLoopedAlpha(beamfx, 255f);

        CannonShoot = true;
    }
}
