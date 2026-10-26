# ระบบควบคุมยานต้นแบบ

เพิ่มระบบซ่อมลูกค้าและ ResourceManager แล้ว ดูขั้นตอนตั้งค่าที่ [Repair/README.md](Repair/README.md) ฉาก `Scenes/Repair.unity` ผูกจุดเดิมและ prefab แล้ว ต้องใส่พูลวัสดุและ Canvas ของผู้ใช้ก่อนเล่นครบวงจร ShipController.Inventory/ShipPickup ใช้คลังกลางเมื่อมี ResourceManager; ไม่มีบันทึกลงดิสก์

อัปเดต 2026-10-26 — Unity 6000.3.8f1, Input System 1.18.0, บินแบบเครื่องบิน 3D เปลี่ยนระดับความสูงแกน Y ได้จริง ไม่ล็อกระนาบ

## พฤติกรรมที่ยืนยัน

- ยานบินไปข้างหน้าตลอดเวลาตามหัว (local +Z) ด้วย Speed แม้ไม่ได้กดปุ่ม ปล่อยปุ่มแล้วบินต่อในทิศปัจจุบัน
- S เชิดหัวไต่ระดับ (+Y), W กดหัวลดระดับ (-Y) เทียบกับท่าบินปกติ จำกัดมุม ±30 องศา กดค้างไม่ตีลังกา ความเร็วก้มเงยไม่เกิน 30 องศา/วินาที และค่อย ๆ รับ/คลายคำสั่ง
- A/D เอียงลำซ้าย/ขวาแล้วเปลี่ยนเส้นทางบินตามมุมเอียงจริง; ปล่อยค่อยคืนปีกตรง และยังบินต่อในทิศใหม่ ไม่หันไปแกนโลก -X/+X โดยตรง
- Handling กำหนดอัตราก้ม/เงยและอัตราเลี้ยว โดยจำกัดเพดานด้วย Pitch Degrees Per Second (30) และ Turn Degrees Per Second (45) ตามลำดับ หน่วยองศาต่อวินาที การเล็งปืนใช้เมาส์แยกจากทิศบิน
- การเปลี่ยนความเร็วรวมถึงบูสต์ใช้เวลาตอบสนอง 0.6 วินาที และอัตราเลี้ยวใช้เวลาตอบสนอง 0.4 วินาที ไม่เปลี่ยนทันที ปล่อยปุ่มก้มเงยแล้วค่อยหยุดหมุนและคงมุมนั้นเพื่อไต่/ลดระดับต่อ
- มุมเอียงเริ่มต้น 25 องศา หมุนลำยานทั้งชุดรวม collider ด้วย Rigidbody ไม่ใช่แค่เอียงกราฟิก
- กด Shift ซ้ายหรือขวาค้างเพื่อบูสต์ขณะเคลื่อนที่ เกจเริ่มต้น 100 ใช้ต่อวินาทีตาม Jet ปล่อยปุ่มแล้วฟื้นหลังเวลาหน่วง ถ้าเกจหมดต้องปล่อยก่อนใช้ครั้งใหม่
- กด Space หนึ่งครั้งขณะกด A/D เพื่อพุ่งซ้าย/ขวาแนวราบเท่านั้นพร้อมหมุนลำหลบ 360 องศา ระยะต้นแบบ 5 หน่วย พุ่งใน 0.6 วินาที แต่หมุนช้าลงเป็น 1.2 วินาที อมตะตลอดท่า และคูลดาวน์ 4 วินาทีหลังจบท่า กด Space เปล่า ๆ หรือ A+D พร้อมกันไม่ทำท่า ยานยังบินหน้าต่อ กล้องค่อยไล่ตามตำแหน่งโดยไม่หมุนตามลำ
- คลิกซ้ายค้างยิงจาก Muzzle ไปจุดใต้เมาส์ในฉาก 3D โดนวัตถุใช้จุดชน ไม่โดนใช้จุดไกลตามแนวเมาส์ ทั้งสองปากยิงเข้าจุดเดียว ไม่เปลี่ยนการควบคุมยาน
- E เก็บของในรัศมีที่กำหนดลง ShipInventory ของที่เก็บสำเร็จหายไป ไม่เพิ่มซ้ำเมื่อมีหลาย collider
- ไม่มี UI ชุดทดลองเดิมไม่มีโมเดลยาน ส่วน StarterShip เพิ่มรูปทรงพื้นฐานตามคำขอภายหลัง ใช้ทรงกลมเป็นกระสุน

## ไฟล์และหน้าที่

| ไฟล์ | หน้าที่ |
| --- | --- |
| Scripts/ShipController.cs | คุมอินพุต เคลื่อนที่ หันหัว เลือด Def เกจบูสต์ เก็บของ รวมสเตตัสและประกอบชิ้นส่วน |
| Scripts/ShipPartStats.cs | ScriptableObject ประเภทชิ้นส่วน Prefab สเตตัส และค่าบูสต์ของ Jet |
| Scripts/ShipGun.cs | ใส่ใน Prefab ปืน กำหนดกระสุน จุดยิง ความถี่และตัวคูณดาเมจ |
| Scripts/ShipProjectile.cs | เคลื่อนกระสุน ตรวจชนแบบ sphere sweep รับดาเมจจากปืน ไม่โดนเจ้าของ ไม่ใช้ Rigidbody |
| Scripts/IShipDamageable.cs | สัญญาสำหรับสิ่งที่รับดาเมจผ่าน TakeDamage(float) |
| Scripts/ShipInventory.cs | เก็บจำนวนวัสดุตาม Item ID ในรอบเล่น |
| Scripts/ShipPickup.cs | ใส่ของดรอป กำหนด Item ID และจำนวน ให้ E เก็บได้ |
| Scripts/ShipHUD.cs | ผูก UI ที่ผู้ใช้จัดไว้ แสดงเลือด เกจบูสต์ ความเร็ว และรับรายการวัสดุที่มาร์กจากระบบอื่น |
| Scripts/ShipTrackedMaterialRow.cs | ผูกแถว UI ของวัสดุหนึ่งรายการ: รูป ชื่อ จำนวนที่มี/ต้องการ |
| Scripts/ShipMaterialData.cs | ScriptableObject วัสดุ: รหัส ชื่อ และ Sprite |
| Scripts/ShipBoostEffect.cs | เปิด/ปิดเอฟเฟกต์บูสต์ที่อยู่ในแต่ละ Part Prefab รองรับ VisualEffect/VFX Graph |
| Scripts/ShipDodgeState.cs | ควบคุมเวลาท่าหลบ ทิศหมุน i-frame และคูลดาวน์ ด้วยนาฬิกาที่รับจากสคริปต์กลาง |
| Scripts/ShipDodgeCameraLag.cs | เพิ่มความหน่วงตำแหน่งและการเล็งกล้องเฉพาะช่วงพุ่งหลบ แล้วค่อยคืนค่ากล้องปกติ |
| Scripts/Enemy.cs | ประกอบยานศัตรู รวมสเตตัส บินไล่ ชะลอในระยะยิง รับดาเมจ และตาย |
| Scripts/EnemySpawnManager.cs | สุ่มข้อมูลชิ้นส่วนจากพูล 4 ช่อง และเกิดศัตรูรอบผู้เล่นตามเวลา |
| Scripts/EnemyAttackTelegraph.cs | เส้นบอกแนวโจมตีก่อน/ระหว่างยิง ใช้ Material ชั่วคราวในหน่วยความจำ |
| Scripts/EnemyCombatMotion.cs | คำนวณความเร็วเข้าหาหรือถอยห่างเพื่อยิงรักษาระยะด้านหน้าผู้เล่น |
| Scripts/EnemyFireCycle.cs | ควบคุมยิงเป็นชุด 1 วินาที/พัก 1.5 วินาที และเตือนก่อนชุดถัดไป |
| Scripts/ShipDamageHitbox.cs | ทำเครื่องหมาย Trigger ที่ให้ปืนและกระสุนรับเป็นจุดโดนดาเมจได้ |
| Scripts/IShipHitReceiver.cs | รับดาเมจพร้อมฝ่ายผู้ยิง จุดชน และแนวผิว สำหรับ HIT/เลขดาเมจ |
| Scripts/ShipCombatFeedback.cs | กำหนด Prefab HIT / HIT Monster / DMG กลางในซีน |
| Scripts/DamagePopup.cs | เติมเลขใน DMG Prefab เดิม หันเข้ากล้อง เคลื่อน/เฟดและทำลายตามค่าที่มีใน Prefab |
| Editor/ShipPrototypeBuilder.cs | เมนูสร้างซีนและ Prefab ทดลองในโฟลเดอร์ใหม่ ไม่เขียนทับซีนเดิม |

## ทดลองแบบเตรียมให้

### ตัวอย่างที่วางในซีนแล้ว (2026-10-26)

วาง StarterShip แบบ Prefab Instance ที่ `(0,0,0)` ใน `Assets/Scenes/SampleScene.unity` ชื่อ **PlayerShip (ShipController)** เลือก root นี้เพื่อดู ShipController, Rigidbody, BoxCollider และ ShipInventory ได้เลย ส่วนลูกมี GunSpawnPoint / ArmorPlateSpawnPoint / WingSpawnPoint / JetSpawnPoint ที่ผูกตำแหน่งเสกครบ 4 ช่องแล้ว

| ช่องบน ShipController | สิ่งที่ผูกให้ |
| --- | --- |
| Gun Part / Gun Mount | Gun.asset / ลูก GunSpawnPoint |
| Armor Plate Part / Armor Plate Mount | ArmorPlate.asset / ลูก ArmorPlateSpawnPoint |
| Wing Part / Wing Mount | Wing.asset / ลูก WingSpawnPoint |
| Jet Part / Jet Mount | Jet.asset / ลูก JetSpawnPoint |
| Inventory | ShipInventory บน root ยาน |

ข้อมูลแต่ละ asset มี Part Prefab ของตัวเอง; ใน GunPart มี ShipGun ผูก Muzzle และ StarterBullet.prefab ไว้แล้ว ไม่ต้องลากช่องเพิ่มเพื่อประกอบตัวอย่าง ตรวจโครงสร้างไฟล์และ reference ของ instance แล้ว แต่ยังไม่ได้เปิดซีนตรวจภาพ/ทดสอบ Play Mode จริง SampleScene มี CinemachineCamera ตามยานตามรายละเอียดด้านล่าง

### กล้องตามยาน Cinemachine 3.1.7

ตั้งค่าใน `Scenes/SampleScene.unity` โดยใช้ CinemachineCamera และ CinemachineBrain ที่ผู้ใช้เพิ่มไว้แล้ว ไม่สร้างกล้องซ้ำและไม่เพิ่มสคริปต์กล้องเอง

- **Main Camera > Cinemachine Brain**: ใช้ Brain เดิม Smart Update และ Late Update สำหรับส่งผลกล้องจริง
- **CinemachineCamera > Tracking Target**: PlayerShip (ShipController) / Transform root ของยาน ผูกไว้แล้ว Look At ใช้เป้าหมายเดียวกันผ่าน Custom Look At Target = false
- **Position Control = Cinemachine Follow**: เพิ่ม component และตั้ง Follow Offset `(0, 3, -10)` คือหลังยาน 10 หน่วย สูง 3 หน่วย; Binding Mode = Lock To Target No Roll ตามก้มเงย/เลี้ยวของยานแต่ไม่เอียงภาพตามปีก
- **Follow > Position Damping**: `(0.15, 0.15, 0.15)` และ Rotation Damping `(0.1, 0.1, 0)` ใช้เป็นค่าต้นแบบ
- **Rotation Control = Cinemachine Rotation Composer**: เพิ่ม component เล็งกลางยาน วางยานกลางภาพ Damping `(0.1, 0.1)` ปิด Lookahead/Dead Zone/Hard Limits
- คง FOV เดิม 50 องศา และตั้งตำแหน่งเริ่มต้นของกล้องทั้งสองที่ `(0, 3, -10)` มองกลับไปหายาน กด Play ได้โดยไม่ต้องต่อช่องเพิ่ม
- ปรับระยะ/ความสูงที่ Follow Offset และความนุ่มที่ Damping บน CinemachineCamera ไม่แก้ Transform ของ Main Camera ระหว่างเล่น เพราะ Brain เป็นผู้ควบคุม
- ขณะ Shift บูสต์จริง **ShipDodgeCameraLag** จะค่อยเพิ่มระยะหลังยานอีก 3 หน่วย (Follow Offset Z จาก -10 ไป -13 ในค่าตัวอย่าง) และค่อยกลับระยะเดิมเมื่อปล่อย/เกจหมด ไม่เปลี่ยน FOV หรือความสูงกล้อง ทำร่วมกับ Dodge lag ได้
- ช่องใหม่กลุ่ม **Boost Camera Pullback** บน ShipDodgeCameraLag: **Boost Extra Distance** ระยะถอยเพิ่ม 3 หน่วย, **Boost Pullback Time** ความหน่วงตอนถอย 0.35 วินาที, **Boost Return Time** ความหน่วงตอนกลับ 0.6 วินาที ตั้งใน SampleScene แล้ว ไม่ต้องต่อ reference เพิ่ม สคริปต์จำ Follow Offset ตอนเริ่มเล่นและคืนเมื่อปิด component
- ตรวจ serialized field กับซอร์ส Cinemachine ใน package จริง ตรวจ Script GUID, target, component references และ fileID ของซีนแล้ว ยังไม่ได้ตรวจภาพ/ทดสอบ Play Mode จริง

### Prefab ประกอบสำเร็จ StarterShip (เพิ่มตามคำขอ 2026-10-26)

แก้การอ้างอิง Material ตามรายงานยานสีชมพู: Prefab เดิมเขียน external reference ของ `.mat` และ `.asset` เป็น `type: 3` ซึ่งเป็นชนิดอ้างอิงไฟล์นำเข้า แก้เป็น `type: 2` สำหรับ NativeFormat asset ครบ StarterShip, ชิ้นส่วนทั้ง 4 และ StarterBullet รวมถึงข้อมูลสเตตัส 4 ช่อง โดยคง GUID เดิมทั้งหมด ตรวจว่า Material มีจริง Shader GUID ตรงกับ URP Unlit ใน package และตรวจไม่เหลือ native reference ชนิดผิดแล้ว ยังไม่ได้ตรวจภาพใน Unity จริง ไม่เพิ่มช่อง Inspector และไม่ต้องลาก Material ใหม่ถ้าไม่ได้ override Renderer ใน instance เอง

- ใช้ `Assets/SpaceGarage/Examples/StarterShip/StarterShip.prefab` ลากลงซีนได้เลย ไม่ต้องกดเมนูสร้างชุดทดลองก่อน
- มีรูปทรงพื้นฐานสีฟ้า ปืนด้านหน้า เกราะกลาง ปีกซ้ายขวารวมเป็น Wing ส่วนเดียว และเครื่องยนต์คู่ด้านหลัง พร้อมวัสดุ URP Unlit 4 สี
- ผูกข้อมูล Gun.asset / ArmorPlate.asset / Wing.asset / Jet.asset และ Prefab ของแต่ละส่วนไว้ครบ พร้อม StarterBullet.prefab ทรงกลม ปืนยิงไป local +Z
- จุดเสกอยู่บนลูกยานชื่อ GunSpawnPoint `(0, 0.14, 0.85)`, ArmorPlateSpawnPoint `(0, 0, 0)`, WingSpawnPoint `(0, 0, -0.1)`, JetSpawnPoint `(0, 0, -1.05)` ทั้ง 4 จุดผูกกับช่อง Mount บน ShipController แล้ว ขยับได้ตามต้องการ
- เห็นชิ้นส่วนประกอบใน Edit Mode ได้เลย โดย ShipController เก็บ reference ชิ้นตัวอย่างในฟิลด์ซ่อน assembledParts เมื่อ Awake จะปิดและถอดชุดตัวอย่างก่อนประกอบจริงจากข้อมูล เพื่อไม่ให้เกิดส่วน/ปืนซ้ำ ไม่มีช่อง Inspector ใหม่ที่ผู้ใช้ต้องกรอก
- ลาก Prefab ลงซีนที่มีกล้องมองเห็นยาน แล้วกด Play เพื่อทดลองบิน 3D หากใช้ SampleScene มีกล้อง Cinemachine ตามยาน instance เดิมให้แล้ว ถ้าสร้างยานใหม่แทนตัวเดิมต้องเปลี่ยน Tracking Target ให้ตรงยานใหม่
- ค่าเพิ่มสเตตัสแต่ละส่วนเริ่มที่ 0 ใช้ค่าพื้นฐานยาน Attack 10 / Def 0 / Speed 8 / Handling 360 / HP 100 / Boost 100 ก่อน ผู้ใช้เปลี่ยน Prefab/Stats ใน asset ได้เอง
- ตรวจ reference ภายใน/ข้าม asset ของไฟล์ตัวอย่าง 14 ไฟล์และตรวจคอมไพล์ผ่าน ไม่มี error/warning ยังไม่ได้ดูภาพหรือทดสอบ Play Mode ใน Unity จริง

### เมนูสร้างชุดทดลองเปล่าเดิม

1. รอ Unity คอมไพล์ เลือกเมนู **Space Garage > Create Control Prototype**
2. เปิดซีน `Assets/SpaceGarage/Prototype/ControlDemo/ControlDemo.unity` ที่ถูกสร้าง (ถ้าสร้างซ้ำชื่อโฟลเดอร์จะมีเลขต่อท้าย)
3. PlayerShip ไม่มี Renderer ตามขอบเขตงาน ให้ใส่โมเดลของคุณเป็นลูกของ Prefab ชิ้นส่วน โดยหัวหัน local +Z หรือเปิด Gizmos และเลือกยานใน Scene View ขณะทดสอบเพื่อเห็น collider และรัศมีเก็บของ
4. กด Play และคลิก Game View ให้มีโฟกัส ใช้ปุ่มด้านบน การกด E ใกล้ลูกกลมที่ (2,0,2) เพิ่ม Scrap 1 ใน Items บน ShipInventory
5. แก้ Jet.asset เพื่อเปรียบเทียบอัตราสิ้นเปลือง/ความเร็วบูสต์ ปรับสเตตัสแล้วเริ่ม Play ใหม่ หรือเรียก RebuildStats() ระหว่างเล่น

ชุดทดลองประกอบด้วย PlayerShip.prefab, Bullet.prefab, ScrapPickup.prefab, Prefab ชิ้นส่วน 4 อัน และข้อมูลชิ้นส่วน 4 asset ที่ผูกไว้แล้ว ส่วน Armor/Wing/Jet เป็น GameObject เปล่าให้ใส่กราฟิกเอง ค่าเพิ่มสเตตัสเริ่มต้นของทุกส่วนเป็น 0 เพื่อคงค่าพื้นฐานยานไว้ก่อน

## ตั้งค่าเองและช่อง Inspector

### HUD ต่อสู้ตัวอย่างด้วย UI Toolkit (2026-10-26)

ผู้ใช้ยืนยันล่าสุดว่า **UI จริงใช้ Canvas** จึงใช้ **ShipHUD** บน **MainUI** ใน SampleScene ผูก ShipController ให้แล้ว เพิ่ม Auto Find Player ค้น Tag Player อัตโนมัติเมื่อไม่ใส่ Ship ผู้ใช้ยังต้องลาก TMP Text/Slider/Image ที่ออกแบบเองลงช่อง ไม่ได้สร้างหรือย้ายหน้าตา Canvas เดิม Toolkit ยังคงเป็นตัวอย่างปิดได้เท่านั้น

แก้ Toolkit ไม่ขึ้นโดยใช้ UIDocument.visualTreeAsset แทนการแทรก CloneTree เอง กำหนด root เต็มจอและ rebind เมื่อ root ถูกสร้างใหม่ ตรวจ Play Mode จริงพร้อมดูภาพ ScreenCapture แล้ว HUD ขึ้นครบและ root 1920×1080 รายละเอียดผลตรวจใน SpaceGarage/UI/README.md

ตามคำขอให้ทำหน้าตาตัวอย่าง เพิ่ม `CombatHUD_Example` ใน SampleScene แยกจาก UI เดิม แสดงเลือด บูสต์ ความเร็ว Dodge วัสดุมาร์กพร้อมรูป และเป้าเมาส์ ปิดที่ GameObject/component หรือช่อง Visible ได้ และแทน UXML จริงผ่าน Layout โดยไม่แตะระบบเกม รายละเอียดไฟล์ ช่อง Inspector สัญญาชื่อ element และข้อจำกัดอยู่ใน **SpaceGarage/UI/README.md**

ไม่ต้องต่อ reference เพิ่มในซีนตัวอย่าง กด Play เพื่อดู HUD ส่วนระบบมาร์กต้องส่งรายการให้ ShipCombatHUDToolkit.SetTrackedMaterials เมื่อมีข้อมูลจริง แยก ShipMaterialRequirement เป็นไฟล์กลางโดยไม่เปลี่ยน API ของ ShipHUD เดิม

### HIT, HIT Monster และเลข DMG (2026-10-26)

เพิ่ม GameObject **CombatFeedback** ใน `Scenes/SampleScene.unity` พร้อม **ShipCombatFeedback** และผูก Prefab ที่ผู้ใช้เตรียมไว้แล้ว:

| ช่องบน ShipCombatFeedback | Prefab / หน้าที่ |
| --- | --- |
| Hit Prefab | `VFX/HIT.prefab` เมื่อผู้เล่นยิงศัตรูและเลือดลดจริง |
| Hit Monster Prefab | `VFX/HIT Monster.prefab` เมื่อศัตรูยิงผู้เล่นและเลือดลดจริง |
| Damage Number Prefab | `VFX/DMG.prefab` ตัวเลขเมื่อผู้เล่นยิงศัตรูเท่านั้น |
| Hit Lifetime | 2 วินาที ก่อนทำลาย HIT ที่เสก ปรับตามความยาว VFX จริงได้ |

ไม่ต้องลาก reference เพิ่มในซีนตัวอย่าง เลือก CombatFeedback เพื่อเปลี่ยน Prefab ได้ ถ้าใช้ซีนอื่นต้องมี component นี้หนึ่งตัวและผูกทั้งสามช่องเอง ไม่สร้าง VFX หรือ UI สำรองเมื่อเว้นช่อง

- กระสุน ShipProjectile เก็บฝ่ายผู้ยิงจาก owner ตั้งแต่ Initialize จึงยังแยก HIT ได้แม้เจ้าของกระสุนถูกทำลายก่อนกระสุนถึงเป้า
- ใช้ตำแหน่งชนจริงและ normal จาก sphere sweep หรือ ClosestPoint กรณีเริ่มซ้อน collider เพื่อเสก HIT แยกจากตัวกระสุน ไม่ผูกเป็นลูกศัตรู จึงยังเห็นนัดที่ฆ่าศัตรูหลังยานถูกทำลาย
- ShipController และ Enemy รองรับ **IShipHitReceiver.TakeHit(amount, source, point, normal)** เลือก HIT ตามฝ่ายผู้ยิง/เป้าหมาย ตัวเลขคือเลือดที่หายจริงหลัง Def จำกัดไม่เกินเลือดก่อนโดน ไม่แสดงเลขดาเมจที่ผู้เล่นได้รับ
- ระหว่าง Dodge i-frame หรือดาเมจหลังเกราะเป็นศูนย์ จะไม่เสก HIT และไม่แสดงเลข ความเสียหายที่ไม่รู้ฝ่ายผู้ยิงหรือศัตรูยิงศัตรูไม่ใช้เอฟเฟกต์สองฝั่งนี้
- `TakeDamage(float)` เดิมยังใช้ได้กับระบบอื่น แต่ไม่มีฝ่ายผู้ยิง จึงไม่เรียก HIT/DMG กระสุน custom ที่ต้องการเอฟเฟกต์ชุดนี้ให้เก็บ `ShipCombatFeedback.SourceOf(owner)` ตั้งแต่เริ่ม แล้วเรียก TakeHit แทน TakeDamage พร้อมข้อมูลจุดชน (มี fallback TakeDamage สำหรับเป้าหมายที่ไม่รองรับ interface ได้)
- DMG Prefab เดิมมี component `DamagePopup` แต่สคริปต์หาย จึงเพิ่ม `DamagePopup.cs` และใช้ GUID เดิม `22c09ef2ba100a3499915918f3177915` ให้ reference เดิมกลับมาใช้ได้ ไม่เปลี่ยนฟอนต์ สี หรือขนาดที่จัดไว้ ไม่สร้าง Canvas ใหม่
- DamagePopup ใส่เลขลง TMP_Text อัตโนมัติ (มีช่อง Damage Text ถ้าจะลากเอง) หันเข้ากล้องจริง รักษา color/fontSize/scale ของ Prefab ใช้ค่า Launch Speed 4, Gravity 10, Side Speed 0.3–0.9, Lifetime 1, Fade Delay 0.7 และค่าป๊อปที่มีใน Prefab ผู้ใช้ปรับบน DMG ได้เอง Script ไม่เพิ่มระบบ critical หรือเปลี่ยนสีตามชนิดดาเมจในงานนี้
- ตรวจคอมไพล์ Runtime/Editor ทั้งหมดผ่าน ไม่มี error/warning และตรวจ 82 จุดของ Prefab/script/fileID ในซีนผ่าน ยังไม่ได้ดู HIT หรือเลขใน Play Mode จริง

### EnemySpawnManager และ Enemy (2026-10-26)

เพิ่ม `SpaceGarage/Examples/Enemies/EnemyShip.prefab` และ `EnemySpawnManager.prefab` พร้อมวาง Manager แบบ Prefab Instance ใน `Scenes/SampleScene.unity` แล้ว เปลี่ยน Tag ของ StarterShip.prefab root เป็น **Player** โดยไม่แก้ ProjectSettings (Player เป็น tag มาตรฐาน) ไม่เพิ่ม Enemy tag

**เลือก EnemySpawnManager ใน Hierarchy** เพื่อตั้งค่า:

| ช่อง | ข้อมูลที่รับ / ค่าในตัวอย่าง |
| --- | --- |
| Enemy Prefab | Prefab root ที่มี Enemy และ Rigidbody/collider ไม่ใช้ ShipController ของผู้เล่น ผูก EnemyShip ไว้แล้ว |
| Gun Pool | รายการ ShipPartStats แบบ Slot Gun |
| Armor Plate Pool | รายการ ShipPartStats แบบ Slot ArmorPlate |
| Wing Pool | รายการ ShipPartStats แบบ Slot Wing |
| Jet Pool | รายการ ShipPartStats แบบ Slot Jet |
| Minimum / Maximum Spawn Distance | ระยะรอบผู้เล่น 40–60 หน่วย |
| Spawn Interval | พยายามสร้างครั้งละหนึ่งตัวทุก 5 วินาที |
| Max Alive | ศัตรูจาก Manager นี้พร้อมกันสูงสุด 5 ตัว |
| Spawn Immediately | เปิดไว้ เกิดตัวแรกเมื่อเริ่มและผู้เล่นพร้อม |
| Spawn Clearance / Blocking Layers / Attempts | ตรวจจุดเกิดไม่ทับ collider ในรัศมี 3 หน่วย ไม่สน trigger ลองสุ่มได้ 12 ครั้ง |

พูลเป็นรายการตัวเลือกสำหรับสุ่ม ไม่ใช่ระบบ reuse GameObject ศัตรู แต่ละส่วนเลือกอิสระหนึ่งอันจากรายการที่ Slot ตรงและมี Part Prefab ค่า null/ผิด Slot/Prefab ที่มี Enemy หรือ ShipController ซ้อนจะไม่ถูกเลือก ถ้าพูลใดไม่มีตัวเลือกถูกต้อง Manager จะข้ามและแจ้งเตือน ไม่สร้างศัตรูไม่ครบส่วน ตัวอย่างมี asset เดิมอย่างละ 1 ตัว ให้เพิ่มหลาย asset ต่อพูลเองเพื่อให้ได้ชุดหลากหลาย

ตำแหน่งเกิดสุ่มบนทรงกลม 3D รอบตำแหน่งผู้เล่นปัจจุบัน รวมสูง/ต่ำ ไม่ใช่เฉพาะพื้น XZ ถ้าไม่มีผู้เล่น Tag Player หรือผู้เล่นตายจะไม่เกิดเพิ่ม ถ้าจุดเกิดถูกบังทั้งหมดจะข้ามรอบนั้น `SpawnOne()` เรียกจากระบบอื่นได้ และ `AliveCount` อ่านจำนวนศัตรูของ Manager นี้ได้ ไม่สร้าง UI

**EnemyShip / Enemy**:

- Root มี Enemy, Rigidbody, BoxCollider และลูก GunSpawnPoint / ArmorPlateSpawnPoint / WingSpawnPoint / JetSpawnPoint ผูกกับ Gun Mount / Armor Plate Mount / Wing Mount / Jet Mount ครบ Root ไม่มีโมเดลสำเร็จใน Edit Mode; ชิ้นส่วนถูกสร้างจริงตอนเล่นตามผลสุ่ม ใช้ Part Prefab ของผู้เล่นตรง ๆ รวมสเตตัส = Base Stats + ส่วนทั้ง 4 ไม่แก้ Material/กราฟิกของชิ้นส่วน
- ค่า Base Stats ตัวอย่าง: Attack 5 / Def 0 / Speed 10 / Handling 60 / Health 40 เพิ่ม Speed จาก 6 เป็น 10 ตามผู้ใช้ยืนยันเพื่อไล่ความเร็วพื้นฐานผู้เล่น 8 ได้ ค่าสเตตัสและการใช้ไอพ่นปกติอ่านจากส่วนเหมือนผู้เล่น แต่ไม่มีอินพุตผู้เล่น ไม่มีบูสต์ ไม่มี Dodge และไม่ใช้ค่าบูสต์ของ Jet
- อยู่ไกลบินไล่และหันเล็งผู้เล่น Tag Player โดยตรง การยิงไม่อิงการอยู่ในจอหรือหลังกล้อง เมื่ออยู่ใน **Fire Range = 30** และหัวเข้าเป้า **Fire Angle = 15°** ก็เริ่มเตือนและยิงตามปกติได้แม้กำลังย้ายเข้าจอ
- เมื่ออยู่ใกล้แต่หลังกล้อง/นอกจอ ใช้ **IsEnteringView** ขยับเข้าจุดในกรอบภาพพร้อมหันเล็งผู้เล่นไปด้วย แยกทิศเคลื่อนที่ออกจากทิศเล็ง จึงไม่ต้องหยุดยิงหรือรอให้เห็นในจอก่อน ไม่วาร์ป ไม่เพิ่มการบินวนหรือ Dodge
- ศัตรูทุกตัวเลือกบินถอยหลังยิงได้เมื่ออยู่ด้านหน้าทิศบินผู้เล่นและอยู่ในระยะยิง ไม่สุ่มแบ่งชนิด 50/50 ตัวเกิดด้านหน้าจากการสุ่มทรงกลมเดิมจะใช้พฤติกรรมนี้เองโดยไม่เปลี่ยน SpawnManager
- ช่องใหม่กลุ่ม **Front-facing Retreat Fire** บน Enemy: **Front Keep Distance = 20** หน่วย ระยะที่พยายามรักษา และ **Front Distance Response = 5** หน่วย ช่วงค่อยเปลี่ยนความเร็วเดินหน้า/ถอยหลัง หากไกลกว่าระยะจะเข้าหา หากใกล้กว่าจะบินถอยโดยหัวและปืนยังเล็งผู้เล่น ความเร็วไม่เกิน Speed ปกติ ไม่ใช่บูสต์ ระยะรักษาถูกจำกัดไม่ให้เกิน 80% ของ Fire Range
- ศัตรูด้านหลังยังบินไล่ยิง ความเร็วในระยะยิงค่อยลดตามระยะ โดย **Close Range Speed Multiplier = 0.25** เป็นค่าต่ำสุดใกล้ตัวผู้เล่น ไม่ลดเหลือ 25% ทันทีที่เข้า Fire Range จนไล่ไม่ทัน เปลี่ยนทิศและความเร็วอย่างนุ่มด้วย **Speed Response Time = 0.6**
- ช่องใหม่กลุ่ม **Enter The Player's View When Nearby** บน Enemy: **Player Camera** กล้องจริงของผู้เล่น (เว้นว่างใช้ Camera.main); **Screen Entry Range** ระยะเปิดโหมดเข้าจอ 45 หน่วย; **Screen Entry Forward Distance** จุดเข้าจออยู่เลยความลึกผู้เล่นไปด้านหน้าอีก 15 หน่วย; **Screen Edge Margin** ขอบกระตุ้น 0.08; **Screen Return Margin** ต้องเข้าภาพลึกถึงขอบ 0.2 ก่อนออกโหมด เพื่อไม่สลับโหมดไปมาที่ขอบภาพ
- ผูกค่าบน EnemyShip.prefab แล้ว ไม่ต้องลากกล้องให้ Prefab ศัตรู ซ้าย/ขวาที่จะเข้าภาพเลือกจากฝั่งที่ศัตรูอยู่ เป้าหมายเข้าจอขยับตามกล้องผู้เล่น และตรวจกรอบด้วย viewport ไม่ใช่ Renderer.isVisible ซึ่งอาจนับ Scene View ของ Editor เป็นการเห็น
- ตรวจคอมไพล์หลังเพิ่มโหมดเข้าจอผ่าน ไม่มี error/warning ยังไม่ได้ทดลองไล่เข้าจอหรือดูภาพใน Play Mode จริง
- อัปเดตยิงนอกจอ/บินถอยหลัง: ตรวจคอมไพล์ผ่าน และทดสอบ EnemyCombatMotion ตัวจริง 11 กรณี ครอบคลุมเข้าหา หยุดรักษาระยะ ถอยห่าง ความเร็วไม่เกินปกติ และเปลี่ยนต่อเนื่อง ยังไม่ได้ดูภาพการบิน/ยิงจริงใน Play Mode ตั้งค่าเพิ่มใน EnemyShip.prefab แล้ว ไม่ต้องต่อ reference เพิ่ม
- ยิงเมื่ออยู่ใน Fire Range และหัวหันเข้าเป้าใน **Fire Angle = 15°** มี **Attack Warning Time = 0.35** วินาทีก่อนชุดแรก จากนั้น **Fire Burst Duration = 1** วินาที สลับ **Fire Pause Duration = 1.5** วินาทีตามผู้ใช้ยืนยันล่าสุด (ไม่ใช่ค่าร่าง 4/3) ตั้งบน EnemyShip.prefab แล้ว ไม่เปลี่ยน Shots Per Second ของปืนผู้เล่น
- ระหว่างพักปืนศัตรูยังบินไล่/เข้าจอ/ถอยหลังตามปกติ เส้นเตือนชุดถัดไปแสดงช่วงท้ายของเวลาพัก จึงไม่บวก 0.35 เพิ่มทุกครั้งเมื่อเป้าหมายยังเข้าเงื่อนไขตลอด ถ้าโดนตีหรือหลุดเป้าระหว่างชุดจะหยุดชุดและพักก่อนยิงใหม่ ยิงนอกจอได้เหมือนเดิม ไม่มีการเล็งเมาส์หรือยิงล่วงหน้าดักทาง
- เพิ่มลูก **DamageHitbox** บน EnemyShip.prefab มี BoxCollider แบบ Is Trigger ขนาด **(4.2, 1.8, 4.5)** จุดกลาง `(0,0,0.2)` และ ShipDamageHitbox เพื่อให้เล็ง/ยิงโดนง่ายขึ้น Collider ชนปกติบน root ยังขนาดเดิม `(2.8,0.8,3.25)` ไม่ขยายการชนกำแพงหรือผู้เล่น
- ปรับขนาดรับกระสุนที่ **EnemyShip.prefab > DamageHitbox > BoxCollider > Size** หากใช้ Prefab ศัตรูอื่น ให้สร้างลูกพร้อม BoxCollider Is Trigger และ ShipDamageHitbox โดยวางใต้ root ที่มี Enemy ปืนและ ShipProjectile รับ Trigger ที่มี marker นี้เท่านั้น ยังข้าม Trigger ของเก็บ/โซนอื่น ดาเมจหนึ่งนัดถูกเรียกครั้งเดียวแม้มีทั้ง Hitbox และ collider หลัก
- ทดสอบ EnemyFireCycle ตัวจริงผ่าน 17 กรณี รวมยิงครบช่วง พักครบ 1.5 วินาที เตือนในเวลาพัก ขัดจังหวะและกลับมายิง ตรวจคอมไพล์ Runtime/Editor และ reference Hitbox ผ่าน ยังไม่ได้ทดสอบภาพการยิง/Hitbox ใน Play Mode จริง ไม่ต้องลากช่องเพิ่มในซีนตัวอย่าง
- ShipGun เพิ่ม API **SetAITarget(bool pressed, float attack, Transform owner, Transform target)** ส่วน SetTrigger เดิมของผู้เล่นยังยิงตามเมาส์
- EnemyAttackTelegraph ถูกเตรียมโดย Enemy ถ้ายังไม่มี ใช้ LineRenderer เส้นแดงจากปืนไปเป้าหมายก่อน/ระหว่างยิง ปิดเมื่อไม่ยิงหรือโดนตี ปรับ Width / Warning Color หรือใส่ Warning Line เองบน component นี้ได้ ถ้าเพิ่ม component ไว้ใน Prefab เองจะไม่สร้างใหม่ Material ของเส้นสร้างในหน่วยความจำและทำลายพร้อมศัตรู ไม่สร้างไฟล์ .mat
- **Hit Reaction Duration = 0.15** และ **Hit Knockback Speed = 3**: เมื่อโดนดาเมจที่ผ่าน Def จะหยุด AI/ปืนชั่วคราว ถอยจากผู้เล่นและแฟลชขาวด้วย MaterialPropertyBlock ก่อนคืนค่าของ Renderer เดิม ไม่เปลี่ยนไฟล์ Material
- ช่อง **Animator** เป็นทางเลือก ใช้ Trigger `Attack` เฉพาะเมื่อมี parameter นี้อยู่แล้ว ไม่สร้าง controller/คลิปแอนิเมชันใหม่
- ใช้ `IShipDamageable.TakeDamage` รับกระสุน สูตร damage = max(0, incoming - Def) เลือดหมดหยุดยิง/ไอพ่น เรียก Died แล้วปิดและทำลายยาน ไม่มีระบบดรอปวัสดุในงานนี้

ในซีนตัวอย่างไม่ต้องต่อ reference เพิ่มเพื่อเริ่มระบบนี้ ผู้ใช้ต้องเพิ่มตัวเลือกในพูลเพื่อให้มีหลายชุดจริง และหากใช้ยานผู้เล่นอื่นต้องตั้ง Tag Player ที่ root ของยานนั้นเอง ชิ้นส่วนปืนที่ใช้สคริปต์กระสุนแบบ custom ต้องรองรับ IShipProjectile หากต้องการรับค่าดาเมจ/เจ้าของจากปืน

ตรวจคอมไพล์ Runtime/Editor ทั้งหมดกับ Unity/Input System/TMP/UGUI/Cinemachine DLL ผ่าน ไม่มี error/warning ตรวจ 120 รายการของ reference ตัวอย่าง (สคริปต์ พูล Slot ชิ้นส่วน Prefab Tag และ fileID ในซีน) ผ่าน ยังไม่ได้ทดสอบการเกิด บิน ยิง รับดาเมจ หรือภาพจริงใน Play Mode

### HUD และวัสดุที่มาร์ก (2026-10-26)

เพิ่มสคริปต์อย่างเดียว ไม่สร้าง Canvas/Panel/Layout หรือจัดหน้าตา UI ใหม่ ผู้ใช้ต้องมี UI แล้วลากช่องดังนี้:

1. ติด **ShipHUD** บน GameObject UI ของผู้ใช้ ลาก PlayerShip ลง **Ship**
2. **Health Text / Boost Text / Speed Text** รับ TMP_Text แสดงเลือดปัจจุบัน/สูงสุด เกจปัจจุบัน/สูงสุด และความเร็วที่ผ่านการหน่วงแล้ว หน่วย Unity ต่อวินาที ช่องที่ไม่ใช้ปล่อยว่างได้
3. **Health Slider / Boost Slider** รับ Slider ของผู้ใช้ หรือใช้ **Health Fill / Boost Fill** รับ Image ที่ตั้ง Type = Filled ไว้เอง รองรับใส่ทั้งสองหรือเลือกอย่างใดอย่างหนึ่ง ไม่เปลี่ยนรูปแบบ/ช่วง min-max ของ Slider ไม่ยิง OnValueChanged ระหว่างอัปเดต
4. สร้างหน้าตาแถววัสดุหนึ่งแถวแล้วติด **ShipTrackedMaterialRow** ลาก **Icon** (Image), **Material Name** (TMP_Text), **Amount Text** (TMP_Text) ของแถวนั้นให้ครบ จากนั้นทำเป็น Prefab แล้วลากลง **Material Row Prefab** บน ShipHUD และลาก RectTransform พื้นที่รายการบน Canvas ลง **Material Container** ระบบจะเสกแถวจากรายการที่มาร์กเข้า Container ตามผู้ใช้ขอ
5. ผู้ใช้จัด LayoutGroup เช่น VerticalLayoutGroup/GridLayoutGroup และขนาดแถวเอง ระบบไม่เพิ่มหรือเปลี่ยน layout ให้อัตโนมัติ ถ้าไม่ใส่ Material Container ยังใช้ **Material Rows** แบบลากแถวสำเร็จทีละอันได้เหมือนเดิม

สร้างข้อมูลวัสดุด้วย **Create > Space Garage > Material Data**:

- **Item Id**: รหัสคลังที่ไม่ซ้ำ เช่น `Scrap` ต้องตรงกันทั้งระบบดรอป คลัง และมาร์ก (ตัวเล็ก/ใหญ่มีผล)
- **Display Name**: ชื่อที่แสดงให้ผู้เล่น ถ้าว่างใช้ Item Id
- **Icon**: Sprite รูปวัสดุที่ผู้ใช้เตรียมไว้
- ShipPickup เพิ่มช่อง **Material** ให้ลาก asset นี้ได้ ถ้าใส่จะใช้รหัสจาก asset; ถ้าไม่ใส่ยังใช้ Item Id เดิม จึงไม่กระทบของดรอปเก่า

HUD ไม่เลือกวัสดุเอง และไม่แสดงทั้งคลัง ระบบมาร์กภายนอกเรียก:

```csharp
hud.SetTrackedMaterials(new[]
{
    new ShipMaterialRequirement { material = scrapMaterialAsset, requiredAmount = 10 },
    new ShipMaterialRequirement { material = circuitMaterialAsset, requiredAmount = 5 }
});
```

แต่ละแถวแสดงรูป ชื่อ และ `จำนวนใน ShipInventory/requiredAmount` อัปเดตเมื่อเก็บของ ระบบเก็บข้อมูลมาร์กเป็นสำเนาของรายการที่รับ ไม่สร้างระบบมาร์ก/เซฟมาร์กในงานนี้ เรียก `SetTrackedMaterials(null)` เพื่อล้างรายการ; `SetShip(newShip)` เพื่อเปลี่ยนยานที่ HUD ติดตาม

เมื่อใส่ **Material Container** ระบบใช้โหมดสร้างแถวตามรายการมาร์ก ไม่ใช้ array Material Rows โดยต้องมี **Material Row Prefab** ที่เป็นแถวแยก ไม่ใช่ Container หรือ Canvas ทั้งอัน สร้างแถวเท่าที่ต้องใช้แล้วเก็บใช้ซ้ำ หากมาร์กลดลงจะ SetActive(false) เฉพาะแถวที่ระบบเสก เมื่อเพิ่มมาร์กจะนำกลับมาใช้ หากล้างรายการจะปิดทุกแถวที่ระบบเสก ไม่แตะลูกอื่นของ Container เช่นหัวข้อ/ของตกแต่ง เมื่อปิด ShipHUD หรือเปลี่ยน Container/Prefab จะลบเฉพาะ instance ที่ระบบเป็นเจ้าของและสร้างใหม่เมื่อใช้งาน

ในโหมด **Material Rows** เดิม (ไม่ใส่ Container) แถวที่ไม่ใช้ยังล้างข้อความและปิดเฉพาะ Image ของรูปที่ไม่มีข้อมูล ไม่ปิด GameObject แถวหรือเปลี่ยน alpha หากจำนวนรายการเกินช่องที่เตรียมจะเตือนเหมือนเดิม

ตรวจคอมไพล์ Runtime/Editor หลังเพิ่ม Container ผ่าน ไม่มี error/warning ยังไม่ได้ทดลองกับ Row Prefab/Container ของจริงใน Play Mode ผู้ใช้ต้องลากสองช่องใหม่และเตรียม layout ของตัวเอง ไม่มีการสร้างกราฟิกแถวสำรอง

### เอฟเฟกต์บูสต์ในแต่ละชิ้นส่วน (2026-10-26)

เพิ่มช่อง **Normal Effect Root** บน ShipBoostEffect สำหรับไอพ่นปกติ ให้ลาก GameObject ลูกที่มี VFX Graph ปกติลงช่องนี้ ส่วนช่อง Effect Root / Effect Prefab เดิมใช้สำหรับไอพ่นบูสต์ ไม่เปลี่ยน reference VFX ที่ผู้ใช้ใส่ไว้แล้ว

| สถานะ | Normal Effect Root | เอฟเฟกต์บูสต์ |
| --- | --- | --- |
| บินปกติ / ปล่อย Shift / เกจหมด | เปิด | ปิด |
| กำลังบูสต์จริง | ปิด | เปิด |
| ยานตาย / ปิด ShipController / ปิดชิ้นส่วน | ปิด | ปิด |

สลับด้วย VisualEffect.Play/Stop และ SetActive จริง ปิดชุดเดิมก่อนเปิดชุดใหม่ ไม่สร้างเฟด ทั้งสอง Root ต้องเป็น GameObject ลูกคนละอัน ห้ามใช้ตัวเดียวกันหรือซ้อนเป็นพ่อ–ลูกกัน ส่วนไอพ่นปกติยังไม่ถูกสร้างให้ ผู้ใช้ต้องใส่ VFX และลากช่อง Normal Effect Root เพิ่ม 1 ช่อง; ช่องเดิมไม่ต้องต่อใหม่ รองรับกรณีใส่เฉพาะไอพ่นปกติหรือเฉพาะบูสต์ด้วย สคริปต์กลางส่ง SetFlightState เพื่อแยกบินปกติจากยานตาย

ใส่ **ShipBoostEffect** บน root ของ Part Prefab ที่ต้องการให้มีเอฟเฟกต์ จะเป็น Jet หรือส่วนอื่นก็ได้ แต่ละ Prefab ใช้ VFX คนละแบบได้ ไม่เก็บ VFX ไว้ใน ShipPartStats และไม่ต้องลากเอฟเฟกต์แต่ละอันบนสคริปต์กลาง

- วิธีฝัง VFX Graph: เพิ่ม GameObject ลูกที่มี VisualEffect ตั้ง Graph Asset ที่ผู้ใช้เตรียม แล้วลาก GameObject ลูกนั้นลง **Effect Root** ของ ShipBoostEffect
- วิธีเสกจาก Prefab: เว้น Effect Root ว่าง ลาก Prefab เอฟเฟกต์ที่มี VisualEffect ลง **Effect Prefab** และใส่ Transform ลูกของส่วนยานลง **Effect Spawn Point** ถ้าไม่ใส่จุด ใช้ตำแหน่ง root ส่วนนั้น
- ถ้าใส่ทั้งสองแบบ ใช้ Effect Root เป็นหลัก ไม่เสกซ้ำ Effect Prefab ถูกเสกครั้งเดียวตอนประกอบชิ้นส่วน จากนั้นเปิด/ปิด object เดิม ไม่ Instantiate ทุกเฟรม
- Effect Root ต้องเป็นลูกแยก ห้ามใส่ root ของชิ้นส่วนหรือตัวยาน เพื่อไม่ให้ระบบปิดยาน/ปืนไปด้วย ปรับตำแหน่งและทิศพ่นใน Prefab เอฟเฟกต์เอง
- ตอนยังไม่บูสต์: Stop VisualEffect และ SetActive(false) ที่ Effect Root; ตอนบูสต์จริงและมีเกจ: SetActive(true) และ Play VisualEffect ภายใน; ปล่อย Shift/เกจหมด/ยานตาย/ปิดสคริปต์จะหยุดและปิด Effect Root ทันที ไม่เพิ่มเฟดหรือปล่อยหางเอฟเฟกต์แทนเอง ใช้ event มาตรฐาน OnPlay/OnStop ของ VisualEffect
- ShipController ค้น ShipBoostEffect ทุกส่วนหลังประกอบหรือเปลี่ยน Loadout ใหม่แล้วสั่งตาม IsBoosting โดยอัตโนมัติ ส่วนที่ไม่ใส่เอฟเฟกต์ไม่สร้างของสำรอง
- ติด component นี้ไว้ใน **Examples/StarterShip/JetPart.prefab** แล้ว พร้อมลูก **BoostEffectPoint** ที่ `(0,0,-0.45)` ผู้ใช้ยังต้องใส่ Effect Prefab ของตนเอง หรือฝัง Graph แล้วลาก Effect Root ไม่มีการสร้าง/แก้ Graph ให้ในงานนี้

ตรวจคอมไพล์กับ Unity/TextMeshPro/UGUI/VFX DLL จริงผ่าน ไม่มี error/warning ตรวจ reference ของ JetPart แล้ว ยังไม่ได้ทดสอบ HUD หรือภาพ VFX ใน Play Mode จริง เพราะยังไม่มี UI และ Graph ของผู้ใช้ให้ผูก

### Shift บูสต์ความเร็ว (อัปเดต 2026-10-26)

ShipController อ่าน Shift ซ้ายหรือขวาแบบกดค้าง และใช้ผลบูสต์กับความเร็วเคลื่อนที่จริง ไม่ใช่เปลี่ยนเฉพาะกล้อง ยานตัวอย่างผูก Jet.asset ไว้แล้ว: ความเร็วปกติ 8 หน่วย/วินาที บูสต์เป้าหมาย 16 หน่วย/วินาที (2 เท่า) ค่อยเร่ง/ผ่อนด้วย Speed Response Time 0.6 วินาทีตามการบินมีน้ำหนัก

เกจเต็ม 100 ใช้ 25 ต่อวินาที บูสต์ต่อเนื่องได้ประมาณ 4 วินาที ปล่อย Shift หน่วง 1 วินาทีแล้วฟื้น 15 ต่อวินาที เกจหมดต้องปล่อยปุ่มก่อนเริ่มรอบใหม่ ปรับค่าบูสต์ใน Jet.asset ได้ ไม่เพิ่มช่องสคริปต์ ไม่ต้องต่อ reference เพิ่ม ตรวจเส้นทาง Input → เกจ → speedTarget → SmoothDamp → Rigidbody.MovePosition และค่าที่ผูกบน Prefab แล้ว ยังไม่ได้ทดสอบกดปุ่มใน Play Mode จริง

### Space + A/D หมุนหลบและ i-frame (2026-10-26)

- เก็บบูสต์ Shift ควบคู่กับท่าหลบตามผู้ใช้ยืนยันล่าสุด HUD บูสต์และ ShipBoostEffect เดิมยังใช้บูสต์ ไม่เปลี่ยนเป็นเอฟเฟกต์ท่าหลบ
- บน **ShipController** มี **Dodge Duration** (เวลาหมุนทั้งท่า 1.2 วินาที) / **Dodge Cooldown** (4 วินาทีหลังจบท่า) / **Dodge Distance** (5 หน่วย) และเพิ่ม **Dodge Dash Duration** (เวลาพุ่งด้านข้าง 0.6 วินาที) ตั้งใน StarterShip.prefab แล้ว ไม่ต้องต่อจุดหมุนใหม่ ถ้าตั้งเวลาพุ่งเกินเวลาท่า ระบบจำกัดให้จบพร้อมท่า
- Space ใช้ wasPressedThisFrame ไม่กดค้างต่อท่าอัตโนมัติ ต้องมี A หรือ D เพียงทิศเดียว ไม่เข้าคิวท่าระหว่างติดคูลดาวน์
- ท่าหลบหมุนรอบ local +Z 360 องศา คง yaw/pitch และมุมเอียงปกติระหว่างท่า ยานบินต่อไปตามหัว พร้อมพุ่งซ้าย/ขวาแนวราบตาม A/D ระยะ 5 หน่วย ทิศพุ่งอิง yaw อย่างเดียว ไม่อิง pitch/roll จึงไม่มีส่วนพุ่งขึ้น/ลงจาก Dodge (การบินหน้าต่อยังอาจไต่/ลดระดับตามหัวเดิม) พุ่งเร็วช่วงแรกแล้วค่อยชะลอให้จบใน 0.6 วินาที ส่วนหมุนใช้ 1.2 วินาที ไม่วาร์ป ไม่คืนตำแหน่งหลังจบท่า สามารถใช้ Shift บูสต์ที่แยกกันพร้อมท่าได้
- ช่วงหมุนทั้งหมด **IsInvulnerable** เป็น true และ **TakeDamage** จะไม่ลดเลือด ไม่ได้ปิด collider จึงยังชนกำแพงตามฟิสิกส์ คูลดาวน์นับหลังท่าจบ ไม่มีอมตะต่อในคูลดาวน์ ยกเลิกท่าเมื่อปิด controller แล้วไม่ล้างคูลดาวน์
- กล้องใน SampleScene ใช้ **Lock To Target No Roll** อยู่แล้ว การหมุนรอบแกนหน้าไม่เปลี่ยน forward ที่กล้องติดตาม จึงไม่เพิ่มการหมุนตามลำให้กล้อง ไม่เปลี่ยน Tracking Target หรือ World Up ของ Brain
- เพิ่ม **ShipDodgeCameraLag** บน **CinemachineCamera** ใน SampleScene และผูก **Ship** กับ PlayerShip แล้ว ระหว่างหลบเพิ่ม **Dodge Position Damping** = 0.9 และ **Dodge Aim Damping** = 0.6 เพื่อให้ยานพุ่งนำและกล้องเล็ง/เคลื่อนตามทีหลัง ปรับเข้าสู่ค่าหลบด้วย **Lag Blend In Time** = 0.08 วินาที และค่อยคืนค่าปกติด้วย **Lag Recovery Time** = 0.8 วินาที ไม่เปลี่ยน offset หรือ binding mode
- สคริปต์เก็บค่า Damping ปกติเมื่อเริ่มเล่นและคืนค่าเดิมเมื่อปิด component ไม่แก้ค่าของกล้องอื่น ผู้ใช้ไม่ต้องลาก reference เพิ่มในซีนตัวอย่าง ถ้าย้ายไปซีนใหม่ให้ติด component นี้บน CinemachineCamera ที่มี CinemachineFollow/RotationComposer และลาก ShipController ลง Ship
- API: **TryDodge(-1)** ซ้าย / **TryDodge(1)** ขวา คืน false ถ้าใช้ไม่ได้; **IsRolling**, **IsInvulnerable**, **CanDodge**, **DodgeCooldownRemaining**, **DodgeReadyProgress** ใช้เชื่อมระบบอื่น
- บน **ShipHUD** เพิ่มช่องแยก **Dodge Cooldown Text** (TMP_Text), **Dodge Ready Slider** (Slider), **Dodge Ready Fill** (Image แบบ Filled) ให้ผู้ใช้ลาก UI ที่เตรียมเอง แสดงเวลาคงเหลือเป็นวินาที: ระหว่างหมุนแสดง 4.0, หลังจบท่านับ 4 → 0, พร้อมใช้แสดง 0.0; แถบเติมจากว่างไปเต็มตามความพร้อม เว้นว่างได้ ไม่สร้างหรือเปลี่ยนหน้าตา UI เอง
- ทดสอบ ShipDodgeState ตัวจริงผ่าน 17 กรณี: ทิศที่ใช้ได้/ไม่ได้ เวลาเริ่ม/จบอมตะ กันท่าซ้อน คูลดาวน์เริ่มหลังท่าจบ ใช้ซ้ำตรงขอบเวลา ยกเลิกแล้วไม่ค้างอมตะและไม่ล้างคูลดาวน์ ยังไม่ได้ดูภาพหมุน/กล้องหรือทดลองปุ่มจริงใน Play Mode
- หลังเพิ่มการพุ่งด้านข้าง ทดสอบตรรกะตัวจริงผ่าน 27 กรณี รวมพุ่งนำช่วงแรก ไม่ย้อนกลับ ไม่เกินระยะ และระยะรวม 5 หน่วยตรงกันที่ 15/30/50/120 Hz ตรวจคอมไพล์พร้อม Cinemachine DLL และตรวจ component/target ของกล้องในซีนแล้ว ยังไม่ได้ดูภาพจริงใน Play Mode
- อัปเดตหมุนช้าลง: ทดสอบเวลาพุ่ง 0.6 แยกจากเวลาหมุน 1.2 ผ่าน 25 กรณี รวมระยะคงที่หลายความถี่ ไม่พุ่งต่อหลังครบระยะ อมตะจนหมุนจบ และคูลดาวน์ 4 วินาทีหลังหมุนจบ ตรวจคอมไพล์ผ่าน ไม่มี error/warning ยังไม่ได้ทดสอบภาพจริงใน Play Mode

### ShipPartStats: ข้อมูลชิ้นส่วนแต่ละแบบ

สร้างด้วย **Create > Space Garage > Ship Part Stats** สร้างกี่แบบก็ได้ ไม่แก้ asset สเตตัสร่วมเพื่อเก็บสถานะเฉพาะยาน

- **Slot**: เลือก Gun / ArmorPlate / Wing / Jet ไม่มี Nose Cone
- **Part Prefab**: ลาก Prefab ชิ้นส่วนนั้น ปืนต้องมี ShipGun และจุด Muzzle อยู่ภายใน Prefab
- **Stats**: Attack, Defense, Speed, Handling (องศา/วินาที), Health, Booster Capacity (ขนาดเกจ ไม่ใช่จำนวนชาร์จ)
- ทุกส่วนเพิ่มสเตตัสได้ทุกช่อง ค่ารวม = Base Stats + Stats ของทั้ง 4 ส่วน
- **Jet settings**: Boost Speed Multiplier, Boost Drain Per Second, Boost Recovery Per Second, Boost Recovery Delay ใช้เฉพาะ asset ที่อยู่ในช่อง Jet
- เช่น ยานพื้นฐานมีเกจ 100 และ Jet เพิ่ม Booster Capacity 20 จะได้เกจเต็ม 120 ถ้าต้องการให้เกจยังเป็น 100 ให้ค่าที่เพิ่มเป็น 0

### ShipController: ติดที่ root ยาน

- ต้องมี Rigidbody และ collider แบบ 3D บนยาน สคริปต์ปิด Gravity และปลด Constraints ทั้งหมดใน Awake เพื่อบินขึ้นลง ก้มเงย และเอียงได้จริง ไม่ใช้ Rigidbody2D
- **Base Stats**: Attack 10, Defense 0, Speed 8, Handling 360, Health 100, Booster Capacity 100
- **Gun Part / Armor Plate Part / Wing Part / Jet Part**: ลาก ShipPartStats ให้ตรง Slot ช่องละ 1 asset
- **Gun Mount / Armor Plate Mount / Wing Mount / Jet Mount**: ลาก Transform ลูกของยานที่เป็นจุดติดตั้ง จะวางชิ้นส่วนที่ local position/rotation เป็นศูนย์บน mount และคงสเกลของ Prefab ถ้าเว้นว่างใช้ root ยาน
- จัดตำแหน่ง mount เอง ปีกคู่ให้รวมซ้าย/ขวาใน Wing Prefab เดียว ชิ้นส่วนไม่ควรมี Rigidbody หรือ ShipController ซ้อน ให้ฟิสิกส์อยู่ที่ root ยาน
- **Guns**: ระบบค้นปืนในลูกยานใหม่หลังประกอบ ไม่ต้องลากเอง
- **Inventory**: ลาก ShipInventory หรือปล่อยว่างให้ค้นบน GameObject เดียวกัน
- ลบช่อง **Bank Visual Root** และวิธีสร้าง ShipVisualBank ออกแล้ว การเอียงทำที่ Rigidbody root จริง ส่วน mount ทั้ง 4 อยู่ตาม hierarchy ที่จัดไว้ ไม่ต้องลากช่องทดแทน
- **Bank Angle / Bank Degrees Per Second**: มุมเอียงสูงสุดและความเร็วเอียง/คืนตรง ค่าเริ่มต้น 25 องศาและ 60 องศาต่อวินาที ปรับได้บน ShipController
- ช่องใหม่กลุ่ม **Weighted Flight Response** บน ShipController: **Max Pitch Angle** มุมก้มเงยสูงสุด 30 องศา; **Pitch Degrees Per Second** เพดานความเร็วก้มเงย 30; **Turn Degrees Per Second** เพดานเลี้ยว 45; **Steering Response Time** ความหน่วงอัตราก้มเงย/เลี้ยว 0.4 วินาที; **Speed Response Time** ความหน่วงเพิ่ม/ลดความเร็วและบูสต์ 0.6 วินาที ค่าหน่วงมากยิ่งตอบสนองช้า ผูกค่าบน StarterShip.prefab แล้ว ไม่ต้องลาก reference เพิ่ม
- **Pickup Radius / Pickup Layers**: ระยะเก็บและเลเยอร์ของ collider ของดรอป
- **Health Changed / Boost Changed**: อีเวนต์ส่งค่าปัจจุบันกับค่าสูงสุดให้ UI ที่ผู้ใช้ทำในอนาคต; **Died**: อีเวนต์ตอนเลือดหมด

เมื่อ Awake ระบบประกอบ Prefab 4 ส่วนตามข้อมูลที่ใส่ไว้โดยอัตโนมัติ ส่วนที่ไม่ใส่ asset/Prefab จะไม่ถูกสร้าง และ asset ผิด Slot จะถูกข้ามพร้อมคำเตือน ไม่สร้างกราฟิกสำรอง

### ShipGun: ติดใน Prefab ส่วน Gun

- **Projectile Prefab**: เปลี่ยนเป็น GameObject Prefab ใส่กระสุนที่มีสคริปต์ชนิดใดก็ได้ ไม่บังคับ ShipProjectile โดยช่อง serialized ภายในใหม่ชื่อ projectilePrefabObject ส่วน reference เก่าเก็บเป็นช่องซ่อนเพื่อย้ายค่า/ใช้ย้อนหลัง ไม่ล้างกระสุนที่ผู้ใช้ใส่ไว้
- **Muzzle 1 / Muzzle 2**: Transform ตำแหน่งยิง กระสุนหัน +Z ของตัวเองเข้าจุดใต้เมาส์ ยิงพร้อมกันทั้งสองจุดตามผู้ใช้ยืนยัน ใส่เพียงจุดเดียวก็ยิงจุดเดียว ถ้าไม่ใส่ทั้งสองใช้ Transform ของ ShipGun ถ้าลาก Transform เดียวกันสองช่องจะไม่ยิงซ้ำ ใส่ Muzzle เดิมเป็น Muzzle 1 ให้อยู่แล้ว ผู้ใช้ต้องสร้างและลากจุดที่สองลง Muzzle 2 เองตามตำแหน่งโมเดล
- ช่องใหม่กลุ่ม **Mouse Aim** บน ShipGun: **Aim Camera** รับ Camera ที่เรนเดอร์จริง ไม่ใช่ CinemachineCamera ปล่อยว่างใช้ Camera.main (SampleScene มี MainCamera tag แล้ว ไม่ต้องลากเพิ่ม); **Aim Layers** เลเยอร์ Collider ที่รับเล็ง; **Aim Distance** ระยะ ray/ระยะเป้าหมายในอวกาศว่าง ค่าเริ่มต้น 1000 หน่วย
- หาเป้าหมายจากเมาส์หลังกล้องอัปเดตใน LateUpdate เลือกจุดชนใกล้ที่สุดที่ไม่ใช่ยานเจ้าของหรือ IShipProjectile รองรับ collider ปกติและ Trigger ที่มี ShipDamageHitbox เท่านั้น กระสุนสองปากใช้เป้าหมายเดียวกัน แต่ยังชนสิ่งกีดขวางตามระบบกระสุนของตนเองตามปกติ ไม่หมุนโมเดลปืนหรือยานตามเมาส์ ถ้าไม่มีกล้องหรือเมาส์อยู่นอก viewport กล้องจะไม่ยิง
- **Shots Per Second / Damage Multiplier / Projectile Speed / Projectile Lifetime**: ความถี่ยิง ตัวคูณ Attack ความเร็ว และอายุกระสุน
- Shots Per Second เป็นจำนวนจังหวะยิงต่อวินาที ไม่ใช่จำนวนกระสุนรวม เช่น 8 กับ 2 จุดยิงจะสร้าง 16 นัดต่อวินาที ดาเมจต่อกระสุน = Attack รวมของยาน × Damage Multiplier ไม่มีการใช้ Attack จากชิ้นปืนซ้ำสองรอบ
- Projectile มี Hit Radius และ Hit Layers เป้าหมายใช้ collider ปกติหรือ Trigger ที่มี ShipDamageHitbox และ implement IShipDamageable ถ้าต้องการรับดาเมจ กำแพงที่ไม่รับดาเมจก็หยุดกระสุนได้

#### กระสุนสคริปต์ชนิดอื่น

Prefab ใด ๆ ถูก Instantiate ตามจุดยิงได้เสมอ หากต้องการรับค่าจากปืน ให้สคริปต์กระสุน implement **IShipProjectile** และเมธอด `Initialize(Transform owner, float damage, float speed, float lifetime)` ปืนจะเรียก receiver ทั้งบน root และลูกหลังสร้าง Prefab กระสุนเดิม ShipProjectile รองรับ interface นี้แล้ว

กระสุนที่ไม่ implement interface ยังเสกและใช้ Awake/Start/ค่าของตัวเองได้ แต่จะไม่รับค่าดาเมจ ความเร็ว อายุ หรือเจ้าของจากปืนอัตโนมัติ สคริปต์กระสุนแต่ละชนิดรับผิดชอบวิธีเคลื่อนที่ ชน อายุ การทำลายตัวเอง และไม่โดนเจ้าของเอง ปืนไม่บังคับเพิ่ม ShipProjectile ลง Prefab ที่มีสคริปต์อื่น

อัปเดตเมนูสร้างชุดทดลองให้ใส่ GameObject ลงช่องใหม่แล้ว ตรวจคอมไพล์สคริปต์ทั้งหมดกับ DLL จริงผ่าน ไม่มี error/warning ยังไม่ได้ทดสอบยิงสองปากกระบอกหรือกระสุนชนิดใหม่ใน Play Mode จริง

### ShipPickup / ShipInventory

- ของดรอปใช้ ShipPickup กับ SphereCollider ตั้ง Is Trigger ไว้ กำหนด Item ID เช่น Scrap และ Amount
- วาง collider ให้ศูนย์กลางตรงวัตถุ ของเก็บต้องอยู่ใน Pickup Layers ของยาน
- ShipInventory.Items เก็บรายการในหน่วยความจำ; Count(itemId) อ่านจำนวน และ Add(itemId, amount) เพิ่มจำนวน; Changed แจ้งให้ UI อัปเดต

## เชื่อมหน้าปรับแต่งภายหลัง

เรียก `controller.SetLoadout(gun, armorPlate, wing, jet)` ด้วย ScriptableObject ทั้ง 4 ตัว รับ null ได้ ถ้าประเภทไม่ตรงช่องจะคืน false และคงชุดเดิม

ระหว่างเล่นจะปิด/ถอดชิ้นเก่า ประกอบ Prefab ใหม่ ค้น ShipGun ใหม่ และรวมสเตตัสใหม่ โดยไม่เติมเลือด/เกจฟรี (คงค่าปัจจุบันและจำกัดไม่ให้เกินค่าสูงสุดใหม่) หากตั้งก่อน Awake ระบบจะประกอบตอนเริ่มเล่น

`RebuildStats()` รวมค่าสเตตัสใหม่อย่างเดียว ไม่เปลี่ยน Prefab; `RebuildStats(true)` เติมเลือด/เกจเต็มสำหรับการเริ่มรอบใหม่ที่ระบบเกมอนุญาต

## ข้อจำกัดและผลตรวจ

- แก้รายงานบินวนกลับหัว: เดิมสะสม quaternion ก้มเงยได้ครบวง ทำให้ยานผ่านแนวดิ่งและกล้อง LockToTargetNoRoll เปลี่ยนกรอบอ้างอิงใหม่ เปลี่ยนเป็น yaw/pitch ที่จำกัดมุมก้มเงย ±30 องศาและ bank ±25 องศา จึงไม่ผ่านแนวดิ่ง ลดความเร็วเลี้ยวและเพิ่มความหน่วงตามผู้ใช้ขอ คง Cinemachine เดิมไว้
- การเปิดดู Unity ผ่าน computer-use ทำไม่ได้ในรอบนี้: native pipe เชื่อมต่อไม่สำเร็จ แม้ลองใหม่และ reset แล้ว จึงยังไม่อ้างว่าตรวจภาพหรือทดสอบ Play Mode จริง

- แก้การควบคุมตามผู้ใช้ยืนยันล่าสุดเป็นเครื่องบิน 3D: S เชิด / W กดหัว / A,D เอียงแล้วเลี้ยว บินต่อเนื่อง ปลด Constraints ของ Rigidbody และลบ Bank Visual Root ตัวอย่างเดิมไม่ต้องตั้งค่าเพิ่ม ค่ามุมเอียง 0 ทำให้ไม่เลี้ยวด้วย A/D เพราะไม่มีการเอียง

- ตรวจคอมไพล์สคริปต์ Runtime และ Editor กับ DLL จริงของ Unity 6000.3.8f1 และ Input System ด้วย Roslyn ผ่าน ไม่มี error/warning
- ยังไม่ได้ทดสอบ Play Mode หรือดูภาพจริง จึงยังไม่ยืนยันความรู้สึกควบคุม/การชน/ภาพจากกล้อง
- สูตร Def ต้นแบบ: ความเสียหายที่ได้รับ = max(0, ดาเมจเข้า - Defense); เลือดหมดหยุดคุมและยิง เรียก Died ไม่ลบยานหรือสร้างหน้าแพ้เอง
- มีระบบศัตรูสุ่มส่วน/บินไล่ยิงแล้ว แต่ยังไม่มีสุ่มดรอปวัสดุ เซฟคลัง/โหลดชุดแต่ง หน้าแต่งยาน Barrier และพฤติกรรมเฉพาะปืน 5 แบบ
- ค่าฟื้นเกจ ตัวคูณบูสต์ สูตร Def และระยะเก็บเป็นค่าต้นแบบที่ปรับต่อได้ ไม่ใช่ข้อสรุปบาลานซ์เกม
- ไม่มีการแก้ SampleScene, UI, Input Actions เดิม หรือ Project Settings เมนูสร้างชุดทดลองต้องกดใน Unity เอง
