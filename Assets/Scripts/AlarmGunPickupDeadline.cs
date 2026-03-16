using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlarmGunPickupDeadline : MonoBehaviour
{
    private const float RestartDelay = 10f;

    private static readonly Vector3 TextOffset = new Vector3(3.1f, -0.15f, 0f);

    private Player_controller _player;
    private GameObject _textObject;
    private TextMeshPro _text;

    private void Start()
    {
        if (OverworldStoryState.IsCompleted)
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerHasGun())
        {
            Destroy(gameObject);
            return;
        }

        CreateText();
        StartCoroutine(WatchTimer());
    }

    private void OnDestroy()
    {
        DestroyText();
    }

    private IEnumerator WatchTimer()
    {
        float elapsed = 0f;
        int shownSeconds = Mathf.CeilToInt(RestartDelay);

        while (elapsed < RestartDelay)
        {
            if (PlayerHasGun())
            {
                DestroyText();
                Destroy(gameObject);
                yield break;
            }

            elapsed += Time.deltaTime;
            int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(RestartDelay - elapsed));
            if (secondsLeft != shownSeconds)
            {
                shownSeconds = secondsLeft;
                UpdateText(shownSeconds);
            }

            yield return null;
        }

        if (PlayerHasGun())
        {
            DestroyText();
            Destroy(gameObject);
            yield break;
        }

        DestroyText();

        return_to_game loader = FindObjectOfType<return_to_game>();
        string sceneName = SceneManager.GetActiveScene().name;

        if (loader != null)
        {
            loader.load_level(sceneName);
            yield break;
        }

        SceneManager.LoadScene(sceneName);
    }

    private bool PlayerHasGun()
    {
        if (_player == null)
        {
            _player = FindObjectOfType<Player_controller>();
        }

        if (_player == null)
        {
            return false;
        }

        DirectionalDragonBonesView view = _player.GetComponent<DirectionalDragonBonesView>();
        return view != null && view.HasGun;
    }

    private void CreateText()
    {
        if (_textObject != null)
        {
            return;
        }

        _textObject = new GameObject("Alarm_0_Message");
        _textObject.transform.position = transform.position + TextOffset;

        _text = _textObject.AddComponent<TextMeshPro>();
        _text.font = TMP_Settings.defaultFontAsset;
        _text.fontSize = 24f;
        _text.alignment = TextAlignmentOptions.Left;
        _text.color = Color.white;
        _text.outlineWidth = 0.2f;
        _text.outlineColor = Color.black;
        UpdateText(Mathf.CeilToInt(RestartDelay));

        MeshRenderer renderer = _text.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 20;
        }

        _textObject.transform.localScale = Vector3.one * 0.2f;
    }

    private void UpdateText(int secondsLeft)
    {
        if (_text == null)
        {
            return;
        }

        _text.text = $"You have {secondsLeft} seconds to pick up a weapon.";
    }

    private void DestroyText()
    {
        if (_textObject == null)
        {
            return;
        }

        Destroy(_textObject);
        _textObject = null;
        _text = null;
    }
}
