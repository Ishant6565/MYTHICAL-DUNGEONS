using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerInput : MonoBehaviour
{
    public float HorizontalInput;
    public float VerticalInput;
    public bool MouseButtonDown;
    public bool SpaceKeyDown;
    private MobileTouchControls _mobileTouchControls;

    private void Awake()
    {
        if (Input.touchSupported)
        {
            ConfigureMobilePerformance();
            _mobileTouchControls = gameObject.AddComponent<MobileTouchControls>();
        }
    }

    private static void ConfigureMobilePerformance()
    {
        Application.targetFrameRate = 60;

        string[] qualityNames = QualitySettings.names;
        int lowQualityIndex = System.Array.IndexOf(qualityNames, "Low");
        if (lowQualityIndex >= 0)
        {
            QualitySettings.SetQualityLevel(lowQualityIndex, true);
        }
        else
        {
            Debug.LogWarning("Mobile quality profile was not applied: the Low quality level is missing.");
        }

        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset pipelineAsset)
        {
            pipelineAsset.renderScale = 0.9f;

            foreach (ScriptableRendererData rendererData in pipelineAsset.rendererDataList)
            {
                foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
                {
                    if (feature != null && feature.name == "NewScreenSpaceAmbientOcclusion")
                    {
                        feature.SetActive(false);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Mobile render scaling was not applied: the active pipeline is not URP.");
        }
    }
 
    void Update()
    {
        if(!MouseButtonDown && Time.timeScale !=0){
            bool mobileAttackPressed = _mobileTouchControls != null &&
                _mobileTouchControls.ConsumeAttack();
            MouseButtonDown = Input.GetMouseButtonDown(0) || mobileAttackPressed;
        }
        if(!SpaceKeyDown && Time.timeScale !=0){
            bool mobileDodgePressed = _mobileTouchControls != null &&
                _mobileTouchControls.ConsumeDodge();
            SpaceKeyDown = Input.GetKeyDown(KeyCode.Space) || mobileDodgePressed;
        }

        Vector2 movement = _mobileTouchControls != null
            ? _mobileTouchControls.Movement
            : Vector2.zero;
        if (movement.sqrMagnitude > 0f)
        {
            HorizontalInput = movement.x;
            VerticalInput = movement.y;
        }
        else
        {
            HorizontalInput = Input.GetAxisRaw("Horizontal");
            VerticalInput = Input.GetAxisRaw("Vertical");
        }
    }

    //stop update when player/gameobject is dead, make it uncontrollable
    private void OnDisable() {
        clearCache();
        
    }

    public void clearCache(){
        MouseButtonDown = false;
        SpaceKeyDown = false;
        HorizontalInput = 0;
        VerticalInput = 0;
        if (_mobileTouchControls != null)
        {
            _mobileTouchControls.ClearActionInputs();
        }
    }
}
