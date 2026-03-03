using DragonBones;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class DirectionalDragonBonesView : MonoBehaviour
{
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
    private bool _isRestPoseApplied;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sourceSpriteRenderer = GetComponent<SpriteRenderer>();

        BuildArmaturesIfNeeded();
        SetActiveView(ResolveDesiredViewMode());
        UpdateAnimationState();
    }

    private void Update()
    {
        if (_sideArmatureComponent == null &&
            _frontArmatureComponent == null &&
            _backArmatureComponent == null)
        {
            return;
        }

        SetActiveView(ResolveDesiredViewMode());
        UpdateFacing();
        UpdateAnimationState();
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
        armatureComponent.transform.localPosition = localOffset;
        armatureComponent.transform.localScale = localScale;
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

        if (_frontArmatureComponent != null)
        {
            _frontArmatureComponent.transform.localScale = _frontBaseVisualScale;
        }

        if (_backArmatureComponent != null)
        {
            _backArmatureComponent.transform.localScale = _backBaseVisualScale;
        }
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

        foreach (Renderer renderer in armatureComponent.GetComponentsInChildren<Renderer>())
        {
            renderer.sortingLayerID = _sourceSpriteRenderer.sortingLayerID;
            renderer.sortingOrder = _sourceSpriteRenderer.sortingOrder;
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

        Vector3 flippedScale = _sideBaseVisualScale;
        float baseHorizontalSign = _sideBaseVisualScale.x == 0f ? 1f : Mathf.Sign(_sideBaseVisualScale.x);
        flippedScale.x = Mathf.Abs(_sideBaseVisualScale.x) * Mathf.Sign(_rb.linearVelocity.x) * baseHorizontalSign;
        _sideArmatureComponent.transform.localScale = flippedScale;
    }

    private void UpdateAnimationState()
    {
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
