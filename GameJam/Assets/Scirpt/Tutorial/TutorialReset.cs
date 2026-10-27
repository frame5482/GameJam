using System.Collections.Generic;
using UnityEngine;

/// <summary>ล้างสถานะ "สอนไปแล้ว" — ใช้ตอนทดสอบ และผูกกับ**ปุ่มเริ่มเกมใหม่**
///
/// ⚠️ `NEWGAME` ไม่ได้ล้าง PlayerPrefs ของบทสอนให้ → เริ่มเกมใหม่แล้วบทสอนจะไม่กลับมา
/// ถ้าไม่ผูก <see cref="ResetNow"/> ไว้กับปุ่มเดียวกัน</summary>
public class TutorialReset : MonoBehaviour
{
    [Tooltip("key ของบทที่จะล้าง (tutorialKey ของ TutorialManager) — **เว้นว่าง = ล้างทุกบท**")]
    public List<string> keys = new List<string>();

    [Tooltip("★ ทดสอบ: ล้างทุกครั้งที่ซีนเริ่ม — ⚠️ เอาติ๊กออกก่อน build")]
    public bool resetOnStart;

    void Awake()
    {
        // ต้องล้างก่อน TutorialManager.Start() ไม่งั้นบทเช็คว่า "ผ่านแล้ว" ไปก่อนแล้ว
        if (!resetOnStart) return;

        Debug.LogWarning("[TutorialReset] resetOnStart ติ๊กอยู่ — ล้างสถานะบทสอนทุกครั้งที่ซีนเริ่ม", this);
        ResetNow();
    }

    /// <summary>ล้างตามรายการใน <see cref="keys"/> (ว่าง = ล้างทุกบท) — ผูกกับปุ่มได้เลย</summary>
    [ContextMenu("ล้างสถานะบทสอน")]
    public void ResetNow()
    {
        if (keys == null || keys.Count == 0)
        {
            TutorialManager.ResetAll();
            Debug.Log("[TutorialReset] ล้างสถานะบทสอนทุกบท + key ที่ค้างอยู่แล้ว", this);
            return;
        }

        foreach (string key in keys)
            TutorialManager.ResetKey(key);

        // key ที่ยิงค้างไว้ต้องล้างด้วย ไม่งั้นบทที่รอ key นั้นจะเด้งขึ้นมาเองทันที
        TutorialSignals.ClearPending();
        Debug.Log($"[TutorialReset] ล้างสถานะบทสอน: {string.Join(", ", keys)}", this);
    }
}
