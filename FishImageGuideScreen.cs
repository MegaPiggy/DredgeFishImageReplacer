using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Winch.Components.UI;
using Winch.Util;

namespace FishImageReplacer;

public sealed class FishImageGuideScreen : MonoBehaviour
{
    private const float MinimumCardWidth = 340f;
    private const float CardHeight = 300f;
    private const float CardSpacing = 16f;

    private static readonly Color Background = Hex("#101010");
    private static readonly Color CardBackground = Hex("#1B1B1B");
    private static readonly Color InputBackground = Hex("#181818");
    private static readonly Color Border = Hex("#333333");
    private static readonly Color Text = Hex("#EEEEEE");
    private static readonly Color Muted = Hex("#999999");
    private static readonly Color Subtitle = Hex("#AAAAAA");
    private static readonly Color Hook = Hex("#E8C46D");
    private static readonly Color Aberration = Hex("#B89BE8");
    private static readonly Color Replacement = Hex("#8ED69B");

    private static FishImageGuideScreen _instance;
    public static FishImageGuideScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                Initialize();
            }

            return _instance;
        }
    }

    private readonly List<Card> _cards = new();

    private TMP_InputField _search;
    private RectTransform _viewport;
    private RectTransform _content;
    private GridLayoutGroup _grid;
    private TextMeshProUGUI _count;

    private int _lastScreenWidth;
    private int _lastScreenHeight;

    private DredgePlayerActionPress closeAction;

    public static void Initialize()
    {
        var root = new GameObject(
            "FishImageGuideScreen",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        DontDestroyOnLoad(root);

        _instance = root.AddComponent<FishImageGuideScreen>();
        _instance.Build();
    }

    public static void Show()
    {
        if (_instance == null)
        {
            Initialize();
        }

        _instance.gameObject.SetActive(true);
        _instance.RebuildCards();
        _instance._search.SetTextWithoutNotify(string.Empty);
        _instance.Filter(string.Empty);

        Canvas.ForceUpdateCanvases();
        _instance.UpdateGridLayout();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                _instance._search.gameObject);
        }
    }

    public static void Hide()
    {
        if (_instance != null)
            _instance.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_lastScreenWidth != Screen.width ||
            _lastScreenHeight != Screen.height)
        {
            UpdateGridLayout();
        }
    }

    private void OnEnable()
    {
        if (closeAction == null)
            return;

        GameManager.Instance.PauseListener.CanShowUnpauseAction(false);

        GameManager.Instance.Input.AddActionListener(
            new DredgePlayerActionBase[] { closeAction },
            ActionLayer.SYSTEM
        );
    }

    private void OnDisable()
    {
        if (closeAction == null ||
            GameManager.Instance == null ||
            GameManager.Instance.Input == null)
            return;

        GameManager.Instance.Input.RemoveActionListener(
            new DredgePlayerActionBase[] { closeAction },
            ActionLayer.SYSTEM
        );

        GameManager.Instance.PauseListener.CanShowUnpauseAction(true);
    }

    public void OnUnpausePressComplete()
    {
        Hide();
    }

    private void Build()
    {
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        var scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var rootRect = (RectTransform)transform;
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        var background = gameObject.AddComponent<Image>();
        background.color = Background;

        var title = CreateLocalizedText(
            transform,
            "Title",
            "megapiggy.fishimagereplacer.guide.title",
            34f,
            Text,
            FontStyles.Bold
        );

        SetRect(
            title.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -66f),
            new Vector2(700f, -24f));

        var subtitle = CreateLocalizedText(
            transform,
            "Subtitle",
            "megapiggy.fishimagereplacer.guide.subtitle",
            17f,
            Subtitle
        );

        SetRect(
            subtitle.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -98f),
            new Vector2(900f, -70f));

        var close = CreateButton(transform, "Close", "×", Hide);
        SetRect(
            (RectTransform)close.transform,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-76f, -68f),
            new Vector2(-32f, -24f));

        _search = CreateSearchField(transform);
        SetRect(
            (RectTransform)_search.transform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -154f),
            new Vector2(632f, -112f));

        _search.onValueChanged.AddListener(Filter);

        _count = CreateText(
            transform,
            "Count",
            string.Empty,
            14f,
            Muted);
        _count.alignment = TextAlignmentOptions.MidlineRight;

        SetRect(
            _count.rectTransform,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-332f, -154f),
            new Vector2(-32f, -112f));

        CreateScrollView();

        gameObject.SetActive(false);

        closeAction = new DredgePlayerActionPress(
            "prompt.leave",
            GameManager.Instance.Input.Controls.Unpause
        );

        closeAction.showInControlArea = true;
        closeAction.evaluateWhenPaused = true;

        closeAction.OnPressComplete += OnUnpausePressComplete;
    }

    private void CreateScrollView()
    {
        var scrollObject = CreateObject("Scroll View", transform);
        SetRect(
            (RectTransform)scrollObject.transform,
            Vector2.zero,
            Vector2.one,
            new Vector2(32f, 32f),
            new Vector2(-32f, -178f));

        var scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 45f;

        var viewportObject = CreateObject("Viewport", scrollObject.transform);
        _viewport = (RectTransform)viewportObject.transform;
        SetRect(
            _viewport,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);

        var viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.001f);

        viewportObject.AddComponent<RectMask2D>();

        var contentObject = CreateObject("Content", viewportObject.transform);
        _content = (RectTransform)contentObject.transform;
        _content.anchorMin = new Vector2(0f, 1f);
        _content.anchorMax = new Vector2(1f, 1f);
        _content.pivot = new Vector2(0.5f, 1f);
        _content.anchoredPosition = Vector2.zero;
        _content.sizeDelta = Vector2.zero;

        _grid = contentObject.AddComponent<GridLayoutGroup>();
        _grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        _grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        _grid.childAlignment = TextAnchor.UpperLeft;
        _grid.spacing = new Vector2(CardSpacing, CardSpacing);
        _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _grid.constraintCount = 1;
        _grid.cellSize = new Vector2(MinimumCardWidth, CardHeight);

        var fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = _viewport;
        scroll.content = _content;
    }

    private void RebuildCards()
    {
        foreach (var card in _cards)
        {
            if (card.Root != null)
                Destroy(card.Root);
        }

        _cards.Clear();

        foreach (var entry in FishImageReplacement.GetTextureMapEntries())
        {
            var sprite = FishImageReplacement.GetReplacementSprite(entry.itemId);
            var root = CreateCard(entry, sprite);

            var searchable = string.Join(
                " ",
                new[]
                {
                    entry.localizedName,
                    entry.englishName,
                    entry.itemId,
                    entry.aberrationOf,
                    entry.replacementFile
                }.Where(x => !string.IsNullOrWhiteSpace(x)))
                .ToLowerInvariant();

            _cards.Add(new Card
            {
                Root = root,
                Searchable = searchable
            });
        }

        Canvas.ForceUpdateCanvases();
        UpdateGridLayout();
        UpdateCount();
    }

    private GameObject CreateCard(
        FishImageReplacement.TextureMapEntry entry,
        Sprite replacementSprite)
    {
        var root = CreateObject(entry.itemId, _content);

        var background = root.AddComponent<Image>();
        background.color = CardBackground;
        background.raycastTarget = false;

        var outline = root.AddComponent<Outline>();
        outline.effectColor = Border;
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = false;

        var preview = CreateObject("Preview", root.transform);
        SetRect(
            (RectTransform)preview.transform,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(14f, -158f),
            new Vector2(-14f, -14f));

        var image = preview.AddComponent<Image>();
        image.sprite = replacementSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        var separator = CreateObject("Separator", root.transform);
        SetRect(
            (RectTransform)separator.transform,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -173f),
            new Vector2(0f, -172f));
        separator.AddComponent<Image>().color = Border;

        var name = CreateLocalizedText(
            root.transform,
            "Name",
            entry.localizedNameKey,
            20f,
            Text,
            FontStyles.Bold);
        name.overflowMode = TextOverflowModes.Ellipsis;
        name.enableWordWrapping = false;

        SetRect(
            name.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(16f, -211f),
            new Vector2(-16f, -181f));

        var id = CreateText(
            root.transform,
            "Id",
            entry.itemId,
            13f,
            Muted);
        id.enableWordWrapping = false;

        SetRect(
            id.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(16f, -231f),
            new Vector2(-16f, -211f));

        float rowTop = -242f;

        if (LocalizationSettings.SelectedLocale != null &&
            LocalizationSettings.SelectedLocale.Identifier.Code != "en")
        {
            AddLocalizedInfoRow(
                root.transform,
                ref rowTop,
                "English Name",
                "megapiggy.fishimagereplacer.guide.englishname",
                entry.englishName,
                Hook);
        }

        //AddInfoRow(
        //    root.transform,
        //    ref rowTop,
        //    "Texture",
        //    entry.sourceTexture,
        //    Hook);

        if (!string.IsNullOrWhiteSpace(entry.aberrationOf))
        {
            AddLocalizedInfoRow(
                root.transform,
                ref rowTop,
                "Aberration Of",
                "megapiggy.fishimagereplacer.guide.aberrationof",
                entry.aberrationOf,
                Aberration);
        }

        AddLocalizedInfoRow(
            root.transform,
            ref rowTop,
            "Replacement",
            "megapiggy.fishimagereplacer.guide.replacement",
            entry.replacementFile,
            Replacement,
            wrapValue: true
        );

        return root;
    }
    private static void AddInfoRow(
        Transform parent,
        ref float top,
        string label,
        string value,
        Color valueColor,
        bool wrapValue = false)
    {
        var labelText = CreateText(
            parent,
            label + " Label",
            label,
            12f,
            Muted
        );

        FinishInfoRow(
            parent,
            ref top,
            label,
            labelText,
            value,
            valueColor,
            wrapValue
        );
    }

    private static void AddLocalizedInfoRow(
        Transform parent,
        ref float top,
        string name,
        string labelKey,
        string value,
        Color valueColor,
        bool wrapValue = false)
    {
        var labelText = CreateLocalizedText(
            parent,
            name + " Label",
            labelKey,
            12f,
            Muted
        );

        FinishInfoRow(
            parent,
            ref top,
            name,
            labelText,
            value,
            valueColor,
            wrapValue
        );
    }

    private static void FinishInfoRow(
        Transform parent,
        ref float top,
        string name,
        TextMeshProUGUI labelText,
        string value,
        Color valueColor,
        bool wrapValue)
    {
        float height = wrapValue ? 36f : 18f;

        SetRect(
            labelText.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(16f, top - 18f),
            new Vector2(112f, top)
        );

        if (wrapValue && value != null)
        {
            value = value
                .Replace(".", ".\u200B")
                .Replace("-", "-\u200B");
        }

        var valueText = CreateText(
            parent,
            name + " Value",
            value ?? string.Empty,
            12f,
            valueColor
        );

        valueText.enableWordWrapping = wrapValue;
        valueText.overflowMode = wrapValue
            ? TextOverflowModes.Overflow
            : TextOverflowModes.Ellipsis;

        SetRect(
            valueText.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(120f, top - height),
            new Vector2(-16f, top)
        );

        top -= height + 2f;
    }

    private void Filter(string query)
    {
        query = (query ?? string.Empty)
            .Trim()
            .ToLowerInvariant();

        foreach (var card in _cards)
        {
            card.Root.SetActive(
                query.Length == 0 ||
                card.Searchable.Contains(query));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        UpdateCount();
    }

    private void UpdateCount()
    {
        int visible = _cards.Count(card => card.Root.activeSelf);
        _count.text = $"{visible} / {_cards.Count}";
    }

    private void UpdateGridLayout()
    {
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;

        if (_viewport == null || _grid == null)
            return;

        Canvas.ForceUpdateCanvases();

        float width = _viewport.rect.width;
        if (width <= 0f)
            return;

        int columns = Mathf.Max(
            1,
            Mathf.FloorToInt(
                (width + CardSpacing) /
                (MinimumCardWidth + CardSpacing)));

        float cardWidth =
            (width - CardSpacing * (columns - 1)) /
            columns;

        _grid.constraintCount = columns;
        _grid.cellSize = new Vector2(cardWidth, CardHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
    }

    private static TMP_InputField CreateSearchField(Transform parent)
    {
        var root = CreateObject("Search", parent);

        var background = root.AddComponent<Image>();
        background.color = InputBackground;

        var outline = root.AddComponent<Outline>();
        outline.effectColor = Border;
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = false;

        var textAreaObject = CreateObject("Text Area", root.transform);
        var textArea = (RectTransform)textAreaObject.transform;
        SetRect(
            textArea,
            Vector2.zero,
            Vector2.one,
            new Vector2(12f, 4f),
            new Vector2(-12f, -4f));

        textAreaObject.AddComponent<RectMask2D>();

        var placeholder = CreateLocalizedText(
            textArea,
            "Placeholder",
            "megapiggy.fishimagereplacer.guide.search",
            15f,
            Muted,
            FontStyles.Italic
        );
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        SetRect(
            placeholder.rectTransform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);

        var text = CreateText(
            textArea,
            "Text",
            string.Empty,
            15f,
            Text);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        SetRect(
            text.rectTransform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);

        var input = root.AddComponent<TMP_InputField>();
        input.targetGraphic = background;
        input.textViewport = textArea;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.richText = false;
        input.caretColor = Text;
        input.selectionColor = new Color(0.35f, 0.45f, 0.65f, 0.6f);

        return input;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        UnityEngine.Events.UnityAction onClick)
    {
        var root = CreateObject(name, parent);

        var image = root.AddComponent<Image>();
        image.color = InputBackground;

        var outline = root.AddComponent<Outline>();
        outline.effectColor = Border;
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = false;

        var button = root.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateText(
            root.transform,
            "Label",
            label,
            28f,
            Text);
        text.alignment = TextAlignmentOptions.Center;
        SetRect(
            text.rectTransform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);

        return button;
    }
    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string value,
        float size,
        Color color,
        FontStyles style = FontStyles.Normal)
    {
        var root = CreateObject(name, parent);
        var text = root.AddComponent<TextMeshProUGUI>();

        text.text = value ?? string.Empty;
        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;
        text.enableWordWrapping = true;

        var localizeFontBypass =
            root.AddComponent<LocalizeFontBypass>();

        localizeFontBypass.textField = text;
        localizeFontBypass.tableString = "Fonts";
        localizeFontBypass.tableEntryString = "DefaultFont";

        return text;
    }

    private static TextMeshProUGUI CreateLocalizedText(
        Transform parent,
        string name,
        string localizationKey,
        float size,
        Color color,
        FontStyles style = FontStyles.Normal)
    {
        return CreateLocalizedText(
            parent,
            name,
            LocalizationUtil.CreateReference(localizationKey),
            size,
            color,
            style
        );
    }

    private static TextMeshProUGUI CreateLocalizedText(
        Transform parent,
        string name,
        LocalizedString localizedString,
        float size,
        Color color,
        FontStyles style = FontStyles.Normal)
    {
        var text = CreateText(
            parent,
            name,
            string.Empty,
            size,
            color,
            style
        );

        var localizeStringEvent =
            text.gameObject.AddComponent<LocalizeStringEvent>();

        localizeStringEvent.OnUpdateString.AddListener(
            value => text.text = value
        );

        var localizedLabel =
            text.gameObject.AddComponent<LocalizedLabel>();

        localizedLabel.LabelString = localizedString;

        return text;
    }

    private static GameObject CreateObject(string name, Transform parent)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out var color);
        return color;
    }

    private sealed class Card
    {
        public GameObject Root;
        public string Searchable;
    }
}
