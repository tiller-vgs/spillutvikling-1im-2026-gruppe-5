using UnityEngine;
using UnityEngine.SceneManagement;

public static class OverworldStoryState
{
    private const string OverworldSceneName = "OverworldScene";
    private const string AlarmObjectName = "alarm_0";
    private const string AlarmTextObjectName = "Alarm_0_Message";
    private const string GunObjectName = "Gun";
    private const string GunClonePrefix = "Gun(";
    private const string PoliceCarObjectName = "Police car_0";

    public static bool IsCompleted { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Object.FindFirstObjectByType<RuntimeBootstrap>() != null)
        {
            return;
        }

        GameObject runtimeObject = new GameObject("OverworldStoryStateRuntime");
        runtimeObject.hideFlags = HideFlags.HideInHierarchy;
        Object.DontDestroyOnLoad(runtimeObject);
        runtimeObject.AddComponent<RuntimeBootstrap>();
    }

    public static void MarkCompleted()
    {
        IsCompleted = true;
    }

    public static void ResetProgress()
    {
        IsCompleted = false;
    }

    private static void ApplySceneState(Scene scene)
    {
        if (!IsCompleted || !scene.isLoaded || scene.name != OverworldSceneName)
        {
            return;
        }

        GameObject alarm = FindObjectInScene(scene, AlarmObjectName);
        if (alarm != null)
        {
            Object.Destroy(alarm);
        }

        GameObject alarmText = FindObjectInScene(scene, AlarmTextObjectName);
        if (alarmText != null)
        {
            Object.Destroy(alarmText);
        }

        RemoveGuns(scene);
        DisablePoliceCarSequence(scene);
        UnequipPlayer(scene);
    }

    private static void RemoveGuns(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            RemoveGunsInHierarchy(rootObject.transform);
        }
    }

    private static void RemoveGunsInHierarchy(Transform current)
    {
        for (int i = current.childCount - 1; i >= 0; i--)
        {
            RemoveGunsInHierarchy(current.GetChild(i));
        }

        string objectName = current.name;
        if (objectName == GunObjectName || objectName.StartsWith(GunClonePrefix))
        {
            Object.Destroy(current.gameObject);
        }
    }

    private static void DisablePoliceCarSequence(Scene scene)
    {
        GameObject policeCar = FindObjectInScene(scene, PoliceCarObjectName);
        if (policeCar == null)
        {
            return;
        }

        PoliceCarAfterGunPickup sequence = policeCar.GetComponent<PoliceCarAfterGunPickup>();
        if (sequence != null)
        {
            sequence.enabled = false;
        }
    }

    private static void UnequipPlayer(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Player_controller player = rootObject.GetComponentInChildren<Player_controller>(true);
            if (player == null)
            {
                continue;
            }

            DirectionalDragonBonesView view = player.GetComponent<DirectionalDragonBonesView>();
            if (view != null && view.HasGun)
            {
                view.UnequipGun();
            }

            return;
        }
    }

    private static GameObject FindObjectInScene(Scene scene, string objectName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Transform match = FindInHierarchy(rootObject.transform, objectName);
            if (match != null)
            {
                return match.gameObject;
            }
        }

        return null;
    }

    private static Transform FindInHierarchy(Transform current, string objectName)
    {
        if (current.name == objectName)
        {
            return current;
        }

        for (int i = 0; i < current.childCount; i++)
        {
            Transform match = FindInHierarchy(current.GetChild(i), objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private sealed class RuntimeBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyLoadedScenes();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            ApplySceneState(scene);
        }

        private static void ApplyLoadedScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                ApplySceneState(SceneManager.GetSceneAt(i));
            }
        }
    }
}
