using UnityEngine;
using UnityEngine.UI;

public class MobileTouchControls : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float JoystickRadius = 118f;
    private const float JoystickTravel = 90f;

    private int _joystickFingerId = -1;
    private Vector2 _movement;
    private bool _attackPressed;
    private bool _dodgePressed;
    private Vector2 _joystickCenter;
    private float _uiScale;
    private RectTransform _joystickKnob;
    private Sprite _circleSprite;
    private GameUI_Manager _gameUIManager;

    public Vector2 Movement => _movement;

    private void Awake()
    {
        _gameUIManager = FindAnyObjectByType<GameUI_Manager>();
        CreateControls();
        UpdateLayout();
    }

    private void Update()
    {
        UpdateLayout();

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            Vector2 position = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                if (IsInside(position, new Vector2(Screen.width - 72f * _uiScale, Screen.height - 72f * _uiScale), 58f * _uiScale))
                {
                    if (_gameUIManager != null)
                    {
                        _gameUIManager.TogglePauseUI();
                    }
                }
                else if (IsInside(position, new Vector2(Screen.width - 220f * _uiScale, 240f * _uiScale), 90f * _uiScale))
                {
                    _attackPressed = Time.timeScale != 0f;
                }
                else if (IsInside(position, new Vector2(Screen.width - 100f * _uiScale, 125f * _uiScale), 76f * _uiScale))
                {
                    _dodgePressed = Time.timeScale != 0f;
                }
                else if (_joystickFingerId == -1 &&
                    IsInside(position, _joystickCenter, JoystickRadius * _uiScale))
                {
                    _joystickFingerId = touch.fingerId;
                    UpdateMovement(position);
                }
            }
            else if (touch.fingerId == _joystickFingerId)
            {
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _joystickFingerId = -1;
                    _movement = Vector2.zero;
                    _joystickKnob.anchoredPosition = Vector2.zero;
                }
                else
                {
                    UpdateMovement(position);
                }
            }
        }
    }

    public bool ConsumeAttack()
    {
        bool pressed = _attackPressed;
        _attackPressed = false;
        return pressed;
    }

    public bool ConsumeDodge()
    {
        bool pressed = _dodgePressed;
        _dodgePressed = false;
        return pressed;
    }

    public void ClearActionInputs()
    {
        _attackPressed = false;
        _dodgePressed = false;
    }

    private void UpdateMovement(Vector2 touchPosition)
    {
        Vector2 offset = (touchPosition - _joystickCenter) / (JoystickTravel * _uiScale);
        _movement = Vector2.ClampMagnitude(offset, 1f);
        _joystickKnob.anchoredPosition = _movement * JoystickTravel;
    }

    private void UpdateLayout()
    {
        _uiScale = Screen.height / ReferenceHeight;
        _joystickCenter = new Vector2(205f * _uiScale, 165f * _uiScale);
    }

    private static bool IsInside(Vector2 point, Vector2 center, float radius)
    {
        return (point - center).sqrMagnitude <= radius * radius;
    }

    private void CreateControls()
    {
        _circleSprite = CreateCircleSprite();

        GameObject canvasObject = new GameObject("Mobile Touch Controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, ReferenceHeight);
        scaler.matchWidthOrHeight = 1f;

        RectTransform root = canvasObject.GetComponent<RectTransform>();

        CreateCircle("Joystick", root, new Vector2(205f, 165f), Vector2.one * 236f, new Color(0.05f, 0.12f, 0.18f, 0.42f));
        _joystickKnob = CreateCircle("Joystick Knob", root, new Vector2(205f, 165f), Vector2.one * 104f, new Color(0.25f, 0.75f, 0.95f, 0.82f));
        CreateButton(root, "Attack", new Vector2(-220f, 240f), 176f, new Color(0.88f, 0.22f, 0.18f, 0.78f));
        CreateButton(root, "DODGE", new Vector2(-100f, 125f), 144f, new Color(0.20f, 0.55f, 0.84f, 0.78f));
        CreateButton(root, "II", new Vector2(-72f, -72f), 92f, new Color(0.05f, 0.12f, 0.18f, 0.68f), true);
    }

    private void OnDestroy()
    {
        if (_circleSprite != null)
        {
            Destroy(_circleSprite.texture);
            Destroy(_circleSprite);
        }
    }

    private RectTransform CreateCircle(string name, RectTransform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject circle = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = circle.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = name == "Joystick" || name == "Joystick Knob"
            ? Vector2.zero
            : new Vector2(1f, 0f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = circle.GetComponent<Image>();
        image.sprite = _circleSprite;
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private void CreateButton(RectTransform parent, string label, Vector2 position, float size, Color color, bool topRight = false)
    {
        RectTransform button = CreateCircle(label, parent, position, Vector2.one * size, color);
        if (topRight)
        {
            button.anchorMin = Vector2.one;
            button.anchorMax = Vector2.one;
        }

        GameObject textObject = new GameObject(label + " Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(button, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = label;
        text.fontSize = label == "Attack" ? 30 : 24;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    private static Sprite CreateCircleSprite()
    {
        const int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "Mobile Control Circle",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.5f;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
    }
}
