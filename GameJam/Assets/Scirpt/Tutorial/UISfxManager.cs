using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// **ศูนย์กลางเสียงเอฟเฟกต์ UI** — เสียงกดปุ่ม / ชี้ปุ่ม / เปิด-ปิดหน้าต่าง / สำเร็จ / ล้มเหลว
/// ลากคลิปใส่ที่นี่**ที่เดียว** แล้วทั้งเกมใช้ร่วมกัน (ไม่ต้องไปลากทีละปุ่ม)
///
/// > ### 🔇 ไม่ใส่คลิป / ไม่มีตัวนี้ในซีน = เงียบ ไม่ error
/// > API เป็น **static ทั้งหมดและกัน null ให้แล้ว** — สคริปต์อื่นเรียก `UISfxManager.PlayClick()`
/// > ได้เลยโดยไม่ต้องเช็คอะไร ถ้าไม่มี manager ในซีนก็แค่ไม่มีเสียง
///
/// **วิธีเซ็ต:** สร้าง GameObject เปล่าในซีนแรก → ใส่สคริปต์นี้ → ลากคลิปใส่ช่องที่อยากได้ จบ
/// (เปิด <see cref="dontDestroyOnLoad"/> ไว้ ตัวเดียวอยู่ยาวข้ามซีนทั้งเกม)
///
/// UI, weapon, engine and music volumes are configured on this component.
///
/// ★ **เพลงพื้นหลัง (BGM) ก็อยู่ที่นี่** (เพิ่ม 2026-09-24) — ตั้ง `sceneBGM` ที่ตัวที่วางใน**แต่ละซีน**
/// (ตัวซ้ำในซีนใหม่บอกเพลงให้ตัวหลักก่อนทิ้งตัวเอง) เปลี่ยนซีนแล้วไล่เสียงข้ามให้เอง · เพลงเดียวกัน = เล่นต่อไม่เริ่มใหม่
/// (ต้องเปิด <see cref="dontDestroyOnLoad"/> ไว้ ไม่งั้นเพลงเริ่มใหม่ทุกซีน)
/// </summary>
[AddComponentMenu("UI/UI Sfx Manager")]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public class UISfxManager : MonoBehaviour
{
    /// <summary>ตัวที่ใช้อยู่ตอนนี้ (null = ไม่มีในซีน → ทุกคำสั่งเล่นเสียงจะเงียบเฉย ๆ)</summary>
    public static UISfxManager Instance { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetInstance() => Instance = null;

    /// <summary>เสียงที่ตั้งชื่อเรียกเองได้ — เรียกด้วย <see cref="Play(string)"/></summary>
    [System.Serializable]
    public class NamedClip
    {
        [Tooltip("ชื่อที่ใช้เรียก (ไม่สนตัวพิมพ์เล็ก/ใหญ่) เช่น \"buy\" / \"levelup\"")]
        public string key;
        public AudioClip clip;
        [Range(0f, 2f)] public float volume = 1f;
    }

    [Header("เสียงมาตรฐาน — เว้นว่างช่องไหน = ไม่มีเสียงนั้น (ไม่ error)")]
    [Tooltip("กดปุ่ม")]                 public AudioClip click;
    [Tooltip("เมาส์ชี้ปุ่ม")]            public AudioClip hover;
    [Tooltip("เปิดหน้าต่าง/แผง")]        public AudioClip open;
    [Tooltip("ปิดหน้าต่าง/แผง")]         public AudioClip close;
    [Tooltip("สำเร็จ — ซื้อของ/อัพเกรดผ่าน")] public AudioClip success;
    [Tooltip("ล้มเหลว — เงินไม่พอ/กดไม่ได้")] public AudioClip fail;

    [Header("Ship weapon sounds")]
    public AudioClip projectileShot;
    public AudioClip railShot;
    public AudioClip plasmaShot;
    public AudioClip getsukaShot;
    public AudioClip laserLoop;
    [Header("Ship engine loops")]
    public AudioClip normalJetLoop;
    public AudioClip boostJetLoop;
    [Range(0f, 1f)] public float gameplayVolume = 1f;
    [Range(0f, 1f)] public float weaponVolume = 1f;
    [Range(0f, 1f)] public float jetVolume = 0.35f;
    [Range(0f, 1f)] public float gameplaySpatialBlend = 1f;
    [Min(0.01f)] public float gameplayMinDistance = 3f;
    [Min(0.01f)] public float gameplayMaxDistance = 80f;

    [Header("เสียงเพิ่มเอง (เรียกด้วยชื่อ)")]
    [SerializeField] List<NamedClip> extraClips = new List<NamedClip>();

    [Header("ตั้งค่า")]
    [Tooltip("ตัวคูณเสียง UI ทั้งหมด (ตั้งค่าได้โดยตรงบน component)")]
    [Range(0f, 2f)] public float volume = 1f;

    [Tooltip("อยู่ข้ามซีน — เปิดไว้แล้ววางตัวเดียวในซีนแรกพอ")]
    public bool dontDestroyOnLoad = true;

    [Tooltip("คลิปเดียวกันห้ามดังซ้ำถี่กว่ากี่วินาที — กันเสียงซ้อนกันตอนหลายปุ่มยิงพร้อมกัน")]
    [Min(0f)] public float minRepeatInterval = 0.04f;

    [Header("ผูกเสียงกดให้ปุ่มอัตโนมัติ")]
    [Tooltip("สแกนหา Button ทั้งซีนแล้วผูกเสียงกดให้เอง (ตอนเริ่ม + ทุกครั้งที่โหลดซีนใหม่)\n\n" +
             "ปุ่มที่มี UIButtonEffect ไม่ต้องพึ่งอันนี้ — มันเล่นเสียงเองอยู่แล้ว\n" +
             "ปุ่มที่ถูกสร้างตอนรัน (เช่นแถบสกิล) ต้องเรียก UISfxManager.BindButtons(root) เอง")]
    public bool autoBindSceneButtons = true;

    public enum EmptyMusicMode
    {
        KeepCurrent,   // เล่นเพลงเดิมต่อ
        Silence,       // ซีนนี้เงียบ (ไล่เสียงลงจนหยุด)
    }

    [Header("★ เพลงของซีนนี้ (ตั้งที่ Gamemanager ของแต่ละซีน)")]
    [Tooltip("★ เพลงพื้นหลังของ**ซีนที่ตัวนี้วางอยู่** — ตั้งค่าทับที่ Gamemanager ในแต่ละซีนได้เลย ไม่ต้องพิมพ์ชื่อซีน\n" +
             "เข้าซีนแล้วเปลี่ยนเพลงให้เอง · **เพลงเดียวกับที่เล่นอยู่ = เล่นต่อ ไม่เริ่มใหม่**\n" +
             "ซีนที่ไม่มี UISfxManager (เช่น Loading) = เพลงเดิมเล่นต่อ")]
    [SerializeField] AudioClip sceneBGM;

    [Tooltip("ตัวคูณเสียงเฉพาะเพลงของซีนนี้ (เพลงที่มาสเตอร์ดังไม่เท่ากัน)")]
    [Range(0f, 2f)] [SerializeField] float sceneBGMVolume = 1f;

    [Tooltip("ช่อง sceneBGM ว่าง → KeepCurrent = เล่นเพลงเดิมต่อ / Silence = ซีนนี้เงียบ")]
    [SerializeField] EmptyMusicMode whenNoSceneBGM = EmptyMusicMode.KeepCurrent;

    [Header("เพลงพื้นหลัง — ตั้งค่ารวม (ใช้ค่าของตัวในซีนแรก) ในหน้า Setting")]
    [Tooltip("เวลาไล่เสียงตอนเปลี่ยน/หยุดเพลง (วินาที) — 0 = ตัดทันที")]
    [Min(0f)] [SerializeField] float musicFadeTime = 1f;

    [Tooltip("ตัวคูณเพลงทั้งหมด (ตั้งค่าได้โดยตรงบน component)")]
    [Range(0f, 2f)] [SerializeField] float musicVolume = 1f;

    // เพลง 2 ช่องไว้ไล่เสียงข้ามกัน (crossfade) — ช่องที่ _active ชี้คือเพลงปัจจุบัน
    AudioSource[] _music;
    float[] _musicFade = new float[2];      // 0..1 ของแต่ละช่อง
    float[] _musicClipVol = new float[2];   // volume ของ entry ที่เล่นในช่องนั้น
    int _active;

    // ปุ่มที่ผูกไปแล้ว — กันผูกซ้ำจนเสียงดัง 2 รอบตอนสแกนใหม่
    readonly HashSet<Button> _bound = new HashSet<Button>();

    // คลิป → เวลาที่เล่นล่าสุด (กันดังซ้ำถี่)
    readonly Dictionary<AudioClip, float> _lastPlayed = new Dictionary<AudioClip, float>();
    AudioSource _uiSource;
    sealed class LoopVoice
    {
        public Behaviour owner;
        public AudioSource source;
        public float gain;
        public bool paused;
    }
    sealed class ShotVoice
    {
        public AudioSource source;
        public float gain;
    }
    readonly Dictionary<Behaviour, LoopVoice> _loops = new Dictionary<Behaviour, LoopVoice>();
    readonly List<Behaviour> _expiredLoops = new List<Behaviour>();
    readonly List<ShotVoice> _shots = new List<ShotVoice>();

    void Awake()
    {
        // มีได้ตัวเดียว — ตัวที่มาทีหลังทิ้งตัวเอง (เผื่อซีนใหม่มี manager ติดมาด้วย)
        // ★ ทิ้งแค่ **component** ไม่ใช่ทั้ง GameObject — `Gamemanager.prefab` มี BattleQuestController /
        //   QuestButtonAnimation อยู่บน object เดียวกัน ทิ้งทั้งก้อน = สคริปต์ของซีนนั้นหายไปด้วย (แก้ 2026-09-24)
        // ★ ตัวเก่าที่ไม่ได้ข้ามซีน (dontDestroyOnLoad ปิด) กำลังจะถูกทำลายไปกับซีนเก่า
        //   → ตัวใหม่ต้องรับช่วงแทน ไม่งั้นทิ้งตัวเองแล้วตัวเก่าก็หาย = ซีนใหม่เงียบทั้งซีน (แก้ 2026-09-25)
        if (Instance != null && Instance != this && !Instance.dontDestroyOnLoad &&
            Instance.gameObject.scene != gameObject.scene)
            Instance = null;

        if (Instance != null && Instance != this)
        {
            // ★ ก่อนทิ้งตัวเอง บอกตัวหลักว่า "ซีนนี้ใช้เพลงนี้" — เพลงของซีนตั้งที่ตัวในซีนนั้นได้เลย
            Instance.ApplySceneMusic(sceneBGM, sceneBGMVolume, whenNoSceneBGM);
            Destroy(this);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad)
        {
            // ★ มีสคริปต์อื่นอยู่บน object เดียวกัน → ห้าม DontDestroyOnLoad ทั้งก้อน
            //   ไม่งั้นสคริปต์ของซีนแรก (BattleQuestController ที่ spawn point ว่าง) ลอยตามไปทุกซีน
            //   → ย้ายตัวเองไปอยู่ object ใหม่ที่มีแค่ตัวนี้แทน
            if (HasOtherComponents())
            {
                MoveToOwnPersistentObject();
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        if (autoBindSceneButtons) BindAllInScene();
    }

    // ซีนแรก (หรือเปิด dontDestroyOnLoad ไว้ = ทุกซีน) — ตัวหลักเล่นเพลงของซีนตัวเอง
    // ซีนต่อ ๆ ไป ตัวซ้ำในซีนนั้นเป็นคนบอกเพลงให้ตอน Awake
    void Start()
    {
        // ตัวซ้ำ / ตัวที่ย้ายไป object ใหม่แล้ว — Destroy(this) ยังไม่ทันทำงาน Start อาจยังถูกเรียก
        if (Instance != this) return;

        // ★ สคริปต์ในซีนสั่งเพลงไปก่อนแล้ว (เพลงของบท / BGMusicNode ตัวแรกของกราฟ) — Start ของใครมาก่อนไม่แน่นอน
        //   ห้ามเอาเพลงประจำซีนไปทับ (แก้ 2026-09-25)
        if (_music != null) return;
        ApplySceneMusic(sceneBGM, sceneBGMVolume, whenNoSceneBGM);
    }

    void OnDestroy()
    {
        foreach (var voice in _loops.Values) DestroyVoice(voice.source);
        _loops.Clear();
        foreach (var voice in _shots) DestroyVoice(voice.source);
        _shots.Clear();
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoBindSceneButtons) BindAllInScene();
    }

    bool HasOtherComponents()
    {
        foreach (Component c in GetComponents<Component>())
            if (c != null && c != this && !(c is Transform)) return true;
        return transform.childCount > 0;   // ลูกก็ลอยตามไปด้วย — ถือว่าไม่ใช่ของเราเหมือนกัน
    }

    /// <summary>สร้าง object ใหม่ที่มีแค่ UISfxManager → ก๊อปค่าที่ตั้งไว้ไปให้ → ตัวนั้นเป็นตัวหลักข้ามซีนแทน
    /// แล้วทิ้ง component นี้ (object เดิมกับสคริปต์อื่นอยู่ในซีนตามปกติ)</summary>
    void MoveToOwnPersistentObject()
    {
        var holder = new GameObject("UISfxManager (ข้ามซีน)");
        holder.SetActive(false);   // เลื่อน Awake ของตัวใหม่ไว้จนก๊อปค่าเสร็จ

        var copy = holder.AddComponent<UISfxManager>();
        copy.click   = click;
        copy.hover   = hover;
        copy.open    = open;
        copy.close   = close;
        copy.success = success;
        copy.fail    = fail;
        copy.extraClips           = new List<NamedClip>(extraClips);
        copy.volume               = volume;
        copy.dontDestroyOnLoad    = true;
        copy.minRepeatInterval    = minRepeatInterval;
        copy.autoBindSceneButtons = autoBindSceneButtons;
        copy.sceneBGM             = sceneBGM;
        copy.sceneBGMVolume       = sceneBGMVolume;
        copy.whenNoSceneBGM       = whenNoSceneBGM;
        copy.musicFadeTime        = musicFadeTime;
        copy.musicVolume          = musicVolume;
        copy.projectileShot = projectileShot; copy.railShot = railShot;
        copy.plasmaShot = plasmaShot; copy.getsukaShot = getsukaShot;
        copy.laserLoop = laserLoop; copy.normalJetLoop = normalJetLoop; copy.boostJetLoop = boostJetLoop;
        copy.gameplayVolume = gameplayVolume; copy.weaponVolume = weaponVolume; copy.jetVolume = jetVolume;
        copy.gameplaySpatialBlend = gameplaySpatialBlend;
        copy.gameplayMinDistance = gameplayMinDistance; copy.gameplayMaxDistance = gameplayMaxDistance;

        Instance = null;
        holder.SetActive(true);    // → copy.Awake() เป็น Instance + DontDestroyOnLoad (ไม่มีอะไรติดไปแล้ว)
        Destroy(this);
    }

    // ─── API (static — เรียกได้เลย ไม่ต้องเช็ค null) ──────────────────────────

    /// <summary>เสียงกดปุ่ม</summary>
    public static void PlayClick() => PlayOn(Instance != null ? Instance.click : null);

    /// <summary>เสียงเมาส์ชี้ปุ่ม</summary>
    public static void PlayHover() => PlayOn(Instance != null ? Instance.hover : null);

    /// <summary>เสียงเปิดหน้าต่าง/แผง</summary>
    public static void PlayOpen() => PlayOn(Instance != null ? Instance.open : null);

    /// <summary>เสียงปิดหน้าต่าง/แผง</summary>
    public static void PlayClose() => PlayOn(Instance != null ? Instance.close : null);

    /// <summary>เสียงสำเร็จ (ซื้อของ/อัพเกรดผ่าน)</summary>
    public static void PlaySuccess() => PlayOn(Instance != null ? Instance.success : null);

    /// <summary>เสียงล้มเหลว (เงินไม่พอ/กดไม่ได้)</summary>
    public static void PlayFail() => PlayOn(Instance != null ? Instance.fail : null);

    /// <summary>เล่นเสียงที่ตั้งชื่อไว้ใน <c>extraClips</c> — ไม่มีชื่อนี้ = เงียบเฉย ๆ</summary>
    public static void Play(string key)
    {
        if (Instance == null || string.IsNullOrWhiteSpace(key)) return;

        for (int i = 0; i < Instance.extraClips.Count; i++)
        {
            NamedClip entry = Instance.extraClips[i];
            if (entry == null || !string.Equals(entry.key, key, System.StringComparison.OrdinalIgnoreCase))
                continue;

            Instance.PlayInternal(entry.clip, entry.volume);
            return;
        }
    }

    /// <summary>เล่นคลิปที่ส่งมาเอง (เช่นเสียงเฉพาะของปุ่มนั้น) — null = เงียบ</summary>
    public static void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        if (Instance != null) Instance.PlayInternal(clip, volume);
        // No manager: remain silent; never create an unconfigured fallback source.
    }

    static void PlayOn(AudioClip clip)
    {
        if (clip == null || Instance == null) return;
        Instance.PlayInternal(clip, 1f);
    }

    void PlayInternal(AudioClip clip, float clipVolume)
    {
        if (clip == null) return;

        // คลิปเดิมเพิ่งดังไปหยก ๆ = ข้าม (กันเสียงซ้อนกันจนแตกตอนหลายปุ่มยิงพร้อมกัน)
        if (minRepeatInterval > 0f &&
            _lastPlayed.TryGetValue(clip, out float last) &&
            Time.unscaledTime - last < minRepeatInterval)
            return;

        _lastPlayed[clip] = Time.unscaledTime;

        // เงียบเพราะระดับเสียงเป็น 0 — บอกครั้งเดียว ไม่งั้นหาสาเหตุยาก (ใส่คลิปแล้วแต่ไม่มีเสียง)
        if (!_warnedMuted && volume * clipVolume <= 0f)
        {
            _warnedMuted = true;
            Debug.LogWarning("[UISfxManager] UI sound volume is zero.", this);
        }

        if (_uiSource == null)
        {
            _uiSource = gameObject.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.spatialBlend = 0;
            _uiSource.ignoreListenerPause = true;
        }
        _uiSource.volume = SpaceGarage.GarageSettings.SfxVolume;
        _uiSource.PlayOneShot(clip, volume * clipVolume);
    }

    bool _warnedMuted;

    // ─── เพลงพื้นหลัง (BGM) ──────────────────────────────────────────────────

    /// <summary>เพลงที่เล่นอยู่ตอนนี้ (null = เงียบ)</summary>
    public static AudioClip CurrentMusic =>
        Instance != null && Instance._music != null && !Instance._stopping && Instance._music[Instance._active].isPlaying
            ? Instance._music[Instance._active].clip : null;

    /// <summary>เปลี่ยนเพลง (ไล่เสียงข้ามกัน) — **เพลงเดียวกับที่เล่นอยู่ = เล่นต่อ ไม่เริ่มใหม่** · null = หยุด
    /// ใช้ตอนอยากเปลี่ยนเพลงกลางซีน (บอส / คัตซีน) — เปลี่ยนซีนแล้วกลับไปใช้เพลงของซีนตามลิสต์</summary>
    public static void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (Instance != null) Instance.PlayMusicInternal(clip, volume);
    }

    /// <summary>หยุดเพลง (ไล่เสียงลง)</summary>
    public static void StopMusic()
    {
        if (Instance != null) Instance.PlayMusicInternal(null, 1f);
    }

    /// <summary>เพลงของซีนที่เพิ่งเข้า — มาจากช่อง sceneBGM ของ UISfxManager ที่วางอยู่ในซีนนั้น</summary>
    void ApplySceneMusic(AudioClip clip, float clipVolume, EmptyMusicMode whenEmpty)
    {
        if (clip != null)                          PlayMusicInternal(clip, clipVolume);
        else if (whenEmpty == EmptyMusicMode.Silence) PlayMusicInternal(null, 1f);
        // KeepCurrent = ไม่แตะ เพลงเดิมเล่นต่อ
    }

    void PlayMusicInternal(AudioClip clip, float clipVolume)
    {
        EnsureMusicSources();

        AudioSource current = _music[_active];

        // หยุดเพลง — Update ไล่ทุกช่องลงจนเงียบแล้ว Stop ให้เอง
        if (clip == null)
        {
            _stopping = true;
            if (musicFadeTime <= 0f)
                for (int i = 0; i < 2; i++) { _music[i].Stop(); _musicFade[i] = 0f; }
            UpdateMusicVolumes();
            return;
        }

        // เพลงเดิมที่ยังเล่นอยู่ (รวมตอนกำลังไล่ลงเพราะสั่งหยุด) — เล่นต่อ ไม่เริ่มใหม่
        if (current.clip == clip && current.isPlaying)
        {
            _stopping = false;
            _musicClipVol[_active] = clipVolume;
            return;
        }

        // สลับช่อง: ตัวเก่าไล่ลง ตัวใหม่ไล่ขึ้น
        _stopping = false;
        _active = 1 - _active;
        AudioSource next = _music[_active];
        next.clip = clip;
        next.time = 0f;
        _musicClipVol[_active] = clipVolume;
        _musicFade[_active] = musicFadeTime <= 0f ? 1f : 0f;
        next.Play();

        if (musicFadeTime <= 0f)
        {
            _music[1 - _active].Stop();
            _musicFade[1 - _active] = 0f;
        }
        UpdateMusicVolumes();
    }

    void EnsureMusicSources()
    {
        if (_music != null) return;

        _music = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake  = false;
            src.loop         = true;
            src.spatialBlend = 0f;
            src.volume       = 0f;
            _music[i] = src;
        }
    }

    public static void PlayWeaponShot(SpaceGarage.ShipWeaponType type, Transform origin)
    {
        var m = Instance;
        if (m == null || origin == null || Time.timeScale <= 0) return;
        AudioClip clip = type == SpaceGarage.ShipWeaponType.RailGun ? m.railShot :
            type == SpaceGarage.ShipWeaponType.Plasma ? m.plasmaShot :
            type == SpaceGarage.ShipWeaponType.Getsuka ? m.getsukaShot : m.projectileShot;
        if (clip == null) return;
        var source = m.CreateGameplaySource(clip, false);
        source.transform.position = origin.position;
        source.volume = m.gameplayVolume * m.weaponVolume * SpaceGarage.GarageSettings.SfxVolume;
        source.Play();
        m._shots.Add(new ShotVoice { source = source, gain = m.weaponVolume });
    }

    public static void SetLaserLoop(Behaviour owner, bool active)
    {
        if (Instance != null) Instance.SetLoop(owner, active ? Instance.laserLoop : null, Instance.weaponVolume);
    }

    public static void SetJetLoop(Behaviour owner, bool flying, bool boosting)
    {
        if (Instance != null) Instance.SetLoop(owner,
            flying ? (boosting ? Instance.boostJetLoop : Instance.normalJetLoop) : null, Instance.jetVolume);
    }

    public static void StopLoop(Behaviour owner)
    {
        if (Instance == null || owner == null || !Instance._loops.TryGetValue(owner, out var voice)) return;
        DestroyVoice(voice.source);
        Instance._loops.Remove(owner);
    }

    AudioSource CreateGameplaySource(AudioClip clip, bool loop)
    {
        var root = new GameObject("SFX_" + clip.name);
        root.transform.SetParent(transform, false);
        var source = root.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = clip;
        source.loop = loop;
        source.spatialBlend = gameplaySpatialBlend;
        source.minDistance = gameplayMinDistance;
        source.maxDistance = Mathf.Max(gameplayMinDistance, gameplayMaxDistance);
        source.dopplerLevel = 0;
        return source;
    }

    static void DestroyVoice(AudioSource source)
    {
        if (source == null) return;
        source.Stop();
        Destroy(source.gameObject);
    }

    void SetLoop(Behaviour owner, AudioClip clip, float gain)
    {
        if (owner == null) return;
        if (clip == null || !owner.isActiveAndEnabled) { StopLoop(owner); return; }
        if (!_loops.TryGetValue(owner, out var voice))
        {
            voice = new LoopVoice { owner = owner, source = CreateGameplaySource(clip, true) };
            _loops.Add(owner, voice);
        }
        if (voice.source.clip != clip) { voice.source.Stop(); voice.source.clip = clip; voice.paused = false; }
        voice.gain = gain;
        voice.source.transform.position = owner.transform.position;
        voice.source.volume = gameplayVolume * gain * SpaceGarage.GarageSettings.SfxVolume;
        if (Time.timeScale > 0 && !voice.paused && !voice.source.isPlaying) voice.source.Play();
    }

    void UpdateGameplayAudio()
    {
        _expiredLoops.Clear();
        foreach (var pair in _loops)
        {
            var v = pair.Value;
            if (pair.Key == null || !pair.Key.isActiveAndEnabled) { DestroyVoice(v.source); _expiredLoops.Add(pair.Key); continue; }
            v.source.transform.position = pair.Key.transform.position;
            v.source.volume = gameplayVolume * v.gain * SpaceGarage.GarageSettings.SfxVolume;
            if (Time.timeScale <= 0 && !v.paused) { v.source.Pause(); v.paused = true; }
            else if (Time.timeScale > 0 && v.paused) { v.source.UnPause(); v.paused = false; if (!v.source.isPlaying) v.source.Play(); }
        }
        foreach (var owner in _expiredLoops) _loops.Remove(owner);
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            var v = _shots[i];
            if (v.source == null || !v.source.isPlaying || Time.timeScale <= 0)
            { DestroyVoice(v.source); _shots.RemoveAt(i); }
            else v.source.volume = gameplayVolume * v.gain * SpaceGarage.GarageSettings.SfxVolume;
        }
    }

    void Update()
    {
        if (_uiSource != null) _uiSource.volume = SpaceGarage.GarageSettings.SfxVolume;
        UpdateGameplayAudio();
        if (_music == null) return;

        // ใช้ unscaled — บทสอน/เมนูหยุดเวลาไว้ เพลงก็ยังไล่เสียงได้
        float step = musicFadeTime <= 0f ? 1f : Time.unscaledDeltaTime / musicFadeTime;

        for (int i = 0; i < 2; i++)
        {
            bool isCurrent = i == _active && _music[i].clip != null && !_stopping;
            float target = isCurrent ? 1f : 0f;
            _musicFade[i] = Mathf.MoveTowards(_musicFade[i], target, step);

            // หยุดเฉพาะช่องที่กำลังไล่ลง — ช่องที่เพิ่งเริ่ม (fade ยัง 0 ในเฟรมแรก) ห้ามโดนหยุด
            if (target <= 0f && _musicFade[i] <= 0f && _music[i].isPlaying) _music[i].Stop();
        }

        UpdateMusicVolumes();
    }

    // ★ ความดังอ่านจากสไลเดอร์ทุกเฟรม — SounddSetting เซฟลง PlayerPrefs ทุกเฟรมอยู่แล้ว ลากแล้วดังเบาตามทันที
    void UpdateMusicVolumes()
    {
        float master = musicVolume * SpaceGarage.GarageSettings.MusicVolume;
        for (int i = 0; i < 2; i++)
            _music[i].volume = master * _musicClipVol[i] * _musicFade[i];
    }

    bool _stopping;   // สั่งหยุดเพลงแล้ว — ทุกช่องไล่ลงจนเงียบ

    // ─── ผูกเสียงกดให้ปุ่ม ─────────────────────────────────────────────────────

    /// <summary>ผูกเสียงกดให้ Button ทุกตัวที่อยู่ใต้ <paramref name="root"/>
    /// (รวมตัวที่ปิดอยู่) — **เรียกหลังสร้างปุ่มตอนรันเอง** เช่นแถบสกิล/รายการร้านค้า
    ///
    /// ผูกซ้ำตัวเดิมไม่มีผล (จำไว้แล้ว) → เรียกกี่รอบก็ได้</summary>
    public static void BindButtons(GameObject root)
    {
        if (Instance == null || root == null) return;

        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
            Instance.Bind(buttons[i]);
    }

    /// <summary>ผูกเสียงกดให้ปุ่มตัวเดียว</summary>
    public static void BindButton(Button button)
    {
        if (Instance == null) return;
        Instance.Bind(button);
    }

    void Bind(Button button)
    {
        if (button == null || !_bound.Add(button)) return;

        // ปุ่มที่มี UIButtonEffect เล่นเสียงเองตอนกดอยู่แล้ว — ผูกซ้ำจะดัง 2 รอบ
        foreach (var behaviour in button.GetComponents<MonoBehaviour>())
            if (behaviour != null && behaviour.GetType().Name == "UIButtonEffect") return;

        button.onClick.AddListener(PlayClick);
        if (button.GetComponent<UISfxButtonHover>() == null) button.gameObject.AddComponent<UISfxButtonHover>();
    }

    void BindAllInScene()
    {
        // ปุ่มที่โดนทำลายไปแล้ว (เปลี่ยนซีน) ค้างเป็น null ใน set — เก็บกวาดก่อนสแกนรอบใหม่
        _bound.RemoveWhere(b => b == null);

        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
            Bind(buttons[i]);
    }
}
