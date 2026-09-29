using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance {get;private set;}

    private Unity.Cinemachine.CinemachineVirtualCamera camera;
    private float ShakeIntensity = 1.3f;
    private float Startintensity;
    private float ShakeTime = 0.3f;
    private float shaketimerTotal;
    private float Timer;
    private Unity.Cinemachine.CinemachineBasicMultiChannelPerlin _cbmcp;

    private void Awake() {
        Instance = this;
        camera = GetComponent<Unity.Cinemachine.CinemachineVirtualCamera>();
    }
    void Start()
    {
        StopShake();
    }

    // Update is called once per frame
    void Update()
    {
        if(Timer>0){
            Timer -=Time.deltaTime;
            Unity.Cinemachine.CinemachineBasicMultiChannelPerlin _cbmcp = camera.GetCinemachineComponent<Unity.Cinemachine.CinemachineBasicMultiChannelPerlin>();
            _cbmcp.AmplitudeGain = Mathf.Lerp(Startintensity, 0f,1 - (Timer / shaketimerTotal));
        }
    }
    public void ShakeCamera(){
        Unity.Cinemachine.CinemachineBasicMultiChannelPerlin _cbmcp = camera.GetCinemachineComponent<Unity.Cinemachine.CinemachineBasicMultiChannelPerlin>();
        _cbmcp.AmplitudeGain = ShakeIntensity;

        Startintensity = ShakeIntensity;
        shaketimerTotal = ShakeTime;
        Timer = ShakeTime;
    }
    void StopShake(){
        Unity.Cinemachine.CinemachineBasicMultiChannelPerlin _cbmcp = camera.GetCinemachineComponent<Unity.Cinemachine.CinemachineBasicMultiChannelPerlin>();
        _cbmcp.AmplitudeGain = 0f;

        Timer = 0;
    }
}
