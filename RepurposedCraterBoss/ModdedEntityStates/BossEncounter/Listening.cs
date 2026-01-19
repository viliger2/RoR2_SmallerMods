using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RepurposedCraterBoss.ModdedEntityStates.BossEncounter
{
    public class Listening : EntityState
    {
        public struct PositionAndRotation
        {
            public Vector3 position;
            public Vector3 rotation;
        }

        public static float delayBeforeBeginningEncounter = 2f;

        public static int camerasDestroyedToTriggerEncounter => RepurposedCraterBossPlugin.CamerasToDestroy.Value;

        public static int camerasCountToSpawn => RepurposedCraterBossPlugin.CamerasToSpawn.Value;

        public static GameObject cameraPrefab;

        public static PositionAndRotation[] cameraPositions = new PositionAndRotation[]
        {
            new PositionAndRotation { position = new Vector3(-106.459999f,6.25f,-5.44000006f), rotation = Vector3.zero },
            new PositionAndRotation { position = new Vector3(-87.3600006f,9.56999969f,28.0200005f), rotation = new Vector3(0,185.242157f,0) },
            new PositionAndRotation { position = new Vector3(-96.4899979f,5.81513739f,-82.7600021f), rotation = new Vector3(0,180f,0) },
            new PositionAndRotation { position = new Vector3(-115.860001f,9.85119724f,-115.950401f), rotation = Vector3.zero },
            new PositionAndRotation { position = new Vector3(-53.0099983f,64.3600006f,-192f), rotation = new Vector3(0,264.001343f,0) },
            new PositionAndRotation { position = new Vector3(-49.9500008f,86.0500031f,-123.110001f), rotation = new Vector3(0,137.270981f,0) },
            new PositionAndRotation { position = new Vector3(-37.8199997f,87.25f,59.8899994f), rotation = new  Vector3(0,298.396729f,0)},
            new PositionAndRotation { position = new Vector3(-182.070007f,95.1200027f,60.4799995f), rotation = new  Vector3(0,272.16983f,0)},
            new PositionAndRotation { position = new Vector3(-237.610001f,89.8199997f,-49.0699997f), rotation = new Vector3(0,114.827164f,0) },
            new PositionAndRotation { position = new Vector3(-127.910004f,66.5400009f,-66.9400024f), rotation = Vector3.zero },
            new PositionAndRotation { position = new Vector3(-96.5518951f,67f,11.643568f), rotation = new Vector3(0,170.957443f,0) },
            new PositionAndRotation { position = new Vector3(-236.270004f,7.48000002f,-3.8039999f), rotation = new Vector3(0,338.596069f,0) },
            new PositionAndRotation { position = new Vector3(-125.239998f,39.0200005f,-183.100006f), rotation = new Vector3(0,246.228882f,0) },
            new PositionAndRotation { position = new Vector3(-74.3737183f,52.0525169f,111.492805f), rotation = new Vector3(6.6730938f,36.2126808f,9.28466225f) },
            new PositionAndRotation { position = new Vector3(-140.229996f,101.308121f,156.191757f), rotation = new Vector3(0,171.870865f,0) },
            new PositionAndRotation { position = new Vector3(-250.699997f,111.750092f,45.1599998f), rotation = Vector3.zero },
            new PositionAndRotation { position = new Vector3(-257.359985f,117.849998f,10.9200001f), rotation = Vector3.zero  },
            new PositionAndRotation { position = new Vector3(-237.955994f,119.459999f,55.151001f), rotation = new Vector3(0,175.384705f,0)  },
            new PositionAndRotation { position = new Vector3(-234.352005f,51.5769997f,-134.994003f), rotation = new Vector3(0,119.490311f,0) },
            new PositionAndRotation { position = new Vector3(-192.080002f,12.1536093f,90.1500015f), rotation = new Vector3(0,176.986725f,0) },
            new PositionAndRotation { position = new Vector3(6.92000008f,63.6918716f,-38.6199989f), rotation = new Vector3(0,275.84613f,0) },
            new PositionAndRotation { position = new Vector3(-13.7159996f,63.5352936f,-83.0800018f), rotation = Vector3.zero  },
            new PositionAndRotation { position = new Vector3(3.8900001f,59.7472f,28.1000004f), rotation = new Vector3(0,309.36496f,0) },
            new PositionAndRotation { position = new Vector3(-92.8300018f,65.4300003f,108.989998f), rotation = new Vector3(0,261.875732f,0) },
            new PositionAndRotation { position = new Vector3(-3.86999989f,8.48556519f,-172), rotation = new Vector3(0,316.884064f,0) },
            new PositionAndRotation { position = new Vector3(-87.3099976f,70.5676575f,-78.6299973f), rotation = new Vector3(0,189.667725f,0) },
            new PositionAndRotation { position = new Vector3(10.1199999f,102.466553f,-90.4800034f), rotation = Vector3.zero  },
            new PositionAndRotation { position = new Vector3(-6.23000002f,95.0729752f,-25.8700008f), rotation = new Vector3(0,268.727203f,0) },
            new PositionAndRotation { position = new Vector3(-13.3400002f,18.4126358f,-21.6540298f), rotation = new Vector3(7.18282413f,231.344009f,8.88435936f) },
            new PositionAndRotation { position = new Vector3(-27.6567497f,13.2146215f,29.4670162f), rotation = new Vector3(341.29306f,20.3687477f,350.32663f) },
            new PositionAndRotation { position = new Vector3(-34.0999985f,70.6900024f,60.1899986f), rotation = new Vector3(0,89.6273117f,0) },
            new PositionAndRotation { position = new Vector3(-234.630005f,21.7000008f,69.8700027f), rotation = new Vector3(0,173.978592f,0) },
            new PositionAndRotation { position = new Vector3(-131.553513f,51.3408661f,-100.06823f), rotation = new Vector3(4.6581645f,314.046722f,4.79530621f) },
            new PositionAndRotation { position = new Vector3(-180.89798f,54.3335876f,-176.289871f), rotation = new Vector3(5.7819519f,231.71936f,8.65081978f) },
            new PositionAndRotation { position = new Vector3(-28.3331223f,9.56434631f,-83.7939377f), rotation = new Vector3(0,357.316376f,0) },
            new PositionAndRotation { position = new Vector3(-89.8099976f,8.38700104f,-121.620003f), rotation = new Vector3(0,269.843323f,0) },
            new PositionAndRotation { position = new Vector3(-100.910004f,13.0732002f,54.8800011f), rotation = new Vector3(0,257.052917f,0) },
            new PositionAndRotation { position = new Vector3(-30.607563f,5.88229084f,-140.952057f), rotation = new Vector3(0,346.750214f,0) },
            new PositionAndRotation { position = new Vector3(-213.690002f,75.2082596f,87.3499985f), rotation = new Vector3(0,125.666496f,0) },
            new PositionAndRotation { position = new Vector3(-116.739998f,20.4400005f,101.57f), rotation = new Vector3(0,290.601196f,0) },
            new PositionAndRotation { position = new Vector3(-104.58812f,24.4300003f,-144.85408f), rotation = new Vector3(0,312.262451f,0) },
            new PositionAndRotation { position = new Vector3(-218.360001f,51.1452026f,-105.57f), rotation = new Vector3(0,114.345848f,0) },
            new PositionAndRotation { position = new Vector3(-15.0200005f,72.120285f,152.600006f), rotation = new Vector3(0,293.239136f,0) },
            new PositionAndRotation { position = new Vector3(-255.449997f,126.015228f,143.160004f), rotation = new Vector3(0,227.520721f,0) },
            new PositionAndRotation { position = new Vector3(-245.279999f,56.3236618f,-149.399994f), rotation = new Vector3(0,339.614624f,0) },
        };

        private List<GameObject> cameraList = new List<GameObject>();

        private ScriptedCombatEncounter scriptedCombatEncounter;

        private bool hasRegisteredCameras = false;

        private int camerasToDestroy;

        private int previousDestroyedCameras;

        private bool beginEncounterCountdown;

        private float encounterCountdown;

        public override void OnEnter()
        {
            base.OnEnter();
            scriptedCombatEncounter = GetComponent<ScriptedCombatEncounter>();
            camerasToDestroy = Mathf.Min(camerasDestroyedToTriggerEncounter, cameraPositions.Length);
            if (NetworkServer.active)
            {
                if (!cameraPrefab)
                {
                    return;
                }

                Util.ShuffleArray(cameraPositions);

                for (int i = 0; i < Mathf.Min(camerasCountToSpawn, cameraPositions.Length); i++)
                {
                    var positionAndRotation = cameraPositions[i];
                    var newCamera = UnityEngine.Object.Instantiate(cameraPrefab, positionAndRotation.position, Quaternion.Euler(positionAndRotation.rotation));
                    NetworkServer.Spawn(newCamera);
                    cameraList.Add(newCamera);
                }
                hasRegisteredCameras = true;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active)
            {
                return;
            }

            if (!hasRegisteredCameras)
            {
                return;
            }

            int destroyedCameraCount = 0;
            for (int i = 0; i < cameraList.Count; i++)
            {
                if (cameraList[i] == null)
                {
                    destroyedCameraCount++;
                }
            }

            if (previousDestroyedCameras < camerasToDestroy - 1 && destroyedCameraCount >= camerasToDestroy - 1)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "VILIGER_ALLOY_CAMERA_CLOSE"
                });
            }
            if (destroyedCameraCount >= camerasToDestroy && !beginEncounterCountdown)
            {
                encounterCountdown = delayBeforeBeginningEncounter;
                beginEncounterCountdown = true;
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "VILIGER_ALLOY_CAMERA_BEGIN"
                });
            }
            if (beginEncounterCountdown)
            {
                encounterCountdown -= GetDeltaTime();
                if (encounterCountdown <= 0f)
                {
                    scriptedCombatEncounter.BeginEncounter();
                    outer.SetNextState(new Idle());
                }
            }
            previousDestroyedCameras = destroyedCameraCount;
        }
    }
}
