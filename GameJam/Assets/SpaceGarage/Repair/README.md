# ระบบซ่อมยานลูกค้า

อัปเดต 2026-10-26 — ใช้ Canvas/UGUI ของผู้ใช้ ไม่สร้างหน้าตา UI หรือ UI Toolkit เพิ่ม

ข้อความ UI ที่ระบบสร้างใช้ภาษาอังกฤษทั้งหมดตามผู้ใช้สั่ง ชื่อวัสดุอ่านจาก Display Name ของ ShipMaterialData ให้กำหนดเป็นภาษาอังกฤษด้วย จำนวนวัสดุใน tooltip แสดงเฉพาะเลขที่ยังขาด เมื่อครบแสดง 0 ไม่แสดง Required/Owned/Complete ไม่มีช่อง Inspector ใหม่

## เริ่มตั้งค่า

### อุโมงค์ท่าจอดยานในซีน Repair (2026-10-27)

ปรับลดความสว่างตามผู้ใช้ยืนยันล่าสุด: ลดค่า RGB ของ BaseColor/Color และสีเงาสองระดับใน Material อุโมงค์ทั้ง 6 ไฟล์ลง 20% จากชุดขาวสดใส รวมแถบไฟนำทาง โดยคง hue และ alpha=1 ให้ยังเป็นโทนขาวทึบที่แสงนุ่มลง ไม่เปลี่ยน shader/geometry/ไฟจริงหรือ Post Processing ไม่มีช่องใหม่ ไม่ต้องตั้งค่าเพิ่ม ตรวจค่าทั้ง 6 ไฟล์และ diff ผ่าน ยังไม่ได้ดูภาพจริงใน Unity

ปรับธีมรอบขาวสดใสตามผู้ใช้ขอ: ใช้ Hull ขาวงาช้างเกือบขาวล้วน, Structure ขาวอมฟ้า และ Deck เทาฟ้าอ่อนที่สว่างขึ้น ลดความเข้มเงา Toon ทั้งสองชั้น ปรับ BaseColor Step=0.35 / BaseShade Feather=0.18 ให้เงานุ่ม และปิดการคูณสีแสงใน Base/1st Shade/2nd Shade ของ Material อุโมงค์สามไฟล์เพื่อไม่ให้แสงฉากย้อมสีขาวหม่น ไม่เปลี่ยน Project Settings หรือระบบแสง/เงาของฉาก Cyan เป็นไฟสีฟ้าเกือบขาว Amber เป็นทองอ่อน และ Markings เป็นฟ้าหม่นเพื่อให้ยังอ่านเส้นบนพื้นสว่างได้ แก้ Material เดิมทั้ง 6 ไฟล์ ไม่มี geometry/reference ใหม่ ไม่มีช่อง Inspector ใหม่ ไม่ต้องตั้งค่าเพิ่ม ตรวจข้อมูลสี/ความสว่างของเงาและ diff ผ่าน ยังไม่ได้เรนเดอร์ตรวจภาพจริงใน Unity

ปรับธีมตามผู้ใช้ขอเป็นขาว–เทาอ่อน: แก้ Hull.mat ให้ผนัง/เพดานสีขาว, Deck.mat ให้พื้น/แผงผนังเทาอ่อน และ Structure.mat ให้โครงค้ำขาวอมเทา พร้อมปรับสีเงา Toon ทั้งสองระดับให้เข้าชุด เก็บไฟฟ้า/ส้มเดิมเป็นจุดนำทาง ใช้ Material asset เดิม ไม่เพิ่มไฟล์หรือช่อง Inspector และไม่ต้องตั้งค่าเพิ่ม ตรวจค่าสีและ diff แล้ว ยังไม่ได้ดูภาพสีจริงใน Unity

แก้ `Scenes/Repair.unity` ที่ root `map`: เปิด map และเพิ่ม instance `SpaceDockTunnel` จาก `Hangar/SpaceDockTunnel.prefab` เก็บ Plane/Plane (1) เดิมไว้แต่ปิด GameObject ทั้งคู่ ไม่ลบ object หรือ reference เดิม อุโมงค์ปิดครบ 4 ด้านด้วย Floor/Ceiling/LeftWall/RightWall มี BoxCollider เฉพาะผนังหลัก 4 ชิ้น เปิดปลายตามแนวบินทั้งสองด้าน ไม่มีประตูหรือผนังปิดขวางยาน

อุโมงค์ยาว 120 หน่วย ช่องภายในกว้าง 30 สูง 21 หน่วย แนวยาวแกน X ยึดตำแหน่ง Stay เดิมเป็น pivot ของ prefab instance (-16.15112, 8.97849, 12.94973) ปากเข้า local X=-25 อยู่หลังจุด IN(-30) 5 หน่วย; ปากออก local X=95 อยู่ก่อน Out(105.1) 10.1 หน่วย จึงเห็นยานบินจากภายนอกเข้ามาจอดแล้วออกอุโมงค์ก่อนถูกทำลาย พื้นอยู่ต่ำกว่า Stay 5 หน่วย เพดานสูงกว่า Stay 16 หน่วย ไม่ย้าย Position/IN/Stay/Out/CustomizationPreview หรือกล้องเดิม ไม่แก้เส้นทางบินหรือ UI

เพิ่มโครงค้ำซ้ำทุก 10 หน่วย ผนังแยกช่อง รางไฟสีฟ้า ไฟเตือนสีส้ม เส้นนำทางและกรอบจุดจอดใต้ Stay เป็น geometry ใน prefab รวม MeshRenderer 199 ชิ้น ใช้ Material ร่วม 6 ไฟล์ใน `Hangar` (Hull/Deck/Structure ใช้ Toon/Toon, Cyan/Amber/Markings ใช้ URP Unlit) ทำเครื่องหมาย Batching Static เฉพาะ geometry ไม่มีสคริปต์สร้างแมพระหว่างเล่น ไม่เพิ่มแสงจริง/ไม่เปลี่ยนคุณภาพภาพ ไฟตกแต่งเป็นแถบ Material สีสว่าง ไม่มีช่อง component ใหม่ให้ลากหรือกรอก ปรับรายละเอียดจากลูกใน prefab และ Material ทั้งหกได้ หรือเปิด/ปิดทั้งอุโมงค์ที่ map > SpaceDockTunnel ด้วย SetActive ตามปกติ

ผลตรวจ: ตรวจ fileID ใน scene/prefab ไม่ซ้ำและ reference ภายในครบ, Material ครบ 6 ไฟล์, collider ครบ 4 ด้าน ตรวจช่องทางบินจาก IN ถึง Out จำนวน 53,929 กรณีเทียบ geometry ทั้งหมด โดยเผื่อกรอบยานครึ่งขนาด (X=2,Y=3,Z=4) ไม่ชนอุโมงค์ และตรวจว่า anchor/กล้องเดิมไม่เปลี่ยน ตรวจ diff ผ่าน ลบเครื่องมือสร้าง/ตรวจชั่วคราวแล้ว ยังไม่ได้ตรวจ Unity import/ภาพจริงหรือทดสอบ Play Mode ไม่ได้วัด draw calls/FPS จริง ซีนเดิมไม่มี occlusion data ที่ bake ไว้

ผู้ใช้ไม่ต้องผูกค่าเพิ่ม หาก Unity ยังแสดงซีนเก่าให้ Reload ซีน Repair จากไฟล์ที่แก้ กล้อง orbit ยังใช้ช่วงซูม/มุมเดิม จึงอาจซูมไกลจนออกนอกอุโมงค์ได้ ยังไม่ได้เพิ่มระบบกันกล้องทะลุผนังหรือเปลี่ยนขอบเขตกล้องแทนผู้ใช้

1. เปิด `Scenes/Repair.unity` เลือก `RepairSystems` มี CustomerRepairManager, RepairInteraction, RepairUI และ RepairOrbitCamera แล้ว จุด IN / Stay / Out เดิมไม่ได้ย้าย และผูกกับ Manager แล้ว
2. เติมวัสดุใน `MaterialPools/GunMaterials.asset`, `ArmorPlateMaterials.asset`, `WingMaterials.asset`, `JetMaterials.asset` แต่ละรายการรับ ShipMaterialData, Units At Full Damage และ Payment Per Unit เลือกจำนวนชนิดที่จะสุ่มด้วย Minimum/Maximum Material Types หากพูลว่างจะไม่เริ่มงานและแจ้งเตือน Console
3. ตั้ง Material Catalog และ Starting Materials บน `ResourceManager.prefab` สำหรับวัสดุเริ่มต้น (จำนวนยังว่างไว้ให้กำหนดเอง) Money คือเงินเริ่มต้น ใช้รหัส Item ID ไม่ซ้ำกันใน ShipMaterialData
4. สร้าง Canvas ของคุณ แล้วลากช่อง UI ลง RepairUI บน RepairSystems ตามรายละเอียดด้านล่าง จากนั้นกด Play และกดปุ่มซ่อมยานลูกค้า

## ช่องบน Inspector

อัปเดต 2026-10-27 — เส้นยานลูกค้าที่สุ่มบินเข้า: `CustomerRepairShip.Assemble` กำหนด ShipToonOutline ให้ทุกชิ้นหลังตั้ง parent/ตำแหน่ง และก่อน RepairableShipPart เก็บ property block สำหรับสีพัง/ไฮไลต์ หาก prefab ชิ้นใดยังไม่มี component จะเพิ่มให้เฉพาะ instance นั้น ไม่แก้ Material asset หรือ VFX ใช้ Toon/Toon ของยานจากงานก่อนหน้า เพิ่มช่อง **CustomerRepairShip > Customer Ship Outline > Outline World Width** บน `CustomerRepairShip.prefab` ตั้ง **0.03** หน่วยโลก (เส้นยานปกติเดิม 0.012) ให้หนาขึ้นสำหรับยานลูกค้า ปรับเป็น 0 เพื่อซ่อนเส้นได้ ชดเชยตามสเกล renderer ผ่าน ShipToonOutline.SetOutlineWidth และอ่าน property block ปัจจุบันก่อนอัปเดตสีใน RepairableShipPart.LateUpdate จึงเก็บค่าเส้นและ property อื่นไว้ ไม่ย้าย In/Stay/Out ไม่เปลี่ยนการบิน/UI/การสุ่มชิ้นส่วน ใส่ค่า prefab ให้แล้วไม่ต้องตั้งค่าเพิ่ม ยานที่ประกอบค้างอยู่ก่อนแก้ต้องเริ่ม Play ใหม่เพื่อรับค่าเริ่มต้นใหม่

ผลตรวจรอบนี้: คอมไพล์ Runtime ทั้งชุดกับ Roslyn/references ของ Unity โปรเจกต์จริงผ่าน exit 0 ตรวจค่า prefab และ diff ผ่าน ยังไม่ได้ตรวจภาพเส้นของยานลูกค้าระหว่างบิน/จอด/ไฮไลต์ใน Play Mode จริง ไม่ได้เรียกรีเซ็ตเซฟหรือเริ่มงานซ่อมแทนผู้ใช้

หน้า Main UI และ Repair ใช้กล้อง Cinemachine แยกกัน: RepairSystems > RepairOrbitCamera มี Main Menu Camera และ Repair Camera รับ CinemachineCamera คนละตัว สลับ enabled ตามสถานะหน้าซ่อม ผ่าน Main Camera/CinemachineBrain เดิม ระบบหมุนรอบยานขยับเฉพาะ Repair Camera และคงตำแหน่งกล้องเมนูไว้ UI ยังใช้ Main Menu Panel / Repair Panel คนละแผงตามเดิม ไม่ต้องเปลี่ยน Canvas Render Camera จาก Main Camera ไปเป็น CinemachineCamera

ผูกใน Repair.unity แล้ว: MainMenuCamera เป็นกล้องเมนูที่สำเนามุมเดิมไว้ ส่วน CinemachineCamera เดิมเป็นกล้องซ่อม ปรับตำแหน่ง/มุม MainMenuCamera เพื่อเลือกภาพหน้าเมนูตามต้องการ คอมไพล์ผ่าน ยังไม่ได้ตรวจการสลับภาพจริงใน Play Mode

### กด Repair แล้วไม่มียาน

ช่องปุ่มอยู่ที่ `RepairSystems > RepairUI > Repair Customer Button` ผูกปุ่ม Repair แล้ว จึงไม่ต้องเพิ่ม On Click ซ้ำ ตรวจล่าสุดผู้ใช้ใส่ Material ในพูลแล้ว แต่ Units At Full Damage เป็น 0 ทั้งสี่พูล ทำให้ไม่มีสูตรที่ใช้ได้ ต้องตั้งค่านี้มากกว่า 0 เช่น 10 คือใช้ 10 หน่วยที่ความเสียหาย 100% ส่วน Payment Per Unit เป็น 0 ได้หากไม่ต้องการค่าจ้างต่อหน่วย ระบบแสดงคำเตือนแยกชื่อพูล/ช่องที่ไม่ผ่านแล้ว การไม่มีวัสดุในคลังไม่ขวางการรับงาน แต่ต้องมีสูตรในพูลก่อน

Main Menu Panel ใส่ MainUi ได้ ส่วน Repair Panel ยังว่าง ให้ใส่แผงหน้าซ่อมของคุณเมื่อสร้างแล้ว การไม่ใส่ Repair Panel ไม่ได้ขวางการเกิดยาน

- CustomerRepairManager: In Point / Stay Point / Out Point รับ Transform; Customer Prefab รับ CustomerRepairShip; Gun / Armor Plate / Wing / Jet Pool รับ ShipPartStats ของช่องนั้น ส่วน Material Pools รับ RepairMaterialPool ทั้งสี่ช่อง ผูกตัวอย่างให้แล้ว ปรับเปอร์เซ็นต์พัง ความเร็วบิน เวลารอลูกค้าคันถัดไป และค่าจ้างพื้นฐานได้
- CustomerRepairShip บน prefab: จุดติดตั้ง Gun / Armor Plate / Wing / Jet รับ Transform ระบบสร้างชิ้นส่วนทั้งสี่ตอนรับงาน ปิด ShipGun และใช้ไอพ่นปกติระหว่างบิน ไอพ่นดับเมื่อจอด
- RepairInteraction: Workshop และกล้องจริงสำหรับ raycast ผูกแล้ว
- RepairOrbitCamera: Workshop และ Camera Rig เป็น Transform ของ CinemachineCamera เดิม คลิกขวาลากหมุนรอบยาน เลื่อนล้อซูม ทำงานเฉพาะเมื่อเปิดหน้าซ่อม คลิกซ้ายใช้ลากวัสดุ

## ต่อ Canvas ของคุณ

Repair Back Button บน RepairUI รับ Button กลับเมนูหลัก กดแล้วเรียก CloseRepair เพื่อพักงาน/ซ่อนยานลูกค้า สลับกล้องกลับเมนู และให้ยานผู้เล่นบินจาก In มาจอด เปิด Repair อีกครั้งทำงานเดิมต่อ รายละเอียดการบินผู้เล่นและ Docking Flight Speed อยู่ `../Customization/README.md` อย่าสลับเฉพาะ Panel โดยไม่เรียก CloseRepair

ปุ่มเข้าโหมดต่อสู้: บน RepairSystems > RepairUI เพิ่มช่อง Combat Button ให้ลาก Button ของคุณมาใส่ และ Combat Scene Name เริ่มต้น SampleScene (ฉากต่อสู้เดิม อยู่ใน Build Settings แล้ว) ระบบผูก onClick ให้ หรือเรียก RepairUI.EnterCombat จาก On Click เองโดยไม่ต้องผูกซ้ำ ใช้ข้อความปุ่ม ENG เช่น Combat ไม่มีการสร้าง/เปลี่ยน layout Canvas ให้ กดจากเมนูหลักจะโหลดฉากแบบ Single และ ResourceManager ที่คงข้ามซีนรักษาเงิน/คลังเดิม กันกดซ้ำระหว่างโหลดและไม่ให้เข้าเมื่อยังอยู่ในงานซ่อมเพื่อไม่ทิ้งวัสดุที่เติมไป หากเปลี่ยนฉากให้เพิ่มฉากใน Build Profile Scene List ด้วย ยังไม่ได้กดปุ่มทดสอบใน Play Mode

เก็บ RepairUI บน RepairSystems ที่เปิดอยู่เสมอ ไม่ย้ายไปไว้ใต้ Panel ที่จะปิด ระบบเปิด/ปิดแผงด้วย SetActive

- Main Menu Panel / Repair Panel: แผงเมนูและแผงซ่อมของคุณ
- Repair Customer Button / Submit Button: ปุ่มเปิดงานและส่งงาน RepairUI ผูก onClick ให้แล้ว ไม่ต้องผูกซ้ำ หรือจะใช้ CustomerRepairManager.OpenRepair / SubmitRepair ใน UnityEvent เองแทนก็ได้
- Money Text / Payment Text / Job Status Text: TMP_Text เงินที่มี ค่าจ้าง และสถานะงาน
- Inventory Container: RectTransform แถบล่าง จัด Horizontal/Grid Layout เอง; Inventory Item Prefab: prefab UI ที่ติด RepairMaterialDragItem ใส่ Image รูป, TMP ชื่อและจำนวนที่มี ตัว Graphic ต้องรับ raycast เพื่อเริ่มลากได้
- Drag Ghost Prefab บน RepairMaterialDragItem: RectTransform รูปตอนลากที่คุณทำเอง ไม่ใส่ก็ยังส่งวัสดุได้ ระบบไม่สร้างกราฟิกสำรอง
- Tooltip Panel / Part Name Text / Damage Text: แผงเล็กและ TMP ของคุณ แสดงใกล้ชิ้นส่วนที่ชี้ ไม่ตามเมาส์ อิงตำแหน่งชิ้นส่วนผ่านกล้องจริง ปรับระยะด้วย Tooltip Offset เดิม จัดให้เป็นลูก Canvas และตั้งขนาดเอง ระบบใส่ CanvasGroup กัน tooltip บัง raycast
- Requirement Container / Requirement Row Prefab: RectTransform และ prefab ที่ติด RepairRequirementRow ใส่ Image กับ TMP ชื่อ/จำนวน แสดงวัสดุที่ยังขาดและจำนวนที่มี

Canvas ต้องมี GraphicRaycaster ในฉากมี EventSystem พร้อม Input System UI module แล้ว ระบบสร้างแถวจาก prefab ที่คุณให้เท่านั้น ไม่ปรับ layout และไม่ลบลูกอื่นใน Container ช่อง UI ทั้งหมดยังว่างรอของจริง

## กติกาและการเชื่อมระบบ

อัปเดต 2026-10-27: ยานลูกค้าสุ่มชุดสีหลัก/สีรอง/โลหะเข้มหนึ่งชุดต่อหนึ่งลำจาก 8 palette ใน ShipRandomColors ตามผู้ใช้เลือก CustomerRepairShip.Assemble สุ่มครั้งเดียวและเก็บใน Colors แล้ว RepairableShipPart ใช้ชุดเดียวกันทุกส่วน ช่อง Material ที่เดิมมืด (RGB สูงสุดต่ำกว่า 0.5) ใช้โลหะเข้ม ช่องสว่างของตัวถัง/ปีกใช้สีหลัก ส่วนปืน/Jet ใช้สีรอง จึงไม่มีการสุ่มสีคนละโทนตาม renderer หรือช่อง material อีก อ่านรายละเอียดชุดสีใน ../README.md

คง alpha/texture/เส้นขอบและทำงานร่วมกับความหม่นตอนพัง/ไฮไลต์เดิม ใช้ MaterialPropertyBlock ไม่แก้ Material asset หรือสียานผู้เล่น ข้าม Material ที่ไม่ใช่ Toon/Toon เพื่อเก็บสี VFX/ไอพ่น ไม่มีช่อง Inspector ใหม่ ไม่ต้องตั้งค่าเพิ่ม คอมไพล์ Runtime จริงผ่าน exit 0 ตรวจ diff ผ่าน แต่ยังไม่ได้ตรวจภาพสีใน Play Mode ให้เกิดลูกค้าคันใหม่หรือเริ่ม Play ใหม่เพื่อใช้ชุดสีใหม่

Tooltip หลบกรอบโมเดลยานทั้งลำที่คำนวณจาก MeshRenderer/SkinnedMeshRenderer บนจอ เลือกด้านขวา ซ้าย บน หรือล่างที่มีพื้นที่แสดงมากที่สุด เว้นระยะอย่างน้อย 12 หน่วย Canvas (หรือค่า X ของ Tooltip Offset หากมากกว่า) ไม่ตามเมาส์ ไม่มีช่องใหม่ หากยานกินจอจนไม่มีพื้นที่พอ แผงอาจล้นขอบจอเพื่อไม่ให้ทับยาน ยังต้องตรวจภาพกับขนาด UI จริง

หัวของยานลูกค้า (local +Z) หันตามแนว Stay ไป Out ตั้งแต่เกิดและคงทิศตอนเข้าจอด เมื่อส่งงานหันไปยังตำแหน่ง Out ไม่ใช้ rotation ของจุด IN/Stay/Out จึงไม่ต้องหมุนจุดเหล่านั้น ไม่มีช่อง Inspector ใหม่

รับงานแล้วสุ่ม ShipPartStats สี่ช่องและเปอร์เซ็นต์พังของแต่ละชิ้น สุ่มวัสดุไม่ซ้ำรหัสจากพูลของช่องนั้น สูตรซ่อมถูกเก็บตลอดงาน ไม่สุ่มใหม่ตอนชี้เมาส์ จำนวนที่ต้องใช้ = ปัดขึ้น(Units At Full Damage × เปอร์เซ็นต์พัง / 100) เช่น 10 หน่วยที่พัง 35% ใช้ 4 หน่วย

ยานเกิดที่ IN บินไป Stay จึงเริ่มซ่อมได้ ชิ้นส่วนพังใช้สีหม่นและการสั่นแบบ noise ผ่าน MaterialPropertyBlock ไม่แก้ไฟล์ Material เดิม เมื่อชี้จะสว่างและลอยขึ้นเล็กน้อย การเรืองแสงขึ้นกับ shader ที่รองรับ ค่าความเสียหายลดตามสัดส่วนหน่วยวัสดุที่ยังขาด เมื่อครบจะหยุดสั่น

ลากหนึ่งครั้งเติมวัสดุชนิดนั้นเท่าที่ยังขาดและเรามี หักคลังจริง ของผิดชนิด วางพลาด หรือยังจอดไม่เสร็จไม่หักวัสดุ ต้องครบทั้งสี่ชิ้นจึงส่งได้ ค่าจ้าง = Base Payment + ผลรวม(จำนวนวัสดุที่กำหนดตอนรับงาน × Payment Per Unit) จ่ายครั้งเดียวเมื่อกดส่ง ยานบินไป Out และทำลายตัวเอง จากนั้นรับคันถัดไปอัตโนมัติหลัง Next Customer Delay (เริ่มต้น 1 วินาที)

ResourceManager เป็น singleton คงอยู่ข้ามซีน มี ShipInventory เก็บวัสดุและเงิน ใน Repair และ SampleScene วาง prefab ไว้แล้ว ตัวซ้ำจะถูกลบโดยรักษาของเดิม ShipController.Inventory และการเก็บ ShipPickup ใช้คลังกลางนี้ หากระบบอื่นเพิ่มวัสดุ ให้ RegisterMaterial(data) แล้ว Inventory.Add(data.itemId, amount) ใช้ changed สำหรับอัปเดต UI และ TrySpendMoney สำหรับใช้เงิน

เพิ่ม JSON save ของเงิน/วัสดุ/ปลดล็อก/ชุดยานและระบบปรับแต่งแล้ว อ่าน `../Customization/README.md` เมนูออกสู้ใช้ Combat Button บน RepairUI ระบบหยุด/ออกซีนระหว่างซ่อมยังไม่มีบันทึกงานค้างหรือคืนวัสดุ

## การตรวจ

คอมไพล์สคริปต์ SpaceGarage กับ assembly ของ Unity 6000.3.8f1 ผ่าน ตรวจสูตรปัดขึ้น ขอบเขตเปอร์เซ็นต์ จำนวนมาก การเติมบางส่วน และสต็อกไม่พอผ่าน 14 กรณี ทดสอบ API วงจรจริงใน Unity Play Mode ผ่าน 35 จุด: ประกอบสี่ส่วน, ห้ามซ่อมก่อนจอด, ห้ามส่งไม่ครบ, ของผิดชนิด, เติมบางส่วนและหักคลัง, ซ่อมครบ, จ่ายครั้งเดียว, ออกถึง Out และรับคันถัดไปอัตโนมัติ ใช้ข้อมูลชั่วคราวแยกจากคลังจริงและลบเครื่องมือทดสอบแล้ว การทดสอบบังคับตำแหน่งถึงจุดหมาย ไม่ได้ตรวจภาพการเดินทาง ยังต้องทดสอบการลากบน Canvas จริงและภาพการบิน/ไฮไลต์เมื่อใส่ UI และวัสดุของผู้ใช้
