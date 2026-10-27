using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>อยากให้กล่องไปอยู่ด้านไหนของปุ่ม/แผงที่ชี้ — ⚠️ เพิ่มค่าใหม่ให้**ต่อท้ายเท่านั้น** (เซฟเป็น index)</summary>
public enum TutorialBoxSide
{
    /// <summary>เลือกด้านที่มีที่ว่างพอให้เอง — ไล่ ล่าง → บน → ขวา → ซ้าย</summary>
    Auto,
    Below,
    Above,
    Right,
    Left,
}

/// <summary>กล่องคำอธิบายของบทสอน — แบบตัวละครคุยกัน: **รูปเล็กตัวละคร + ชื่อ + คำอธิบาย**
///
/// **แปะที่ root ของ prefab กล่อง** แล้วลากของในกล่องใส่ช่องข้างล่าง (เว้นว่างช่องไหน = ซ่อนส่วนนั้นให้เอง)
/// → ลาก prefab ไปใส่ช่อง <c>dialogPrefab</c> ของ <see cref="TutorialManager"/>
/// (จะวางกล่องไว้ในซีนเลยก็ได้ — ปิด GameObject ไว้ แล้วปล่อยช่อง <c>dialogPrefab</c> ว่าง)
///
/// เปลี่ยนภาษากลางเกม ข้อความในกล่องเปลี่ยนตามเอง</summary>
public class TutorialDialog : MonoBehaviour
{
    [Header("--- ตัวที่เปิด-ปิด ---")]
    [Tooltip("เว้นว่าง = ใช้ GameObject ตัวเอง")]
    public GameObject root;

    [Header("--- ช่องในกล่อง (เว้นว่าง = ซ่อนส่วนนั้น) ---")]
    [Tooltip("รูปเล็กของตัวละคร")]
    public Image portraitImage;
    [Tooltip("ชื่อคนพูด — สเต็ปที่ไม่ได้กรอกชื่อ = ซ่อนบรรทัดนี้ (ข้อความระบบ)")]
    public TextMeshProUGUI nameText;
    [Tooltip("คำอธิบาย")]
    public TextMeshProUGUI descriptionText;
    [Tooltip("ชื่อปุ่มที่บอกให้กด เช่น [ W / A / S / D ]")]
    public TextMeshProUGUI keyText;
    [Tooltip("บอกว่าสเต็ปที่เท่าไหร่ เช่น 2 / 5")]
    public TextMeshProUGUI stepCounterText;

    [Header("--- ปุ่ม (เว้นว่างได้ทั้งคู่) ---")]
    [Tooltip("ปุ่ม \"ถัดไป\" — โผล่เฉพาะสเต็ปแบบ Message\n" +
             "สเต็ปที่ติ๊ก waitForTargetButton ไว้: กดแล้ว **แค่เก็บกล่องให้พ้นทาง** ยังไม่ไปสเต็ปถัดไป")]
    public Button nextButton;
    [Tooltip("ปุ่ม \"ข้าม\" — ข้ามทั้งบท")]
    public Button skipButton;

    [Header("--- จอมืดรอบปุ่ม (สร้างให้เองตอนรัน) ---")]
    [Tooltip("★ ทำที่อื่นนอกจากปุ่มที่ต้องกดให้มืดลง — เจาะรูไว้ตรงปุ่มเท่านั้น")]
    public bool dimOutside = true;
    public Color dimColor = new Color(0f, 0f, 0f, 0.65f);
    [Tooltip("กดตรงที่มืดไม่ได้ — ผู้เล่นกดได้เฉพาะในรู (ปิด = มืดเฉย ๆ กดทะลุได้)")]
    public bool blockClickOutside = true;

    [Header("--- กรอบไฮไลต์ปุ่มที่ต้องกด (สร้างให้เองตอนรัน) ---")]
    [Tooltip("★ ตีกรอบรอบปุ่มที่สเต็ปนั้นสั่งให้กด (type = ClickButton หรือ Message ที่ติ๊ก waitForTargetButton)")]
    public bool highlightTarget = true;
    public Color highlightColor = new Color(1f, 0.82f, 0.3f, 1f);
    [Min(1f)] public float highlightThickness = 4f;
    [Tooltip("กรอบห่างจากขอบปุ่มกี่พิกเซล")]
    public float highlightPadding = 8f;
    [Tooltip("กรอบกะพริบช้า ๆ ให้สะดุดตา")]
    public bool highlightPulse = true;

    [Header("--- ที่วางกล่อง (ย้ายไปอยู่ข้าง ๆ ปุ่ม/แผงที่ชี้) ---")]
    [Tooltip("★ ย้ายกล่องไปอยู่ใกล้ targetButton/targetPanel ของสเต็ปนั้น — **ไม่ทับตัวมัน และไม่ล้นออกนอกจอ**\n" +
             "สเต็ปที่ไม่ได้ชี้อะไร (หรือปิดช่องนี้) = กล่องอยู่ที่เดิมตามที่วางไว้ใน prefab")]
    public bool followTarget = true;
    [Tooltip("กล่องห่างจากขอบกรอบไฮไลต์กี่พิกเซล")]
    [Min(0f)] public float targetGap = 24f;
    [Tooltip("กล่องห่างจากขอบจออย่างน้อยกี่พิกเซล")]
    [Min(0f)] public float screenMargin = 16f;
    [Tooltip("อยากให้ไปอยู่ด้านไหนของปุ่ม — Auto = เลือกด้านที่ว่างพอให้เอง (ล่าง → บน → ขวา → ซ้าย)\n" +
             "ด้านที่เลือกไว้ไม่พอที่ = ย้ายไปด้านอื่นให้เอง ไม่ปล่อยให้ล้นจอ")]
    public TutorialBoxSide preferSide = TutorialBoxSide.Auto;

    [Header("--- รูปแบบข้อความ ---")]
    [Tooltip("{0} = ชื่อปุ่ม")]
    public string keyTextFormat = "[ {0} ]";
    [Tooltip("{0} = สเต็ปที่, {1} = ทั้งหมด")]
    public string counterFormat = "{0} / {1}";
    [Tooltip("โชว์ปุ่มถัดไปทุกสเต็ป (ทางหนีเผื่อผู้เล่นติด) — ปิดไว้ = โชว์เฉพาะสเต็ปแบบ Message")]
    public bool alwaysShowNextButton;

    TutorialManager _owner;
    TutorialClick _step;
    int _stepNumber, _stepCount;
    bool _showNext;

    // สเต็ป Message ที่ติ๊ก waitForTargetButton ไว้ — "ถัดไป" = เก็บกล่อง ไม่ใช่จบสเต็ป
    bool _dismissed;              // เก็บกล่องไปแล้ว แต่สเต็ปยังทำงานอยู่ (กรอบไฮไลต์ยังตามปุ่ม + จอมืดหายไป)
    CanvasGroup _boxGroup;        // ใส่ให้เองตอนต้องเก็บกล่องครั้งแรก (ไม่แตะ prefab ที่ไม่ได้ใช้ฟีเจอร์นี้)
    float _boxAlpha = 1f;         // alpha เดิมของกล่อง เผื่อ prefab ตั้ง CanvasGroup ไว้เอง

    // กรอบไฮไลต์ = Image บาง ๆ 4 แผ่นล้อมปุ่มไว้ (ไม่ใช้ shader) — สร้างไว้ใต้ Canvas ไม่ใช่ใต้ root
    // (root ถูก SetActive(false) ตอนปิดกล่อง → LateUpdate จะไม่ทำงาน)
    static readonly Vector3[] Corners = new Vector3[4];
    static readonly List<Graphic> BoxGraphics = new List<Graphic>();   // ใช้ซ้ำ ไม่ให้ LateUpdate แจก garbage
    RectTransform _highlightRoot;
    readonly Image[] _dimPanels = new Image[4];      // ล่าง บน ซ้าย ขวา — ล้อมรูไว้
    readonly Image[] _highlightBars = new Image[4];
    RectTransform _target;
    RectTransform _runtimeTarget;    // ปุ่มที่เพิ่งถูกเสกแล้ว TutorialManager หาเจอ (สลับภาษาแล้วต้องไม่หาย)

    RectTransform _canvasRect;
    Vector2 _homePosition;                           // ที่เดิมของกล่องตามที่วางไว้ใน prefab
    bool _homeSaved;

    /// <summary>ตัวที่ถูกย้าย = ตัวเดียวกับที่เปิด-ปิด</summary>
    RectTransform BoxRect => (root != null ? root : gameObject).transform as RectTransform;

    void Awake()
    {
        if (root == null) root = gameObject;

        // จำที่เดิมไว้ก่อนขยับ — สเต็ปที่ไม่ได้ชี้อะไรจะได้กลับมาอยู่ที่นี่
        RectTransform box = BoxRect;
        if (box != null)
        {
            _homePosition = box.anchoredPosition;
            _homeSaved = true;
        }

        if (nextButton != null) nextButton.onClick.AddListener(HandleNext);
        if (skipButton != null) skipButton.onClick.AddListener(HandleSkip);

        UISfxManager.BindButtons(gameObject);     // เสียงกดปุ่มมาตรฐานของเกม
        Hide();
    }

    void OnEnable() { }

    void OnDisable() { }

    void OnDestroy()
    {
        if (nextButton != null) nextButton.onClick.RemoveListener(HandleNext);
        if (skipButton != null) skipButton.onClick.RemoveListener(HandleSkip);

        // กรอบอยู่ใต้ Canvas ไม่ใช่ใต้กล่อง → ต้องเก็บเอง ไม่งั้นค้างบนจอ
        if (_highlightRoot != null) Destroy(_highlightRoot.gameObject);
    }

    /// <summary>บอกกล่องว่าบทไหนเป็นเจ้าของอยู่ — <see cref="TutorialManager"/> เรียกให้เอง</summary>
    public void Bind(TutorialManager owner) => _owner = owner;

    /// <summary>เปิดกล่องแล้วเติมข้อมูลของสเต็ปนั้น</summary>
    public void Show(TutorialClick step, int stepNumber, int stepCount, bool showNext)
    {
        // สเต็ปใหม่ = ลืมปุ่มที่เคยหาเจอ · สเต็ปเดิม (สลับภาษากลางสเต็ป) = เก็บไว้ ไม่งั้นกรอบหลุดจากปุ่ม
        if (!ReferenceEquals(_step, step)) _runtimeTarget = null;

        _step = step;
        _stepNumber = stepNumber;
        _stepCount = stepCount;
        _showNext = showNext;
        if (step == null) { Hide(); return; }

        if (root == null) root = gameObject;
        root.SetActive(true);

        // รูปเล็กตัวละคร — ไม่ใส่รูป = ซ่อนช่องไปเลย
        if (portraitImage != null)
        {
            portraitImage.sprite = step.portrait;
            portraitImage.gameObject.SetActive(step.portrait != null);
        }

        int lang = 0;

        // ชื่อคนพูด — ไม่กรอกสักภาษา = ข้อความระบบ ซ่อนบรรทัดชื่อ
        if (nameText != null)
        {
            string speaker = step.GetSpeaker(lang);
            nameText.text = speaker;
            nameText.gameObject.SetActive(!string.IsNullOrWhiteSpace(speaker));
        }

        if (descriptionText != null)
        {
            descriptionText.text = step.GetText(lang);
        }

        if (keyText != null)
        {
            string keyLabel = step.GetKeyLabel();
            bool hasKey = !string.IsNullOrWhiteSpace(keyLabel);
            keyText.text = hasKey ? string.Format(keyTextFormat, keyLabel) : "";
            keyText.gameObject.SetActive(hasKey);
        }

        if (stepCounterText != null)
        {
            stepCounterText.text = string.Format(counterFormat, stepNumber, stepCount);
            stepCounterText.gameObject.SetActive(stepCount > 1);
        }

        if (nextButton != null)
            nextButton.gameObject.SetActive(showNext || alwaysShowNextButton);

        bool waitsButton = step.WaitsForTargetButton;
        _dismissed = false;                 // กล่องใหม่ = โผล่เต็มตัวเสมอ (จอมืดกลับมาด้วย)
        ApplyDismissed();

        // เป้าของกรอบ/จอมืด: แผงที่ระบุไว้มาก่อน ไม่งั้นใช้ปุ่มที่สเต็ปนี้สั่งให้กด
        // ★ ปุ่มที่เสกทีหลัง (targetParent) ตอนนี้อาจยังไม่เกิด → ไม่มีเป้า = ไม่มีกรอบ ไม่มีจอมืด
        //   TutorialManager จะเรียก SetButtonTarget() ให้เองทันทีที่หาปุ่มเจอ
        if (step.targetPanel != null)
            _target = step.targetPanel;
        else if (step.targetButton != null &&
                 (step.type == TutorialStepType.ClickButton || waitsButton))
            _target = step.targetButton.transform as RectTransform;
        else
            _target = _runtimeTarget;

        // ข้อความเพิ่งเปลี่ยน → บังคับคำนวณขนาดกล่องใหม่เดี๋ยวนี้ ไม่งั้นเฟรมแรกวางด้วยขนาดเก่า (กล่องกระตุก)
        RectTransform box = BoxRect;
        if (box != null) LayoutRebuilder.ForceRebuildLayoutImmediate(box);

        Refresh();
    }

    /// <summary>★ ปุ่มที่ต้องกดเพิ่งถูกเสกออกมา — ย้ายกรอบไฮไลต์/จอมืดมาเกาะตัวนี้
    /// (<see cref="TutorialManager"/> เรียกให้เองตอนหาปุ่มใน <c>targetParent</c> เจอ)
    ///
    /// <para>สเต็ปที่ระบุ <c>targetPanel</c> ไว้ = ชี้แผงตามเดิม ปุ่มไม่แย่งเป้า</para></summary>
    public void SetButtonTarget(Button button)
    {
        _runtimeTarget = button != null ? button.transform as RectTransform : null;

        if (_step == null || _step.targetPanel != null) return;
        if (_step.targetButton != null) return;          // ลากปุ่มมาเองแล้ว ตัวนั้นมาก่อน

        _target = _runtimeTarget;
    }

    /// <summary>★ เก็บเฉพาะ **กล่องข้อความ** ให้พ้นทาง — **สเต็ปยังไม่จบ** กรอบไฮไลต์ยังตามปุ่มอยู่
    ///
    /// สเต็ป Message ที่ติ๊ก <c>waitForTargetButton</c>: กด "ถัดไป" = เรียกตัวนี้
    /// (ไม่ใช่ <see cref="Hide"/> เพราะต้องให้ <c>LateUpdate</c> เดินต่อเพื่อวาดกรอบรอบปุ่ม)</summary>
    public void DismissBox()
    {
        if (_step == null || _dismissed) return;

        _dismissed = true;
        ApplyDismissed();
    }

    /// <summary>ซ่อน/โชว์กล่องด้วย CanvasGroup — **ไม่ใช้ SetActive** เพราะสคริปต์นี้อยู่บนตัวกล่อง
    /// ปิด GameObject แล้ว <c>LateUpdate</c> จะหยุด กรอบไฮไลต์จะค้างอยู่กับที่</summary>
    void ApplyDismissed()
    {
        // ไม่เคยเก็บกล่องเลย = ไม่ต้องไปใส่ CanvasGroup ให้ prefab เปล่า ๆ
        if (!_dismissed && _boxGroup == null) return;

        CanvasGroup group = EnsureBoxGroup();
        if (group == null) return;

        group.alpha = _dismissed ? 0f : _boxAlpha;
        group.interactable = !_dismissed;
        group.blocksRaycasts = !_dismissed;
    }

    CanvasGroup EnsureBoxGroup()
    {
        if (_boxGroup != null) return _boxGroup;

        GameObject go = root != null ? root : gameObject;
        _boxGroup = go.GetComponent<CanvasGroup>();
        if (_boxGroup == null) _boxGroup = go.AddComponent<CanvasGroup>();
        else _boxAlpha = _boxGroup.alpha;      // prefab ตั้ง alpha เองไว้ → คืนค่านั้น ไม่ใช่ 1 ดื้อ ๆ

        return _boxGroup;
    }

    public void Hide()
    {
        _step = null;
        _target = null;
        _runtimeTarget = null;
        _dismissed = false;
        ApplyDismissed();                      // คืนกล่องให้มองเห็นได้ก่อน เผื่อสเต็ปหน้าใช้กล่องตัวเดิม
        ShowHighlightBars(false);
        if (root == null) root = gameObject;
        root.SetActive(false);
    }

    // ─── กรอบไฮไลต์ + ที่วางกล่อง ───────────────────────────────────────────

    // วาดทุกเฟรมหลัง layout → ปุ่มที่ขยับ/แผงที่เพิ่งเปิดได้กรอบ+กล่องตรงตำแหน่งจริง
    void LateUpdate() => Refresh();

    void Refresh()
    {
        bool hasTarget = TryGetTargetRect(out Rect target);
        UpdateHighlight(hasTarget, target);
        UpdateBoxPosition(hasTarget, target);
    }

    /// <summary>กรอบของเป้า (รวม <see cref="highlightPadding"/>) ในพิกัด local ของ Canvas —
    /// false = สเต็ปนี้ไม่ได้ชี้อะไร / เป้าถูกปิดอยู่ / หา Canvas ไม่เจอ</summary>
    bool TryGetTargetRect(out Rect rect)
    {
        rect = default;
        if (_target == null || !_target.gameObject.activeInHierarchy) return false;

        RectTransform canvasRect = ResolveCanvas();
        if (canvasRect == null) return false;
        if (!TryGetLocalRect(_target, canvasRect, out rect)) return false;

        rect = new Rect(rect.x - highlightPadding, rect.y - highlightPadding,
                        rect.width + highlightPadding * 2f, rect.height + highlightPadding * 2f);
        return true;
    }

    void UpdateHighlight(bool hasTarget, Rect target)
    {
        if (!hasTarget || !highlightTarget)
        {
            ShowHighlightBars(false);
            return;
        }

        RectTransform canvasRect = ResolveCanvas();
        EnsureHighlightBuilt();
        if (_highlightRoot == null || canvasRect == null) { ShowHighlightBars(false); return; }

        // พิกัด Canvas → พิกัดของ _highlightRoot (ลูกที่ anchor กลาง Canvas)
        Rect r = new Rect(target.position - canvasRect.rect.center, target.size);

        UpdateDim(canvasRect, r);

        float t = highlightThickness;
        SetBar(0, new Vector2(r.center.x, r.yMin + t * 0.5f), new Vector2(r.width, t));            // ล่าง
        SetBar(1, new Vector2(r.center.x, r.yMax - t * 0.5f), new Vector2(r.width, t));            // บน
        SetBar(2, new Vector2(r.xMin + t * 0.5f, r.center.y), new Vector2(t, r.height - t * 2f));  // ซ้าย
        SetBar(3, new Vector2(r.xMax - t * 0.5f, r.center.y), new Vector2(t, r.height - t * 2f));  // ขวา

        Color c = highlightColor;
        if (highlightPulse)
            c.a *= Mathf.Lerp(0.45f, 1f, Mathf.PingPong(Time.unscaledTime * 1.5f, 1f));

        ShowHighlightBars(true);
        foreach (Image bar in _highlightBars)
            if (bar != null) bar.color = c;

        KeepDrawOrder();
    }

    /// <summary>จอมืด/กรอบ อยู่บนสุดของ Canvas — แต่ **กล่องข้อความต้องอยู่เหนือขึ้นไปอีก**
    /// ไม่งั้นแผ่นมืดทับกล่องจนอ่านไม่ออก + กดปุ่มถัดไป/ข้ามไม่ได้</summary>
    void KeepDrawOrder()
    {
        _highlightRoot.SetAsLastSibling();

        Transform canvasT = _highlightRoot.parent;
        Transform box = (root != null ? root : gameObject).transform;
        while (box != null && box.parent != canvasT) box = box.parent;

        if (box != null) box.SetAsLastSibling();
    }

    void SetBar(int index, Vector2 center, Vector2 size)
    {
        RectTransform rt = _highlightBars[index].rectTransform;
        rt.anchoredPosition = center;
        rt.sizeDelta = new Vector2(Mathf.Max(0f, size.x), Mathf.Max(0f, size.y));
    }

    /// <summary>จอมืด = แผ่นทึบ 4 แผ่นล้อมรูไว้ (ไม่ใช้ shader) → กดในรูโดนปุ่มจริงข้างใต้</summary>
    void UpdateDim(RectTransform canvasRect, Rect hole)
    {
        // ผู้เล่นเก็บกล่องไปแล้ว (สเต็ปที่รอ targetButton) = เก็บจอมืดตามไปด้วย
        // ไม่งั้นอ่านจบปิดกล่องแล้วยังโดนบังอยู่ กดปุ่มอื่นไม่ได้ — กรอบไฮไลต์ยังอยู่ ไม่ได้หายไปไหน
        if (!dimOutside || _dismissed)
        {
            foreach (Image panel in _dimPanels)
                if (panel != null && panel.gameObject.activeSelf) panel.gameObject.SetActive(false);
            return;
        }

        Vector2 size = canvasRect.rect.size;
        float left = -size.x * 0.5f, right = size.x * 0.5f;
        float bottom = -size.y * 0.5f, top = size.y * 0.5f;

        // รูที่ล้นออกนอกจอ = ตัดให้อยู่ในจอก่อน ไม่งั้นแผ่นมืดได้ขนาดติดลบ
        float x0 = Mathf.Clamp(hole.xMin, left, right), x1 = Mathf.Clamp(hole.xMax, left, right);
        float y0 = Mathf.Clamp(hole.yMin, bottom, top), y1 = Mathf.Clamp(hole.yMax, bottom, top);

        SetDim(0, new Vector2(0f, (bottom + y0) * 0.5f), new Vector2(size.x, y0 - bottom));
        SetDim(1, new Vector2(0f, (y1 + top) * 0.5f), new Vector2(size.x, top - y1));
        SetDim(2, new Vector2((left + x0) * 0.5f, (y0 + y1) * 0.5f), new Vector2(x0 - left, y1 - y0));
        SetDim(3, new Vector2((x1 + right) * 0.5f, (y0 + y1) * 0.5f), new Vector2(right - x1, y1 - y0));
    }

    void SetDim(int index, Vector2 center, Vector2 size)
    {
        Image panel = _dimPanels[index];
        if (panel == null) return;

        if (!panel.gameObject.activeSelf) panel.gameObject.SetActive(true);
        panel.color = dimColor;
        panel.raycastTarget = blockClickOutside;

        RectTransform rt = panel.rectTransform;
        rt.anchoredPosition = center;
        rt.sizeDelta = new Vector2(Mathf.Max(0f, size.x), Mathf.Max(0f, size.y));
    }

    void ShowHighlightBars(bool show)
    {
        if (_highlightRoot != null && _highlightRoot.gameObject.activeSelf != show)
            _highlightRoot.gameObject.SetActive(show);
    }

    /// <summary>Canvas ที่ใช้เป็น "จอ" — กรอบ/จอมืดไปอยู่ใต้ตัวนี้ และกล่องห้ามล้นขอบของตัวนี้</summary>
    RectTransform ResolveCanvas()
    {
        if (_canvasRect != null) return _canvasRect;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return null;

        // กล่องมี Canvas ของตัวเอง (override sorting) → ใช้ Canvas ใหญ่แทน
        // ไม่งั้น "ขอบจอ" กลายเป็นขอบกล่องเอง แล้วกล่องจะวางตัวเองไม่ได้เลย
        Transform box = (root != null ? root : gameObject).transform;
        if (canvas.transform == box || canvas.transform.IsChildOf(box)) canvas = canvas.rootCanvas;

        _canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        return _canvasRect;
    }

    void EnsureHighlightBuilt()
    {
        if (_highlightRoot != null) return;

        RectTransform canvas = ResolveCanvas();
        if (canvas == null) return;

        var go = new GameObject("TutorialHighlight", typeof(RectTransform));
        _highlightRoot = (RectTransform)go.transform;
        _highlightRoot.SetParent(canvas, false);
        _highlightRoot.anchorMin = _highlightRoot.anchorMax = new Vector2(0.5f, 0.5f);
        _highlightRoot.pivot = new Vector2(0.5f, 0.5f);
        _highlightRoot.anchoredPosition = Vector2.zero;
        _highlightRoot.sizeDelta = Vector2.zero;

        // แผ่นมืดสร้างก่อน = อยู่หลังกรอบ (ลำดับลูก = ลำดับการวาด)
        for (int i = 0; i < 4; i++)
            _dimPanels[i] = NewImage($"Dim{i}", true);

        for (int i = 0; i < 4; i++)
            _highlightBars[i] = NewImage($"Bar{i}", false);   // ★ กรอบห้ามรับคลิก ไม่งั้นบังปุ่มที่สั่งให้กด
    }

    Image NewImage(string name, bool raycastTarget)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(_highlightRoot, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Image image = go.GetComponent<Image>();
        image.raycastTarget = raycastTarget;
        return image;
    }

    // ─── ย้ายกล่องไปอยู่ข้าง ๆ เป้า ──────────────────────────────────────────

    static readonly TutorialBoxSide[] AutoOrder =
    {
        TutorialBoxSide.Below, TutorialBoxSide.Above, TutorialBoxSide.Right, TutorialBoxSide.Left,
    };

    /// <summary>วางกล่องชิดเป้าแต่**ไม่ทับ** และ**ไม่ล้นจอ** — ไม่มีเป้า = กลับไปที่เดิมใน prefab</summary>
    void UpdateBoxPosition(bool hasTarget, Rect target)
    {
        if (!followTarget) return;

        RectTransform box = BoxRect;
        if (box == null) return;

        if (!hasTarget)
        {
            if (_homeSaved && box.anchoredPosition != _homePosition) box.anchoredPosition = _homePosition;
            return;
        }

        RectTransform canvasRect = ResolveCanvas();
        if (canvasRect == null) return;
        if (!TryGetVisualRect(box, canvasRect, out Rect boxRect)) return;

        Rect screen = canvasRect.rect;
        Rect safe = Rect.MinMaxRect(screen.xMin + screenMargin, screen.yMin + screenMargin,
                                    screen.xMax - screenMargin, screen.yMax - screenMargin);
        Vector2 size = boxRect.size;

        // ไม่เหลือด้านไหนที่กล่องลงพอดีเลย (กล่องใหญ่/ปุ่มกลางจอ) → เอาด้านที่ว่างสุดแล้วยัดให้อยู่ในจอไว้ก่อน
        if (!TryPickSide(target, safe, size, out Vector2 center))
            center = ClampInside(WidestSide(target, safe, size), safe, size);

        Vector2 delta = center - boxRect.center;
        if (delta.sqrMagnitude < 0.01f) return;

        // ขยับผ่าน world → ใช้ได้ไม่ว่ากล่องจะ anchor/pivot แบบไหน หรืออยู่ลึกใต้แผงอื่น
        box.position += canvasRect.TransformVector(delta);
    }

    bool TryPickSide(Rect target, Rect safe, Vector2 size, out Vector2 center)
    {
        // ด้านที่เลือกไว้ก่อน — ไม่พอที่ค่อยไล่หาด้านอื่นให้
        if (preferSide != TutorialBoxSide.Auto && TrySide(preferSide, target, safe, size, out center))
            return true;

        foreach (TutorialBoxSide side in AutoOrder)
            if (TrySide(side, target, safe, size, out center)) return true;

        center = Vector2.zero;
        return false;
    }

    /// <summary>กล่องลงด้านนี้ได้ไหม — ได้ = คืนจุดกลางกล่องที่ชิดเป้าที่สุดโดยยังอยู่ในจอ</summary>
    bool TrySide(TutorialBoxSide side, Rect target, Rect safe, Vector2 size, out Vector2 center)
    {
        center = Vector2.zero;
        float need;

        switch (side)
        {
            case TutorialBoxSide.Below:
                need = target.yMin - targetGap - size.y;
                if (need < safe.yMin) return false;
                center = new Vector2(target.center.x, need + size.y * 0.5f);
                break;

            case TutorialBoxSide.Above:
                need = target.yMax + targetGap + size.y;
                if (need > safe.yMax) return false;
                center = new Vector2(target.center.x, need - size.y * 0.5f);
                break;

            case TutorialBoxSide.Right:
                need = target.xMax + targetGap + size.x;
                if (need > safe.xMax) return false;
                center = new Vector2(need - size.x * 0.5f, target.center.y);
                break;

            default:   // Left
                need = target.xMin - targetGap - size.x;
                if (need < safe.xMin) return false;
                center = new Vector2(need + size.x * 0.5f, target.center.y);
                break;
        }

        // เลื่อนได้เฉพาะแกนขวาง — แกนที่ชี้เข้าหาเป้าห้ามขยับ ไม่งั้นเลื่อนไปทับเป้า
        bool vertical = side == TutorialBoxSide.Below || side == TutorialBoxSide.Above;
        if (vertical) center.x = ClampAxis(center.x, size.x, safe.xMin, safe.xMax);
        else          center.y = ClampAxis(center.y, size.y, safe.yMin, safe.yMax);

        return true;
    }

    /// <summary>ด้านที่เหลือที่ว่างมากสุด — ใช้ตอนไม่มีด้านไหนลงพอดี (ยอมทับนิดหน่อยดีกว่าหลุดจอ)</summary>
    static Vector2 WidestSide(Rect target, Rect safe, Vector2 size)
    {
        float below = target.yMin - safe.yMin;
        float above = safe.yMax - target.yMax;
        float right = safe.xMax - target.xMax;
        float left = target.xMin - safe.xMin;
        float best = Mathf.Max(Mathf.Max(below, above), Mathf.Max(right, left));

        if (best <= below) return new Vector2(target.center.x, safe.yMin + size.y * 0.5f);
        if (best <= above) return new Vector2(target.center.x, safe.yMax - size.y * 0.5f);
        if (best <= right) return new Vector2(safe.xMax - size.x * 0.5f, target.center.y);
        return new Vector2(safe.xMin + size.x * 0.5f, target.center.y);
    }

    /// <summary>ขนาด/ที่อยู่จริงของกล่องบนจอ = **กรอบรวมของทุกชิ้นที่มองเห็นอยู่** (Image/Text ที่เปิดอยู่)
    /// ไม่ใช่ rect ของ root
    ///
    /// ⚠️ root ของ prefab กล่องมักเป็น RectTransform **เปล่า 100x100** ที่ไม่ได้ครอบอะไรเลย
    /// (ของจริงเป็นลูกที่ถูกเลื่อนออกไปหลายร้อยพิกเซล) — วัดจาก root ตรง ๆ จะได้ทั้งขนาดและจุดกึ่งกลางผิด
    /// แล้วกล่องจริงจะไปโผล่นอกจอ · ช่องที่ถูกซ่อน (รูป/ชื่อ/ปุ่มถัดไป) ไม่ถูกนับด้วย กรอบเลยหดตามจริง</summary>
    static bool TryGetVisualRect(RectTransform box, RectTransform space, out Rect rect)
    {
        rect = default;

        BoxGraphics.Clear();
        box.GetComponentsInChildren(false, BoxGraphics);       // false = เอาเฉพาะตัวที่เปิดอยู่

        Matrix4x4 toSpace = space.worldToLocalMatrix;
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        bool any = false;

        foreach (Graphic g in BoxGraphics)
        {
            if (g == null || !g.enabled) continue;
            Encapsulate(g.rectTransform, toSpace, ref min, ref max);
            any = true;
        }

        if (!any) Encapsulate(box, toSpace, ref min, ref max);   // กล่องเปล่า → ใช้ rect ของ root ไปก่อน

        if (max.x <= min.x || max.y <= min.y) return false;

        rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }

    static void Encapsulate(RectTransform rt, Matrix4x4 toSpace, ref Vector2 min, ref Vector2 max)
    {
        rt.GetWorldCorners(Corners);
        for (int i = 0; i < 4; i++)
        {
            Vector3 p = toSpace.MultiplyPoint3x4(Corners[i]);
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
    }

    static Vector2 ClampInside(Vector2 center, Rect safe, Vector2 size) =>
        new Vector2(ClampAxis(center.x, size.x, safe.xMin, safe.xMax),
                    ClampAxis(center.y, size.y, safe.yMin, safe.yMax));

    static float ClampAxis(float center, float size, float min, float max)
    {
        if (max - min <= size) return (min + max) * 0.5f;    // กล่องกว้างกว่าจอ = อยู่กลางไว้
        float half = size * 0.5f;
        return Mathf.Clamp(center, min + half, max - half);
    }

    /// <summary>หากรอบของ <paramref name="target"/> ในพิกัด local ของ <paramref name="space"/></summary>
    static bool TryGetLocalRect(RectTransform target, RectTransform space, out Rect rect)
    {
        rect = default;

        Canvas targetCanvas = target.GetComponentInParent<Canvas>();
        Canvas spaceCanvas = space.GetComponentInParent<Canvas>();
        if (targetCanvas == null || spaceCanvas == null) return false;

        Camera targetCam = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : targetCanvas.worldCamera;
        Camera spaceCam = spaceCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : spaceCanvas.worldCamera;

        target.GetWorldCorners(Corners);

        Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
        for (int i = 0; i < 4; i++)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(targetCam, Corners[i]);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(space, screen, spaceCam, out Vector2 local))
                return false;

            min = Vector2.Min(min, local);
            max = Vector2.Max(max, local);
        }

        if (max.x <= min.x || max.y <= min.y) return false;

        rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }

    void HandleNext()
    {
        if (_owner != null) _owner.OnDialogNext();
    }

    void HandleSkip()
    {
        if (_owner != null) _owner.Skip();
    }

    void HandleLanguageChanged(int _)
    {
        // กำลังโชว์อยู่ → วาดใหม่ด้วยภาษาใหม่ (สเต็ปที่เท่าไหร่ / ปุ่มถัดไป คงเดิม)
        if (_step == null || root == null || !root.activeSelf) return;

        bool dismissed = _dismissed;       // เก็บกล่องไปแล้วต้องไม่เด้งกลับมาเพราะสลับภาษากลางสเต็ป
        Show(_step, _stepNumber, _stepCount, _showNext);
        if (dismissed) DismissBox();
    }
}
