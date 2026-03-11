using DragonBones;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using DBAnimationState = DragonBones.AnimationState;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class DirectionalDragonBonesView : MonoBehaviour
{
    private const string DefaultSideShootingAnimationName = "shootingAnimation";
    private const string SideWalkingBlendGroup = "SideWalkingBlend";
    private const string SideShootingBlendGroup = "SideShootingBlend";
    private const int SideWalkingBlendLayer = 0;
    private const int SideShootingBlendLayer = 1;
    private const string RightLegBoneName = "rightLeg";
    private const string LeftLegBoneName = "leftLeg";
    private const string SideHandSlotName = "leftHand";
    private const string FrontHandSlotName = "leftHand";
    private const string BackHandSlotName = "rightHand";
    private const int UnarmedSideHandDisplayIndex = 0;
    private const int ArmedSideHandDisplayIndex = 1;
    private const int UnarmedFrontHandDisplayIndex = 0;
    private const int ArmedFrontHandDisplayIndex = 1;
    private const int UnarmedBackHandDisplayIndex = 0;
    private const int ArmedBackHandDisplayIndex = 1;
    private const string ManualSideHandDisplayController = "none";

    [Header("Side View")]
    [FormerlySerializedAs("skeletonData")]
    public TextAsset sideSkeletonData;
    [FormerlySerializedAs("textureAtlasData")]
    public TextAsset sideTextureAtlasData;
    [FormerlySerializedAs("textureAtlasTexture")]
    public Texture2D sideTextureAtlasTexture;
    [FormerlySerializedAs("dragonBonesDataName")]
    public string sideDragonBonesDataName = string.Empty;
    [FormerlySerializedAs("armatureName")]
    public string sideArmatureName = string.Empty;
    [FormerlySerializedAs("walkingAnimationName")]
    public string sideWalkingAnimationName = string.Empty;
    public string sideShootingAnimationName = string.Empty;

    [Header("Front View")]
    public TextAsset frontSkeletonData;
    public TextAsset frontTextureAtlasData;
    public Texture2D frontTextureAtlasTexture;
    public string frontDragonBonesDataName = string.Empty;
    public string frontArmatureName = string.Empty;
    public string frontWalkingAnimationName = string.Empty;

    [Header("Back View")]
    public TextAsset backSkeletonData;
    public TextAsset backTextureAtlasData;
    public Texture2D backTextureAtlasTexture;
    public string backDragonBonesDataName = string.Empty;
    public string backArmatureName = string.Empty;
    public string backWalkingAnimationName = string.Empty;

    [Header("View")]
    public bool hideSourceSpriteRenderer = true;
    [FormerlySerializedAs("visualOffset")]
    public Vector3 sideVisualOffset = Vector3.zero;
    [FormerlySerializedAs("visualScale")]
    public Vector3 sideVisualScale = Vector3.one;
    public Vector3 frontVisualOffset = Vector3.zero;
    public Vector3 frontVisualScale = Vector3.one;
    public Vector3 backVisualOffset = Vector3.zero;
    public Vector3 backVisualScale = Vector3.one;
    public int sortingOrderOffset = 0;
    public float armatureScale = 0.01f;
    public float textureScale = 1f;
    public float moveThreshold = 0.05f;

    private enum ViewMode
    {
        Side,
        Front,
        Back
    }

    private Rigidbody2D _rb;
    private SpriteRenderer _sourceSpriteRenderer;
    private UnityArmatureComponent _sideArmatureComponent;
    private UnityArmatureComponent _frontArmatureComponent;
    private UnityArmatureComponent _backArmatureComponent;
    private UnityArmatureComponent _activeArmatureComponent;
    private string _activeAnimation = string.Empty;
    private string _activeWalkingAnimationName = string.Empty;
    private Vector3 _sideBaseVisualScale = Vector3.one;
    private Vector3 _frontBaseVisualScale = Vector3.one;
    private Vector3 _backBaseVisualScale = Vector3.one;
    private ViewMode _lastViewMode = ViewMode.Side;
    private float _sideFacingDirection = 1f;
    private bool _isRestPoseApplied;
    private bool _isInitialized;
    private bool _isShooting;
    private bool _isSideAnimationListenerRegistered;
    private bool _hasGun;
    private DBAnimationState _sideWalkingBlendState;
    private DBAnimationState _sideShootingBlendState;

    private void Awake()
    {
        CacheComponents();
        RefreshView();
    }

    private void Update()
    {
        if (!_isInitialized)
        {
            RefreshView();
        }

        if (_sideArmatureComponent == null &&
            _frontArmatureComponent == null &&
            _backArmatureComponent == null)
        {
            return;
        }

        SetActiveView(ResolveDesiredViewMode());
        if (_isShooting)
        {
            SetSideFacing(1f);
        }
        else
        {
            UpdateFacing();
        }
        RefreshArmatureTransforms();
        UpdateAnimationState();
        UpdateHandDisplays();
    }

    public void RefreshView()
    {
        CacheComponents();
        _sideBaseVisualScale = sideVisualScale;
        _frontBaseVisualScale = frontVisualScale;
        _backBaseVisualScale = backVisualScale;
        BuildArmaturesIfNeeded();

        _isInitialized =
            _sideArmatureComponent != null ||
            _frontArmatureComponent != null ||
            _backArmatureComponent != null;

        if (!_isInitialized)
        {
            return;
        }

        SetActiveView(ResolveDesiredViewMode());
        if (_isShooting)
        {
            SetSideFacing(1f);
        }
        RefreshArmatureTransforms();
        UpdateAnimationState();
        UpdateHandDisplays();
    }

    public bool HasGun => _hasGun;

    public void EquipGun()
    {
        if (_hasGun)
        {
            return;
        }

        _hasGun = true;
        if (!_isInitialized)
        {
            RefreshView();
        }

        UpdateHandDisplays();
    }

    public void UnequipGun()
    {
        if (!_hasGun)
        {
            return;
        }

        _hasGun = false;
        if (_isShooting)
        {
            _isShooting = false;
            ClearSideBlendStates();
            ResetSideAnimationState();
            _activeAnimation = string.Empty;
            _isRestPoseApplied = false;
            SetActiveView(ResolveDesiredViewMode());
            UpdateFacing();
            UpdateAnimationState();
        }

        UpdateHandDisplays();
    }

    public bool PlayShootingAnimation()
    {
        if (!_isInitialized)
        {
            RefreshView();
        }

        if (!_hasGun)
        {
            return false;
        }

        string shootingAnimationName = GetSideShootingAnimationName();
        if (_sideArmatureComponent == null ||
            string.IsNullOrWhiteSpace(shootingAnimationName) ||
            _sideArmatureComponent.animation == null ||
            !_sideArmatureComponent.animation.HasAnimation(shootingAnimationName))
        {
            return false;
        }

        SetActiveView(ViewMode.Side);
        _lastViewMode = ViewMode.Side;
        SetSideFacing(1f);

        if (!_isShooting)
        {
            ClearSideBlendStates();
            ResetSideAnimationState();
        }

        _isShooting = true;
        UpdateHandDisplays();
        _sideShootingBlendState = PlaySideShootingBlendState(shootingAnimationName);
        if (_sideShootingBlendState == null)
        {
            _isShooting = false;
            UpdateHandDisplays();
            return false;
        }

        if (_sideWalkingBlendState == null || _sideWalkingBlendState.isFadeOut)
        {
            _sideWalkingBlendState = PlaySideWalkingBlendState();
        }

        UpdateSideWalkingBlendState();
        _activeAnimation = shootingAnimationName;
        _isRestPoseApplied = false;
        return true;
    }

    private void CacheComponents()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        if (_sourceSpriteRenderer == null)
        {
            _sourceSpriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void BuildArmaturesIfNeeded()
    {
        if (_sideArmatureComponent == null)
        {
            _sideArmatureComponent = BuildArmature(
                sideSkeletonData,
                sideTextureAtlasData,
                sideTextureAtlasTexture,
                sideDragonBonesDataName,
                sideArmatureName,
                sideVisualOffset,
                sideVisualScale,
                "SideArmature"
            );
            _sideBaseVisualScale = sideVisualScale;
        }

        if (_sideArmatureComponent != null && !_isSideAnimationListenerRegistered)
        {
            _sideArmatureComponent.AddDBEventListener(EventObject.COMPLETE, HandleAnimationComplete);
            _isSideAnimationListenerRegistered = true;
        }

        if (_frontArmatureComponent == null && HasFrontViewAssets())
        {
            _frontArmatureComponent = BuildArmature(
                frontSkeletonData,
                frontTextureAtlasData,
                frontTextureAtlasTexture,
                frontDragonBonesDataName,
                frontArmatureName,
                frontVisualOffset,
                frontVisualScale,
                "FrontArmature"
            );
            _frontBaseVisualScale = frontVisualScale;
        }

        if (_backArmatureComponent == null && HasBackViewAssets())
        {
            _backArmatureComponent = BuildArmature(
                backSkeletonData,
                backTextureAtlasData,
                backTextureAtlasTexture,
                backDragonBonesDataName,
                backArmatureName,
                backVisualOffset,
                backVisualScale,
                "BackArmature"
            );
            _backBaseVisualScale = backVisualScale;
        }

        if (hideSourceSpriteRenderer &&
            _sourceSpriteRenderer != null &&
            (_sideArmatureComponent != null ||
             _frontArmatureComponent != null ||
             _backArmatureComponent != null))
        {
            _sourceSpriteRenderer.enabled = false;
        }
    }

    private UnityArmatureComponent BuildArmature(
        TextAsset skeletonAsset,
        TextAsset atlasAsset,
        Texture2D textureAsset,
        string dataNameOverride,
        string armatureNameOverride,
        Vector3 localOffset,
        Vector3 localScale,
        string objectName)
    {
        if (skeletonAsset == null || atlasAsset == null || textureAsset == null)
        {
            return null;
        }

        string dataName = string.IsNullOrWhiteSpace(dataNameOverride) ? skeletonAsset.name : dataNameOverride;
        string armatureId = string.IsNullOrWhiteSpace(armatureNameOverride) ? "armature1" : armatureNameOverride;
        UnityFactory factory = UnityFactory.factory;

        if (!EnsureDragonBonesData(factory, dataName, skeletonAsset))
        {
            return null;
        }

        if (!EnsureTextureAtlasData(factory, dataName, atlasAsset, textureAsset))
        {
            return null;
        }

        UnityArmatureComponent armatureComponent = factory.BuildArmatureComponent(armatureId, dataName);
        if (armatureComponent == null)
        {
            Debug.LogError(
                "DragonBones armature could not be built. Check armatureName and asset references.",
                this
            );
            return null;
        }

        armatureComponent.name = objectName;
        armatureComponent.transform.SetParent(transform, false);
        SetArmatureTransform(armatureComponent.transform, localOffset, localScale);
        armatureComponent.gameObject.layer = gameObject.layer;

        ApplySortingFromSourceSprite(armatureComponent);

        return armatureComponent;
    }

    private bool EnsureDragonBonesData(UnityFactory factory, string dataName, TextAsset skeletonAsset)
    {
        factory.RemoveDragonBonesData(dataName, false);

        Dictionary<string, object> rawSkeletonData =
            MiniJSON.Json.Deserialize(skeletonAsset.text) as Dictionary<string, object>;

        if (rawSkeletonData == null)
        {
            Debug.LogError("DragonBones skeleton json could not be parsed.", this);
            return false;
        }

        rawSkeletonData["version"] = "5.5";
        rawSkeletonData["compatibleVersion"] = "5.5";
        rawSkeletonData["name"] = dataName;
        SanitizeDuplicateArmatures(rawSkeletonData);
        SanitizeArmatureDefaults(rawSkeletonData);

        if (factory.ParseDragonBonesData(rawSkeletonData, dataName, armatureScale) == null)
        {
            Debug.LogError("DragonBones skeleton data could not be loaded by the runtime.", this);
            return false;
        }

        return true;
    }

    private bool EnsureTextureAtlasData(
        UnityFactory factory,
        string dataName,
        TextAsset atlasAsset,
        Texture2D textureAsset)
    {
        if (factory.GetTextureAtlasData(dataName) != null)
        {
            return true;
        }

        var atlas = new UnityDragonBonesData.TextureAtlas
        {
            textureAtlasJSON = atlasAsset,
            texture = textureAsset,
            material = UnityFactoryHelper.GenerateMaterial(
                UnityFactory.defaultShaderName,
                textureAsset.name + "_Mat_Runtime_" + dataName,
                textureAsset
            )
        };

        factory.LoadTextureAtlasData(atlas, dataName, textureScale, false);
        return factory.GetTextureAtlasData(dataName) != null;
    }

    private bool HasFrontViewAssets()
    {
        return frontSkeletonData != null &&
               frontTextureAtlasData != null &&
               frontTextureAtlasTexture != null;
    }

    private bool HasBackViewAssets()
    {
        return backSkeletonData != null &&
               backTextureAtlasData != null &&
               backTextureAtlasTexture != null;
    }

    private ViewMode ResolveDesiredViewMode()
    {
        if (_isShooting && _sideArmatureComponent != null)
        {
            return ViewMode.Side;
        }

        if (_rb == null)
        {
            if (_sideArmatureComponent != null)
            {
                return ViewMode.Side;
            }

            if (_frontArmatureComponent != null)
            {
                return ViewMode.Front;
            }

            return ViewMode.Back;
        }

        Vector2 velocity = _rb.linearVelocity;
        if (velocity.sqrMagnitude <= moveThreshold * moveThreshold)
        {
            return _lastViewMode;
        }

        bool hasHorizontalInput = Mathf.Abs(velocity.x) > moveThreshold;
        bool hasVerticalInput = Mathf.Abs(velocity.y) > moveThreshold;
        bool isMovingDown = velocity.y < -moveThreshold;
        bool isMovingUp = velocity.y > moveThreshold;

        if (hasHorizontalInput && hasVerticalInput && _sideArmatureComponent != null)
        {
            return ViewMode.Side;
        }

        if (isMovingDown && _frontArmatureComponent != null)
        {
            return ViewMode.Front;
        }

        if (isMovingUp && _backArmatureComponent != null)
        {
            return ViewMode.Back;
        }

        if (_sideArmatureComponent != null)
        {
            return ViewMode.Side;
        }

        if (isMovingDown && _frontArmatureComponent != null)
        {
            return ViewMode.Front;
        }

        if (isMovingUp && _backArmatureComponent != null)
        {
            return ViewMode.Back;
        }

        return _frontArmatureComponent != null ? ViewMode.Front : ViewMode.Back;
    }

    private void SetActiveView(ViewMode desiredView)
    {
        UnityArmatureComponent desiredArmature = null;

        if (desiredView == ViewMode.Front && _frontArmatureComponent != null)
        {
            desiredArmature = _frontArmatureComponent;
        }
        else if (desiredView == ViewMode.Back && _backArmatureComponent != null)
        {
            desiredArmature = _backArmatureComponent;
        }
        else if (_sideArmatureComponent != null)
        {
            desiredArmature = _sideArmatureComponent;
        }
        else if (_frontArmatureComponent != null)
        {
            desiredArmature = _frontArmatureComponent;
        }
        else
        {
            desiredArmature = _backArmatureComponent;
        }

        if (desiredArmature == null)
        {
            return;
        }

        if (_activeArmatureComponent == desiredArmature)
        {
            return;
        }

        StopActiveAnimation();

        _activeArmatureComponent = desiredArmature;
        if (_activeArmatureComponent == _frontArmatureComponent)
        {
            _activeWalkingAnimationName = frontWalkingAnimationName;
        }
        else if (_activeArmatureComponent == _backArmatureComponent)
        {
            _activeWalkingAnimationName = backWalkingAnimationName;
        }
        else
        {
            _activeWalkingAnimationName = sideWalkingAnimationName;
        }

        _activeAnimation = string.Empty;
        _isRestPoseApplied = false;
        if (_activeArmatureComponent == _frontArmatureComponent)
        {
            _lastViewMode = ViewMode.Front;
        }
        else if (_activeArmatureComponent == _backArmatureComponent)
        {
            _lastViewMode = ViewMode.Back;
        }
        else
        {
            _lastViewMode = ViewMode.Side;
        }

        SetArmatureVisible(_sideArmatureComponent, _activeArmatureComponent == _sideArmatureComponent);
        SetArmatureVisible(_frontArmatureComponent, _activeArmatureComponent == _frontArmatureComponent);
        SetArmatureVisible(_backArmatureComponent, _activeArmatureComponent == _backArmatureComponent);
        RefreshArmatureTransforms();
    }

    private void SetArmatureVisible(UnityArmatureComponent armatureComponent, bool isVisible)
    {
        if (armatureComponent == null)
        {
            return;
        }

        armatureComponent.gameObject.SetActive(isVisible);
    }

    private void ApplySortingFromSourceSprite(UnityArmatureComponent armatureComponent)
    {
        if (_sourceSpriteRenderer == null || armatureComponent == null)
        {
            return;
        }

        Renderer[] renderers = armatureComponent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        int minSortingOrder = int.MaxValue;
        foreach (Renderer renderer in renderers)
        {
            if (renderer.sortingOrder < minSortingOrder)
            {
                minSortingOrder = renderer.sortingOrder;
            }
        }

        int baseSortingOrder = _sourceSpriteRenderer.sortingOrder + sortingOrderOffset;
        foreach (Renderer renderer in renderers)
        {
            renderer.sortingLayerID = _sourceSpriteRenderer.sortingLayerID;
            renderer.sortingOrder = baseSortingOrder + (renderer.sortingOrder - minSortingOrder);
        }
    }

    private void UpdateFacing()
    {
        if (_activeArmatureComponent == null ||
            _activeArmatureComponent != _sideArmatureComponent ||
            _rb == null ||
            Mathf.Abs(_rb.linearVelocity.x) <= moveThreshold)
        {
            return;
        }

        SetSideFacing(_rb.linearVelocity.x);
    }

    private void RefreshArmatureTransforms()
    {
        if (_sideArmatureComponent != null)
        {
            SetArmatureTransform(_sideArmatureComponent.transform, sideVisualOffset, GetSideVisualScale());
        }

        if (_frontArmatureComponent != null)
        {
            SetArmatureTransform(_frontArmatureComponent.transform, frontVisualOffset, _frontBaseVisualScale);
        }

        if (_backArmatureComponent != null)
        {
            SetArmatureTransform(_backArmatureComponent.transform, backVisualOffset, _backBaseVisualScale);
        }
    }

    private void SetArmatureTransform(UnityEngine.Transform armatureTransform, Vector3 desiredOffset, Vector3 desiredScale)
    {
        if (armatureTransform == null)
        {
            return;
        }

        Vector3 parentScale = transform.lossyScale;
        armatureTransform.localPosition = new Vector3(
            desiredOffset.x / GetSafeScaleComponent(parentScale.x),
            desiredOffset.y / GetSafeScaleComponent(parentScale.y),
            desiredOffset.z / GetSafeScaleComponent(parentScale.z)
        );
        armatureTransform.localScale = new Vector3(
            desiredScale.x / GetSafeScaleComponent(parentScale.x),
            desiredScale.y / GetSafeScaleComponent(parentScale.y),
            desiredScale.z / GetSafeScaleComponent(parentScale.z)
        );
    }

    private Vector3 GetSideVisualScale()
    {
        Vector3 visualScale = _sideBaseVisualScale;
        float baseHorizontalSign = _sideBaseVisualScale.x == 0f ? 1f : Mathf.Sign(_sideBaseVisualScale.x);
        float facingDirection = _sideFacingDirection == 0f ? 1f : Mathf.Sign(_sideFacingDirection);
        visualScale.x = Mathf.Abs(_sideBaseVisualScale.x) * facingDirection * baseHorizontalSign;
        return visualScale;
    }

    private static float GetSafeScaleComponent(float scaleComponent)
    {
        return Mathf.Abs(scaleComponent) <= 0.0001f ? 1f : scaleComponent;
    }

    private void UpdateAnimationState()
    {
        if (_isShooting)
        {
            UpdateSideWalkingBlendState();
            return;
        }

        if (_activeArmatureComponent == null || string.IsNullOrWhiteSpace(_activeWalkingAnimationName))
        {
            return;
        }

        bool isMoving = _rb != null && _rb.linearVelocity.sqrMagnitude > moveThreshold * moveThreshold;
        if (isMoving)
        {
            if (_activeAnimation != _activeWalkingAnimationName)
            {
                _activeArmatureComponent.animation.Play(_activeWalkingAnimationName, 0);
                _activeAnimation = _activeWalkingAnimationName;
                _isRestPoseApplied = false;
            }

            return;
        }

        StopActiveAnimation();
    }

    private void StopActiveAnimation()
    {
        if (_activeArmatureComponent == null || _isRestPoseApplied)
        {
            return;
        }

        RestoreArmaturePose(_activeArmatureComponent);
        _activeAnimation = string.Empty;
        _isRestPoseApplied = true;
    }

    private static void RestoreArmaturePose(UnityArmatureComponent armatureComponent)
    {
        if (armatureComponent == null || armatureComponent.armature == null)
        {
            return;
        }

        armatureComponent.animation.Reset();

        foreach (Bone bone in armatureComponent.armature.GetBones())
        {
            bone.animationPose.Identity();
            bone.InvalidUpdate();
        }

        foreach (Slot slot in armatureComponent.armature.GetSlots())
        {
            if (slot._deformVertices != null)
            {
                for (int i = 0; i < slot._deformVertices.vertices.Count; i++)
                {
                    slot._deformVertices.vertices[i] = 0.0f;
                }

                slot._deformVertices.verticesDirty = true;
            }

            slot.InvalidUpdate();
        }

        armatureComponent.armature.InvalidUpdate(null, true);
        armatureComponent.armature.AdvanceTime(0.0f);
    }

    private void OnDestroy()
    {
        if (_sideArmatureComponent != null && _isSideAnimationListenerRegistered)
        {
            _sideArmatureComponent.RemoveDBEventListener(EventObject.COMPLETE, HandleAnimationComplete);
            _isSideAnimationListenerRegistered = false;
        }
    }

    private void HandleAnimationComplete(string type, EventObject eventObject)
    {
        if (!_isShooting ||
            eventObject == null ||
            eventObject.animationState == null ||
            eventObject.animationState != _sideShootingBlendState)
        {
            return;
        }

        _isShooting = false;
        ClearSideBlendStates();
        ResetSideAnimationState();
        _activeAnimation = string.Empty;
        _isRestPoseApplied = false;
        SetActiveView(ResolveDesiredViewMode());
        UpdateFacing();
        UpdateAnimationState();
        UpdateHandDisplays();
    }

    private string GetSideShootingAnimationName()
    {
        return string.IsNullOrWhiteSpace(sideShootingAnimationName)
            ? DefaultSideShootingAnimationName
            : sideShootingAnimationName;
    }

    private void UpdateHandDisplays()
    {
        UpdateSideHandDisplay();
        UpdateArmatureHandDisplay(
            _frontArmatureComponent,
            FrontHandSlotName,
            _hasGun ? ArmedFrontHandDisplayIndex : UnarmedFrontHandDisplayIndex
        );
        UpdateArmatureHandDisplay(
            _backArmatureComponent,
            BackHandSlotName,
            _hasGun ? ArmedBackHandDisplayIndex : UnarmedBackHandDisplayIndex
        );
    }

    private void UpdateSideHandDisplay()
    {
        if (_sideArmatureComponent == null || _sideArmatureComponent.armature == null)
        {
            return;
        }

        Slot sideHandSlot = _sideArmatureComponent.armature.GetSlot(SideHandSlotName);
        if (sideHandSlot == null)
        {
            return;
        }

        if (_isShooting)
        {
            sideHandSlot.displayController = SideShootingBlendGroup;
            sideHandSlot.InvalidUpdate();
            _sideArmatureComponent.armature.InvalidUpdate(null, true);
            _sideArmatureComponent.armature.AdvanceTime(0f);
            return;
        }

        sideHandSlot.displayController = ManualSideHandDisplayController;
        sideHandSlot.displayIndex = _hasGun ? ArmedSideHandDisplayIndex : UnarmedSideHandDisplayIndex;
        sideHandSlot.InvalidUpdate();
        _sideArmatureComponent.armature.InvalidUpdate(null, true);
        _sideArmatureComponent.armature.AdvanceTime(0f);
    }

    private void UpdateArmatureHandDisplay(
        UnityArmatureComponent armatureComponent,
        string slotName,
        int displayIndex)
    {
        if (armatureComponent == null || armatureComponent.armature == null)
        {
            return;
        }

        Slot handSlot = armatureComponent.armature.GetSlot(slotName);
        if (handSlot == null || handSlot.displayList.Count <= displayIndex)
        {
            return;
        }

        handSlot.displayController = ManualSideHandDisplayController;
        handSlot.displayIndex = displayIndex;
        handSlot.InvalidUpdate();
        armatureComponent.armature.InvalidUpdate(null, true);
        armatureComponent.armature.AdvanceTime(0f);
    }

    private void SetSideFacing(float horizontalDirection)
    {
        if (_sideArmatureComponent == null)
        {
            return;
        }

        if (horizontalDirection != 0f)
        {
            _sideFacingDirection = Mathf.Sign(horizontalDirection);
        }

        SetArmatureTransform(_sideArmatureComponent.transform, sideVisualOffset, GetSideVisualScale());
    }

    private DBAnimationState PlaySideShootingBlendState(string animationName)
    {
        if (_sideArmatureComponent == null ||
            _sideArmatureComponent.animation == null ||
            _sideArmatureComponent.armature == null ||
            string.IsNullOrWhiteSpace(animationName) ||
            !_sideArmatureComponent.animation.HasAnimation(animationName))
        {
            return null;
        }

        var animation = _sideArmatureComponent.animation;
        AnimationConfig animationConfig = animation.animationConfig;
        animationConfig.animation = animationName;
        animationConfig.playTimes = 1;
        animationConfig.fadeInTime = 0f;
        animationConfig.fadeOutTime = 0f;
        animationConfig.fadeOutMode = AnimationFadeOutMode.SameLayerAndGroup;
        animationConfig.layer = SideShootingBlendLayer;
        animationConfig.group = SideShootingBlendGroup;
        animationConfig.resetToPose = false;
        animationConfig.displayControl = true;
        animationConfig.RemoveBoneMask(_sideArmatureComponent.armature, RightLegBoneName, true);
        animationConfig.RemoveBoneMask(_sideArmatureComponent.armature, LeftLegBoneName, true);
        return animation.PlayConfig(animationConfig);
    }

    private DBAnimationState PlaySideWalkingBlendState()
    {
        if (_sideArmatureComponent == null ||
            _sideArmatureComponent.animation == null ||
            _sideArmatureComponent.armature == null ||
            string.IsNullOrWhiteSpace(sideWalkingAnimationName) ||
            !_sideArmatureComponent.animation.HasAnimation(sideWalkingAnimationName))
        {
            return null;
        }

        var animation = _sideArmatureComponent.animation;
        AnimationConfig animationConfig = animation.animationConfig;
        animationConfig.animation = sideWalkingAnimationName;
        animationConfig.playTimes = 0;
        animationConfig.fadeInTime = 0f;
        animationConfig.fadeOutTime = 0f;
        animationConfig.fadeOutMode = AnimationFadeOutMode.SameLayerAndGroup;
        animationConfig.layer = SideWalkingBlendLayer;
        animationConfig.group = SideWalkingBlendGroup;
        animationConfig.resetToPose = false;
        animationConfig.displayControl = false;
        animationConfig.AddBoneMask(_sideArmatureComponent.armature, RightLegBoneName, true);
        animationConfig.AddBoneMask(_sideArmatureComponent.armature, LeftLegBoneName, true);
        return animation.PlayConfig(animationConfig);
    }

    private void UpdateSideWalkingBlendState()
    {
        if (!_isShooting)
        {
            return;
        }

        if (_sideWalkingBlendState == null || _sideWalkingBlendState.isCompleted || _sideWalkingBlendState.isFadeOut)
        {
            _sideWalkingBlendState = PlaySideWalkingBlendState();
        }

        if (_sideWalkingBlendState == null)
        {
            return;
        }

        bool isMoving = _rb != null && _rb.linearVelocity.sqrMagnitude > moveThreshold * moveThreshold;
        if (isMoving)
        {
            _sideWalkingBlendState.Play();
            return;
        }

        _sideWalkingBlendState.Stop();
        _sideWalkingBlendState.currentTime = 0f;
    }

    private void ClearSideBlendStates()
    {
        if (_sideWalkingBlendState != null)
        {
            _sideWalkingBlendState.FadeOut(0f, true);
            _sideWalkingBlendState = null;
        }

        if (_sideShootingBlendState != null)
        {
            _sideShootingBlendState.FadeOut(0f, true);
            _sideShootingBlendState = null;
        }
    }

    private void ResetSideAnimationState()
    {
        if (_sideArmatureComponent == null || _sideArmatureComponent.animation == null)
        {
            return;
        }

        _sideArmatureComponent.animation.Reset();
    }

    private static void SanitizeDuplicateArmatures(Dictionary<string, object> rawSkeletonData)
    {
        if (!rawSkeletonData.TryGetValue("armature", out object armaturesObject) ||
            armaturesObject is not List<object> armatures)
        {
            return;
        }

        var sanitizedArmatures = new List<object>();
        var indexByName = new Dictionary<string, int>();

        foreach (object armatureObject in armatures)
        {
            if (armatureObject is not Dictionary<string, object> armature)
            {
                continue;
            }

            string name = armature.TryGetValue("name", out object armatureName)
                ? armatureName?.ToString()
                : string.Empty;

            if (!string.IsNullOrEmpty(name) && indexByName.TryGetValue(name, out int existingIndex))
            {
                sanitizedArmatures[existingIndex] = armature;
                continue;
            }

            if (!string.IsNullOrEmpty(name))
            {
                indexByName[name] = sanitizedArmatures.Count;
            }

            sanitizedArmatures.Add(armature);
        }

        rawSkeletonData["armature"] = sanitizedArmatures;
    }

    private static void SanitizeArmatureDefaults(Dictionary<string, object> rawSkeletonData)
    {
        if (!rawSkeletonData.TryGetValue("armature", out object armaturesObject) ||
            armaturesObject is not List<object> armatures)
        {
            return;
        }

        foreach (object armatureObject in armatures)
        {
            if (armatureObject is not Dictionary<string, object> armature)
            {
                continue;
            }

            armature["defaultActions"] = new List<object>();
            armature["actions"] = new List<object>();
        }
    }
}
