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
            Ped player = Game.Player.Character;

            // Posisi di atas player (200 unit ke atas)
            Vector3 basePos = player.GetOffsetInWorldCoords(new Vector3(0f, 0f, 200f));

            // Acak posisi di sekitar titik itu
            Vector3 spawnPos = basePos.Around(rnd.Next(10, 300));

            // Load model UFO dan base dummy
            Model ufoModel = new Model("p_spinning_anus_s");   // UFO
            Model baseModel = new Model("prop_ld_test_01");    // base tak terlihat

            ufoModel.Request();
            baseModel.Request();

            while (!ufoModel.IsLoaded || !baseModel.IsLoaded)
            {
                Script.Wait(0);
            }

            // Load particle effect
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
                while (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
                    Script.Wait(0);
            }

            // Mainkan efek alien disintegrate beberapa kali
            for (int i = 0; i < 3; i++)
            {
                Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");
                Function.Call(
                    Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
                    "scr_alien_disintegrate",
                    spawnPos.X, spawnPos.Y, spawnPos.Z,
                    0f, 0f, 0f,
                    30f,
                    false, false, false
                );

                Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 255f);
            }

            // Buat base UFO (tak terlihat, beku)
            UfoiaBase = World.CreateProp(
                baseModel,
                spawnPos,
                false,
                false
            );

            UfoiaBase.IsVisible = false;
            UfoiaBase.FreezePosition = true;
            UfoiaBase.IsCollisionEnabled = false;

            // Buat UFO utama
            Ufoia = World.CreateProp(
                ufoModel,
                spawnPos,
                false,
                true
            );

            Ufoia.AddBlip();

            // Kamera tembakan
            UfoiaFireT = World.CreateCamera(
                UfoiaBase.GetOffsetInWorldCoords(new Vector3(0f, 0f, 12f)),
                UfoiaBase.Rotation,
                80f
            );
            UfoiaFireT.AttachTo(UfoiaBase, new Vector3(0f, 0f, -15f));

            // Kamera gerak
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
            Function.Call(Hash.SET_ENTITY_PROOFS, Ufoia, true);
        }

        public void DestroyUFOIA()
        {
            if (Ufoia == null || !Ufoia.Exists())
                return;

            Vector3 pos = Ufoia.Position;

            // Pastikan particle asset sudah diload
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
                while (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
                    Script.Wait(0);
            }

            // Efek alien disintegrate (dramatis = 3x)
            for (int i = 0; i < 3; i++)
            {
                Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

                Function.Call(
                    Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD,
                    "scr_alien_disintegrate",
                    pos.X, pos.Y, pos.Z,
                    0f, 0f, 0f,
                    30f,
                    false, false, false
                );

                Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 255f);
            }

            // Reset state
            abduction = false;
            Health = 100;

            // Bersihkan blip
            if (Ufoia.CurrentBlip != null)
                Ufoia.CurrentBlip.Remove();

            // Hapus entity
            Ufoia.Delete();

            if (UfoiaBase != null && UfoiaBase.Exists())
                UfoiaBase.Delete();

            // Hapus kamera
            if (UfoiaFireT != null)
                UfoiaFireT.Destroy();

            if (UfoiaMoveT != null)
                UfoiaMoveT.Destroy();

            // Kurangi counter invasi
            UFOInvasion.UFOCount--;
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
                // Rotasi UFO (muter + pitch dikit biar kelihatan hidup)
                Ufoia.Rotation = new Vector3(
                    0f,
                    -2f,
                    Ufoia.Rotation.Z + 4f
                );

                // Sinkronkan base invisible dengan UFO
                UfoiaBase.Position = Ufoia.Position;

                // Simpan posisi terakhir
                UFOIALastpos = Ufoia.Position;

                // Heading base mengikuti kamera tembak
                UfoiaBase.Heading = UfoiaFireT.Rotation.Z;

                // Disable collision / physics tertentu (anti nyangkut)
                Function.Call(
                    Hash.SET_ENTITY_COLLISION,
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
            Vector3 pos = Ufoia.Position;

            if (GoToOK == 0)
            {
                UfoiaMoveT.PointAt(GoToPosition);

                float dist = pos.DistanceTo(GoToPosition);
                float speed = (dist <= 200f) ? UFOIASpeed : UFOIAMaxSpeed;

                Ufoia.Velocity = UFOInvasion.RotToDir(UfoiaMoveT.Rotation) * speed;
            }

            if (GoToOK == 0 && pos.DistanceTo(GoToPosition) <= 20f)
            {
                GoToOK = -1; // sampai tujuan
            }

            if (ChaseOK == 0)
            {
                Random rnd = new Random();

                ChasePos = new Vector3(
                    ChasePos.X,
                    ChasePos.Y,
                    World.GetGroundHeight(ChasePos) + rnd.Next(80, 100)
                );

                UfoiaMoveT.PointAt(ChasePos);
                Ufoia.Velocity = UFOInvasion.RotToDir(UfoiaMoveT.Rotation) * 15f;
            }

            if (ChaseOK == 0 && pos.DistanceTo(ChaseTarget.Position) > 500f)
            {
                Teleport(ChasePos);

                Random rnd = new Random();
                ChasePos = ChaseTarget.Position.Around(
                    rnd.Next(100, UFOIAMaxDistance)
                );
            }

            if (ChaseOK == 0 && pos.DistanceTo(ChasePos) <= 30f)
            {
                Random rnd = new Random();
                ChasePos = ChaseTarget.Position.Around(
                    rnd.Next(10, UFOIAMaxDistance)
                );
            }

            if (AttackOK)
            {
                UfoiaFireT.PointAt(AttackTarget);

                RaycastResult hit = World.Raycast(
                    UfoiaFireT.Position,
                    AttackTarget.Position,
                    IntersectOptions.Everything
                );

                if (Game.GameTime > firetime &&
                    hit.HitEntity == AttackTarget &&
                    pos.DistanceTo(AttackTarget.Position) <= AttackDistance)
                {
                    firetime = Game.GameTime + UFOIAWaitFire;

                    // Laser invisible
                    Prop laser = World.CreateProp(
                        new Model("w_lr_homing_rocket"),
                        UfoiaFireT.Position,
                        UfoiaFireT.Rotation,
                        false,
                        false
                    );

                    laser.IsVisible = false;

                    Lasers.Add(laser);
                    Dirs.Add(UFOInvasion.RotToDir(UfoiaFireT.Rotation));

                    // Particle FX
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

        if (LeaveOK)
        {
            UfoiaMoveT.PointAt(LeavePos);

            float dist = pos.DistanceTo(LeavePos);
            float speed = (dist <= 200f) ? UFOIASpeed : UFOIAMaxSpeed;

            Ufoia.Velocity = UFOInvasion.RotToDir(UfoiaMoveT.Rotation) * speed;
        }

        if (LeaveOK && pos.DistanceTo(LeavePos) <= 20f)
        {
            DestroyUFOIA();
            LeaveOK = false;
        }
    }

    public void Teleport(Vector3 targetPos)
    {
        // Pastikan particle asset tersedia
        if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
            while (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
                Script.Yield();
        }

        // === DISINTEGRATE (sebelum teleport) ===
        PlayAlienDisintegrateFX(Ufoia);

        // Sedikit jeda biar efek kelihatan
        for (int i = 0; i < 5; i++)
            Script.Yield();

        // Bikin UFO "menghilang"
        Function.Call(Hash.SET_ENTITY_ALPHA, Ufoia, 0, false);

        // Teleport aktual
        Ufoia.Position = targetPos;

        // === REINTEGRATE (setelah teleport) ===
        PlayAlienDisintegrateFX(Ufoia);

        // Jeda kecil lagi
        for (int i = 0; i < 5; i++)
            Script.Yield();

        // UFO muncul kembali
        Function.Call(Hash.SET_ENTITY_ALPHA, Ufoia, 255, false);
    }

    private void PlayAlienDisintegrateFX(Entity ent)
    {
        for (int i = 0; i < 3; i++)
        {
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

            Function.Call(
                Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY,
                "scr_alien_disintegrate",
                ent,
                0f, 0f, 0f,
                0f, 0f, 0f,
                30f,
                false, false, false
            );

            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, 1f);
        }
    }

    public void Specials()
    {
        Vector3 position;

        if (abduction && Ufoia.HeightAboveGround <= 250f)
        {
            Vector3 beamOrigin = Ufoia.GetOffsetInWorldCoords(new Vector3(0f, 0f, -5f));
            Vector3 groundPos = new Vector3(
                Ufoia.Position.X,
                Ufoia.Position.Y,
                World.GetGroundHeight(Ufoia.Position)
            );

            Vector3 direction = groundPos - beamOrigin;
            direction.Normalize();

            // Red beam light
            Function.Call(Hash.DRAW_SPOT_LIGHT,
                beamOrigin.X,
                beamOrigin.Y,
                beamOrigin.Z - 5f,
                255, 0, 0,
                30f,
                70f
            );

            // Beam with shadow
            Function.Call(Hash.DRAW_SPOT_LIGHT_WITH_SHADOW,
                beamOrigin.X,
                beamOrigin.Y,
                beamOrigin.Z,
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

            World.RenderingCamera = World.RenderingCamera;

            Ped[] nearbyPeds = World.GetNearbyPeds(groundPos, 30f);
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(groundPos, 30f);

            foreach (Ped ped in nearbyPeds)
            {
                position = ped.Position;
                if (position.DistanceTo(groundPos) <= 30f)
                {
                    Function.Call(Hash.APPLY_DAMAGE_TO_PED,
                        ped,
                        1000,
                        false,
                        false,
                        false
                    );

                    ped.ApplyForce(Vector3.WorldUp * Ufoia.Position.DistanceTo(groundPos));
                }
            }

            foreach (Vehicle veh in nearbyVehicles)
            {
                position = veh.Position;
                if (position.DistanceTo(groundPos) <= 30f)
                {
                     veh.ApplyForce(Vector3.WorldUp * Ufoia.Position.DistanceTo(groundPos));
                }
            }
        }
        if (!CannonShoot)
            return;

        Vector3 impact = new Vector3(
            CannonStart.X,
            CannonStart.Y,
            World.GetGroundHeight(CannonStart) - 1f
        );

        Cannon.Position = Vector3.Lerp(Cannon.Position, impact, 0.3f);

        if (Cannon.Position.DistanceTo(impact) > 3f)
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

        // Physical explosion
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

        Ped[] blastPeds = World.GetNearbyPeds(impact, 50f);
        Vehicle[] blastVehicles = World.GetNearbyVehicles(impact, 50f);

        foreach (Ped ped in blastPeds)
        {
            Function.Call(Hash.APPLY_DAMAGE_TO_PED,
                ped,
                1000,
                false,
                false,
                false
            );

            ped.ApplyForce(Vector3.WorldUp * 20f + Vector3.Reflect(impact, ped.Position));

            if (ped != Game.Player.Character)
                ped.Kill();
        }

        foreach (Vehicle veh in blastVehicles)
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

        // Pakai loop mundur karena kita remove item di dalam loop
        for (int i = Lasers.Count - 1; i >= 0; i--)
        {
            Vector3 pos = Lasers[i].Position;

            // Explosion dari projectile laser
            World.AddExplosion(
                pos,
                ExplosionType.Rocket,
                80f,
                0f
            );

            // Stop particle FX (looped / non-looped)
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
        for (int i = Lasers.Count - 1; i >= 0; i--)
        {
            Prop laser = Lasers[i];

            // Dorong laser ke arah target
            laser.Velocity += Dirs[i] * 2000f;

            // Raycast ke arah laser terbang
            RaycastResult hit = World.Raycast(
                laser.Position,
                Dirs[i] * 1000f,
                IntersectOptions.Everything
            );

            // Kena sesuatu
            if (laser.Position.DistanceTo(hit.HitCoords) < 3f)
            {
                ExplodeLaser(i);
                continue;
            }

            // Terlalu jauh dari UFO
            if (laser.Position.DistanceTo(Ufoia.Position) > 400f)
            {
                ExplodeLaser(i);
            }
        }
    }

    private void ExplodeLaser(int index)
    {
        Vector3 pos = Lasers[index].Position;

        World.AddExplosion(
            pos,
            ExplosionType.Rocket,
            80f,
            0f
        );

        // Stop particle FX
        if (Function.Call<bool>(
            Hash.DOES_PARTICLE_FX_LOOPED_EXIST,
            FireFXL[index]))
        {
            Function.Call(
                Hash.STOP_PARTICLE_FX_LOOPED,
                FireFXL[index],
                false
            );
        }
        else
        {
            Function.Call(
                Hash.REMOVE_PARTICLE_FX,
                FireFXL[index],
                false
            );
        }

        FireFXL.RemoveAt(index);
        Dirs.RemoveAt(index);
        Lasers[index].Delete();
        Lasers.RemoveAt(index);
    }


	public double GenRand(double one, double two)
	{
		Random random = new Random();
		return one + random.NextDouble() * (two - one);
	}

	public void UFOIAControls()
    {
        // Pasang fire transform ke badan UFO
        UfoiaFireT.AttachTo(UfoiaBase, new Vector3(0f, 0f, -18f));

        // UFO ngejar player
        ChaseTask(Game.Player.Character);

        // Mode normal: muter & nembak ke sekitar player
        if (!abduction && !CannonShoot)
        {
            float radius = (float)GenRand(0.0, 80.0);

            Vector3 playerPos = Game.Player.Character.Position;
            Vector3 targetPos = playerPos.Around(radius);
            targetPos.Z = World.GetGroundHeight(targetPos);

            ShootAtTask(targetPos, 400);
        }

        // Chance nembak destructive cannon
        if (new Random().Next(0, 100) == 62 &&
            Game.GameTime > DesCannonTimer &&
            !abduction)
        {
            CannonFXTimer = Game.GameTime + 3000;
            DestructiveCannon();
            DesCannonTimer = Game.GameTime + 10000;
        }

        // Chance abduction toggle
        if (new Random().Next(0, 150) == 46 &&
            Game.GameTime > AbductionTimer)
        {
            if (UFOInvasion.CanAbduct && !abduction)
            {
                abduction = true;
                UFOInvasion.CanAbduct = false;
                AbductionTimer = Game.GameTime + 10000;
            }
            else if (!UFOInvasion.CanAbduct && abduction)
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
        return closestPed;
    }

    public void ShootAtTask(Vector3 target, int waitFire = 1000)
    {
        // Arahkan turret ke target
        UfoiaFireT.PointAt(target);

        // Cek cooldown tembak
        if (Game.GameTime <= firetime)
            return;

        firetime = Game.GameTime + waitFire;

        // Spawn projectile (prop roket)
        Prop laser = World.CreateProp(
            new Model("w_lr_homing_rocket"),
            UfoiaFireT.Position,
            UfoiaFireT.Rotation,
            false,
            false
        );

        laser.IsVisible = false;

        // Set lifespan projectile
        Function.Call(Hash.SET_ENTITY_LIFESPAN, laser, 500);

        Lasers.Add(laser);

        // Simpan arah tembakan
        Vector3 dir = UFOInvasion.RotToDir(UfoiaFireT.Rotation);
        Dirs.Add(dir);

        // Pastikan particle FX asset ter-load
        if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry1"))
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry1");
        }

        Function.Call(Hash.USE_PARTICLE_FX_ASSET, "scr_rcbarry1");

        // Spawn muzzle / laser FX
        int fxHandle = Function.Call<int>(
            Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY,
            "scr_alien_impact",
            laser,
            0f, 0f, 0f,
            0f, 0f, 0f,
            4f,
            false,
            false,
            false
        );

        // Set warna FX (ungu alien 👽)
        Function.Call(Hash.SET_PARTICLE_FX_LOOPED_COLOUR, fxHandle, 255f);

        FireFXL.Add(fxHandle);
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
			ChasePos = ((Vector3)(ref position)).Around((float)random.Next(10, MaxDistance));
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
			Vector3 position = ((Entity)Game.Player.Character).Position;
			Vector3 val = ((Vector3)(ref position)).Around(500f);
			Vector3 leavePos = default(Vector3);
			((Vector3)(ref leavePos))._002Ector(val.X, val.Y, val.Z + 200f);
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
            Vector3 pos = Ufoia.Position;

            bool hit =
                World.IsBulletInArea(pos, 35f) ||
                World.IsProjectileInArea(pos, 35f) ||
                World.IsExplosionInArea(pos, 35f);

            if (hit)
                Health--;
        }

        if (Health <= 0 && !IsDead)
        {
            IsDead = true;
            expdtimer = Game.GameTime + 200;

            Ufoia.IsInvincible = true;
            Ufoia.FreezePosition = false;
        }

        if (!IsDead)
            return;

        if (Game.GameTime > expdtimer)
        {
            Vector3 off1 = Ufoia.GetOffsetInWorldCoords(
                (float)GenRand(-1, 1),
                (float)GenRand(-1, 1),
                0.5f
            );

            Vector3 off2 = Ufoia.GetOffsetInWorldCoords(
                (float)GenRand(-1, 1),
                (float)GenRand(-1, 1),
                0.5f
            );

            World.AddExplosion(off1, ExplosionType.GasCanister, 20f, 1f);
            World.AddExplosion(off2, ExplosionType.GasCanister, 20f, 0.5f);

            Ufoia.ApplyForce(Vector3.WorldDown * 20f);
            expdtimer = Game.GameTime + 500;
        }

        Vector3 groundPos = new Vector3(
            Ufoia.Position.X,
            Ufoia.Position.Y,
            World.GetGroundHeight(Ufoia.Position)
        );

        if (Ufoia.Position.DistanceTo(groundPos) > 80f || !Ufoia.HasCollided)
            return;

        Vector3 blastPos = Ufoia.GetOffsetInWorldCoords(0f, 0f, -10f);

        World.AddExplosion(
            Ufoia.Position,
            ExplosionType.Rocket,
            100f,
            2f
        );

        World.AddParticleEffect(
            "scr_agencyheistb",
            "scr_agency3b_heli_expl",
            blastPos,
            Vector3.Zero,
            5f
        );

        GameplayCamera.Shake(CameraShake.Explosion, 1f);

        foreach (Ped ped in World.GetNearbyPeds(Ufoia.Position, 80f))
        {
            ped.ApplyForce(
                Vector3.WorldUp * 20f +
                Vector3.Reflect(ped.Position, Ufoia.Position) * 5f
            );

            if (ped != Game.Player.Character)
                ped.Kill();
        }

        foreach (Vehicle veh in World.GetNearbyVehicles(Ufoia.Position, 80f))
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
