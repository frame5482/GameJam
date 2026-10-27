using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>ไปสเต็ปถัดไปเมื่อไหร่ — ⚠️ enum เซฟเป็นตัวเลข index เพิ่มค่าใหม่ให้**ต่อท้ายเท่านั้น**</summary>
public enum TutorialStepType
{
    /// <summary>รอผู้เล่นกดปุ่ม UI ที่ลากใส่ช่อง <c>targetButton</c></summary>
    ClickButton,
    /// <summary>ขึ้นคำอธิบายบอกว่าให้กดปุ่มอะไร — รอผู้เล่นกดปุ่มใน <c>pressKeys</c></summary>
    PressKey,
    /// <summary>แค่อ่านข้อความ แล้วกดปุ่ม "ถัดไป" บนกล่อง</summary>
    Message,
    /// <summary>รอ key ของสเต็ปนี้ (<c>signalKey</c>) — ใช้ตอนต้องรอให้ผู้เล่นไปทำอะไรที่ไม่ใช่กดปุ่ม UI</summary>
    WaitSignal,
}

/// <summary>★ ปุ่มที่ **เสกจาก prefab ทีหลัง** (ลากใส่ <c>targetButton</c> ไม่ได้) — จะเอาปุ่มไหนในคอนเทนเนอร์
/// ⚠️ enum เซฟเป็นตัวเลข index เพิ่มค่าใหม่ให้**ต่อท้ายเท่านั้น**</summary>
public enum TutorialPickMode
{
    /// <summary>ปุ่มแรกที่เจอในคอนเทนเนอร์ (ไล่ตามลำดับลูก)</summary>
    First,
    /// <summary>ปุ่มที่ **ชื่อ GameObject หรือข้อความในปุ่ม** มีคำใน <c>pickName</c></summary>
    ByName,
    /// <summary>ปุ่มลำดับที่ <c>pickIndex</c> (0 = ตัวแรก)</summary>
    ByIndex,
}

/// <summary>บทสอน 1 สเต็ป = กล่องคำอธิบาย 1 กล่อง — เป็นแถวในลิสต์ <see cref="TutorialManager.steps"/></summary>
[Serializable]
public class TutorialClick
{
    [Tooltip("ชื่อไว้ดูใน Inspector เฉย ๆ ไม่โชว์ในเกม")]
    public string label;

    [Header("--- คนพูด (เว้นว่างช่องไหน = ใช้ภาษาที่กรอกไว้แทน) ---")]
    [Tooltip("รูปเล็กของตัวละคร — ไม่ใส่ = ซ่อนช่องรูป")]
    public Sprite portrait;
    public string speakerENG;
    public string speakerTHAI;
    public string speakerJP;
    [TextArea(2, 5)] public string textENG;
    [TextArea(2, 5)] public string textTHAI;
    [TextArea(2, 5)] public string textJP;

    [Header("--- ไปสเต็ปถัดไปเมื่อ ---")]
    public TutorialStepType type = TutorialStepType.ClickButton;

    [Tooltip("ClickButton: ลากปุ่ม UI ที่อยากให้ผู้เล่นกดมาใส่\n" +
             "★ ปุ่มที่เสกจาก prefab ทีหลัง (ลากไม่ได้) = เว้นช่องนี้ไว้ แล้วใช้ targetParent ข้างล่างแทน")]
    public Button targetButton;

    [Tooltip("★ ตีกรอบ/เจาะรูที่ตัวนี้แทน — ลาก **แผง UI** (หรือของ UI ชิ้นไหนก็ได้) มาใส่\n" +
             "เว้นว่าง = ใช้ targetButton · ใช้กับ type ไหนก็ได้ (Message ก็ชี้แผงได้)")]
    public RectTransform targetPanel;

    [Header("--- ปุ่มที่เสกจาก Prefab ทีหลัง (ลากใส่ targetButton ไม่ได้) ---")]
    [Tooltip("★ **ลากคอนเทนเนอร์ที่ปุ่มจะไปเกิดข้างใน** (ตัวที่อยู่ในซีนอยู่แล้ว เช่น Quest Paper Container)\n" +
             "ระบบจะหาปุ่มในนั้นให้เองตอนมันเกิด — ปุ่มยังไม่เกิด = **รอ ไม่มีจอมืด ผู้เล่นทำอะไรก็ได้**\n" +
             "ลิสต์ถูกสร้างใหม่ (ปุ่มเก่าโดนลบ) = หาปุ่มใหม่ให้เองอัตโนมัติ\n" +
             "ใส่ targetButton ไว้ด้วย = ใช้ targetButton ก่อน ช่องนี้ไม่ทำงาน")]
    public RectTransform targetParent;

    [Tooltip("เอาปุ่มไหนในคอนเทนเนอร์ — First (ตัวแรก) / ByName (ชื่อหรือข้อความตรงกับ pickName) / ByIndex (ลำดับที่)")]
    public TutorialPickMode pickMode = TutorialPickMode.First;

    [Tooltip("ByName: คำที่ต้องมีใน **ชื่อ GameObject ของปุ่ม** หรือ **ข้อความในปุ่ม** (ไม่สนตัวพิมพ์เล็ก-ใหญ่)\n" +
             "เช่น ปุ่มตัวละครที่เสกมา พิมพ์ \"Aria\" ก็เจอ")]
    public string pickName;

    [Tooltip("ByIndex: ปุ่มลำดับที่เท่าไหร่ในคอนเทนเนอร์ (0 = ตัวแรก)")]
    [Min(0)] public int pickIndex;

    [Tooltip("นับปุ่มที่ถูกปิดอยู่ (SetActive false) ด้วย — ปกติไม่ต้องติ๊ก เพราะปุ่มที่ปิดอยู่ผู้เล่นกดไม่ได้")]
    public bool pickIncludeInactive;

    [Tooltip("★ Message: ปุ่ม \"ถัดไป\" บนกล่อง = **เก็บกล่องข้อความให้พ้นทางเฉย ๆ ยังไม่จบสเต็ป**\n" +
             "สเต็ปจะจบ (+ ยิง raiseSignalKey / ไปกล่องถัดไป) ต่อเมื่อกดปุ่มที่ลากใส่ targetButton\n" +
             "ตอนกล่องยังเปิด = มีจอมืดตามปกติ · พอกด \"ถัดไป\" **จอมืดหายไปด้วย** ผู้เล่นกดปุ่มอื่นได้\n" +
             "(กรอบไฮไลต์ยังล้อมปุ่มอยู่จนกว่าจะกด)\n" +
             "ใช้ได้เฉพาะ type = Message + ใส่ targetButton ไว้")]
    public bool waitForTargetButton;

    [Tooltip("PressKey: ปุ่มที่รับ — ใส่หลายอันได้ (กดอันไหนก็ผ่าน เช่น W A S D)\nไม่ใส่เลย = กดปุ่มอะไรก็ได้")]
    public List<Key> pressKeys = new List<Key>();

    [Tooltip("ชื่อปุ่มที่โชว์บนกล่อง เช่น \"Tab ค้าง + 1-9\" — เว้นว่าง = สร้างจาก pressKeys ให้เอง")]
    public string keyLabel;

    [Header("--- Key ของสเต็ปนี้ (เว้นว่าง = ไม่ใช้) ---")]
    [Tooltip("★ **กล่องนี้รอ key นี้ก่อนถึงจะโผล่** — ไม่มีใครยิง key นี้ = ไม่เล่นสเต็ปนี้ (รอไปเรื่อย ๆ)\n" +
             "ระหว่างรอ **เวลาในเกมเดินปกติ** ผู้เล่นทำอะไรก็ได้ · เว้นว่าง = โผล่เลยตามลำดับในลิสต์\n" +
             "ยิงมาก่อนถึงสเต็ปนี้ก็ได้ (ค้างใน PlayerPrefs) → มาถึงแล้วโผล่เลย\n\n" +
             "⚠️ type = WaitSignal ใช้ช่องนี้คนละความหมาย — เป็นเงื่อนไข \"รอ key นี้ถึงจะ**ไปต่อ**\" " +
             "(กล่องโผล่ก่อนแล้วค่อยรอ)")]
    public string signalKey;

    [Tooltip("★ สเต็ปนี้ผ่านแล้วยิง key นี้ออกไป — ทำ 2 อย่างพร้อมกัน:\n" +
             "1) **ไปสเต็ปที่ signalKey ตรงกันในบทนี้** (ไม่ใช่สเต็ปถัดไปในลิสต์)\n" +
             "2) ส่งออกไปข้างนอกด้วย — บทอื่น / ระบบอื่นที่รอ key นี้ได้รับตามปกติ\n\n" +
             "เว้นว่าง = ไปสเต็ปถัดไปในลิสต์เหมือนเดิม · หา signalKey ที่ตรงไม่เจอ = เตือนใน Console แล้วไล่ตามลิสต์")]
    public string raiseSignalKey;

    [Header("--- ดีเลย์ (เพิ่ม 2026-09-25) ---")]
    [Tooltip("★ รอกี่วินาทีก่อนกล่องนี้โผล่ — นับหลังสเต็ปก่อนหน้าผ่าน (และหลังได้ signalKey ถ้าใส่ไว้)\n" +
             "ระหว่างรอ **กล่องเก่าหายไป + เวลาในเกมเดินปกติ** (ให้ผู้เล่นเห็นผลที่เพิ่งกด / เล่นไปสักพักก่อน)\n" +
             "0 = โผล่ทันทีเหมือนเดิม · สเต็ปแรกของบท = นับต่อจาก startDelay")]
    [Min(0f)] public float delayBefore;

    [Header("--- ของที่ใช้เฉพาะสเต็ปนี้ (คืนสภาพเดิมให้เองตอนสเต็ปผ่าน) ---")]
    [Tooltip("★ เปิดของพวกนี้ตอนกล่องนี้โผล่ → ปิดคืนตอนสเต็ปผ่าน\nเช่น หุ่นซ้อม / มอนสเตอร์ / แผงที่ต้องเปิดให้ดู")]
    public List<GameObject> showObjects = new List<GameObject>();

    [Tooltip("ปิดของพวกนี้ตอนกล่องนี้โผล่ → เปิดคืนตอนสเต็ปผ่าน (ตรงข้ามกับช่องบน)")]
    public List<GameObject> hideObjects = new List<GameObject>();

    [Header("--- Events (ขั้นสูง — งานทั่วไปใช้ช่องข้างบนพอ) ---")]
    public UnityEvent onStepStart = new UnityEvent();
    public UnityEvent onStepEnd = new UnityEvent();

    /// <summary>สเต็ปนี้เป็น Message ที่ปุ่ม "ถัดไป" แค่เก็บกล่อง แล้วรอ <see cref="targetButton"/>
    /// ถึงจะจบสเต็ปไหม — ติ๊กไว้แต่ไม่ได้บอกว่าปุ่มไหน = ไม่นับ (ปุ่ม "ถัดไป" ไปสเต็ปถัดไปตามปกติ)</summary>
    public bool WaitsForTargetButton =>
        type == TutorialStepType.Message && waitForTargetButton && HasButtonSource;

    /// <summary>บอกไว้หรือยังว่าจะให้กดปุ่มไหน — ลากปุ่มมาเอง หรือบอกคอนเทนเนอร์ที่ปุ่มจะไปเกิด</summary>
    public bool HasButtonSource => targetButton != null || targetParent != null;

    /// <summary>สเต็ปนี้จบด้วยการกดปุ่ม UI ไหม (<c>ClickButton</c> หรือ <c>Message</c> ที่รอปุ่ม)</summary>
    public bool NeedsButton => type == TutorialStepType.ClickButton || WaitsForTargetButton;

    // ใช้ซ้ำ — หาปุ่มทุกเฟรมตอนรอปุ่มที่ยังไม่เกิด ไม่ให้แจก garbage
    static readonly List<Button> Found = new List<Button>();
    static readonly List<TMPro.TMP_Text> Labels = new List<TMPro.TMP_Text>();

    /// <summary>★ ปุ่มที่สเต็ปนี้สั่งให้กด — ลากมาเองมาก่อนเสมอ ไม่งั้นไปหาใน <see cref="targetParent"/>
    /// (ปุ่มที่ <c>Instantiate</c> ทีหลังถึงชี้ได้) · **ยังไม่เกิด = คืน null** ให้คนเรียกไปรอเอง</summary>
    public Button ResolveButton()
    {
        if (targetButton != null) return targetButton;
        if (targetParent == null) return null;

        targetParent.GetComponentsInChildren(pickIncludeInactive, Found);
        if (Found.Count == 0) return null;

        switch (pickMode)
        {
            case TutorialPickMode.ByIndex:
                return pickIndex >= 0 && pickIndex < Found.Count ? Found[pickIndex] : null;

            case TutorialPickMode.ByName:
                if (string.IsNullOrWhiteSpace(pickName)) return Found[0];
                foreach (Button b in Found)
                    if (b != null && MatchesName(b, pickName.Trim())) return b;
                return null;

            default:
                return Found[0];
        }
    }

    /// <summary>ชื่อ GameObject หรือข้อความในปุ่มมีคำนี้ไหม — ปุ่มที่เสกมามักชื่อซ้ำกันหมด
    /// (<c>ItemCard(Clone)</c>) เลยต้องดูข้อความข้างในด้วย</summary>
    static bool MatchesName(Button button, string word)
    {
        if (button.name.IndexOf(word, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;

        button.GetComponentsInChildren(true, Labels);
        foreach (TMPro.TMP_Text label in Labels)
        {
            if (label == null || string.IsNullOrEmpty(label.text)) continue;
            if (label.text.IndexOf(word, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
        }
        return false;
    }

    public string GetSpeaker(int language) =>
        speakerENG;

    public string GetText(int language) =>
        textENG;

    /// <summary>ข้อความชื่อปุ่มที่โชว์ — ไม่ได้เขียนเอง = ต่อจาก <see cref="pressKeys"/> ให้</summary>
    public string GetKeyLabel()
    {
        if (!string.IsNullOrWhiteSpace(keyLabel)) return keyLabel;
        if (type != TutorialStepType.PressKey) return "";
        if (pressKeys == null || pressKeys.Count == 0) return "";

        var parts = new List<string>();
        foreach (Key k in pressKeys)
        {
            if (k == Key.None) continue;
            parts.Add(k.ToString());
        }
        return parts.Count > 0 ? string.Join(" / ", parts) : "";
    }
}

/// <summary>★ ตัวควบคุมหลักของบทสอน — วางบน **GameObject เปล่าในซีน** (1 ตัว = 1 บท)
///
/// กรอกสเต็ปในลิสต์ <see cref="steps"/> แล้วมันจะขึ้นทีละกล่องตามลำดับ
/// — ปุ่ม UI ที่อยากให้กด **ลากใส่ช่อง <c>targetButton</c> ของสเต็ปนั้นได้ตรง ๆ ไม่ต้องแก้สคริปต์ปุ่ม**
///
/// <para><b>เริ่มเมื่อไหร่</b> — <see cref="startSignalKey"/> เว้นว่าง = เริ่มตอน Start ของซีน /
/// ใส่ key = รอจนมีใครเรียก <see cref="TutorialSignals.Raise"/> ด้วย key นั้น (**ข้ามซีนได้**)</para>
///
/// <para><b>จบแล้วไปต่อ</b> — ใส่ <see cref="nextSignalKey"/> ไว้ พอบทนี้จบมันจะยิง key นั้นให้เอง
/// → บทถัดไป (ซีนไหนก็ได้) ที่ตั้ง <c>startSignalKey</c> ตรงกันจะเริ่มต่อทันที</para></summary>
[DisallowMultipleComponent]
public class TutorialManager : MonoBehaviour
{
    // ─── PlayerPrefs ────────────────────────────────────────────────────────

    const string DonePrefix = "Tutorial_Done_";
    const string IndexKey = "Tutorial_Index";
    const char Separator = '\n';

    [Header("--- บทสอนนี้ ---")]
    [Tooltip("ชื่อบทนี้ ใช้จำว่าสอนไปแล้ว (PlayerPrefs) — เว้นว่าง = ใช้ชื่อ GameObject")]
    public string tutorialKey;

    [Tooltip("สอนครั้งเดียว — ผ่านแล้วไม่ขึ้นอีก (เรียก Play() ยังเล่นซ้ำได้)")]
    public bool playOnce = true;

    [Header("--- เริ่มเมื่อ ---")]
    [Tooltip("★ รอ key นี้ก่อนถึงเริ่ม — **เว้นว่าง = เริ่มทันทีตอนซีนเริ่ม**\n" +
             "key ส่งข้ามซีนได้ (เก็บใน PlayerPrefs) → ยิงจากซีนก่อนหน้าแล้วค่อยมาเริ่มที่ซีนนี้ได้")]
    public string startSignalKey;

    [Tooltip("รอกี่วินาทีก่อนขึ้นกล่องแรก (ให้แผง/อนิเมชันเปิดจบก่อน)")]
    [Min(0f)] public float startDelay = 0.4f;

    [Header("--- หยุดเวลาตอนสอน ---")]
    [Tooltip("★ บทนี้เริ่มแล้ว **หยุดเวลาในเกม** (Time.timeScale = 0) — มอน/ตัวละคร/คูลดาวน์หยุดหมด\n" +
             "แต่ **UI ยังกดได้ตามปกติ** (EventSystem กับ Input System ไม่ได้อิง timeScale)\n" +
             "จบบท / กดข้าม / เปลี่ยนซีนกลางบท = คืนความเร็วเดิมให้เอง\n\n" +
             "⚠️ ปิดช่องนี้ถ้าบทนั้นต้องให้ผู้เล่นขยับตัว (สอนเดิน/โจมตี) หรือใช้เขตเดินผ่าน " +
             "(TutorialSignal.raiseOnPlayerEnter) — เวลาหยุด = ฟิสิกส์หยุด ตัวละครเดินไม่ได้ trigger ไม่ยิง")]
    public bool pauseGameWhileRunning = true;

    [Header("--- จบแล้วเรียกบทไหนต่อ ---")]
    [Tooltip("★ บทนี้จบแล้วยิง key นี้ให้เอง — บทที่ตั้ง startSignalKey ตรงกันจะเริ่มต่อ (ข้ามซีนได้)\nเว้นว่าง = จบเลย ไม่เรียกใคร")]
    public string nextSignalKey;

    [Header("--- กล่องคำอธิบาย ---")]
    [Tooltip("Prefab ที่มี TutorialDialog — เว้นว่าง = หา TutorialDialog ที่วางอยู่ในซีนแทน")]
    public TutorialDialog dialogPrefab;

    [Tooltip("เสกกล่องไว้ใต้ตัวไหน (Canvas) — เว้นว่าง = หา Canvas ในซีนให้เอง")]
    public Transform dialogParent;

    [Header("--- สเต็ป (ขึ้นตามลำดับในลิสต์ · ใส่ key แล้วข้ามไปตัวที่ระบุได้) ---")]
    public List<TutorialClick> steps = new List<TutorialClick>();

    [Header("--- เสียง (เว้นว่าง = เงียบ) ---")]
    [Tooltip("ตอนขึ้นกล่องใหม่")]
    public AudioClip stepSfx;
    [Tooltip("ตอนจบบท")]
    public AudioClip finishSfx;

    [Header("--- Events (ขั้นสูง — เรียกสคริปต์อื่นตอนบทเริ่ม/จบ) ---")]
    public UnityEvent onStarted = new UnityEvent();
    public UnityEvent onFinished = new UnityEvent();
    public UnityEvent onSkipped = new UnityEvent();

    /// <summary>บทที่กำลังเล่นอยู่ (ทั้งเกมเล่นได้ทีละบท) — null = ว่าง</summary>
    public static TutorialManager Running { get; private set; }

    public string SaveKey => string.IsNullOrWhiteSpace(tutorialKey) ? gameObject.name : tutorialKey;
    public bool IsRunning => Running == this;
    public int CurrentStep => IsRunning ? _index : -1;

    TutorialDialog _dialog;
    int _index = -1;
    bool _waitingForSignal;
    float _beginAt = -1f;
    bool _pendingBegin;
    bool _stepSignalReceived;
    Button _hookedButton;

    // ★ มาถึงสเต็ปนี้แล้วแต่ยังไม่โผล่ — รอ signalKey ของมันก่อน (เวลาเดินปกติระหว่างรอ)
    bool _waitingEntryKey;
    bool _entryKeyReceived;
    float _entryWarnAt;

    // ★ เข้าสเต็ปแล้ว แต่รอ delayBefore ก่อนกล่องโผล่ (เวลาเดินปกติระหว่างรอ)
    bool _waitingDelay;
    float _showAt;

    // ★ กล่องโผล่แล้ว แต่ปุ่มที่ต้องกดยังไม่ถูกเสกออกมา — หาใหม่ทุกเฟรมจนเจอ
    bool _waitingButton;
    float _buttonWarnAt;

    // หยุดเวลา — จำค่าเดิมไว้คืน (ไม่ใช่ตั้งเป็น 1 ดื้อ ๆ เผื่อมีระบบอื่น pause อยู่ก่อนแล้ว)
    bool _paused;
    float _prevTimeScale = 1f;

    // จำสภาพเดิมของสิ่งที่ไปปิด/เปิด เพื่อคืนให้เหมือนก่อนสอน
    TutorialClick _objectsStep;
    readonly List<bool> _showPrev = new List<bool>();
    readonly List<bool> _hidePrev = new List<bool>();

    // ─── วงจรชีวิต ──────────────────────────────────────────────────────────

    void OnEnable() => TutorialSignals.OnSignal += HandleSignal;

    void OnDisable()
    {
        TutorialSignals.OnSignal -= HandleSignal;

        // เปลี่ยนซีน / ปิด object กลางบท → เก็บของคืน แต่ไม่จดว่าผ่าน
        if (IsRunning) Cleanup();
        else ResumeGame();      // กันเกมค้างหยุดถ้าหลุดออกนอกทางปกติ — timeScale ค้าง 0 = เกมตาย
    }

    void Start()
    {
        if (playOnce && IsDone(SaveKey)) return;

        if (string.IsNullOrWhiteSpace(startSignalKey))
        {
            ScheduleBegin();
            return;
        }

        // key ที่ถูกยิงมาจากซีนก่อนหน้า (ค้างอยู่ใน PlayerPrefs) → เริ่มได้เลย
        if (TutorialSignals.IsPending(startSignalKey))
        {
            TutorialSignals.Consume(startSignalKey);
            ScheduleBegin();
            return;
        }

        _waitingForSignal = true;
    }

    void Update()
    {
        if (_pendingBegin && Time.unscaledTime >= _beginAt && Running == null)
        {
            _pendingBegin = false;
            Begin();
            return;
        }

        if (IsRunning) TickStep();
    }

    void HandleSignal(string key)
    {
        // key ของสเต็ปที่กำลังรออยู่ — เช็คก่อน เพราะบทนี้เล่นอยู่แล้ว
        if (IsRunning)
        {
            TutorialClick step = steps[_index];

            // ★ รอ key เพื่อ "เข้า" สเต็ปนี้ (กล่องยังไม่โผล่)
            if (_waitingEntryKey)
            {
                if (KeyMatches(key, step.signalKey))
                {
                    TutorialSignals.Consume(key);
                    _entryKeyReceived = true;    // โผล่ใน TickStep เฟรมถัดไป (กันเรียกซ้อนกลาง event)
                }
                return;
            }

            // WaitSignal: รอ key เพื่อ "ไปต่อ" จากสเต็ปนี้ (กล่องโผล่อยู่แล้ว)
            if (step.type == TutorialStepType.WaitSignal && KeyMatches(key, step.signalKey))
            {
                TutorialSignals.Consume(key);
                _stepSignalReceived = true;
            }
            return;
        }

        if (!_waitingForSignal || key != startSignalKey) return;

        _waitingForSignal = false;
        TutorialSignals.Consume(startSignalKey);
        ScheduleBegin();
    }

    void ScheduleBegin()
    {
        _pendingBegin = true;
        _beginAt = Time.unscaledTime + startDelay;
    }

    // ─── เล่นบท ─────────────────────────────────────────────────────────────

    /// <summary>เริ่มบทนี้ทันที ไม่สนว่าสอนไปแล้วหรือยัง (ผูกกับปุ่ม "ดูบทสอนอีกครั้ง" ได้)</summary>
    public void Play()
    {
        if (IsRunning) return;

        if (Running != null)
        {
            Debug.LogWarning($"[TutorialManager] '{SaveKey}' เล่นไม่ได้ — บท '{Running.SaveKey}' เล่นอยู่ " +
                             "(ทั้งเกมเล่นได้ทีละบท)", this);
            return;
        }

        _waitingForSignal = false;
        _pendingBegin = false;
        Begin();
    }

    void Begin()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning($"[TutorialManager] '{SaveKey}' ไม่มีสเต็ปในลิสต์ — ข้ามไปเรียก nextSignalKey เลย", this);
            Finish();
            return;
        }

        if (!ResolveDialog())
        {
            Debug.LogWarning($"[TutorialManager] '{SaveKey}' หากล่องคำอธิบายไม่เจอ — ลาก prefab ใส่ dialogPrefab", this);
            return;
        }

        Running = this;
        _index = -1;
        onStarted.Invoke();
        NextStep();
    }

    void NextStep()
    {
        // ★ ผู้เล่นกดผ่านสเต็ปนี้แล้ว (ปุ่ม "ถัดไป" / ปุ่มที่กำหนด / ปุ่มคีย์บอร์ด) = **คืนเวลาทันที**
        //   กล่องถัดไปจะหยุดเวลาใหม่เองตอนมันโผล่ (ShowStep) — ระหว่างนี้เกมต้องไม่ค้าง
        ResumeGame();

        UnhookStep();

        RestoreStepObjects();                             // เก็บของที่สเต็ปก่อนหน้าเปิดไว้

        bool firstStep = _index < 0;                      // บทเพิ่งเริ่ม ยังไม่เคยมีสเต็ปไหนเล่น
        int next = _index + 1;

        if (_index >= 0 && _index < steps.Count)
        {
            TutorialClick done = steps[_index];
            done.onStepEnd.Invoke();
            TutorialSignals.Raise(done.raiseSignalKey);   // สเต็ปนี้ผ่านแล้ว บอกคนอื่นต่อ

            // ★ ใส่ raiseSignalKey ไว้ = ไปสเต็ปที่ signalKey ตรงกัน ไม่ใช่ไล่ตามลำดับในลิสต์
            next = ResolveNextStep(done, next);
        }

        _index = next;
        if (_index < 0 || _index >= steps.Count) { Finish(); return; }

        EnterStep(firstStep);
    }

    /// <summary>★ มาถึงสเต็ป <c>_index</c> แล้ว — **ใส่ <c>signalKey</c> ไว้ = รอ key นั้นก่อน กล่องถึงจะโผล่**
    /// (ไม่ได้ใส่ = โผล่เลยตามลำดับเหมือนเดิม)
    ///
    /// <para><paramref name="firstStep"/> = สเต็ปแรกของบท → **เข้าได้เลยเสมอ ไม่ต้องรอ key**
    /// เงื่อนไข "รอก่อนเริ่ม" ของทั้งบทคือ <see cref="startSignalKey"/> ไม่ใช่ key ของสเต็ป
    /// (ไม่งั้นบทที่สเต็ปแรกใส่ key ไว้จะไม่มีอะไรโผล่เลย — ไม่มีใครยิง key ให้)</para></summary>
    void EnterStep(bool firstStep)
    {
        TutorialClick step = steps[_index];
        _stepSignalReceived = false;
        _waitingEntryKey = false;
        _entryKeyReceived = false;
        _waitingDelay = false;

        if (!firstStep && NeedsEntryKey(step))
        {
            // ยิงมาก่อนถึงสเต็ปนี้แล้ว (ค้างใน PlayerPrefs / เพิ่งมาจาก raiseSignalKey) → เข้าได้เลย
            if (TutorialSignals.IsPending(step.signalKey.Trim()))
            {
                TutorialSignals.Consume(step.signalKey.Trim());
            }
            else
            {
                _waitingEntryKey = true;
                _entryWarnAt = Time.unscaledTime + 5f; // รอนานผิดปกติ = บอกใน Console ไม่ปล่อยค้างเงียบ ๆ
                if (_dialog != null) _dialog.Hide();   // กล่องของสเต็ปก่อนหน้าต้องไม่ค้างระหว่างรอ
                ResumeGame();                          // key อาจต้องเล่นเกมถึงจะได้มา — ห้ามหยุดเวลาค้าง
                return;
            }
        }

        ShowStepAfterDelay();
    }

    /// <summary>★ โผล่กล่องของสเต็ปนี้ — ตั้ง <c>delayBefore</c> ไว้ = รอก่อน (กล่องเก่าหาย + เวลาเดินปกติ)
    /// แล้ว <see cref="TickStep"/> ค่อยเรียก <see cref="ShowStep"/> เมื่อครบเวลา</summary>
    void ShowStepAfterDelay()
    {
        float delay = steps[_index].delayBefore;
        if (delay <= 0f)
        {
            ShowStep();
            return;
        }

        _waitingDelay = true;
        _showAt = Time.unscaledTime + delay;
        if (_dialog != null) _dialog.Hide();
        ResumeGame();
    }

    /// <summary>สเต็ปนี้ต้องรอ key ก่อนโผล่ไหม — <c>WaitSignal</c> **ไม่นับ** เพราะ <c>signalKey</c>
    /// ของมันคือเงื่อนไข "ไปต่อ" ไม่ใช่เงื่อนไข "เข้า"</summary>
    static bool NeedsEntryKey(TutorialClick step) =>
        step.type != TutorialStepType.WaitSignal && !string.IsNullOrWhiteSpace(step.signalKey);

    static bool KeyMatches(string a, string b) =>
        !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b) && a.Trim() == b.Trim();

    /// <summary>เปิดกล่องของสเต็ป <c>_index</c> จริง ๆ — หยุดเวลาตรงนี้ ไม่ใช่ตอนบทเริ่ม
    /// (เวลาจะได้เดินปกติระหว่างที่รอ key ของสเต็ปถัดไป)</summary>
    void ShowStep()
    {
        TutorialClick step = steps[_index];

        PauseGame();                                      // หยุดเวลาเฉพาะตอนมีกล่องอยู่บนจอ
        ApplyStepObjects(step);                           // เปิดหุ่นซ้อม/แผงที่สเต็ปนี้ต้องใช้
        step.onStepStart.Invoke();

        // key ถูกยิงมาก่อนถึงสเต็ปนี้ (ค้างใน PlayerPrefs) → นับว่าได้แล้ว
        if (step.type == TutorialStepType.WaitSignal && TutorialSignals.IsPending(step.signalKey))
        {
            TutorialSignals.Consume(step.signalKey);
            _stepSignalReceived = true;
        }

        _dialog.Show(step, _index + 1, steps.Count, step.type == TutorialStepType.Message);
        PlaySfx(stepSfx);

        // ปุ่ม UI: ฟัง onClick ของปุ่มเดิม — ไม่ต้องแก้สคริปต์ที่ผูกไว้อยู่แล้ว
        // (Message ที่ติ๊ก waitForTargetButton ก็จบสเต็ปด้วยปุ่มนี้เหมือนกัน — ต่างกันแค่ปุ่ม "ถัดไป" บนกล่อง)
        _waitingButton = false;
        _buttonWarnAt = 0f;

        if (step.NeedsButton && step.HasButtonSource)
        {
            HookTargetButton(step);       // ปุ่มที่เสกทีหลังยังไม่เกิด = ตั้งธงรอ แล้วหาใหม่ทุกเฟรมใน TickStep
        }
        else if (step.type == TutorialStepType.ClickButton)
        {
            Debug.LogWarning($"[TutorialManager] '{SaveKey}' สเต็ปที่ {_index + 1} เป็น ClickButton " +
                             "แต่ยังไม่ได้บอกว่าปุ่มไหน — ลากปุ่มใส่ targetButton " +
                             "(ปุ่มที่เสกจาก prefab ทีหลัง ลากคอนเทนเนอร์ใส่ targetParent แทน) — กล่องจะค้าง", this);
        }
        else if (step.type == TutorialStepType.Message && step.waitForTargetButton)
        {
            Debug.LogWarning($"[TutorialManager] '{SaveKey}' สเต็ปที่ {_index + 1} ติ๊ก waitForTargetButton " +
                             "ไว้ แต่ยังไม่ได้บอกว่าปุ่มไหน (targetButton / targetParent) — " +
                             "ปุ่ม \"ถัดไป\" จะไปสเต็ปถัดไปตามปกติ", this);
        }
        else if (step.type == TutorialStepType.WaitSignal && string.IsNullOrWhiteSpace(step.signalKey))
        {
            Debug.LogWarning($"[TutorialManager] '{SaveKey}' สเต็ปที่ {_index + 1} เป็น WaitSignal " +
                             "แต่ยังไม่ได้ใส่ signalKey — กล่องจะค้าง", this);
        }
    }

    /// <summary>★ สเต็ปถัดไปคือตัวไหน — สเต็ปที่ใส่ <c>raiseSignalKey</c> ไว้จะ **กระโดดไปหาสเต็ปที่
    /// <c>signalKey</c> ตรงกัน** ไม่ใช่ไล่ตามลำดับในลิสต์ · เว้นว่าง = ไล่ตามลิสต์เหมือนเดิม
    ///
    /// <para>key ยัง**ถูกยิงออกไปข้างนอกด้วยตามปกติ** (ไม่ได้ Consume ทิ้ง) — บทอื่น/ระบบอื่นที่รอ key
    /// เดียวกันยังได้รับเหมือนเดิม</para></summary>
    int ResolveNextStep(TutorialClick done, int byList)
    {
        string key = done.raiseSignalKey;
        if (string.IsNullOrWhiteSpace(key)) return byList;

        key = key.Trim();
        for (int i = 0; i < steps.Count; i++)
        {
            TutorialClick s = steps[i];
            if (s == null || string.IsNullOrWhiteSpace(s.signalKey)) continue;
            if (s.signalKey.Trim() != key) continue;

            // ★ กระโดดกลับหาตัวเอง = วนสเต็ปเดิมไม่มีวันจบ → บทไม่เคยถึง Finish() ไม่ถูกจดว่าสอนแล้ว
            //   เข้าซีนใหม่ก็ขึ้นซ้ำ (เจอใน Room: บท Dis สเต็ปเดียว signalKey = raiseSignalKey = Dis01 — แก้ 2026-09-26)
            //   → ไม่นับเป็นปลายทาง ไล่ตามลำดับในลิสต์แทน (สเต็ปสุดท้าย = จบบท)
            if (i == _index)
            {
                Debug.LogWarning($"[TutorialManager] '{SaveKey}' สเต็ปที่ {i + 1} ยิง key '{key}' ซึ่งเป็น signalKey " +
                                 "ของตัวเอง — จะวนสเต็ปเดิมไม่จบ ข้ามไปไล่ตามลำดับในลิสต์แทน " +
                                 "(ลบ raiseSignalKey ของสเต็ปนี้ทิ้ง หรือเปลี่ยนเป็น key ของสเต็ปอื่น)", this);
                continue;
            }

            // key เพิ่งถูกยิงไปเมื่อกี้ → สเต็ปปลายทางที่เป็น WaitSignal จะผ่านทันทีตั้งแต่เฟรมแรก
            if (s.type == TutorialStepType.WaitSignal)
                Debug.LogWarning($"[TutorialManager] '{SaveKey}' สเต็ปที่ {i + 1} ถูกเรียกด้วย key '{key}' " +
                                 "แต่เป็น type = WaitSignal ที่รอ key เดียวกัน — กล่องจะผ่านทันที " +
                                 "(เปลี่ยนเป็น Message / ClickButton แทน)", this);
            return i;
        }

        Debug.LogWarning($"[TutorialManager] '{SaveKey}' สเต็ปที่ {_index + 1} ยิง key '{key}' " +
                         "แต่ไม่มีสเต็ปไหนในบทนี้ตั้ง signalKey ตรงกัน — ไล่ตามลำดับในลิสต์แทน", this);
        return byList;
    }

    void TickStep()
    {
        // ★ ยังไม่เข้าสเต็ป — รอ signalKey ของมันอยู่ (กล่องยังไม่โผล่ ห้ามเช็คเงื่อนไขไปต่อ)
        if (_waitingEntryKey)
        {
            if (!_entryKeyReceived)
            {
                if (_entryWarnAt > 0f && Time.unscaledTime >= _entryWarnAt)
                {
                    _entryWarnAt = 0f;
                    Debug.LogWarning($"[TutorialManager] '{SaveKey}' ค้างอยู่ที่สเต็ปที่ {_index + 1} — " +
                                     $"รอ key '{steps[_index].signalKey}' มา 5 วิแล้วยังไม่มีใครยิง " +
                                     "(ตรวจว่ามีสเต็ป/ระบบไหนยิง key นี้จริงไหม หรือสะกดตรงกันหรือเปล่า)", this);
                }
                return;
            }

            _waitingEntryKey = false;
            _entryKeyReceived = false;
            ShowStepAfterDelay();
            return;
        }

        // ★ รอ delayBefore อยู่ — กล่องยังไม่โผล่ ห้ามเช็คเงื่อนไขไปต่อ
        if (_waitingDelay)
        {
            if (Time.unscaledTime < _showAt) return;
            _waitingDelay = false;
            ShowStep();
            return;
        }

        TutorialClick step = steps[_index];

        // ★ ปุ่มเพิ่งถูกเสกออกมา / ลิสต์ถูกสร้างใหม่จนปุ่มเก่าโดนลบ (_hookedButton กลายเป็น null)
        //   → หาใหม่แล้ว hook ใหม่ ไม่งั้นกดยังไงสเต็ปก็ไม่ผ่าน
        if (step.NeedsButton && step.HasButtonSource && _hookedButton == null)
            HookTargetButton(step);

        if (step.type == TutorialStepType.PressKey && IsStepKeyPressed(step))
            NextStep();
        else if (step.type == TutorialStepType.WaitSignal && _stepSignalReceived)
            NextStep();
    }

    /// <summary>★ หาปุ่มที่สเต็ปนี้สั่งให้กดแล้วฟัง <c>onClick</c> — ปุ่มที่ <c>Instantiate</c> ทีหลัง
    /// ตอนกล่องโผล่ยังไม่มีตัวตน เลยต้องหาใหม่ได้เรื่อย ๆ (<see cref="TickStep"/> เรียกซ้ำให้ทุกเฟรม)
    ///
    /// <para>ระหว่างรอ **ไม่มีกรอบไฮไลต์และไม่มีจอมืด** — ผู้เล่นกดอะไรก็ได้ตามปกติ
    /// (ไม่งั้นจอมืดจะบังของที่ต้องกดเพื่อให้ปุ่มนั้นเกิด) · เจอแล้วกรอบค่อยไปเกาะ</para></summary>
    void HookTargetButton(TutorialClick step)
    {
        Button button = step.ResolveButton();
        if (button == null)
        {
            if (!_waitingButton)
            {
                _waitingButton = true;
                _buttonWarnAt = Time.unscaledTime + 5f;   // รอนานผิดปกติ = บอกใน Console ไม่ปล่อยค้างเงียบ ๆ
            }
            else if (_buttonWarnAt > 0f && Time.unscaledTime >= _buttonWarnAt)
            {
                _buttonWarnAt = 0f;
                Debug.LogWarning($"[TutorialManager] '{SaveKey}' สเต็ปที่ {_index + 1} หาปุ่มใน " +
                                 $"targetParent '{(step.targetParent != null ? step.targetParent.name : "-")}' " +
                                 "ไม่เจอมา 5 วิแล้ว (ตรวจว่าปุ่มถูกเสกลงคอนเทนเนอร์ตัวนี้จริงไหม " +
                                 "หรือ pickMode/pickName ตรงกับปุ่มหรือเปล่า)", this);
            }
            return;
        }

        _waitingButton = false;
        _buttonWarnAt = 0f;
        _hookedButton = button;
        _hookedButton.onClick.AddListener(NextStep);

        // กรอบไฮไลต์ + จอมืดเพิ่งรู้ว่าต้องไปเกาะตรงไหน (ตอน Show ปุ่มยังไม่เกิด)
        if (_dialog != null) _dialog.SetButtonTarget(button);
    }

    static bool IsStepKeyPressed(TutorialClick step)
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;

        // ไม่ได้ระบุปุ่ม = กดอะไรก็ผ่าน
        if (step.pressKeys == null || step.pressKeys.Count == 0)
            return kb.anyKey.wasPressedThisFrame;

        foreach (Key k in step.pressKeys)
        {
            if (k == Key.None) continue;
            if (kb[k] != null && kb[k].wasPressedThisFrame) return true;
        }
        return false;
    }

    /// <summary>ข้ามบทที่เล่นอยู่ (นับว่าผ่าน + ยิง nextSignalKey ตามปกติ) — ผูกกับปุ่มข้ามบนกล่อง</summary>
    public void Skip()
    {
        if (!IsRunning) return;

        // สเต็ปที่ค้างอยู่ต้องได้ onStepEnd ด้วย ไม่งั้นของที่ onStepStart เสกไว้ค้างในซีน
        // (สเต็ปที่ยังรอ key เข้าอยู่ยังไม่เคยได้ onStepStart → ไม่ต้องยิง onStepEnd ให้)
        if (!_waitingEntryKey && !_waitingDelay && _index >= 0 && _index < steps.Count) steps[_index].onStepEnd.Invoke();

        onSkipped.Invoke();
        Finish();
    }

    void Finish()
    {
        Cleanup();

        MarkDone(SaveKey);                       // เขียนลงเครื่องทันที — สเต็ปสุดท้ายมักเปลี่ยนซีน
        TutorialSignals.Raise(nextSignalKey);
        PlaySfx(finishSfx);
        onFinished.Invoke();
    }

    void Cleanup()
    {
        UnhookStep();
        RestoreStepObjects();
        ResumeGame();                // ทุกทางออกผ่านตรงนี้ — จบบท / กดข้าม / เปลี่ยนซีนกลางบท
        _waitingEntryKey = false;
        _entryKeyReceived = false;
        _waitingDelay = false;

        if (_dialog != null) _dialog.Hide();
        if (Running == this) Running = null;
        _index = -1;
    }

    // ─── หยุดเวลา ───────────────────────────────────────────────────────────

    /// <summary>หยุดเวลาในเกม — **UI ยังกดได้** เพราะ EventSystem + Input System ไม่ได้อิง
    /// <c>Time.timeScale</c> · ตัวจับเวลาของบทสอนเองใช้ <c>Time.unscaledTime</c> อยู่แล้ว</summary>
    void PauseGame()
    {
        if (!pauseGameWhileRunning) return;

        // หยุดอยู่แล้วแต่ timeScale ไม่ใช่ 0 = มีคนไปแก้ระหว่างทาง (กล่องอยู่บนจอแต่เวลาเดิน) → หยุดซ้ำให้
        // ⚠️ ห้ามจำ _prevTimeScale ใหม่ตรงนี้ ไม่งั้นจะจำ 0 ไว้แล้วคืนเวลาไม่ได้
        if (_paused)
        {
            if (Time.timeScale != 0f) Time.timeScale = 0f;
            return;
        }

        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _paused = true;
    }

    /// <summary>คืนความเร็วเดิม (ไม่ใช่ตั้ง 1) — เผื่อมีระบบอื่นหยุดเวลาค้างไว้ก่อนบทสอนจะเริ่ม
    ///
    /// <para>⚠️ ค่าเดิมที่จำไว้เป็น <c>0</c> (บทสอนเริ่มตอนเกมหยุดอยู่แล้ว) = **คืนเป็น 1**
    /// ไม่งั้นคืนไปก็ยังค้างหยุดอยู่ดี แล้วไม่มีใครมาปลดให้อีก = เกมตาย</para></summary>
    void ResumeGame()
    {
        if (!_paused) return;

        Time.timeScale = _prevTimeScale > 0f ? _prevTimeScale : 1f;
        _paused = false;
    }

    // ─── เปิด-ปิดของให้ แล้วคืนสภาพเดิม ─────────────────────────────────────

    /// <summary>เปิด <c>showObjects</c> / ปิด <c>hideObjects</c> ของสเต็ปนี้ — จำสภาพเดิมไว้ก่อนเสมอ</summary>
    void ApplyStepObjects(TutorialClick step)
    {
        _objectsStep = step;
        _showPrev.Clear();
        _hidePrev.Clear();

        foreach (GameObject go in step.showObjects)
        {
            _showPrev.Add(go != null && go.activeSelf);
            if (go != null) go.SetActive(true);
        }

        foreach (GameObject go in step.hideObjects)
        {
            _hidePrev.Add(go != null && go.activeSelf);
            if (go != null) go.SetActive(false);
        }
    }

    void RestoreStepObjects()
    {
        if (_objectsStep == null) return;

        for (int i = 0; i < _objectsStep.showObjects.Count && i < _showPrev.Count; i++)
        {
            GameObject go = _objectsStep.showObjects[i];
            if (go != null) go.SetActive(_showPrev[i]);
        }

        for (int i = 0; i < _objectsStep.hideObjects.Count && i < _hidePrev.Count; i++)
        {
            GameObject go = _objectsStep.hideObjects[i];
            if (go != null) go.SetActive(_hidePrev[i]);
        }

        _objectsStep = null;
        _showPrev.Clear();
        _hidePrev.Clear();
    }

    void UnhookStep()
    {
        _waitingButton = false;
        _buttonWarnAt = 0f;

        // ปุ่มที่โดนลบไปแล้ว (ลิสต์สร้างใหม่) = null อยู่แล้ว listener หายไปพร้อมตัวมัน ไม่ต้องถอด
        if (_hookedButton == null) return;
        _hookedButton.onClick.RemoveListener(NextStep);
        _hookedButton = null;
    }

    // ─── กล่องคำอธิบาย ──────────────────────────────────────────────────────

    bool ResolveDialog()
    {
        if (_dialog != null) return true;

        if (dialogPrefab != null)
        {
            Transform parent = dialogParent != null ? dialogParent : FindCanvas();
            if (parent == null)
                Debug.LogWarning($"[TutorialManager] '{SaveKey}' ซีนนี้ไม่มี Canvas — กล่องคำอธิบายจะไม่โผล่ " +
                                 "(ลาก Canvas ใส่ช่อง dialogParent)", this);

            _dialog = Instantiate(dialogPrefab, parent);
            _dialog.name = dialogPrefab.name;
        }
        else
        {
            // ไม่ได้ลาก prefab มา → ใช้กล่องที่วางไว้ในซีน (รวมตัวที่ปิดอยู่)
            _dialog = FindFirstObjectByType<TutorialDialog>(FindObjectsInactive.Include);
        }

        if (_dialog != null) _dialog.Bind(this);
        return _dialog != null;
    }

    static Transform FindCanvas()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        return canvas != null ? canvas.transform : null;
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip != null) UISfxManager.PlayClip(clip);
    }

    /// <summary>กล่องกดปุ่ม "ถัดไป" — <see cref="TutorialDialog"/> เรียกเข้ามา</summary>
    public void OnDialogNext()
    {
        if (!IsRunning) return;

        // สเต็ปที่รอปุ่มจริง: "ถัดไป" = **เก็บกล่องข้อความให้พ้นทางเฉย ๆ**
        // ยังไม่จบสเต็ป ยังไม่ยิง raiseSignalKey — รอให้กด targetButton ก่อน
        if (steps[_index].WaitsForTargetButton)
        {
            if (_dialog != null) _dialog.DismissBox();
            ResumeGame();     // ★ กล่อง + จอมืดหายไปแล้ว = ผู้เล่นต้องเล่นต่อได้ ห้ามปล่อยเกมค้างหยุด
            return;
        }

        NextStep();
    }

    // ─── จำว่าสอนแล้ว (PlayerPrefs) ─────────────────────────────────────────

    public static bool IsDone(string key) =>
        !string.IsNullOrWhiteSpace(key) && PlayerPrefs.GetInt(DonePrefix + key, 0) == 1;

    public static void MarkDone(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || IsDone(key)) return;

        PlayerPrefs.SetInt(DonePrefix + key, 1);

        // PlayerPrefs ไล่ดูทุก key เองไม่ได้ → จดรายชื่อไว้ให้ ResetAll ลบได้ครบ
        List<string> index = LoadIndex();
        if (!index.Contains(key))
        {
            index.Add(key);
            PlayerPrefs.SetString(IndexKey, string.Join(Separator.ToString(), index));
        }
        PlayerPrefs.Save();
    }

    public static void ResetKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        PlayerPrefs.DeleteKey(DonePrefix + key);

        List<string> index = LoadIndex();
        if (index.Remove(key))
            PlayerPrefs.SetString(IndexKey, string.Join(Separator.ToString(), index));

        PlayerPrefs.Save();
    }

    /// <summary>ล้างทุกบท + ล้าง key ที่ค้างอยู่ด้วย</summary>
    public static void ResetAll()
    {
        foreach (string key in LoadIndex())
            PlayerPrefs.DeleteKey(DonePrefix + key);

        PlayerPrefs.DeleteKey(IndexKey);
        TutorialSignals.ClearPending();
        PlayerPrefs.Save();
    }

    static List<string> LoadIndex()
    {
        var index = new List<string>();
        foreach (string key in PlayerPrefs.GetString(IndexKey, "").Split(Separator))
        {
            if (!string.IsNullOrWhiteSpace(key)) index.Add(key);
        }
        return index;
    }

    [ContextMenu("ล้างสถานะบทนี้")]
    void ResetThis() => ResetKey(SaveKey);
}
