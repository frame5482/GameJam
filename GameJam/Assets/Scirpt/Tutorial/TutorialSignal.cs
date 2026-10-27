using System.Collections.Generic;
using UnityEngine;

/// <summary>ตัวบอกว่า "ให้บทไหนเริ่มทำ" — ยิง key ออกไป บทที่ตั้ง <c>startSignalKey</c> ตรงกันจะเริ่ม
///
/// <para><b>ข้ามซีนได้</b> — key ที่ยิงออกไปถูกจดค้างไว้ใน <b>PlayerPrefs</b> ด้วย
/// ถ้าตอนยิงยังไม่มีใครฟัง (บทนั้นอยู่อีกซีน) พอโหลดซีนนั้นแล้ว <see cref="TutorialManager"/>
/// จะมาหยิบ key ที่ค้างไว้เองตอน Start → เก็บ key ทิ้งทันทีที่ใช้ (ใช้ได้ครั้งเดียว)</para></summary>
public static class TutorialSignals
{
    const string PendingKey = "Tutorial_Pending";
    const char Separator = '\n';

    /// <summary>บทที่อยู่ในซีนเดียวกันฟังผ่าน event นี้</summary>
    public static event System.Action<string> OnSignal;

    /// <summary>ยิง key ออกไป — จดค้างไว้ใน PlayerPrefs ด้วย เผื่อบทที่รออยู่คนละซีน</summary>
    public static void Raise(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        AddPending(key);          // จดก่อน — ผู้ฟังในซีนนี้จะ Consume ทิ้งเองถ้ารับไปแล้ว
        OnSignal?.Invoke(key);
    }

    /// <summary>มี key นี้ค้างอยู่ไหม (ยิงมาจากซีนก่อนหน้า)</summary>
    public static bool IsPending(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return LoadPending().Contains(key);
    }

    /// <summary>หยิบ key ที่ค้างไปใช้แล้ว — เก็บทิ้ง (ไม่งั้นเข้าซีนนี้อีกรอบบทจะเริ่มซ้ำ)</summary>
    public static void Consume(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        List<string> pending = LoadPending();
        if (!pending.Remove(key)) return;

        SavePending(pending);
    }

    /// <summary>ล้าง key ที่ค้างทั้งหมด (ใช้ตอนเริ่มเกมใหม่ / รีเซ็ตบทสอน)</summary>
    public static void ClearPending()
    {
        PlayerPrefs.DeleteKey(PendingKey);
        PlayerPrefs.Save();
    }

    static void AddPending(string key)
    {
        List<string> pending = LoadPending();
        if (pending.Contains(key)) return;

        pending.Add(key);
        SavePending(pending);
    }

    static List<string> LoadPending()
    {
        var list = new List<string>();
        foreach (string key in PlayerPrefs.GetString(PendingKey, "").Split(Separator))
        {
            if (!string.IsNullOrWhiteSpace(key)) list.Add(key);
        }
        return list;
    }

    static void SavePending(List<string> pending)
    {
        PlayerPrefs.SetString(PendingKey, string.Join(Separator.ToString(), pending));
        PlayerPrefs.Save();       // ยิงแล้วมักเปลี่ยนซีนต่อทันที
    }
}

/// <summary>ยิง key ให้บทสอนจาก Inspector — **ไม่ต้องแก้สคริปต์ระบบเดิม**
///
/// วางที่ไหนก็ได้ แล้วเลือกวิธีใดวิธีหนึ่ง:
/// <list type="bullet">
/// <item>ลาก object ตัวนี้ใส่ <c>Button.onClick</c> → เลือก <c>TutorialSignal.Raise</c></item>
/// <item>ติ๊ก <see cref="raiseOnStart"/> = "ซีนนี้เริ่มแล้ว" (ยิงครั้งเดียวตอน Start)</item>
/// <item>แปะไว้ที่แผง แล้วติ๊ก <see cref="raiseOnEnable"/> = "แผงนี้เปิดแล้ว"</item>
/// <item>แปะไว้ที่ <b>Collider ที่ติ๊ก Is Trigger</b> แล้วติ๊ก <see cref="raiseOnPlayerEnter"/>
///       = "ผู้เล่นเดินผ่านจุดนี้แล้ว"</item>
/// <item>ติ๊ก <see cref="raiseOnChapter"/> + ใส่ <see cref="chapterNumber"/> = "เกมอยู่บทนี้แล้ว"</item>
/// <item>ผูกกับ <c>InteractableObject.onClicked</c> / Animation Event</item>
/// </list></summary>
public class TutorialSignal : MonoBehaviour
{
    [Tooltip("key ที่ยิงตอนเรียก Raise() แบบไม่ใส่ชื่อ — ต้องตรงกับ startSignalKey ของบทที่จะให้เริ่ม")]
    public string signalKey;

    [Tooltip("★ ยิงครั้งเดียวตอน**ซีนเริ่ม** (Start) — ใช้เปิดบทแรกของซีนนี้\n" +
             "ต่างจาก raiseOnEnable: อันนั้นยิงใหม่ทุกครั้งที่ object ถูกเปิด โดยรอ Update และยกเลิกถ้าถูกปิดก่อน")]
    public bool raiseOnStart;

    [Tooltip("ยิงเองทุกครั้งที่ object นี้ถูกเปิด (แปะที่แผง = \"แผงนี้เปิดแล้ว\")")]
    public bool raiseOnEnable;

    [Header("--- ยิงตอนผู้เล่นเดินผ่าน ---")]
    [Tooltip("★ ต้องมี Collider ที่ติ๊ก Is Trigger ที่ object นี้ — ผู้เล่นเดินเข้าเขตแล้วยิง key")]
    public bool raiseOnPlayerEnter;

    [Tooltip("Tag ของผู้เล่น")]
    public string playerTag = "Player";

    [Tooltip("ยิงครั้งเดียว — เดินเข้า-ออกซ้ำก็ไม่ยิงอีก (ปิด = ยิงทุกครั้งที่เดินเข้า)")]
    public bool triggerOnce = true;

    [Header("--- ยิงตอนอยู่บทที่กำหนด (เพิ่ม 2026-09-25) ---")]
    [Tooltip("★ ยิง key เมื่อเกมอยู่ที่บท chapterNumber — เช็คตอนซีนเริ่ม + ตอนบทเปลี่ยนระหว่างอยู่ในซีนนี้\n" +
             "เช่น วางในซีน Room + chapterNumber = 2 → เข้าบท 2 แล้วกลับห้อง บทสอนที่รอ key นี้เริ่ม\n" +
             "ยิงซ้ำทุกครั้งที่เข้าซีนตอนอยู่บทนั้น — กันสอนซ้ำด้วย playOnce ของ TutorialManager (ค่าเริ่มต้นติ๊กอยู่แล้ว)")]
    public bool raiseOnChapter;

    [Tooltip("เลขบท (chapterNumber ของ StoryChapterData)")]
    public int chapterNumber = 1;

    [Tooltip("ติ๊ก = บทนี้ **หรือบทหลังจากนั้น** ก็ยิง (เผื่อผู้เล่นข้ามผ่านบทนั้นไปก่อนเจอซีนนี้)")]
    public bool orLaterChapters;

    bool _triggered;
    bool _enableSignalPending;
    int _lastCheckedChapter = int.MinValue;   // ยิงไปแล้วสำหรับบทไหน — กันยิงซ้ำทุกเฟรมในซีนเดียวกัน

    void Awake()
    {
        if (!raiseOnPlayerEnter) return;

        // ไม่มี trigger = ไม่มีอะไรมาเรียก OnTriggerEnter ให้เลย
        Collider[] colliders = GetComponents<Collider>();
        bool hasTrigger = false;
        foreach (Collider c in colliders)
        {
            if (c.isTrigger) { hasTrigger = true; break; }
        }

        if (!hasTrigger)
            Debug.LogWarning("[TutorialSignal] ติ๊ก raiseOnPlayerEnter ไว้ แต่ object นี้ไม่มี Collider " +
                             "ที่ติ๊ก Is Trigger — จะไม่ยิง key ให้", this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!raiseOnPlayerEnter) return;
        if (triggerOnce && _triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;
        Raise();
    }

    void OnEnable()
    {
        // UI owners may hide their initially enabled panels in Awake/Start.
        // Wait until Update so a panel hidden during initialization cannot
        // leave a false "opened" signal in the persistent pending queue.
        _enableSignalPending = raiseOnEnable;
    }

    void OnDisable()
    {
        _enableSignalPending = false;
    }

    bool HasEarlierEnableSender()
    {
        // Scene overrides can add a second copy of the prefab's signal.
        // Sending twice can re-add a key just consumed by TutorialManager.
        foreach (var sender in GetComponents<TutorialSignal>())
        {
            if (sender == this) return false;
            if (sender.isActiveAndEnabled && sender.raiseOnEnable &&
                sender.signalKey == signalKey) return true;
        }
        return false;
    }

    /// <summary>ยิงตอนซีนเริ่ม — จังหวะนี้ทุกตัวในซีน <c>Awake</c>/<c>OnEnable</c> จบแล้ว
    /// <see cref="TutorialManager"/> จึงรับได้แน่นอน ไม่ว่า <c>Start</c> ของใครจะมาก่อนมาหลัง
    /// (มาทีหลัง = หยิบจาก PlayerPrefs ที่ <see cref="TutorialSignals.Raise"/> จดไว้ให้)</summary>
    [SerializeField, Min(0)] int currentChapter;
    public void SetChapter(int chapter) { currentChapter = chapter; CheckChapter(); }

    void Start()
    {
        if (raiseOnStart) Raise();
        CheckChapter();
    }

    void Update()
    {
        if (_enableSignalPending)
        {
            _enableSignalPending = false;
            if (raiseOnEnable && !HasEarlierEnableSender()) Raise();
        }
        // บทเปลี่ยนระหว่างอยู่ในซีนนี้ (เช่นจบรอบ Dispatch แล้วกด "ไปต่อ") — เทียบเลขเฉย ๆ ไม่แพง
        if (raiseOnChapter && currentChapter != _lastCheckedChapter) CheckChapter();
    }

    /// <summary>★ อยู่บทที่ตั้งไว้ไหม → ยิง key (ครั้งเดียวต่อเลขบทต่อการเข้าซีน)</summary>
    void CheckChapter()
    {
        if (!raiseOnChapter) return;

        int current = currentChapter;
        if (current == _lastCheckedChapter) return;
        _lastCheckedChapter = current;

        bool match = orLaterChapters ? current >= chapterNumber : current == chapterNumber;
        if (!match) return;

        Debug.Log($"[TutorialSignal] อยู่บท {current} (ตั้งไว้ {chapterNumber}{(orLaterChapters ? " ขึ้นไป" : "")}) → ยิง '{signalKey}'", this);
        Raise();
    }

    public void Raise() => TutorialSignals.Raise(signalKey);

    /// <summary>ยิง key อื่นโดยไม่ต้องใช้ช่อง signalKey (พิมพ์ใส่ในช่องของ UnityEvent ได้เลย)</summary>
    public void Raise(string key) => TutorialSignals.Raise(key);
}
