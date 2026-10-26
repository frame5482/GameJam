# Combat HUD ตัวอย่าง — UI Toolkit

สร้างตามคำขอ 2026-10-26 เป็นตัวอย่างที่ปิด/แทนได้ ไม่แก้หรือปิด MainUI และ ShipHUD แบบ UGUI เดิม

**ข้อกำหนดล่าสุด: UI จริงใช้ Canvas/UGUI ไม่ใช่ UI Toolkit** ติด **ShipHUD** ไว้บน **MainUI** ใน SampleScene แล้ว เลือก MainUI เพื่อผูก TMP Text, Slider หรือ Image Filled ของจริง ข้อมูลที่ต้องลากและวิธีส่งรายการมาร์กอยู่ใน SpaceGarage/README.md ส่วน Toolkit ในโฟลเดอร์นี้คงเป็นตัวอย่างที่ปิดได้เท่านั้น

## Canvas ของจริง

- สคริปต์ **ShipHUD** อยู่ใน Add Component เมนู **Space Garage > UI > Canvas Combat HUD** ใช้บน Canvas หรือ panel ที่จัดไว้ได้ ไม่สร้างหน้าตา runtime สำรอง
- MainUI ผูก ShipController ไว้แล้ว เพิ่ม **Auto Find Player** เพื่อค้น Tag Player เมื่อไม่ได้ผูกหรือเปลี่ยนยาน
- ลาก TMP Text ลง Health Text / Boost Text / Speed Text / Dodge Cooldown Text ตามข้อมูลที่จะใช้
- ลาก Slider หรือ Image แบบ Filled ลง Health / Boost / Dodge Ready แต่ละกลุ่ม เลือกใช้เท่าที่ UI จริงมีได้
- แถววัสดุของจริงติด ShipTrackedMaterialRow ผูก Image/TMP ของแถวแล้วทำเป็น Prefab ลากลง **Material Row Prefab** และลาก RectTransform พื้นที่รายการลง **Material Container** บน ShipHUD ระบบจะเสกวัสดุที่มาร์กเข้าไปตาม SetTrackedMaterials และใช้แถวซ้ำ ไม่แตะลูกที่ผู้ใช้จัดไว้ใน Container ถ้าไม่ใส่ Container ยังใช้ array Material Rows แบบเดิมได้
- Container ต้องจัด VerticalLayoutGroup/GridLayoutGroup หรือวิธีจัดตำแหน่งของตัวเองไว้ ระบบไม่สร้างหรือแก้ layout ให้ ไม่สร้างแถวสำรองเมื่อไม่ใส่ Row Prefab
- ตอนนี้ MainUI ในไฟล์ซีนยังไม่มี Text/Slider/Image ลูกให้ผูก จึงเว้นช่องไว้ ไม่สร้างหรือย้าย layout ให้ ผู้ใช้ต้องเตรียมกราฟิกและลากช่องเหล่านี้เอง

## เปิดดูและปิด

วาง `CombatHUD_Example.prefab` ไว้ใน `Scenes/SampleScene.unity` แล้ว เปิดซีนและกด Play เพื่อแสดงข้อมูลจริงจาก PlayerShip

- ปิดตัวอย่างทั้งหมด: ยกเลิก Active ที่ **CombatHUD_Example** ใน Hierarchy หรือปิด component **ShipCombatHUDToolkit**
- ซ่อนชั่วคราว: เอาติ๊ก **Visible** ออก หรือเรียก `SetVisible(false)` คืนด้วย `SetVisible(true)`
- ปิดเฉพาะเป้าเมาส์: เอาติ๊ก **Show Mouse Reticle** ออก
- ลบ GameObject ตัวอย่างได้โดยไม่กระทบยาน ศัตรู กล้อง หรือ UGUI เดิม ถ้าใช้คนละซีนลาก Prefab นี้ลงได้เลย จะค้น Tag Player ถ้าไม่ได้ลาก Ship

## โครงไฟล์

| ไฟล์ | หน้าที่ |
| --- | --- |
| CombatHUD_Example.prefab | GameObject ตัวอย่าง เปิด/ปิดหรือเอาออกจากซีนได้ |
| ../Scripts/ShipCombatHUDToolkit.cs | ผูกข้อมูลยานและรายการมาร์กกับ VisualElement ไม่แก้กติกาเกม |
| Resources/SpaceGarageHUD/CombatHUD.uxml | โครงหน้าจอ เปิดด้วย UI Builder ได้ |
| Resources/SpaceGarageHUD/CombatHUD.uss | สี ขนาด ตำแหน่ง และรูปแบบแถบ/แถววัสดุ |
| Resources/SpaceGarageHUD/CombatHUDTheme.tss | นำเข้า default runtime theme ของ Unity เพื่อให้ Label มีฟอนต์มาตรฐาน |
| ../Scripts/ShipMaterialRequirement.cs | ข้อมูลวัสดุที่แชร์กับ ShipHUD เดิม แยกไฟล์เพื่อไม่ผูก Toolkit กับ component UGUI |

หน้าตาใช้แผงเข้มโปร่งใส สีฟ้า/เขียว/ส้ม ข้อมูลอยู่ขอบจอ: สถานะบนซ้าย วัสดุมาร์กด้านขวา เลือด/บูสต์ล่างซ้าย ความเร็ว/ความพร้อมหลบล่างขวา เป้าเล็งตามเมาส์ และป้ายปุ่มควบคุม ไม่เปลี่ยน cursor ของระบบ ไม่แย่งการคลิกหรือโฟกัสเกม

## ปรับหรือเปลี่ยน UXML ของตัวอย่าง Toolkit

1. สร้างหรือทำสำเนา UXML/USS ไปแก้ใน UI Builder
2. ลาก UXML ใหม่ลงช่อง **Layout** บน ShipCombatHUDToolkit แล้วเริ่ม Play ใหม่
3. ถ้ามี PanelSettings ของจริง ลากลง **Panel Settings** ได้ ถ้าเว้นว่างตัวอย่างสร้าง panel ในหน่วยความจำแบบ Scale With Screen Size อ้างอิง 1920×1080, Match 0.5 และ sorting order 100 ให้ โดยทำลาย panel เมื่อเอา component ออก
4. คงชื่อ element ที่ต้องการผูกข้อมูลตามตารางด้านล่าง ชื่อที่ไม่อยู่ใน UXML จะถูกข้าม ไม่สร้าง widget สำรองแทน
5. ถ้าจะใช้สคริปต์ของจริงเอง ให้ปิด/เอา GameObject ตัวอย่างออกแล้วใช้ HUD ของคุณได้เลย ไม่ต้องแก้ระบบยาน

| ชื่อใน UXML | ชนิด | ข้อมูล |
| --- | --- | --- |
| combat-hud | VisualElement | root สำหรับคลาส boosting/low-health |
| health-value | Label | HP ปัจจุบัน / สูงสุด |
| health-fill | VisualElement | เติม width 0–100% ตามเลือด |
| boost-value | Label | เกจบูสต์ปัจจุบัน / สูงสุด |
| boost-fill | VisualElement | เติมตามเกจ Shift boost |
| speed-value | Label | ความเร็วบินปัจจุบัน 1 ตำแหน่งทศนิยม |
| dodge-value | Label | READY / ROLLING / เวลาคูลดาวน์ |
| dodge-fill | VisualElement | ความพร้อมท่าหลบ 0–100% |
| mode-status | Label | สถานะ CRUISE / BOOST / EVADE / OFFLINE |
| mouse-reticle | VisualElement | เป้าเล็ง 30×30 ตามเมาส์ ปิดได้จาก Inspector |
| materials-list | VisualElement | container รับแถววัสดุที่ระบบมาร์กส่งมา |
| materials-empty | Label | ข้อความเมื่อไม่มีรายการมาร์ก |

แถววัสดุสร้างจากข้อมูลภายใน materials-list ใช้คลาส USS `material-row`, `material-icon`, `material-name`, `material-count`, `material-complete` ถ้าหน้าตาจริงเปลี่ยนโครงของแถวมากกว่านี้ ให้แก้ RebuildMaterialRows หรือใช้สคริปต์ UI ของคุณเอง ตัวอย่างนี้เป็น display-only จึงตั้ง PickingMode.Ignore และไม่รับคลิกปุ่มใน UXML

## เชื่อมข้อมูล

- **Ship**: ลาก ShipController ของยานผู้เล่น ตัวอย่างใน SampleScene ผูกไว้แล้ว
- `SetShip(controller)` เปลี่ยนยานที่ติดตามได้
- ระบบมาร์กภายนอกส่ง `SetTrackedMaterials(IEnumerable<ShipMaterialRequirement>)` เหมือน API ของ ShipHUD เดิม โดยส่ง asset ShipMaterialData และ requiredAmount
- HUD อ่านรูป/ชื่อจาก asset และจำนวนที่มีจาก ShipInventory อัปเดตเมื่อคลังเปลี่ยน ไม่สร้างรายการมาร์กเอง ไม่ใส่ตัวอย่างวัสดุปลอม และไม่อ่านรายการจาก component ShipHUD เดิมอัตโนมัติ ถ้าจะแสดงทั้งสอง HUD ให้ระบบมาร์กส่งให้ทั้งสองตัว
- `SetTrackedMaterials(null)` ล้างรายการ; เมื่อยังไม่มีรายการจะแสดง No materials marked

## ผลตรวจและข้อจำกัด

- คอมไพล์กับ DLL จริงของ Unity 6000.3.8f1 / Input System / UIElements / Cinemachine ผ่าน ไม่มี error/warning
- UXML ผ่าน XML parser และตรวจชื่อผูกครบ 12 จุด ตำแหน่งไฟล์ USS/TSS และ GUID ของ Prefab/สคริปต์/ยานในซีนตรวจแล้ว
- แก้รายงาน Toolkit ไม่ขึ้น: เปลี่ยนจาก CloneTree แทรกเองไปใช้ UIDocument.visualTreeAsset ซึ่งให้ Unity จัด lifecycle ของต้นไม้ UI กำหนด root เต็มจอชัดเจน และผูกข้อมูลใหม่เมื่อ UIDocument เปลี่ยน root ไม่ปล่อย reference ชี้ต้นไม้เก่าที่ไม่อยู่บน panel
- ตรวจใน Play Mode จริงผ่านสคริปต์ Editor ชั่วคราวและดู PNG จาก ScreenCapture แล้ว: พบ HUD/UIDocument/panel/UXML/theme ครบ root และ combat-hud มีขนาด 1920×1080 ภาพแสดง HP 100/100, Boost 100/100, Speed และ Dodge READY พร้อมแผงวัสดุว่าง ตัวอย่างขึ้นจริงหลังแก้ ไม่ได้อ้างผลนี้จากคอมไพล์อย่างเดียว
- การตรวจภาพนี้ไม่ได้ทดสอบการกดบูสต์/Dodge หรือ Canvas กราฟิกจริงที่ยังไม่ได้ผูก สคริปต์ตรวจและภาพชั่วคราวลบหลังตรวจตามกติกาโปรเจกต์ ไม่ทิ้งระบบอัตโนมัติสั่ง Play ไว้
- HUD แสดงเมื่อ Play; ใช้ UI Builder เปิด UXML เพื่อแก้และดูตัวอย่างใน Editor ไม่มีการสร้าง UI ค้างใน Edit Mode
- ข้อความตัวอย่างเป็นอังกฤษและใช้ default runtime theme ถ้าข้อความวัสดุภาษาไทยแสดงไม่ครบ ต้องเตรียม PanelSettings/TextSettings และฟอนต์ไทยของจริงเอง
- ไม่มีการย้าย ลบ ซ่อน หรือเปลี่ยนขนาด MainUI/Canvas/UGUI เดิม ผู้ใช้เลือกปิดชุดที่ไม่ต้องการเองได้
